using Binner.Common.Integrations;
using Binner.Common.Models;
using Binner.Common.StorageProviders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Binner.Common.Services
{
    public class PartService : IPartService
    {
        private IStorageProvider _storageProvider;
        private OctopartApi _octopartApi;
        private DigikeyApi _digikeyApi;
        private MouserApi _mouserApi;
        private RequestContextAccessor _requestContext;

        public PartService(IStorageProvider storageProvider, RequestContextAccessor requestContextAccessor, OctopartApi octopartApi, DigikeyApi digikeyApi, MouserApi mouserApi)
        {
            _storageProvider = storageProvider;
            _requestContext = requestContextAccessor;
            _octopartApi = octopartApi;
            _digikeyApi = digikeyApi;
            _mouserApi = mouserApi;
        }

        public async Task<ICollection<SearchResult<Part>>> FindPartsAsync(string keywords)
        {
            return await _storageProvider.FindPartsAsync(keywords, _requestContext.GetUserContext());
        }

        public async Task<Part> GetPartAsync(string partNumber)
        {
            return await _storageProvider.GetPartAsync(partNumber, _requestContext.GetUserContext());
        }

        public async Task<ICollection<Part>> GetPartsAsync(PaginatedRequest request)
        {
            return await _storageProvider.GetPartsAsync(request, _requestContext.GetUserContext());
        }

        public async Task<Part> AddPartAsync(Part part)
        {
            return await _storageProvider.AddPartAsync(part, _requestContext.GetUserContext());
        }

        public async Task<Part> UpdatePartAsync(Part part)
        {
            return await _storageProvider.UpdatePartAsync(part, _requestContext.GetUserContext());
        }

        public async Task<bool> DeletePartAsync(Part part)
        {
            return await _storageProvider.DeletePartAsync(part, _requestContext.GetUserContext());
        }

        public async Task<PartType> GetOrCreatePartTypeAsync(PartType partType)
        {
            return await _storageProvider.GetOrCreatePartTypeAsync(partType, _requestContext.GetUserContext());
        }

        public async Task<PartMetadata> GetPartMetadataAsync(string partNumber)
        {
            var dataSheets = new List<string>();
            var digikeyResponse = new Integrations.Models.Digikey.KeywordSearchResponse();
            ICollection<Integrations.Models.Mouser.MouserPart> mouserParts = new List<Integrations.Models.Mouser.MouserPart>();
            if (_octopartApi.IsConfigured)
                dataSheets.AddRange(await _octopartApi.GetDatasheetsAsync(partNumber));
            if (_digikeyApi.IsConfigured)
                digikeyResponse = await _digikeyApi.GetPartsAsync(partNumber);
            if (_mouserApi.IsConfigured)
                mouserParts = await _mouserApi.GetPartsAsync(partNumber);

            // find the most appropriate listing for each api
            var digikeyPart = digikeyResponse
                .Products
                .Where(x => !string.IsNullOrEmpty(x.PrimaryDatasheet))
                .OrderByDescending(x => x.QuantityAvailable)
                .FirstOrDefault();
            var mouserPart = mouserParts
                .Where(x => !string.IsNullOrEmpty(x.DataSheetUrl))
                .OrderByDescending(x => x.AvailabilityInteger)
                .FirstOrDefault();

            // determine lowest price information
            var digikeyPrice = digikeyPart?.UnitPrice;
            var mouserLowestQuantityPrice = mouserPart?.PriceBreaks?.OrderBy(x => x.Quantity).FirstOrDefault();
            var mouserPrice = mouserLowestQuantityPrice?.Cost;
            decimal? lowestCost = 0M;
            var lowestCostSupplier = "";
            var lowestCostCurrency = "";
            var lowestCostProductUrl = "";
            if (digikeyPrice <= mouserPrice)
            {
                lowestCostSupplier = "DigiKey";
                lowestCost = digikeyPrice;
                lowestCostCurrency = digikeyResponse?.SearchLocaleUsed?.Currency;
                lowestCostProductUrl = digikeyPart?.ProductUrl;
            }
            else
            {
                lowestCostSupplier = "Mouser";
                lowestCost = mouserPrice;
                lowestCostCurrency = mouserLowestQuantityPrice?.Currency;
                lowestCostProductUrl = mouserPart?.ProductDetailUrl;
            }

            // append all datasheets
            dataSheets.AddRange(digikeyResponse.Products.Select(x => x.PrimaryDatasheet).ToList());
            dataSheets.AddRange(mouserParts.Select(x => x.DataSheetUrl).ToList());

            // no results found at all
            if (digikeyPart == null && mouserPart == null && !dataSheets.Any())
                return null;

            // todo: cache datasheets and images when downloaded on datasheets.binner.io

            var metadata = new PartMetadata()
            {
                PartNumber = partNumber,
                DigikeyPartNumber = digikeyPart?.DigiKeyPartNumber,
                MouserPartNumber = mouserPart?.MouserPartNumber,
                DatasheetUrl = digikeyPart?.PrimaryDatasheet ?? mouserPart?.DataSheetUrl ?? dataSheets.FirstOrDefault(),
                AdditionalDatasheets = dataSheets.Distinct().ToList(),
                Description = digikeyPart?.ProductDescription ?? mouserPart?.Description,
                ManufacturerPartNumber = digikeyPart?.ManufacturerPartNumber ?? mouserPart?.ManufacturerPartNumber,
                DetailedDescription = digikeyPart?.DetailedDescription,
                Cost = lowestCost,
                Currency = lowestCostCurrency,
                LowestCostSupplier = lowestCostSupplier,
                LowestCostSupplierUrl = lowestCostProductUrl,
                Package = digikeyPart?.Parameters
                        ?.Where(x => x.Parameter.Equals("Package / Case", StringComparison.InvariantCultureIgnoreCase))
                        .Select(x => x.Value)
                        .FirstOrDefault(),
                Manufacturer = digikeyPart?.Manufacturer?.Value ?? mouserPart?.Manufacturer,
                ProductStatus = digikeyPart?.ProductStatus ?? mouserPart?.LifecycleStatus,
                ProductUrl = digikeyPart?.ProductUrl ?? mouserPart?.ProductDetailUrl,
                ImageUrl = digikeyPart?.PrimaryPhoto ?? mouserPart.ImagePath,
                Integrations = new Models.Integrations
                {
                    Digikey = digikeyPart,
                    Mouser = mouserPart,
                    AliExpress = null
                }
            };
            var partTypes = await _storageProvider.GetPartTypesAsync(_requestContext.GetUserContext());
            metadata.PartType = DeterminePartType(metadata, partTypes);
            metadata.Keywords = DetermineKeywords(metadata, partTypes);
            return metadata;
        }

        private ICollection<string> DetermineKeywords(PartMetadata metadata, ICollection<PartType> partTypes)
        {
            // part type
            // important parts from description
            // alternate series numbers etc
            var keywords = new List<string>();
            var possiblePartTypes = GetMatchingPartTypes(metadata, partTypes);
            foreach (var possiblePartType in possiblePartTypes)
                if (!keywords.Contains(possiblePartType.Key.Name, StringComparer.InvariantCultureIgnoreCase))
                    keywords.Add(possiblePartType.Key.Name);

            if (!keywords.Contains(metadata.ManufacturerPartNumber, StringComparer.InvariantCultureIgnoreCase))
                keywords.Add(metadata.ManufacturerPartNumber.ToLower());
            var desc = metadata.Description.ToLower().Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries);
            // add the first 4 words of desc
            var wordCount = 0;
            var ignoredWords = new string[] { "and", "the", "in", "or", "in", "a", };
            foreach (var word in desc)
            {
                if (!ignoredWords.Contains(word, StringComparer.InvariantCultureIgnoreCase) && !keywords.Contains(word, StringComparer.InvariantCultureIgnoreCase))
                {
                    keywords.Add(word);
                    wordCount++;
                }
                if (wordCount >= 4)
                    break;
            }
            var basePart = metadata.Integrations?.Digikey?.Parameters.Where(x => x.Parameter.Equals("Base Part Number")).Select(x => x.Value).FirstOrDefault();
            if (basePart != null && !keywords.Contains(basePart, StringComparer.InvariantCultureIgnoreCase))
                keywords.Add(basePart);
            var mountingType = metadata.Integrations?.Digikey?.Parameters.Where(x => x.Parameter.Equals("Mounting Type")).Select(x => x.Value).FirstOrDefault();
            if (mountingType != null && !keywords.Contains(mountingType, StringComparer.InvariantCultureIgnoreCase))
                keywords.Add(mountingType);

            return keywords.Distinct().ToList();
        }

        private IDictionary<PartType, int> GetMatchingPartTypes(PartMetadata metadata, ICollection<PartType> partTypes)
        {
            // load all part types
            var possiblePartTypes = new Dictionary<PartType, int>();
            foreach (var partType in partTypes)
            {
                var addPart = false;
                if (metadata.Description?.IndexOf(partType.Name, StringComparison.InvariantCultureIgnoreCase) >= 0)
                    addPart = true;
                if (metadata.DetailedDescription?.IndexOf(partType.Name, StringComparison.InvariantCultureIgnoreCase) >= 0)
                    addPart = true;
                if (metadata.PartNumber?.IndexOf(partType.Name, StringComparison.InvariantCultureIgnoreCase) >= 0)
                    addPart = true;
                if (metadata.DatasheetUrl?.IndexOf(partType.Name, StringComparison.InvariantCultureIgnoreCase) >= 0)
                    addPart = true;
                if (addPart)
                {
                    if (possiblePartTypes.ContainsKey(partType))
                        possiblePartTypes[partType]++;
                    else
                        possiblePartTypes.Add(partType, 1);
                }

            }
            return possiblePartTypes;
        }

        private string DeterminePartType(PartMetadata metadata, ICollection<PartType> partTypes)
        {
            var possiblePartTypes = GetMatchingPartTypes(metadata, partTypes);
            return possiblePartTypes
                .OrderByDescending(x => x.Value)
                .Select(x => x.Key?.Name)
                .FirstOrDefault();
        }
    }
}
