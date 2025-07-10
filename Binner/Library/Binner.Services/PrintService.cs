using AutoMapper;
using Binner.Data;
using Binner.Global.Common;
using Binner.Model;
using Binner.Model.Configuration;
using Microsoft.EntityFrameworkCore;
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

        public PrintService(IMapper mapper, IStorageProvider storageProvider, IRequestContextAccessor requestContextAccessor, IDbContextFactory<BinnerContext> contextFactory, IUserConfigurationService userConfigurationService)
        {
            _mapper = mapper;
            _storageProvider = storageProvider;
            _requestContext = requestContextAccessor;
            _contextFactory = contextFactory;
            _userConfigurationService = userConfigurationService;
        }

        public async Task<bool> HasPartLabelTemplateAsync()
        {
            var user = _requestContext.GetUserContext();
            await using var context = await _contextFactory.CreateDbContextAsync();
            return await context.UserConfigurations
                .Where(x => x.UserId == user.UserId && x.OrganizationId == user.OrganizationId && x.DefaultPartLabelId != null)
                .AnyAsync();
        }

        public async Task<Label> GetPartLabelTemplateAsync()
        {
            var user = _requestContext.GetUserContext();
            await using var context = await _contextFactory.CreateDbContextAsync();
            var config = _userConfigurationService.GetCachedUserConfiguration();
            var defaultPartLabelId = config.DefaultPartLabelId;
            var entity = await context.Labels
                .Where(x => x.LabelId == defaultPartLabelId && x.OrganizationId == user.OrganizationId)
                .OrderBy(x => x.OrganizationId)
                .FirstOrDefaultAsync();
            if (entity == null)
            {
                // if no default is set, use the default system template
                entity = await context.Labels
                    // null organization ids indicate a system template
                    .Where(x => x.IsPartLabelTemplate && x.OrganizationId == null)
                    .OrderBy(x => x.OrganizationId)
                    .FirstOrDefaultAsync();
            }
            return _mapper.Map<Label>(entity);
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
                var config = _userConfigurationService.GetCachedUserConfiguration();
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
    }
}
