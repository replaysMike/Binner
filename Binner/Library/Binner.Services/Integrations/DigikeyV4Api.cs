﻿using Binner.Common;
using Binner.Common.Extensions;
using Binner.Model;
using Binner.Model.Configuration;
using Binner.Model.Configuration.Integrations;
using Binner.Model.Integrations;
using Binner.Model.Integrations.DigiKey;
using Binner.Model.Integrations.DigiKey.V4;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Text;
using System.Web;

namespace Binner.Services.Integrations
{
    public sealed class DigikeyV4Api : BaseDigikeyApi, IDigikeyApi
    {
        private const DigiKeyApiVersion ApiVersion = DigiKeyApiVersion.V4;
        private readonly ILogger<DigikeyApi> _logger;
        private readonly DigikeyConfiguration _configuration;
        private readonly JsonSerializerSettings _serializerSettings;

        #region Include Fields
        private static readonly List<string> V4IncludeFieldNames = new List<string> {
            "Description",
            "Manufacturer",
            "ManufacturerProductNumber",
            "UnitPrice",
            "ProductUrl",
            "DatasheetUrl",
            "PhotoUrl",
            "ProductVariations",
            "QuantityAvailable",
            "ProductStatus",
            "BackOrderNotAllowed",
            "NormallyStocking",
            "Discontinued",
            "EndOfLife",
            "PrimaryVideoUrl",
            "Parameters",
            "BaseProductNumber",
            "Category",
            "ManufacturerLeadWeeks",
            "ManufacturerPublicQuantity",
            "Series",
            "Classifications",
            "OtherNames"
        };
        #endregion

        public DigikeyV4Api(ILogger<DigikeyApi> logger, DigikeyConfiguration configuration, UserConfiguration userConfiguration, JsonSerializerSettings serializerSettings, IApiHttpClientFactory httpClientFactory)
            : base(logger, configuration, userConfiguration, serializerSettings, httpClientFactory)
        {
            _logger = logger;
            _configuration = configuration;
            _serializerSettings = serializerSettings;
        }

        public async Task<IApiResponse> GetOrderAsync(OAuthAuthorization authenticationResponse, string orderId)
        {
            //var fakeOrderResponse = @"{""CustomerId"":1234567,""Contact"":{""FirstName"":null,""LastName"":null,""Email"":""USER@EXAMPLE.COM""},""SalesOrderId"":12345678,""Status"":{""SalesOrderStatus"":""Shipped"",""ShortDescription"":""Shipped"",""LongDescription"":""All products in this order have been shipped. Invoice and tracking information is available.""},""PurchaseOrder"":"""",""TotalPrice"":758.56,""DateEntered"":""2025-02-02T03:41:48.109-06:00"",""OrderNumber"":9910000353889302,""ShipMethod"":""FedEx International Priority"",""Currency"":""USD"",""ShippingAddress"":{""FirstName"":""TEST"",""LastName"":""USER"",""CompanyName"":"""",""AddressLine1"":"""",""AddressLine2"":""123 EXAMPLE RD"",""AddressLine3"":"""",""City"":""VANCOUVER"",""State"":""BC"",""County"":"""",""ZipCode"":""V0N2E0"",""IsoCode"":""CA"",""Phone"":""5555551212"",""InvoiceId"":109914231},""LineItems"":[{""SalesOrderId"":12345678,""DetailId"":1,""TotalPrice"":13.2,""PurchaseOrder"":"""",""CustomerReference"":""BINNER 24V CAB C10"",""CountryOfOrigin"":""PH"",""DigiKeyProductNumber"":""1276-6455-1-ND"",""ManufacturerProductNumber"":""CL21A106KOQNNNG"",""Description"":""CAP CER 10UF 16V X5R 0805"",""PackType"":""CutTape"",""QuantityInitialRequested"":500,""QuantityOrdered"":500,""QuantityShipped"":500,""QuantityReserved"":0,""QuantityBackOrder"":0,""UnitPrice"":0.0264,""PoLineItemNumber"":null,""ItemShipments"":[{""QuantityShipped"":500,""InvoiceId"":109914231,""ShippedDate"":""2025-02-03T13:21:41Z"",""TrackingNumber"":""731620024942"",""ExpectedDeliveryDate"":null}],""Schedules"":[]},{""SalesOrderId"":12345678,""DetailId"":2,""TotalPrice"":2.89,""PurchaseOrder"":"""",""CustomerReference"":""BINNER 24V CAB C11"",""CountryOfOrigin"":""KR"",""DigiKeyProductNumber"":""4713-GMC21X7R103J50NTCT-ND"",""ManufacturerProductNumber"":""GMC21X7R103J50NT"",""Description"":""CAP0805 X7R .01UF 5% 50V"",""PackType"":""CutTape"",""QuantityInitialRequested"":100,""QuantityOrdered"":100,""QuantityShipped"":100,""QuantityReserved"":0,""QuantityBackOrder"":0,""UnitPrice"":0.0289,""PoLineItemNumber"":null,""ItemShipments"":[{""QuantityShipped"":100,""InvoiceId"":109914231,""ShippedDate"":""2025-02-03T13:21:41Z"",""TrackingNumber"":""731620024942"",""ExpectedDeliveryDate"":null}],""Schedules"":[]},{""SalesOrderId"":12345678,""DetailId"":3,""TotalPrice"":42.79,""PurchaseOrder"":"""",""CustomerReference"":""BINNER 24V CAB C14-C17"",""CountryOfOrigin"":""CN"",""DigiKeyProductNumber"":""1276-6780-1-ND"",""ManufacturerProductNumber"":""CL21A226MOCLRNC"",""Description"":""CAP CER 22UF 16V X5R 0805"",""PackType"":""CutTape"",""QuantityInitialRequested"":500,""QuantityOrdered"":500,""QuantityShipped"":500,""QuantityReserved"":0,""QuantityBackOrder"":0,""UnitPrice"":0.08558,""PoLineItemNumber"":null,""ItemShipments"":[{""QuantityShipped"":500,""InvoiceId"":109914231,""ShippedDate"":""2025-02-03T13:21:41Z"",""TrackingNumber"":""731620024942"",""ExpectedDeliveryDate"":null}],""Schedules"":[]},{""SalesOrderId"":12345678,""DetailId"":4,""TotalPrice"":13.6,""PurchaseOrder"":"""",""CustomerReference"":""BINNER 24V CAB D1 - TSP10H45 ALT"",""CountryOfOrigin"":""TW"",""DigiKeyProductNumber"":""1801-TSUP10M45SHCT-ND"",""ManufacturerProductNumber"":""TSUP10M45SH"",""Description"":""DIODE SCHOTTKY 45V 10A SMPC4.6U"",""PackType"":""CutTape"",""QuantityInitialRequested"":20,""QuantityOrdered"":20,""QuantityShipped"":20,""QuantityReserved"":0,""QuantityBackOrder"":0,""UnitPrice"":0.68,""PoLineItemNumber"":null,""ItemShipments"":[{""QuantityShipped"":20,""InvoiceId"":109914231,""ShippedDate"":""2025-02-03T13:21:41Z"",""TrackingNumber"":""731620024942"",""ExpectedDeliveryDate"":null}],""Schedules"":[]},{""SalesOrderId"":12345678,""DetailId"":5,""TotalPrice"":15.8,""PurchaseOrder"":"""",""CustomerReference"":""BINNER 24V CAB D2"",""CountryOfOrigin"":""CN"",""DigiKeyProductNumber"":""18-SMF4L24CACT-ND"",""ManufacturerProductNumber"":""SMF4L24CA"",""Description"":""TVS DIODE 24VWM 38.9VC SOD123F"",""PackType"":""CutTape"",""QuantityInitialRequested"":100,""QuantityOrdered"":100,""QuantityShipped"":100,""QuantityReserved"":0,""QuantityBackOrder"":0,""UnitPrice"":0.158,""PoLineItemNumber"":null,""ItemShipments"":[{""QuantityShipped"":100,""InvoiceId"":109914231,""ShippedDate"":""2025-02-03T13:21:41Z"",""TrackingNumber"":""731620024942"",""ExpectedDeliveryDate"":null}],""Schedules"":[]},{""SalesOrderId"":12345678,""DetailId"":6,""TotalPrice"":23.15,""PurchaseOrder"":"""",""CustomerReference"":""BINNER 24V CAB L1"",""CountryOfOrigin"":""TW"",""DigiKeyProductNumber"":""SRP6540-1R0MCT-ND"",""ManufacturerProductNumber"":""SRP6540-1R0M"",""Description"":""FIXED IND 1UH 12A 7.2 MOHM SMD"",""PackType"":""CutTape"",""QuantityInitialRequested"":25,""QuantityOrdered"":25,""QuantityShipped"":25,""QuantityReserved"":0,""QuantityBackOrder"":0,""UnitPrice"":0.926,""PoLineItemNumber"":null,""ItemShipments"":[{""QuantityShipped"":25,""InvoiceId"":109914231,""ShippedDate"":""2025-02-03T13:21:41Z"",""TrackingNumber"":""731620024942"",""ExpectedDeliveryDate"":null}],""Schedules"":[]},{""SalesOrderId"":12345678,""DetailId"":7,""TotalPrice"":8.25,""PurchaseOrder"":"""",""CustomerReference"":""BINNER 24V CAB U2"",""CountryOfOrigin"":""CN"",""DigiKeyProductNumber"":""31-AP7375-50W5-7CT-ND"",""ManufacturerProductNumber"":""AP7375-50W5-7"",""Description"":""IC REG LIN 5V 300MA SOT25"",""PackType"":""CutTape"",""QuantityInitialRequested"":25,""QuantityOrdered"":25,""QuantityShipped"":25,""QuantityReserved"":0,""QuantityBackOrder"":0,""UnitPrice"":0.33,""PoLineItemNumber"":null,""ItemShipments"":[{""QuantityShipped"":25,""InvoiceId"":109914231,""ShippedDate"":""2025-02-03T13:21:41Z"",""TrackingNumber"":""731620024942"",""ExpectedDeliveryDate"":null}],""Schedules"":[]},{""SalesOrderId"":12345678,""DetailId"":8,""TotalPrice"":58.09,""PurchaseOrder"":"""",""CustomerReference"":""BINNER 24V CAB U7"",""CountryOfOrigin"":""CN"",""DigiKeyProductNumber"":""785-AOZ2261BQI-28CT-ND"",""ManufacturerProductNumber"":""AOZ2261BQI-28"",""Description"":""IC REG BUCK ADJ 8A 23QFNB"",""PackType"":""CutTape"",""QuantityInitialRequested"":100,""QuantityOrdered"":100,""QuantityShipped"":100,""QuantityReserved"":0,""QuantityBackOrder"":0,""UnitPrice"":0.5809,""PoLineItemNumber"":null,""ItemShipments"":[{""QuantityShipped"":100,""InvoiceId"":109914231,""ShippedDate"":""2025-02-03T13:21:41Z"",""TrackingNumber"":""731620024942"",""ExpectedDeliveryDate"":null}],""Schedules"":[]},{""SalesOrderId"":12345678,""DetailId"":9,""TotalPrice"":28.45,""PurchaseOrder"":"""",""CustomerReference"":""BINNER CABINET U1"",""CountryOfOrigin"":""CN"",""DigiKeyProductNumber"":""AP2120N-3.3TRG1DICT-ND"",""ManufacturerProductNumber"":""AP2120N-3.3TRG1"",""Description"":""IC REG LINEAR 3.3V 150MA SOT23"",""PackType"":""CutTape"",""QuantityInitialRequested"":250,""QuantityOrdered"":250,""QuantityShipped"":250,""QuantityReserved"":0,""QuantityBackOrder"":0,""UnitPrice"":0.1138,""PoLineItemNumber"":null,""ItemShipments"":[{""QuantityShipped"":250,""InvoiceId"":109914231,""ShippedDate"":""2025-02-03T13:21:41Z"",""TrackingNumber"":""731620024942"",""ExpectedDeliveryDate"":null}],""Schedules"":[]},{""SalesOrderId"":12345678,""DetailId"":10,""TotalPrice"":30.31,""PurchaseOrder"":"""",""CustomerReference"":""BINNER 24V CAB U7"",""CountryOfOrigin"":""TH"",""DigiKeyProductNumber"":""296-21931-1-ND"",""ManufacturerProductNumber"":""TXS0102DCUR"",""Description"":""IC TRANSLTR BIDIRECTIONAL 8VSSOP"",""PackType"":""CutTape"",""QuantityInitialRequested"":100,""QuantityOrdered"":100,""QuantityShipped"":100,""QuantityReserved"":0,""QuantityBackOrder"":0,""UnitPrice"":0.3031,""PoLineItemNumber"":null,""ItemShipments"":[{""QuantityShipped"":100,""InvoiceId"":109914231,""ShippedDate"":""2025-02-03T13:21:41Z"",""TrackingNumber"":""731620024942"",""ExpectedDeliveryDate"":null}],""Schedules"":[]},{""SalesOrderId"":12345678,""DetailId"":11,""TotalPrice"":43,""PurchaseOrder"":"""",""CustomerReference"":""BINNER STOCK"",""CountryOfOrigin"":""TH"",""DigiKeyProductNumber"":""ATTINY402-SSNRCT-ND"",""ManufacturerProductNumber"":""ATTINY402-SSNR"",""Description"":""IC MCU 8BIT 4KB FLASH 8SOIC"",""PackType"":""CutTape"",""QuantityInitialRequested"":100,""QuantityOrdered"":100,""QuantityShipped"":100,""QuantityReserved"":0,""QuantityBackOrder"":0,""UnitPrice"":0.43,""PoLineItemNumber"":null,""ItemShipments"":[{""QuantityShipped"":100,""InvoiceId"":109914231,""ShippedDate"":""2025-02-03T13:21:41Z"",""TrackingNumber"":""731620024942"",""ExpectedDeliveryDate"":null}],""Schedules"":[]},{""SalesOrderId"":12345678,""DetailId"":12,""TotalPrice"":3.56,""PurchaseOrder"":"""",""CustomerReference"":""BINNER STOCK"",""CountryOfOrigin"":""TW"",""DigiKeyProductNumber"":""732-5316-ND"",""ManufacturerProductNumber"":""61300311121"",""Description"":""CONN HEADER VERT 3POS 2.54MM"",""PackType"":""Bulk"",""QuantityInitialRequested"":50,""QuantityOrdered"":50,""QuantityShipped"":50,""QuantityReserved"":0,""QuantityBackOrder"":0,""UnitPrice"":0.0712,""PoLineItemNumber"":null,""ItemShipments"":[{""QuantityShipped"":50,""InvoiceId"":109914231,""ShippedDate"":""2025-02-03T13:21:41Z"",""TrackingNumber"":""731620024942"",""ExpectedDeliveryDate"":null}],""Schedules"":[]},{""SalesOrderId"":12345678,""DetailId"":13,""TotalPrice"":5.04,""PurchaseOrder"":"""",""CustomerReference"":""BINNER STOCK"",""CountryOfOrigin"":""JP"",""DigiKeyProductNumber"":""490-14467-1-ND"",""ManufacturerProductNumber"":""GRM21BR71E225KE11L"",""Description"":""CAP CER 2.2UF 25V X7R 0805"",""PackType"":""CutTape"",""QuantityInitialRequested"":100,""QuantityOrdered"":100,""QuantityShipped"":100,""QuantityReserved"":0,""QuantityBackOrder"":0,""UnitPrice"":0.0504,""PoLineItemNumber"":null,""ItemShipments"":[{""QuantityShipped"":100,""InvoiceId"":109914231,""ShippedDate"":""2025-02-03T13:21:41Z"",""TrackingNumber"":""731620024942"",""ExpectedDeliveryDate"":null}],""Schedules"":[]},{""SalesOrderId"":12345678,""DetailId"":14,""TotalPrice"":1.07,""PurchaseOrder"":"""",""CustomerReference"":""BINNER STOCK"",""CountryOfOrigin"":""TW"",""DigiKeyProductNumber"":""311-2.20KCRCT-ND"",""ManufacturerProductNumber"":""RC0805FR-072K2L"",""Description"":""RES 2.2K OHM 1% 1/8W 0805"",""PackType"":""CutTape"",""QuantityInitialRequested"":100,""QuantityOrdered"":100,""QuantityShipped"":100,""QuantityReserved"":0,""QuantityBackOrder"":0,""UnitPrice"":0.0107,""PoLineItemNumber"":null,""ItemShipments"":[{""QuantityShipped"":100,""InvoiceId"":109914231,""ShippedDate"":""2025-02-03T13:21:41Z"",""TrackingNumber"":""731620024942"",""ExpectedDeliveryDate"":null}],""Schedules"":[]},{""SalesOrderId"":12345678,""DetailId"":15,""TotalPrice"":2.8,""PurchaseOrder"":"""",""CustomerReference"":""BINNER STOCK"",""CountryOfOrigin"":""TW"",""DigiKeyProductNumber"":""RMCF0805JT2K20CT-ND"",""ManufacturerProductNumber"":""RMCF0805JT2K20"",""Description"":""RES 2.2K OHM 5% 1/8W 0805"",""PackType"":""CutTape"",""QuantityInitialRequested"":500,""QuantityOrdered"":500,""QuantityShipped"":500,""QuantityReserved"":0,""QuantityBackOrder"":0,""UnitPrice"":0.0056,""PoLineItemNumber"":null,""ItemShipments"":[{""QuantityShipped"":500,""InvoiceId"":109914231,""ShippedDate"":""2025-02-03T13:21:41Z"",""TrackingNumber"":""731620024942"",""ExpectedDeliveryDate"":null}],""Schedules"":[]},{""SalesOrderId"":12345678,""DetailId"":16,""TotalPrice"":2.62,""PurchaseOrder"":"""",""CustomerReference"":""BINNER STOCK"",""CountryOfOrigin"":""TH"",""DigiKeyProductNumber"":""568-1835-1-ND"",""ManufacturerProductNumber"":""PCA9536D,118"",""Description"":""IC XPNDR 400KHZ I2C SMBUS 8SO"",""PackType"":""CutTape"",""QuantityInitialRequested"":2,""QuantityOrdered"":2,""QuantityShipped"":2,""QuantityReserved"":0,""QuantityBackOrder"":0,""UnitPrice"":1.31,""PoLineItemNumber"":null,""ItemShipments"":[{""QuantityShipped"":2,""InvoiceId"":109914231,""ShippedDate"":""2025-02-03T13:21:41Z"",""TrackingNumber"":""731620024942"",""ExpectedDeliveryDate"":null}],""Schedules"":[]},{""SalesOrderId"":12345678,""DetailId"":17,""TotalPrice"":2.34,""PurchaseOrder"":"""",""CustomerReference"":""BINNER STOCK"",""CountryOfOrigin"":""TW"",""DigiKeyProductNumber"":""296-13105-1-ND"",""ManufacturerProductNumber"":""PCF8574ADWR"",""Description"":""IC XPNDR 100KHZ I2C 16SOIC"",""PackType"":""CutTape"",""QuantityInitialRequested"":2,""QuantityOrdered"":2,""QuantityShipped"":2,""QuantityReserved"":0,""QuantityBackOrder"":0,""UnitPrice"":1.17,""PoLineItemNumber"":null,""ItemShipments"":[{""QuantityShipped"":2,""InvoiceId"":109914231,""ShippedDate"":""2025-02-03T13:21:41Z"",""TrackingNumber"":""731620024942"",""ExpectedDeliveryDate"":null}],""Schedules"":[]},{""SalesOrderId"":12345678,""DetailId"":18,""TotalPrice"":3.38,""PurchaseOrder"":"""",""CustomerReference"":""BINNER STOCK"",""CountryOfOrigin"":""TH"",""DigiKeyProductNumber"":""MCP23017-E/SS-ND"",""ManufacturerProductNumber"":""MCP23017-E/SS"",""Description"":""IC XPNDR 1.7MHZ I2C 28SSOP"",""PackType"":""Tube"",""QuantityInitialRequested"":2,""QuantityOrdered"":2,""QuantityShipped"":2,""QuantityReserved"":0,""QuantityBackOrder"":0,""UnitPrice"":1.69,""PoLineItemNumber"":null,""ItemShipments"":[{""QuantityShipped"":2,""InvoiceId"":109914231,""ShippedDate"":""2025-02-03T13:21:41Z"",""TrackingNumber"":""731620024942"",""ExpectedDeliveryDate"":null}],""Schedules"":[]},{""SalesOrderId"":12345678,""DetailId"":19,""TotalPrice"":3.21,""PurchaseOrder"":"""",""CustomerReference"":""BINNER 24V CAB C4,C8"",""CountryOfOrigin"":""PH"",""DigiKeyProductNumber"":""1276-1029-1-ND"",""ManufacturerProductNumber"":""CL21B105KBFNNNE"",""Description"":""CAP CER 1UF 50V X7R 0805"",""PackType"":""CutTape"",""QuantityInitialRequested"":100,""QuantityOrdered"":100,""QuantityShipped"":100,""QuantityReserved"":0,""QuantityBackOrder"":0,""UnitPrice"":0.0321,""PoLineItemNumber"":null,""ItemShipments"":[{""QuantityShipped"":100,""InvoiceId"":109914231,""ShippedDate"":""2025-02-03T13:21:41Z"",""TrackingNumber"":""731620024942"",""ExpectedDeliveryDate"":null}],""Schedules"":[]},{""SalesOrderId"":12345678,""DetailId"":20,""TotalPrice"":4.73,""PurchaseOrder"":"""",""CustomerReference"":""BINNER STOCK"",""CountryOfOrigin"":""TW"",""DigiKeyProductNumber"":""RMCF0805ZT0R00CT-ND"",""ManufacturerProductNumber"":""RMCF0805ZT0R00"",""Description"":""RES 0 OHM JUMPER 1/8W 0805"",""PackType"":""CutTape"",""QuantityInitialRequested"":1000,""QuantityOrdered"":1000,""QuantityShipped"":1000,""QuantityReserved"":0,""QuantityBackOrder"":0,""UnitPrice"":0.00473,""PoLineItemNumber"":null,""ItemShipments"":[{""QuantityShipped"":1000,""InvoiceId"":109914231,""ShippedDate"":""2025-02-03T13:21:41Z"",""TrackingNumber"":""731620024942"",""ExpectedDeliveryDate"":null}],""Schedules"":[]},{""SalesOrderId"":12345678,""DetailId"":21,""TotalPrice"":42.39,""PurchaseOrder"":"""",""CustomerReference"":""BINNER 24V CAB DISPLAY J8"",""CountryOfOrigin"":""CN"",""DigiKeyProductNumber"":""455-SM05B-SRSS-TBCT-ND"",""ManufacturerProductNumber"":""SM05B-SRSS-TB"",""Description"":""CONN HEADER SMD R/A 5POS 1MM"",""PackType"":""CutTape"",""QuantityInitialRequested"":100,""QuantityOrdered"":100,""QuantityShipped"":100,""QuantityReserved"":0,""QuantityBackOrder"":0,""UnitPrice"":0.4239,""PoLineItemNumber"":null,""ItemShipments"":[{""QuantityShipped"":100,""InvoiceId"":109914231,""ShippedDate"":""2025-02-03T13:21:41Z"",""TrackingNumber"":""731620024942"",""ExpectedDeliveryDate"":null}],""Schedules"":[]},{""SalesOrderId"":12345678,""DetailId"":22,""TotalPrice"":41.18,""PurchaseOrder"":"""",""CustomerReference"":""BINNER 24V CAB DISPLAY J1"",""CountryOfOrigin"":""CN"",""DigiKeyProductNumber"":""455-BM05B-SRSS-TBCT-ND"",""ManufacturerProductNumber"":""BM05B-SRSS-TB"",""Description"":""CONN HEADER SMD 5POS 1MM"",""PackType"":""CutTape"",""QuantityInitialRequested"":100,""QuantityOrdered"":100,""QuantityShipped"":100,""QuantityReserved"":0,""QuantityBackOrder"":0,""UnitPrice"":0.4118,""PoLineItemNumber"":null,""ItemShipments"":[{""QuantityShipped"":100,""InvoiceId"":109914231,""ShippedDate"":""2025-02-03T13:21:41Z"",""TrackingNumber"":""731620024942"",""ExpectedDeliveryDate"":null}],""Schedules"":[]},{""SalesOrderId"":12345678,""DetailId"":23,""TotalPrice"":18.48,""PurchaseOrder"":"""",""CustomerReference"":""BINNER 24V CAB J6,J7"",""CountryOfOrigin"":""CN"",""DigiKeyProductNumber"":""455-S3B-XH-A-ND"",""ManufacturerProductNumber"":""S3B-XH-A"",""Description"":""CONN HEADER R/A 3POS 2.5MM"",""PackType"":""Bulk"",""QuantityInitialRequested"":200,""QuantityOrdered"":200,""QuantityShipped"":200,""QuantityReserved"":0,""QuantityBackOrder"":0,""UnitPrice"":0.0924,""PoLineItemNumber"":null,""ItemShipments"":[{""QuantityShipped"":200,""InvoiceId"":109914231,""ShippedDate"":""2025-02-03T13:21:41Z"",""TrackingNumber"":""731620024942"",""ExpectedDeliveryDate"":null}],""Schedules"":[]},{""SalesOrderId"":12345678,""DetailId"":24,""TotalPrice"":6.02,""PurchaseOrder"":"""",""CustomerReference"":""BINNER CONTROLLER"",""CountryOfOrigin"":""TW"",""DigiKeyProductNumber"":""RMCF0805FT10R0CT-ND"",""ManufacturerProductNumber"":""RMCF0805FT10R0"",""Description"":""RES 10 OHM 1% 1/8W 0805"",""PackType"":""CutTape"",""QuantityInitialRequested"":1000,""QuantityOrdered"":1000,""QuantityShipped"":1000,""QuantityReserved"":0,""QuantityBackOrder"":0,""UnitPrice"":0.00602,""PoLineItemNumber"":null,""ItemShipments"":[{""QuantityShipped"":1000,""InvoiceId"":109914231,""ShippedDate"":""2025-02-03T13:21:41Z"",""TrackingNumber"":""731620024942"",""ExpectedDeliveryDate"":null}],""Schedules"":[]},{""SalesOrderId"":12345678,""DetailId"":25,""TotalPrice"":3.48,""PurchaseOrder"":"""",""CustomerReference"":"""",""CountryOfOrigin"":""CN"",""DigiKeyProductNumber"":""RMCF0805FT51K0CT-ND"",""ManufacturerProductNumber"":""RMCF0805FT51K0"",""Description"":""RES 51K OHM 1% 1/8W 0805"",""PackType"":""CutTape"",""QuantityInitialRequested"":500,""QuantityOrdered"":500,""QuantityShipped"":500,""QuantityReserved"":0,""QuantityBackOrder"":0,""UnitPrice"":0.00696,""PoLineItemNumber"":null,""ItemShipments"":[{""QuantityShipped"":500,""InvoiceId"":109914231,""ShippedDate"":""2025-02-03T13:21:41Z"",""TrackingNumber"":""731620024942"",""ExpectedDeliveryDate"":null}],""Schedules"":[]},{""SalesOrderId"":12345678,""DetailId"":26,""TotalPrice"":18.46,""PurchaseOrder"":"""",""CustomerReference"":""BINNER 24V CONTROLLER C3"",""CountryOfOrigin"":""CN"",""DigiKeyProductNumber"":""399-13672-1-ND"",""ManufacturerProductNumber"":""A765EB107M1CLAE025"",""Description"":""CAP ALUM POLY 100UF 20% 16V SMD"",""PackType"":""CutTape"",""QuantityInitialRequested"":50,""QuantityOrdered"":50,""QuantityShipped"":50,""QuantityReserved"":0,""QuantityBackOrder"":0,""UnitPrice"":0.3692,""PoLineItemNumber"":null,""ItemShipments"":[{""QuantityShipped"":50,""InvoiceId"":109914231,""ShippedDate"":""2025-02-03T13:21:41Z"",""TrackingNumber"":""731620024942"",""ExpectedDeliveryDate"":null}],""Schedules"":[]},{""SalesOrderId"":12345678,""DetailId"":27,""TotalPrice"":3.64,""PurchaseOrder"":"""",""CustomerReference"":""BINNER CAB R21"",""CountryOfOrigin"":""TW"",""DigiKeyProductNumber"":""118-CR0805-FX-7502ELFCT-ND"",""ManufacturerProductNumber"":""CR0805-FX-7502ELF"",""Description"":""RES SMD 75K OHM 1% 1/8W 0805"",""PackType"":""CutTape"",""QuantityInitialRequested"":500,""QuantityOrdered"":500,""QuantityShipped"":500,""QuantityReserved"":0,""QuantityBackOrder"":0,""UnitPrice"":0.00728,""PoLineItemNumber"":null,""ItemShipments"":[{""QuantityShipped"":500,""InvoiceId"":109914231,""ShippedDate"":""2025-02-03T13:21:41Z"",""TrackingNumber"":""731620024942"",""ExpectedDeliveryDate"":null}],""Schedules"":[]},{""SalesOrderId"":12345678,""DetailId"":28,""TotalPrice"":3.48,""PurchaseOrder"":"""",""CustomerReference"":""BINNER CAB R20"",""CountryOfOrigin"":""CN"",""DigiKeyProductNumber"":""RMCF0805FT390KCT-ND"",""ManufacturerProductNumber"":""RMCF0805FT390K"",""Description"":""RES 390K OHM 1% 1/8W 0805"",""PackType"":""CutTape"",""QuantityInitialRequested"":500,""QuantityOrdered"":500,""QuantityShipped"":500,""QuantityReserved"":0,""QuantityBackOrder"":0,""UnitPrice"":0.00696,""PoLineItemNumber"":null,""ItemShipments"":[{""QuantityShipped"":500,""InvoiceId"":109914231,""ShippedDate"":""2025-02-03T13:21:41Z"",""TrackingNumber"":""731620024942"",""ExpectedDeliveryDate"":null}],""Schedules"":[]},{""SalesOrderId"":12345678,""DetailId"":29,""TotalPrice"":7.9,""PurchaseOrder"":"""",""CustomerReference"":""BINNER D7"",""CountryOfOrigin"":""CN"",""DigiKeyProductNumber"":""1N4148W-FDICT-ND"",""ManufacturerProductNumber"":""1N4148W-7-F"",""Description"":""DIODE STANDARD 100V 300MA SOD123"",""PackType"":""CutTape"",""QuantityInitialRequested"":200,""QuantityOrdered"":200,""QuantityShipped"":200,""QuantityReserved"":0,""QuantityBackOrder"":0,""UnitPrice"":0.0395,""PoLineItemNumber"":null,""ItemShipments"":[{""QuantityShipped"":200,""InvoiceId"":109914231,""ShippedDate"":""2025-02-03T13:21:41Z"",""TrackingNumber"":""731620024942"",""ExpectedDeliveryDate"":null}],""Schedules"":[]},{""SalesOrderId"":12345678,""DetailId"":30,""TotalPrice"":16.35,""PurchaseOrder"":"""",""CustomerReference"":""BINNER DISPLAY"",""CountryOfOrigin"":""CN"",""DigiKeyProductNumber"":""CKN12319-1-ND"",""ManufacturerProductNumber"":""PTS636SM50JSMTR LFS"",""Description"":""SWITCH TACTILE SPST-NO 0.05A 12V"",""PackType"":""CutTape"",""QuantityInitialRequested"":100,""QuantityOrdered"":100,""QuantityShipped"":100,""QuantityReserved"":0,""QuantityBackOrder"":0,""UnitPrice"":0.1635,""PoLineItemNumber"":null,""ItemShipments"":[{""QuantityShipped"":100,""InvoiceId"":109914231,""ShippedDate"":""2025-02-03T13:21:41Z"",""TrackingNumber"":""731620024942"",""ExpectedDeliveryDate"":null}],""Schedules"":[]},{""SalesOrderId"":12345678,""DetailId"":31,""TotalPrice"":2.75,""PurchaseOrder"":"""",""CustomerReference"":""BINNER STOCK"",""CountryOfOrigin"":""CN"",""DigiKeyProductNumber"":""497-1201-1-ND"",""ManufacturerProductNumber"":""L78M05ABDT-TR"",""Description"":""IC REG LINEAR 5V 500MA DPAK"",""PackType"":""CutTape"",""QuantityInitialRequested"":10,""QuantityOrdered"":10,""QuantityShipped"":10,""QuantityReserved"":0,""QuantityBackOrder"":0,""UnitPrice"":0.275,""PoLineItemNumber"":null,""ItemShipments"":[{""QuantityShipped"":10,""InvoiceId"":109914231,""ShippedDate"":""2025-02-03T13:21:41Z"",""TrackingNumber"":""731620024942"",""ExpectedDeliveryDate"":null}],""Schedules"":[]},{""SalesOrderId"":12345678,""DetailId"":32,""TotalPrice"":29.67,""PurchaseOrder"":"""",""CustomerReference"":""BINNER CAB U2 NEW"",""CountryOfOrigin"":""CN"",""DigiKeyProductNumber"":""296-17616-1-ND"",""ManufacturerProductNumber"":""UA78M05IDCYR"",""Description"":""IC REG LINEAR 5V 500MA SOT223-4"",""PackType"":""CutTape"",""QuantityInitialRequested"":100,""QuantityOrdered"":100,""QuantityShipped"":100,""QuantityReserved"":0,""QuantityBackOrder"":0,""UnitPrice"":0.2967,""PoLineItemNumber"":null,""ItemShipments"":[{""QuantityShipped"":100,""InvoiceId"":109914231,""ShippedDate"":""2025-02-03T13:21:41Z"",""TrackingNumber"":""731620024942"",""ExpectedDeliveryDate"":null}],""Schedules"":[]},{""SalesOrderId"":12345678,""DetailId"":33,""TotalPrice"":3.56,""PurchaseOrder"":"""",""CustomerReference"":""FOR UA78M05"",""CountryOfOrigin"":""PH"",""DigiKeyProductNumber"":""1276-2959-1-ND"",""ManufacturerProductNumber"":""CL21B334KBFNFNE"",""Description"":""CAP CER 0.33UF 50V X7R 0805"",""PackType"":""CutTape"",""QuantityInitialRequested"":100,""QuantityOrdered"":100,""QuantityShipped"":100,""QuantityReserved"":0,""QuantityBackOrder"":0,""UnitPrice"":0.0356,""PoLineItemNumber"":null,""ItemShipments"":[{""QuantityShipped"":100,""InvoiceId"":109914231,""ShippedDate"":""2025-02-03T13:21:41Z"",""TrackingNumber"":""731620024942"",""ExpectedDeliveryDate"":null}],""Schedules"":[]},{""SalesOrderId"":12345678,""DetailId"":34,""TotalPrice"":88,""PurchaseOrder"":"""",""CustomerReference"":""BINNER CAB U1"",""CountryOfOrigin"":""TH"",""DigiKeyProductNumber"":""150-ATTINY3226-MURCT-ND"",""ManufacturerProductNumber"":""ATTINY3226-MUR"",""Description"":""IC MCU 8BIT 32KB FLASH 20VQFN"",""PackType"":""CutTape"",""QuantityInitialRequested"":100,""QuantityOrdered"":100,""QuantityShipped"":100,""QuantityReserved"":0,""QuantityBackOrder"":0,""UnitPrice"":0.88,""PoLineItemNumber"":null,""ItemShipments"":[{""QuantityShipped"":100,""InvoiceId"":109914231,""ShippedDate"":""2025-02-03T13:21:41Z"",""TrackingNumber"":""731620024942"",""ExpectedDeliveryDate"":null}],""Schedules"":[]},{""SalesOrderId"":12345678,""DetailId"":35,""TotalPrice"":7.25,""PurchaseOrder"":"""",""CustomerReference"":""BINNER STOCK"",""CountryOfOrigin"":""TW"",""DigiKeyProductNumber"":""150-ATTINY3226-SFRCT-ND"",""ManufacturerProductNumber"":""ATTINY3226-SFR"",""Description"":""IC MCU 8BIT 32KB FLASH 20SOIC"",""PackType"":""CutTape"",""QuantityInitialRequested"":5,""QuantityOrdered"":5,""QuantityShipped"":5,""QuantityReserved"":0,""QuantityBackOrder"":0,""UnitPrice"":1.45,""PoLineItemNumber"":null,""ItemShipments"":[{""QuantityShipped"":5,""InvoiceId"":109914231,""ShippedDate"":""2025-02-03T13:21:41Z"",""TrackingNumber"":""731620024942"",""ExpectedDeliveryDate"":null}],""Schedules"":[]},{""SalesOrderId"":12345678,""DetailId"":36,""TotalPrice"":3.48,""PurchaseOrder"":"""",""CustomerReference"":"""",""CountryOfOrigin"":""CN"",""DigiKeyProductNumber"":""RMCF0805FT1K50CT-ND"",""ManufacturerProductNumber"":""RMCF0805FT1K50"",""Description"":""RES 1.5K OHM 1% 1/8W 0805"",""PackType"":""CutTape"",""QuantityInitialRequested"":500,""QuantityOrdered"":500,""QuantityShipped"":500,""QuantityReserved"":0,""QuantityBackOrder"":0,""UnitPrice"":0.00696,""PoLineItemNumber"":null,""ItemShipments"":[{""QuantityShipped"":500,""InvoiceId"":109914231,""ShippedDate"":""2025-02-03T13:21:41Z"",""TrackingNumber"":""731620024942"",""ExpectedDeliveryDate"":null}],""Schedules"":[]},{""SalesOrderId"":12345678,""DetailId"":37,""TotalPrice"":3.48,""PurchaseOrder"":"""",""CustomerReference"":"""",""CountryOfOrigin"":""TW"",""DigiKeyProductNumber"":""RMCF0805FT1K10CT-ND"",""ManufacturerProductNumber"":""RMCF0805FT1K10"",""Description"":""RES 1.1K OHM 1% 1/8W 0805"",""PackType"":""CutTape"",""QuantityInitialRequested"":500,""QuantityOrdered"":500,""QuantityShipped"":500,""QuantityReserved"":0,""QuantityBackOrder"":0,""UnitPrice"":0.00696,""PoLineItemNumber"":null,""ItemShipments"":[{""QuantityShipped"":500,""InvoiceId"":109914231,""ShippedDate"":""2025-02-03T13:21:41Z"",""TrackingNumber"":""731620024942"",""ExpectedDeliveryDate"":null}],""Schedules"":[]},{""SalesOrderId"":12345678,""DetailId"":38,""TotalPrice"":9,""PurchaseOrder"":"""",""CustomerReference"":""BINNER C10"",""CountryOfOrigin"":""CN"",""DigiKeyProductNumber"":""1276-1244-1-ND"",""ManufacturerProductNumber"":""CL21A475KAQNNNE"",""Description"":""CAP CER 4.7UF 25V X5R 0805"",""PackType"":""CutTape"",""QuantityInitialRequested"":300,""QuantityOrdered"":300,""QuantityShipped"":300,""QuantityReserved"":0,""QuantityBackOrder"":0,""UnitPrice"":0.03,""PoLineItemNumber"":null,""ItemShipments"":[{""QuantityShipped"":300,""InvoiceId"":109914231,""ShippedDate"":""2025-02-03T13:21:41Z"",""TrackingNumber"":""731620024942"",""ExpectedDeliveryDate"":null}],""Schedules"":[]},{""SalesOrderId"":12345678,""DetailId"":39,""TotalPrice"":32.53,""PurchaseOrder"":"""",""CustomerReference"":"""",""CountryOfOrigin"":""CN"",""DigiKeyProductNumber"":""AZ1117CH-3.3TRG1DICT-ND"",""ManufacturerProductNumber"":""AZ1117CH-3.3TRG1"",""Description"":""IC REG LINEAR 3.3V 1A SOT223"",""PackType"":""CutTape"",""QuantityInitialRequested"":250,""QuantityOrdered"":250,""QuantityShipped"":250,""QuantityReserved"":0,""QuantityBackOrder"":0,""UnitPrice"":0.13012,""PoLineItemNumber"":null,""ItemShipments"":[{""QuantityShipped"":250,""InvoiceId"":109914231,""ShippedDate"":""2025-02-03T13:21:41Z"",""TrackingNumber"":""731620024942"",""ExpectedDeliveryDate"":null}],""Schedules"":[]},{""SalesOrderId"":12345678,""DetailId"":40,""TotalPrice"":14.66,""PurchaseOrder"":"""",""CustomerReference"":""BINNER 24V CONTROLLER U1"",""CountryOfOrigin"":""MY"",""DigiKeyProductNumber"":""NCV7805BDTRKGOSCT-ND"",""ManufacturerProductNumber"":""NCV7805BDTRKG"",""Description"":""IC REG LINEAR 5V 1A DPAK"",""PackType"":""CutTape"",""QuantityInitialRequested"":25,""QuantityOrdered"":25,""QuantityShipped"":25,""QuantityReserved"":0,""QuantityBackOrder"":0,""UnitPrice"":0.5864,""PoLineItemNumber"":null,""ItemShipments"":[{""QuantityShipped"":25,""InvoiceId"":109914231,""ShippedDate"":""2025-02-03T13:21:41Z"",""TrackingNumber"":""731620024942"",""ExpectedDeliveryDate"":null}],""Schedules"":[]},{""SalesOrderId"":12345678,""DetailId"":41,""TotalPrice"":24.95,""PurchaseOrder"":"""",""CustomerReference"":"""",""CountryOfOrigin"":""TH"",""DigiKeyProductNumber"":""296-22863-1-ND"",""ManufacturerProductNumber"":""TXS0101DBVR"",""Description"":""IC TRANSLATOR BIDIR SOT23-6"",""PackType"":""CutTape"",""QuantityInitialRequested"":100,""QuantityOrdered"":100,""QuantityShipped"":100,""QuantityReserved"":0,""QuantityBackOrder"":0,""UnitPrice"":0.2495,""PoLineItemNumber"":null,""ItemShipments"":[{""QuantityShipped"":100,""InvoiceId"":109914231,""ShippedDate"":""2025-02-03T13:21:41Z"",""TrackingNumber"":""731620024942"",""ExpectedDeliveryDate"":null}],""Schedules"":[]},{""SalesOrderId"":12345678,""DetailId"":42,""TotalPrice"":22.27,""PurchaseOrder"":"""",""CustomerReference"":""BINNER U6"",""CountryOfOrigin"":""TH"",""DigiKeyProductNumber"":""DS2484R+TCT-ND"",""ManufacturerProductNumber"":""DS2484R+T"",""Description"":""IC MASTER I2C-1WIRE 1CH SOT-6"",""PackType"":""CutTape"",""QuantityInitialRequested"":10,""QuantityOrdered"":10,""QuantityShipped"":10,""QuantityReserved"":0,""QuantityBackOrder"":0,""UnitPrice"":2.227,""PoLineItemNumber"":null,""ItemShipments"":[{""QuantityShipped"":10,""InvoiceId"":109914231,""ShippedDate"":""2025-02-03T13:21:41Z"",""TrackingNumber"":""731620024942"",""ExpectedDeliveryDate"":null}],""Schedules"":[]},{""SalesOrderId"":12345678,""DetailId"":43,""TotalPrice"":16.69,""PurchaseOrder"":"""",""CustomerReference"":""BINNER 24V CONTROLLER"",""CountryOfOrigin"":""JP"",""DigiKeyProductNumber"":""296-23770-1-ND"",""ManufacturerProductNumber"":""INA219AIDCNR"",""Description"":""IC CURRENT MONITOR 1% SOT23-8"",""PackType"":""CutTape"",""QuantityInitialRequested"":10,""QuantityOrdered"":10,""QuantityShipped"":10,""QuantityReserved"":0,""QuantityBackOrder"":0,""UnitPrice"":1.669,""PoLineItemNumber"":null,""ItemShipments"":[{""QuantityShipped"":10,""InvoiceId"":109914231,""ShippedDate"":""2025-02-03T13:21:41Z"",""TrackingNumber"":""731620024942"",""ExpectedDeliveryDate"":null}],""Schedules"":[]},{""SalesOrderId"":12345678,""DetailId"":44,""TotalPrice"":30.61,""PurchaseOrder"":"""",""CustomerReference"":""BINNER CABINET ESD J10-J13"",""CountryOfOrigin"":""CN"",""DigiKeyProductNumber"":""455-SM03B-SRSS-TBCT-ND"",""ManufacturerProductNumber"":""SM03B-SRSS-TB"",""Description"":""CONN HEADER SMD R/A 3POS 1MM"",""PackType"":""CutTape"",""QuantityInitialRequested"":100,""QuantityOrdered"":100,""QuantityShipped"":100,""QuantityReserved"":0,""QuantityBackOrder"":0,""UnitPrice"":0.3061,""PoLineItemNumber"":null,""ItemShipments"":[{""QuantityShipped"":100,""InvoiceId"":109914231,""ShippedDate"":""2025-02-03T13:21:41Z"",""TrackingNumber"":""731620024942"",""ExpectedDeliveryDate"":null}],""Schedules"":[]}]}";
            //var fakeResults = JsonConvert.DeserializeObject<SalesOrder>(fakeOrderResponse, _serializerSettings) ?? new();
            //return new ApiResponse(fakeResults, nameof(DigikeyApi));

            try
            {
                // set what fields we want from the API (RetrieveSalesOrder)
                var uri = Url.Combine(_configuration.ApiUrl, $"orderstatus/v4/salesorder/{orderId}");
                var requestMessage = CreateRequest(authenticationResponse, HttpMethod.Get, uri);
                // perform a keywords API search
                var response = await _client.SendAsync(requestMessage);
                var result = await TryHandleResponseAsync(response, authenticationResponse, ApiVersion);
                if (!result.IsSuccessful)
                {
                    // return api error
                    return result.Response;
                }

                // 200 OK
                var resultString = await response.Content.ReadAsStringAsync();
                var results = JsonConvert.DeserializeObject<SalesOrder>(resultString, _serializerSettings) ?? new();
                return new ApiResponse(results, nameof(DigikeyApi));
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<IApiResponse> SearchAsync(OAuthAuthorization authenticationResponse, string partNumber, string? partType, string? mountingType, int recordCount = ApiConstants.MaxRecords, Dictionary<string, string>? additionalOptions = null)
        {
            /* important reminder - don't reference authResponse in here! */
            _logger.LogInformation($"[{nameof(SearchAsync)}] Called using accesstoken='{authenticationResponse.AccessToken.Sanitize()}'");

            var keywords = new List<string>();
            if (!string.IsNullOrEmpty(partNumber))
                keywords = partNumber
                    .ToLower()
                    .Split([" "], StringSplitOptions.RemoveEmptyEntries)
                    .ToList();
            var packageTypeEnum = MountingTypes.None;
            if (!string.IsNullOrEmpty(mountingType))
            {
                switch (mountingType.ToLower())
                {
                    case "surface mount":
                    case "surfacemount":
                        packageTypeEnum = MountingTypes.SurfaceMount;
                        break;
                    case "through hole":
                    case "throughhole":
                        packageTypeEnum = MountingTypes.ThroughHole;
                        break;
                }
            }

            try
            {
                // set what fields we want from the API
                var values = new Dictionary<string, string> {
                    { "includes", $"Products({string.Join(",", V4IncludeFieldNames)})" },
                };
                // includes are passed as Products({comma delimited list of field names})
                //var uri = Url.Combine(_configuration.ApiUrl, $"products/v4/search/keyword?" + string.Join("&", values.Select(x => $"{x.Key}={x.Value}")));
                var uri = Url.Combine(_configuration.ApiUrl, $"products/v4/search/keyword"); // fetches all fields

                // map taxonomies (categories) to the part type
                var taxonomies = MapTaxonomies(partType, packageTypeEnum);

                // attempt to detect certain words and apply them as parametric filters
                var (parametricFilters, filteredKeywords) = MapParametricFilters(keywords, packageTypeEnum, taxonomies);

                // todo: we will need a lot more data in order to do filtering pre-emptively.
                // DigiKey v4 requires an initial search to be done, and the ability to filter category & child category at the same time.
                // so, it's very complicated and requires some data mapping.
                /*var parameterFilterRequest = new ParameterFilterRequest();
                parameterFilterRequest.ParameterFilters = new List<ParametricCategory>();
                parameterFilterRequest.CategoryFilter = new FilterId { Id = "797" };
                foreach (var parametricFilter in parametricFilters)
                {
                    var filter = new ParametricCategory
                    {
                        ParameterId = 1742,
                        FilterValues = new List<FilterId> { new FilterId { Id = "351775" } },
                        //ParameterId = parametricFilter.ParameterId,
                        // valueId is resistance
                        //FilterValues = new List<FilterId> { new FilterId { Id = parametricFilter?.ValueId ?? string.Empty } },
                    };
                    parameterFilterRequest.ParameterFilters.Add(filter);
                }*/

                var request = new KeywordSearchRequest
                {
                    //Keywords = string.Join(" ", filteredKeywords),
                    Keywords = string.Join(" ", keywords),
                    Limit = recordCount,
                    Offset = 0,
                    FilterOptionsRequest = new FilterOptionsRequest
                    {
                        // manufacturers must be referenced by numeric manufactuer id as string, not name.
                        //ManufacturerFilter = new List<FilterId>() { new FilterId("1882") },
                        // category's must be referenced by numeric category id as string, not name.
                        //CategoryFilter = taxonomies.Any() 
                        //    ? taxonomies.Select(x => new FilterId() { Id = ((int)x).ToString() }).ToList() 
                        //    : new List<FilterId>(),
                        // v4 parametric filters are much more complicated. They require an initial search first which returns a list of
                        // all the possible parametric filters available - more like how the DigiKey website works.
                        // The Category Id, subcategory Id, and parameter Id are all required to be passed in the request.
                        //ParameterFilterRequest = parameterFilterRequest,
                        /*ParameterFilterRequest = new ParameterFilterRequest
                        {
                            CategoryFilter = new FilterId { Id = "797" },
                            ParameterFilters = new List<ParametricCategory>
                            {
                                new ParametricCategory
                                {
                                    ParameterId = 1742,
                                    FilterValues = new List<FilterId> { new FilterId { Id = "351775" } },
                                }
                            }
                        }*/
                    },
                    SortOptions = new SortOptions
                    {
                        Field = SortFields.QuantityAvailable,
                        SortOrder = SortDirection.Descending
                    },
                };
                var result = await PerformApiSearchQueryAsync(authenticationResponse, uri, request);
                if (result.ErrorResponse != null) return result.ErrorResponse;

                // if no results are returned, perform a secondary search on the original keyword search with no modifications via parametric filtering
                if (!result.SuccessResponse.Products.Any() && filteredKeywords.Count != keywords.Count)
                {
                    request = new KeywordSearchRequest
                    {
                        Keywords = string.Join(" ", keywords),
                        Limit = recordCount,
                        Offset = 0,
                        // no filter options specified
                        FilterOptionsRequest = new FilterOptionsRequest { },
                        SortOptions = new SortOptions
                        {
                            Field = SortFields.QuantityAvailable,
                            SortOrder = SortDirection.Descending
                        },
                    };
                    result = await PerformApiSearchQueryAsync(authenticationResponse, uri, request);
                    if (result.ErrorResponse != null) return result.ErrorResponse;
                }

                return new ApiResponse(result.SuccessResponse, nameof(DigikeyApi));
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<IApiResponse> GetProductDetailsAsync(OAuthAuthorization authenticationResponse, string partNumber)
        {
            /* same as SearchAsync() but passing the api different parameters */
            try
            {
                // set what fields we want from the API
                var uri = Url.Combine(_configuration.ApiUrl, $"products/v4/search/{HttpUtility.UrlEncode(partNumber)}/productdetails");
                _logger.LogInformation($"[{nameof(GetProductDetailsAsync)}] Creating product search request for '{partNumber}'...");
                var requestMessage = CreateRequest(authenticationResponse, HttpMethod.Get, uri);
                // perform a keywords API search
                var response = await _client.SendAsync(requestMessage);
                _logger.LogInformation($"[{nameof(GetProductDetailsAsync)}] Api responded with '{response.StatusCode}'");
                var result = await TryHandleResponseAsync(response, authenticationResponse, ApiVersion);
                if (!result.IsSuccessful)
                {
                    // return api error
                    return result.Response;
                }

                // 200 OK
                var responseJson = await response.Content.ReadAsStringAsync();
                var results = JsonConvert.DeserializeObject<ProductDetails>(responseJson, _serializerSettings) ?? new();
                return new ApiResponse(results, nameof(DigikeyApi));
            }
            catch (Exception)
            {
                throw;
            }
        }

        private async Task<(IApiResponse? ErrorResponse, KeywordSearchResponse SuccessResponse, string JsonRequest, string JsonResponse)> PerformApiSearchQueryAsync(OAuthAuthorization authenticationResponse, Uri uri, KeywordSearchRequest request)
        {
            _logger.LogInformation($"[{nameof(PerformApiSearchQueryAsync)}] Creating search request for '{request.Keywords}' using accesstoken='{authenticationResponse.AccessToken.Sanitize()}'...");
            using var requestMessage = CreateRequest(authenticationResponse, HttpMethod.Post, uri);
            var requestJson = JsonConvert.SerializeObject(request, _serializerSettings);
            requestMessage.Content = new StringContent(requestJson, Encoding.UTF8, "application/json");
            // perform a keywords API search
            using var response = await _client.SendAsync(requestMessage);
            _logger.LogInformation($"[{nameof(PerformApiSearchQueryAsync)}] Api responded with '{response.StatusCode}'. accesstoken='{authenticationResponse.AccessToken.Sanitize()}'");
            var result = await TryHandleResponseAsync(response, authenticationResponse, ApiVersion);
            if (!result.IsSuccessful)
            {
                // return api error
                return (result.Response, new KeywordSearchResponse(), requestJson, string.Empty);
            }

            // 200 OK
            var responseJson = await response.Content.ReadAsStringAsync();
            var results = JsonConvert.DeserializeObject<KeywordSearchResponse>(responseJson, _serializerSettings) ?? new();
            return (null, results, requestJson, responseJson);
        }

        public async Task<IApiResponse> ProductSearchAsync(OAuthAuthorization authenticationResponse, string partNumber)
        {
            /* important reminder - don't reference authResponse in here! */
            try
            {
                // set what fields we want from the API
                var uri = Url.Combine(_configuration.ApiUrl, $"products/v4/search/{HttpUtility.UrlEncode(partNumber)}/productdetails");
                _logger.LogInformation($"[{nameof(ProductSearchAsync)}] Creating product search request for '{partNumber}'...");
                var requestMessage = CreateRequest(authenticationResponse, HttpMethod.Get, uri);
                // perform a keywords API search
                var response = await _client.SendAsync(requestMessage);
                _logger.LogInformation($"[{nameof(ProductSearchAsync)}] Api responded with '{response.StatusCode}'");
                var result = await TryHandleResponseAsync(response, authenticationResponse, ApiVersion);
                if (!result.IsSuccessful)
                {
                    // return api error
                    return result.Response;
                }

                // 200 OK
                var responseJson = await response.Content.ReadAsStringAsync();
                var results = JsonConvert.DeserializeObject<Product>(responseJson, _serializerSettings) ?? new();
                return new ApiResponse(results, nameof(DigikeyApi));
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<IApiResponse> GetBarcodeDetailsAsync(OAuthAuthorization authenticationResponse, string barcode, ScannedLabelType barcodeType)
        {
            /* important reminder - don't reference authResponse in here! */
            try
            {
                // set what fields we want from the API
                // https://developer.digikey.com/products/barcode/barcoding/productbarcode

                var is2dBarcode = barcode.StartsWith("[)>");
                var endpoint = "Barcoding/v3/ProductBarcodes/";
                switch (barcodeType)
                {
                    case ScannedLabelType.Product:
                    default:
                        endpoint = "Barcoding/v3/ProductBarcodes/";
                        if (is2dBarcode)
                            endpoint = "Barcoding/v3/Product2DBarcodes/";
                        break;
                    case ScannedLabelType.Packlist:
                        endpoint = "Barcoding/v3/PackListBarcodes/";
                        if (is2dBarcode)
                            endpoint = "Barcoding/v3/PackList2DBarcodes/";
                        break;
                }

                var barcodeFormatted = barcode.ToString();
                if (is2dBarcode)
                {
                    // DigiKey requires the GS (Group separator) to be \u241D, and the RS (Record separator) to be \u241E
                    // GS
                    var gsReplacement = "\u241D";
                    barcodeFormatted = barcodeFormatted.Replace("\u001d", gsReplacement);
                    barcodeFormatted = barcodeFormatted.Replace("\u005d", gsReplacement);
                    // RS
                    var rsReplacement = "\u241E";
                    barcodeFormatted = barcodeFormatted.Replace("\u001e", rsReplacement);
                    barcodeFormatted = barcodeFormatted.Replace("\u005e", rsReplacement);

                }
                var barcodeFormattedPath = HttpUtility.UrlEncode(barcodeFormatted);

                var uri = new Uri(string.Join("/", _configuration.ApiUrl, endpoint) + barcodeFormattedPath);
                var requestMessage = CreateRequest(authenticationResponse, HttpMethod.Get, uri);
                // perform a keywords API search
                var response = await _client.SendAsync(requestMessage);
                var result = await TryHandleResponseAsync(response, authenticationResponse, ApiVersion);
                if (!result.IsSuccessful)
                {
                    // return api error
                    var contentString = await response.Content.ReadAsStringAsync();
                    result.Response.Errors.Add(contentString);
                    return result.Response;
                }

                // 200 OK
                var responseJson = await response.Content.ReadAsStringAsync();
                var results = JsonConvert.DeserializeObject<ProductBarcodeResponse>(responseJson, _serializerSettings) ?? new();
                return new ApiResponse(results, nameof(DigikeyApi));
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<IApiResponse> GetCategoriesAsync(OAuthAuthorization authenticationResponse)
        {
            /* important reminder - don't reference authResponse in here! */
            try
            {
                // set what fields we want from the API
                var uri = Url.Combine(_configuration.ApiUrl, "products/v4/search/categories");
                _logger.LogInformation($"[{nameof(GetCategoriesAsync)}] Creating categories request...");
                var requestMessage = CreateRequest(authenticationResponse, HttpMethod.Get, uri);
                // perform a keywords API search
                var response = await _client.SendAsync(requestMessage);
                _logger.LogInformation($"[{nameof(GetCategoriesAsync)}] Api responded with '{response.StatusCode}'");
                var result = await TryHandleResponseAsync(response, authenticationResponse, ApiVersion);
                if (!result.IsSuccessful)
                {
                    // return api error
                    return result.Response;
                }

                // 200 OK
                var responseJson = await response.Content.ReadAsStringAsync();
                var results = JsonConvert.DeserializeObject<Model.Integrations.DigiKey.V4.CategoriesResponse>(responseJson, _serializerSettings) ?? new();
                return new ApiResponse(results, nameof(DigikeyApi));
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}
