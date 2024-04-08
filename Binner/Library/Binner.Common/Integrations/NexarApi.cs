using Binner.Common.Integrations.Models;
using Binner.Model.Configuration;
using Binner.Model.Configuration.Integrations;
using Binner.Model.Integrations.Nexar;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Nexar.Client;
using Nexar.Client.Token;
using StrawberryShake;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace Binner.Common.Integrations
{
    public class NexarApi : IIntegrationApi
    {
        public string Name => "Nexar";
        private readonly OctopartConfiguration _configuration;
        private readonly LocaleConfiguration _localeConfiguration;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private static readonly TimeSpan TokenLifetime = TimeSpan.FromDays(1);
        private static DateTime _nexarTokenExpiresAt;
        private static string? _nexarToken;

        public bool IsEnabled => _configuration.Enabled;
        
        public IApiConfiguration Configuration => _configuration;

        public NexarApi(OctopartConfiguration configuration, LocaleConfiguration localeConfiguration, IHttpContextAccessor httpContextAccessor)
        {
            _configuration = configuration;
            _localeConfiguration = localeConfiguration;
            _httpContextAccessor = httpContextAccessor;
        }

        private static NexarClient CreateNexarClient()
        {
            var serviceCollection = new ServiceCollection();
            serviceCollection
                .AddNexarClient()
                .ConfigureHttpClient(httpClient =>
                {
                    httpClient.BaseAddress = new Uri("https://api.nexar.com/graphql");
                    httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_nexarToken}");
                });
            var services = serviceCollection.BuildServiceProvider();
            return services.GetRequiredService<NexarClient>();
        }

        private async Task UpdateNexarToken()
        {
            // use the existing not expired token
            if (_nexarToken != null && DateTime.UtcNow < _nexarTokenExpiresAt)
                return;

            if (string.IsNullOrEmpty(_configuration.ClientId))
                throw new UnauthorizedAccessException("Nexar Api does not have a configured ClientId");
            if (string.IsNullOrEmpty(_configuration.ClientSecret))
                throw new UnauthorizedAccessException("Nexar Api does not have a configured ClientSecret");

            // get the token
            using var httpClient = new HttpClient();
            _nexarTokenExpiresAt = DateTime.UtcNow + TokenLifetime;
            _nexarToken = await httpClient.GetNexarTokenAsync(_configuration.ClientId, _configuration.ClientSecret);
        }

        public Task<IApiResponse> SearchAsync(string partNumber, int recordCount = 25, Dictionary<string, string>? additionalOptions = null)
            => SearchAsync(partNumber, string.Empty, string.Empty, recordCount, additionalOptions);

        public Task<IApiResponse> SearchAsync(string partNumber, string partType, int recordCount = 25, Dictionary<string, string>? additionalOptions = null)
            => SearchAsync(partNumber, partType, string.Empty, recordCount, additionalOptions);

        public async Task<IApiResponse> SearchAsync(string partNumber, string partType, string mountingType, int recordCount = 25, Dictionary<string, string>? additionalOptions = null)
        {
            var nexarClient = CreateNexarClient();
            // update the token
            await UpdateNexarToken();
            //var result = await nexarClient.SearchMpn.ExecuteAsync(partNumber);
            var result = await nexarClient.PartSearch.ExecuteAsync(partNumber, recordCount);
            result.EnsureNoErrors();

            var partResults = new NexarPartResults();
            // return empty response
            if (result.Data?.SupSearch.Results == null)
                return new ApiResponse(partResults, nameof(NexarApi));

            foreach (var it in result.Data.SupSearch.Results)
            {
                var part = new NexarPart
                {
                    BaseNumber = it.Part.GenericMpn,
                    BestDatasheet = new Document
                    {
                        Name= it.Part.BestDatasheet?.Name,
                        PageCount = it.Part.BestDatasheet?.PageCount ?? 0,
                        SourceUrl = it.Part.BestDatasheet?.CreditUrl,
                        Url = it.Part.BestDatasheet?.Url,
                    },
                    BestImageUrl = it.Part.BestImage?.Url,
                    Category = new Category
                    {
                        Id = it.Part.Category?.Id,
                        Name= it.Part.Category?.Name,
                        ParentId = it.Part.Category?.ParentId,
                        Path = it.Part.Category?.Path,
                    },
                    Descriptions = it.Part.Descriptions?.Select(x => new Description
                    {
                        Text = x.Text,
                        CreditString = x.CreditString,
                        CreditUrl = x.CreditUrl
                    }).ToList() ?? new List<Description>(),
                    Documents = it.Part.DocumentCollections?.SelectMany(x => x.Documents.Select(y => new Document
                    {
                        Name = y.Name,
                        PageCount = y.PageCount ?? 0,
                        SourceUrl = y.CreditUrl,
                        Url = y.Url
                    })).ToList() ?? new List<Document>(),
                    GenericMpn = it.Part.GenericMpn,
                    Images = it.Part.Images?.Select(x => new Image
                    {
                        CreditString = x.CreditString,
                        CreditUrl = x.CreditUrl,
                        Url = x.Url
                    }).ToList() ?? new List<Image>(),
                    Manufacturer = new Manufacturer
                    {
                        Id = it.Part.Manufacturer?.Id,
                        Name = it.Part.Manufacturer?.Name,
                        Aliases = it.Part.Manufacturer?.Aliases.Select(x => x).ToList() ?? new List<string>(),
                        Url = it.Part.Manufacturer?.HomepageUrl,
                    },
                    Name = it.Part.Mpn,
                    ManufacturerPartNumber = it.Part.Mpn,
                    ManufacturerUrl = it.Part.ManufacturerUrl,
                    MedianPrice1000 = new PriceValue
                    {
                      Currency = it.Part.MedianPrice1000?.Currency ?? _localeConfiguration.Currency.ToString().ToUpper(),
                      Price = it.Part.MedianPrice1000?.Price ?? 0,
                      Quantity = it.Part.MedianPrice1000?.Quantity ?? 0
                    },
                    Sellers = it.Part.Sellers?.Select(x => new Seller
                    {
                        Company = new Company
                        {
                            Id = x.Company.Id,
                            Name = x.Company.Name,
                            Url = x.Company.HomepageUrl
                        },
                        IsAuthorized = x.IsAuthorized,
                        Offers = x.Offers.Select(y => new Offer
                        {
                            InventoryLevel = y.InventoryLevel,
                            Moq = y.Moq ?? 0,
                            Packaging = y.Packaging,
                            Url = y.ClickUrl
                        }).ToList(),
                    }).ToList() ?? new List<Seller>(),
                    Specs = it.Part.Specs?.Select(x => new Spec
                    {
                        DisplayValue = x.DisplayValue,
                        Units = x.Units,
                        UnitsName = x.UnitsName,
                        Value = x.Value,
                        ValueType = x.ValueType,
                        Attribute = new Binner.Model.Integrations.Nexar.Attribute
                        {
                            Id = x.Attribute.Id,
                            Name = x.Attribute.Name,
                            ShortName = x.Attribute.Shortname,
                        }
                    }).ToList() ?? new List<Spec>(),
                    TotalAvail = it.Part.TotalAvail,
                    ShortDescription = it.Part.ShortDescription,
                    ManufacturerName = it.Part.Manufacturer?.Name
                };
                partResults.Parts.Add(part);
            }

            return new ApiResponse(partResults, nameof(NexarApi));
        }

        public Task<IApiResponse> GetOrderAsync(string orderId, Dictionary<string, string>? additionalOptions = null)
        {
            throw new NotImplementedException();
        }

        public Task<IApiResponse> GetProductDetailsAsync(string partNumber, Dictionary<string, string>? additionalOptions = null)
        {
            throw new NotImplementedException();
        }
    }
}
