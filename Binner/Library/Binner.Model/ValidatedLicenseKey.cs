namespace Binner.Model
{
    public class ValidatedLicenseKey : LicenseKey
    {
        public bool IsValid { get; set; }
        public DateTime? Expires { get; set; }
    }
}
