using AngleSharp.Dom;
using AutoMapper;
using Binner.Common.Extensions;
using Binner.Data;
using Binner.Global.Common;
using Binner.Global.Common.Services;
using Binner.Model;
using Binner.Model.Configuration;
using Binner.Model.IO.Printing;
using Binner.Model.IO.Printing.PrinterHardware;
using Binner.Model.Requests;
using Binner.Services.IO.Printing;
using Microsoft.EntityFrameworkCore;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using DataModel = Binner.Data.Model;

namespace Binner.Services
{
    public class PrintService : IPrintService
    {
        private readonly IStorageProvider _storageProvider;
        private readonly IMapper _mapper;
        private readonly IDbContextFactory<BinnerContext> _contextFactory;
        private readonly IRequestContextAccessor _requestContext;
        private readonly IUserConfigurationService _userConfigurationService;
        private readonly ILabelGenerator _labelGenerator;
        private readonly ILabelPrinterHardware _labelPrinter;
        private readonly IPrintSpoolQueueService _printSpoolQueueService;
        private readonly ISystemHubProxy _systemHubProxy;

        public PrintService(IMapper mapper, IStorageProvider storageProvider, IRequestContextAccessor requestContextAccessor, IDbContextFactory<BinnerContext> contextFactory, IUserConfigurationService userConfigurationService, ILabelGenerator labelGenerator, ILabelPrinterHardware labelPrinter, IPrintSpoolQueueService printSpoolQueueService, ISystemHubProxy systemHubProxy)
        {
            _mapper = mapper;
            _storageProvider = storageProvider;
            _requestContext = requestContextAccessor;
            _contextFactory = contextFactory;
            _userConfigurationService = userConfigurationService;
            _labelGenerator = labelGenerator;
            _labelPrinter = labelPrinter;
            _printSpoolQueueService = printSpoolQueueService;
            _systemHubProxy = systemHubProxy;
        }

        public async Task<bool> HasPartLabelTemplateAsync()
        {
            var user = _requestContext.GetUserContext();
            await using var context = await _contextFactory.CreateDbContextAsync();
            var hasUserConfig = await context.UserConfigurations
                .Where(x => x.UserId == user.UserId
                    && x.OrganizationId == user.OrganizationId
                    && x.DefaultPartLabelId != null)
                .AnyAsync();
            if (!hasUserConfig)
            {
                var hasDefaultSystemLabel = await context.Labels.Where(x => x.UserId == null && x.OrganizationId == null && x.IsPartLabelTemplate).AnyAsync();
                return hasDefaultSystemLabel;
            }
            return hasUserConfig;
        }

        public async Task<Label> GetPartLabelTemplateAsync(int? labelId = null)
        {
            var user = _requestContext.GetUserContext();
            await using var context = await _contextFactory.CreateDbContextAsync();
            var config = _userConfigurationService.GetCachedUserConfiguration();
            var defaultPartLabelId = config.DefaultPartLabelId;
            DataModel.Label? entity = null;
            if (labelId > 0)
            {
                entity = await context.Labels
                    .Include(x => x.LabelTemplate)
                    .Where(x => x.LabelId == labelId
                        && (x.OrganizationId == user.OrganizationId || x.OrganizationId == null))
                    .FirstOrDefaultAsync();
            }
            if (entity == null && defaultPartLabelId > 0)
            {
                // no specified label, get the default
                entity = await context.Labels
                    .Include(x => x.LabelTemplate)
                    .Where(x => x.LabelId == defaultPartLabelId)
                    .FirstOrDefaultAsync();
            }

            if (entity == null)
            {
                // if no default is set, use the default system template
                entity = await context.Labels
                    .Include(x => x.LabelTemplate)
                    // null organization ids indicate a system template
                    .Where(x => x.IsPartLabelTemplate && x.OrganizationId == null)
                    .FirstOrDefaultAsync();
            }
            return _mapper.Map<Label>(entity);
        }

        public async Task<Label?> SetDefaultLabelAsync(UpdateLabelRequest label)
        {
            var user = _requestContext.GetUserContext();
            await using var context = await _contextFactory.CreateDbContextAsync();
            var entity = await context.Labels
                .Where(x => x.LabelId == label.LabelId
                    // make sure it's either their own label, or a system label
                    && (x.UserId == user.UserId && x.OrganizationId == user.OrganizationId)
                    || (x.OrganizationId == null)
                    )
                .FirstOrDefaultAsync();
            if (entity != null)
            {
                // set the default part label template
                var config = await _userConfigurationService.GetUserConfigurationAsync();
                config.DefaultPartLabelId = label.LabelId;
                await _userConfigurationService.CreateOrUpdateUserConfigurationAsync(_mapper.Map<UserConfiguration>(config));
                return _mapper.Map<Label>(entity);
            }
            return null;
        }

        public async Task<LabelTemplate> AddLabelTemplateAsync(LabelTemplate model)
        {
            var user = _requestContext.GetUserContext();
            await using var context = await _contextFactory.CreateDbContextAsync();
            var entity = _mapper.Map<DataModel.LabelTemplate>(model);
            entity.UserId = user.UserId;
            entity.OrganizationId = user.OrganizationId;
            entity.DateCreatedUtc = DateTime.UtcNow;
            context.LabelTemplates.Add(entity);
            await context.SaveChangesAsync();
            return _mapper.Map<LabelTemplate>(entity);
        }

        public async Task<LabelTemplate?> UpdateLabelTemplateAsync(LabelTemplate model)
        {
            var user = _requestContext.GetUserContext();
            await using var context = await _contextFactory.CreateDbContextAsync();
            var entity = await context.LabelTemplates
                .Where(x => x.LabelTemplateId == model.LabelTemplateId && x.OrganizationId == user.OrganizationId)
                .FirstOrDefaultAsync();
            if (entity == null) return null;

            entity = _mapper.Map(model, entity);
            entity.UserId = user.UserId;
            entity.OrganizationId = user.OrganizationId;
            entity.DateModifiedUtc = DateTime.UtcNow;
            await context.SaveChangesAsync();

            return _mapper.Map<LabelTemplate>(entity);
        }

        public async Task<bool> DeleteLabelTemplateAsync(LabelTemplate model)
        {
            var user = _requestContext.GetUserContext();
            await using var context = await _contextFactory.CreateDbContextAsync();
            var entity = await context.LabelTemplates
                .Where(x => x.LabelTemplateId == model.LabelTemplateId && x.OrganizationId == user.OrganizationId)
                .FirstOrDefaultAsync();
            if (entity != null)
            {
                context.LabelTemplates.Remove(entity);
                return await context.SaveChangesAsync() > 0;
            }
            return false;
        }

        public async Task<LabelTemplate?> GetLabelTemplateAsync(int labelTemplateId)
        {
            var user = _requestContext.GetUserContext();
            await using var context = await _contextFactory.CreateDbContextAsync();
            var entity = await context.LabelTemplates
                // null organization ids indicate a system template
                .Where(x => x.LabelTemplateId == labelTemplateId && (x.OrganizationId == null || x.OrganizationId == user.OrganizationId))
                .FirstOrDefaultAsync();
            return _mapper.Map<LabelTemplate?>(entity);
        }

        public async Task<ICollection<LabelTemplate>> GetLabelTemplatesAsync()
        {
            var user = _requestContext.GetUserContext();
            await using var context = await _contextFactory.CreateDbContextAsync();
            var entities = await context.LabelTemplates
                // null organization ids indicate a system template
                .Where(x => x.OrganizationId == null || x.OrganizationId == user.OrganizationId)
                .OrderBy(x => x.OrganizationId).ThenBy(x => x.Name)
                .ToListAsync();
            return _mapper.Map<ICollection<LabelTemplate>>(entities);
        }

        public async Task<Label> AddOrUpdateLabelAsync(Label model)
        {
            var user = _requestContext.GetUserContext();
            await using var context = await _contextFactory.CreateDbContextAsync();

            DataModel.Label? entity = null;

            if (model.LabelId > 0)
            {
                entity = await context.Labels
                    .Where(x => x.LabelId == model.LabelId && x.OrganizationId == user.OrganizationId)
                    .FirstOrDefaultAsync();
                if (entity != null)
                {
                    entity = _mapper.Map(model, entity);
                    entity.LabelId = model.LabelId;
                }
            }

            if (entity == null)
            {
                entity = _mapper.Map<DataModel.Label>(model);
                context.Labels.Add(entity);
                entity.DateCreatedUtc = DateTime.UtcNow;
                entity.DateModifiedUtc = DateTime.UtcNow;
            }

            entity.UserId = user.UserId;
            entity.OrganizationId = user.OrganizationId;
            await context.SaveChangesAsync();

            if (model.IsPartLabelTemplate)
            {
                // set the default part label template
                var config = await _userConfigurationService.GetUserConfigurationAsync();
                config.DefaultPartLabelId = entity.LabelId;
                await _userConfigurationService.CreateOrUpdateUserConfigurationAsync(_mapper.Map<UserConfiguration>(config));
            }

            return _mapper.Map<Label>(entity);
        }

        public async Task<Label> UpdateLabelAsync(Label model)
        {
            var user = _requestContext.GetUserContext();
            await using var context = await _contextFactory.CreateDbContextAsync();
            var entity = await context.Labels
                .Where(x => x.LabelId == model.LabelId && x.OrganizationId == user.OrganizationId)
                .FirstOrDefaultAsync();
            if (entity != null)
            {
                entity = _mapper.Map(model, entity);
                entity.UserId = user.UserId;
                entity.OrganizationId = user.OrganizationId;
                entity.DateModifiedUtc = DateTime.UtcNow;
                await context.SaveChangesAsync();

                if (model.IsPartLabelTemplate)
                {
                    // set the default part label template
                    var config = _userConfigurationService.GetCachedUserConfiguration();
                    config.DefaultPartLabelId = entity.LabelId;
                    await _userConfigurationService.CreateOrUpdateUserConfigurationAsync(_mapper.Map<UserConfiguration>(config));
                }
            }

            return _mapper.Map<Label>(entity);
        }

        public async Task<bool> DeleteLabelAsync(Label model)
        {
            var user = _requestContext.GetUserContext();
            await using var context = await _contextFactory.CreateDbContextAsync();
            var entity = await context.Labels
                .Where(x => x.LabelId == model.LabelId && x.OrganizationId == user.OrganizationId)
                .FirstOrDefaultAsync();
            if (entity != null)
            {
                context.Labels.Remove(entity);
                var result = await context.SaveChangesAsync() > 0;
                if (result)
                {
                    var config = _userConfigurationService.GetCachedUserConfiguration();
                    if (config.DefaultPartLabelId == model.LabelId)
                    {
                        // set the default part label template to next label
                        var label = await context.Labels
                            .Where(x => x.UserId == user.UserId && x.OrganizationId == user.OrganizationId)
                            .FirstOrDefaultAsync();
                        if (label == null)
                            label = await context.Labels
                                .Where(x => x.OrganizationId == null && x.IsPartLabelTemplate)
                                .FirstOrDefaultAsync();
                        config.DefaultPartLabelId = label?.LabelId;
                        await _userConfigurationService.CreateOrUpdateUserConfigurationAsync(_mapper.Map<UserConfiguration>(config));
                    }
                }

                return result;
            }
            return false;
        }

        public async Task<ICollection<Label>> GetLabelsAsync()
        {
            var user = _requestContext.GetUserContext();
            await using var context = await _contextFactory.CreateDbContextAsync();
            var entities = await context.Labels
                // null organization ids indicate a system template
                .Where(x => x.OrganizationId == null || x.OrganizationId == user.OrganizationId)
                .OrderByDescending(x => x.OrganizationId).ThenByDescending(x => x.IsPartLabelTemplate).ThenByDescending(x => x.DateModifiedUtc)
                .ToListAsync();
            var config = _userConfigurationService.GetCachedUserConfiguration();
            var models = _mapper.Map<ICollection<Label>>(entities);
            if (config.DefaultPartLabelId != null && models.Any(x => x.LabelId == config.DefaultPartLabelId))
            {
                models = models.Select(x =>
                {
                    x.IsPartLabelTemplate = false; // ensure other labels are not marked as part label templates
                    if (x.LabelId == config.DefaultPartLabelId)
                        x.IsPartLabelTemplate = true; // mark the default part label template
                    return x;
                }).ToList();
            }
            return models;
        }

        public async Task<Stream> PrintAsync(Part part, int? labelId = null, bool generateImageOnly = false)
        {
            var printerConfig = _userConfigurationService.GetCachedPrinterConfiguration();
            var hasPartLabelTemplate = await HasPartLabelTemplateAsync();
            switch (printerConfig.PrintMode)
            {
                case PrintModes.PrintSpoolService:
                    // print to a remote print server running the Binner Print Spool service
                    {
                        if (hasPartLabelTemplate)
                        {
                            // use the new part label template
                            var label = await GetPartLabelTemplateAsync(labelId);

                            // load the label template
                            var template = await GetLabelTemplateAsync(label.LabelTemplateId);

                            // send to the print spool service
                            await _printSpoolQueueService.QueuePrintAsync(part, label, template);

                            // render the image, purely to return the label image in the response
                            var (stream, image) = await CreateLabelImageAsync(label, part);
                            return stream;
                        }
                        else
                        {
                            // use legacy print template

                            // send to the print spool service
                            await _printSpoolQueueService.QueuePrintAsync(part);

                            // render the image, purely to return the label image in the response
                            var stream = await PrintLegacyAsync(part, generateImageOnly);
                            return stream;
                        }
                    }
                case PrintModes.Direct:
                    // direct printing to a device attached to the same machine as Binner
                    {
                        if (hasPartLabelTemplate)
                        {
                            // use the new part label template
                            var label = await GetPartLabelTemplateAsync(labelId);

                            // load the label template
                            var template = await GetLabelTemplateAsync(label.LabelTemplateId);

                            var (stream, image) = await CreateLabelImageAsync(label, part);
                            if (!generateImageOnly)
                                _labelPrinter.PrintLabelImage(image, new PrinterOptions(printerConfig.PartLabelSource, template.Name, false));
                            image.SaveAsPng(@"C:\Users\mikeb\Downloads\test.png");

                            return stream;
                        }
                        else
                        {
                            // use legacy print template
                            var stream = await PrintLegacyAsync(part, generateImageOnly);
                            return stream;
                        }
                    }
                case PrintModes.WebBrowser:
                    // print using the web browser print function.
                    // nothing to do, ignore request
                    break;
            }
            return new MemoryStream();
        }

        private async Task<(Stream, Image<Rgba32>)> CreateLabelImageAsync(Label label, Part part)
        {
            var image = _labelGenerator.CreateLabelImage(label, part);
            var stream = new MemoryStream();
            await image.SaveAsPngAsync(stream);
            stream.Seek(0, SeekOrigin.Begin);
            return (stream, image);
        }

        private async Task<Stream> PrintLegacyAsync(Part part, bool generateImageOnly)
        {
            var stream = new MemoryStream();
            var image = _labelPrinter.PrintLabel(new LabelContent { Part = part }, new PrinterOptions(generateImageOnly));
            await image.SaveAsPngAsync(stream);
            stream.Seek(0, SeekOrigin.Begin);
            return stream;
        }
    }
}
