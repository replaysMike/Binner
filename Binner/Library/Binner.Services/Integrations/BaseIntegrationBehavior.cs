using Binner.Common.Extensions;
using Binner.Global.Common;
using Binner.Model;
using Microsoft.Extensions.Logging;

namespace Binner.Services.Integrations
{
    public class BaseIntegrationBehavior : IBaseIntegrationBehavior
    {
        protected const StringComparison ComparisonType = StringComparison.InvariantCultureIgnoreCase;
        private readonly ILogger<BaseIntegrationBehavior> _logger;
        protected readonly IStorageProvider _storageProvider;
        protected readonly IRequestContextAccessor _requestContext;
        protected readonly IPartTypeDetection<CommonPart> _partTypeDetection;

        public BaseIntegrationBehavior(ILogger<BaseIntegrationBehavior> logger, IStorageProvider storageProvider, IRequestContextAccessor requestContextAccessor, IPartTypeDetection<CommonPart> partTypeDetection)
        {
            _logger = logger;
            _storageProvider = storageProvider;
            _requestContext = requestContextAccessor;
            _partTypeDetection = partTypeDetection;
        }

        public virtual async Task<List<CommonPart>> MapCommonPartIdsAsync(List<CommonPart> parts)
        {
            var partNumbers = parts.Select(x => x.ManufacturerPartNumber ?? string.Empty).ToList();
            var partIds = await _storageProvider.GetPartIdsFromManufacturerPartNumbersAsync(partNumbers, _requestContext.GetUserContext());
            foreach (var part in parts)
            {
                var key = part.ManufacturerPartNumber ?? string.Empty;
                if (partIds.ContainsKey(key))
                    part.PartId = partIds[key];
            }
            return parts;
        }

        public virtual ICollection<string> DetermineKeywordsFromPart(CommonPart part, ICollection<PartType> partTypes)
        {
            // part type
            // important parts from description
            // alternate series numbers etc
            var keywords = new List<string>();
            var possiblePartTypes = GetMatchingPartTypes(part, partTypes);
            foreach (var possiblePartType in possiblePartTypes)
            {
                if (!string.IsNullOrEmpty(possiblePartType.Key.Name) && !keywords.Contains(possiblePartType.Key.Name, StringComparer.InvariantCultureIgnoreCase))
                {
                    keywords.Add(possiblePartType.Key.Name.ToLower());
                }
            }

            if (!string.IsNullOrEmpty(part.ManufacturerPartNumber) && !keywords.Contains(part.ManufacturerPartNumber, StringComparer.InvariantCultureIgnoreCase))
                keywords.Add(part.ManufacturerPartNumber.ToLower());
            var desc = part.Description?.ToLower().Split([" ", "\r\n"], StringSplitOptions.RemoveEmptyEntries);
            // add the first 4 words of desc
            var wordCount = 0;
            var ignoredWords = new[] { "and", "the", "in", "or", "in", "a", };
            if (desc != null)
            {
                foreach (var word in desc)
                {
                    if (word.Length > 1 && !ignoredWords.Contains(word, StringComparer.InvariantCultureIgnoreCase) &&
                        !keywords.Contains(word, StringComparer.InvariantCultureIgnoreCase))
                    {
                        keywords.Add(word.ToLower());
                        wordCount++;
                    }

                    if (wordCount >= 4)
                        break;
                }
            }

            foreach (var basePart in part.AdditionalPartNumbers)
                if (basePart.Length > 1 && !keywords.Contains(basePart, StringComparer.InvariantCultureIgnoreCase))
                    keywords.Add(basePart);
            var mountingType = (MountingType)part.MountingTypeId;
            if (mountingType != MountingType.None && !string.IsNullOrEmpty(mountingType.ToString()) && !keywords.Contains(mountingType.ToString(), StringComparer.InvariantCultureIgnoreCase))
                keywords.Add(mountingType.ToString().ToLower());

            return keywords.Distinct().ToList();
        }

        public virtual async Task<PartType?> DeterminePartTypeAsync(CommonPart part)
            => await _partTypeDetection.DeterminePartTypeAsync(part);

        public PartTypeInfoAttribute? GetPartTypeInfo(SystemDefaults.DefaultPartTypes? partTypeEnum)
            => _partTypeDetection.GetPartTypeInfo(partTypeEnum);

        public virtual string RemovePlurals(string input)
            => _partTypeDetection.RemovePlurals(input);

        public virtual IDictionary<PartType, int> GetMatchingPartTypes(CommonPart part, ICollection<PartType> partTypes)
            => _partTypeDetection.GetMatchingPartTypes(part, partTypes);
    }
}
