namespace Binner.Model.IO.Printing
{
    public class PrintContext : IPrintContext
    {
        public string PartName { get; set; } = string.Empty;
        public int UserId { get; set; }
        public int OrganizationId { get; set; }

        public PrintContext(string partName, int userId, int organizationId)
        {
            PartName = partName;
            UserId = userId;
            OrganizationId = organizationId;
        }
    }
}
