namespace Binner.Model.Responses
{
    /// <summary>
    /// A response to provide if object is a duplicate
    /// </summary>
    public class PossibleDuplicateResponse
    {
        public bool IsPossibleDuplicate => true;
        public string ErrorMessage { get; }
        public ICollection<PartResponse> Parts { get; }

        public PossibleDuplicateResponse(ICollection<PartResponse> parts) 
            : this("There are possible existing records matching the new resource. Specify AllowPotentialDuplicate if you wish to add it anyway.", parts)
        {
        }

        public PossibleDuplicateResponse(string errorMessage, ICollection<PartResponse> parts)
        {
            ErrorMessage = errorMessage;
            Parts = parts;
        }
    }
}
