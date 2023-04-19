namespace Binner.Model.Requests;

public class ForgetCachedCredentialsRequest
{
    /// <summary>
    /// The api name to clear
    /// </summary>
    public string Name { get; set; } = null!;
}