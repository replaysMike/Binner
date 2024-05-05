﻿using AutoMapper;
using Binner.Data;
using Binner.Global.Common;
using Binner.Model;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DataModel = Binner.Data.Model;

namespace Binner.Common.Services
{
    public class PrintService : IPrintService
    {
        private readonly IStorageProvider _storageProvider;
        private readonly IMapper _mapper;
        private readonly IDbContextFactory<BinnerContext> _contextFactory;
        private readonly RequestContextAccessor _requestContext;

        public PrintService(IMapper mapper, IStorageProvider storageProvider, RequestContextAccessor requestContextAccessor, IDbContextFactory<BinnerContext> contextFactory)
        {
            _mapper = mapper;
            _storageProvider = storageProvider;
            _requestContext = requestContextAccessor;
            _contextFactory = contextFactory;
        }

        public async Task<bool> HasPartLabelTemplateAsync()
        {
            var user = _requestContext.GetUserContext();
            await using var context = await _contextFactory.CreateDbContextAsync();
            return await context.Labels
                .Where(x => x.IsPartLabelTemplate && x.OrganizationId == user.OrganizationId)
                .AnyAsync();
        }

        public async Task<Label> GetPartLabelTemplateAsync()
        {
            var user = _requestContext.GetUserContext();
            await using var context = await _contextFactory.CreateDbContextAsync();
            var entity = await context.Labels
                .Where(x => x.IsPartLabelTemplate && x.OrganizationId == user.OrganizationId)
                .FirstOrDefaultAsync();
            return _mapper.Map<Label>(entity);
        }

        public async Task<LabelTemplate> AddLabelTemplateAsync(LabelTemplate model)
        {
            var user = _requestContext.GetUserContext();
            await using var context = await _contextFactory.CreateDbContextAsync();
            var entity = _mapper.Map<DataModel.LabelTemplate>(model);
            entity.UserId = user.UserId;
            entity.OrganizationId = user.OrganizationId;
            context.LabelTemplates.Add(entity);
            await context.SaveChangesAsync();
            return _mapper.Map<LabelTemplate>(entity);
        }

        public async Task<LabelTemplate> UpdateLabelTemplateAsync(LabelTemplate model)
        {
            var user = _requestContext.GetUserContext();
            await using var context = await _contextFactory.CreateDbContextAsync();
            var entity = await context.LabelTemplates
                .Where(x => x.LabelTemplateId == model.LabelTemplateId && x.OrganizationId == user.OrganizationId)
                .FirstOrDefaultAsync();
            if (entity != null)
            {
                entity = _mapper.Map(model, entity);
                entity.UserId = user.UserId;
                entity.OrganizationId = user.OrganizationId;
                entity.DateModifiedUtc = DateTime.UtcNow;
                await context.SaveChangesAsync();
            }

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
                .Where(x => x.LabelTemplateId == labelTemplateId && x.OrganizationId == user.OrganizationId)
                .FirstOrDefaultAsync();
            return _mapper.Map<LabelTemplate?>(entity);
        }

        public async Task<ICollection<LabelTemplate>> GetLabelTemplatesAsync()
        {
            var user = _requestContext.GetUserContext();
            await using var context = await _contextFactory.CreateDbContextAsync();
            var entities = await context.LabelTemplates
                .Where(x => x.OrganizationId == user.OrganizationId)
                .ToListAsync();
            return _mapper.Map<ICollection<LabelTemplate>>(entities);
        }

        public async Task<Label> AddOrUpdateLabelAsync(Label model)
        {
            var user = _requestContext.GetUserContext();
            await using var context = await _contextFactory.CreateDbContextAsync();
            if (model.IsPartLabelTemplate)
            {
                // unset any labels marked as the part label template
                var defaultPartLabel = await context.Labels.Where(x => x.IsPartLabelTemplate && x.OrganizationId == user.OrganizationId).ToListAsync();
                foreach (var label in defaultPartLabel)
                    label.IsPartLabelTemplate = false;
            }

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
                entity.DateModifiedUtc = DateTime.UtcNow;
            }

            entity.UserId = user.UserId;
            entity.OrganizationId = user.OrganizationId;
            await context.SaveChangesAsync();
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
                return await context.SaveChangesAsync() > 0;
            }
            return false;
        }

        public async Task<ICollection<Label>> GetLabelsAsync()
        {
            var user = _requestContext.GetUserContext();
            await using var context = await _contextFactory.CreateDbContextAsync();
            var entities = await context.Labels
                .Where(x => x.OrganizationId == user.OrganizationId)
                .OrderByDescending(x => x.DateModifiedUtc)
                .ToListAsync();
            return _mapper.Map<ICollection<Label>>(entities);
        }
    }
}
