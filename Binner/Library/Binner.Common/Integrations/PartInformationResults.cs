using Binner.Model;
using System.Collections.Generic;

namespace Binner.Common.Integrations
{
    public class PartInformationResults
    {
        public Dictionary<string, Model.Integrations.ApiResponseState> ApiResponses { get; set; }
        public PartResults PartResults { get; set; }
        public PartInformationResults(Dictionary<string, Model.Integrations.ApiResponseState> apiResponses, PartResults partResults)
        {
            ApiResponses = apiResponses;
            PartResults = partResults;
        }
    }
}
