namespace Binner.Services.Integrations.ResponseProcessors
{
    public interface IResponseProcessor
    {
        Task ExecuteAsync(IIntegrationApi api, ProcessingContext context);
    }
}
