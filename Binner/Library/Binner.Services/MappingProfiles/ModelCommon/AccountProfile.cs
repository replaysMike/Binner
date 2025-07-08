using AutoMapper;
using Binner.Model;
using DataModel = Binner.Data.Model;

namespace Binner.Services.MappingProfiles.ModelCommon
{
    public class AccountProfile : Profile
    {
        public AccountProfile()
        {
            CreateMap<Account, DataModel.User>()
                .ForMember(x => x.EmailAddress, options => options.MapFrom(x => x.EmailAddress))
                .ForMember(x => x.IsEmailConfirmed, options => options.MapFrom(x => x.IsEmailConfirmed))
                .ForMember(x => x.DateEmailConfirmedUtc, options => options.MapFrom(x => x.DateEmailConfirmedUtc))
                .ForMember(x => x.Name, options => options.MapFrom(x => x.Name))
                .ForMember(x => x.PhoneNumber, options => options.MapFrom(x => x.PhoneNumber))
                .ForMember(x => x.LocaleLanguage, options => options.MapFrom(x => x.LocaleLanguage))
                .ForMember(x => x.LocaleCurrency, options => options.MapFrom(x => x.LocaleCurrency))
                .ForMember(x => x.CustomFields, options => options.MapFrom(x => x.CustomFields))
                .ForMember(x => x.CustomFieldValues, options => options.MapFrom(x => x.CustomFieldValues))

                .ForMember(x => x.ProfileImage, options => options.Ignore())
                .ForMember(x => x.UserId, options => options.Ignore())
                .ForMember(x => x.IsEmailSubscribed, options => options.Ignore())
                .ForMember(x => x.Parts, options => options.Ignore())
                .ForMember(x => x.PartTypes, options => options.Ignore())
                .ForMember(x => x.PasswordHash, options => options.Ignore())
                .ForMember(x => x.UserTokens, options => options.Ignore())
                .ForMember(x => x.OAuthCredentials, options => options.Ignore())
                .ForMember(x => x.OAuthRequests, options => options.Ignore())
                .ForMember(x => x.DateCreatedUtc, options => options.Ignore())
                .ForMember(x => x.DateLastLoginUtc, options => options.Ignore())
                .ForMember(x => x.DateLastActiveUtc, options => options.Ignore())
                .ForMember(x => x.DateLockedUtc, options => options.Ignore())
                .ForMember(x => x.DateModifiedUtc, options => options.Ignore())
                .ForMember(x => x.EmailConfirmationToken, options => options.Ignore())
                .ForMember(x => x.IsAdmin, options => options.Ignore())
                .ForMember(x => x.Projects, options => options.Ignore())
                .ForMember(x => x.OrganizationIntegrationConfigurations, options => options.Ignore())
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
                .ForMember(x => x.StoredFiles, options => options.Ignore())
                .ForMember(x => x.Organization, options => options.Ignore())
                .ForMember(x => x.OrganizationId, options => options.Ignore())
                .ForMember(x => x.ProjectProduceHistory, options => options.Ignore())
                .ForMember(x => x.ProjectPcbProduceHistory, options => options.Ignore())
                .ForMember(x => x.PartScanHistories, options => options.Ignore())
                .ForMember(x => x.OrderImportHistory, options => options.Ignore())
                .ForMember(x => x.OrderImportHistoryLineItems, options => options.Ignore())
                .ForMember(x => x.PartParametrics, options => options.Ignore())
                .ForMember(x => x.PartModels, options => options.Ignore())
                .ForMember(x => x.OrganizationIntegrationConfigurations, options => options.Ignore())
                .ForMember(x => x.OrganizationConfigurations, options => options.Ignore())
                .ForMember(x => x.UserConfigurations, options => options.Ignore())
                .ForMember(x => x.UserPrinterConfigurations, options => options.Ignore())
                .ForMember(x => x.UserPrinterTemplateConfigurations, options => options.Ignore())
            ;

            CreateMap<DataModel.User, Account>()
                    .ForMember(x => x.EmailAddress, options => options.MapFrom(x => x.EmailAddress))
                    .ForMember(x => x.IsEmailConfirmed, options => options.MapFrom(x => x.IsEmailConfirmed))
                    .ForMember(x => x.DateEmailConfirmedUtc, options => options.MapFrom(x => x.DateEmailConfirmedUtc))
                    .ForMember(x => x.Name, options => options.MapFrom(x => x.Name))
                    .ForMember(x => x.PhoneNumber, options => options.MapFrom(x => x.PhoneNumber))
                    .ForMember(x => x.LocaleLanguage, options => options.MapFrom(x => x.LocaleLanguage))
                    .ForMember(x => x.LocaleCurrency, options => options.MapFrom(x => x.LocaleCurrency))
                    .ForMember(x => x.ProfileImage, options => options.MapFrom(x => GetBase64Image(x.ProfileImage)))
                    .ForMember(x => x.IPAddress, options => options.Ignore())
                    .ForMember(x => x.PartsInventoryCount, options => options.Ignore())
                    .ForMember(x => x.PartTypesCount, options => options.Ignore())
                    .ForMember(x => x.Password, options => options.Ignore())
                    .ForMember(x => x.NewPassword, options => options.Ignore())
                    .ForMember(x => x.ConfirmNewPassword, options => options.Ignore())
                    .ForMember(x => x.Tokens, options => options.MapFrom(x => x.UserTokens))
                    .ForMember(x => x.CustomFields, options => options.MapFrom(x => x.CustomFields))
                    .ForMember(x => x.CustomFieldValues, options => options.MapFrom(x => x.CustomFieldValues))
                ;
        }

        private string GetBase64Image(byte[]? imageBytes) => imageBytes != null && imageBytes.Length > 0 ? Convert.ToBase64String(imageBytes) : string.Empty;
    }
}
