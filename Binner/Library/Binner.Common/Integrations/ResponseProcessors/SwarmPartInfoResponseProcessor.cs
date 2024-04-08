using Binner.Common.Integrations.Models;
using Binner.Model;
using Binner.Model.Configuration;
using Binner.Model.Swarm;
using Binner.SwarmApi.Response;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Binner.Common.Integrations.ResponseProcessors
{
    public class SwarmPartInfoResponseProcessor : IResponseProcessor
    {
        private const StringComparison ComparisonType = StringComparison.InvariantCultureIgnoreCase;
        private readonly ILogger _logger;
        private readonly WebHostServiceConfiguration _configuration;
        private readonly int _resultsRank;

        public SwarmPartInfoResponseProcessor(ILogger logger, WebHostServiceConfiguration configuration, int resultsRank)
        {
            _logger = logger;
            _configuration = configuration;
            _resultsRank = resultsRank;
        }

        public async Task ExecuteAsync(IIntegrationApi api, ProcessingContext context)
        {
            // fetch part info
            var response = await FetchPartsAsync(api, context);
            // merge response
            if (response != null)
                await ToCommonPartAsync(api, response, context);
        }

        private async Task<SearchPartResponse?> FetchPartsAsync(IIntegrationApi api, ProcessingContext context)
        {
            IApiResponse? apiResponse = null;
            try
            {
                apiResponse = await api.SearchAsync(context.PartNumber);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"[{nameof(SwarmApi)}]: {ex.GetBaseException().Message}");
                apiResponse = new ApiResponse(new List<string> { ex.GetBaseException().Message }, nameof(SwarmApi));

            }
            context.ApiResponses.Add(nameof(SwarmApi), new Model.Integrations.ApiResponseState(false, apiResponse));

            if (apiResponse.Warnings?.Any() == true)
            {
                _logger.LogWarning($"[{apiResponse.ApiName}]: {string.Join(". ", apiResponse.Warnings)}");
            }
            if (apiResponse.Errors?.Any() == true)
            {
                _logger.LogError($"[{apiResponse.ApiName}]: {string.Join(". ", apiResponse.Errors)}");
            }

            var swarmResponse = (SearchPartResponse?)apiResponse.Response ?? new SearchPartResponse();
            context.ApiResponses[nameof(SwarmApi)].IsSuccess = swarmResponse.Parts?.Any() == true;
            return swarmResponse;
        }

        private Task ToCommonPartAsync(IIntegrationApi api, SearchPartResponse response, ProcessingContext context)
        {
            var imagesAdded = 0;
            foreach (var swarmPart in response.Parts)
            {
                var part = new PartNumber
                {
                    AlternateDescription = swarmPart.AlternateDescription,
                    AlternateNames = swarmPart.AlternateNames,
                    CreatedFromSupplierId = swarmPart.CreatedFromSupplierId,
                    DateCreatedUtc = swarmPart.DateCreatedUtc,
                    DatePrunedUtc = swarmPart.DatePrunedUtc,
                    DefaultImageId = swarmPart.DefaultImageId,
                    DefaultImageResourcePath = swarmPart.DefaultImageResourcePath,
                    DefaultImageResourceSourceUrl = swarmPart.DefaultImageResourceSourceUrl,
                    Description = swarmPart.Description,
                    Name = swarmPart.Name,
                    PartNumberId = swarmPart.PartNumberId,
                    PartTypeId = swarmPart.PartTypeId,
                    PrimaryDatasheetId = swarmPart.PrimaryDatasheetId,
                    ResourceId = swarmPart.ResourceId,
                    Source = (DataSource)(int)swarmPart.Source,
                    SwarmPartNumberId = swarmPart.SwarmPartNumberId,
                    PartNumberManufacturers = swarmPart.PartNumberManufacturers?.Select(x => new PartNumberManufacturer
                    {
                        AlternateDescription = x.AlternateDescription,
                        AlternateNames = x.AlternateNames,
                        CreatedFromSupplierId = x.CreatedFromSupplierId,
                        Datasheets = x.Datasheets.Select(d => new DatasheetBasic
                        {
                            BasePartNumber = d.BasePartNumber,
                            DatasheetId = d.DatasheetId,
                            DocumentType = (PdfDocumentTypes)(int)d.DocumentType,
                            ImageCount = d.ImageCount,
                            ManufacturerName = d.ManufacturerName,
                            OriginalUrl = d.OriginalUrl,
                            PageCount = d.PageCount,
                            ProductUrl = d.ProductUrl,
                            ResourceId = d.ResourceId,
                            ResourcePath = d.ResourcePath,
                            ResourceSourceUrl = d.ResourceSourceUrl,
                            ShortDescription = d.ShortDescription,
                            Title = d.Title
                        }).ToList(),
                        DateCreatedUtc = x.DateCreatedUtc,
                        DatePrunedUtc = x.DatePrunedUtc,
                        DefaultPartNumberManufacturerImageMetadataId = x.DefaultPartNumberManufacturerImageMetadataId,
                        //Description = x.Description,
                        ImageMetadata = x.ImageMetadata.Select(i => new PartNumberManufacturerImageMetadata
                        {
                            CreatedFromSupplierId = i.CreatedFromSupplierId,
                            ImageId = i.ImageId,
                            ImageType = (ImageTypes)(int)i.ImageType,
                            IsDefault = i.IsDefault,
                            OriginalUrl = i.OriginalUrl,
                            PartNumberManufacturerId = i.PartNumberManufacturerId,
                            PartNumberManufacturerImageMetadataId = i.PartNumberManufacturerImageMetadataId,
                            ResourcePath = i.ResourcePath,
                            ResourceSourceUrl = i.ResourceSourceUrl
                        }).ToList(),
                        IsObsolete = x.IsObsolete,
                        Keywords = x.Keywords.Select(k => new Keyword
                        {
                            KeywordId = k.KeywordId,
                            Name = k.Name
                        }).ToList(),
                        ManufacturerId = x.ManufacturerId,
                        ManufacturerName = x.ManufacturerName,
                        Name = x.Name,
                        Package = x.Package.Select(p => new Model.Swarm.Package
                        {
                            Name = p.Name,
                            PackageId = p.PackageId,
                            PinCount = p.PinCount,
                            SizeDepthMm = p.SizeDepthMm,
                            SizeHeightMm = p.SizeHeightMm,
                            SizeWidthMm = p.SizeWidthMm
                        }).ToList(),
                        Parametrics = x.Parametrics.Select(p => new PartNumberManufacturerParametric
                        {
                            Name = p.Name,
                            ParametricType = (ParametricTypes)(int)p.ParametricType,
                            PartNumberManufacturerId = p.PartNumberManufacturerId,
                            PartNumberManufacturerParametricId = p.PartNumberManufacturerParametricId,
                            Units = (ParametricUnits?)(int?)p.Units,
                            ValueAsBool = p.ValueAsBool,
                            ValueAsDouble = p.ValueAsDouble,
                            ValueAsString = p.ValueAsString
                        }).ToList(),
                        PartNumberId = x.PartNumberId,
                        PartNumberManufacturerId = x.PartNumberManufacturerId,
                        PartTypeId = x.PartTypeId,
                        PrimaryDatasheetId = x.PrimaryDatasheetId,
                        Source = (DataSource)(int)x.Source,
                        Suppliers = x.Suppliers.Select(s => new PartNumberManufacturerSupplierBasic
                        {
                            Cost = s.Cost,
                            Currency = s.Currency,
                            DateCreatedUtc = s.DateCreatedUtc,
                            FactoryLeadTime = s.FactoryLeadTime,
                            FactoryStockAvailable = s.FactoryStockAvailable,
                            MinimumOrderQuantity = s.MinimumOrderQuantity,
                            Packaging = s.Packaging,
                            PartNumberManufacturerId = s.PartNumberManufacturerId,
                            PartNumberManufacturerSupplierId = s.PartNumberManufacturerSupplierId,
                            ProductUrl = s.ProductUrl,
                            QuantityAvailable = s.QuantityAvailable,
                            StockLastUpdatedUtc = s.StockLastUpdatedUtc,
                            SupplierId = s.SupplierId,
                            SupplierName = s.SupplierName,
                            SupplierPartNumber = s.SupplierPartNumber
                        }).ToList(),
                        SwarmPartNumberId = x.SwarmPartNumberId
                    }).ToList()
                };
                var defaultImageUrl = GetDefaultImageUrl(part);
                if (!string.IsNullOrEmpty(defaultImageUrl))
                    context.Results.ProductImages.Add(new NameValuePair<string>(part.Name, defaultImageUrl));
                if (part.PartNumberManufacturers?.Any() == true)
                {
                    foreach (var manufacturerPart in part.PartNumberManufacturers)
                    {
                        foreach (var image in manufacturerPart.ImageMetadata
                            .OrderByDescending(x => x.IsDefault)
                            .ThenBy(x => x.ImageType))
                        {
                            var imageUrl = GetImageUrl(image);
                            if (!string.IsNullOrEmpty(imageUrl)
                                && !context.Results.ProductImages.Any(x => x.Value?.Equals(imageUrl, ComparisonType) == true)
                                && imagesAdded < ProcessingContext.MaxImagesPerSupplier && context.Results.ProductImages.Count < ProcessingContext.MaxImagesTotal)
                            {
                                imagesAdded++;
                                context.Results.ProductImages.Add(new NameValuePair<string>(manufacturerPart.Name, imageUrl));
                            }
                        }

                        foreach (var datasheet in manufacturerPart.Datasheets.OrderByDescending(x => x.DatasheetId == manufacturerPart.PrimaryDatasheetId))
                        {
                            var datasheetUrl = GetDatasheetUrl(datasheet);
                            var datasheetCoverImageUrl = GetDatasheetCoverImageUrl(datasheet);
                            if (!string.IsNullOrEmpty(datasheetUrl) && !context.Results.Datasheets.Any(x => x.Value?.DatasheetUrl.Equals(datasheetUrl, ComparisonType) == true))
                            {
                                var datasheetSource = new DatasheetSource(datasheet.ResourceId,
                                    datasheet.ImageCount,
                                    datasheet.PageCount,
                                    datasheetCoverImageUrl,
                                    datasheetUrl,
                                    datasheet.Title ?? manufacturerPart.Name,
                                    datasheet?.ShortDescription,
                                    datasheet?.ManufacturerName,
                                    datasheet?.OriginalUrl,
                                    datasheet?.ProductUrl);
                                context.Results.Datasheets.Add(new NameValuePair<DatasheetSource>(manufacturerPart.Name, datasheetSource));
                            }
                        }

                        var mountingType = manufacturerPart.Parametrics
                            .Where(x => x.Name.Equals("Mounting Type", ComparisonType))
                            .Select(x => x.ValueAsString)
                            .FirstOrDefault();
                        var mountingTypeId = MountingType.None;
                        Enum.TryParse<MountingType>(mountingType, out mountingTypeId);

                        foreach (var supplierPart in manufacturerPart.Suppliers)
                        {
                            context.Results.Parts.Add(new CommonPart
                            {
                                Rank = _resultsRank,
                                SwarmPartNumberManufacturerId = manufacturerPart.PartNumberManufacturerId,
                                Supplier = supplierPart.SupplierName,
                                SupplierPartNumber = supplierPart.SupplierPartNumber,
                                BasePartNumber = part.Name,
                                Manufacturer = manufacturerPart.ManufacturerName,
                                ManufacturerPartNumber = manufacturerPart.Name,
                                Cost = supplierPart.Cost ?? 0,
                                Currency = supplierPart.Currency,
                                Description = !string.IsNullOrEmpty(manufacturerPart.Description)
                                    ? manufacturerPart.Description
                                    : part.Description,
                                DatasheetUrls = GetDatasheetUrls(manufacturerPart),
                                ImageUrl = GetDefaultManufacturerImageUrl(manufacturerPart),
                                PackageType = manufacturerPart.Package.Select(x => x.Name).FirstOrDefault() ??
                                              string.Empty,
                                MountingTypeId = (int)mountingTypeId,
                                PartType = context.PartTypes.Where(x => x.PartTypeId == part.PartTypeId).Select(x => x.Name)
                                    .FirstOrDefault() ?? string.Empty,
                                ProductUrl = supplierPart.ProductUrl,
                                Status = manufacturerPart.IsObsolete ? "Inactive" : "Active",
                                QuantityAvailable = supplierPart.QuantityAvailable ?? 0,
                                MinimumOrderQuantity = supplierPart.MinimumOrderQuantity ?? 0,
                                FactoryStockAvailable = supplierPart.FactoryStockAvailable ?? 0,
                                FactoryLeadTime = supplierPart.FactoryLeadTime,
                            });
                        }
                    }
                }

            }

            return Task.CompletedTask;

            static string GetDatasheetUrl(DatasheetBasic datasheet)
            {
                return $"https://{datasheet.ResourceSourceUrl}/{datasheet.ResourcePath}.pdf";
            }

            static string GetDatasheetCoverImageUrl(DatasheetBasic datasheet)
            {
                if (datasheet.ImageCount > 0)
                    return $"https://{datasheet.ResourceSourceUrl}/{datasheet.ResourcePath}_1.png";
                return $"https://{datasheet.ResourceSourceUrl}/{ProcessingContext.MissingDatasheetCoverName}";
            }

            static ICollection<string> GetDatasheetUrls(PartNumberManufacturer part)
            {
                var urls = new List<string>();
                if (part.Datasheets.Any())
                {
                    foreach (var datasheet in part.Datasheets)
                    {
                        var datasheetUrl = GetDatasheetUrl(datasheet);
                        if (!string.IsNullOrEmpty(datasheetUrl) && !urls.Contains(datasheetUrl))
                            urls.Add(datasheetUrl);
                    }
                }
                return urls;
            }

            static string? GetDefaultManufacturerImageUrl(PartNumberManufacturer part)
            {
                var firstImage = part.ImageMetadata?
                    .OrderByDescending(x => x.IsDefault)
                    .ThenBy(x => x.ImageType)
                    .FirstOrDefault();
                if (firstImage != null)
                {
                    return GetImageUrl(firstImage);
                }
                return null;
            }

            static string GetImageUrl(PartNumberManufacturerImageMetadata image)
            {
                return $"https://{image.ResourceSourceUrl}/{image.ResourcePath}_{image.ImageId}.png";
            }

            static string GetDefaultImageUrl(PartNumber part)
            {
                if (part.DefaultImageId != null && !string.IsNullOrEmpty(part.DefaultImageResourcePath) && !string.IsNullOrEmpty(part.DefaultImageResourceSourceUrl))
                    return $"https://{part.DefaultImageResourceSourceUrl}/{part.DefaultImageResourcePath}_{part.DefaultImageId}.png";
                return string.Empty;
            }
        }
    }
}
