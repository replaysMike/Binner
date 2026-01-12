using Binner.Common.Extensions;
using Binner.Global.Common;
using Binner.Model;
using System;
using System.Diagnostics;
using System.Reflection;

namespace Binner.Services.Integrations
{
    public class BaseIntegrationBehavior : IBaseIntegrationBehavior
    {
        protected const StringComparison ComparisonType = StringComparison.InvariantCultureIgnoreCase;
        protected readonly IStorageProvider _storageProvider;
        protected readonly IRequestContextAccessor _requestContext;

        public BaseIntegrationBehavior(IStorageProvider storageProvider, IRequestContextAccessor requestContextAccessor)
        {
            _storageProvider = storageProvider;
            _requestContext = requestContextAccessor;
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
            var desc = part.Description?.ToLower().Split(new[] { " ", "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
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
        {
            // note: partTypes call is cached
            var partTypes = await _storageProvider.GetPartTypesAsync(_requestContext.GetUserContext());
            var possiblePartTypes = GetMatchingPartTypes(part, partTypes);
            var bestGuessPartType = possiblePartTypes
                .OrderByDescending(x => x.Value)
                .Select(x => x.Key)
                .FirstOrDefault();
            return bestGuessPartType;
        }

        public PartTypeInfoAttribute? GetPartTypeInfo(SystemDefaults.DefaultPartTypes partTypeEnum)
        {
            var typeOfEnum = partTypeEnum.GetType();
            var fi = typeOfEnum.GetField(partTypeEnum.ToString());

            //get the attribute from the field
            return fi.GetCustomAttributes(typeof(PartTypeInfoAttribute), false).
                FirstOrDefault()
                as PartTypeInfoAttribute;
        }

        public virtual IDictionary<PartType, int> GetMatchingPartTypes(CommonPart part, ICollection<PartType> partTypes)
        {
            // load all part types
            var possiblePartTypes = new Dictionary<PartType, int>();
            var depth = 0;
            var categories = part.Categories.SelectRecursive(x => x.ChildCategories)
                .Select(x =>
                {
                    depth++;
                    return new { x.Name, x.Description, Priority = depth };
                }).ToList();
            foreach (var partType in partTypes)
            {
                var defaultPriority = 1;
                if (string.IsNullOrEmpty(partType.Name))
                    continue;
                var addPart = false;

                // do we already have an exact name match?
                if (!string.IsNullOrEmpty(part.PartType) && part.PartType.Equals(partType.Name, StringComparison.InvariantCultureIgnoreCase))
                {
                    addPart = true;
                    defaultPriority += 3;
                }

                // check the categories on the part. if it matches on category, base the priority on the deepest category as highest
                var nameResult = categories.Where(x => x.Name.Contains(partType.Name, ComparisonType)).FirstOrDefault();
                if(nameResult != null)
                {
                    addPart = true;
                    defaultPriority += nameResult.Priority + 2;
                }
                var descriptionResult = categories.Where(x => x.Description?.Contains(partType.Name, ComparisonType) == true).FirstOrDefault();
                if (descriptionResult != null)
                {
                    addPart = true;
                    defaultPriority += descriptionResult.Priority + 1;
                }

                // check the keywords on the part type
                var keywords = partType.Keywords?.Split([',']).ToList() ?? new List<string>();
                var defaultPartType = (SystemDefaults.DefaultPartTypes)partType.PartTypeId;
                var info = GetPartTypeInfo(defaultPartType);
                if (info != null && !string.IsNullOrEmpty(info.Keywords))
                    keywords.AddRange(info.Keywords.Split([',']));
                keywords = keywords.Distinct().ToList();

                foreach (var keyword in keywords)
                {
                    if (part.Description?.Contains(keyword, ComparisonType) == true)
                    {
                        addPart = true;
                        defaultPriority += 1;
                    }
                    // also check the keyword in the categories
                    var nameKeywordResult = categories.Where(x => x.Name.Contains(keyword, ComparisonType)).FirstOrDefault();
                    if (nameKeywordResult != null)
                    {
                        addPart = true;
                        defaultPriority += nameKeywordResult.Priority + 1;
                    }
                    var descriptionKeywordResult = categories.Where(x => x.Description?.Contains(keyword, ComparisonType) == true).FirstOrDefault();
                    if (descriptionKeywordResult != null)
                    {
                        addPart = true;
                        defaultPriority += descriptionKeywordResult.Priority;
                    }
                }

                if (part.ManufacturerPartNumber?.IndexOf(partType.Name, ComparisonType) >= 0)
                    addPart = true;
                foreach (var datasheet in part.DatasheetUrls)
                    if (datasheet.IndexOf(partType.Name, ComparisonType) >= 0)
                    {
                        addPart = true;
                        defaultPriority = 0;
                    }
                if (addPart)
                {
                    if (possiblePartTypes.ContainsKey(partType))
                        possiblePartTypes[partType]++;
                    else
                        possiblePartTypes.Add(partType, defaultPriority);
                }

            }
            return possiblePartTypes;
        }
    }
}
