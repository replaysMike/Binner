using Binner.Common.Integrations.Models;
using Binner.Model;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Binner.Common.Integrations.ResponseProcessors
{
    public interface IResponseProcessor
    {
        Task ExecuteAsync(IIntegrationApi api, ProcessingContext context);
    }
}
