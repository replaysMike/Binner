using AutoMapper;
using Binner.Common.IO;
using Binner.Data;
using Binner.Global.Common;
using Binner.Global.Common.Services;
using Binner.Model;
using Binner.Model.IO.Printing;
using Binner.Model.Responses;
using Microsoft.EntityFrameworkCore;
using System.Data;
using System.Text.Json;
using DataModel = Binner.Data.Model;

namespace Binner.Services
{
    /// <summary>
    /// Manage users
    /// </summary>
    public class PrintSpoolQueueService : IPrintSpoolQueueService
    {
        protected readonly IDbContextFactory<BinnerContext> _contextFactory;
        protected readonly IMapper _mapper;
        protected readonly IRequestContextAccessor _requestContext;
        protected readonly IPrintHubProxy _printHubProxy;
        private readonly IUserConfigurationService _userConfigurationService;
        private JsonSerializerOptions _jsonSerializerOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            // minimize whitespace
            WriteIndented = false,
        };

        public PrintSpoolQueueService(IDbContextFactory<BinnerContext> contextFactory, IMapper mapper, IRequestContextAccessor requestContext, IUserConfigurationService userConfigurationService, IPrintHubProxy printHubProxy)
        {
            _contextFactory = contextFactory;
            _mapper = mapper;
            _requestContext = requestContext;
            _userConfigurationService = userConfigurationService;
            _printHubProxy = printHubProxy;
        }

        protected virtual IQueryable<DataModel.PrintSpoolQueue> GetPrintSpoolQueueQueryable(BinnerContext context, int organizationId)
            => context.PrintSpoolQueue
                .Where(x => x.OrganizationId == organizationId)
                .AsQueryable();

        private async Task<int?> GetOrganizationIdAsync(BinnerContext context, Guid printSpoolQueueId)
        {
            var orgConfig = await context.OrganizationConfigurations
                .Where(x => x.PrintSpoolQueueId == printSpoolQueueId)
                .FirstOrDefaultAsync();
            if (orgConfig == null || orgConfig.OrganizationId == null) return null;
            return orgConfig.OrganizationId;
        }

        private async Task<Guid?> GetPrintSpoolQueueIdAsync(BinnerContext context, int organizationId)
        {
            var orgConfig = await context.OrganizationConfigurations
                .Where(x => x.OrganizationId == organizationId)
                .FirstOrDefaultAsync();
            if (orgConfig == null || orgConfig.PrintSpoolQueueId == Guid.Empty) return null;
            return orgConfig.PrintSpoolQueueId;
        }

        public virtual async Task<PrinterConfigurationResponse?> GetConfigurationAsync(Guid printSpoolQueueId)
        {
            if (printSpoolQueueId == Guid.Empty) return null;
            await using var context = await _contextFactory.CreateDbContextAsync();
            var organizationId = await GetOrganizationIdAsync(context, printSpoolQueueId);
            if (organizationId == null) return null;

            var printerConfig = _userConfigurationService.GetCachedPrinterConfiguration();

            return new PrinterConfigurationResponse { 
                PrinterConfiguration = _mapper.Map<PrinterSettings>(printerConfig),
            };
        }

        public virtual async Task<PrintSpoolQueueResponse?> GetPendingAsync(Guid printSpoolQueueId)
        {
            if (printSpoolQueueId == Guid.Empty) return null;
            await using var context = await _contextFactory.CreateDbContextAsync();
            var organizationId = await GetOrganizationIdAsync(context, printSpoolQueueId);
            if (organizationId == null) return null;

            var entities = await GetPrintSpoolQueueQueryable(context, organizationId.Value)
                .ToListAsync();

            return new PrintSpoolQueueResponse() { 
                Queue = _mapper.Map<ICollection<PrintSpoolQueue>>(entities),
                PrinterConfigurationCrc = 0
            };
        }

        public virtual async Task<bool> DeletePrintSpoolQueueAsync(Guid printSpoolQueueId, Guid globalId)
        {
            if (printSpoolQueueId == Guid.Empty) return false;
            if (globalId == Guid.Empty) return false;
            await using var context = await _contextFactory.CreateDbContextAsync();
            var organizationId = await GetOrganizationIdAsync(context, printSpoolQueueId);
            if (organizationId == null) return false;

            // delete it from the print queue
            return await context.PrintSpoolQueue
                .Where(x => x.OrganizationId == organizationId && x.GlobalId == globalId)
                .ExecuteDeleteAsync() > 0;
        }

        public virtual async Task<bool> QueuePrintAsync(Part part, Label? label = null, LabelTemplate? template = null)
        {
            var userContext = _requestContext.GetUserContext();
            if (userContext == null) throw new UserContextUnauthorizedException();

            await using var context = await _contextFactory.CreateDbContextAsync();
            var partJson = JsonSerializer.Serialize(part, _jsonSerializerOptions);
            var labelJson = label != null ? JsonSerializer.Serialize(label, _jsonSerializerOptions) : string.Empty;
            var templateJson = label != null ? JsonSerializer.Serialize(template, _jsonSerializerOptions) : string.Empty;
            var crc32 = Checksum.Compute(partJson);

            // don't allow duplicate print jobs
            var exists = await context.PrintSpoolQueue.AnyAsync(x => x.Crc32 == crc32);
            if (exists)
            {
                // don't print it again, but notify of a print queue job
                var id = await GetPrintSpoolQueueIdAsync(context, userContext.OrganizationId);
                if (id != null && id != Guid.Empty)
                    await _printHubProxy.NotifyPrintAsync(id.Value);

                return false;
            }

            var entity = new DataModel.PrintSpoolQueue
            {
                DateCreatedUtc = DateTime.UtcNow,
                GlobalId = Guid.NewGuid(),
                OrganizationId = userContext.OrganizationId,
                UserId = userContext.UserId,

                PrintType = PrintTypes.PartLabel,
                Json = partJson,
                LabelJson = labelJson,
                TemplateJson = templateJson,
                Crc32 = crc32
            };
            context.PrintSpoolQueue.Add(entity);
            
            var isSuccess = await context.SaveChangesAsync() > 0;

            // notify of a print queue job
            var printSpoolQueueId = await GetPrintSpoolQueueIdAsync(context, userContext.OrganizationId);
            if (printSpoolQueueId != null && printSpoolQueueId != Guid.Empty)
                await _printHubProxy.NotifyPrintAsync(printSpoolQueueId.Value);

            return isSuccess;
        }
    }
}
