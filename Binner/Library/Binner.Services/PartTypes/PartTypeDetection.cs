using Binner.Common.Extensions;
using Binner.Global.Common;
using Binner.Model;

namespace Binner.Services.PartTypes
{
    public class PartTypeDetection : IPartTypeDetection<CommonPart>
    {
        protected const StringComparison ComparisonType = StringComparison.InvariantCultureIgnoreCase;
        protected readonly IStorageProvider _storageProvider;
        protected readonly IRequestContextAccessor _requestContext;

        public PartTypeDetection(IStorageProvider storageProvider, IRequestContextAccessor requestContextAccessor)
        {
            _storageProvider = storageProvider;
            _requestContext = requestContextAccessor;
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

            // if we chose a parent category when there is a more specific child category available, choose it instead
            if (bestGuessPartType != null && possiblePartTypes.Any(x => x.Key.ParentPartTypeId == bestGuessPartType.PartTypeId))
                bestGuessPartType = possiblePartTypes.Where(x => x.Key.ParentPartTypeId == bestGuessPartType.PartTypeId).Select(x => x.Key).FirstOrDefault();

            // default to Other if we can't find a match
            return bestGuessPartType ?? partTypes.FirstOrDefault(x => x.Name == "Other");
        }

        public virtual IDictionary<PartType, int> GetMatchingPartTypes(CommonPart part, ICollection<PartType> partTypes)
        {
            string[] wordDelimiters = [" ", ";", ":", ","];
            // load all part types
            var possiblePartTypes = new Dictionary<PartType, int>();
            var depth = 0;
            var categories = part.Categories.SelectRecursive(x => x.ChildCategories)
                .Select(x =>
                {
                    depth++;
                    return new { x.Name, x.Description, Priority = depth };
                }).ToList();

            var descriptionWords = part.Description?.ToLower().Split(wordDelimiters, StringSplitOptions.RemoveEmptyEntries).Distinct().ToArray() ?? Array.Empty<string>();
            for (var i = 0; i < descriptionWords.Length; i++)
                descriptionWords[i] = RemovePlurals(descriptionWords[i]) ?? string.Empty;
            var partTypeWords = part.PartType?.ToLower().Split(wordDelimiters, StringSplitOptions.RemoveEmptyEntries).Distinct().ToArray() ?? Array.Empty<string>();
            for (var i = 0; i < partTypeWords.Length; i++)
                partTypeWords[i] = RemovePlurals(partTypeWords[i]) ?? string.Empty;

            foreach (var partType in partTypes)
            {
                var defaultPriority = 1;
                if (string.IsNullOrEmpty(partType.Name))
                    continue;
                var addPart = false;

                var partTypeName = RemovePlurals(partType.Name);
                partTypeName = partTypeName?.ToLower() ?? string.Empty;
                var partPartType = RemovePlurals(part.PartType) ?? string.Empty;

                // do we already have an exact name match?
                if (!string.IsNullOrEmpty(partPartType) && partPartType.Equals(partTypeName, ComparisonType))
                {
                    addPart = true;
                    defaultPriority += 3;
                }

                // check the categories on the part. if it matches on category, base the priority on the deepest category as highest
                var nameResult = categories.Where(x => RemovePlurals(x.Name)?.Contains(partTypeName, ComparisonType) == true).FirstOrDefault();
                if (nameResult != null)
                {
                    addPart = true;
                    defaultPriority += nameResult.Priority + 2;
                }

                // get matches on the part type if it contains multiple words
                var index = Array.IndexOf(partTypeWords, partTypeName);
                if (index >= 0)
                {
                    addPart = true;
                    // calculate a priority based on how early in the description the part type is found
                    var oldRange = partTypeWords.Length - 0;
                    var newRange = (10 - 1);
                    defaultPriority = 10 - (((index - 0) * newRange) / oldRange);
                }

                // get matches on the description if it contains multiple words
                index = Array.IndexOf(descriptionWords, partTypeName);
                if (index >= 0)
                {
                    addPart = true;
                    // calculate a priority based on how early in the description the part type is found
                    var oldRange = descriptionWords.Length - 0;
                    var newRange = (5 - 1);
                    defaultPriority = 5 - (((index - 0) * newRange) / oldRange);
                }

                // check the keywords on the part type
                var keywords = partType.Keywords?.Split([','], StringSplitOptions.RemoveEmptyEntries).ToList() ?? new List<string>();
                for (var i = 0; i < keywords.Count; i++)
                {
                    keywords[i] = RemovePlurals(keywords[i])?.ToLower() ?? string.Empty;
                }
                var defaultPartType = (SystemDefaults.DefaultPartTypes?)partType.PartTypeId;
                if (defaultPartType != null)
                {
                    var info = GetPartTypeInfo(defaultPartType);
                    if (info != null && !string.IsNullOrEmpty(info.Keywords))
                        keywords.AddRange(info.Keywords.Split([','], StringSplitOptions.RemoveEmptyEntries));
                }
                keywords = keywords.Distinct().ToList();

                foreach (var keyword in keywords)
                {
                    var keywordWords = keyword.Split(wordDelimiters, StringSplitOptions.RemoveEmptyEntries);
                    var keywordWordCount = keywordWords.Length;
                    
                    if (partPartType.Equals(keyword, ComparisonType) == true)
                    {
                        addPart = true;
                        defaultPriority += keywordWordCount * 3;
                    }
                    var partTypeCrossMatches = partTypeWords.Intersect(keywordWords, StringComparer.InvariantCultureIgnoreCase);
                    if (partTypeCrossMatches.Count() == keywordWords.Length)
                    {
                        addPart = true;
                        defaultPriority += partTypeCrossMatches.Count() * 3;
                    }
                    var descriptionCrossMatches = descriptionWords.Intersect(keywordWords, StringComparer.InvariantCultureIgnoreCase);
                    if (descriptionCrossMatches.Count() == keywordWords.Length)
                    {
                        addPart = true;
                        defaultPriority += descriptionCrossMatches.Count() * 2;
                    }
                    if (part.ManufacturerPartNumber?.Contains(keyword, ComparisonType) == true)
                    {
                        addPart = true;
                        defaultPriority += keywordWordCount * 2;
                    }
                    if (part.BasePartNumber?.Contains(keyword, ComparisonType) == true)
                    {
                        addPart = true;
                        defaultPriority += keywordWordCount * 2;
                    }
                    // low priority match as this could misclassify parts easily on short keywords, but if there are multiple keywords that match it could increase confidence
                    if ((keywordWordCount > 1 || keyword.Length > 4) && part.Description?.Contains(keyword, ComparisonType) == true)
                    {
                        addPart = true;
                        defaultPriority += keywordWordCount * 1;
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

                if (part.ManufacturerPartNumber?.IndexOf(partTypeName, ComparisonType) >= 0)
                    addPart = true;
                foreach (var datasheet in part.DatasheetUrls)
                {
                    if (datasheet.IndexOf(partTypeName, ComparisonType) >= 0)
                    {
                        addPart = true;
                        defaultPriority = 0;
                    }
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

        public virtual string? RemovePlurals(string? input)
        {
            if (string.IsNullOrEmpty(input)) return input;
            var whitelist = new List<string> { "res", "trans" };
            if (!whitelist.Contains(input) && input.EndsWith("s", ComparisonType))
                return input.Substring(0, input.Length - 1);
            return input;
        }

        public PartTypeInfoAttribute? GetPartTypeInfo(SystemDefaults.DefaultPartTypes? partTypeEnum)
        {
            if (partTypeEnum != null)
            {
                var typeOfEnum = partTypeEnum.GetType();
                var fieldName = partTypeEnum.ToString() ?? string.Empty;
                var fi = typeOfEnum.GetField(fieldName);

                //get the attribute from the field
                return fi?.GetCustomAttributes(typeof(PartTypeInfoAttribute), false).
                    FirstOrDefault()
                    as PartTypeInfoAttribute;
            }
            return null;
        }
    }
}
