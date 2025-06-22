using AutoMapper;
using System;
using Binner.Model;
using DataModel = Binner.Data.Model;

namespace Binner.Services.MappingProfiles.ModelCommon
{
#if INITIALCREATE
    public class UserProfile : Profile
    {
        public UserProfile()
        {
            CreateMap<User, DataModel.User>()
                .ForMember(x => x.UserId, options => options.MapFrom(x => x.UserId))
                .ForMember(x => x.OrganizationId, options => options.MapFrom(x => x.OrganizationId))
                .ForMember(x => x.DateCreatedUtc, options => options.MapFrom(x => x.DateCreatedUtc))
                .ForMember(x => x.DateLastLoginUtc, options => options.MapFrom(x => x.DateLastLoginUtc))
                .ForMember(x => x.DateLastActiveUtc, options => options.MapFrom(x => x.DateLastActiveUtc))
                .ForMember(x => x.DateModifiedUtc, options => options.MapFrom(x => x.DateModifiedUtc))
                .ForMember(x => x.EmailAddress, options => options.MapFrom(x => x.EmailAddress))
                .ForMember(x => x.EmailConfirmationToken, options => options.MapFrom(x => x.EmailConfirmationToken))
                .ForMember(x => x.IsAdmin, options => options.MapFrom(x => x.IsAdmin))
                .ForMember(x => x.IsEmailConfirmed, options => options.MapFrom(x => x.IsEmailConfirmed))
                .ForMember(x => x.DateEmailConfirmedUtc, options => options.MapFrom(x => x.DateEmailConfirmedUtc))
                .ForMember(x => x.Name, options => options.MapFrom(x => x.Name))
                .ForMember(x => x.PhoneNumber, options => options.MapFrom(x => x.PhoneNumber))
                .ForMember(x => x.LocaleLanguage, options => options.MapFrom(x => x.LocaleLanguage))
                .ForMember(x => x.LocaleCurrency, options => options.MapFrom(x => x.LocaleCurrency))
                .ForMember(x => x.UserIntegrationConfigurations, options => options.Ignore())
                .ForMember(x => x.UserPrinterConfigurations, options => options.Ignore())
                .ForMember(x => x.UserPrinterTemplateConfigurations, options => options.Ignore())
                .ForMember(x => x.ProfileImage, options => options.Ignore())
                .ForMember(x => x.IsEmailSubscribed, options => options.Ignore())
                .ForMember(x => x.Parts, options => options.Ignore())
                .ForMember(x => x.PartTypes, options => options.Ignore())
                .ForMember(x => x.PasswordHash, options => options.Ignore())
                .ForMember(x => x.Projects, options => options.Ignore())
                .ForMember(x => x.UserTokens, options => options.Ignore())
                .ForMember(x => x.OAuthCredentials, options => options.Ignore())
                .ForMember(x => x.OAuthRequests, options => options.Ignore())
                .ForMember(x => x.ProfileImage, options => options.Ignore())
                .ForMember(x => x.Ip, options => options.Ignore())
                .ForMember(x => x.EmailConfirmedIp, options => options.Ignore())
                .ForMember(x => x.LastSetPasswordIp, options => options.Ignore())
                .ForMember(x => x.UserLoginHistory, options => options.Ignore())
                .ForMember(x => x.ReCaptchaScore, options => options.Ignore())
                .ForMember(x => x.PartSuppliers, options => options.Ignore())
                .ForMember(x => x.Pcbs, options => options.Ignore())
                .ForMember(x => x.PcbStoredFileAssignments, options => options.Ignore())
                .ForMember(x => x.ProjectPartAssignments, options => options.Ignore())
                .ForMember(x => x.ProjectPcbAssignments, options => options.Ignore())
                .ForMember(x => x.Pcbs, options => options.Ignore())
                .ForMember(x => x.StoredFiles, options => options.Ignore())
                .ForMember(x => x.Organization, options => options.Ignore())
                .ForMember(x => x.OrganizationId, options => options.Ignore())
                .ForMember(x => x.ProjectProduceHistory, options => options.Ignore())
                .ForMember(x => x.ProjectPcbProduceHistory, options => options.Ignore())
                .ForMember(x => x.PartScanHistories, options => options.Ignore())
                .ForMember(x => x.OrderImportHistory, options => options.Ignore())
                .ForMember(x => x.OrderImportHistoryLineItems, options => options.Ignore())
                .ForMember(x => x.CustomFields, options => options.Ignore())
                .ForMember(x => x.CustomFieldValues, options => options.Ignore())
                .ForMember(x => x.PartParametrics, options => options.Ignore())
                .ForMember(x => x.PartModels, options => options.Ignore())
                ;

            CreateMap<DataModel.User, User>()
                .ForMember(x => x.UserId, options => options.MapFrom(x => x.UserId))
                .ForMember(x => x.OrganizationId, options => options.MapFrom(x => x.OrganizationId))
                .ForMember(x => x.DateCreatedUtc, options => options.MapFrom(x => x.DateCreatedUtc))
                .ForMember(x => x.DateLastLoginUtc, options => options.MapFrom(x => x.DateLastLoginUtc))
                .ForMember(x => x.DateLastActiveUtc, options => options.MapFrom(x => x.DateLastActiveUtc))
                .ForMember(x => x.DateModifiedUtc, options => options.MapFrom(x => x.DateModifiedUtc))
                .ForMember(x => x.EmailAddress, options => options.MapFrom(x => x.EmailAddress))
                .ForMember(x => x.EmailConfirmationToken, options => options.MapFrom(x => x.EmailConfirmationToken))
                .ForMember(x => x.IsAdmin, options => options.MapFrom(x => x.IsAdmin))
                .ForMember(x => x.IsEmailConfirmed, options => options.MapFrom(x => x.IsEmailConfirmed))
                .ForMember(x => x.DateEmailConfirmedUtc, options => options.MapFrom(x => x.DateEmailConfirmedUtc))
                .ForMember(x => x.Name, options => options.MapFrom(x => x.Name))
                .ForMember(x => x.PhoneNumber, options => options.MapFrom(x => x.PhoneNumber))
                .ForMember(x => x.LocaleLanguage, options => options.MapFrom(x => x.LocaleLanguage))
                .ForMember(x => x.LocaleCurrency, options => options.MapFrom(x => x.LocaleCurrency))
                .ForMember(x => x.IpAddress, options => options.Ignore())
                .ForMember(x => x.Password, options => options.Ignore())
                .ForMember(x => x.PartsInventoryCount, options => options.Ignore())
                .ForMember(x => x.PartTypesCount, options => options.Ignore())
                .ForMember(x => x.ProfileImage, options => options.MapFrom(x => GetBase64Image(x.ProfileImage)))
                .ForMember(x => x.CustomFields, options => options.Ignore()) // mapped manually
                ;
        }

        private string GetBase64Image(byte[]? imageBytes)
        {
            try
            {
                return imageBytes != null && imageBytes.Length > 0 ? Convert.ToBase64String(imageBytes) : string.Empty;
            }
            catch (Exception)
            {
                return string.Empty;
            }
        }
    }
#endif
}
