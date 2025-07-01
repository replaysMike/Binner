using Binner.Global.Common;

namespace Binner.Model
{
    public interface IAccount
    {
        string? ConfirmNewPassword { get; set; }
        ICollection<CustomField>? CustomFields { get; set; }
        ICollection<CustomFieldValue>? CustomFieldValues { get; set; }
        DateTime? DateEmailConfirmedUtc { get; set; }
        string EmailAddress { get; set; }
        string? IPAddress { get; set; }
        bool IsEmailConfirmed { get; set; }
        string? LocaleCurrency { get; set; }
        string? LocaleLanguage { get; set; }
        string Name { get; set; }
        string? NewPassword { get; set; }
        int PartsInventoryCount { get; set; }
        int PartTypesCount { get; set; }
        string? Password { get; set; }
        string? PhoneNumber { get; set; }
        string? ProfileImage { get; set; }
        ICollection<Token>? Tokens { get; set; }
    }
}