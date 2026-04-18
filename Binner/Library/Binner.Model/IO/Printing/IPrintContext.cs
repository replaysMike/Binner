namespace Binner.Model.IO.Printing
{
    public interface IPrintContext
    {
        int OrganizationId { get; set; }
        string PartName { get; set; }
        int UserId { get; set; }
    }
}