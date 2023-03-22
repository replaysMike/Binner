namespace Binner.Common.Integrations
{
    public partial class ArrowApi
    {
        private static class FakeResults
        {
            internal const string SearchResult1 = @"{""itemserviceresult"": {
  ""serviceMetaData"": [
    {
      ""version"": ""4.0.0""
    }
  ],
  ""transactionArea"": [
    {
      ""response"": {
        ""returnCode"": ""0"",
        ""returnMsg"": """",
        ""success"": true
      },
      ""responseSequence"": {
        ""transactionTime"": ""52.252 ms"",
        ""queryTime"": ""0 ms"",
        ""dbTime"": ""0 ms"",
        ""totalItems"": 71,
        ""resources"": [],
        ""qq"": 150
      }
    }
  ],
  ""data"": [
    {
      ""resources"": [
        {
          ""type"": ""search"",
          ""uri"": ""https://www.arrow.com/en/products/search?q=lm358""
        }
      ],
      ""PartList"": [
        {
          ""itemId"": 50282904,
          ""partNum"": ""LM358AM/NOPB"",
          ""manufacturer"": {
            ""mfrCd"": ""TI"",
            ""mfrName"": ""Texas Instruments""
          },
          ""desc"": ""Op Amp Dual Low Power Amplifier ±16V/32V 8-Pin SOIC Tube"",
          ""packageType"": """",
          ""resources"": [
            {
              ""type"": ""image_small"",
              ""uri"": ""https://static6.arrow.com/aropdfconversion/arrowimages/3f304a36ac6e93000334d1a2cf03ec6bd35c733b/d0008a.jpg""
            },
            {
              ""type"": ""image_large"",
              ""uri"": ""https://static6.arrow.com/aropdfconversion/arrowimages/e5533f271a03578b303601b99b62d8a183cba66a/d0008a.jpg""
            },
            {
              ""type"": ""cloud_part_detail"",
              ""uri"": ""https://www.arrow.com/en/products/lm358amnopb/texas-instruments""
            },
            {
              ""type"": ""api_part_detail"",
              ""uri"": ""http://api.arrow.com/itemservice/v2/en/detail?mfrCd=TI&partNum=LM358AM/NOPB""
            },
            {
              ""type"": ""part_detail"",
              ""uri"": ""http://api.arrow.com/itemservice/v2/en/detail?pkey=DQi2LkaVyHH5spKorRCBCY96Z5RTA0FYNLm8vY1Ud88""
            }
          ],
          ""EnvData"": {
            ""compliance"": [
              {
                ""displayLabel"": ""eurohs"",
                ""displayValue"": ""Compliant""
              },
              {
                ""displayLabel"": ""cnrohs"",
                ""displayValue"": ""Compliant""
              }
            ]
          },
          ""InvOrg"": {
            ""webSites"": [
              {
                ""code"": ""Verical.com"",
                ""name"": ""Verical.com"",
                ""sources"": [
                  {
                    ""currency"": ""USD"",
                    ""sourceCd"": ""VERICAL"",
                    ""displayName"": ""Verical"",
                    ""sourceParts"": [
                      {
                        ""packSize"": 1,
                        ""minimumOrderQuantity"": 16,
                        ""sourcePartId"": ""65130973"",
                        ""Prices"": {
                          ""resaleList"": [
                            {
                              ""displayPrice"": ""1.0927"",
                              ""price"": 1.0927,
                              ""minQty"": 16,
                              ""maxQty"": 24
                            },
                            {
                              ""displayPrice"": ""1.0818"",
                              ""price"": 1.0818,
                              ""minQty"": 25,
                              ""maxQty"": 94
                            },
                            {
                              ""displayPrice"": ""0.9385"",
                              ""price"": 0.9385,
                              ""minQty"": 95,
                              ""maxQty"": 284
                            },
                            {
                              ""displayPrice"": ""0.8794"",
                              ""price"": 0.8794,
                              ""minQty"": 285,
                              ""maxQty"": 569
                            },
                            {
                              ""displayPrice"": ""0.7786"",
                              ""price"": 0.7786,
                              ""minQty"": 570,
                              ""maxQty"": 1044
                            },
                            {
                              ""displayPrice"": ""0.5811"",
                              ""price"": 0.5811,
                              ""minQty"": 1045,
                              ""maxQty"": 2564
                            },
                            {
                              ""displayPrice"": ""0.5752"",
                              ""price"": 0.5752,
                              ""minQty"": 2565,
                              ""maxQty"": 999999999
                            }
                          ]
                        },
                        ""Availability"": [
                          {
                            ""fohQty"": 85,
                            ""availabilityCd"": ""INSTK"",
                            ""availabilityMessage"": ""In Stock"",
                            ""pipeline"": []
                          }
                        ],
                        ""customerSpecificPricing"": [],
                        ""customerSpecificInventory"": [],
                        ""dateCode"": ""2230"",
                        ""resources"": [
                          {
                            ""type"": ""detail"",
                            ""uri"": ""https://www.verical.com/pd/national-semiconductor-amplifier---operational-LM358AM-NOPB-55404""
                          },
                          {
                            ""type"": ""add_to_cart"",
                            ""uri"": ""https://www.verical.com/pd/national-semiconductor-amplifier---operational-LM358AM-NOPB-55404""
                          }
                        ],
                        ""inStock"": true,
                        ""mfrLeadTime"": 0,
                        ""tariffFlag"": false,
                        ""arrowLeadTime"": """",
                        ""isNcnr"": false,
                        ""isNpi"": false,
                        ""productCode"": ""LM358AM/NOPB"",
                        ""eccnCode"": ""EAR99"",
                        ""htsCode"": ""8542.33.00.01"",
                        ""countryOfOrigin"": ""MY"",
                        ""containerType"": """"
                      }
                    ]
                  }
                ]
              },
              {
                ""code"": ""arrow.com"",
                ""name"": ""arrow.com"",
                ""sources"": [
                  {
                    ""currency"": ""USD"",
                    ""sourceCd"": ""ASIA"",
                    ""displayName"": ""Asia"",
                    ""sourceParts"": [
                      {
                        ""packSize"": 95,
                        ""minimumOrderQuantity"": 380,
                        ""sourcePartId"": ""0898_00015182"",
                        ""Prices"": {
                          ""resaleList"": [
                            {
                              ""displayPrice"": ""0.5523"",
                              ""price"": 0.5523,
                              ""minQty"": 380,
                              ""maxQty"": 569
                            },
                            {
                              ""displayPrice"": ""0.4378"",
                              ""price"": 0.4378,
                              ""minQty"": 570,
                              ""maxQty"": 1044
                            },
                            {
                              ""displayPrice"": ""0.4133"",
                              ""price"": 0.4133,
                              ""minQty"": 1045,
                              ""maxQty"": 99999999
                            }
                          ]
                        },
                        ""Availability"": [
                          {
                            ""fohQty"": 380,
                            ""availabilityCd"": ""INSTK"",
                            ""availabilityMessage"": ""In Stock"",
                            ""pipeline"": []
                          }
                        ],
                        ""customerSpecificPricing"": [],
                        ""customerSpecificInventory"": [],
                        ""dateCode"": ""2245"",
                        ""resources"": [
                          {
                            ""type"": ""detail"",
                            ""uri"": ""https://www.arrow.com/en/products/lm358amnopb/texas-instruments?region=asia""
                          },
                          {
                            ""type"": ""add_to_cart"",
                            ""uri"": ""https://www.arrow.com/en/products/lm358amnopb/texas-instruments?region=asia""
                          }
                        ],
                        ""inStock"": true,
                        ""mfrLeadTime"": 6,
                        ""tariffFlag"": false,
                        ""shipsFrom"": ""Hong Kong"",
                        ""shipsIn"": ""Ships in 3 days "",
                        ""arrowLeadTime"": """",
                        ""isNcnr"": false,
                        ""isNpi"": false,
                        ""productCode"": ""LM358AM/NOPB"",
                        ""eccnCode"": ""EAR99"",
                        ""htsCode"": ""8542.33.00.01"",
                        ""countryOfOrigin"": ""MY"",
                        ""containerType"": """"
                      }
                    ]
                  },
                  {
                    ""currency"": ""USD"",
                    ""sourceCd"": ""ACNA"",
                    ""displayName"": ""Arrow Americas"",
                    ""sourceParts"": [
                      {
                        ""packSize"": 1,
                        ""minimumOrderQuantity"": 1,
                        ""sourcePartId"": ""V99:2348_07275143"",
                        ""Prices"": {
                          ""resaleList"": [
                            {
                              ""displayPrice"": ""0.9598"",
                              ""price"": 0.9598,
                              ""minQty"": 1,
                              ""maxQty"": 9
                            },
                            {
                              ""displayPrice"": ""0.8596"",
                              ""price"": 0.8596,
                              ""minQty"": 10,
                              ""maxQty"": 24
                            },
                            {
                              ""displayPrice"": ""0.8171"",
                              ""price"": 0.8171,
                              ""minQty"": 25,
                              ""maxQty"": 94
                            },
                            {
                              ""displayPrice"": ""0.6313"",
                              ""price"": 0.6313,
                              ""minQty"": 95,
                              ""maxQty"": 284
                            },
                            {
                              ""displayPrice"": ""0.5592"",
                              ""price"": 0.5592,
                              ""minQty"": 285,
                              ""maxQty"": 569
                            },
                            {
                              ""displayPrice"": ""0.4431"",
                              ""price"": 0.4431,
                              ""minQty"": 570,
                              ""maxQty"": 1044
                            },
                            {
                              ""displayPrice"": ""0.4183"",
                              ""price"": 0.4183,
                              ""minQty"": 1045,
                              ""maxQty"": 2564
                            },
                            {
                              ""displayPrice"": ""0.4141"",
                              ""price"": 0.4141,
                              ""minQty"": 2565,
                              ""maxQty"": 99999999
                            }
                          ]
                        },
                        ""Availability"": [
                          {
                            ""fohQty"": 85,
                            ""availabilityCd"": ""INSTK"",
                            ""availabilityMessage"": ""In Stock"",
                            ""pipeline"": [
                              {
                                ""delivery"": ""10-NOV-2023"",
                                ""quantity"": 665
                              }
                            ]
                          }
                        ],
                        ""customerSpecificPricing"": [],
                        ""customerSpecificInventory"": [],
                        ""dateCode"": ""2230"",
                        ""resources"": [
                          {
                            ""type"": ""detail"",
                            ""uri"": ""https://www.arrow.com/en/products/lm358amnopb/texas-instruments?region=nac""
                          },
                          {
                            ""type"": ""add_to_cart"",
                            ""uri"": ""https://www.arrow.com/en/products/lm358amnopb/texas-instruments?region=nac""
                          }
                        ],
                        ""inStock"": true,
                        ""mfrLeadTime"": 6,
                        ""tariffFlag"": false,
                        ""shipsFrom"": ""United States of America"",
                        ""shipsIn"": ""Ships today"",
                        ""arrowLeadTime"": """",
                        ""isNcnr"": false,
                        ""isNpi"": false,
                        ""productCode"": ""LM358AM/NOPB"",
                        ""eccnCode"": ""EAR99"",
                        ""htsCode"": ""8542.33.00.01"",
                        ""countryOfOrigin"": ""MY"",
                        ""containerType"": """"
                      },
                      {
                        ""packSize"": 95,
                        ""minimumOrderQuantity"": 760,
                        ""sourcePartId"": ""V39:1801_07275143"",
                        ""Prices"": {
                          ""resaleList"": [
                            {
                              ""displayPrice"": ""0.5166"",
                              ""price"": 0.5166,
                              ""minQty"": 95,
                              ""maxQty"": 284
                            },
                            {
                              ""displayPrice"": ""0.5014"",
                              ""price"": 0.5014,
                              ""minQty"": 285,
                              ""maxQty"": 569
                            },
                            {
                              ""displayPrice"": ""0.4727"",
                              ""price"": 0.4727,
                              ""minQty"": 570,
                              ""maxQty"": 1044
                            },
                            {
                              ""displayPrice"": ""0.4113"",
                              ""price"": 0.4113,
                              ""minQty"": 1045,
                              ""maxQty"": 2564
                            },
                            {
                              ""displayPrice"": ""0.4108"",
                              ""price"": 0.4108,
                              ""minQty"": 2565,
                              ""maxQty"": 99999999
                            }
                          ]
                        },
                        ""Availability"": [
                          {
                            ""fohQty"": 0,
                            ""availabilityCd"": ""NOSTK"",
                            ""availabilityMessage"": ""No Stock"",
                            ""pipeline"": [
                              {
                                ""delivery"": ""10-NOV-2023"",
                                ""quantity"": 665
                              }
                            ]
                          }
                        ],
                        ""customerSpecificPricing"": [],
                        ""customerSpecificInventory"": [],
                        ""dateCode"": ""2245"",
                        ""resources"": [
                          {
                            ""type"": ""detail"",
                            ""uri"": ""https://www.arrow.com/en/products/lm358amnopb/texas-instruments?region=nac""
                          },
                          {
                            ""type"": ""add_to_cart"",
                            ""uri"": ""https://www.arrow.com/en/products/lm358amnopb/texas-instruments?region=nac""
                          }
                        ],
                        ""inStock"": false,
                        ""mfrLeadTime"": 6,
                        ""tariffFlag"": false,
                        ""shipsFrom"": ""United States of America"",
                        ""shipsIn"": ""Ships today"",
                        ""arrowLeadTime"": """",
                        ""isNcnr"": false,
                        ""isNpi"": false,
                        ""productCode"": ""LM358AM/NOPB"",
                        ""eccnCode"": ""EAR99"",
                        ""htsCode"": ""8542.33.00.01"",
                        ""countryOfOrigin"": ""MY"",
                        ""containerType"": """"
                      },
                      {
                        ""packSize"": 95,
                        ""minimumOrderQuantity"": 760,
                        ""sourcePartId"": ""V36:1790_07275143"",
                        ""Prices"": {
                          ""resaleList"": [
                            {
                              ""displayPrice"": ""0.4727"",
                              ""price"": 0.4727,
                              ""minQty"": 760,
                              ""maxQty"": 1044
                            },
                            {
                              ""displayPrice"": ""0.416"",
                              ""price"": 0.416,
                              ""minQty"": 1045,
                              ""maxQty"": 2564
                            },
                            {
                              ""displayPrice"": ""0.4108"",
                              ""price"": 0.4108,
                              ""minQty"": 2565,
                              ""maxQty"": 99999999
                            }
                          ]
                        },
                        ""Availability"": [
                          {
                            ""fohQty"": 0,
                            ""availabilityCd"": ""QUOTE"",
                            ""availabilityMessage"": ""Call For Quote"",
                            ""pipeline"": []
                          }
                        ],
                        ""customerSpecificPricing"": [],
                        ""customerSpecificInventory"": [],
                        ""dateCode"": """",
                        ""resources"": [
                          {
                            ""type"": ""detail"",
                            ""uri"": ""https://www.arrow.com/en/products/lm358amnopb/texas-instruments?region=nac""
                          },
                          {
                            ""type"": ""add_to_cart"",
                            ""uri"": ""https://www.arrow.com/en/products/lm358amnopb/texas-instruments?region=nac""
                          }
                        ],
                        ""inStock"": false,
                        ""mfrLeadTime"": 6,
                        ""tariffFlag"": false,
                        ""shipsFrom"": ""United States of America"",
                        ""shipsIn"": ""Ships today"",
                        ""arrowLeadTime"": """",
                        ""isNcnr"": false,
                        ""isNpi"": false,
                        ""productCode"": ""LM358AM/NOPB"",
                        ""eccnCode"": ""EAR99"",
                        ""htsCode"": ""8542.33.00.01"",
                        ""containerType"": """"
                      }
                    ]
                  }
                ]
              }
            ]
          },
          ""hasDatasheet"": false,
          ""categoryName"": ""Operational Amplifiers - Op Amps"",
          ""status"": ""Active""
        },
        {
          ""itemId"": 10221973,
          ""partNum"": ""LM358PWRG3"",
          ""manufacturer"": {
            ""mfrCd"": ""TI"",
            ""mfrName"": ""Texas Instruments""
          },
          ""desc"": ""Op Amp Dual GP ±16V/32V 8-Pin TSSOP T/R"",
          ""packageType"": """",
          ""resources"": [
            {
              ""type"": ""image_small"",
              ""uri"": ""https://static6.arrow.com/aropdfconversion/arrowimages/3b520cab184e09c762a1279a9bd0defeeee93b3e/pw0008a.jpg""
            },
            {
              ""type"": ""image_large"",
              ""uri"": ""https://static6.arrow.com/aropdfconversion/arrowimages/345870321d113bab9c76ab327f30600d9a04705d/pw0008a.jpg""
            },
            {
              ""type"": ""cloud_part_detail"",
              ""uri"": ""https://www.arrow.com/en/products/lm358pwrg3/texas-instruments""
            },
            {
              ""type"": ""api_part_detail"",
              ""uri"": ""http://api.arrow.com/itemservice/v2/en/detail?mfrCd=TI&partNum=LM358PWRG3""
            },
            {
              ""type"": ""part_detail"",
              ""uri"": ""http://api.arrow.com/itemservice/v2/en/detail?pkey=6Ngxh91oiQ_ZrwKPKaHpKVZ_apYYmJbK8aktXq_Z-RU""
            }
          ],
          ""EnvData"": {
            ""compliance"": [
              {
                ""displayLabel"": ""eurohs"",
                ""displayValue"": ""Compliant""
              },
              {
                ""displayLabel"": ""cnrohs"",
                ""displayValue"": ""Compliant""
              }
            ]
          },
          ""InvOrg"": {
            ""webSites"": [
              {
                ""code"": ""arrow.com"",
                ""name"": ""arrow.com"",
                ""sources"": [
                  {
                    ""currency"": ""USD"",
                    ""sourceCd"": ""ASIA"",
                    ""displayName"": ""Asia"",
                    ""sourceParts"": [
                      {
                        ""packSize"": 2000,
                        ""minimumOrderQuantity"": 2000,
                        ""sourcePartId"": ""0898_00957669"",
                        ""Prices"": {
                          ""resaleList"": [
                            {
                              ""displayPrice"": ""0.18"",
                              ""price"": 0.18,
                              ""minQty"": 2000,
                              ""maxQty"": 3999
                            },
                            {
                              ""displayPrice"": ""0.0982"",
                              ""price"": 0.0982,
                              ""minQty"": 4000,
                              ""maxQty"": 5999
                            },
                            {
                              ""displayPrice"": ""0.0913"",
                              ""price"": 0.0913,
                              ""minQty"": 6000,
                              ""maxQty"": 9999
                            },
                            {
                              ""displayPrice"": ""0.0845"",
                              ""price"": 0.0845,
                              ""minQty"": 10000,
                              ""maxQty"": 49999
                            },
                            {
                              ""displayPrice"": ""0.0816"",
                              ""price"": 0.0816,
                              ""minQty"": 50000,
                              ""maxQty"": 99999999
                            }
                          ]
                        },
                        ""Availability"": [
                          {
                            ""fohQty"": 6000,
                            ""availabilityCd"": ""INSTK"",
                            ""availabilityMessage"": ""In Stock"",
                            ""pipeline"": []
                          }
                        ],
                        ""customerSpecificPricing"": [],
                        ""customerSpecificInventory"": [],
                        ""dateCode"": ""2245"",
                        ""resources"": [
                          {
                            ""type"": ""detail"",
                            ""uri"": ""https://www.arrow.com/en/products/lm358pwrg3/texas-instruments?region=asia""
                          },
                          {
                            ""type"": ""add_to_cart"",
                            ""uri"": ""https://www.arrow.com/en/products/lm358pwrg3/texas-instruments?region=asia""
                          }
                        ],
                        ""inStock"": true,
                        ""mfrLeadTime"": 6,
                        ""tariffFlag"": false,
                        ""shipsFrom"": ""Hong Kong"",
                        ""shipsIn"": ""Ships in 3 days "",
                        ""arrowLeadTime"": """",
                        ""isNcnr"": false,
                        ""isNpi"": false,
                        ""productCode"": ""LM358PWRG3"",
                        ""eccnCode"": ""EAR99"",
                        ""htsCode"": ""8542.33.00.01"",
                        ""countryOfOrigin"": ""CN"",
                        ""containerType"": """"
                      }
                    ]
                  },
                  {
                    ""currency"": ""USD"",
                    ""sourceCd"": ""ACNA"",
                    ""displayName"": ""Arrow Americas"",
                    ""sourceParts"": [
                      {
                        ""packSize"": 2000,
                        ""minimumOrderQuantity"": 2000,
                        ""sourcePartId"": ""V36:1790_07275175"",
                        ""Prices"": {
                          ""resaleList"": [
                            {
                              ""displayPrice"": ""0.0775"",
                              ""price"": 0.0775,
                              ""minQty"": 2000,
                              ""maxQty"": 9999
                            },
                            {
                              ""displayPrice"": ""0.0756"",
                              ""price"": 0.0756,
                              ""minQty"": 10000,
                              ""maxQty"": 99999999
                            }
                          ]
                        },
                        ""Availability"": [
                          {
                            ""fohQty"": 0,
                            ""availabilityCd"": ""QUOTE"",
                            ""availabilityMessage"": ""Call For Quote"",
                            ""pipeline"": []
                          }
                        ],
                        ""customerSpecificPricing"": [],
                        ""customerSpecificInventory"": [],
                        ""dateCode"": """",
                        ""resources"": [
                          {
                            ""type"": ""detail"",
                            ""uri"": ""https://www.arrow.com/en/products/lm358pwrg3/texas-instruments?region=nac""
                          },
                          {
                            ""type"": ""add_to_cart"",
                            ""uri"": ""https://www.arrow.com/en/products/lm358pwrg3/texas-instruments?region=nac""
                          }
                        ],
                        ""inStock"": false,
                        ""mfrLeadTime"": 6,
                        ""tariffFlag"": false,
                        ""shipsFrom"": ""United States of America"",
                        ""shipsIn"": ""Ships today"",
                        ""arrowLeadTime"": """",
                        ""isNcnr"": false,
                        ""isNpi"": false,
                        ""productCode"": ""LM358PWRG3"",
                        ""eccnCode"": ""EAR99"",
                        ""htsCode"": ""8542.33.00.01"",
                        ""containerType"": """"
                      },
                      {
                        ""packSize"": 1,
                        ""minimumOrderQuantity"": 2000,
                        ""sourcePartId"": ""V72:2272_07275175"",
                        ""Availability"": [
                          {
                            ""fohQty"": 0,
                            ""availabilityCd"": ""QUOTE"",
                            ""availabilityMessage"": ""Call For Quote"",
                            ""pipeline"": []
                          }
                        ],
                        ""customerSpecificPricing"": [],
                        ""customerSpecificInventory"": [],
                        ""dateCode"": """",
                        ""resources"": [
                          {
                            ""type"": ""detail"",
                            ""uri"": ""https://www.arrow.com/en/products/lm358pwrg3/texas-instruments?region=nac""
                          },
                          {
                            ""type"": ""add_to_cart"",
                            ""uri"": ""https://www.arrow.com/en/products/lm358pwrg3/texas-instruments?region=nac""
                          }
                        ],
                        ""inStock"": false,
                        ""mfrLeadTime"": 6,
                        ""tariffFlag"": false,
                        ""shipsFrom"": ""United States of America"",
                        ""shipsIn"": ""Ships today"",
                        ""arrowLeadTime"": """",
                        ""isNcnr"": false,
                        ""isNpi"": false,
                        ""productCode"": ""LM358PWRG3"",
                        ""eccnCode"": ""EAR99"",
                        ""htsCode"": ""8542.33.00.01"",
                        ""containerType"": """"
                      },
                      {
                        ""packSize"": 2000,
                        ""minimumOrderQuantity"": 2000,
                        ""sourcePartId"": ""V39:1801_07275175"",
                        ""Prices"": {
                          ""resaleList"": [
                            {
                              ""displayPrice"": ""0.0775"",
                              ""price"": 0.0775,
                              ""minQty"": 2000,
                              ""maxQty"": 9999
                            },
                            {
                              ""displayPrice"": ""0.0756"",
                              ""price"": 0.0756,
                              ""minQty"": 10000,
                              ""maxQty"": 99999999
                            }
                          ]
                        },
                        ""Availability"": [
                          {
                            ""fohQty"": 0,
                            ""availabilityCd"": ""QUOTE"",
                            ""availabilityMessage"": ""Call For Quote"",
                            ""pipeline"": []
                          }
                        ],
                        ""customerSpecificPricing"": [],
                        ""customerSpecificInventory"": [],
                        ""dateCode"": """",
                        ""resources"": [
                          {
                            ""type"": ""detail"",
                            ""uri"": ""https://www.arrow.com/en/products/lm358pwrg3/texas-instruments?region=nac""
                          },
                          {
                            ""type"": ""add_to_cart"",
                            ""uri"": ""https://www.arrow.com/en/products/lm358pwrg3/texas-instruments?region=nac""
                          }
                        ],
                        ""inStock"": false,
                        ""mfrLeadTime"": 6,
                        ""tariffFlag"": false,
                        ""shipsFrom"": ""United States of America"",
                        ""shipsIn"": ""Ships today"",
                        ""arrowLeadTime"": """",
                        ""isNcnr"": false,
                        ""isNpi"": false,
                        ""productCode"": ""LM358PWRG3"",
                        ""eccnCode"": ""EAR99"",
                        ""htsCode"": ""8542.33.00.01"",
                        ""containerType"": """"
                      }
                    ]
                  }
                ]
              }
            ]
          },
          ""hasDatasheet"": false,
          ""categoryName"": ""Operational Amplifiers - Op Amps"",
          ""status"": ""Active""
        },
        {
          ""itemId"": 1474646,
          ""partNum"": ""LM358D"",
          ""manufacturer"": {
            ""mfrCd"": ""STMICRO"",
            ""mfrName"": ""STMicroelectronics""
          },
          ""desc"": ""Op Amp Dual Low Power Amplifier ±15V/30V 8-Pin SO N Tube"",
          ""packageType"": """",
          ""resources"": [
            {
              ""type"": ""datasheet"",
              ""uri"": ""https://static6.arrow.com/aropdfconversion/a92fa424e858bb82ca0c0d7bf3ec5dcc00ea0f1/2717545374532467cd00000464.pdf""
            },
            {
              ""type"": ""image_small"",
              ""uri"": ""https://static6.arrow.com/aropdfconversion/arrowimages/d6443add18dd1ef15bdc646129513969596b9c5/l6902d013tr.jpg""
            },
            {
              ""type"": ""image_large"",
              ""uri"": ""https://static6.arrow.com/aropdfconversion/arrowimages/7b58dbb14a78f7cd43be974b11faa4b8806e654/l6902d013tr.jpg""
            },
            {
              ""type"": ""cloud_part_detail"",
              ""uri"": ""https://www.arrow.com/en/products/lm358d/stmicroelectronics""
            },
            {
              ""type"": ""api_part_detail"",
              ""uri"": ""http://api.arrow.com/itemservice/v2/en/detail?mfrCd=STMICRO&partNum=LM358D""
            },
            {
              ""type"": ""part_detail"",
              ""uri"": ""http://api.arrow.com/itemservice/v2/en/detail?pkey=-modOE1Ww8XLgX211IX6kx_7qBrjUy7mnpHB90MzGT8""
            }
          ],
          ""EnvData"": {
            ""compliance"": [
              {
                ""displayLabel"": ""eurohs"",
                ""displayValue"": ""Compliant""
              },
              {
                ""displayLabel"": ""cnrohs"",
                ""displayValue"": ""Compliant""
              }
            ]
          },
          ""InvOrg"": {
            ""webSites"": [
              {
                ""code"": ""Verical.com"",
                ""name"": ""Verical.com"",
                ""sources"": [
                  {
                    ""currency"": ""USD"",
                    ""sourceCd"": ""VERICAL"",
                    ""displayName"": ""Verical"",
                    ""sourceParts"": [
                      {
                        ""packSize"": 1,
                        ""minimumOrderQuantity"": 63,
                        ""sourcePartId"": ""66242651"",
                        ""Prices"": {
                          ""resaleList"": [
                            {
                              ""displayPrice"": ""0.1078"",
                              ""price"": 0.1078,
                              ""minQty"": 63,
                              ""maxQty"": 999999999
                            }
                          ]
                        },
                        ""Availability"": [
                          {
                            ""fohQty"": 2303,
                            ""availabilityCd"": ""INSTK"",
                            ""availabilityMessage"": ""In Stock"",
                            ""pipeline"": []
                          }
                        ],
                        ""customerSpecificPricing"": [],
                        ""customerSpecificInventory"": [],
                        ""dateCode"": ""2301"",
                        ""resources"": [
                          {
                            ""type"": ""detail"",
                            ""uri"": ""https://www.verical.com/pd/stmicroelectronics-amplifier---operational-LM358D-50655""
                          },
                          {
                            ""type"": ""add_to_cart"",
                            ""uri"": ""https://www.verical.com/pd/stmicroelectronics-amplifier---operational-LM358D-50655""
                          }
                        ],
                        ""inStock"": true,
                        ""mfrLeadTime"": 0,
                        ""tariffFlag"": false,
                        ""arrowLeadTime"": """",
                        ""isNcnr"": false,
                        ""isNpi"": false,
                        ""productCode"": ""LM358D"",
                        ""eccnCode"": ""EAR99"",
                        ""htsCode"": ""8542.33.00.01"",
                        ""countryOfOrigin"": ""MA"",
                        ""containerType"": """"
                      }
                    ]
                  }
                ]
              },
              {
                ""code"": ""arrow.com"",
                ""name"": ""arrow.com"",
                ""sources"": [
                  {
                    ""currency"": ""USD"",
                    ""sourceCd"": ""EUROPE"",
                    ""displayName"": ""Europe"",
                    ""sourceParts"": [
                      {
                        ""packSize"": 1,
                        ""minimumOrderQuantity"": 1,
                        ""sourcePartId"": ""E02:0323_00034415"",
                        ""Prices"": {
                          ""resaleList"": [
                            {
                              ""displayPrice"": ""0.0678"",
                              ""price"": 0.0678,
                              ""minQty"": 1,
                              ""maxQty"": 99999999
                            }
                          ]
                        },
                        ""Availability"": [
                          {
                            ""fohQty"": 211196,
                            ""availabilityCd"": ""INSTKEU"",
                            ""availabilityMessage"": ""In Stock"",
                            ""pipeline"": [
                              {
                                ""delivery"": ""17-MAY-2023"",
                                ""quantity"": 57166
                              }
                            ]
                          }
                        ],
                        ""customerSpecificPricing"": [],
                        ""customerSpecificInventory"": [],
                        ""dateCode"": ""2309"",
                        ""resources"": [
                          {
                            ""type"": ""detail"",
                            ""uri"": ""https://www.arrow.com/en/products/lm358d/stmicroelectronics?region=europe""
                          },
                          {
                            ""type"": ""add_to_cart"",
                            ""uri"": ""https://www.arrow.com/en/products/lm358d/stmicroelectronics?region=europe""
                          }
                        ],
                        ""inStock"": true,
                        ""mfrLeadTime"": 36,
                        ""tariffFlag"": false,
                        ""shipsFrom"": ""Netherlands"",
                        ""shipsIn"": ""Ships in 2 days "",
                        ""arrowLeadTime"": """",
                        ""isNcnr"": false,
                        ""isNpi"": false,
                        ""productCode"": ""LM358D"",
                        ""eccnCode"": ""EAR99"",
                        ""htsCode"": ""8542.33.00.01"",
                        ""countryOfOrigin"": ""MA"",
                        ""containerType"": """"
                      }
                    ]
                  },
                  {
                    ""currency"": ""USD"",
                    ""sourceCd"": ""ACNA"",
                    ""displayName"": ""Arrow Americas"",
                    ""sourceParts"": [
                      {
                        ""packSize"": 1,
                        ""minimumOrderQuantity"": 2000,
                        ""sourcePartId"": ""V79:2366_22607576"",
                        ""Availability"": [
                          {
                            ""fohQty"": 0,
                            ""availabilityCd"": ""QUOTE"",
                            ""availabilityMessage"": ""Call For Quote"",
                            ""pipeline"": []
                          }
                        ],
                        ""customerSpecificPricing"": [],
                        ""customerSpecificInventory"": [],
                        ""dateCode"": """",
                        ""resources"": [
                          {
                            ""type"": ""detail"",
                            ""uri"": ""https://www.arrow.com/en/products/lm358d/stmicroelectronics?region=nac""
                          },
                          {
                            ""type"": ""add_to_cart"",
                            ""uri"": ""https://www.arrow.com/en/products/lm358d/stmicroelectronics?region=nac""
                          }
                        ],
                        ""inStock"": false,
                        ""mfrLeadTime"": 0,
                        ""tariffFlag"": false,
                        ""shipsFrom"": ""United States of America"",
                        ""shipsIn"": ""Ships today"",
                        ""arrowLeadTime"": """",
                        ""isNcnr"": false,
                        ""isNpi"": false,
                        ""productCode"": ""LM358D"",
                        ""eccnCode"": ""EAR99"",
                        ""htsCode"": ""8542.33.00.01"",
                        ""containerType"": """"
                      },
                      {
                        ""packSize"": 1,
                        ""minimumOrderQuantity"": 1,
                        ""sourcePartId"": ""V36:1790_06553393"",
                        ""Prices"": {
                          ""resaleList"": [
                            {
                              ""displayPrice"": ""0.0937"",
                              ""price"": 0.0937,
                              ""minQty"": 1,
                              ""maxQty"": 99999999
                            }
                          ]
                        },
                        ""Availability"": [
                          {
                            ""fohQty"": 2303,
                            ""availabilityCd"": ""INSTK"",
                            ""availabilityMessage"": ""In Stock"",
                            ""pipeline"": []
                          }
                        ],
                        ""customerSpecificPricing"": [],
                        ""customerSpecificInventory"": [],
                        ""dateCode"": ""2301"",
                        ""resources"": [
                          {
                            ""type"": ""detail"",
                            ""uri"": ""https://www.arrow.com/en/products/lm358d/stmicroelectronics?region=nac""
                          },
                          {
                            ""type"": ""add_to_cart"",
                            ""uri"": ""https://www.arrow.com/en/products/lm358d/stmicroelectronics?region=nac""
                          }
                        ],
                        ""inStock"": true,
                        ""mfrLeadTime"": 36,
                        ""tariffFlag"": false,
                        ""shipsFrom"": ""United States of America"",
                        ""shipsIn"": ""Ships today"",
                        ""arrowLeadTime"": """",
                        ""isNcnr"": false,
                        ""isNpi"": false,
                        ""productCode"": ""LM358D"",
                        ""eccnCode"": ""EAR99"",
                        ""htsCode"": ""8542.33.00.01"",
                        ""countryOfOrigin"": ""MA"",
                        ""containerType"": """"
                      },
                      {
                        ""packSize"": 1,
                        ""minimumOrderQuantity"": 2000,
                        ""sourcePartId"": ""V36:1790_22607576"",
                        ""Availability"": [
                          {
                            ""fohQty"": 0,
                            ""availabilityCd"": ""QUOTE"",
                            ""availabilityMessage"": ""Call For Quote"",
                            ""pipeline"": []
                          }
                        ],
                        ""customerSpecificPricing"": [],
                        ""customerSpecificInventory"": [],
                        ""dateCode"": """",
                        ""resources"": [
                          {
                            ""type"": ""detail"",
                            ""uri"": ""https://www.arrow.com/en/products/lm358d/stmicroelectronics?region=nac""
                          },
                          {
                            ""type"": ""add_to_cart"",
                            ""uri"": ""https://www.arrow.com/en/products/lm358d/stmicroelectronics?region=nac""
                          }
                        ],
                        ""inStock"": false,
                        ""mfrLeadTime"": 0,
                        ""tariffFlag"": false,
                        ""shipsFrom"": ""United States of America"",
                        ""shipsIn"": ""Ships today"",
                        ""arrowLeadTime"": """",
                        ""isNcnr"": false,
                        ""isNpi"": false,
                        ""productCode"": ""LM358D"",
                        ""eccnCode"": ""EAR99"",
                        ""htsCode"": ""8542.33.00.01"",
                        ""containerType"": """"
                      }
                    ]
                  }
                ]
              }
            ]
          },
          ""hasDatasheet"": true,
          ""categoryName"": ""Operational Amplifiers - Op Amps"",
          ""status"": ""Active""
        },
        {
          ""itemId"": 1512054,
          ""partNum"": ""LM358AD"",
          ""manufacturer"": {
            ""mfrCd"": ""STMICRO"",
            ""mfrName"": ""STMicroelectronics""
          },
          ""desc"": ""Op Amp Dual Low Power Amplifier ±15V/30V 8-Pin SO N Tube"",
          ""packageType"": """",
          ""resources"": [
            {
              ""type"": ""datasheet"",
              ""uri"": ""https://static6.arrow.com/aropdfconversion/a92fa424e858bb82ca0c0d7bf3ec5dcc00ea0f1/2717545374532467cd00000464.pdf""
            },
            {
              ""type"": ""image_small"",
              ""uri"": ""https://static6.arrow.com/aropdfconversion/arrowimages/d6443add18dd1ef15bdc646129513969596b9c5/l6902d013tr.jpg""
            },
            {
              ""type"": ""image_large"",
              ""uri"": ""https://static6.arrow.com/aropdfconversion/arrowimages/7b58dbb14a78f7cd43be974b11faa4b8806e654/l6902d013tr.jpg""
            },
            {
              ""type"": ""cloud_part_detail"",
              ""uri"": ""https://www.arrow.com/en/products/lm358ad/stmicroelectronics""
            },
            {
              ""type"": ""api_part_detail"",
              ""uri"": ""http://api.arrow.com/itemservice/v2/en/detail?mfrCd=STMICRO&partNum=LM358AD""
            },
            {
              ""type"": ""part_detail"",
              ""uri"": ""http://api.arrow.com/itemservice/v2/en/detail?pkey=aoDPOxd0ejWhCJfCs7NgqQ0ng6E2Eo1mq0PVKyJlvTs""
            }
          ],
          ""EnvData"": {
            ""compliance"": [
              {
                ""displayLabel"": ""eurohs"",
                ""displayValue"": ""Compliant""
              },
              {
                ""displayLabel"": ""cnrohs"",
                ""displayValue"": ""Compliant""
              }
            ]
          },
          ""InvOrg"": {
            ""webSites"": [
              {
                ""code"": ""Verical.com"",
                ""name"": ""Verical.com"",
                ""sources"": [
                  {
                    ""currency"": ""USD"",
                    ""sourceCd"": ""VERICAL"",
                    ""displayName"": ""Verical"",
                    ""sourceParts"": [
                      {
                        ""packSize"": 1,
                        ""minimumOrderQuantity"": 6,
                        ""sourcePartId"": ""34239228"",
                        ""Prices"": {
                          ""resaleList"": [
                            {
                              ""displayPrice"": ""1.163"",
                              ""price"": 1.163,
                              ""minQty"": 6,
                              ""maxQty"": 999999999
                            }
                          ]
                        },
                        ""Availability"": [
                          {
                            ""fohQty"": 9740,
                            ""availabilityCd"": ""INSTK"",
                            ""availabilityMessage"": ""In Stock"",
                            ""pipeline"": []
                          }
                        ],
                        ""customerSpecificPricing"": [],
                        ""customerSpecificInventory"": [],
                        ""dateCode"": ""1139"",
                        ""resources"": [
                          {
                            ""type"": ""detail"",
                            ""uri"": ""https://www.verical.com/pd/stmicroelectronics-amplifier---operational-LM358AD-302419""
                          },
                          {
                            ""type"": ""add_to_cart"",
                            ""uri"": ""https://www.verical.com/pd/stmicroelectronics-amplifier---operational-LM358AD-302419""
                          }
                        ],
                        ""inStock"": true,
                        ""mfrLeadTime"": 0,
                        ""tariffFlag"": false,
                        ""arrowLeadTime"": """",
                        ""isNcnr"": false,
                        ""isNpi"": false,
                        ""productCode"": ""LM358AD"",
                        ""eccnCode"": ""EAR99"",
                        ""htsCode"": ""8542.33.00.01"",
                        ""countryOfOrigin"": ""CN"",
                        ""containerType"": """"
                      }
                    ]
                  }
                ]
              },
              {
                ""code"": ""arrow.com"",
                ""name"": ""arrow.com"",
                ""sources"": [
                  {
                    ""currency"": ""USD"",
                    ""sourceCd"": ""EUROPE"",
                    ""displayName"": ""Europe"",
                    ""sourceParts"": [
                      {
                        ""packSize"": 1,
                        ""minimumOrderQuantity"": 1,
                        ""sourcePartId"": ""E02:0323_00211301"",
                        ""Prices"": {
                          ""resaleList"": [
                            {
                              ""displayPrice"": ""0.0887"",
                              ""price"": 0.0887,
                              ""minQty"": 1,
                              ""maxQty"": 99999999
                            }
                          ]
                        },
                        ""Availability"": [
                          {
                            ""fohQty"": 14737,
                            ""availabilityCd"": ""INSTKEU"",
                            ""availabilityMessage"": ""In Stock"",
                            ""pipeline"": [
                              {
                                ""delivery"": ""02-JUN-2025"",
                                ""quantity"": 8500
                              }
                            ]
                          }
                        ],
                        ""customerSpecificPricing"": [],
                        ""customerSpecificInventory"": [],
                        ""dateCode"": ""2242"",
                        ""resources"": [
                          {
                            ""type"": ""detail"",
                            ""uri"": ""https://www.arrow.com/en/products/lm358ad/stmicroelectronics?region=europe""
                          },
                          {
                            ""type"": ""add_to_cart"",
                            ""uri"": ""https://www.arrow.com/en/products/lm358ad/stmicroelectronics?region=europe""
                          }
                        ],
                        ""inStock"": true,
                        ""mfrLeadTime"": 36,
                        ""tariffFlag"": false,
                        ""shipsFrom"": ""Netherlands"",
                        ""shipsIn"": ""Ships in 2 days "",
                        ""arrowLeadTime"": """",
                        ""isNcnr"": false,
                        ""isNpi"": false,
                        ""productCode"": ""LM358AD"",
                        ""eccnCode"": ""EAR99"",
                        ""htsCode"": ""8542.33.00.01"",
                        ""countryOfOrigin"": ""MY"",
                        ""containerType"": """"
                      }
                    ]
                  },
                  {
                    ""currency"": ""USD"",
                    ""sourceCd"": ""ACNA"",
                    ""displayName"": ""Arrow Americas"",
                    ""sourceParts"": [
                      {
                        ""packSize"": 1,
                        ""minimumOrderQuantity"": 1,
                        ""sourcePartId"": ""V36:1790_06553390"",
                        ""Prices"": {
                          ""resaleList"": [
                            {
                              ""displayPrice"": ""2.396"",
                              ""price"": 2.396,
                              ""minQty"": 1,
                              ""maxQty"": 9
                            },
                            {
                              ""displayPrice"": ""2.266"",
                              ""price"": 2.266,
                              ""minQty"": 10,
                              ""maxQty"": 24
                            },
                            {
                              ""displayPrice"": ""2.19"",
                              ""price"": 2.19,
                              ""minQty"": 25,
                              ""maxQty"": 99
                            },
                            {
                              ""displayPrice"": ""2.021"",
                              ""price"": 2.021,
                              ""minQty"": 100,
                              ""maxQty"": 249
                            },
                            {
                              ""displayPrice"": ""1.953"",
                              ""price"": 1.953,
                              ""minQty"": 250,
                              ""maxQty"": 499
                            },
                            {
                              ""displayPrice"": ""1.822"",
                              ""price"": 1.822,
                              ""minQty"": 500,
                              ""maxQty"": 999
                            },
                            {
                              ""displayPrice"": ""1.776"",
                              ""price"": 1.776,
                              ""minQty"": 1000,
                              ""maxQty"": 1999
                            },
                            {
                              ""displayPrice"": ""1.771"",
                              ""price"": 1.771,
                              ""minQty"": 2000,
                              ""maxQty"": 99999999
                            }
                          ]
                        },
                        ""Availability"": [
                          {
                            ""fohQty"": 1,
                            ""availabilityCd"": ""INSTK"",
                            ""availabilityMessage"": ""In Stock"",
                            ""pipeline"": []
                          }
                        ],
                        ""customerSpecificPricing"": [],
                        ""customerSpecificInventory"": [],
                        ""dateCode"": ""2147"",
                        ""resources"": [
                          {
                            ""type"": ""detail"",
                            ""uri"": ""https://www.arrow.com/en/products/lm358ad/stmicroelectronics?region=nac""
                          },
                          {
                            ""type"": ""add_to_cart"",
                            ""uri"": ""https://www.arrow.com/en/products/lm358ad/stmicroelectronics?region=nac""
                          }
                        ],
                        ""inStock"": true,
                        ""mfrLeadTime"": 36,
                        ""tariffFlag"": false,
                        ""shipsFrom"": ""United States of America"",
                        ""shipsIn"": ""Ships today"",
                        ""arrowLeadTime"": """",
                        ""isNcnr"": false,
                        ""isNpi"": false,
                        ""productCode"": ""LM358AD"",
                        ""eccnCode"": ""EAR99"",
                        ""htsCode"": ""8542.33.00.01"",
                        ""countryOfOrigin"": ""MA"",
                        ""containerType"": """"
                      },
                      {
                        ""packSize"": 1,
                        ""minimumOrderQuantity"": 1,
                        ""sourcePartId"": ""V79:2366_17776784"",
                        ""Prices"": {
                          ""resaleList"": [
                            {
                              ""displayPrice"": ""1.163"",
                              ""price"": 1.163,
                              ""minQty"": 1,
                              ""maxQty"": 99999999
                            }
                          ]
                        },
                        ""Availability"": [
                          {
                            ""fohQty"": 9740,
                            ""availabilityCd"": ""INSTK"",
                            ""availabilityMessage"": ""In Stock"",
                            ""pipeline"": [
                              {
                                ""delivery"": ""07-APR-2025"",
                                ""quantity"": 1191
                              }
                            ]
                          }
                        ],
                        ""customerSpecificPricing"": [],
                        ""customerSpecificInventory"": [],
                        ""dateCode"": ""1139"",
                        ""resources"": [
                          {
                            ""type"": ""detail"",
                            ""uri"": ""https://www.arrow.com/en/products/lm358ad/stmicroelectronics?region=nac""
                          },
                          {
                            ""type"": ""add_to_cart"",
                            ""uri"": ""https://www.arrow.com/en/products/lm358ad/stmicroelectronics?region=nac""
                          }
                        ],
                        ""inStock"": true,
                        ""mfrLeadTime"": 99,
                        ""tariffFlag"": true,
                        ""shipsFrom"": ""United States of America"",
                        ""shipsIn"": ""Ships today"",
                        ""arrowLeadTime"": """",
                        ""isNcnr"": false,
                        ""isNpi"": false,
                        ""productCode"": ""LM358AD"",
                        ""eccnCode"": ""EAR99"",
                        ""htsCode"": ""8542.33.00.01"",
                        ""countryOfOrigin"": ""CN"",
                        ""containerType"": """"
                      }
                    ]
                  }
                ]
              }
            ]
          },
          ""hasDatasheet"": true,
          ""categoryName"": ""Operational Amplifiers - Op Amps"",
          ""status"": ""Active""
        },
        {
          ""itemId"": 1340885,
          ""partNum"": ""LM358DR2G"",
          ""manufacturer"": {
            ""mfrCd"": ""ONSEMI"",
            ""mfrName"": ""ON Semiconductor""
          },
          ""desc"": ""Op Amp Dual GP ±16V/32V 8-Pin SOIC N T/R"",
          ""packageType"": """",
          ""resources"": [
            {
              ""type"": ""datasheet"",
              ""uri"": ""https://static6.arrow.com/aropdfconversion/673bae871315ec144268deda76298da17ecb69f6/lm358-d.pdf""
            },
            {
              ""type"": ""image_small"",
              ""uri"": ""https://static6.arrow.com/aropdfconversion/arrowimages/1dec370fc8537f0308b6d4c600fda213d2dbf08e/lm393edr2g.jpg""
            },
            {
              ""type"": ""image_large"",
              ""uri"": ""https://static6.arrow.com/aropdfconversion/arrowimages/b4dda76d34c9e9f53dd6b8d5eed0d9c47be155e8/lm393edr2g.jpg""
            },
            {
              ""type"": ""cloud_part_detail"",
              ""uri"": ""https://www.arrow.com/en/products/lm358dr2g/on-semiconductor""
            },
            {
              ""type"": ""api_part_detail"",
              ""uri"": ""http://api.arrow.com/itemservice/v2/en/detail?mfrCd=ONSEMI&partNum=LM358DR2G""
            },
            {
              ""type"": ""part_detail"",
              ""uri"": ""http://api.arrow.com/itemservice/v2/en/detail?pkey=a23rYErGZ5jxxOfZqwfCv-rHOszApHm98-c1cUw_Avs""
            }
          ],
          ""EnvData"": {
            ""compliance"": [
              {
                ""displayLabel"": ""eurohs"",
                ""displayValue"": ""Compliant""
              },
              {
                ""displayLabel"": ""cnrohs"",
                ""displayValue"": ""Compliant""
              }
            ]
          },
          ""InvOrg"": {
            ""webSites"": [
              {
                ""code"": ""Verical.com"",
                ""name"": ""Verical.com"",
                ""sources"": [
                  {
                    ""currency"": ""USD"",
                    ""sourceCd"": ""VERICAL"",
                    ""displayName"": ""Verical"",
                    ""sourceParts"": [
                      {
                        ""packSize"": 1,
                        ""minimumOrderQuantity"": 65,
                        ""sourcePartId"": ""53490074"",
                        ""Prices"": {
                          ""resaleList"": [
                            {
                              ""displayPrice"": ""0.3741"",
                              ""price"": 0.3741,
                              ""minQty"": 65,
                              ""maxQty"": 99
                            },
                            {
                              ""displayPrice"": ""0.3207"",
                              ""price"": 0.3207,
                              ""minQty"": 100,
                              ""maxQty"": 249
                            },
                            {
                              ""displayPrice"": ""0.2273"",
                              ""price"": 0.2273,
                              ""minQty"": 250,
                              ""maxQty"": 499
                            },
                            {
                              ""displayPrice"": ""0.2251"",
                              ""price"": 0.2251,
                              ""minQty"": 500,
                              ""maxQty"": 999
                            },
                            {
                              ""displayPrice"": ""0.2228"",
                              ""price"": 0.2228,
                              ""minQty"": 1000,
                              ""maxQty"": 2999
                            },
                            {
                              ""displayPrice"": ""0.1246"",
                              ""price"": 0.1246,
                              ""minQty"": 3000,
                              ""maxQty"": 999999999
                            }
                          ]
                        },
                        ""Availability"": [
                          {
                            ""fohQty"": 3875,
                            ""availabilityCd"": ""INSTK"",
                            ""availabilityMessage"": ""In Stock"",
                            ""pipeline"": []
                          }
                        ],
                        ""customerSpecificPricing"": [],
                        ""customerSpecificInventory"": [],
                        ""dateCode"": ""2103"",
                        ""resources"": [
                          {
                            ""type"": ""detail"",
                            ""uri"": ""https://www.verical.com/pd/aptina-imaging-amplifier---operational-LM358DR2G-8416978""
                          },
                          {
                            ""type"": ""add_to_cart"",
                            ""uri"": ""https://www.verical.com/pd/aptina-imaging-amplifier---operational-LM358DR2G-8416978""
                          }
                        ],
                        ""inStock"": true,
                        ""mfrLeadTime"": 0,
                        ""tariffFlag"": false,
                        ""arrowLeadTime"": """",
                        ""isNcnr"": false,
                        ""isNpi"": false,
                        ""productCode"": ""LM358DR2G"",
                        ""eccnCode"": ""EAR99"",
                        ""htsCode"": ""8542.33.00.01"",
                        ""countryOfOrigin"": ""CN"",
                        ""containerType"": """"
                      },
                      {
                        ""packSize"": 1,
                        ""minimumOrderQuantity"": 89,
                        ""sourcePartId"": ""63477567"",
                        ""Prices"": {
                          ""resaleList"": [
                            {
                              ""displayPrice"": ""0.3327"",
                              ""price"": 0.3327,
                              ""minQty"": 89,
                              ""maxQty"": 99
                            },
                            {
                              ""displayPrice"": ""0.2269"",
                              ""price"": 0.2269,
                              ""minQty"": 100,
                              ""maxQty"": 249
                            },
                            {
                              ""displayPrice"": ""0.2179"",
                              ""price"": 0.2179,
                              ""minQty"": 250,
                              ""maxQty"": 499
                            },
                            {
                              ""displayPrice"": ""0.188"",
                              ""price"": 0.188,
                              ""minQty"": 500,
                              ""maxQty"": 999
                            },
                            {
                              ""displayPrice"": ""0.1405"",
                              ""price"": 0.1405,
                              ""minQty"": 1000,
                              ""maxQty"": 2999
                            },
                            {
                              ""displayPrice"": ""0.1282"",
                              ""price"": 0.1282,
                              ""minQty"": 3000,
                              ""maxQty"": 5999
                            },
                            {
                              ""displayPrice"": ""0.1166"",
                              ""price"": 0.1166,
                              ""minQty"": 6000,
                              ""maxQty"": 999999999
                            }
                          ]
                        },
                        ""Availability"": [
                          {
                            ""fohQty"": 6012,
                            ""availabilityCd"": ""INSTK"",
                            ""availabilityMessage"": ""In Stock"",
                            ""pipeline"": []
                          }
                        ],
                        ""customerSpecificPricing"": [],
                        ""customerSpecificInventory"": [],
                        ""dateCode"": ""2215"",
                        ""resources"": [
                          {
                            ""type"": ""detail"",
                            ""uri"": ""https://www.verical.com/pd/aptina-imaging-amplifier---operational-LM358DR2G-8416978""
                          },
                          {
                            ""type"": ""add_to_cart"",
                            ""uri"": ""https://www.verical.com/pd/aptina-imaging-amplifier---operational-LM358DR2G-8416978""
                          }
                        ],
                        ""inStock"": true,
                        ""mfrLeadTime"": 0,
                        ""tariffFlag"": false,
                        ""arrowLeadTime"": """",
                        ""isNcnr"": false,
                        ""isNpi"": false,
                        ""productCode"": ""LM358DR2G"",
                        ""eccnCode"": ""EAR99"",
                        ""htsCode"": ""8542.33.00.01"",
                        ""countryOfOrigin"": ""CN"",
                        ""containerType"": """"
                      },
                      {
                        ""packSize"": 2500,
                        ""minimumOrderQuantity"": 2500,
                        ""sourcePartId"": ""66311780"",
                        ""Prices"": {
                          ""resaleList"": [
                            {
                              ""displayPrice"": ""0.4678"",
                              ""price"": 0.4678,
                              ""minQty"": 2500,
                              ""maxQty"": 9999
                            },
                            {
                              ""displayPrice"": ""0.4632"",
                              ""price"": 0.4632,
                              ""minQty"": 10000,
                              ""maxQty"": 24999
                            },
                            {
                              ""displayPrice"": ""0.4585"",
                              ""price"": 0.4585,
                              ""minQty"": 25000,
                              ""maxQty"": 49999
                            },
                            {
                              ""displayPrice"": ""0.4538"",
                              ""price"": 0.4538,
                              ""minQty"": 50000,
                              ""maxQty"": 999999999
                            }
                          ]
                        },
                        ""Availability"": [
                          {
                            ""fohQty"": 117500,
                            ""availabilityCd"": ""INSTK"",
                            ""availabilityMessage"": ""In Stock"",
                            ""pipeline"": []
                          }
                        ],
                        ""customerSpecificPricing"": [],
                        ""customerSpecificInventory"": [],
                        ""dateCode"": ""2230"",
                        ""resources"": [
                          {
                            ""type"": ""detail"",
                            ""uri"": ""https://www.verical.com/pd/aptina-imaging-amplifier---operational-LM358DR2G-8416978""
                          },
                          {
                            ""type"": ""add_to_cart"",
                            ""uri"": ""https://www.verical.com/pd/aptina-imaging-amplifier---operational-LM358DR2G-8416978""
                          }
                        ],
                        ""inStock"": true,
                        ""mfrLeadTime"": 0,
                        ""tariffFlag"": false,
                        ""arrowLeadTime"": """",
                        ""isNcnr"": false,
                        ""isNpi"": false,
                        ""productCode"": ""LM358DR2G"",
                        ""eccnCode"": ""EAR99"",
                        ""htsCode"": ""8542.33.00.01"",
                        ""countryOfOrigin"": ""CN"",
                        ""containerType"": """"
                      }
                    ]
                  }
                ]
              },
              {
                ""code"": ""arrow.com"",
                ""name"": ""arrow.com"",
                ""sources"": [
                  {
                    ""currency"": ""USD"",
                    ""sourceCd"": ""EUROPE"",
                    ""displayName"": ""Europe"",
                    ""sourceParts"": [
                      {
                        ""packSize"": 1,
                        ""minimumOrderQuantity"": 1,
                        ""sourcePartId"": ""E21:3489_00017864"",
                        ""Prices"": {
                          ""resaleList"": [
                            {
                              ""displayPrice"": ""0.4218"",
                              ""price"": 0.4218,
                              ""minQty"": 1,
                              ""maxQty"": 9
                            },
                            {
                              ""displayPrice"": ""0.3234"",
                              ""price"": 0.3234,
                              ""minQty"": 10,
                              ""maxQty"": 24
                            },
                            {
                              ""displayPrice"": ""0.2991"",
                              ""price"": 0.2991,
                              ""minQty"": 25,
                              ""maxQty"": 99
                            },
                            {
                              ""displayPrice"": ""0.2184"",
                              ""price"": 0.2184,
                              ""minQty"": 100,
                              ""maxQty"": 249
                            },
                            {
                              ""displayPrice"": ""0.2097"",
                              ""price"": 0.2097,
                              ""minQty"": 250,
                              ""maxQty"": 499
                            },
                            {
                              ""displayPrice"": ""0.172"",
                              ""price"": 0.172,
                              ""minQty"": 500,
                              ""maxQty"": 999
                            },
                            {
                              ""displayPrice"": ""0.1272"",
                              ""price"": 0.1272,
                              ""minQty"": 1000,
                              ""maxQty"": 2499
                            },
                            {
                              ""displayPrice"": ""0.1152"",
                              ""price"": 0.1152,
                              ""minQty"": 2500,
                              ""maxQty"": 4999
                            },
                            {
                              ""displayPrice"": ""0.1141"",
                              ""price"": 0.1141,
                              ""minQty"": 5000,
                              ""maxQty"": 9999
                            },
                            {
                              ""displayPrice"": ""0.1088"",
                              ""price"": 0.1088,
                              ""minQty"": 10000,
                              ""maxQty"": 12499
                            },
                            {
                              ""displayPrice"": ""0.1076"",
                              ""price"": 0.1076,
                              ""minQty"": 12500,
                              ""maxQty"": 24999
                            },
                            {
                              ""displayPrice"": ""0.0998"",
                              ""price"": 0.0998,
                              ""minQty"": 25000,
                              ""maxQty"": 49999
                            },
                            {
                              ""displayPrice"": ""0.0987"",
                              ""price"": 0.0987,
                              ""minQty"": 50000,
                              ""maxQty"": 99999999
                            }
                          ]
                        },
                        ""Availability"": [
                          {
                            ""fohQty"": 1179,
                            ""availabilityCd"": ""INSTKEU"",
                            ""availabilityMessage"": ""In Stock"",
                            ""pipeline"": [
                              {
                                ""delivery"": ""29-SEP-2025"",
                                ""quantity"": 991000
                              }
                            ]
                          }
                        ],
                        ""customerSpecificPricing"": [],
                        ""customerSpecificInventory"": [],
                        ""dateCode"": ""2223"",
                        ""resources"": [
                          {
                            ""type"": ""detail"",
                            ""uri"": ""https://www.arrow.com/en/products/lm358dr2g/on-semiconductor?region=europe""
                          },
                          {
                            ""type"": ""add_to_cart"",
                            ""uri"": ""https://www.arrow.com/en/products/lm358dr2g/on-semiconductor?region=europe""
                          }
                        ],
                        ""inStock"": true,
                        ""mfrLeadTime"": 45,
                        ""tariffFlag"": false,
                        ""shipsFrom"": ""Netherlands"",
                        ""shipsIn"": ""Ships in 2 days "",
                        ""arrowLeadTime"": """",
                        ""isNcnr"": false,
                        ""isNpi"": false,
                        ""productCode"": ""LM358DR2G"",
                        ""eccnCode"": ""EAR99"",
                        ""htsCode"": ""8542.33.00.01"",
                        ""countryOfOrigin"": ""CN"",
                        ""containerType"": """"
                      }
                    ]
                  },
                  {
                    ""currency"": ""USD"",
                    ""sourceCd"": ""ACNA"",
                    ""displayName"": ""Arrow Americas"",
                    ""sourceParts"": [
                      {
                        ""packSize"": 1,
                        ""minimumOrderQuantity"": 1,
                        ""sourcePartId"": ""V72:2272_07323297"",
                        ""Prices"": {
                          ""resaleList"": [
                            {
                              ""displayPrice"": ""0.4443"",
                              ""price"": 0.4443,
                              ""minQty"": 1,
                              ""maxQty"": 9
                            },
                            {
                              ""displayPrice"": ""0.3736"",
                              ""price"": 0.3736,
                              ""minQty"": 10,
                              ""maxQty"": 24
                            },
                            {
                              ""displayPrice"": ""0.3356"",
                              ""price"": 0.3356,
                              ""minQty"": 25,
                              ""maxQty"": 99
                            },
                            {
                              ""displayPrice"": ""0.2544"",
                              ""price"": 0.2544,
                              ""minQty"": 100,
                              ""maxQty"": 249
                            },
                            {
                              ""displayPrice"": ""0.2273"",
                              ""price"": 0.2273,
                              ""minQty"": 250,
                              ""maxQty"": 499
                            },
                            {
                              ""displayPrice"": ""0.1874"",
                              ""price"": 0.1874,
                              ""minQty"": 500,
                              ""maxQty"": 999
                            },
                            {
                              ""displayPrice"": ""0.1394"",
                              ""price"": 0.1394,
                              ""minQty"": 1000,
                              ""maxQty"": 2999
                            },
                            {
                              ""displayPrice"": ""0.138"",
                              ""price"": 0.138,
                              ""minQty"": 3000,
                              ""maxQty"": 999999999
                            }
                          ]
                        },
                        ""Availability"": [
                          {
                            ""fohQty"": 3875,
                            ""availabilityCd"": ""CSTRP"",
                            ""availabilityMessage"": ""Cut Strips"",
                            ""pipeline"": []
                          }
                        ],
                        ""customerSpecificPricing"": [],
                        ""customerSpecificInventory"": [],
                        ""dateCode"": ""2103"",
                        ""resources"": [
                          {
                            ""type"": ""detail"",
                            ""uri"": ""https://www.arrow.com/en/products/lm358dr2g/on-semiconductor?region=nac""
                          },
                          {
                            ""type"": ""add_to_cart"",
                            ""uri"": ""https://www.arrow.com/en/products/lm358dr2g/on-semiconductor?region=nac""
                          }
                        ],
                        ""inStock"": true,
                        ""mfrLeadTime"": 45,
                        ""tariffFlag"": true,
                        ""shipsFrom"": ""United States of America"",
                        ""shipsIn"": ""Ships today"",
                        ""arrowLeadTime"": """",
                        ""isNcnr"": false,
                        ""isNpi"": false,
                        ""productCode"": ""LM358DR2G"",
                        ""eccnCode"": ""EAR99"",
                        ""htsCode"": ""8542.33.00.01"",
                        ""countryOfOrigin"": ""CN"",
                        ""containerType"": ""Cut Strips""
                      },
                      {
                        ""packSize"": 1,
                        ""minimumOrderQuantity"": 1,
                        ""sourcePartId"": ""V72:2272_26884750"",
                        ""Prices"": {
                          ""resaleList"": [
                            {
                              ""displayPrice"": ""0.4351"",
                              ""price"": 0.4351,
                              ""minQty"": 1,
                              ""maxQty"": 9
                            },
                            {
                              ""displayPrice"": ""0.367"",
                              ""price"": 0.367,
                              ""minQty"": 10,
                              ""maxQty"": 24
                            },
                            {
                              ""displayPrice"": ""0.3303"",
                              ""price"": 0.3303,
                              ""minQty"": 25,
                              ""maxQty"": 99
                            },
                            {
                              ""displayPrice"": ""0.2513"",
                              ""price"": 0.2513,
                              ""minQty"": 100,
                              ""maxQty"": 249
                            },
                            {
                              ""displayPrice"": ""0.2249"",
                              ""price"": 0.2249,
                              ""minQty"": 250,
                              ""maxQty"": 499
                            },
                            {
                              ""displayPrice"": ""0.1858"",
                              ""price"": 0.1858,
                              ""minQty"": 500,
                              ""maxQty"": 999
                            },
                            {
                              ""displayPrice"": ""0.1385"",
                              ""price"": 0.1385,
                              ""minQty"": 1000,
                              ""maxQty"": 2999
                            },
                            {
                              ""displayPrice"": ""0.1371"",
                              ""price"": 0.1371,
                              ""minQty"": 3000,
                              ""maxQty"": 5999
                            },
                            {
                              ""displayPrice"": ""0.1356"",
                              ""price"": 0.1356,
                              ""minQty"": 6000,
                              ""maxQty"": 999999999
                            }
                          ]
                        },
                        ""Availability"": [
                          {
                            ""fohQty"": 6012,
                            ""availabilityCd"": ""CSTRP"",
                            ""availabilityMessage"": ""Cut Strips"",
                            ""pipeline"": []
                          }
                        ],
                        ""customerSpecificPricing"": [],
                        ""customerSpecificInventory"": [],
                        ""dateCode"": ""2215"",
                        ""resources"": [
                          {
                            ""type"": ""detail"",
                            ""uri"": ""https://www.arrow.com/en/products/lm358dr2g/on-semiconductor?region=nac""
                          },
                          {
                            ""type"": ""add_to_cart"",
                            ""uri"": ""https://www.arrow.com/en/products/lm358dr2g/on-semiconductor?region=nac""
                          }
                        ],
                        ""inStock"": true,
                        ""mfrLeadTime"": 45,
                        ""tariffFlag"": true,
                        ""shipsFrom"": ""United States of America"",
                        ""shipsIn"": ""Ships today"",
                        ""arrowLeadTime"": """",
                        ""isNcnr"": false,
                        ""isNpi"": false,
                        ""productCode"": ""LM358DR2G"",
                        ""eccnCode"": ""EAR99"",
                        ""htsCode"": ""8542.33.00.01"",
                        ""countryOfOrigin"": ""CN"",
                        ""containerType"": ""Cut Strips""
                      },
                      {
                        ""packSize"": 2500,
                        ""minimumOrderQuantity"": 2500,
                        ""sourcePartId"": ""V36:1790_07323297"",
                        ""Prices"": {
                          ""resaleList"": [
                            {
                              ""displayPrice"": ""0.1387"",
                              ""price"": 0.1387,
                              ""minQty"": 2500,
                              ""maxQty"": 9999
                            },
                            {
                              ""displayPrice"": ""0.1361"",
                              ""price"": 0.1361,
                              ""minQty"": 10000,
                              ""maxQty"": 24999
                            },
                            {
                              ""displayPrice"": ""0.1247"",
                              ""price"": 0.1247,
                              ""minQty"": 25000,
                              ""maxQty"": 49999
                            },
                            {
                              ""displayPrice"": ""0.1234"",
                              ""price"": 0.1234,
                              ""minQty"": 50000,
                              ""maxQty"": 99999999
                            }
                          ]
                        },
                        ""Availability"": [
                          {
                            ""fohQty"": 118398,
                            ""availabilityCd"": ""INSTK"",
                            ""availabilityMessage"": ""In Stock"",
                            ""pipeline"": [
                              {
                                ""delivery"": ""14-JUL-2023"",
                                ""quantity"": 34500
                              }
                            ]
                          }
                        ],
                        ""customerSpecificPricing"": [],
                        ""customerSpecificInventory"": [],
                        ""dateCode"": ""2230"",
                        ""resources"": [
                          {
                            ""type"": ""detail"",
                            ""uri"": ""https://www.arrow.com/en/products/lm358dr2g/on-semiconductor?region=nac""
                          },
                          {
                            ""type"": ""add_to_cart"",
                            ""uri"": ""https://www.arrow.com/en/products/lm358dr2g/on-semiconductor?region=nac""
                          }
                        ],
                        ""inStock"": true,
                        ""mfrLeadTime"": 45,
                        ""tariffFlag"": true,
                        ""shipsFrom"": ""United States of America"",
                        ""shipsIn"": ""Ships today"",
                        ""arrowLeadTime"": """",
                        ""isNcnr"": false,
                        ""isNpi"": false,
                        ""productCode"": ""LM358DR2G"",
                        ""eccnCode"": ""EAR99"",
                        ""htsCode"": ""8542.33.00.01"",
                        ""countryOfOrigin"": ""CN"",
                        ""containerType"": """"
                      }
                    ]
                  }
                ]
              }
            ]
          },
          ""hasDatasheet"": true,
          ""categoryName"": ""Operational Amplifiers - Op Amps"",
          ""status"": ""Active""
        },
        {
          ""itemId"": 2010135,
          ""partNum"": ""LM358DT"",
          ""manufacturer"": {
            ""mfrCd"": ""STMICRO"",
            ""mfrName"": ""STMicroelectronics""
          },
          ""desc"": ""The LM328DT is a low power dual op-amp IC with 1.1MHz bandwidth, single and dual rail operation and full ground swing"",
          ""packageType"": """",
          ""resources"": [
            {
              ""type"": ""datasheet"",
              ""uri"": ""https://static6.arrow.com/aropdfconversion/a92fa424e858bb82ca0c0d7bf3ec5dcc00ea0f1/2717545374532467cd00000464.pdf""
            },
            {
              ""type"": ""image_small"",
              ""uri"": ""https://static6.arrow.com/aropdfconversion/arrowimages/d6443add18dd1ef15bdc646129513969596b9c5/l6902d013tr.jpg""
            },
            {
              ""type"": ""image_large"",
              ""uri"": ""https://static6.arrow.com/aropdfconversion/arrowimages/7b58dbb14a78f7cd43be974b11faa4b8806e654/l6902d013tr.jpg""
            },
            {
              ""type"": ""cloud_part_detail"",
              ""uri"": ""https://www.arrow.com/en/products/lm358dt/stmicroelectronics""
            },
            {
              ""type"": ""api_part_detail"",
              ""uri"": ""http://api.arrow.com/itemservice/v2/en/detail?mfrCd=STMICRO&partNum=LM358DT""
            },
            {
              ""type"": ""part_detail"",
              ""uri"": ""http://api.arrow.com/itemservice/v2/en/detail?pkey=c_IRRBRxIDz7gbj-C8zJx04A3GHrnuFg41FvTEJH9zQ""
            }
          ],
          ""EnvData"": {
            ""compliance"": [
              {
                ""displayLabel"": ""eurohs"",
                ""displayValue"": ""Compliant""
              },
              {
                ""displayLabel"": ""cnrohs"",
                ""displayValue"": ""Compliant""
              }
            ]
          },
          ""InvOrg"": {
            ""webSites"": [
              {
                ""code"": ""Verical.com"",
                ""name"": ""Verical.com"",
                ""sources"": [
                  {
                    ""currency"": ""USD"",
                    ""sourceCd"": ""VERICAL"",
                    ""displayName"": ""Verical"",
                    ""sourceParts"": [
                      {
                        ""packSize"": 2500,
                        ""minimumOrderQuantity"": 5000,
                        ""sourcePartId"": ""64130269"",
                        ""Prices"": {
                          ""resaleList"": [
                            {
                              ""displayPrice"": ""0.155"",
                              ""price"": 0.155,
                              ""minQty"": 5000,
                              ""maxQty"": 9999
                            },
                            {
                              ""displayPrice"": ""0.093"",
                              ""price"": 0.093,
                              ""minQty"": 10000,
                              ""maxQty"": 14999
                            },
                            {
                              ""displayPrice"": ""0.0685"",
                              ""price"": 0.0685,
                              ""minQty"": 15000,
                              ""maxQty"": 999999999
                            }
                          ]
                        },
                        ""Availability"": [
                          {
                            ""fohQty"": 52500,
                            ""availabilityCd"": ""INSTK"",
                            ""availabilityMessage"": ""In Stock"",
                            ""pipeline"": []
                          }
                        ],
                        ""customerSpecificPricing"": [],
                        ""customerSpecificInventory"": [],
                        ""dateCode"": ""2239"",
                        ""resources"": [
                          {
                            ""type"": ""detail"",
                            ""uri"": ""https://www.verical.com/pd/stmicroelectronics-amplifier---operational-LM358DT-39583""
                          },
                          {
                            ""type"": ""add_to_cart"",
                            ""uri"": ""https://www.verical.com/pd/stmicroelectronics-amplifier---operational-LM358DT-39583""
                          }
                        ],
                        ""inStock"": true,
                        ""mfrLeadTime"": 0,
                        ""tariffFlag"": false,
                        ""arrowLeadTime"": """",
                        ""isNcnr"": false,
                        ""isNpi"": false,
                        ""productCode"": ""LM358DT"",
                        ""eccnCode"": ""EAR99"",
                        ""htsCode"": ""8542.33.00.01"",
                        ""countryOfOrigin"": ""CN"",
                        ""containerType"": """"
                      },
                      {
                        ""packSize"": 1,
                        ""minimumOrderQuantity"": 129,
                        ""sourcePartId"": ""33741244"",
                        ""Prices"": {
                          ""resaleList"": [
                            {
                              ""displayPrice"": ""0.2486"",
                              ""price"": 0.2486,
                              ""minQty"": 129,
                              ""maxQty"": 199
                            },
                            {
                              ""displayPrice"": ""0.2473"",
                              ""price"": 0.2473,
                              ""minQty"": 200,
                              ""maxQty"": 499
                            },
                            {
                              ""displayPrice"": ""0.2014"",
                              ""price"": 0.2014,
                              ""minQty"": 500,
                              ""maxQty"": 999
                            },
                            {
                              ""displayPrice"": ""0.1874"",
                              ""price"": 0.1874,
                              ""minQty"": 1000,
                              ""maxQty"": 4999
                            },
                            {
                              ""displayPrice"": ""0.167"",
                              ""price"": 0.167,
                              ""minQty"": 5000,
                              ""maxQty"": 999999999
                            }
                          ]
                        },
                        ""Availability"": [
                          {
                            ""fohQty"": 7205,
                            ""availabilityCd"": ""INSTK"",
                            ""availabilityMessage"": ""In Stock"",
                            ""pipeline"": []
                          }
                        ],
                        ""customerSpecificPricing"": [],
                        ""customerSpecificInventory"": [],
                        ""dateCode"": """",
                        ""resources"": [
                          {
                            ""type"": ""detail"",
                            ""uri"": ""https://www.verical.com/pd/stmicroelectronics-amplifier---operational-LM358DT-39583""
                          },
                          {
                            ""type"": ""add_to_cart"",
                            ""uri"": ""https://www.verical.com/pd/stmicroelectronics-amplifier---operational-LM358DT-39583""
                          }
                        ],
                        ""inStock"": true,
                        ""mfrLeadTime"": 0,
                        ""tariffFlag"": false,
                        ""arrowLeadTime"": """",
                        ""isNcnr"": false,
                        ""isNpi"": false,
                        ""productCode"": ""LM358DT"",
                        ""eccnCode"": ""EAR99"",
                        ""htsCode"": ""8542.33.00.01"",
                        ""containerType"": """"
                      },
                      {
                        ""packSize"": 2500,
                        ""minimumOrderQuantity"": 2500,
                        ""sourcePartId"": ""66596827"",
                        ""Prices"": {
                          ""resaleList"": [
                            {
                              ""displayPrice"": ""0.0591"",
                              ""price"": 0.0591,
                              ""minQty"": 2500,
                              ""maxQty"": 999999999
                            }
                          ]
                        },
                        ""Availability"": [
                          {
                            ""fohQty"": 237500,
                            ""availabilityCd"": ""INSTK"",
                            ""availabilityMessage"": ""In Stock"",
                            ""pipeline"": []
                          }
                        ],
                        ""customerSpecificPricing"": [],
                        ""customerSpecificInventory"": [],
                        ""dateCode"": ""2307"",
                        ""resources"": [
                          {
                            ""type"": ""detail"",
                            ""uri"": ""https://www.verical.com/pd/stmicroelectronics-amplifier---operational-LM358DT-39583""
                          },
                          {
                            ""type"": ""add_to_cart"",
                            ""uri"": ""https://www.verical.com/pd/stmicroelectronics-amplifier---operational-LM358DT-39583""
                          }
                        ],
                        ""inStock"": true,
                        ""mfrLeadTime"": 0,
                        ""tariffFlag"": false,
                        ""arrowLeadTime"": """",
                        ""isNcnr"": false,
                        ""isNpi"": false,
                        ""productCode"": ""LM358DT"",
                        ""eccnCode"": ""EAR99"",
                        ""htsCode"": ""8542.33.00.01"",
                        ""countryOfOrigin"": ""CN"",
                        ""containerType"": """"
                      }
                    ]
                  }
                ]
              },
              {
                ""code"": ""arrow.com"",
                ""name"": ""arrow.com"",
                ""sources"": [
                  {
                    ""currency"": ""USD"",
                    ""sourceCd"": ""EUROPE"",
                    ""displayName"": ""Europe"",
                    ""sourceParts"": [
                      {
                        ""packSize"": 2500,
                        ""minimumOrderQuantity"": 2500,
                        ""sourcePartId"": ""E02:0323_00035655"",
                        ""Prices"": {
                          ""resaleList"": [
                            {
                              ""displayPrice"": ""0.1394"",
                              ""price"": 0.1394,
                              ""minQty"": 2500,
                              ""maxQty"": 4999
                            },
                            {
                              ""displayPrice"": ""0.1309"",
                              ""price"": 0.1309,
                              ""minQty"": 5000,
                              ""maxQty"": 12499
                            },
                            {
                              ""displayPrice"": ""0.1224"",
                              ""price"": 0.1224,
                              ""minQty"": 12500,
                              ""maxQty"": 24999
                            },
                            {
                              ""displayPrice"": ""0.1195"",
                              ""price"": 0.1195,
                              ""minQty"": 25000,
                              ""maxQty"": 99999999
                            }
                          ]
                        },
                        ""Availability"": [
                          {
                            ""fohQty"": 237500,
                            ""availabilityCd"": ""INSTKEU"",
                            ""availabilityMessage"": ""In Stock"",
                            ""pipeline"": [
                              {
                                ""delivery"": ""12-MAY-2023"",
                                ""quantity"": 5000
                              }
                            ]
                          }
                        ],
                        ""customerSpecificPricing"": [],
                        ""customerSpecificInventory"": [],
                        ""dateCode"": ""2307"",
                        ""resources"": [
                          {
                            ""type"": ""detail"",
                            ""uri"": ""https://www.arrow.com/en/products/lm358dt/stmicroelectronics?region=europe""
                          },
                          {
                            ""type"": ""add_to_cart"",
                            ""uri"": ""https://www.arrow.com/en/products/lm358dt/stmicroelectronics?region=europe""
                          }
                        ],
                        ""inStock"": true,
                        ""mfrLeadTime"": 38,
                        ""tariffFlag"": false,
                        ""shipsFrom"": ""Netherlands"",
                        ""shipsIn"": ""Ships in 2 days "",
                        ""arrowLeadTime"": """",
                        ""isNcnr"": false,
                        ""isNpi"": false,
                        ""productCode"": ""LM358DT"",
                        ""eccnCode"": ""EAR99"",
                        ""htsCode"": ""8542.33.00.01"",
                        ""countryOfOrigin"": ""CN"",
                        ""containerType"": """"
                      }
                    ]
                  },
                  {
                    ""currency"": ""USD"",
                    ""sourceCd"": ""ACNA"",
                    ""displayName"": ""Arrow Americas"",
                    ""sourceParts"": [
                      {
                        ""packSize"": 2500,
                        ""minimumOrderQuantity"": 5000,
                        ""sourcePartId"": ""V36:1790_06548549"",
                        ""Prices"": {
                          ""resaleList"": [
                            {
                              ""displayPrice"": ""0.1549"",
                              ""price"": 0.1549,
                              ""minQty"": 5000,
                              ""maxQty"": 12499
                            },
                            {
                              ""displayPrice"": ""0.1505"",
                              ""price"": 0.1505,
                              ""minQty"": 12500,
                              ""maxQty"": 24999
                            },
                            {
                              ""displayPrice"": ""0.138"",
                              ""price"": 0.138,
                              ""minQty"": 25000,
                              ""maxQty"": 99999999
                            }
                          ]
                        },
                        ""Availability"": [
                          {
                            ""fohQty"": 52500,
                            ""availabilityCd"": ""INSTK"",
                            ""availabilityMessage"": ""In Stock"",
                            ""pipeline"": [
                              {
                                ""delivery"": ""01-MAY-2023"",
                                ""quantity"": 5000
                              }
                            ]
                          }
                        ],
                        ""customerSpecificPricing"": [],
                        ""customerSpecificInventory"": [],
                        ""dateCode"": ""2239"",
                        ""resources"": [
                          {
                            ""type"": ""detail"",
                            ""uri"": ""https://www.arrow.com/en/products/lm358dt/stmicroelectronics?region=nac""
                          },
                          {
                            ""type"": ""add_to_cart"",
                            ""uri"": ""https://www.arrow.com/en/products/lm358dt/stmicroelectronics?region=nac""
                          }
                        ],
                        ""inStock"": true,
                        ""mfrLeadTime"": 38,
                        ""tariffFlag"": true,
                        ""shipsFrom"": ""United States of America"",
                        ""shipsIn"": ""Ships today"",
                        ""arrowLeadTime"": """",
                        ""isNcnr"": false,
                        ""isNpi"": false,
                        ""productCode"": ""LM358DT"",
                        ""eccnCode"": ""EAR99"",
                        ""htsCode"": ""8542.33.00.01"",
                        ""countryOfOrigin"": ""CN"",
                        ""containerType"": """"
                      },
                      {
                        ""packSize"": 1,
                        ""minimumOrderQuantity"": 5000,
                        ""sourcePartId"": ""V72:2272_06548549"",
                        ""Availability"": [
                          {
                            ""fohQty"": 0,
                            ""availabilityCd"": ""QUOTE"",
                            ""availabilityMessage"": ""Call For Quote"",
                            ""pipeline"": []
                          }
                        ],
                        ""customerSpecificPricing"": [],
                        ""customerSpecificInventory"": [],
                        ""dateCode"": """",
                        ""resources"": [
                          {
                            ""type"": ""detail"",
                            ""uri"": ""https://www.arrow.com/en/products/lm358dt/stmicroelectronics?region=nac""
                          },
                          {
                            ""type"": ""add_to_cart"",
                            ""uri"": ""https://www.arrow.com/en/products/lm358dt/stmicroelectronics?region=nac""
                          }
                        ],
                        ""inStock"": false,
                        ""mfrLeadTime"": 38,
                        ""tariffFlag"": false,
                        ""shipsFrom"": ""United States of America"",
                        ""shipsIn"": ""Ships today"",
                        ""arrowLeadTime"": """",
                        ""isNcnr"": false,
                        ""isNpi"": false,
                        ""productCode"": ""LM358DT"",
                        ""eccnCode"": ""EAR99"",
                        ""htsCode"": ""8542.33.00.01"",
                        ""containerType"": """"
                      }
                    ]
                  }
                ]
              }
            ]
          },
          ""hasDatasheet"": true,
          ""categoryName"": ""Operational Amplifiers - Op Amps"",
          ""status"": ""Active""
        },
        {
          ""itemId"": 2054273,
          ""partNum"": ""LM358PT"",
          ""manufacturer"": {
            ""mfrCd"": ""STMICRO"",
            ""mfrName"": ""STMicroelectronics""
          },
          ""desc"": ""Op Amp Dual Low Power Amplifier ±15V/30V 8-Pin TSSOP T/R"",
          ""packageType"": """",
          ""resources"": [
            {
              ""type"": ""datasheet"",
              ""uri"": ""https://static6.arrow.com/aropdfconversion/a92fa424e858bb82ca0c0d7bf3ec5dcc00ea0f1/2717545374532467cd00000464.pdf""
            },
            {
              ""type"": ""image_small"",
              ""uri"": ""https://static6.arrow.com/aropdfconversion/arrowimages/15d5ce10d2078465dd89ad33dfce4e7de05d27e4/8tssop.jpg""
            },
            {
              ""type"": ""image_large"",
              ""uri"": ""https://static6.arrow.com/aropdfconversion/arrowimages/da6fb0490db0c66d118ec26331ce4fc2870abdb3/8tssop.jpg""
            },
            {
              ""type"": ""cloud_part_detail"",
              ""uri"": ""https://www.arrow.com/en/products/lm358pt/stmicroelectronics""
            },
            {
              ""type"": ""api_part_detail"",
              ""uri"": ""http://api.arrow.com/itemservice/v2/en/detail?mfrCd=STMICRO&partNum=LM358PT""
            },
            {
              ""type"": ""part_detail"",
              ""uri"": ""http://api.arrow.com/itemservice/v2/en/detail?pkey=w-B2h1-pWQMcngKVK54S3H7MIkH37hnN0DpIo56UVqA""
            }
          ],
          ""EnvData"": {
            ""compliance"": [
              {
                ""displayLabel"": ""eurohs"",
                ""displayValue"": ""Compliant""
              },
              {
                ""displayLabel"": ""cnrohs"",
                ""displayValue"": ""Compliant""
              }
            ]
          },
          ""InvOrg"": {
            ""webSites"": [
              {
                ""code"": ""Verical.com"",
                ""name"": ""Verical.com"",
                ""sources"": [
                  {
                    ""currency"": ""USD"",
                    ""sourceCd"": ""VERICAL"",
                    ""displayName"": ""Verical"",
                    ""sourceParts"": [
                      {
                        ""packSize"": 4000,
                        ""minimumOrderQuantity"": 4000,
                        ""sourcePartId"": ""66596938"",
                        ""Prices"": {
                          ""resaleList"": [
                            {
                              ""displayPrice"": ""0.1071"",
                              ""price"": 0.1071,
                              ""minQty"": 4000,
                              ""maxQty"": 999999999
                            }
                          ]
                        },
                        ""Availability"": [
                          {
                            ""fohQty"": 16000,
                            ""availabilityCd"": ""INSTK"",
                            ""availabilityMessage"": ""In Stock"",
                            ""pipeline"": []
                          }
                        ],
                        ""customerSpecificPricing"": [],
                        ""customerSpecificInventory"": [],
                        ""dateCode"": ""2306"",
                        ""resources"": [
                          {
                            ""type"": ""detail"",
                            ""uri"": ""https://www.verical.com/pd/stmicroelectronics-amplifier---operational-LM358PT-202892""
                          },
                          {
                            ""type"": ""add_to_cart"",
                            ""uri"": ""https://www.verical.com/pd/stmicroelectronics-amplifier---operational-LM358PT-202892""
                          }
                        ],
                        ""inStock"": true,
                        ""mfrLeadTime"": 0,
                        ""tariffFlag"": false,
                        ""arrowLeadTime"": """",
                        ""isNcnr"": false,
                        ""isNpi"": false,
                        ""productCode"": ""LM358PT"",
                        ""eccnCode"": ""EAR99"",
                        ""htsCode"": ""8542.33.00.01"",
                        ""countryOfOrigin"": ""CN"",
                        ""containerType"": """"
                      },
                      {
                        ""packSize"": 4000,
                        ""minimumOrderQuantity"": 4000,
                        ""sourcePartId"": ""65030211"",
                        ""Prices"": {
                          ""resaleList"": [
                            {
                              ""displayPrice"": ""0.1951"",
                              ""price"": 0.1951,
                              ""minQty"": 4000,
                              ""maxQty"": 11999
                            },
                            {
                              ""displayPrice"": ""0.165"",
                              ""price"": 0.165,
                              ""minQty"": 12000,
                              ""maxQty"": 19999
                            },
                            {
                              ""displayPrice"": ""0.1633"",
                              ""price"": 0.1633,
                              ""minQty"": 20000,
                              ""maxQty"": 23999
                            },
                            {
                              ""displayPrice"": ""0.1617"",
                              ""price"": 0.1617,
                              ""minQty"": 24000,
                              ""maxQty"": 39999
                            },
                            {
                              ""displayPrice"": ""0.16"",
                              ""price"": 0.16,
                              ""minQty"": 40000,
                              ""maxQty"": 99999
                            },
                            {
                              ""displayPrice"": ""0.1583"",
                              ""price"": 0.1583,
                              ""minQty"": 100000,
                              ""maxQty"": 999999999
                            }
                          ]
                        },
                        ""Availability"": [
                          {
                            ""fohQty"": 4000,
                            ""availabilityCd"": ""INSTK"",
                            ""availabilityMessage"": ""In Stock"",
                            ""pipeline"": []
                          }
                        ],
                        ""customerSpecificPricing"": [],
                        ""customerSpecificInventory"": [],
                        ""dateCode"": ""2237"",
                        ""resources"": [
                          {
                            ""type"": ""detail"",
                            ""uri"": ""https://www.verical.com/pd/stmicroelectronics-amplifier---operational-LM358PT-202892""
                          },
                          {
                            ""type"": ""add_to_cart"",
                            ""uri"": ""https://www.verical.com/pd/stmicroelectronics-amplifier---operational-LM358PT-202892""
                          }
                        ],
                        ""inStock"": true,
                        ""mfrLeadTime"": 0,
                        ""tariffFlag"": false,
                        ""arrowLeadTime"": """",
                        ""isNcnr"": false,
                        ""isNpi"": false,
                        ""productCode"": ""LM358PT"",
                        ""eccnCode"": ""EAR99"",
                        ""htsCode"": ""8542.33.00.01"",
                        ""countryOfOrigin"": ""CN"",
                        ""containerType"": """"
                      },
                      {
                        ""packSize"": 1,
                        ""minimumOrderQuantity"": 65,
                        ""sourcePartId"": ""63405791"",
                        ""Prices"": {
                          ""resaleList"": [
                            {
                              ""displayPrice"": ""0.1947"",
                              ""price"": 0.1947,
                              ""minQty"": 65,
                              ""maxQty"": 99
                            },
                            {
                              ""displayPrice"": ""0.1155"",
                              ""price"": 0.1155,
                              ""minQty"": 100,
                              ""maxQty"": 249
                            },
                            {
                              ""displayPrice"": ""0.1143"",
                              ""price"": 0.1143,
                              ""minQty"": 250,
                              ""maxQty"": 499
                            },
                            {
                              ""displayPrice"": ""0.1132"",
                              ""price"": 0.1132,
                              ""minQty"": 500,
                              ""maxQty"": 999
                            },
                            {
                              ""displayPrice"": ""0.112"",
                              ""price"": 0.112,
                              ""minQty"": 1000,
                              ""maxQty"": 2999
                            },
                            {
                              ""displayPrice"": ""0.1109"",
                              ""price"": 0.1109,
                              ""minQty"": 3000,
                              ""maxQty"": 5999
                            },
                            {
                              ""displayPrice"": ""0.1098"",
                              ""price"": 0.1098,
                              ""minQty"": 6000,
                              ""maxQty"": 999999999
                            }
                          ]
                        },
                        ""Availability"": [
                          {
                            ""fohQty"": 7988,
                            ""availabilityCd"": ""INSTK"",
                            ""availabilityMessage"": ""In Stock"",
                            ""pipeline"": []
                          }
                        ],
                        ""customerSpecificPricing"": [],
                        ""customerSpecificInventory"": [],
                        ""dateCode"": ""2235"",
                        ""resources"": [
                          {
                            ""type"": ""detail"",
                            ""uri"": ""https://www.verical.com/pd/stmicroelectronics-amplifier---operational-LM358PT-202892""
                          },
                          {
                            ""type"": ""add_to_cart"",
                            ""uri"": ""https://www.verical.com/pd/stmicroelectronics-amplifier---operational-LM358PT-202892""
                          }
                        ],
                        ""inStock"": true,
                        ""mfrLeadTime"": 0,
                        ""tariffFlag"": false,
                        ""arrowLeadTime"": """",
                        ""isNcnr"": false,
                        ""isNpi"": false,
                        ""productCode"": ""LM358PT"",
                        ""eccnCode"": ""EAR99"",
                        ""htsCode"": ""8542.33.00.01"",
                        ""countryOfOrigin"": ""US"",
                        ""containerType"": """"
                      }
                    ]
                  }
                ]
              },
              {
                ""code"": ""arrow.com"",
                ""name"": ""arrow.com"",
                ""sources"": [
                  {
                    ""currency"": ""USD"",
                    ""sourceCd"": ""EUROPE"",
                    ""displayName"": ""Europe"",
                    ""sourceParts"": [
                      {
                        ""packSize"": 4000,
                        ""minimumOrderQuantity"": 4000,
                        ""sourcePartId"": ""E02:0323_00211282"",
                        ""Prices"": {
                          ""resaleList"": [
                            {
                              ""displayPrice"": ""0.1992"",
                              ""price"": 0.1992,
                              ""minQty"": 4000,
                              ""maxQty"": 99999999
                            }
                          ]
                        },
                        ""Availability"": [
                          {
                            ""fohQty"": 16000,
                            ""availabilityCd"": ""INSTKEU"",
                            ""availabilityMessage"": ""In Stock"",
                            ""pipeline"": [
                              {
                                ""delivery"": ""03-NOV-2023"",
                                ""quantity"": 4000
                              }
                            ]
                          }
                        ],
                        ""customerSpecificPricing"": [],
                        ""customerSpecificInventory"": [],
                        ""dateCode"": ""2306"",
                        ""resources"": [
                          {
                            ""type"": ""detail"",
                            ""uri"": ""https://www.arrow.com/en/products/lm358pt/stmicroelectronics?region=europe""
                          },
                          {
                            ""type"": ""add_to_cart"",
                            ""uri"": ""https://www.arrow.com/en/products/lm358pt/stmicroelectronics?region=europe""
                          }
                        ],
                        ""inStock"": true,
                        ""mfrLeadTime"": 36,
                        ""tariffFlag"": false,
                        ""shipsFrom"": ""Netherlands"",
                        ""shipsIn"": ""Ships in 2 days "",
                        ""arrowLeadTime"": """",
                        ""isNcnr"": false,
                        ""isNpi"": false,
                        ""productCode"": ""LM358PT"",
                        ""eccnCode"": ""EAR99"",
                        ""htsCode"": ""8542.33.00.01"",
                        ""countryOfOrigin"": ""CN"",
                        ""containerType"": """"
                      }
                    ]
                  },
                  {
                    ""currency"": ""USD"",
                    ""sourceCd"": ""ACNA"",
                    ""displayName"": ""Arrow Americas"",
                    ""sourceParts"": [
                      {
                        ""packSize"": 4000,
                        ""minimumOrderQuantity"": 8000,
                        ""sourcePartId"": ""V36:1790_06548550"",
                        ""Prices"": {
                          ""resaleList"": [
                            {
                              ""displayPrice"": ""0.1951"",
                              ""price"": 0.1951,
                              ""minQty"": 8000,
                              ""maxQty"": 11999
                            },
                            {
                              ""displayPrice"": ""0.1932"",
                              ""price"": 0.1932,
                              ""minQty"": 12000,
                              ""maxQty"": 19999
                            },
                            {
                              ""displayPrice"": ""0.1913"",
                              ""price"": 0.1913,
                              ""minQty"": 20000,
                              ""maxQty"": 23999
                            },
                            {
                              ""displayPrice"": ""0.1893"",
                              ""price"": 0.1893,
                              ""minQty"": 24000,
                              ""maxQty"": 39999
                            },
                            {
                              ""displayPrice"": ""0.1875"",
                              ""price"": 0.1875,
                              ""minQty"": 40000,
                              ""maxQty"": 99999
                            },
                            {
                              ""displayPrice"": ""0.1856"",
                              ""price"": 0.1856,
                              ""minQty"": 100000,
                              ""maxQty"": 99999999
                            }
                          ]
                        },
                        ""Availability"": [
                          {
                            ""fohQty"": 4000,
                            ""availabilityCd"": ""INSTK"",
                            ""availabilityMessage"": ""In Stock"",
                            ""pipeline"": []
                          }
                        ],
                        ""customerSpecificPricing"": [],
                        ""customerSpecificInventory"": [],
                        ""dateCode"": ""2237"",
                        ""resources"": [
                          {
                            ""type"": ""detail"",
                            ""uri"": ""https://www.arrow.com/en/products/lm358pt/stmicroelectronics?region=nac""
                          },
                          {
                            ""type"": ""add_to_cart"",
                            ""uri"": ""https://www.arrow.com/en/products/lm358pt/stmicroelectronics?region=nac""
                          }
                        ],
                        ""inStock"": true,
                        ""mfrLeadTime"": 36,
                        ""tariffFlag"": true,
                        ""shipsFrom"": ""United States of America"",
                        ""shipsIn"": ""Ships today"",
                        ""arrowLeadTime"": """",
                        ""isNcnr"": false,
                        ""isNpi"": false,
                        ""productCode"": ""LM358PT"",
                        ""eccnCode"": ""EAR99"",
                        ""htsCode"": ""8542.33.00.01"",
                        ""countryOfOrigin"": ""CN"",
                        ""containerType"": """"
                      },
                      {
                        ""packSize"": 1,
                        ""minimumOrderQuantity"": 1,
                        ""sourcePartId"": ""V72:2272_06548550"",
                        ""Prices"": {
                          ""resaleList"": [
                            {
                              ""displayPrice"": ""0.4896"",
                              ""price"": 0.4896,
                              ""minQty"": 1,
                              ""maxQty"": 9
                            },
                            {
                              ""displayPrice"": ""0.3744"",
                              ""price"": 0.3744,
                              ""minQty"": 10,
                              ""maxQty"": 24
                            },
                            {
                              ""displayPrice"": ""0.1947"",
                              ""price"": 0.1947,
                              ""minQty"": 25,
                              ""maxQty"": 99
                            },
                            {
                              ""displayPrice"": ""0.1108"",
                              ""price"": 0.1108,
                              ""minQty"": 100,
                              ""maxQty"": 249
                            },
                            {
                              ""displayPrice"": ""0.1097"",
                              ""price"": 0.1097,
                              ""minQty"": 250,
                              ""maxQty"": 499
                            },
                            {
                              ""displayPrice"": ""0.1086"",
                              ""price"": 0.1086,
                              ""minQty"": 500,
                              ""maxQty"": 999
                            },
                            {
                              ""displayPrice"": ""0.1075"",
                              ""price"": 0.1075,
                              ""minQty"": 1000,
                              ""maxQty"": 2999
                            },
                            {
                              ""displayPrice"": ""0.1065"",
                              ""price"": 0.1065,
                              ""minQty"": 3000,
                              ""maxQty"": 5999
                            },
                            {
                              ""displayPrice"": ""0.1054"",
                              ""price"": 0.1054,
                              ""minQty"": 6000,
                              ""maxQty"": 999999999
                            }
                          ]
                        },
                        ""Availability"": [
                          {
                            ""fohQty"": 7988,
                            ""availabilityCd"": ""CSTRP"",
                            ""availabilityMessage"": ""Cut Strips"",
                            ""pipeline"": []
                          }
                        ],
                        ""customerSpecificPricing"": [],
                        ""customerSpecificInventory"": [],
                        ""dateCode"": ""2235"",
                        ""resources"": [
                          {
                            ""type"": ""detail"",
                            ""uri"": ""https://www.arrow.com/en/products/lm358pt/stmicroelectronics?region=nac""
                          },
                          {
                            ""type"": ""add_to_cart"",
                            ""uri"": ""https://www.arrow.com/en/products/lm358pt/stmicroelectronics?region=nac""
                          }
                        ],
                        ""inStock"": true,
                        ""mfrLeadTime"": 36,
                        ""tariffFlag"": false,
                        ""shipsFrom"": ""United States of America"",
                        ""shipsIn"": ""Ships today"",
                        ""arrowLeadTime"": """",
                        ""isNcnr"": false,
                        ""isNpi"": false,
                        ""productCode"": ""LM358PT"",
                        ""eccnCode"": ""EAR99"",
                        ""htsCode"": ""8542.33.00.01"",
                        ""countryOfOrigin"": ""US"",
                        ""containerType"": ""Cut Strips""
                      },
                      {
                        ""packSize"": 1,
                        ""minimumOrderQuantity"": 8000,
                        ""sourcePartId"": ""V79:2366_17776785"",
                        ""Availability"": [
                          {
                            ""fohQty"": 0,
                            ""availabilityCd"": ""QUOTE"",
                            ""availabilityMessage"": ""Call For Quote"",
                            ""pipeline"": []
                          }
                        ],
                        ""customerSpecificPricing"": [],
                        ""customerSpecificInventory"": [],
                        ""dateCode"": """",
                        ""resources"": [
                          {
                            ""type"": ""detail"",
                            ""uri"": ""https://www.arrow.com/en/products/lm358pt/stmicroelectronics?region=nac""
                          },
                          {
                            ""type"": ""add_to_cart"",
                            ""uri"": ""https://www.arrow.com/en/products/lm358pt/stmicroelectronics?region=nac""
                          }
                        ],
                        ""inStock"": false,
                        ""mfrLeadTime"": 99,
                        ""tariffFlag"": false,
                        ""shipsFrom"": ""United States of America"",
                        ""shipsIn"": ""Ships today"",
                        ""arrowLeadTime"": """",
                        ""isNcnr"": false,
                        ""isNpi"": false,
                        ""productCode"": ""LM358PT"",
                        ""eccnCode"": ""EAR99"",
                        ""htsCode"": ""8542.33.00.01"",
                        ""containerType"": """"
                      }
                    ]
                  }
                ]
              }
            ]
          },
          ""hasDatasheet"": true,
          ""categoryName"": ""Operational Amplifiers - Op Amps"",
          ""status"": ""Active""
        },
        {
          ""itemId"": 1629656,
          ""partNum"": ""LM358ADT"",
          ""manufacturer"": {
            ""mfrCd"": ""STMICRO"",
            ""mfrName"": ""STMicroelectronics""
          },
          ""desc"": ""Op Amp Dual Low Power Amplifier ±15V/30V 8-Pin SO N T/R"",
          ""packageType"": """",
          ""resources"": [
            {
              ""type"": ""datasheet"",
              ""uri"": ""https://static6.arrow.com/aropdfconversion/a92fa424e858bb82ca0c0d7bf3ec5dcc00ea0f1/2717545374532467cd00000464.pdf""
            },
            {
              ""type"": ""image_small"",
              ""uri"": ""https://static6.arrow.com/aropdfconversion/arrowimages/d6443add18dd1ef15bdc646129513969596b9c5/l6902d013tr.jpg""
            },
            {
              ""type"": ""image_large"",
              ""uri"": ""https://static6.arrow.com/aropdfconversion/arrowimages/7b58dbb14a78f7cd43be974b11faa4b8806e654/l6902d013tr.jpg""
            },
            {
              ""type"": ""cloud_part_detail"",
              ""uri"": ""https://www.arrow.com/en/products/lm358adt/stmicroelectronics""
            },
            {
              ""type"": ""api_part_detail"",
              ""uri"": ""http://api.arrow.com/itemservice/v2/en/detail?mfrCd=STMICRO&partNum=LM358ADT""
            },
            {
              ""type"": ""part_detail"",
              ""uri"": ""http://api.arrow.com/itemservice/v2/en/detail?pkey=2IcAzgRy9ge0_fK0tnEBYcYl9A7IdjPy6DUPDSVNlUw""
            }
          ],
          ""EnvData"": {
            ""compliance"": [
              {
                ""displayLabel"": ""eurohs"",
                ""displayValue"": ""Compliant""
              },
              {
                ""displayLabel"": ""cnrohs"",
                ""displayValue"": ""Compliant""
              }
            ]
          },
          ""InvOrg"": {
            ""webSites"": [
              {
                ""code"": ""Verical.com"",
                ""name"": ""Verical.com"",
                ""sources"": [
                  {
                    ""currency"": ""USD"",
                    ""sourceCd"": ""VERICAL"",
                    ""displayName"": ""Verical"",
                    ""sourceParts"": [
                      {
                        ""packSize"": 2500,
                        ""minimumOrderQuantity"": 2500,
                        ""sourcePartId"": ""66451204"",
                        ""Prices"": {
                          ""resaleList"": [
                            {
                              ""displayPrice"": ""0.0735"",
                              ""price"": 0.0735,
                              ""minQty"": 2500,
                              ""maxQty"": 999999999
                            }
                          ]
                        },
                        ""Availability"": [
                          {
                            ""fohQty"": 77500,
                            ""availabilityCd"": ""INSTK"",
                            ""availabilityMessage"": ""In Stock"",
                            ""pipeline"": []
                          }
                        ],
                        ""customerSpecificPricing"": [],
                        ""customerSpecificInventory"": [],
                        ""dateCode"": ""2304"",
                        ""resources"": [
                          {
                            ""type"": ""detail"",
                            ""uri"": ""https://www.verical.com/pd/stmicroelectronics-amplifier---operational-LM358ADT-94395""
                          },
                          {
                            ""type"": ""add_to_cart"",
                            ""uri"": ""https://www.verical.com/pd/stmicroelectronics-amplifier---operational-LM358ADT-94395""
                          }
                        ],
                        ""inStock"": true,
                        ""mfrLeadTime"": 0,
                        ""tariffFlag"": false,
                        ""arrowLeadTime"": """",
                        ""isNcnr"": false,
                        ""isNpi"": false,
                        ""productCode"": ""LM358ADT"",
                        ""eccnCode"": ""EAR99"",
                        ""htsCode"": ""8542.33.00.01"",
                        ""countryOfOrigin"": ""MA"",
                        ""containerType"": """"
                      }
                    ]
                  }
                ]
              },
              {
                ""code"": ""arrow.com"",
                ""name"": ""arrow.com"",
                ""sources"": [
                  {
                    ""currency"": ""USD"",
                    ""sourceCd"": ""EUROPE"",
                    ""displayName"": ""Europe"",
                    ""sourceParts"": [
                      {
                        ""packSize"": 2500,
                        ""minimumOrderQuantity"": 2500,
                        ""sourcePartId"": ""E02:0323_00035448"",
                        ""Prices"": {
                          ""resaleList"": [
                            {
                              ""displayPrice"": ""0.1445"",
                              ""price"": 0.1445,
                              ""minQty"": 2500,
                              ""maxQty"": 4999
                            },
                            {
                              ""displayPrice"": ""0.1394"",
                              ""price"": 0.1394,
                              ""minQty"": 5000,
                              ""maxQty"": 99999999
                            }
                          ]
                        },
                        ""Availability"": [
                          {
                            ""fohQty"": 77500,
                            ""availabilityCd"": ""INSTKEU"",
                            ""availabilityMessage"": ""In Stock"",
                            ""pipeline"": [
                              {
                                ""delivery"": ""24-JUL-2023"",
                                ""quantity"": 7500
                              }
                            ]
                          }
                        ],
                        ""customerSpecificPricing"": [],
                        ""customerSpecificInventory"": [],
                        ""dateCode"": ""2304"",
                        ""resources"": [
                          {
                            ""type"": ""detail"",
                            ""uri"": ""https://www.arrow.com/en/products/lm358adt/stmicroelectronics?region=europe""
                          },
                          {
                            ""type"": ""add_to_cart"",
                            ""uri"": ""https://www.arrow.com/en/products/lm358adt/stmicroelectronics?region=europe""
                          }
                        ],
                        ""inStock"": true,
                        ""mfrLeadTime"": 36,
                        ""tariffFlag"": false,
                        ""shipsFrom"": ""Netherlands"",
                        ""shipsIn"": ""Ships in 2 days "",
                        ""arrowLeadTime"": """",
                        ""isNcnr"": false,
                        ""isNpi"": false,
                        ""productCode"": ""LM358ADT"",
                        ""eccnCode"": ""EAR99"",
                        ""htsCode"": ""8542.33.00.01"",
                        ""countryOfOrigin"": ""MA"",
                        ""containerType"": """"
                      }
                    ]
                  },
                  {
                    ""currency"": ""USD"",
                    ""sourceCd"": ""ACNA"",
                    ""displayName"": ""Arrow Americas"",
                    ""sourceParts"": [
                      {
                        ""packSize"": 1,
                        ""minimumOrderQuantity"": 5000,
                        ""sourcePartId"": ""V72:2272_06548541"",
                        ""Availability"": [
                          {
                            ""fohQty"": 0,
                            ""availabilityCd"": ""QUOTE"",
                            ""availabilityMessage"": ""Call For Quote"",
                            ""pipeline"": []
                          }
                        ],
                        ""customerSpecificPricing"": [],
                        ""customerSpecificInventory"": [],
                        ""dateCode"": """",
                        ""resources"": [
                          {
                            ""type"": ""detail"",
                            ""uri"": ""https://www.arrow.com/en/products/lm358adt/stmicroelectronics?region=nac""
                          },
                          {
                            ""type"": ""add_to_cart"",
                            ""uri"": ""https://www.arrow.com/en/products/lm358adt/stmicroelectronics?region=nac""
                          }
                        ],
                        ""inStock"": false,
                        ""mfrLeadTime"": 36,
                        ""tariffFlag"": false,
                        ""shipsFrom"": ""United States of America"",
                        ""shipsIn"": ""Ships today"",
                        ""arrowLeadTime"": """",
                        ""isNcnr"": false,
                        ""isNpi"": false,
                        ""productCode"": ""LM358ADT"",
                        ""eccnCode"": ""EAR99"",
                        ""htsCode"": ""8542.33.00.01"",
                        ""containerType"": """"
                      },
                      {
                        ""packSize"": 2500,
                        ""minimumOrderQuantity"": 5000,
                        ""sourcePartId"": ""V36:1790_06548541"",
                        ""Prices"": {
                          ""resaleList"": [
                            {
                              ""displayPrice"": ""0.1429"",
                              ""price"": 0.1429,
                              ""minQty"": 5000,
                              ""maxQty"": 99999999
                            }
                          ]
                        },
                        ""Availability"": [
                          {
                            ""fohQty"": 0,
                            ""availabilityCd"": ""QUOTE"",
                            ""availabilityMessage"": ""Call For Quote"",
                            ""pipeline"": [
                              {
                                ""delivery"": ""14-NOV-2023"",
                                ""quantity"": 10000
                              }
                            ]
                          }
                        ],
                        ""customerSpecificPricing"": [],
                        ""customerSpecificInventory"": [],
                        ""dateCode"": ""2232"",
                        ""resources"": [
                          {
                            ""type"": ""detail"",
                            ""uri"": ""https://www.arrow.com/en/products/lm358adt/stmicroelectronics?region=nac""
                          },
                          {
                            ""type"": ""add_to_cart"",
                            ""uri"": ""https://www.arrow.com/en/products/lm358adt/stmicroelectronics?region=nac""
                          }
                        ],
                        ""inStock"": false,
                        ""mfrLeadTime"": 36,
                        ""tariffFlag"": false,
                        ""shipsFrom"": ""United States of America"",
                        ""shipsIn"": ""Ships today"",
                        ""arrowLeadTime"": """",
                        ""isNcnr"": false,
                        ""isNpi"": false,
                        ""productCode"": ""LM358ADT"",
                        ""eccnCode"": ""EAR99"",
                        ""htsCode"": ""8542.33.00.01"",
                        ""countryOfOrigin"": ""MA"",
                        ""containerType"": """"
                      }
                    ]
                  }
                ]
              }
            ]
          },
          ""hasDatasheet"": true,
          ""categoryName"": ""Operational Amplifiers - Op Amps"",
          ""status"": ""Active""
        },
        {
          ""itemId"": 1687769,
          ""partNum"": ""LM358QT"",
          ""manufacturer"": {
            ""mfrCd"": ""STMICRO"",
            ""mfrName"": ""STMicroelectronics""
          },
          ""desc"": ""Op Amp Dual Low Power Amplifier ±15V/30V 8-Pin DFN EP T/R"",
          ""packageType"": """",
          ""resources"": [
            {
              ""type"": ""datasheet"",
              ""uri"": ""https://static6.arrow.com/aropdfconversion/a92fa424e858bb82ca0c0d7bf3ec5dcc00ea0f1/2717545374532467cd00000464.pdf""
            },
            {
              ""type"": ""image_small"",
              ""uri"": ""https://static6.arrow.com/aropdfconversion/arrowimages/309dd2bde4997e82fe61abac3aa31a876c69895c/son127p500x600x80-9t310x410_cyp_n.step.jpg""
            },
            {
              ""type"": ""image_large"",
              ""uri"": ""https://static6.arrow.com/aropdfconversion/arrowimages/3674980a8ae0b06e6c976065f0fd4c27d9bf67f5/son127p500x600x80-9t310x410_cyp_n.step.jpg""
            },
            {
              ""type"": ""cloud_part_detail"",
              ""uri"": ""https://www.arrow.com/en/products/lm358qt/stmicroelectronics""
            },
            {
              ""type"": ""api_part_detail"",
              ""uri"": ""http://api.arrow.com/itemservice/v2/en/detail?mfrCd=STMICRO&partNum=LM358QT""
            },
            {
              ""type"": ""part_detail"",
              ""uri"": ""http://api.arrow.com/itemservice/v2/en/detail?pkey=Kxi3HZBgz_d1smkQ-D3_-cjPavwFNB3f4mWz7VPA0CY""
            }
          ],
          ""EnvData"": {
            ""compliance"": [
              {
                ""displayLabel"": ""eurohs"",
                ""displayValue"": ""Compliant""
              },
              {
                ""displayLabel"": ""cnrohs"",
                ""displayValue"": ""Compliant""
              }
            ]
          },
          ""InvOrg"": {
            ""webSites"": [
              {
                ""code"": ""Verical.com"",
                ""name"": ""Verical.com"",
                ""sources"": [
                  {
                    ""currency"": ""USD"",
                    ""sourceCd"": ""VERICAL"",
                    ""displayName"": ""Verical"",
                    ""sourceParts"": [
                      {
                        ""packSize"": 3000,
                        ""minimumOrderQuantity"": 3000,
                        ""sourcePartId"": ""66198465"",
                        ""Prices"": {
                          ""resaleList"": [
                            {
                              ""displayPrice"": ""0.1736"",
                              ""price"": 0.1736,
                              ""minQty"": 3000,
                              ""maxQty"": 99999999
                            }
                          ]
                        },
                        ""Availability"": [
                          {
                            ""fohQty"": 18000,
                            ""availabilityCd"": ""INSTK"",
                            ""availabilityMessage"": ""In Stock"",
                            ""pipeline"": []
                          }
                        ],
                        ""customerSpecificPricing"": [],
                        ""customerSpecificInventory"": [],
                        ""dateCode"": ""2301"",
                        ""resources"": [
                          {
                            ""type"": ""detail"",
                            ""uri"": ""https://www.verical.com/pd/stmicroelectronics-amplifier---operational-LM358QT-563544""
                          },
                          {
                            ""type"": ""add_to_cart"",
                            ""uri"": ""https://www.verical.com/pd/stmicroelectronics-amplifier---operational-LM358QT-563544""
                          }
                        ],
                        ""inStock"": true,
                        ""mfrLeadTime"": 0,
                        ""tariffFlag"": false,
                        ""arrowLeadTime"": """",
                        ""isNcnr"": false,
                        ""isNpi"": false,
                        ""productCode"": ""LM358QT"",
                        ""eccnCode"": ""EAR99"",
                        ""htsCode"": ""8542.33.00.01"",
                        ""countryOfOrigin"": ""CN"",
                        ""containerType"": """"
                      }
                    ]
                  }
                ]
              },
              {
                ""code"": ""arrow.com"",
                ""name"": ""arrow.com"",
                ""sources"": [
                  {
                    ""currency"": ""USD"",
                    ""sourceCd"": ""EUROPE"",
                    ""displayName"": ""Europe"",
                    ""sourceParts"": [
                      {
                        ""packSize"": 3000,
                        ""minimumOrderQuantity"": 3000,
                        ""sourcePartId"": ""E02:0323_04182880"",
                        ""Prices"": {
                          ""resaleList"": [
                            {
                              ""displayPrice"": ""0.1909"",
                              ""price"": 0.1909,
                              ""minQty"": 3000,
                              ""maxQty"": 5999
                            },
                            {
                              ""displayPrice"": ""0.1787"",
                              ""price"": 0.1787,
                              ""minQty"": 6000,
                              ""maxQty"": 14999
                            },
                            {
                              ""displayPrice"": ""0.1744"",
                              ""price"": 0.1744,
                              ""minQty"": 15000,
                              ""maxQty"": 999999999
                            }
                          ]
                        },
                        ""Availability"": [
                          {
                            ""fohQty"": 24000,
                            ""availabilityCd"": ""INSTKEU"",
                            ""availabilityMessage"": ""In Stock"",
                            ""pipeline"": [
                              {
                                ""delivery"": ""06-APR-2023"",
                                ""quantity"": 6000
                              }
                            ]
                          }
                        ],
                        ""customerSpecificPricing"": [],
                        ""customerSpecificInventory"": [],
                        ""dateCode"": ""2253"",
                        ""resources"": [
                          {
                            ""type"": ""detail"",
                            ""uri"": ""https://www.arrow.com/en/products/lm358qt/stmicroelectronics?region=europe""
                          },
                          {
                            ""type"": ""add_to_cart"",
                            ""uri"": ""https://www.arrow.com/en/products/lm358qt/stmicroelectronics?region=europe""
                          }
                        ],
                        ""inStock"": true,
                        ""mfrLeadTime"": 53,
                        ""tariffFlag"": false,
                        ""shipsFrom"": ""Netherlands"",
                        ""shipsIn"": ""Ships in 2 days "",
                        ""arrowLeadTime"": """",
                        ""isNcnr"": false,
                        ""isNpi"": false,
                        ""productCode"": ""LM358QT"",
                        ""eccnCode"": ""EAR99"",
                        ""htsCode"": ""8542.33.00.01"",
                        ""countryOfOrigin"": ""CN"",
                        ""containerType"": """"
                      }
                    ]
                  },
                  {
                    ""currency"": ""USD"",
                    ""sourceCd"": ""ACNA"",
                    ""displayName"": ""Arrow Americas"",
                    ""sourceParts"": [
                      {
                        ""packSize"": 1,
                        ""minimumOrderQuantity"": 6000,
                        ""sourcePartId"": ""V72:2272_06548554"",
                        ""Availability"": [
                          {
                            ""fohQty"": 0,
                            ""availabilityCd"": ""QUOTE"",
                            ""availabilityMessage"": ""Call For Quote"",
                            ""pipeline"": []
                          }
                        ],
                        ""customerSpecificPricing"": [],
                        ""customerSpecificInventory"": [],
                        ""dateCode"": """",
                        ""resources"": [
                          {
                            ""type"": ""detail"",
                            ""uri"": ""https://www.arrow.com/en/products/lm358qt/stmicroelectronics?region=nac""
                          },
                          {
                            ""type"": ""add_to_cart"",
                            ""uri"": ""https://www.arrow.com/en/products/lm358qt/stmicroelectronics?region=nac""
                          }
                        ],
                        ""inStock"": false,
                        ""mfrLeadTime"": 53,
                        ""tariffFlag"": false,
                        ""shipsFrom"": ""United States of America"",
                        ""shipsIn"": ""Ships today"",
                        ""arrowLeadTime"": """",
                        ""isNcnr"": false,
                        ""isNpi"": false,
                        ""productCode"": ""LM358QT"",
                        ""eccnCode"": ""EAR99"",
                        ""htsCode"": ""8542.33.00.01"",
                        ""containerType"": """"
                      },
                      {
                        ""packSize"": 3000,
                        ""minimumOrderQuantity"": 6000,
                        ""sourcePartId"": ""V36:1790_06548554"",
                        ""Prices"": {
                          ""resaleList"": [
                            {
                              ""displayPrice"": ""0.1429"",
                              ""price"": 0.1429,
                              ""minQty"": 6000,
                              ""maxQty"": 8999
                            },
                            {
                              ""displayPrice"": ""0.1415"",
                              ""price"": 0.1415,
                              ""minQty"": 9000,
                              ""maxQty"": 11999
                            },
                            {
                              ""displayPrice"": ""0.1401"",
                              ""price"": 0.1401,
                              ""minQty"": 12000,
                              ""maxQty"": 14999
                            },
                            {
                              ""displayPrice"": ""0.1387"",
                              ""price"": 0.1387,
                              ""minQty"": 15000,
                              ""maxQty"": 23999
                            },
                            {
                              ""displayPrice"": ""0.1373"",
                              ""price"": 0.1373,
                              ""minQty"": 24000,
                              ""maxQty"": 29999
                            },
                            {
                              ""displayPrice"": ""0.1359"",
                              ""price"": 0.1359,
                              ""minQty"": 30000,
                              ""maxQty"": 44999
                            },
                            {
                              ""displayPrice"": ""0.1346"",
                              ""price"": 0.1346,
                              ""minQty"": 45000,
                              ""maxQty"": 47999
                            },
                            {
                              ""displayPrice"": ""0.1332"",
                              ""price"": 0.1332,
                              ""minQty"": 48000,
                              ""maxQty"": 74999
                            },
                            {
                              ""displayPrice"": ""0.1319"",
                              ""price"": 0.1319,
                              ""minQty"": 75000,
                              ""maxQty"": 99999999
                            }
                          ]
                        },
                        ""Availability"": [
                          {
                            ""fohQty"": 0,
                            ""availabilityCd"": ""QUOTE"",
                            ""availabilityMessage"": ""Call For Quote"",
                            ""pipeline"": []
                          }
                        ],
                        ""customerSpecificPricing"": [],
                        ""customerSpecificInventory"": [],
                        ""dateCode"": """",
                        ""resources"": [
                          {
                            ""type"": ""detail"",
                            ""uri"": ""https://www.arrow.com/en/products/lm358qt/stmicroelectronics?region=nac""
                          },
                          {
                            ""type"": ""add_to_cart"",
                            ""uri"": ""https://www.arrow.com/en/products/lm358qt/stmicroelectronics?region=nac""
                          }
                        ],
                        ""inStock"": false,
                        ""mfrLeadTime"": 53,
                        ""tariffFlag"": false,
                        ""shipsFrom"": ""United States of America"",
                        ""shipsIn"": ""Ships today"",
                        ""arrowLeadTime"": """",
                        ""isNcnr"": false,
                        ""isNpi"": false,
                        ""productCode"": ""LM358QT"",
                        ""eccnCode"": ""EAR99"",
                        ""htsCode"": ""8542.33.00.01"",
                        ""containerType"": """"
                      }
                    ]
                  }
                ]
              }
            ]
          },
          ""hasDatasheet"": true,
          ""categoryName"": ""Operational Amplifiers - Op Amps"",
          ""status"": ""Active""
        },
        {
          ""itemId"": 21767123,
          ""partNum"": ""LM358EDR2G"",
          ""manufacturer"": {
            ""mfrCd"": ""ONSEMI"",
            ""mfrName"": ""ON Semiconductor""
          },
          ""desc"": ""Op Amp Dual GP ±16V/32V 8-Pin SOIC N T/R"",
          ""packageType"": """",
          ""resources"": [
            {
              ""type"": ""datasheet"",
              ""uri"": ""https://static6.arrow.com/aropdfconversion/673bae871315ec144268deda76298da17ecb69f6/lm358-d.pdf""
            },
            {
              ""type"": ""image_small"",
              ""uri"": ""https://static6.arrow.com/aropdfconversion/arrowimages/1dec370fc8537f0308b6d4c600fda213d2dbf08e/lm393edr2g.jpg""
            },
            {
              ""type"": ""image_large"",
              ""uri"": ""https://static6.arrow.com/aropdfconversion/arrowimages/b4dda76d34c9e9f53dd6b8d5eed0d9c47be155e8/lm393edr2g.jpg""
            },
            {
              ""type"": ""cloud_part_detail"",
              ""uri"": ""https://www.arrow.com/en/products/lm358edr2g/on-semiconductor""
            },
            {
              ""type"": ""api_part_detail"",
              ""uri"": ""http://api.arrow.com/itemservice/v2/en/detail?mfrCd=ONSEMI&partNum=LM358EDR2G""
            },
            {
              ""type"": ""part_detail"",
              ""uri"": ""http://api.arrow.com/itemservice/v2/en/detail?pkey=gLIPnhxqZfkRVU2bHZ_Baso_47zTHPcZ09dl6eN3PWg""
            }
          ],
          ""EnvData"": {
            ""compliance"": [
              {
                ""displayLabel"": ""eurohs"",
                ""displayValue"": ""Compliant""
              },
              {
                ""displayLabel"": ""cnrohs"",
                ""displayValue"": ""Compliant""
              }
            ]
          },
          ""InvOrg"": {
            ""webSites"": [
              {
                ""code"": ""Verical.com"",
                ""name"": ""Verical.com"",
                ""sources"": [
                  {
                    ""currency"": ""USD"",
                    ""sourceCd"": ""VERICAL"",
                    ""displayName"": ""Verical"",
                    ""sourceParts"": [
                      {
                        ""packSize"": 1,
                        ""minimumOrderQuantity"": 80,
                        ""sourcePartId"": ""61602089"",
                        ""Prices"": {
                          ""resaleList"": [
                            {
                              ""displayPrice"": ""0.299"",
                              ""price"": 0.299,
                              ""minQty"": 80,
                              ""maxQty"": 99
                            },
                            {
                              ""displayPrice"": ""0.2788"",
                              ""price"": 0.2788,
                              ""minQty"": 100,
                              ""maxQty"": 249
                            },
                            {
                              ""displayPrice"": ""0.2637"",
                              ""price"": 0.2637,
                              ""minQty"": 250,
                              ""maxQty"": 499
                            },
                            {
                              ""displayPrice"": ""0.168"",
                              ""price"": 0.168,
                              ""minQty"": 500,
                              ""maxQty"": 999
                            },
                            {
                              ""displayPrice"": ""0.1663"",
                              ""price"": 0.1663,
                              ""minQty"": 1000,
                              ""maxQty"": 999999999
                            }
                          ]
                        },
                        ""Availability"": [
                          {
                            ""fohQty"": 2079,
                            ""availabilityCd"": ""INSTK"",
                            ""availabilityMessage"": ""In Stock"",
                            ""pipeline"": []
                          }
                        ],
                        ""customerSpecificPricing"": [],
                        ""customerSpecificInventory"": [],
                        ""dateCode"": ""2150"",
                        ""resources"": [
                          {
                            ""type"": ""detail"",
                            ""uri"": ""https://www.verical.com/pd/aptina-imaging-amplifier---operational-LM358EDR2G-9861596""
                          },
                          {
                            ""type"": ""add_to_cart"",
                            ""uri"": ""https://www.verical.com/pd/aptina-imaging-amplifier---operational-LM358EDR2G-9861596""
                          }
                        ],
                        ""inStock"": true,
                        ""mfrLeadTime"": 0,
                        ""tariffFlag"": false,
                        ""arrowLeadTime"": """",
                        ""isNcnr"": false,
                        ""isNpi"": false,
                        ""productCode"": ""LM358EDR2G"",
                        ""eccnCode"": ""EAR99"",
                        ""htsCode"": ""8542.33.00.01"",
                        ""countryOfOrigin"": ""TH"",
                        ""containerType"": """"
                      }
                    ]
                  }
                ]
              },
              {
                ""code"": ""arrow.com"",
                ""name"": ""arrow.com"",
                ""sources"": [
                  {
                    ""currency"": ""USD"",
                    ""sourceCd"": ""ACNA"",
                    ""displayName"": ""Arrow Americas"",
                    ""sourceParts"": [
                      {
                        ""packSize"": 1,
                        ""minimumOrderQuantity"": 1,
                        ""sourcePartId"": ""V72:2272_16564967"",
                        ""Prices"": {
                          ""resaleList"": [
                            {
                              ""displayPrice"": ""0.3822"",
                              ""price"": 0.3822,
                              ""minQty"": 1,
                              ""maxQty"": 9
                            },
                            {
                              ""displayPrice"": ""0.2875"",
                              ""price"": 0.2875,
                              ""minQty"": 10,
                              ""maxQty"": 24
                            },
                            {
                              ""displayPrice"": ""0.2735"",
                              ""price"": 0.2735,
                              ""minQty"": 25,
                              ""maxQty"": 99
                            },
                            {
                              ""displayPrice"": ""0.1991"",
                              ""price"": 0.1991,
                              ""minQty"": 100,
                              ""maxQty"": 249
                            },
                            {
                              ""displayPrice"": ""0.1907"",
                              ""price"": 0.1907,
                              ""minQty"": 250,
                              ""maxQty"": 499
                            },
                            {
                              ""displayPrice"": ""0.168"",
                              ""price"": 0.168,
                              ""minQty"": 500,
                              ""maxQty"": 999
                            },
                            {
                              ""displayPrice"": ""0.1593"",
                              ""price"": 0.1593,
                              ""minQty"": 1000,
                              ""maxQty"": 999999999
                            }
                          ]
                        },
                        ""Availability"": [
                          {
                            ""fohQty"": 2079,
                            ""availabilityCd"": ""CSTRP"",
                            ""availabilityMessage"": ""Cut Strips"",
                            ""pipeline"": []
                          }
                        ],
                        ""customerSpecificPricing"": [],
                        ""customerSpecificInventory"": [],
                        ""dateCode"": ""2150"",
                        ""resources"": [
                          {
                            ""type"": ""detail"",
                            ""uri"": ""https://www.arrow.com/en/products/lm358edr2g/on-semiconductor?region=nac""
                          },
                          {
                            ""type"": ""add_to_cart"",
                            ""uri"": ""https://www.arrow.com/en/products/lm358edr2g/on-semiconductor?region=nac""
                          }
                        ],
                        ""inStock"": true,
                        ""mfrLeadTime"": 45,
                        ""tariffFlag"": false,
                        ""shipsFrom"": ""United States of America"",
                        ""shipsIn"": ""Ships today"",
                        ""arrowLeadTime"": """",
                        ""isNcnr"": false,
                        ""isNpi"": false,
                        ""productCode"": ""LM358EDR2G"",
                        ""eccnCode"": ""EAR99"",
                        ""htsCode"": ""8542.33.00.01"",
                        ""countryOfOrigin"": ""TH"",
                        ""containerType"": ""Cut Strips""
                      },
                      {
                        ""packSize"": 2500,
                        ""minimumOrderQuantity"": 2500,
                        ""sourcePartId"": ""V36:1790_16564967"",
                        ""Prices"": {
                          ""resaleList"": [
                            {
                              ""displayPrice"": ""0.0933"",
                              ""price"": 0.0933,
                              ""minQty"": 2500,
                              ""maxQty"": 24999
                            },
                            {
                              ""displayPrice"": ""0.0886"",
                              ""price"": 0.0886,
                              ""minQty"": 25000,
                              ""maxQty"": 49999
                            },
                            {
                              ""displayPrice"": ""0.0855"",
                              ""price"": 0.0855,
                              ""minQty"": 50000,
                              ""maxQty"": 99999999
                            }
                          ]
                        },
                        ""Availability"": [
                          {
                            ""fohQty"": 0,
                            ""availabilityCd"": ""QUOTE"",
                            ""availabilityMessage"": ""Call For Quote"",
                            ""pipeline"": [
                              {
                                ""delivery"": ""04-DEC-2024"",
                                ""quantity"": 7500
                              }
                            ]
                          }
                        ],
                        ""customerSpecificPricing"": [],
                        ""customerSpecificInventory"": [],
                        ""dateCode"": """",
                        ""resources"": [
                          {
                            ""type"": ""detail"",
                            ""uri"": ""https://www.arrow.com/en/products/lm358edr2g/on-semiconductor?region=nac""
                          },
                          {
                            ""type"": ""add_to_cart"",
                            ""uri"": ""https://www.arrow.com/en/products/lm358edr2g/on-semiconductor?region=nac""
                          }
                        ],
                        ""inStock"": false,
                        ""mfrLeadTime"": 45,
                        ""tariffFlag"": false,
                        ""shipsFrom"": ""United States of America"",
                        ""shipsIn"": ""Ships today"",
                        ""arrowLeadTime"": """",
                        ""isNcnr"": false,
                        ""isNpi"": false,
                        ""productCode"": ""LM358EDR2G"",
                        ""eccnCode"": ""EAR99"",
                        ""htsCode"": ""8542.33.00.01"",
                        ""containerType"": """"
                      }
                    ]
                  }
                ]
              }
            ]
          },
          ""hasDatasheet"": true,
          ""categoryName"": ""Operational Amplifiers - Op Amps"",
          ""status"": ""Active""
        },
        {
          ""itemId"": 1351519,
          ""partNum"": ""LM358ADR2G"",
          ""manufacturer"": {
            ""mfrCd"": ""ONSEMI"",
            ""mfrName"": ""ON Semiconductor""
          },
          ""desc"": ""Op Amp Dual GP ±16V/32V Automotive 8-Pin SOIC N T/R"",
          ""packageType"": """",
          ""resources"": [
            {
              ""type"": ""datasheet"",
              ""uri"": ""https://static6.arrow.com/aropdfconversion/673bae871315ec144268deda76298da17ecb69f6/lm358-d.pdf""
            },
            {
              ""type"": ""image_small"",
              ""uri"": ""https://static6.arrow.com/aropdfconversion/arrowimages/1dec370fc8537f0308b6d4c600fda213d2dbf08e/lm393edr2g.jpg""
            },
            {
              ""type"": ""image_large"",
              ""uri"": ""https://static6.arrow.com/aropdfconversion/arrowimages/b4dda76d34c9e9f53dd6b8d5eed0d9c47be155e8/lm393edr2g.jpg""
            },
            {
              ""type"": ""cloud_part_detail"",
              ""uri"": ""https://www.arrow.com/en/products/lm358adr2g/on-semiconductor""
            },
            {
              ""type"": ""api_part_detail"",
              ""uri"": ""http://api.arrow.com/itemservice/v2/en/detail?mfrCd=ONSEMI&partNum=LM358ADR2G""
            },
            {
              ""type"": ""part_detail"",
              ""uri"": ""http://api.arrow.com/itemservice/v2/en/detail?pkey=PkaVNfDouhtizFswTJd1t1OGuJefxRNH792Dbj2iTxQ""
            }
          ],
          ""EnvData"": {
            ""compliance"": [
              {
                ""displayLabel"": ""eurohs"",
                ""displayValue"": ""Compliant""
              },
              {
                ""displayLabel"": ""cnrohs"",
                ""displayValue"": ""Compliant""
              }
            ]
          },
          ""InvOrg"": {
            ""webSites"": [
              {
                ""code"": ""Verical.com"",
                ""name"": ""Verical.com"",
                ""sources"": [
                  {
                    ""currency"": ""USD"",
                    ""sourceCd"": ""VERICAL"",
                    ""displayName"": ""Verical"",
                    ""sourceParts"": [
                      {
                        ""packSize"": 2500,
                        ""minimumOrderQuantity"": 2500,
                        ""sourcePartId"": ""61768694"",
                        ""Prices"": {
                          ""resaleList"": [
                            {
                              ""displayPrice"": ""0.5166"",
                              ""price"": 0.5166,
                              ""minQty"": 2500,
                              ""maxQty"": 24999
                            },
                            {
                              ""displayPrice"": ""0.1495"",
                              ""price"": 0.1495,
                              ""minQty"": 25000,
                              ""maxQty"": 49999
                            },
                            {
                              ""displayPrice"": ""0.148"",
                              ""price"": 0.148,
                              ""minQty"": 50000,
                              ""maxQty"": 999999999
                            }
                          ]
                        },
                        ""Availability"": [
                          {
                            ""fohQty"": 10000,
                            ""availabilityCd"": ""INSTK"",
                            ""availabilityMessage"": ""In Stock"",
                            ""pipeline"": []
                          }
                        ],
                        ""customerSpecificPricing"": [],
                        ""customerSpecificInventory"": [],
                        ""dateCode"": ""2212"",
                        ""resources"": [
                          {
                            ""type"": ""detail"",
                            ""uri"": ""https://www.verical.com/pd/aptina-imaging-amplifier---operational-LM358ADR2G-9650723""
                          },
                          {
                            ""type"": ""add_to_cart"",
                            ""uri"": ""https://www.verical.com/pd/aptina-imaging-amplifier---operational-LM358ADR2G-9650723""
                          }
                        ],
                        ""inStock"": true,
                        ""mfrLeadTime"": 0,
                        ""tariffFlag"": false,
                        ""arrowLeadTime"": """",
                        ""isNcnr"": false,
                        ""isNpi"": false,
                        ""productCode"": ""LM358ADR2G"",
                        ""eccnCode"": ""EAR99"",
                        ""htsCode"": ""8542.39.00.01"",
                        ""countryOfOrigin"": ""CN"",
                        ""containerType"": """"
                      }
                    ]
                  }
                ]
              },
              {
                ""code"": ""arrow.com"",
                ""name"": ""arrow.com"",
                ""sources"": [
                  {
                    ""currency"": ""USD"",
                    ""sourceCd"": ""ACNA"",
                    ""displayName"": ""Arrow Americas"",
                    ""sourceParts"": [
                      {
                        ""packSize"": 2500,
                        ""minimumOrderQuantity"": 2500,
                        ""sourcePartId"": ""V36:1790_07301763"",
                        ""Prices"": {
                          ""resaleList"": [
                            {
                              ""displayPrice"": ""0.194"",
                              ""price"": 0.194,
                              ""minQty"": 2500,
                              ""maxQty"": 24999
                            },
                            {
                              ""displayPrice"": ""0.192"",
                              ""price"": 0.192,
                              ""minQty"": 25000,
                              ""maxQty"": 49999
                            },
                            {
                              ""displayPrice"": ""0.1901"",
                              ""price"": 0.1901,
                              ""minQty"": 50000,
                              ""maxQty"": 99999999
                            }
                          ]
                        },
                        ""Availability"": [
                          {
                            ""fohQty"": 10000,
                            ""availabilityCd"": ""INSTK"",
                            ""availabilityMessage"": ""In Stock"",
                            ""pipeline"": [
                              {
                                ""delivery"": ""01-SEP-2023"",
                                ""quantity"": 10000
                              }
                            ]
                          }
                        ],
                        ""customerSpecificPricing"": [],
                        ""customerSpecificInventory"": [],
                        ""dateCode"": ""2212"",
                        ""resources"": [
                          {
                            ""type"": ""detail"",
                            ""uri"": ""https://www.arrow.com/en/products/lm358adr2g/on-semiconductor?region=nac""
                          },
                          {
                            ""type"": ""add_to_cart"",
                            ""uri"": ""https://www.arrow.com/en/products/lm358adr2g/on-semiconductor?region=nac""
                          }
                        ],
                        ""inStock"": true,
                        ""mfrLeadTime"": 45,
                        ""tariffFlag"": true,
                        ""shipsFrom"": ""United States of America"",
                        ""shipsIn"": ""Ships today"",
                        ""arrowLeadTime"": """",
                        ""isNcnr"": false,
                        ""isNpi"": false,
                        ""productCode"": ""LM358ADR2G"",
                        ""eccnCode"": ""EAR99"",
                        ""htsCode"": ""8542.39.00.01"",
                        ""countryOfOrigin"": ""CN"",
                        ""containerType"": """"
                      },
                      {
                        ""packSize"": 2500,
                        ""minimumOrderQuantity"": 5000,
                        ""sourcePartId"": ""V36:1790_22402993"",
                        ""Availability"": [
                          {
                            ""fohQty"": 0,
                            ""availabilityCd"": ""QUOTE"",
                            ""availabilityMessage"": ""Call For Quote"",
                            ""pipeline"": []
                          }
                        ],
                        ""customerSpecificPricing"": [],
                        ""customerSpecificInventory"": [],
                        ""dateCode"": """",
                        ""resources"": [
                          {
                            ""type"": ""detail"",
                            ""uri"": ""https://www.arrow.com/en/products/lm358adr2g/on-semiconductor?region=nac""
                          },
                          {
                            ""type"": ""add_to_cart"",
                            ""uri"": ""https://www.arrow.com/en/products/lm358adr2g/on-semiconductor?region=nac""
                          }
                        ],
                        ""inStock"": false,
                        ""mfrLeadTime"": 0,
                        ""tariffFlag"": false,
                        ""shipsFrom"": ""United States of America"",
                        ""shipsIn"": ""Ships today"",
                        ""arrowLeadTime"": """",
                        ""isNcnr"": false,
                        ""isNpi"": false,
                        ""productCode"": ""LM358ADR2G"",
                        ""eccnCode"": ""EAR99"",
                        ""htsCode"": ""8542.39.00.01"",
                        ""containerType"": """"
                      },
                      {
                        ""packSize"": 1,
                        ""minimumOrderQuantity"": 1,
                        ""sourcePartId"": ""V72:2272_07301763"",
                        ""Prices"": {
                          ""resaleList"": [
                            {
                              ""displayPrice"": ""0.5142"",
                              ""price"": 0.5142,
                              ""minQty"": 1,
                              ""maxQty"": 9
                            },
                            {
                              ""displayPrice"": ""0.367"",
                              ""price"": 0.367,
                              ""minQty"": 10,
                              ""maxQty"": 24
                            },
                            {
                              ""displayPrice"": ""0.3633"",
                              ""price"": 0.3633,
                              ""minQty"": 25,
                              ""maxQty"": 999999999
                            }
                          ]
                        },
                        ""Availability"": [
                          {
                            ""fohQty"": 29,
                            ""availabilityCd"": ""CSTRP"",
                            ""availabilityMessage"": ""Cut Strips"",
                            ""pipeline"": [
                              {
                                ""delivery"": ""26-JUL-2023"",
                                ""quantity"": 7500
                              }
                            ]
                          }
                        ],
                        ""customerSpecificPricing"": [],
                        ""customerSpecificInventory"": [],
                        ""dateCode"": ""2131"",
                        ""resources"": [
                          {
                            ""type"": ""detail"",
                            ""uri"": ""https://www.arrow.com/en/products/lm358adr2g/on-semiconductor?region=nac""
                          },
                          {
                            ""type"": ""add_to_cart"",
                            ""uri"": ""https://www.arrow.com/en/products/lm358adr2g/on-semiconductor?region=nac""
                          }
                        ],
                        ""inStock"": true,
                        ""mfrLeadTime"": 45,
                        ""tariffFlag"": true,
                        ""shipsFrom"": ""United States of America"",
                        ""shipsIn"": ""Ships today"",
                        ""arrowLeadTime"": """",
                        ""isNcnr"": false,
                        ""isNpi"": false,
                        ""productCode"": ""LM358ADR2G"",
                        ""eccnCode"": ""EAR99"",
                        ""htsCode"": ""8542.39.00.01"",
                        ""countryOfOrigin"": ""CN"",
                        ""containerType"": ""Cut Strips""
                      },
                      {
                        ""packSize"": 1,
                        ""minimumOrderQuantity"": 5000,
                        ""sourcePartId"": ""V79:2366_22402993"",
                        ""Availability"": [
                          {
                            ""fohQty"": 0,
                            ""availabilityCd"": ""QUOTE"",
                            ""availabilityMessage"": ""Call For Quote"",
                            ""pipeline"": []
                          }
                        ],
                        ""customerSpecificPricing"": [],
                        ""customerSpecificInventory"": [],
                        ""dateCode"": """",
                        ""resources"": [
                          {
                            ""type"": ""detail"",
                            ""uri"": ""https://www.arrow.com/en/products/lm358adr2g/on-semiconductor?region=nac""
                          },
                          {
                            ""type"": ""add_to_cart"",
                            ""uri"": ""https://www.arrow.com/en/products/lm358adr2g/on-semiconductor?region=nac""
                          }
                        ],
                        ""inStock"": false,
                        ""mfrLeadTime"": 0,
                        ""tariffFlag"": false,
                        ""shipsFrom"": ""United States of America"",
                        ""shipsIn"": ""Ships today"",
                        ""arrowLeadTime"": """",
                        ""isNcnr"": false,
                        ""isNpi"": false,
                        ""productCode"": ""LM358ADR2G"",
                        ""eccnCode"": ""EAR99"",
                        ""htsCode"": ""8542.39.00.01"",
                        ""containerType"": """"
                      }
                    ]
                  }
                ]
              }
            ]
          },
          ""hasDatasheet"": true,
          ""categoryName"": ""Operational Amplifiers - Op Amps"",
          ""status"": ""Active""
        },
        {
          ""itemId"": 2488069,
          ""partNum"": ""LM358S-13"",
          ""manufacturer"": {
            ""mfrCd"": ""DIODEZTX"",
            ""mfrName"": ""Diodes Zetex""
          },
          ""desc"": ""Op Amp Dual Low Power Amplifier ±16V/32V 8-Pin SOP T/R"",
          ""packageType"": """",
          ""resources"": [
            {
              ""type"": ""datasheet"",
              ""uri"": ""https://static6.arrow.com/aropdfconversion/77225bdc637cccb8d855bc7246b9458ee36a7cab/lm358.pdf""
            },
            {
              ""type"": ""image_small"",
              ""uri"": ""https://static6.arrow.com/aropdfconversion/arrowimages/80deece09f16df320e13724f9905e917608cd105/296-8-soic.jpg""
            },
            {
              ""type"": ""image_large"",
              ""uri"": ""https://static6.arrow.com/aropdfconversion/arrowimages/e8a214efd5b674ddcd8a4a60c6e0007f2948c906/296-8-soic.jpg""
            },
            {
              ""type"": ""cloud_part_detail"",
              ""uri"": ""https://www.arrow.com/en/products/lm358s-13/diodes-incorporated""
            },
            {
              ""type"": ""api_part_detail"",
              ""uri"": ""http://api.arrow.com/itemservice/v2/en/detail?mfrCd=DIODEZTX&partNum=LM358S13""
            },
            {
              ""type"": ""part_detail"",
              ""uri"": ""http://api.arrow.com/itemservice/v2/en/detail?pkey=OI85wcJyjwfWmUhiF63R4JbZUVlOWoRmvPAL3H3Vv3o""
            }
          ],
          ""EnvData"": {
            ""compliance"": [
              {
                ""displayLabel"": ""eurohs"",
                ""displayValue"": ""Compliant""
              },
              {
                ""displayLabel"": ""cnrohs"",
                ""displayValue"": ""Compliant""
              }
            ]
          },
          ""InvOrg"": {
            ""webSites"": [
              {
                ""code"": ""Verical.com"",
                ""name"": ""Verical.com"",
                ""sources"": [
                  {
                    ""currency"": ""USD"",
                    ""sourceCd"": ""VERICAL"",
                    ""displayName"": ""Verical"",
                    ""sourceParts"": [
                      {
                        ""packSize"": 1,
                        ""minimumOrderQuantity"": 89,
                        ""sourcePartId"": ""64044275"",
                        ""Prices"": {
                          ""resaleList"": [
                            {
                              ""displayPrice"": ""0.3006"",
                              ""price"": 0.3006,
                              ""minQty"": 89,
                              ""maxQty"": 99
                            },
                            {
                              ""displayPrice"": ""0.1885"",
                              ""price"": 0.1885,
                              ""minQty"": 100,
                              ""maxQty"": 249
                            },
                            {
                              ""displayPrice"": ""0.1865"",
                              ""price"": 0.1865,
                              ""minQty"": 250,
                              ""maxQty"": 499
                            },
                            {
                              ""displayPrice"": ""0.1847"",
                              ""price"": 0.1847,
                              ""minQty"": 500,
                              ""maxQty"": 999
                            },
                            {
                              ""displayPrice"": ""0.1147"",
                              ""price"": 0.1147,
                              ""minQty"": 1000,
                              ""maxQty"": 999999999
                            }
                          ]
                        },
                        ""Availability"": [
                          {
                            ""fohQty"": 2230,
                            ""availabilityCd"": ""INSTK"",
                            ""availabilityMessage"": ""In Stock"",
                            ""pipeline"": []
                          }
                        ],
                        ""customerSpecificPricing"": [],
                        ""customerSpecificInventory"": [],
                        ""dateCode"": ""2235"",
                        ""resources"": [
                          {
                            ""type"": ""detail"",
                            ""uri"": ""https://www.verical.com/pd/diodes-inc-amplifier---operational-LM358S-13-631747""
                          },
                          {
                            ""type"": ""add_to_cart"",
                            ""uri"": ""https://www.verical.com/pd/diodes-inc-amplifier---operational-LM358S-13-631747""
                          }
                        ],
                        ""inStock"": true,
                        ""mfrLeadTime"": 0,
                        ""tariffFlag"": false,
                        ""arrowLeadTime"": """",
                        ""isNcnr"": false,
                        ""isNpi"": false,
                        ""productCode"": ""LM358S13"",
                        ""eccnCode"": ""EAR99"",
                        ""htsCode"": ""8542.33.00.01"",
                        ""countryOfOrigin"": ""CN"",
                        ""containerType"": """"
                      }
                    ]
                  }
                ]
              },
              {
                ""code"": ""arrow.com"",
                ""name"": ""arrow.com"",
                ""sources"": [
                  {
                    ""currency"": ""USD"",
                    ""sourceCd"": ""ACNA"",
                    ""displayName"": ""Arrow Americas"",
                    ""sourceParts"": [
                      {
                        ""packSize"": 1,
                        ""minimumOrderQuantity"": 1,
                        ""sourcePartId"": ""V72:2272_06702720"",
                        ""Prices"": {
                          ""resaleList"": [
                            {
                              ""displayPrice"": ""0.3721"",
                              ""price"": 0.3721,
                              ""minQty"": 1,
                              ""maxQty"": 9
                            },
                            {
                              ""displayPrice"": ""0.3032"",
                              ""price"": 0.3032,
                              ""minQty"": 10,
                              ""maxQty"": 24
                            },
                            {
                              ""displayPrice"": ""0.3002"",
                              ""price"": 0.3002,
                              ""minQty"": 25,
                              ""maxQty"": 99
                            },
                            {
                              ""displayPrice"": ""0.1877"",
                              ""price"": 0.1877,
                              ""minQty"": 100,
                              ""maxQty"": 249
                            },
                            {
                              ""displayPrice"": ""0.1855"",
                              ""price"": 0.1855,
                              ""minQty"": 250,
                              ""maxQty"": 499
                            },
                            {
                              ""displayPrice"": ""0.1527"",
                              ""price"": 0.1527,
                              ""minQty"": 500,
                              ""maxQty"": 999
                            },
                            {
                              ""displayPrice"": ""0.1147"",
                              ""price"": 0.1147,
                              ""minQty"": 1000,
                              ""maxQty"": 999999999
                            }
                          ]
                        },
                        ""Availability"": [
                          {
                            ""fohQty"": 2230,
                            ""availabilityCd"": ""CSTRP"",
                            ""availabilityMessage"": ""Cut Strips"",
                            ""pipeline"": []
                          }
                        ],
                        ""customerSpecificPricing"": [],
                        ""customerSpecificInventory"": [],
                        ""dateCode"": ""2235"",
                        ""resources"": [
                          {
                            ""type"": ""detail"",
                            ""uri"": ""https://www.arrow.com/en/products/lm358s-13/diodes-incorporated?region=nac""
                          },
                          {
                            ""type"": ""add_to_cart"",
                            ""uri"": ""https://www.arrow.com/en/products/lm358s-13/diodes-incorporated?region=nac""
                          }
                        ],
                        ""inStock"": true,
                        ""mfrLeadTime"": 12,
                        ""tariffFlag"": true,
                        ""shipsFrom"": ""United States of America"",
                        ""shipsIn"": ""Ships today"",
                        ""arrowLeadTime"": """",
                        ""isNcnr"": false,
                        ""isNpi"": false,
                        ""productCode"": ""LM358S13"",
                        ""eccnCode"": ""EAR99"",
                        ""htsCode"": ""8542.33.00.01"",
                        ""countryOfOrigin"": ""CN"",
                        ""containerType"": ""Cut Strips""
                      },
                      {
                        ""packSize"": 2500,
                        ""minimumOrderQuantity"": 2500,
                        ""sourcePartId"": ""V36:1790_06702720"",
                        ""Availability"": [
                          {
                            ""fohQty"": 0,
                            ""availabilityCd"": ""QUOTE"",
                            ""availabilityMessage"": ""Call For Quote"",
                            ""pipeline"": []
                          }
                        ],
                        ""customerSpecificPricing"": [],
                        ""customerSpecificInventory"": [],
                        ""dateCode"": """",
                        ""resources"": [
                          {
                            ""type"": ""detail"",
                            ""uri"": ""https://www.arrow.com/en/products/lm358s-13/diodes-incorporated?region=nac""
                          },
                          {
                            ""type"": ""add_to_cart"",
                            ""uri"": ""https://www.arrow.com/en/products/lm358s-13/diodes-incorporated?region=nac""
                          }
                        ],
                        ""inStock"": false,
                        ""mfrLeadTime"": 12,
                        ""tariffFlag"": false,
                        ""shipsFrom"": ""United States of America"",
                        ""shipsIn"": ""Ships today"",
                        ""arrowLeadTime"": """",
                        ""isNcnr"": false,
                        ""isNpi"": false,
                        ""productCode"": ""LM358S13"",
                        ""eccnCode"": ""EAR99"",
                        ""htsCode"": ""8542.33.00.01"",
                        ""containerType"": """"
                      }
                    ]
                  }
                ]
              }
            ]
          },
          ""hasDatasheet"": true,
          ""categoryName"": ""Operational Amplifiers - Op Amps"",
          ""status"": ""Active""
        },
        {
          ""itemId"": 41646741,
          ""partNum"": ""LM358LVIDGKR"",
          ""manufacturer"": {
            ""mfrCd"": ""TI"",
            ""mfrName"": ""Texas Instruments""
          },
          ""desc"": ""Op Amp Dual Low Voltage Amplifier ±2.75V/5.5V 8-Pin VSSOP T/R"",
          ""packageType"": """",
          ""resources"": [
            {
              ""type"": ""image_small"",
              ""uri"": ""https://static6.arrow.com/aropdfconversion/arrowimages/d434fa3d801a5aae250ae767e885b92a7f67789/dgk0008a.jpg""
            },
            {
              ""type"": ""image_large"",
              ""uri"": ""https://static6.arrow.com/aropdfconversion/arrowimages/93b7157dcd26e99206bafbcbea41981ebe83d36e/dgk0008a.jpg""
            },
            {
              ""type"": ""cloud_part_detail"",
              ""uri"": ""https://www.arrow.com/en/products/lm358lvidgkr/texas-instruments""
            },
            {
              ""type"": ""api_part_detail"",
              ""uri"": ""http://api.arrow.com/itemservice/v2/en/detail?mfrCd=TI&partNum=LM358LVIDGKR""
            },
            {
              ""type"": ""part_detail"",
              ""uri"": ""http://api.arrow.com/itemservice/v2/en/detail?pkey=Y7s5fwvSBMCfQCKf_PnGwmhCbfcbvNllpc9mtNY28gA""
            }
          ],
          ""EnvData"": {
            ""compliance"": [
              {
                ""displayLabel"": ""eurohs"",
                ""displayValue"": ""Compliant""
              },
              {
                ""displayLabel"": ""cnrohs"",
                ""displayValue"": ""Compliant""
              }
            ]
          },
          ""InvOrg"": {
            ""webSites"": [
              {
                ""code"": ""Verical.com"",
                ""name"": ""Verical.com"",
                ""sources"": [
                  {
                    ""currency"": ""USD"",
                    ""sourceCd"": ""VERICAL"",
                    ""displayName"": ""Verical"",
                    ""sourceParts"": [
                      {
                        ""packSize"": 1,
                        ""minimumOrderQuantity"": 87,
                        ""sourcePartId"": ""65205611"",
                        ""Prices"": {
                          ""resaleList"": [
                            {
                              ""displayPrice"": ""0.5296"",
                              ""price"": 0.5296,
                              ""minQty"": 87,
                              ""maxQty"": 99
                            },
                            {
                              ""displayPrice"": ""0.5245"",
                              ""price"": 0.5245,
                              ""minQty"": 100,
                              ""maxQty"": 249
                            },
                            {
                              ""displayPrice"": ""0.5196"",
                              ""price"": 0.5196,
                              ""minQty"": 250,
                              ""maxQty"": 499
                            },
                            {
                              ""displayPrice"": ""0.5146"",
                              ""price"": 0.5146,
                              ""minQty"": 500,
                              ""maxQty"": 999
                            },
                            {
                              ""displayPrice"": ""0.5096"",
                              ""price"": 0.5096,
                              ""minQty"": 1000,
                              ""maxQty"": 999999999
                            }
                          ]
                        },
                        ""Availability"": [
                          {
                            ""fohQty"": 1494,
                            ""availabilityCd"": ""INSTK"",
                            ""availabilityMessage"": ""In Stock"",
                            ""pipeline"": []
                          }
                        ],
                        ""customerSpecificPricing"": [],
                        ""customerSpecificInventory"": [],
                        ""dateCode"": ""2244"",
                        ""resources"": [
                          {
                            ""type"": ""detail"",
                            ""uri"": ""https://www.verical.com/pd/national-semiconductor-amplifier---operational-LM358LVIDGKR-10124645""
                          },
                          {
                            ""type"": ""add_to_cart"",
                            ""uri"": ""https://www.verical.com/pd/national-semiconductor-amplifier---operational-LM358LVIDGKR-10124645""
                          }
                        ],
                        ""inStock"": true,
                        ""mfrLeadTime"": 0,
                        ""tariffFlag"": false,
                        ""arrowLeadTime"": """",
                        ""isNcnr"": false,
                        ""isNpi"": false,
                        ""productCode"": ""LM358LVIDGKR"",
                        ""eccnCode"": ""EAR99"",
                        ""htsCode"": ""8542.33.00.01"",
                        ""countryOfOrigin"": ""CN"",
                        ""containerType"": """"
                      }
                    ]
                  }
                ]
              },
              {
                ""code"": ""arrow.com"",
                ""name"": ""arrow.com"",
                ""sources"": [
                  {
                    ""currency"": ""USD"",
                    ""sourceCd"": ""ACNA"",
                    ""displayName"": ""Arrow Americas"",
                    ""sourceParts"": [
                      {
                        ""packSize"": 2500,
                        ""minimumOrderQuantity"": 2500,
                        ""sourcePartId"": ""V36:1790_22822370"",
                        ""Prices"": {
                          ""resaleList"": [
                            {
                              ""displayPrice"": ""0.0642"",
                              ""price"": 0.0642,
                              ""minQty"": 2500,
                              ""maxQty"": 9999
                            },
                            {
                              ""displayPrice"": ""0.0609"",
                              ""price"": 0.0609,
                              ""minQty"": 10000,
                              ""maxQty"": 24999
                            },
                            {
                              ""displayPrice"": ""0.059"",
                              ""price"": 0.059,
                              ""minQty"": 25000,
                              ""maxQty"": 49999
                            },
                            {
                              ""displayPrice"": ""0.0574"",
                              ""price"": 0.0574,
                              ""minQty"": 50000,
                              ""maxQty"": 99999999
                            }
                          ]
                        },
                        ""Availability"": [
                          {
                            ""fohQty"": 0,
                            ""availabilityCd"": ""QUOTE"",
                            ""availabilityMessage"": ""Call For Quote"",
                            ""pipeline"": []
                          }
                        ],
                        ""customerSpecificPricing"": [],
                        ""customerSpecificInventory"": [],
                        ""dateCode"": """",
                        ""resources"": [
                          {
                            ""type"": ""detail"",
                            ""uri"": ""https://www.arrow.com/en/products/lm358lvidgkr/texas-instruments?region=nac""
                          },
                          {
                            ""type"": ""add_to_cart"",
                            ""uri"": ""https://www.arrow.com/en/products/lm358lvidgkr/texas-instruments?region=nac""
                          }
                        ],
                        ""inStock"": false,
                        ""mfrLeadTime"": 12,
                        ""tariffFlag"": false,
                        ""shipsFrom"": ""United States of America"",
                        ""shipsIn"": ""Ships today"",
                        ""arrowLeadTime"": """",
                        ""isNcnr"": false,
                        ""isNpi"": false,
                        ""productCode"": ""LM358LVIDGKR"",
                        ""eccnCode"": ""EAR99"",
                        ""htsCode"": ""8542.33.00.01"",
                        ""containerType"": """"
                      },
                      {
                        ""packSize"": 2500,
                        ""minimumOrderQuantity"": 2500,
                        ""sourcePartId"": ""V39:1801_22822370"",
                        ""Prices"": {
                          ""resaleList"": [
                            {
                              ""displayPrice"": ""0.0642"",
                              ""price"": 0.0642,
                              ""minQty"": 2500,
                              ""maxQty"": 9999
                            },
                            {
                              ""displayPrice"": ""0.0609"",
                              ""price"": 0.0609,
                              ""minQty"": 10000,
                              ""maxQty"": 24999
                            },
                            {
                              ""displayPrice"": ""0.059"",
                              ""price"": 0.059,
                              ""minQty"": 25000,
                              ""maxQty"": 49999
                            },
                            {
                              ""displayPrice"": ""0.0574"",
                              ""price"": 0.0574,
                              ""minQty"": 50000,
                              ""maxQty"": 99999999
                            }
                          ]
                        },
                        ""Availability"": [
                          {
                            ""fohQty"": 0,
                            ""availabilityCd"": ""QUOTE"",
                            ""availabilityMessage"": ""Call For Quote"",
                            ""pipeline"": []
                          }
                        ],
                        ""customerSpecificPricing"": [],
                        ""customerSpecificInventory"": [],
                        ""dateCode"": """",
                        ""resources"": [
                          {
                            ""type"": ""detail"",
                            ""uri"": ""https://www.arrow.com/en/products/lm358lvidgkr/texas-instruments?region=nac""
                          },
                          {
                            ""type"": ""add_to_cart"",
                            ""uri"": ""https://www.arrow.com/en/products/lm358lvidgkr/texas-instruments?region=nac""
                          }
                        ],
                        ""inStock"": false,
                        ""mfrLeadTime"": 12,
                        ""tariffFlag"": false,
                        ""shipsFrom"": ""United States of America"",
                        ""shipsIn"": ""Ships today"",
                        ""arrowLeadTime"": """",
                        ""isNcnr"": false,
                        ""isNpi"": false,
                        ""productCode"": ""LM358LVIDGKR"",
                        ""eccnCode"": ""EAR99"",
                        ""htsCode"": ""8542.33.00.01"",
                        ""containerType"": """"
                      },
                      {
                        ""packSize"": 1,
                        ""minimumOrderQuantity"": 1,
                        ""sourcePartId"": ""V72:2272_22822370"",
                        ""Prices"": {
                          ""resaleList"": [
                            {
                              ""displayPrice"": ""0.4691"",
                              ""price"": 0.4691,
                              ""minQty"": 1,
                              ""maxQty"": 9
                            },
                            {
                              ""displayPrice"": ""0.3539"",
                              ""price"": 0.3539,
                              ""minQty"": 10,
                              ""maxQty"": 24
                            },
                            {
                              ""displayPrice"": ""0.3091"",
                              ""price"": 0.3091,
                              ""minQty"": 25,
                              ""maxQty"": 99
                            },
                            {
                              ""displayPrice"": ""0.2019"",
                              ""price"": 0.2019,
                              ""minQty"": 100,
                              ""maxQty"": 249
                            },
                            {
                              ""displayPrice"": ""0.1664"",
                              ""price"": 0.1664,
                              ""minQty"": 250,
                              ""maxQty"": 499
                            },
                            {
                              ""displayPrice"": ""0.1325"",
                              ""price"": 0.1325,
                              ""minQty"": 500,
                              ""maxQty"": 999
                            },
                            {
                              ""displayPrice"": ""0.1011"",
                              ""price"": 0.1011,
                              ""minQty"": 1000,
                              ""maxQty"": 999999999
                            }
                          ]
                        },
                        ""Availability"": [
                          {
                            ""fohQty"": 1494,
                            ""availabilityCd"": ""CSTRP"",
                            ""availabilityMessage"": ""Cut Strips"",
                            ""pipeline"": []
                          }
                        ],
                        ""customerSpecificPricing"": [],
                        ""customerSpecificInventory"": [],
                        ""dateCode"": ""2244"",
                        ""resources"": [
                          {
                            ""type"": ""detail"",
                            ""uri"": ""https://www.arrow.com/en/products/lm358lvidgkr/texas-instruments?region=nac""
                          },
                          {
                            ""type"": ""add_to_cart"",
                            ""uri"": ""https://www.arrow.com/en/products/lm358lvidgkr/texas-instruments?region=nac""
                          }
                        ],
                        ""inStock"": true,
                        ""mfrLeadTime"": 12,
                        ""tariffFlag"": false,
                        ""shipsFrom"": ""United States of America"",
                        ""shipsIn"": ""Ships today"",
                        ""arrowLeadTime"": """",
                        ""isNcnr"": false,
                        ""isNpi"": false,
                        ""productCode"": ""LM358LVIDGKR"",
                        ""eccnCode"": ""EAR99"",
                        ""htsCode"": ""8542.33.00.01"",
                        ""countryOfOrigin"": ""CN"",
                        ""containerType"": ""Cut Strips""
                      }
                    ]
                  }
                ]
              }
            ]
          },
          ""hasDatasheet"": false,
          ""categoryName"": ""Operational Amplifiers - Op Amps"",
          ""status"": ""Active""
        },
        {
          ""itemId"": 50282909,
          ""partNum"": ""LM358MX/NOPB"",
          ""manufacturer"": {
            ""mfrCd"": ""TI"",
            ""mfrName"": ""Texas Instruments""
          },
          ""desc"": ""Op Amp Dual GP ±16V/32V 8-Pin SOIC T/R"",
          ""packageType"": """",
          ""resources"": [
            {
              ""type"": ""image_small"",
              ""uri"": ""https://static6.arrow.com/aropdfconversion/arrowimages/3f304a36ac6e93000334d1a2cf03ec6bd35c733b/d0008a.jpg""
            },
            {
              ""type"": ""image_large"",
              ""uri"": ""https://static6.arrow.com/aropdfconversion/arrowimages/e5533f271a03578b303601b99b62d8a183cba66a/d0008a.jpg""
            },
            {
              ""type"": ""cloud_part_detail"",
              ""uri"": ""https://www.arrow.com/en/products/lm358mxnopb/texas-instruments""
            },
            {
              ""type"": ""api_part_detail"",
              ""uri"": ""http://api.arrow.com/itemservice/v2/en/detail?mfrCd=TI&partNum=LM358MX/NOPB""
            },
            {
              ""type"": ""part_detail"",
              ""uri"": ""http://api.arrow.com/itemservice/v2/en/detail?pkey=nNP1mzb2Pm2_wCyVN50pc2cMGSB4vGFteQxxFBF5iyA""
            }
          ],
          ""EnvData"": {
            ""compliance"": [
              {
                ""displayLabel"": ""eurohs"",
                ""displayValue"": ""Compliant""
              },
              {
                ""displayLabel"": ""cnrohs"",
                ""displayValue"": ""Compliant""
              }
            ]
          },
          ""InvOrg"": {
            ""webSites"": [
              {
                ""code"": ""Verical.com"",
                ""name"": ""Verical.com"",
                ""sources"": [
                  {
                    ""currency"": ""USD"",
                    ""sourceCd"": ""VERICAL"",
                    ""displayName"": ""Verical"",
                    ""sourceParts"": [
                      {
                        ""packSize"": 1,
                        ""minimumOrderQuantity"": 20,
                        ""sourcePartId"": ""56259658"",
                        ""Prices"": {
                          ""resaleList"": [
                            {
                              ""displayPrice"": ""0.8588"",
                              ""price"": 0.8588,
                              ""minQty"": 20,
                              ""maxQty"": 24
                            },
                            {
                              ""displayPrice"": ""0.8507"",
                              ""price"": 0.8507,
                              ""minQty"": 25,
                              ""maxQty"": 99
                            },
                            {
                              ""displayPrice"": ""0.7095"",
                              ""price"": 0.7095,
                              ""minQty"": 100,
                              ""maxQty"": 249
                            },
                            {
                              ""displayPrice"": ""0.7024"",
                              ""price"": 0.7024,
                              ""minQty"": 250,
                              ""maxQty"": 499
                            },
                            {
                              ""displayPrice"": ""0.6574"",
                              ""price"": 0.6574,
                              ""minQty"": 500,
                              ""maxQty"": 999
                            },
                            {
                              ""displayPrice"": ""0.6509"",
                              ""price"": 0.6509,
                              ""minQty"": 1000,
                              ""maxQty"": 999999999
                            }
                          ]
                        },
                        ""Availability"": [
                          {
                            ""fohQty"": 1530,
                            ""availabilityCd"": ""INSTK"",
                            ""availabilityMessage"": ""In Stock"",
                            ""pipeline"": []
                          }
                        ],
                        ""customerSpecificPricing"": [],
                        ""customerSpecificInventory"": [],
                        ""dateCode"": ""2121"",
                        ""resources"": [
                          {
                            ""type"": ""detail"",
                            ""uri"": ""https://www.verical.com/pd/national-semiconductor-amplifier---operational-LM358MX-NOPB-186432""
                          },
                          {
                            ""type"": ""add_to_cart"",
                            ""uri"": ""https://www.verical.com/pd/national-semiconductor-amplifier---operational-LM358MX-NOPB-186432""
                          }
                        ],
                        ""inStock"": true,
                        ""mfrLeadTime"": 0,
                        ""tariffFlag"": false,
                        ""arrowLeadTime"": """",
                        ""isNcnr"": false,
                        ""isNpi"": false,
                        ""productCode"": ""LM358MX/NOPB"",
                        ""eccnCode"": ""EAR99"",
                        ""htsCode"": ""8542.33.00.01"",
                        ""countryOfOrigin"": ""MY"",
                        ""containerType"": """"
                      }
                    ]
                  }
                ]
              },
              {
                ""code"": ""arrow.com"",
                ""name"": ""arrow.com"",
                ""sources"": [
                  {
                    ""currency"": ""USD"",
                    ""sourceCd"": ""ACNA"",
                    ""displayName"": ""Arrow Americas"",
                    ""sourceParts"": [
                      {
                        ""packSize"": 2500,
                        ""minimumOrderQuantity"": 2500,
                        ""sourcePartId"": ""V39:1801_07275166"",
                        ""Prices"": {
                          ""resaleList"": [
                            {
                              ""displayPrice"": ""0.4299"",
                              ""price"": 0.4299,
                              ""minQty"": 2500,
                              ""maxQty"": 4999
                            },
                            {
                              ""displayPrice"": ""0.4281"",
                              ""price"": 0.4281,
                              ""minQty"": 5000,
                              ""maxQty"": 9999
                            },
                            {
                              ""displayPrice"": ""0.4238"",
                              ""price"": 0.4238,
                              ""minQty"": 10000,
                              ""maxQty"": 14999
                            },
                            {
                              ""displayPrice"": ""0.4196"",
                              ""price"": 0.4196,
                              ""minQty"": 15000,
                              ""maxQty"": 19999
                            },
                            {
                              ""displayPrice"": ""0.4154"",
                              ""price"": 0.4154,
                              ""minQty"": 20000,
                              ""maxQty"": 24999
                            },
                            {
                              ""displayPrice"": ""0.4112"",
                              ""price"": 0.4112,
                              ""minQty"": 25000,
                              ""maxQty"": 44999
                            },
                            {
                              ""displayPrice"": ""0.4071"",
                              ""price"": 0.4071,
                              ""minQty"": 45000,
                              ""maxQty"": 49999
                            },
                            {
                              ""displayPrice"": ""0.403"",
                              ""price"": 0.403,
                              ""minQty"": 50000,
                              ""maxQty"": 99999
                            },
                            {
                              ""displayPrice"": ""0.399"",
                              ""price"": 0.399,
                              ""minQty"": 100000,
                              ""maxQty"": 99999999
                            }
                          ]
                        },
                        ""Availability"": [
                          {
                            ""fohQty"": 0,
                            ""availabilityCd"": ""QUOTE"",
                            ""availabilityMessage"": ""Call For Quote"",
                            ""pipeline"": []
                          }
                        ],
                        ""customerSpecificPricing"": [],
                        ""customerSpecificInventory"": [],
                        ""dateCode"": ""2229"",
                        ""resources"": [
                          {
                            ""type"": ""detail"",
                            ""uri"": ""https://www.arrow.com/en/products/lm358mxnopb/texas-instruments?region=nac""
                          },
                          {
                            ""type"": ""add_to_cart"",
                            ""uri"": ""https://www.arrow.com/en/products/lm358mxnopb/texas-instruments?region=nac""
                          }
                        ],
                        ""inStock"": false,
                        ""mfrLeadTime"": 6,
                        ""tariffFlag"": false,
                        ""shipsFrom"": ""United States of America"",
                        ""shipsIn"": ""Ships today"",
                        ""arrowLeadTime"": """",
                        ""isNcnr"": false,
                        ""isNpi"": false,
                        ""productCode"": ""LM358MX/NOPB"",
                        ""eccnCode"": ""EAR99"",
                        ""htsCode"": ""8542.33.00.01"",
                        ""countryOfOrigin"": ""MY"",
                        ""containerType"": """"
                      },
                      {
                        ""packSize"": 2500,
                        ""minimumOrderQuantity"": 2500,
                        ""sourcePartId"": ""V36:1790_07275166"",
                        ""Prices"": {
                          ""resaleList"": [
                            {
                              ""displayPrice"": ""0.4299"",
                              ""price"": 0.4299,
                              ""minQty"": 2500,
                              ""maxQty"": 4999
                            },
                            {
                              ""displayPrice"": ""0.4281"",
                              ""price"": 0.4281,
                              ""minQty"": 5000,
                              ""maxQty"": 9999
                            },
                            {
                              ""displayPrice"": ""0.4238"",
                              ""price"": 0.4238,
                              ""minQty"": 10000,
                              ""maxQty"": 14999
                            },
                            {
                              ""displayPrice"": ""0.4196"",
                              ""price"": 0.4196,
                              ""minQty"": 15000,
                              ""maxQty"": 19999
                            },
                            {
                              ""displayPrice"": ""0.4154"",
                              ""price"": 0.4154,
                              ""minQty"": 20000,
                              ""maxQty"": 24999
                            },
                            {
                              ""displayPrice"": ""0.4112"",
                              ""price"": 0.4112,
                              ""minQty"": 25000,
                              ""maxQty"": 44999
                            },
                            {
                              ""displayPrice"": ""0.4071"",
                              ""price"": 0.4071,
                              ""minQty"": 45000,
                              ""maxQty"": 49999
                            },
                            {
                              ""displayPrice"": ""0.403"",
                              ""price"": 0.403,
                              ""minQty"": 50000,
                              ""maxQty"": 99999
                            },
                            {
                              ""displayPrice"": ""0.399"",
                              ""price"": 0.399,
                              ""minQty"": 100000,
                              ""maxQty"": 99999999
                            }
                          ]
                        },
                        ""Availability"": [
                          {
                            ""fohQty"": 0,
                            ""availabilityCd"": ""QUOTE"",
                            ""availabilityMessage"": ""Call For Quote"",
                            ""pipeline"": []
                          }
                        ],
                        ""customerSpecificPricing"": [],
                        ""customerSpecificInventory"": [],
                        ""dateCode"": """",
                        ""resources"": [
                          {
                            ""type"": ""detail"",
                            ""uri"": ""https://www.arrow.com/en/products/lm358mxnopb/texas-instruments?region=nac""
                          },
                          {
                            ""type"": ""add_to_cart"",
                            ""uri"": ""https://www.arrow.com/en/products/lm358mxnopb/texas-instruments?region=nac""
                          }
                        ],
                        ""inStock"": false,
                        ""mfrLeadTime"": 6,
                        ""tariffFlag"": false,
                        ""shipsFrom"": ""United States of America"",
                        ""shipsIn"": ""Ships today"",
                        ""arrowLeadTime"": """",
                        ""isNcnr"": false,
                        ""isNpi"": false,
                        ""productCode"": ""LM358MX/NOPB"",
                        ""eccnCode"": ""EAR99"",
                        ""htsCode"": ""8542.33.00.01"",
                        ""containerType"": """"
                      },
                      {
                        ""packSize"": 1,
                        ""minimumOrderQuantity"": 1,
                        ""sourcePartId"": ""V72:2272_07275166"",
                        ""Prices"": {
                          ""resaleList"": [
                            {
                              ""displayPrice"": ""0.9624"",
                              ""price"": 0.9624,
                              ""minQty"": 1,
                              ""maxQty"": 9
                            },
                            {
                              ""displayPrice"": ""0.8582"",
                              ""price"": 0.8582,
                              ""minQty"": 10,
                              ""maxQty"": 24
                            },
                            {
                              ""displayPrice"": ""0.8076"",
                              ""price"": 0.8076,
                              ""minQty"": 25,
                              ""maxQty"": 99
                            },
                            {
                              ""displayPrice"": ""0.6805"",
                              ""price"": 0.6805,
                              ""minQty"": 100,
                              ""maxQty"": 249
                            },
                            {
                              ""displayPrice"": ""0.6372"",
                              ""price"": 0.6372,
                              ""minQty"": 250,
                              ""maxQty"": 499
                            },
                            {
                              ""displayPrice"": ""0.5647"",
                              ""price"": 0.5647,
                              ""minQty"": 500,
                              ""maxQty"": 999
                            },
                            {
                              ""displayPrice"": ""0.4479"",
                              ""price"": 0.4479,
                              ""minQty"": 1000,
                              ""maxQty"": 999999999
                            }
                          ]
                        },
                        ""Availability"": [
                          {
                            ""fohQty"": 1530,
                            ""availabilityCd"": ""CSTRP"",
                            ""availabilityMessage"": ""Cut Strips"",
                            ""pipeline"": []
                          }
                        ],
                        ""customerSpecificPricing"": [],
                        ""customerSpecificInventory"": [],
                        ""dateCode"": ""2121"",
                        ""resources"": [
                          {
                            ""type"": ""detail"",
                            ""uri"": ""https://www.arrow.com/en/products/lm358mxnopb/texas-instruments?region=nac""
                          },
                          {
                            ""type"": ""add_to_cart"",
                            ""uri"": ""https://www.arrow.com/en/products/lm358mxnopb/texas-instruments?region=nac""
                          }
                        ],
                        ""inStock"": true,
                        ""mfrLeadTime"": 6,
                        ""tariffFlag"": false,
                        ""shipsFrom"": ""United States of America"",
                        ""shipsIn"": ""Ships today"",
                        ""arrowLeadTime"": """",
                        ""isNcnr"": false,
                        ""isNpi"": false,
                        ""productCode"": ""LM358MX/NOPB"",
                        ""eccnCode"": ""EAR99"",
                        ""htsCode"": ""8542.33.00.01"",
                        ""countryOfOrigin"": ""MY"",
                        ""containerType"": ""Cut Strips""
                      }
                    ]
                  }
                ]
              }
            ]
          },
          ""hasDatasheet"": false,
          ""categoryName"": ""Operational Amplifiers - Op Amps"",
          ""status"": ""Active""
        },
        {
          ""itemId"": 2403233,
          ""partNum"": ""LM358AP"",
          ""manufacturer"": {
            ""mfrCd"": ""TI"",
            ""mfrName"": ""Texas Instruments""
          },
          ""desc"": ""Op Amp Dual GP ±16V/32V 8-Pin PDIP Tube"",
          ""packageType"": """",
          ""resources"": [
            {
              ""type"": ""image_small"",
              ""uri"": ""https://static6.arrow.com/aropdfconversion/arrowimages/177418fca087bcbb3aa970adba9f405b16ef5ff5/p0008a.jpg""
            },
            {
              ""type"": ""image_large"",
              ""uri"": ""https://static6.arrow.com/aropdfconversion/arrowimages/46549d20734c25e404bb7adab9cb9bc841bbd7c6/p0008a.jpg""
            },
            {
              ""type"": ""cloud_part_detail"",
              ""uri"": ""https://www.arrow.com/en/products/lm358ap/texas-instruments""
            },
            {
              ""type"": ""api_part_detail"",
              ""uri"": ""http://api.arrow.com/itemservice/v2/en/detail?mfrCd=TI&partNum=LM358AP""
            },
            {
              ""type"": ""part_detail"",
              ""uri"": ""http://api.arrow.com/itemservice/v2/en/detail?pkey=QP-TBeHb5VSMzEShpzUQ-2mjCmUQue4_LPOuKOD5_FI""
            }
          ],
          ""EnvData"": {
            ""compliance"": [
              {
                ""displayLabel"": ""eurohs"",
                ""displayValue"": ""Compliant""
              },
              {
                ""displayLabel"": ""cnrohs"",
                ""displayValue"": ""Compliant""
              }
            ]
          },
          ""InvOrg"": {
            ""webSites"": [
              {
                ""code"": ""Verical.com"",
                ""name"": ""Verical.com"",
                ""sources"": [
                  {
                    ""currency"": ""USD"",
                    ""sourceCd"": ""VERICAL"",
                    ""displayName"": ""Verical"",
                    ""sourceParts"": [
                      {
                        ""packSize"": 1,
                        ""minimumOrderQuantity"": 51,
                        ""sourcePartId"": ""62506874"",
                        ""Prices"": {
                          ""resaleList"": [
                            {
                              ""displayPrice"": ""0.4205"",
                              ""price"": 0.4205,
                              ""minQty"": 51,
                              ""maxQty"": 99
                            },
                            {
                              ""displayPrice"": ""0.3745"",
                              ""price"": 0.3745,
                              ""minQty"": 100,
                              ""maxQty"": 499
                            },
                            {
                              ""displayPrice"": ""0.3481"",
                              ""price"": 0.3481,
                              ""minQty"": 500,
                              ""maxQty"": 999
                            },
                            {
                              ""displayPrice"": ""0.3239"",
                              ""price"": 0.3239,
                              ""minQty"": 1000,
                              ""maxQty"": 2499
                            },
                            {
                              ""displayPrice"": ""0.303"",
                              ""price"": 0.303,
                              ""minQty"": 2500,
                              ""maxQty"": 9999
                            },
                            {
                              ""displayPrice"": ""0.3"",
                              ""price"": 0.3,
                              ""minQty"": 10000,
                              ""maxQty"": 999999999
                            }
                          ]
                        },
                        ""Availability"": [
                          {
                            ""fohQty"": 1237,
                            ""availabilityCd"": ""INSTK"",
                            ""availabilityMessage"": ""In Stock"",
                            ""pipeline"": []
                          }
                        ],
                        ""customerSpecificPricing"": [],
                        ""customerSpecificInventory"": [],
                        ""dateCode"": ""2152"",
                        ""resources"": [
                          {
                            ""type"": ""detail"",
                            ""uri"": ""https://www.verical.com/pd/national-semiconductor-amplifier---operational-LM358AP-691103""
                          },
                          {
                            ""type"": ""add_to_cart"",
                            ""uri"": ""https://www.verical.com/pd/national-semiconductor-amplifier---operational-LM358AP-691103""
                          }
                        ],
                        ""inStock"": true,
                        ""mfrLeadTime"": 0,
                        ""tariffFlag"": false,
                        ""arrowLeadTime"": """",
                        ""isNcnr"": false,
                        ""isNpi"": false,
                        ""productCode"": ""LM358AP"",
                        ""eccnCode"": ""EAR99"",
                        ""htsCode"": ""8542.33.00.01"",
                        ""countryOfOrigin"": ""MX"",
                        ""containerType"": """"
                      }
                    ]
                  }
                ]
              },
              {
                ""code"": ""arrow.com"",
                ""name"": ""arrow.com"",
                ""sources"": [
                  {
                    ""currency"": ""USD"",
                    ""sourceCd"": ""ACNA"",
                    ""displayName"": ""Arrow Americas"",
                    ""sourceParts"": [
                      {
                        ""packSize"": 1000,
                        ""minimumOrderQuantity"": 1000,
                        ""sourcePartId"": ""V36:1790_07275147"",
                        ""Prices"": {
                          ""resaleList"": [
                            {
                              ""displayPrice"": ""0.115"",
                              ""price"": 0.115,
                              ""minQty"": 1000,
                              ""maxQty"": 9999
                            },
                            {
                              ""displayPrice"": ""0.1139"",
                              ""price"": 0.1139,
                              ""minQty"": 10000,
                              ""maxQty"": 99999999
                            }
                          ]
                        },
                        ""Availability"": [
                          {
                            ""fohQty"": 0,
                            ""availabilityCd"": ""QUOTE"",
                            ""availabilityMessage"": ""Call For Quote"",
                            ""pipeline"": []
                          }
                        ],
                        ""customerSpecificPricing"": [],
                        ""customerSpecificInventory"": [],
                        ""dateCode"": """",
                        ""resources"": [
                          {
                            ""type"": ""detail"",
                            ""uri"": ""https://www.arrow.com/en/products/lm358ap/texas-instruments?region=nac""
                          },
                          {
                            ""type"": ""add_to_cart"",
                            ""uri"": ""https://www.arrow.com/en/products/lm358ap/texas-instruments?region=nac""
                          }
                        ],
                        ""inStock"": false,
                        ""mfrLeadTime"": 6,
                        ""tariffFlag"": false,
                        ""shipsFrom"": ""United States of America"",
                        ""shipsIn"": ""Ships today"",
                        ""arrowLeadTime"": """",
                        ""isNcnr"": false,
                        ""isNpi"": false,
                        ""productCode"": ""LM358AP"",
                        ""eccnCode"": ""EAR99"",
                        ""htsCode"": ""8542.33.00.01"",
                        ""containerType"": """"
                      },
                      {
                        ""packSize"": 1000,
                        ""minimumOrderQuantity"": 1000,
                        ""sourcePartId"": ""V39:1801_07275147"",
                        ""Prices"": {
                          ""resaleList"": [
                            {
                              ""displayPrice"": ""0.115"",
                              ""price"": 0.115,
                              ""minQty"": 1000,
                              ""maxQty"": 9999
                            },
                            {
                              ""displayPrice"": ""0.1139"",
                              ""price"": 0.1139,
                              ""minQty"": 10000,
                              ""maxQty"": 99999999
                            }
                          ]
                        },
                        ""Availability"": [
                          {
                            ""fohQty"": 0,
                            ""availabilityCd"": ""QUOTE"",
                            ""availabilityMessage"": ""Call For Quote"",
                            ""pipeline"": []
                          }
                        ],
                        ""customerSpecificPricing"": [],
                        ""customerSpecificInventory"": [],
                        ""dateCode"": """",
                        ""resources"": [
                          {
                            ""type"": ""detail"",
                            ""uri"": ""https://www.arrow.com/en/products/lm358ap/texas-instruments?region=nac""
                          },
                          {
                            ""type"": ""add_to_cart"",
                            ""uri"": ""https://www.arrow.com/en/products/lm358ap/texas-instruments?region=nac""
                          }
                        ],
                        ""inStock"": false,
                        ""mfrLeadTime"": 6,
                        ""tariffFlag"": false,
                        ""shipsFrom"": ""United States of America"",
                        ""shipsIn"": ""Ships today"",
                        ""arrowLeadTime"": """",
                        ""isNcnr"": false,
                        ""isNpi"": false,
                        ""productCode"": ""LM358AP"",
                        ""eccnCode"": ""EAR99"",
                        ""htsCode"": ""8542.33.00.01"",
                        ""containerType"": """"
                      },
                      {
                        ""packSize"": 1000,
                        ""minimumOrderQuantity"": 1000,
                        ""sourcePartId"": ""V38:1800_07275147"",
                        ""Prices"": {
                          ""resaleList"": [
                            {
                              ""displayPrice"": ""0.115"",
                              ""price"": 0.115,
                              ""minQty"": 1000,
                              ""maxQty"": 9999
                            },
                            {
                              ""displayPrice"": ""0.1139"",
                              ""price"": 0.1139,
                              ""minQty"": 10000,
                              ""maxQty"": 99999999
                            }
                          ]
                        },
                        ""Availability"": [
                          {
                            ""fohQty"": 0,
                            ""availabilityCd"": ""QUOTE"",
                            ""availabilityMessage"": ""Call For Quote"",
                            ""pipeline"": []
                          }
                        ],
                        ""customerSpecificPricing"": [],
                        ""customerSpecificInventory"": [],
                        ""dateCode"": """",
                        ""resources"": [
                          {
                            ""type"": ""detail"",
                            ""uri"": ""https://www.arrow.com/en/products/lm358ap/texas-instruments?region=nac""
                          },
                          {
                            ""type"": ""add_to_cart"",
                            ""uri"": ""https://www.arrow.com/en/products/lm358ap/texas-instruments?region=nac""
                          }
                        ],
                        ""inStock"": false,
                        ""mfrLeadTime"": 6,
                        ""tariffFlag"": false,
                        ""shipsFrom"": ""United States of America"",
                        ""shipsIn"": ""Ships today"",
                        ""arrowLeadTime"": """",
                        ""isNcnr"": false,
                        ""isNpi"": false,
                        ""productCode"": ""LM358AP"",
                        ""eccnCode"": ""EAR99"",
                        ""htsCode"": ""8542.33.00.01"",
                        ""containerType"": """"
                      },
                      {
                        ""packSize"": 1,
                        ""minimumOrderQuantity"": 1,
                        ""sourcePartId"": ""V99:2348_07275147"",
                        ""Prices"": {
                          ""resaleList"": [
                            {
                              ""displayPrice"": ""0.4347"",
                              ""price"": 0.4347,
                              ""minQty"": 1,
                              ""maxQty"": 9
                            },
                            {
                              ""displayPrice"": ""0.4192"",
                              ""price"": 0.4192,
                              ""minQty"": 10,
                              ""maxQty"": 99
                            },
                            {
                              ""displayPrice"": ""0.3739"",
                              ""price"": 0.3739,
                              ""minQty"": 100,
                              ""maxQty"": 499
                            },
                            {
                              ""displayPrice"": ""0.3474"",
                              ""price"": 0.3474,
                              ""minQty"": 500,
                              ""maxQty"": 999
                            },
                            {
                              ""displayPrice"": ""0.3237"",
                              ""price"": 0.3237,
                              ""minQty"": 1000,
                              ""maxQty"": 2499
                            },
                            {
                              ""displayPrice"": ""0.2161"",
                              ""price"": 0.2161,
                              ""minQty"": 2500,
                              ""maxQty"": 9999
                            },
                            {
                              ""displayPrice"": ""0.2111"",
                              ""price"": 0.2111,
                              ""minQty"": 10000,
                              ""maxQty"": 99999999
                            }
                          ]
                        },
                        ""Availability"": [
                          {
                            ""fohQty"": 1237,
                            ""availabilityCd"": ""INSTK"",
                            ""availabilityMessage"": ""In Stock"",
                            ""pipeline"": []
                          }
                        ],
                        ""customerSpecificPricing"": [],
                        ""customerSpecificInventory"": [],
                        ""dateCode"": ""2152"",
                        ""resources"": [
                          {
                            ""type"": ""detail"",
                            ""uri"": ""https://www.arrow.com/en/products/lm358ap/texas-instruments?region=nac""
                          },
                          {
                            ""type"": ""add_to_cart"",
                            ""uri"": ""https://www.arrow.com/en/products/lm358ap/texas-instruments?region=nac""
                          }
                        ],
                        ""inStock"": true,
                        ""mfrLeadTime"": 6,
                        ""tariffFlag"": false,
                        ""shipsFrom"": ""United States of America"",
                        ""shipsIn"": ""Ships today"",
                        ""arrowLeadTime"": """",
                        ""isNcnr"": false,
                        ""isNpi"": false,
                        ""productCode"": ""LM358AP"",
                        ""eccnCode"": ""EAR99"",
                        ""htsCode"": ""8542.33.00.01"",
                        ""countryOfOrigin"": ""MX"",
                        ""containerType"": """"
                      }
                    ]
                  }
                ]
              }
            ]
          },
          ""hasDatasheet"": false,
          ""categoryName"": ""Operational Amplifiers - Op Amps"",
          ""status"": ""Active""
        },
        {
          ""itemId"": 50282908,
          ""partNum"": ""LM358H/NOPB"",
          ""manufacturer"": {
            ""mfrCd"": ""TI"",
            ""mfrName"": ""Texas Instruments""
          },
          ""desc"": ""Op Amp Dual Low Power Amplifier ±16V/32V 8-Pin TO-99 Tube"",
          ""packageType"": """",
          ""resources"": [
            {
              ""type"": ""image_small"",
              ""uri"": ""https://static6.arrow.com/aropdfconversion/arrowimages/3e249ab759b49b20f9d3285ec951ced3ef688063/lmc0008a.jpg""
            },
            {
              ""type"": ""image_large"",
              ""uri"": ""https://static6.arrow.com/aropdfconversion/arrowimages/e7fb29fd3b6e32730207f32d0ba286c2e7195e13/lmc0008a.jpg""
            },
            {
              ""type"": ""cloud_part_detail"",
              ""uri"": ""https://www.arrow.com/en/products/lm358hnopb/texas-instruments""
            },
            {
              ""type"": ""api_part_detail"",
              ""uri"": ""http://api.arrow.com/itemservice/v2/en/detail?mfrCd=TI&partNum=LM358H/NOPB""
            },
            {
              ""type"": ""part_detail"",
              ""uri"": ""http://api.arrow.com/itemservice/v2/en/detail?pkey=CL0ewuoh8H_OBi-_ie68oFO97oKUp2DM5bzPKNf8vQ8""
            }
          ],
          ""EnvData"": {
            ""compliance"": [
              {
                ""displayLabel"": ""eurohs"",
                ""displayValue"": ""Compliant""
              },
              {
                ""displayLabel"": ""cnrohs"",
                ""displayValue"": ""Compliant""
              }
            ]
          },
          ""InvOrg"": {
            ""webSites"": [
              {
                ""code"": ""Verical.com"",
                ""name"": ""Verical.com"",
                ""sources"": [
                  {
                    ""currency"": ""USD"",
                    ""sourceCd"": ""VERICAL"",
                    ""displayName"": ""Verical"",
                    ""sourceParts"": [
                      {
                        ""packSize"": 1,
                        ""minimumOrderQuantity"": 2,
                        ""sourcePartId"": ""35856769"",
                        ""Prices"": {
                          ""resaleList"": [
                            {
                              ""displayPrice"": ""5.252"",
                              ""price"": 5.252,
                              ""minQty"": 2,
                              ""maxQty"": 9
                            },
                            {
                              ""displayPrice"": ""5.158"",
                              ""price"": 5.158,
                              ""minQty"": 10,
                              ""maxQty"": 24
                            },
                            {
                              ""displayPrice"": ""5.124"",
                              ""price"": 5.124,
                              ""minQty"": 25,
                              ""maxQty"": 99
                            },
                            {
                              ""displayPrice"": ""5.069"",
                              ""price"": 5.069,
                              ""minQty"": 100,
                              ""maxQty"": 999999999
                            }
                          ]
                        },
                        ""Availability"": [
                          {
                            ""fohQty"": 192,
                            ""availabilityCd"": ""INSTK"",
                            ""availabilityMessage"": ""In Stock"",
                            ""pipeline"": []
                          }
                        ],
                        ""customerSpecificPricing"": [],
                        ""customerSpecificInventory"": [],
                        ""dateCode"": ""1839"",
                        ""resources"": [
                          {
                            ""type"": ""detail"",
                            ""uri"": ""https://www.verical.com/pd/national-semiconductor-amplifier---operational-LM358H-NOPB-55399""
                          },
                          {
                            ""type"": ""add_to_cart"",
                            ""uri"": ""https://www.verical.com/pd/national-semiconductor-amplifier---operational-LM358H-NOPB-55399""
                          }
                        ],
                        ""inStock"": true,
                        ""mfrLeadTime"": 0,
                        ""tariffFlag"": false,
                        ""arrowLeadTime"": """",
                        ""isNcnr"": false,
                        ""isNpi"": false,
                        ""productCode"": ""LM358H/NOPB"",
                        ""eccnCode"": ""EAR99"",
                        ""htsCode"": ""8542.33.00.01"",
                        ""countryOfOrigin"": ""PH"",
                        ""containerType"": """"
                      }
                    ]
                  }
                ]
              },
              {
                ""code"": ""arrow.com"",
                ""name"": ""arrow.com"",
                ""sources"": [
                  {
                    ""currency"": ""USD"",
                    ""sourceCd"": ""ACNA"",
                    ""displayName"": ""Arrow Americas"",
                    ""sourceParts"": [
                      {
                        ""packSize"": 1,
                        ""minimumOrderQuantity"": 1,
                        ""sourcePartId"": ""V99:2348_07275162"",
                        ""Prices"": {
                          ""resaleList"": [
                            {
                              ""displayPrice"": ""5.252"",
                              ""price"": 5.252,
                              ""minQty"": 1,
                              ""maxQty"": 9
                            },
                            {
                              ""displayPrice"": ""5.158"",
                              ""price"": 5.158,
                              ""minQty"": 10,
                              ""maxQty"": 24
                            },
                            {
                              ""displayPrice"": ""5.106"",
                              ""price"": 5.106,
                              ""minQty"": 25,
                              ""maxQty"": 99
                            },
                            {
                              ""displayPrice"": ""5.069"",
                              ""price"": 5.069,
                              ""minQty"": 100,
                              ""maxQty"": 99999999
                            }
                          ]
                        },
                        ""Availability"": [
                          {
                            ""fohQty"": 192,
                            ""availabilityCd"": ""INSTK"",
                            ""availabilityMessage"": ""In Stock"",
                            ""pipeline"": []
                          }
                        ],
                        ""customerSpecificPricing"": [],
                        ""customerSpecificInventory"": [],
                        ""dateCode"": ""1839"",
                        ""resources"": [
                          {
                            ""type"": ""detail"",
                            ""uri"": ""https://www.arrow.com/en/products/lm358hnopb/texas-instruments?region=nac""
                          },
                          {
                            ""type"": ""add_to_cart"",
                            ""uri"": ""https://www.arrow.com/en/products/lm358hnopb/texas-instruments?region=nac""
                          }
                        ],
                        ""inStock"": true,
                        ""mfrLeadTime"": 6,
                        ""tariffFlag"": false,
                        ""shipsFrom"": ""United States of America"",
                        ""shipsIn"": ""Ships today"",
                        ""arrowLeadTime"": """",
                        ""isNcnr"": false,
                        ""isNpi"": false,
                        ""productCode"": ""LM358H/NOPB"",
                        ""eccnCode"": ""EAR99"",
                        ""htsCode"": ""8542.33.00.01"",
                        ""countryOfOrigin"": ""PH"",
                        ""containerType"": """"
                      },
                      {
                        ""packSize"": 500,
                        ""minimumOrderQuantity"": 500,
                        ""sourcePartId"": ""V36:1790_07275162"",
                        ""Prices"": {
                          ""resaleList"": [
                            {
                              ""displayPrice"": ""5.486"",
                              ""price"": 5.486,
                              ""minQty"": 500,
                              ""maxQty"": 999
                            },
                            {
                              ""displayPrice"": ""5.278"",
                              ""price"": 5.278,
                              ""minQty"": 1000,
                              ""maxQty"": 99999999
                            }
                          ]
                        },
                        ""Availability"": [
                          {
                            ""fohQty"": 0,
                            ""availabilityCd"": ""QUOTE"",
                            ""availabilityMessage"": ""Call For Quote"",
                            ""pipeline"": []
                          }
                        ],
                        ""customerSpecificPricing"": [],
                        ""customerSpecificInventory"": [],
                        ""dateCode"": """",
                        ""resources"": [
                          {
                            ""type"": ""detail"",
                            ""uri"": ""https://www.arrow.com/en/products/lm358hnopb/texas-instruments?region=nac""
                          },
                          {
                            ""type"": ""add_to_cart"",
                            ""uri"": ""https://www.arrow.com/en/products/lm358hnopb/texas-instruments?region=nac""
                          }
                        ],
                        ""inStock"": false,
                        ""mfrLeadTime"": 6,
                        ""tariffFlag"": false,
                        ""shipsFrom"": ""United States of America"",
                        ""shipsIn"": ""Ships today"",
                        ""arrowLeadTime"": """",
                        ""isNcnr"": false,
                        ""isNpi"": false,
                        ""productCode"": ""LM358H/NOPB"",
                        ""eccnCode"": ""EAR99"",
                        ""htsCode"": ""8542.33.00.01"",
                        ""containerType"": """"
                      },
                      {
                        ""packSize"": 500,
                        ""minimumOrderQuantity"": 500,
                        ""sourcePartId"": ""V39:1801_07275162"",
                        ""Prices"": {
                          ""resaleList"": [
                            {
                              ""displayPrice"": ""5.486"",
                              ""price"": 5.486,
                              ""minQty"": 500,
                              ""maxQty"": 999
                            },
                            {
                              ""displayPrice"": ""5.278"",
                              ""price"": 5.278,
                              ""minQty"": 1000,
                              ""maxQty"": 99999999
                            }
                          ]
                        },
                        ""Availability"": [
                          {
                            ""fohQty"": 0,
                            ""availabilityCd"": ""QUOTE"",
                            ""availabilityMessage"": ""Call For Quote"",
                            ""pipeline"": []
                          }
                        ],
                        ""customerSpecificPricing"": [],
                        ""customerSpecificInventory"": [],
                        ""dateCode"": """",
                        ""resources"": [
                          {
                            ""type"": ""detail"",
                            ""uri"": ""https://www.arrow.com/en/products/lm358hnopb/texas-instruments?region=nac""
                          },
                          {
                            ""type"": ""add_to_cart"",
                            ""uri"": ""https://www.arrow.com/en/products/lm358hnopb/texas-instruments?region=nac""
                          }
                        ],
                        ""inStock"": false,
                        ""mfrLeadTime"": 6,
                        ""tariffFlag"": false,
                        ""shipsFrom"": ""United States of America"",
                        ""shipsIn"": ""Ships today"",
                        ""arrowLeadTime"": """",
                        ""isNcnr"": false,
                        ""isNpi"": false,
                        ""productCode"": ""LM358H/NOPB"",
                        ""eccnCode"": ""EAR99"",
                        ""htsCode"": ""8542.33.00.01"",
                        ""containerType"": """"
                      }
                    ]
                  }
                ]
              }
            ]
          },
          ""hasDatasheet"": false,
          ""categoryName"": """",
          ""status"": ""Active""
        },
        {
          ""itemId"": 1504775,
          ""partNum"": ""LM358PWR"",
          ""manufacturer"": {
            ""mfrCd"": ""TI"",
            ""mfrName"": ""Texas Instruments""
          },
          ""desc"": ""Op Amp Dual GP ±16V/32V 8-Pin TSSOP T/R"",
          ""packageType"": """",
          ""resources"": [
            {
              ""type"": ""image_small"",
              ""uri"": ""https://static6.arrow.com/aropdfconversion/arrowimages/3b520cab184e09c762a1279a9bd0defeeee93b3e/pw0008a.jpg""
            },
            {
              ""type"": ""image_large"",
              ""uri"": ""https://static6.arrow.com/aropdfconversion/arrowimages/345870321d113bab9c76ab327f30600d9a04705d/pw0008a.jpg""
            },
            {
              ""type"": ""cloud_part_detail"",
              ""uri"": ""https://www.arrow.com/en/products/lm358pwr/texas-instruments""
            },
            {
              ""type"": ""api_part_detail"",
              ""uri"": ""http://api.arrow.com/itemservice/v2/en/detail?mfrCd=TI&partNum=LM358PWR""
            },
            {
              ""type"": ""part_detail"",
              ""uri"": ""http://api.arrow.com/itemservice/v2/en/detail?pkey=5fNzhRT6w-cMSmNYaeBLzDJNZG7opDEzhftE-ExCco8""
            }
          ],
          ""EnvData"": {
            ""compliance"": [
              {
                ""displayLabel"": ""eurohs"",
                ""displayValue"": ""Compliant""
              },
              {
                ""displayLabel"": ""cnrohs"",
                ""displayValue"": ""Compliant""
              }
            ]
          },
          ""InvOrg"": {
            ""webSites"": [
              {
                ""code"": ""Verical.com"",
                ""name"": ""Verical.com"",
                ""sources"": [
                  {
                    ""currency"": ""USD"",
                    ""sourceCd"": ""VERICAL"",
                    ""displayName"": ""Verical"",
                    ""sourceParts"": [
                      {
                        ""packSize"": 2000,
                        ""minimumOrderQuantity"": 2000,
                        ""sourcePartId"": ""65070341"",
                        ""Prices"": {
                          ""resaleList"": [
                            {
                              ""displayPrice"": ""0.1569"",
                              ""price"": 0.1569,
                              ""minQty"": 2000,
                              ""maxQty"": 3999
                            },
                            {
                              ""displayPrice"": ""0.1554"",
                              ""price"": 0.1554,
                              ""minQty"": 4000,
                              ""maxQty"": 999999999
                            }
                          ]
                        },
                        ""Availability"": [
                          {
                            ""fohQty"": 52000,
                            ""availabilityCd"": ""INSTK"",
                            ""availabilityMessage"": ""In Stock"",
                            ""pipeline"": []
                          }
                        ],
                        ""customerSpecificPricing"": [],
                        ""customerSpecificInventory"": [],
                        ""dateCode"": ""2236"",
                        ""resources"": [
                          {
                            ""type"": ""detail"",
                            ""uri"": ""https://www.verical.com/pd/national-semiconductor-amplifier---operational-LM358PWR-10105525""
                          },
                          {
                            ""type"": ""add_to_cart"",
                            ""uri"": ""https://www.verical.com/pd/national-semiconductor-amplifier---operational-LM358PWR-10105525""
                          }
                        ],
                        ""inStock"": true,
                        ""mfrLeadTime"": 0,
                        ""tariffFlag"": false,
                        ""arrowLeadTime"": """",
                        ""isNcnr"": false,
                        ""isNpi"": false,
                        ""productCode"": ""LM358PWR"",
                        ""eccnCode"": ""EAR99"",
                        ""htsCode"": ""8542.33.00.01"",
                        ""countryOfOrigin"": ""MY"",
                        ""containerType"": """"
                      },
                      {
                        ""packSize"": 1,
                        ""minimumOrderQuantity"": 78,
                        ""sourcePartId"": ""65057271"",
                        ""Prices"": {
                          ""resaleList"": [
                            {
                              ""displayPrice"": ""0.3666"",
                              ""price"": 0.3666,
                              ""minQty"": 78,
                              ""maxQty"": 99
                            },
                            {
                              ""displayPrice"": ""0.2859"",
                              ""price"": 0.2859,
                              ""minQty"": 100,
                              ""maxQty"": 249
                            },
                            {
                              ""displayPrice"": ""0.2829"",
                              ""price"": 0.2829,
                              ""minQty"": 250,
                              ""maxQty"": 499
                            },
                            {
                              ""displayPrice"": ""0.2502"",
                              ""price"": 0.2502,
                              ""minQty"": 500,
                              ""maxQty"": 999
                            },
                            {
                              ""displayPrice"": ""0.2477"",
                              ""price"": 0.2477,
                              ""minQty"": 1000,
                              ""maxQty"": 999999999
                            }
                          ]
                        },
                        ""Availability"": [
                          {
                            ""fohQty"": 2000,
                            ""availabilityCd"": ""INSTK"",
                            ""availabilityMessage"": ""In Stock"",
                            ""pipeline"": []
                          }
                        ],
                        ""customerSpecificPricing"": [],
                        ""customerSpecificInventory"": [],
                        ""dateCode"": ""2236"",
                        ""resources"": [
                          {
                            ""type"": ""detail"",
                            ""uri"": ""https://www.verical.com/pd/national-semiconductor-amplifier---operational-LM358PWR-10105525""
                          },
                          {
                            ""type"": ""add_to_cart"",
                            ""uri"": ""https://www.verical.com/pd/national-semiconductor-amplifier---operational-LM358PWR-10105525""
                          }
                        ],
                        ""inStock"": true,
                        ""mfrLeadTime"": 0,
                        ""tariffFlag"": false,
                        ""arrowLeadTime"": """",
                        ""isNcnr"": false,
                        ""isNpi"": false,
                        ""productCode"": ""LM358PWR"",
                        ""eccnCode"": ""EAR99"",
                        ""htsCode"": ""8542.33.00.01"",
                        ""countryOfOrigin"": ""MY"",
                        ""containerType"": """"
                      }
                    ]
                  }
                ]
              },
              {
                ""code"": ""arrow.com"",
                ""name"": ""arrow.com"",
                ""sources"": [
                  {
                    ""currency"": ""USD"",
                    ""sourceCd"": ""ACNA"",
                    ""displayName"": ""Arrow Americas"",
                    ""sourceParts"": [
                      {
                        ""packSize"": 2000,
                        ""minimumOrderQuantity"": 2000,
                        ""sourcePartId"": ""V36:1790_07275174"",
                        ""Prices"": {
                          ""resaleList"": [
                            {
                              ""displayPrice"": ""0.0923"",
                              ""price"": 0.0923,
                              ""minQty"": 2000,
                              ""maxQty"": 3999
                            },
                            {
                              ""displayPrice"": ""0.0914"",
                              ""price"": 0.0914,
                              ""minQty"": 4000,
                              ""maxQty"": 99999999
                            }
                          ]
                        },
                        ""Availability"": [
                          {
                            ""fohQty"": 0,
                            ""availabilityCd"": ""QUOTE"",
                            ""availabilityMessage"": ""Call For Quote"",
                            ""pipeline"": []
                          }
                        ],
                        ""customerSpecificPricing"": [],
                        ""customerSpecificInventory"": [],
                        ""dateCode"": """",
                        ""resources"": [
                          {
                            ""type"": ""detail"",
                            ""uri"": ""https://www.arrow.com/en/products/lm358pwr/texas-instruments?region=nac""
                          },
                          {
                            ""type"": ""add_to_cart"",
                            ""uri"": ""https://www.arrow.com/en/products/lm358pwr/texas-instruments?region=nac""
                          }
                        ],
                        ""inStock"": false,
                        ""mfrLeadTime"": 6,
                        ""tariffFlag"": false,
                        ""shipsFrom"": ""United States of America"",
                        ""shipsIn"": ""Ships today"",
                        ""arrowLeadTime"": """",
                        ""isNcnr"": false,
                        ""isNpi"": false,
                        ""productCode"": ""LM358PWR"",
                        ""eccnCode"": ""EAR99"",
                        ""htsCode"": ""8542.33.00.01"",
                        ""containerType"": """"
                      },
                      {
                        ""packSize"": 1,
                        ""minimumOrderQuantity"": 1,
                        ""sourcePartId"": ""V72:2272_07275174"",
                        ""Prices"": {
                          ""resaleList"": [
                            {
                              ""displayPrice"": ""0.4228"",
                              ""price"": 0.4228,
                              ""minQty"": 1,
                              ""maxQty"": 9
                            },
                            {
                              ""displayPrice"": ""0.3489"",
                              ""price"": 0.3489,
                              ""minQty"": 10,
                              ""maxQty"": 24
                            },
                            {
                              ""displayPrice"": ""0.3256"",
                              ""price"": 0.3256,
                              ""minQty"": 25,
                              ""maxQty"": 99
                            },
                            {
                              ""displayPrice"": ""0.2354"",
                              ""price"": 0.2354,
                              ""minQty"": 100,
                              ""maxQty"": 249
                            },
                            {
                              ""displayPrice"": ""0.233"",
                              ""price"": 0.233,
                              ""minQty"": 250,
                              ""maxQty"": 499
                            },
                            {
                              ""displayPrice"": ""0.1847"",
                              ""price"": 0.1847,
                              ""minQty"": 500,
                              ""maxQty"": 999
                            },
                            {
                              ""displayPrice"": ""0.1828"",
                              ""price"": 0.1828,
                              ""minQty"": 1000,
                              ""maxQty"": 999999999
                            }
                          ]
                        },
                        ""Availability"": [
                          {
                            ""fohQty"": 2000,
                            ""availabilityCd"": ""CSTRP"",
                            ""availabilityMessage"": ""Cut Strips"",
                            ""pipeline"": [
                              {
                                ""delivery"": ""21-MAR-2023"",
                                ""quantity"": 52000
                              }
                            ]
                          }
                        ],
                        ""customerSpecificPricing"": [],
                        ""customerSpecificInventory"": [],
                        ""dateCode"": ""2236"",
                        ""resources"": [
                          {
                            ""type"": ""detail"",
                            ""uri"": ""https://www.arrow.com/en/products/lm358pwr/texas-instruments?region=nac""
                          },
                          {
                            ""type"": ""add_to_cart"",
                            ""uri"": ""https://www.arrow.com/en/products/lm358pwr/texas-instruments?region=nac""
                          }
                        ],
                        ""inStock"": true,
                        ""mfrLeadTime"": 6,
                        ""tariffFlag"": false,
                        ""shipsFrom"": ""United States of America"",
                        ""shipsIn"": ""Ships today"",
                        ""arrowLeadTime"": """",
                        ""isNcnr"": false,
                        ""isNpi"": false,
                        ""productCode"": ""LM358PWR"",
                        ""eccnCode"": ""EAR99"",
                        ""htsCode"": ""8542.33.00.01"",
                        ""countryOfOrigin"": ""MY"",
                        ""containerType"": ""Cut Strips""
                      },
                      {
                        ""packSize"": 2000,
                        ""minimumOrderQuantity"": 2000,
                        ""sourcePartId"": ""V39:1801_07275174"",
                        ""Prices"": {
                          ""resaleList"": [
                            {
                              ""displayPrice"": ""0.1568"",
                              ""price"": 0.1568,
                              ""minQty"": 2000,
                              ""maxQty"": 3999
                            },
                            {
                              ""displayPrice"": ""0.1099"",
                              ""price"": 0.1099,
                              ""minQty"": 4000,
                              ""maxQty"": 99999999
                            }
                          ]
                        },
                        ""Availability"": [
                          {
                            ""fohQty"": 52000,
                            ""availabilityCd"": ""INSTK"",
                            ""availabilityMessage"": ""In Stock"",
                            ""pipeline"": []
                          }
                        ],
                        ""customerSpecificPricing"": [],
                        ""customerSpecificInventory"": [],
                        ""dateCode"": ""2236"",
                        ""resources"": [
                          {
                            ""type"": ""detail"",
                            ""uri"": ""https://www.arrow.com/en/products/lm358pwr/texas-instruments?region=nac""
                          },
                          {
                            ""type"": ""add_to_cart"",
                            ""uri"": ""https://www.arrow.com/en/products/lm358pwr/texas-instruments?region=nac""
                          }
                        ],
                        ""inStock"": true,
                        ""mfrLeadTime"": 6,
                        ""tariffFlag"": false,
                        ""shipsFrom"": ""United States of America"",
                        ""shipsIn"": ""Ships today"",
                        ""arrowLeadTime"": """",
                        ""isNcnr"": false,
                        ""isNpi"": false,
                        ""productCode"": ""LM358PWR"",
                        ""eccnCode"": ""EAR99"",
                        ""htsCode"": ""8542.33.00.01"",
                        ""countryOfOrigin"": ""MY"",
                        ""containerType"": """"
                      }
                    ]
                  }
                ]
              }
            ]
          },
          ""hasDatasheet"": false,
          ""categoryName"": ""Operational Amplifiers - Op Amps"",
          ""status"": ""Active""
        },
        {
          ""itemId"": 2058050,
          ""partNum"": ""LM358APWR"",
          ""manufacturer"": {
            ""mfrCd"": ""TI"",
            ""mfrName"": ""Texas Instruments""
          },
          ""desc"": ""Op Amp Dual GP ±16V/32V 8-Pin TSSOP T/R"",
          ""packageType"": """",
          ""resources"": [
            {
              ""type"": ""image_small"",
              ""uri"": ""https://static6.arrow.com/aropdfconversion/arrowimages/3b520cab184e09c762a1279a9bd0defeeee93b3e/pw0008a.jpg""
            },
            {
              ""type"": ""image_large"",
              ""uri"": ""https://static6.arrow.com/aropdfconversion/arrowimages/345870321d113bab9c76ab327f30600d9a04705d/pw0008a.jpg""
            },
            {
              ""type"": ""cloud_part_detail"",
              ""uri"": ""https://www.arrow.com/en/products/lm358apwr/texas-instruments""
            },
            {
              ""type"": ""api_part_detail"",
              ""uri"": ""http://api.arrow.com/itemservice/v2/en/detail?mfrCd=TI&partNum=LM358APWR""
            },
            {
              ""type"": ""part_detail"",
              ""uri"": ""http://api.arrow.com/itemservice/v2/en/detail?pkey=Q-Rzw0vCXKAUPyQlAYBW1NBE1ZLCzih1P1l-sCmIhlU""
            }
          ],
          ""EnvData"": {
            ""compliance"": [
              {
                ""displayLabel"": ""eurohs"",
                ""displayValue"": ""Compliant""
              },
              {
                ""displayLabel"": ""cnrohs"",
                ""displayValue"": ""Compliant""
              }
            ]
          },
          ""InvOrg"": {
            ""webSites"": [
              {
                ""code"": ""Verical.com"",
                ""name"": ""Verical.com"",
                ""sources"": [
                  {
                    ""currency"": ""USD"",
                    ""sourceCd"": ""VERICAL"",
                    ""displayName"": ""Verical"",
                    ""sourceParts"": [
                      {
                        ""packSize"": 1,
                        ""minimumOrderQuantity"": 59,
                        ""sourcePartId"": ""63346759"",
                        ""Prices"": {
                          ""resaleList"": [
                            {
                              ""displayPrice"": ""0.3948"",
                              ""price"": 0.3948,
                              ""minQty"": 59,
                              ""maxQty"": 99
                            },
                            {
                              ""displayPrice"": ""0.3534"",
                              ""price"": 0.3534,
                              ""minQty"": 100,
                              ""maxQty"": 249
                            },
                            {
                              ""displayPrice"": ""0.2407"",
                              ""price"": 0.2407,
                              ""minQty"": 250,
                              ""maxQty"": 499
                            },
                            {
                              ""displayPrice"": ""0.2326"",
                              ""price"": 0.2326,
                              ""minQty"": 500,
                              ""maxQty"": 999
                            },
                            {
                              ""displayPrice"": ""0.2302"",
                              ""price"": 0.2302,
                              ""minQty"": 1000,
                              ""maxQty"": 999999999
                            }
                          ]
                        },
                        ""Availability"": [
                          {
                            ""fohQty"": 1200,
                            ""availabilityCd"": ""INSTK"",
                            ""availabilityMessage"": ""In Stock"",
                            ""pipeline"": []
                          }
                        ],
                        ""customerSpecificPricing"": [],
                        ""customerSpecificInventory"": [],
                        ""dateCode"": ""2233"",
                        ""resources"": [
                          {
                            ""type"": ""detail"",
                            ""uri"": ""https://www.verical.com/pd/national-semiconductor-amplifier---operational-LM358APWR-9650727""
                          },
                          {
                            ""type"": ""add_to_cart"",
                            ""uri"": ""https://www.verical.com/pd/national-semiconductor-amplifier---operational-LM358APWR-9650727""
                          }
                        ],
                        ""inStock"": true,
                        ""mfrLeadTime"": 0,
                        ""tariffFlag"": false,
                        ""arrowLeadTime"": """",
                        ""isNcnr"": false,
                        ""isNpi"": false,
                        ""productCode"": ""LM358APWR"",
                        ""eccnCode"": ""EAR99"",
                        ""htsCode"": ""8542.33.00.01"",
                        ""countryOfOrigin"": ""MY"",
                        ""containerType"": """"
                      },
                      {
                        ""packSize"": 2000,
                        ""minimumOrderQuantity"": 2000,
                        ""sourcePartId"": ""65144278"",
                        ""Prices"": {
                          ""resaleList"": [
                            {
                              ""displayPrice"": ""0.3373"",
                              ""price"": 0.3373,
                              ""minQty"": 2000,
                              ""maxQty"": 3999
                            },
                            {
                              ""displayPrice"": ""0.3339"",
                              ""price"": 0.3339,
                              ""minQty"": 4000,
                              ""maxQty"": 7999
                            },
                            {
                              ""displayPrice"": ""0.3305"",
                              ""price"": 0.3305,
                              ""minQty"": 8000,
                              ""maxQty"": 9999
                            },
                            {
                              ""displayPrice"": ""0.3271"",
                              ""price"": 0.3271,
                              ""minQty"": 10000,
                              ""maxQty"": 19999
                            },
                            {
                              ""displayPrice"": ""0.3238"",
                              ""price"": 0.3238,
                              ""minQty"": 20000,
                              ""maxQty"": 23999
                            },
                            {
                              ""displayPrice"": ""0.3205"",
                              ""price"": 0.3205,
                              ""minQty"": 24000,
                              ""maxQty"": 999999999
                            }
                          ]
                        },
                        ""Availability"": [
                          {
                            ""fohQty"": 4000,
                            ""availabilityCd"": ""INSTK"",
                            ""availabilityMessage"": ""In Stock"",
                            ""pipeline"": []
                          }
                        ],
                        ""customerSpecificPricing"": [],
                        ""customerSpecificInventory"": [],
                        ""dateCode"": ""2225"",
                        ""resources"": [
                          {
                            ""type"": ""detail"",
                            ""uri"": ""https://www.verical.com/pd/national-semiconductor-amplifier---operational-LM358APWR-9650727""
                          },
                          {
                            ""type"": ""add_to_cart"",
                            ""uri"": ""https://www.verical.com/pd/national-semiconductor-amplifier---operational-LM358APWR-9650727""
                          }
                        ],
                        ""inStock"": true,
                        ""mfrLeadTime"": 0,
                        ""tariffFlag"": false,
                        ""arrowLeadTime"": """",
                        ""isNcnr"": false,
                        ""isNpi"": false,
                        ""productCode"": ""LM358APWR"",
                        ""eccnCode"": ""EAR99"",
                        ""htsCode"": ""8542.33.00.01"",
                        ""countryOfOrigin"": ""MY"",
                        ""containerType"": """"
                      }
                    ]
                  }
                ]
              },
              {
                ""code"": ""arrow.com"",
                ""name"": ""arrow.com"",
                ""sources"": [
                  {
                    ""currency"": ""USD"",
                    ""sourceCd"": ""ACNA"",
                    ""displayName"": ""Arrow Americas"",
                    ""sourceParts"": [
                      {
                        ""packSize"": 1,
                        ""minimumOrderQuantity"": 1,
                        ""sourcePartId"": ""V72:2272_07275151"",
                        ""Prices"": {
                          ""resaleList"": [
                            {
                              ""displayPrice"": ""0.4633"",
                              ""price"": 0.4633,
                              ""minQty"": 1,
                              ""maxQty"": 9
                            },
                            {
                              ""displayPrice"": ""0.3757"",
                              ""price"": 0.3757,
                              ""minQty"": 10,
                              ""maxQty"": 24
                            },
                            {
                              ""displayPrice"": ""0.3379"",
                              ""price"": 0.3379,
                              ""minQty"": 25,
                              ""maxQty"": 99
                            },
                            {
                              ""displayPrice"": ""0.2543"",
                              ""price"": 0.2543,
                              ""minQty"": 100,
                              ""maxQty"": 249
                            },
                            {
                              ""displayPrice"": ""0.2288"",
                              ""price"": 0.2288,
                              ""minQty"": 250,
                              ""maxQty"": 499
                            },
                            {
                              ""displayPrice"": ""0.1886"",
                              ""price"": 0.1886,
                              ""minQty"": 500,
                              ""maxQty"": 999
                            },
                            {
                              ""displayPrice"": ""0.148"",
                              ""price"": 0.148,
                              ""minQty"": 1000,
                              ""maxQty"": 999999999
                            }
                          ]
                        },
                        ""Availability"": [
                          {
                            ""fohQty"": 1200,
                            ""availabilityCd"": ""CSTRP"",
                            ""availabilityMessage"": ""Cut Strips"",
                            ""pipeline"": [
                              {
                                ""delivery"": ""21-MAR-2023"",
                                ""quantity"": 4000
                              }
                            ]
                          }
                        ],
                        ""customerSpecificPricing"": [],
                        ""customerSpecificInventory"": [],
                        ""dateCode"": ""2233"",
                        ""resources"": [
                          {
                            ""type"": ""detail"",
                            ""uri"": ""https://www.arrow.com/en/products/lm358apwr/texas-instruments?region=nac""
                          },
                          {
                            ""type"": ""add_to_cart"",
                            ""uri"": ""https://www.arrow.com/en/products/lm358apwr/texas-instruments?region=nac""
                          }
                        ],
                        ""inStock"": true,
                        ""mfrLeadTime"": 6,
                        ""tariffFlag"": false,
                        ""shipsFrom"": ""United States of America"",
                        ""shipsIn"": ""Ships today"",
                        ""arrowLeadTime"": """",
                        ""isNcnr"": false,
                        ""isNpi"": false,
                        ""productCode"": ""LM358APWR"",
                        ""eccnCode"": ""EAR99"",
                        ""htsCode"": ""8542.33.00.01"",
                        ""countryOfOrigin"": ""MY"",
                        ""containerType"": ""Cut Strips""
                      },
                      {
                        ""packSize"": 2000,
                        ""minimumOrderQuantity"": 2000,
                        ""sourcePartId"": ""V36:1790_07275151"",
                        ""Prices"": {
                          ""resaleList"": [
                            {
                              ""displayPrice"": ""0.0787"",
                              ""price"": 0.0787,
                              ""minQty"": 2000,
                              ""maxQty"": 9999
                            },
                            {
                              ""displayPrice"": ""0.0746"",
                              ""price"": 0.0746,
                              ""minQty"": 10000,
                              ""maxQty"": 23999
                            },
                            {
                              ""displayPrice"": ""0.0713"",
                              ""price"": 0.0713,
                              ""minQty"": 24000,
                              ""maxQty"": 49999
                            },
                            {
                              ""displayPrice"": ""0.0696"",
                              ""price"": 0.0696,
                              ""minQty"": 50000,
                              ""maxQty"": 99999999
                            }
                          ]
                        },
                        ""Availability"": [
                          {
                            ""fohQty"": 0,
                            ""availabilityCd"": ""QUOTE"",
                            ""availabilityMessage"": ""Call For Quote"",
                            ""pipeline"": []
                          }
                        ],
                        ""customerSpecificPricing"": [],
                        ""customerSpecificInventory"": [],
                        ""dateCode"": """",
                        ""resources"": [
                          {
                            ""type"": ""detail"",
                            ""uri"": ""https://www.arrow.com/en/products/lm358apwr/texas-instruments?region=nac""
                          },
                          {
                            ""type"": ""add_to_cart"",
                            ""uri"": ""https://www.arrow.com/en/products/lm358apwr/texas-instruments?region=nac""
                          }
                        ],
                        ""inStock"": false,
                        ""mfrLeadTime"": 6,
                        ""tariffFlag"": false,
                        ""shipsFrom"": ""United States of America"",
                        ""shipsIn"": ""Ships today"",
                        ""arrowLeadTime"": """",
                        ""isNcnr"": false,
                        ""isNpi"": false,
                        ""productCode"": ""LM358APWR"",
                        ""eccnCode"": ""EAR99"",
                        ""htsCode"": ""8542.33.00.01"",
                        ""containerType"": """"
                      },
                      {
                        ""packSize"": 2000,
                        ""minimumOrderQuantity"": 2000,
                        ""sourcePartId"": ""V39:1801_07275151"",
                        ""Prices"": {
                          ""resaleList"": [
                            {
                              ""displayPrice"": ""0.152"",
                              ""price"": 0.152,
                              ""minQty"": 2000,
                              ""maxQty"": 5999
                            },
                            {
                              ""displayPrice"": ""0.1467"",
                              ""price"": 0.1467,
                              ""minQty"": 6000,
                              ""maxQty"": 9999
                            },
                            {
                              ""displayPrice"": ""0.1098"",
                              ""price"": 0.1098,
                              ""minQty"": 10000,
                              ""maxQty"": 49999
                            },
                            {
                              ""displayPrice"": ""0.1087"",
                              ""price"": 0.1087,
                              ""minQty"": 50000,
                              ""maxQty"": 99999999
                            }
                          ]
                        },
                        ""Availability"": [
                          {
                            ""fohQty"": 4000,
                            ""availabilityCd"": ""INSTK"",
                            ""availabilityMessage"": ""In Stock"",
                            ""pipeline"": []
                          }
                        ],
                        ""customerSpecificPricing"": [],
                        ""customerSpecificInventory"": [],
                        ""dateCode"": ""2225"",
                        ""resources"": [
                          {
                            ""type"": ""detail"",
                            ""uri"": ""https://www.arrow.com/en/products/lm358apwr/texas-instruments?region=nac""
                          },
                          {
                            ""type"": ""add_to_cart"",
                            ""uri"": ""https://www.arrow.com/en/products/lm358apwr/texas-instruments?region=nac""
                          }
                        ],
                        ""inStock"": true,
                        ""mfrLeadTime"": 6,
                        ""tariffFlag"": false,
                        ""shipsFrom"": ""United States of America"",
                        ""shipsIn"": ""Ships today"",
                        ""arrowLeadTime"": """",
                        ""isNcnr"": false,
                        ""isNpi"": false,
                        ""productCode"": ""LM358APWR"",
                        ""eccnCode"": ""EAR99"",
                        ""htsCode"": ""8542.33.00.01"",
                        ""countryOfOrigin"": ""MY"",
                        ""containerType"": """"
                      }
                    ]
                  }
                ]
              }
            ]
          },
          ""hasDatasheet"": false,
          ""categoryName"": ""Operational Amplifiers - Op Amps"",
          ""status"": ""Active""
        },
        {
          ""itemId"": 15906764,
          ""partNum"": ""LM358AM"",
          ""manufacturer"": {
            ""mfrCd"": ""TI"",
            ""mfrName"": ""Texas Instruments""
          },
          ""desc"": ""Op Amp Dual Low Power Amplifier ±16V/32V 8-Pin SOIC Tube"",
          ""packageType"": """",
          ""resources"": [
            {
              ""type"": ""image_small"",
              ""uri"": ""https://static6.arrow.com/aropdfconversion/arrowimages/3f304a36ac6e93000334d1a2cf03ec6bd35c733b/d0008a.jpg""
            },
            {
              ""type"": ""image_large"",
              ""uri"": ""https://static6.arrow.com/aropdfconversion/arrowimages/e5533f271a03578b303601b99b62d8a183cba66a/d0008a.jpg""
            },
            {
              ""type"": ""cloud_part_detail"",
              ""uri"": ""https://www.arrow.com/en/products/lm358am/texas-instruments""
            },
            {
              ""type"": ""api_part_detail"",
              ""uri"": ""http://api.arrow.com/itemservice/v2/en/detail?mfrCd=TI&partNum=LM358AM""
            },
            {
              ""type"": ""part_detail"",
              ""uri"": ""http://api.arrow.com/itemservice/v2/en/detail?pkey=02WBAvZ7SHpJHWWAuZJ8nUgLWVj-0oghEb2Q17QGRpM""
            }
          ],
          ""EnvData"": {
            ""compliance"": [
              {
                ""displayLabel"": ""eurohs"",
                ""displayValue"": ""Not Compliant""
              },
              {
                ""displayLabel"": ""cnrohs"",
                ""displayValue"": ""Not Compliant""
              }
            ]
          },
          ""InvOrg"": {
            ""webSites"": [
              {
                ""code"": ""Verical.com"",
                ""name"": ""Verical.com"",
                ""sources"": [
                  {
                    ""currency"": ""USD"",
                    ""sourceCd"": ""VERICAL"",
                    ""displayName"": ""Verical"",
                    ""sourceParts"": [
                      {
                        ""packSize"": 1,
                        ""minimumOrderQuantity"": 11,
                        ""sourcePartId"": ""62167637"",
                        ""Prices"": {
                          ""resaleList"": [
                            {
                              ""displayPrice"": ""0.682"",
                              ""price"": 0.682,
                              ""minQty"": 11,
                              ""maxQty"": 999999999
                            }
                          ]
                        },
                        ""Availability"": [
                          {
                            ""fohQty"": 349,
                            ""availabilityCd"": ""INSTK"",
                            ""availabilityMessage"": ""In Stock"",
                            ""pipeline"": []
                          }
                        ],
                        ""customerSpecificPricing"": [],
                        ""customerSpecificInventory"": [],
                        ""dateCode"": ""2140"",
                        ""resources"": [
                          {
                            ""type"": ""detail"",
                            ""uri"": ""https://www.verical.com/pd/national-semiconductor-amplifier---operational-LM358AM-55403""
                          },
                          {
                            ""type"": ""add_to_cart"",
                            ""uri"": ""https://www.verical.com/pd/national-semiconductor-amplifier---operational-LM358AM-55403""
                          }
                        ],
                        ""inStock"": true,
                        ""mfrLeadTime"": 0,
                        ""tariffFlag"": false,
                        ""arrowLeadTime"": """",
                        ""isNcnr"": false,
                        ""isNpi"": false,
                        ""productCode"": ""LM358AM"",
                        ""eccnCode"": ""EAR99"",
                        ""htsCode"": ""8542.33.00.01"",
                        ""countryOfOrigin"": ""MY"",
                        ""containerType"": """"
                      },
                      {
                        ""packSize"": 95,
                        ""minimumOrderQuantity"": 95,
                        ""sourcePartId"": ""64984753"",
                        ""Prices"": {
                          ""resaleList"": [
                            {
                              ""displayPrice"": ""0.8853"",
                              ""price"": 0.8853,
                              ""minQty"": 95,
                              ""maxQty"": 284
                            },
                            {
                              ""displayPrice"": ""0.8438"",
                              ""price"": 0.8438,
                              ""minQty"": 285,
                              ""maxQty"": 569
                            },
                            {
                              ""displayPrice"": ""0.7738"",
                              ""price"": 0.7738,
                              ""minQty"": 570,
                              ""maxQty"": 1044
                            },
                            {
                              ""displayPrice"": ""0.6461"",
                              ""price"": 0.6461,
                              ""minQty"": 1045,
                              ""maxQty"": 2564
                            },
                            {
                              ""displayPrice"": ""0.6294"",
                              ""price"": 0.6294,
                              ""minQty"": 2565,
                              ""maxQty"": 999999999
                            }
                          ]
                        },
                        ""Availability"": [
                          {
                            ""fohQty"": 285,
                            ""availabilityCd"": ""INSTK"",
                            ""availabilityMessage"": ""In Stock"",
                            ""pipeline"": []
                          }
                        ],
                        ""customerSpecificPricing"": [],
                        ""customerSpecificInventory"": [],
                        ""dateCode"": ""2230"",
                        ""resources"": [
                          {
                            ""type"": ""detail"",
                            ""uri"": ""https://www.verical.com/pd/national-semiconductor-amplifier---operational-LM358AM-55403""
                          },
                          {
                            ""type"": ""add_to_cart"",
                            ""uri"": ""https://www.verical.com/pd/national-semiconductor-amplifier---operational-LM358AM-55403""
                          }
                        ],
                        ""inStock"": true,
                        ""mfrLeadTime"": 0,
                        ""tariffFlag"": false,
                        ""arrowLeadTime"": """",
                        ""isNcnr"": false,
                        ""isNpi"": false,
                        ""productCode"": ""LM358AM"",
                        ""eccnCode"": ""EAR99"",
                        ""htsCode"": ""8542.33.00.01"",
                        ""countryOfOrigin"": ""MY"",
                        ""containerType"": """"
                      }
                    ]
                  }
                ]
              },
              {
                ""code"": ""arrow.com"",
                ""name"": ""arrow.com"",
                ""sources"": [
                  {
                    ""currency"": ""USD"",
                    ""sourceCd"": ""ACNA"",
                    ""displayName"": ""Arrow Americas"",
                    ""sourceParts"": [
                      {
                        ""packSize"": 1,
                        ""minimumOrderQuantity"": 1,
                        ""sourcePartId"": ""V99:2348_07275142"",
                        ""Prices"": {
                          ""resaleList"": [
                            {
                              ""displayPrice"": ""1.5123"",
                              ""price"": 1.5123,
                              ""minQty"": 1,
                              ""maxQty"": 9
                            },
                            {
                              ""displayPrice"": ""1.3549"",
                              ""price"": 1.3549,
                              ""minQty"": 10,
                              ""maxQty"": 24
                            },
                            {
                              ""displayPrice"": ""1.0676"",
                              ""price"": 1.0676,
                              ""minQty"": 25,
                              ""maxQty"": 99
                            },
                            {
                              ""displayPrice"": ""1.0391"",
                              ""price"": 1.0391,
                              ""minQty"": 100,
                              ""maxQty"": 249
                            },
                            {
                              ""displayPrice"": ""0.9617"",
                              ""price"": 0.9617,
                              ""minQty"": 250,
                              ""maxQty"": 499
                            },
                            {
                              ""displayPrice"": ""0.8457"",
                              ""price"": 0.8457,
                              ""minQty"": 500,
                              ""maxQty"": 999
                            },
                            {
                              ""displayPrice"": ""0.6632"",
                              ""price"": 0.6632,
                              ""minQty"": 1000,
                              ""maxQty"": 2499
                            },
                            {
                              ""displayPrice"": ""0.6272"",
                              ""price"": 0.6272,
                              ""minQty"": 2500,
                              ""maxQty"": 99999999
                            }
                          ]
                        },
                        ""Availability"": [
                          {
                            ""fohQty"": 349,
                            ""availabilityCd"": ""INSTK"",
                            ""availabilityMessage"": ""In Stock"",
                            ""pipeline"": [
                              {
                                ""delivery"": ""21-MAR-2023"",
                                ""quantity"": 285
                              }
                            ]
                          }
                        ],
                        ""customerSpecificPricing"": [],
                        ""customerSpecificInventory"": [],
                        ""dateCode"": ""2140"",
                        ""resources"": [
                          {
                            ""type"": ""detail"",
                            ""uri"": ""https://www.arrow.com/en/products/lm358am/texas-instruments?region=nac""
                          },
                          {
                            ""type"": ""add_to_cart"",
                            ""uri"": ""https://www.arrow.com/en/products/lm358am/texas-instruments?region=nac""
                          }
                        ],
                        ""inStock"": true,
                        ""mfrLeadTime"": 6,
                        ""tariffFlag"": false,
                        ""shipsFrom"": ""United States of America"",
                        ""shipsIn"": ""Ships today"",
                        ""arrowLeadTime"": """",
                        ""isNcnr"": false,
                        ""isNpi"": false,
                        ""productCode"": ""LM358AM"",
                        ""eccnCode"": ""EAR99"",
                        ""htsCode"": ""8542.33.00.01"",
                        ""countryOfOrigin"": ""MY"",
                        ""containerType"": """"
                      },
                      {
                        ""packSize"": 95,
                        ""minimumOrderQuantity"": 570,
                        ""sourcePartId"": ""V36:1790_07275142"",
                        ""Prices"": {
                          ""resaleList"": [
                            {
                              ""displayPrice"": ""0.6193"",
                              ""price"": 0.6193,
                              ""minQty"": 570,
                              ""maxQty"": 1044
                            },
                            {
                              ""displayPrice"": ""0.5479"",
                              ""price"": 0.5479,
                              ""minQty"": 1045,
                              ""maxQty"": 2564
                            },
                            {
                              ""displayPrice"": ""0.5319"",
                              ""price"": 0.5319,
                              ""minQty"": 2565,
                              ""maxQty"": 99999999
                            }
                          ]
                        },
                        ""Availability"": [
                          {
                            ""fohQty"": 0,
                            ""availabilityCd"": ""QUOTE"",
                            ""availabilityMessage"": ""Call For Quote"",
                            ""pipeline"": []
                          }
                        ],
                        ""customerSpecificPricing"": [],
                        ""customerSpecificInventory"": [],
                        ""dateCode"": """",
                        ""resources"": [
                          {
                            ""type"": ""detail"",
                            ""uri"": ""https://www.arrow.com/en/products/lm358am/texas-instruments?region=nac""
                          },
                          {
                            ""type"": ""add_to_cart"",
                            ""uri"": ""https://www.arrow.com/en/products/lm358am/texas-instruments?region=nac""
                          }
                        ],
                        ""inStock"": false,
                        ""mfrLeadTime"": 6,
                        ""tariffFlag"": false,
                        ""shipsFrom"": ""United States of America"",
                        ""shipsIn"": ""Ships today"",
                        ""arrowLeadTime"": """",
                        ""isNcnr"": false,
                        ""isNpi"": false,
                        ""productCode"": ""LM358AM"",
                        ""eccnCode"": ""EAR99"",
                        ""htsCode"": ""8542.33.00.01"",
                        ""containerType"": """"
                      },
                      {
                        ""packSize"": 95,
                        ""minimumOrderQuantity"": 95,
                        ""sourcePartId"": ""V39:1801_07275142"",
                        ""Prices"": {
                          ""resaleList"": [
                            {
                              ""displayPrice"": ""0.8468"",
                              ""price"": 0.8468,
                              ""minQty"": 95,
                              ""maxQty"": 284
                            },
                            {
                              ""displayPrice"": ""0.7739"",
                              ""price"": 0.7739,
                              ""minQty"": 285,
                              ""maxQty"": 569
                            },
                            {
                              ""displayPrice"": ""0.6463"",
                              ""price"": 0.6463,
                              ""minQty"": 570,
                              ""maxQty"": 1044
                            },
                            {
                              ""displayPrice"": ""0.6144"",
                              ""price"": 0.6144,
                              ""minQty"": 1045,
                              ""maxQty"": 2564
                            },
                            {
                              ""displayPrice"": ""0.6035"",
                              ""price"": 0.6035,
                              ""minQty"": 2565,
                              ""maxQty"": 99999999
                            }
                          ]
                        },
                        ""Availability"": [
                          {
                            ""fohQty"": 285,
                            ""availabilityCd"": ""INSTK"",
                            ""availabilityMessage"": ""In Stock"",
                            ""pipeline"": []
                          }
                        ],
                        ""customerSpecificPricing"": [],
                        ""customerSpecificInventory"": [],
                        ""dateCode"": ""2230"",
                        ""resources"": [
                          {
                            ""type"": ""detail"",
                            ""uri"": ""https://www.arrow.com/en/products/lm358am/texas-instruments?region=nac""
                          },
                          {
                            ""type"": ""add_to_cart"",
                            ""uri"": ""https://www.arrow.com/en/products/lm358am/texas-instruments?region=nac""
                          }
                        ],
                        ""inStock"": true,
                        ""mfrLeadTime"": 6,
                        ""tariffFlag"": false,
                        ""shipsFrom"": ""United States of America"",
                        ""shipsIn"": ""Ships today"",
                        ""arrowLeadTime"": """",
                        ""isNcnr"": false,
                        ""isNpi"": false,
                        ""productCode"": ""LM358AM"",
                        ""eccnCode"": ""EAR99"",
                        ""htsCode"": ""8542.33.00.01"",
                        ""countryOfOrigin"": ""MY"",
                        ""containerType"": """"
                      }
                    ]
                  }
                ]
              }
            ]
          },
          ""hasDatasheet"": false,
          ""categoryName"": ""Operational Amplifiers - Op Amps"",
          ""status"": ""NRND""
        },
        {
          ""itemId"": 2392171,
          ""partNum"": ""LM358PSR"",
          ""manufacturer"": {
            ""mfrCd"": ""TI"",
            ""mfrName"": ""Texas Instruments""
          },
          ""desc"": ""Op Amp Dual GP ±16V/32V 8-Pin SOP T/R"",
          ""packageType"": """",
          ""resources"": [
            {
              ""type"": ""image_small"",
              ""uri"": ""https://static6.arrow.com/aropdfconversion/arrowimages/fb89249abc6bd9b2bffdb362b608665c5f4e8efb/8-soic.jpg""
            },
            {
              ""type"": ""image_large"",
              ""uri"": ""https://static6.arrow.com/aropdfconversion/arrowimages/e49802024078fb0e23dc19492e6f16445927eba/8-soic.jpg""
            },
            {
              ""type"": ""cloud_part_detail"",
              ""uri"": ""https://www.arrow.com/en/products/lm358psr/texas-instruments""
            },
            {
              ""type"": ""api_part_detail"",
              ""uri"": ""http://api.arrow.com/itemservice/v2/en/detail?mfrCd=TI&partNum=LM358PSR""
            },
            {
              ""type"": ""part_detail"",
              ""uri"": ""http://api.arrow.com/itemservice/v2/en/detail?pkey=_FfwnWkw9_DlQpqPtCO8-x32VyYXzwCqE97Lz2Z21hk""
            }
          ],
          ""EnvData"": {
            ""compliance"": [
              {
                ""displayLabel"": ""eurohs"",
                ""displayValue"": ""Compliant""
              },
              {
                ""displayLabel"": ""cnrohs"",
                ""displayValue"": ""Compliant""
              }
            ]
          },
          ""InvOrg"": {
            ""webSites"": [
              {
                ""code"": ""Verical.com"",
                ""name"": ""Verical.com"",
                ""sources"": [
                  {
                    ""currency"": ""USD"",
                    ""sourceCd"": ""VERICAL"",
                    ""displayName"": ""Verical"",
                    ""sourceParts"": [
                      {
                        ""packSize"": 1,
                        ""minimumOrderQuantity"": 50,
                        ""sourcePartId"": ""62246302"",
                        ""Prices"": {
                          ""resaleList"": [
                            {
                              ""displayPrice"": ""0.4145"",
                              ""price"": 0.4145,
                              ""minQty"": 50,
                              ""maxQty"": 99
                            },
                            {
                              ""displayPrice"": ""0.2941"",
                              ""price"": 0.2941,
                              ""minQty"": 100,
                              ""maxQty"": 249
                            },
                            {
                              ""displayPrice"": ""0.2911"",
                              ""price"": 0.2911,
                              ""minQty"": 250,
                              ""maxQty"": 999999999
                            }
                          ]
                        },
                        ""Availability"": [
                          {
                            ""fohQty"": 485,
                            ""availabilityCd"": ""INSTK"",
                            ""availabilityMessage"": ""In Stock"",
                            ""pipeline"": []
                          }
                        ],
                        ""customerSpecificPricing"": [],
                        ""customerSpecificInventory"": [],
                        ""dateCode"": ""2201"",
                        ""resources"": [
                          {
                            ""type"": ""detail"",
                            ""uri"": ""https://www.verical.com/pd/national-semiconductor-amplifier---operational-LM358PSR-9650731""
                          },
                          {
                            ""type"": ""add_to_cart"",
                            ""uri"": ""https://www.verical.com/pd/national-semiconductor-amplifier---operational-LM358PSR-9650731""
                          }
                        ],
                        ""inStock"": true,
                        ""mfrLeadTime"": 0,
                        ""tariffFlag"": false,
                        ""arrowLeadTime"": """",
                        ""isNcnr"": false,
                        ""isNpi"": false,
                        ""productCode"": ""LM358PSR"",
                        ""eccnCode"": ""EAR99"",
                        ""htsCode"": ""8542.33.00.01"",
                        ""countryOfOrigin"": ""MY"",
                        ""containerType"": """"
                      }
                    ]
                  }
                ]
              },
              {
                ""code"": ""arrow.com"",
                ""name"": ""arrow.com"",
                ""sources"": [
                  {
                    ""currency"": ""USD"",
                    ""sourceCd"": ""ACNA"",
                    ""displayName"": ""Arrow Americas"",
                    ""sourceParts"": [
                      {
                        ""packSize"": 1,
                        ""minimumOrderQuantity"": 1,
                        ""sourcePartId"": ""V72:2272_07275171"",
                        ""Prices"": {
                          ""resaleList"": [
                            {
                              ""displayPrice"": ""0.4207"",
                              ""price"": 0.4207,
                              ""minQty"": 1,
                              ""maxQty"": 9
                            },
                            {
                              ""displayPrice"": ""0.3641"",
                              ""price"": 0.3641,
                              ""minQty"": 10,
                              ""maxQty"": 24
                            },
                            {
                              ""displayPrice"": ""0.363"",
                              ""price"": 0.363,
                              ""minQty"": 25,
                              ""maxQty"": 99
                            },
                            {
                              ""displayPrice"": ""0.2556"",
                              ""price"": 0.2556,
                              ""minQty"": 100,
                              ""maxQty"": 249
                            },
                            {
                              ""displayPrice"": ""0.2461"",
                              ""price"": 0.2461,
                              ""minQty"": 250,
                              ""maxQty"": 999999999
                            }
                          ]
                        },
                        ""Availability"": [
                          {
                            ""fohQty"": 485,
                            ""availabilityCd"": ""CSTRP"",
                            ""availabilityMessage"": ""Cut Strips"",
                            ""pipeline"": [
                              {
                                ""delivery"": ""16-AUG-2024"",
                                ""quantity"": 2000
                              }
                            ]
                          }
                        ],
                        ""customerSpecificPricing"": [],
                        ""customerSpecificInventory"": [],
                        ""dateCode"": ""2201"",
                        ""resources"": [
                          {
                            ""type"": ""detail"",
                            ""uri"": ""https://www.arrow.com/en/products/lm358psr/texas-instruments?region=nac""
                          },
                          {
                            ""type"": ""add_to_cart"",
                            ""uri"": ""https://www.arrow.com/en/products/lm358psr/texas-instruments?region=nac""
                          }
                        ],
                        ""inStock"": true,
                        ""mfrLeadTime"": 12,
                        ""tariffFlag"": false,
                        ""shipsFrom"": ""United States of America"",
                        ""shipsIn"": ""Ships today"",
                        ""arrowLeadTime"": """",
                        ""isNcnr"": false,
                        ""isNpi"": false,
                        ""productCode"": ""LM358PSR"",
                        ""eccnCode"": ""EAR99"",
                        ""htsCode"": ""8542.33.00.01"",
                        ""countryOfOrigin"": ""MY"",
                        ""containerType"": ""Cut Strips""
                      },
                      {
                        ""packSize"": 2000,
                        ""minimumOrderQuantity"": 2000,
                        ""sourcePartId"": ""V39:1801_07275171"",
                        ""Prices"": {
                          ""resaleList"": [
                            {
                              ""displayPrice"": ""0.1223"",
                              ""price"": 0.1223,
                              ""minQty"": 2000,
                              ""maxQty"": 99999999
                            }
                          ]
                        },
                        ""Availability"": [
                          {
                            ""fohQty"": 0,
                            ""availabilityCd"": ""QUOTE"",
                            ""availabilityMessage"": ""Call For Quote"",
                            ""pipeline"": [
                              {
                                ""delivery"": ""16-AUG-2024"",
                                ""quantity"": 2000
                              }
                            ]
                          }
                        ],
                        ""customerSpecificPricing"": [],
                        ""customerSpecificInventory"": [],
                        ""dateCode"": """",
                        ""resources"": [
                          {
                            ""type"": ""detail"",
                            ""uri"": ""https://www.arrow.com/en/products/lm358psr/texas-instruments?region=nac""
                          },
                          {
                            ""type"": ""add_to_cart"",
                            ""uri"": ""https://www.arrow.com/en/products/lm358psr/texas-instruments?region=nac""
                          }
                        ],
                        ""inStock"": false,
                        ""mfrLeadTime"": 12,
                        ""tariffFlag"": false,
                        ""shipsFrom"": ""United States of America"",
                        ""shipsIn"": ""Ships today"",
                        ""arrowLeadTime"": """",
                        ""isNcnr"": false,
                        ""isNpi"": false,
                        ""productCode"": ""LM358PSR"",
                        ""eccnCode"": ""EAR99"",
                        ""htsCode"": ""8542.33.00.01"",
                        ""containerType"": """"
                      },
                      {
                        ""packSize"": 2000,
                        ""minimumOrderQuantity"": 2000,
                        ""sourcePartId"": ""V36:1790_07275171"",
                        ""Prices"": {
                          ""resaleList"": [
                            {
                              ""displayPrice"": ""0.1223"",
                              ""price"": 0.1223,
                              ""minQty"": 2000,
                              ""maxQty"": 99999999
                            }
                          ]
                        },
                        ""Availability"": [
                          {
                            ""fohQty"": 0,
                            ""availabilityCd"": ""QUOTE"",
                            ""availabilityMessage"": ""Call For Quote"",
                            ""pipeline"": []
                          }
                        ],
                        ""customerSpecificPricing"": [],
                        ""customerSpecificInventory"": [],
                        ""dateCode"": """",
                        ""resources"": [
                          {
                            ""type"": ""detail"",
                            ""uri"": ""https://www.arrow.com/en/products/lm358psr/texas-instruments?region=nac""
                          },
                          {
                            ""type"": ""add_to_cart"",
                            ""uri"": ""https://www.arrow.com/en/products/lm358psr/texas-instruments?region=nac""
                          }
                        ],
                        ""inStock"": false,
                        ""mfrLeadTime"": 12,
                        ""tariffFlag"": false,
                        ""shipsFrom"": ""United States of America"",
                        ""shipsIn"": ""Ships today"",
                        ""arrowLeadTime"": """",
                        ""isNcnr"": false,
                        ""isNpi"": false,
                        ""productCode"": ""LM358PSR"",
                        ""eccnCode"": ""EAR99"",
                        ""htsCode"": ""8542.33.00.01"",
                        ""containerType"": """"
                      }
                    ]
                  }
                ]
              }
            ]
          },
          ""hasDatasheet"": false,
          ""categoryName"": ""Operational Amplifiers - Op Amps"",
          ""status"": ""Active""
        },
        {
          ""itemId"": 1399202,
          ""partNum"": ""LM358DR"",
          ""manufacturer"": {
            ""mfrCd"": ""TI"",
            ""mfrName"": ""Texas Instruments""
          },
          ""desc"": ""Op Amp Dual GP ±16V/32V 8-Pin SOIC T/R"",
          ""packageType"": """",
          ""resources"": [
            {
              ""type"": ""image_small"",
              ""uri"": ""https://static6.arrow.com/aropdfconversion/arrowimages/3f304a36ac6e93000334d1a2cf03ec6bd35c733b/d0008a.jpg""
            },
            {
              ""type"": ""image_large"",
              ""uri"": ""https://static6.arrow.com/aropdfconversion/arrowimages/e5533f271a03578b303601b99b62d8a183cba66a/d0008a.jpg""
            },
            {
              ""type"": ""cloud_part_detail"",
              ""uri"": ""https://www.arrow.com/en/products/lm358dr/texas-instruments""
            },
            {
              ""type"": ""api_part_detail"",
              ""uri"": ""http://api.arrow.com/itemservice/v2/en/detail?mfrCd=TI&partNum=LM358DR""
            },
            {
              ""type"": ""part_detail"",
              ""uri"": ""http://api.arrow.com/itemservice/v2/en/detail?pkey=r5Uhugj19PZwpL9gVECMdhfiLGwMMe-UFz8QfjJBaRM""
            }
          ],
          ""EnvData"": {
            ""compliance"": [
              {
                ""displayLabel"": ""eurohs"",
                ""displayValue"": ""Compliant""
              },
              {
                ""displayLabel"": ""cnrohs"",
                ""displayValue"": ""Compliant""
              }
            ]
          },
          ""InvOrg"": {
            ""webSites"": [
              {
                ""code"": ""Verical.com"",
                ""name"": ""Verical.com"",
                ""sources"": [
                  {
                    ""currency"": ""USD"",
                    ""sourceCd"": ""VERICAL"",
                    ""displayName"": ""Verical"",
                    ""sourceParts"": [
                      {
                        ""packSize"": 2500,
                        ""minimumOrderQuantity"": 2500,
                        ""sourcePartId"": ""66348393"",
                        ""Prices"": {
                          ""resaleList"": [
                            {
                              ""displayPrice"": ""0.0653"",
                              ""price"": 0.0653,
                              ""minQty"": 2500,
                              ""maxQty"": 99999999
                            }
                          ]
                        },
                        ""Availability"": [
                          {
                            ""fohQty"": 2500,
                            ""availabilityCd"": ""INSTK"",
                            ""availabilityMessage"": ""In Stock"",
                            ""pipeline"": []
                          }
                        ],
                        ""customerSpecificPricing"": [],
                        ""customerSpecificInventory"": [],
                        ""dateCode"": ""2236"",
                        ""resources"": [
                          {
                            ""type"": ""detail"",
                            ""uri"": ""https://www.verical.com/pd/national-semiconductor-amplifier---operational-LM358DR-635382""
                          },
                          {
                            ""type"": ""add_to_cart"",
                            ""uri"": ""https://www.verical.com/pd/national-semiconductor-amplifier---operational-LM358DR-635382""
                          }
                        ],
                        ""inStock"": true,
                        ""mfrLeadTime"": 0,
                        ""tariffFlag"": false,
                        ""arrowLeadTime"": """",
                        ""isNcnr"": false,
                        ""isNpi"": false,
                        ""productCode"": ""LM358DR"",
                        ""eccnCode"": ""EAR99"",
                        ""htsCode"": ""8542.33.00.01"",
                        ""countryOfOrigin"": ""MX"",
                        ""containerType"": """"
                      },
                      {
                        ""packSize"": 1,
                        ""minimumOrderQuantity"": 100,
                        ""sourcePartId"": ""60616932"",
                        ""Prices"": {
                          ""resaleList"": [
                            {
                              ""displayPrice"": ""0.4125"",
                              ""price"": 0.4125,
                              ""minQty"": 100,
                              ""maxQty"": 249
                            },
                            {
                              ""displayPrice"": ""0.4086"",
                              ""price"": 0.4086,
                              ""minQty"": 250,
                              ""maxQty"": 499
                            },
                            {
                              ""displayPrice"": ""0.4046"",
                              ""price"": 0.4046,
                              ""minQty"": 500,
                              ""maxQty"": 999
                            },
                            {
                              ""displayPrice"": ""0.4007"",
                              ""price"": 0.4007,
                              ""minQty"": 1000,
                              ""maxQty"": 999999999
                            }
                          ]
                        },
                        ""Availability"": [
                          {
                            ""fohQty"": 2403,
                            ""availabilityCd"": ""INSTK"",
                            ""availabilityMessage"": ""In Stock"",
                            ""pipeline"": []
                          }
                        ],
                        ""customerSpecificPricing"": [],
                        ""customerSpecificInventory"": [],
                        ""dateCode"": ""2140"",
                        ""resources"": [
                          {
                            ""type"": ""detail"",
                            ""uri"": ""https://www.verical.com/pd/national-semiconductor-amplifier---operational-LM358DR-635382""
                          },
                          {
                            ""type"": ""add_to_cart"",
                            ""uri"": ""https://www.verical.com/pd/national-semiconductor-amplifier---operational-LM358DR-635382""
                          }
                        ],
                        ""inStock"": true,
                        ""mfrLeadTime"": 0,
                        ""tariffFlag"": false,
                        ""arrowLeadTime"": """",
                        ""isNcnr"": false,
                        ""isNpi"": false,
                        ""productCode"": ""LM358DR"",
                        ""eccnCode"": ""EAR99"",
                        ""htsCode"": ""8542.33.00.01"",
                        ""countryOfOrigin"": ""MY"",
                        ""containerType"": """"
                      }
                    ]
                  }
                ]
              },
              {
                ""code"": ""arrow.com"",
                ""name"": ""arrow.com"",
                ""sources"": [
                  {
                    ""currency"": ""USD"",
                    ""sourceCd"": ""ACNA"",
                    ""displayName"": ""Arrow Americas"",
                    ""sourceParts"": [
                      {
                        ""packSize"": 2500,
                        ""minimumOrderQuantity"": 2500,
                        ""sourcePartId"": ""V36:1790_07275158"",
                        ""Prices"": {
                          ""resaleList"": [
                            {
                              ""displayPrice"": ""0.1149"",
                              ""price"": 0.1149,
                              ""minQty"": 2500,
                              ""maxQty"": 4999
                            },
                            {
                              ""displayPrice"": ""0.1137"",
                              ""price"": 0.1137,
                              ""minQty"": 5000,
                              ""maxQty"": 9999
                            },
                            {
                              ""displayPrice"": ""0.1126"",
                              ""price"": 0.1126,
                              ""minQty"": 10000,
                              ""maxQty"": 14999
                            },
                            {
                              ""displayPrice"": ""0.1115"",
                              ""price"": 0.1115,
                              ""minQty"": 15000,
                              ""maxQty"": 19999
                            },
                            {
                              ""displayPrice"": ""0.1104"",
                              ""price"": 0.1104,
                              ""minQty"": 20000,
                              ""maxQty"": 24999
                            },
                            {
                              ""displayPrice"": ""0.1093"",
                              ""price"": 0.1093,
                              ""minQty"": 25000,
                              ""maxQty"": 44999
                            },
                            {
                              ""displayPrice"": ""0.1082"",
                              ""price"": 0.1082,
                              ""minQty"": 45000,
                              ""maxQty"": 49999
                            },
                            {
                              ""displayPrice"": ""0.1071"",
                              ""price"": 0.1071,
                              ""minQty"": 50000,
                              ""maxQty"": 99999
                            },
                            {
                              ""displayPrice"": ""0.106"",
                              ""price"": 0.106,
                              ""minQty"": 100000,
                              ""maxQty"": 99999999
                            }
                          ]
                        },
                        ""Availability"": [
                          {
                            ""fohQty"": 0,
                            ""availabilityCd"": ""QUOTE"",
                            ""availabilityMessage"": ""Call For Quote"",
                            ""pipeline"": []
                          }
                        ],
                        ""customerSpecificPricing"": [],
                        ""customerSpecificInventory"": [],
                        ""dateCode"": ""2244"",
                        ""resources"": [
                          {
                            ""type"": ""detail"",
                            ""uri"": ""https://www.arrow.com/en/products/lm358dr/texas-instruments?region=nac""
                          },
                          {
                            ""type"": ""add_to_cart"",
                            ""uri"": ""https://www.arrow.com/en/products/lm358dr/texas-instruments?region=nac""
                          }
                        ],
                        ""inStock"": false,
                        ""mfrLeadTime"": 6,
                        ""tariffFlag"": false,
                        ""shipsFrom"": ""United States of America"",
                        ""shipsIn"": ""Ships today"",
                        ""arrowLeadTime"": """",
                        ""isNcnr"": false,
                        ""isNpi"": false,
                        ""productCode"": ""LM358DR"",
                        ""eccnCode"": ""EAR99"",
                        ""htsCode"": ""8542.33.00.01"",
                        ""countryOfOrigin"": ""MX"",
                        ""containerType"": """"
                      },
                      {
                        ""packSize"": 2500,
                        ""minimumOrderQuantity"": 2500,
                        ""sourcePartId"": ""V39:1801_07275158"",
                        ""Availability"": [
                          {
                            ""fohQty"": 2500,
                            ""availabilityCd"": ""QUOTE"",
                            ""availabilityMessage"": ""Call For Quote"",
                            ""pipeline"": [
                              {
                                ""delivery"": ""05-APR-2024"",
                                ""quantity"": 2500
                              }
                            ]
                          }
                        ],
                        ""customerSpecificPricing"": [],
                        ""customerSpecificInventory"": [],
                        ""dateCode"": ""2236"",
                        ""resources"": [
                          {
                            ""type"": ""detail"",
                            ""uri"": ""https://www.arrow.com/en/products/lm358dr/texas-instruments?region=nac""
                          },
                          {
                            ""type"": ""add_to_cart"",
                            ""uri"": ""https://www.arrow.com/en/products/lm358dr/texas-instruments?region=nac""
                          }
                        ],
                        ""inStock"": true,
                        ""mfrLeadTime"": 6,
                        ""tariffFlag"": false,
                        ""shipsFrom"": ""United States of America"",
                        ""shipsIn"": ""Ships today"",
                        ""arrowLeadTime"": """",
                        ""isNcnr"": false,
                        ""isNpi"": false,
                        ""productCode"": ""LM358DR"",
                        ""eccnCode"": ""EAR99"",
                        ""htsCode"": ""8542.33.00.01"",
                        ""countryOfOrigin"": ""MX"",
                        ""containerType"": """"
                      },
                      {
                        ""packSize"": 1,
                        ""minimumOrderQuantity"": 1,
                        ""sourcePartId"": ""V72:2272_07275158"",
                        ""Prices"": {
                          ""resaleList"": [
                            {
                              ""displayPrice"": ""0.369"",
                              ""price"": 0.369,
                              ""minQty"": 1,
                              ""maxQty"": 9
                            },
                            {
                              ""displayPrice"": ""0.2985"",
                              ""price"": 0.2985,
                              ""minQty"": 10,
                              ""maxQty"": 24
                            },
                            {
                              ""displayPrice"": ""0.2718"",
                              ""price"": 0.2718,
                              ""minQty"": 25,
                              ""maxQty"": 99
                            },
                            {
                              ""displayPrice"": ""0.203"",
                              ""price"": 0.203,
                              ""minQty"": 100,
                              ""maxQty"": 249
                            },
                            {
                              ""displayPrice"": ""0.1828"",
                              ""price"": 0.1828,
                              ""minQty"": 250,
                              ""maxQty"": 499
                            },
                            {
                              ""displayPrice"": ""0.1508"",
                              ""price"": 0.1508,
                              ""minQty"": 500,
                              ""maxQty"": 999
                            },
                            {
                              ""displayPrice"": ""0.1127"",
                              ""price"": 0.1127,
                              ""minQty"": 1000,
                              ""maxQty"": 999999999
                            }
                          ]
                        },
                        ""Availability"": [
                          {
                            ""fohQty"": 2403,
                            ""availabilityCd"": ""CSTRP"",
                            ""availabilityMessage"": ""Cut Strips"",
                            ""pipeline"": [
                              {
                                ""delivery"": ""21-MAR-2023"",
                                ""quantity"": 2435
                              }
                            ]
                          }
                        ],
                        ""customerSpecificPricing"": [],
                        ""customerSpecificInventory"": [],
                        ""dateCode"": ""2140"",
                        ""resources"": [
                          {
                            ""type"": ""detail"",
                            ""uri"": ""https://www.arrow.com/en/products/lm358dr/texas-instruments?region=nac""
                          },
                          {
                            ""type"": ""add_to_cart"",
                            ""uri"": ""https://www.arrow.com/en/products/lm358dr/texas-instruments?region=nac""
                          }
                        ],
                        ""inStock"": true,
                        ""mfrLeadTime"": 6,
                        ""tariffFlag"": false,
                        ""shipsFrom"": ""United States of America"",
                        ""shipsIn"": ""Ships today"",
                        ""arrowLeadTime"": """",
                        ""isNcnr"": false,
                        ""isNpi"": false,
                        ""productCode"": ""LM358DR"",
                        ""eccnCode"": ""EAR99"",
                        ""htsCode"": ""8542.33.00.01"",
                        ""countryOfOrigin"": ""MY"",
                        ""containerType"": ""Cut Strips""
                      }
                    ]
                  }
                ]
              }
            ]
          },
          ""hasDatasheet"": false,
          ""categoryName"": """",
          ""status"": ""Active""
        },
        {
          ""itemId"": 1588304,
          ""partNum"": ""LM358ADGKR"",
          ""manufacturer"": {
            ""mfrCd"": ""TI"",
            ""mfrName"": ""Texas Instruments""
          },
          ""desc"": ""Op Amp Dual GP ±16V/32V 8-Pin VSSOP T/R"",
          ""packageType"": """",
          ""resources"": [
            {
              ""type"": ""image_small"",
              ""uri"": ""https://static6.arrow.com/aropdfconversion/arrowimages/d434fa3d801a5aae250ae767e885b92a7f67789/dgk0008a.jpg""
            },
            {
              ""type"": ""image_large"",
              ""uri"": ""https://static6.arrow.com/aropdfconversion/arrowimages/93b7157dcd26e99206bafbcbea41981ebe83d36e/dgk0008a.jpg""
            },
            {
              ""type"": ""cloud_part_detail"",
              ""uri"": ""https://www.arrow.com/en/products/lm358adgkr/texas-instruments""
            },
            {
              ""type"": ""api_part_detail"",
              ""uri"": ""http://api.arrow.com/itemservice/v2/en/detail?mfrCd=TI&partNum=LM358ADGKR""
            },
            {
              ""type"": ""part_detail"",
              ""uri"": ""http://api.arrow.com/itemservice/v2/en/detail?pkey=-g2AjNfMYUj4LqJmFv6oKyreli7MfAYLnevEhoOu3pQ""
            }
          ],
          ""EnvData"": {
            ""compliance"": [
              {
                ""displayLabel"": ""eurohs"",
                ""displayValue"": ""Compliant""
              },
              {
                ""displayLabel"": ""cnrohs"",
                ""displayValue"": ""Compliant""
              }
            ]
          },
          ""InvOrg"": {
            ""webSites"": [
              {
                ""code"": ""Verical.com"",
                ""name"": ""Verical.com"",
                ""sources"": [
                  {
                    ""currency"": ""USD"",
                    ""sourceCd"": ""VERICAL"",
                    ""displayName"": ""Verical"",
                    ""sourceParts"": [
                      {
                        ""packSize"": 2500,
                        ""minimumOrderQuantity"": 2500,
                        ""sourcePartId"": ""33972739"",
                        ""Prices"": {
                          ""resaleList"": [
                            {
                              ""displayPrice"": ""0.1029"",
                              ""price"": 0.1029,
                              ""minQty"": 2500,
                              ""maxQty"": 99999999
                            }
                          ]
                        },
                        ""Availability"": [
                          {
                            ""fohQty"": 2500,
                            ""availabilityCd"": ""INSTK"",
                            ""availabilityMessage"": ""In Stock"",
                            ""pipeline"": []
                          }
                        ],
                        ""customerSpecificPricing"": [],
                        ""customerSpecificInventory"": [],
                        ""dateCode"": """",
                        ""resources"": [
                          {
                            ""type"": ""detail"",
                            ""uri"": ""https://www.verical.com/pd/national-semiconductor-amplifier---operational-LM358ADGKR-8096450""
                          },
                          {
                            ""type"": ""add_to_cart"",
                            ""uri"": ""https://www.verical.com/pd/national-semiconductor-amplifier---operational-LM358ADGKR-8096450""
                          }
                        ],
                        ""inStock"": true,
                        ""mfrLeadTime"": 0,
                        ""tariffFlag"": false,
                        ""arrowLeadTime"": """",
                        ""isNcnr"": false,
                        ""isNpi"": false,
                        ""productCode"": ""LM358ADGKR"",
                        ""eccnCode"": ""EAR99"",
                        ""htsCode"": ""8542.33.00.01"",
                        ""containerType"": """"
                      },
                      {
                        ""packSize"": 1,
                        ""minimumOrderQuantity"": 73,
                        ""sourcePartId"": ""62249909"",
                        ""Prices"": {
                          ""resaleList"": [
                            {
                              ""displayPrice"": ""0.3694"",
                              ""price"": 0.3694,
                              ""minQty"": 73,
                              ""maxQty"": 99
                            },
                            {
                              ""displayPrice"": ""0.3658"",
                              ""price"": 0.3658,
                              ""minQty"": 100,
                              ""maxQty"": 249
                            },
                            {
                              ""displayPrice"": ""0.3622"",
                              ""price"": 0.3622,
                              ""minQty"": 250,
                              ""maxQty"": 499
                            },
                            {
                              ""displayPrice"": ""0.3585"",
                              ""price"": 0.3585,
                              ""minQty"": 500,
                              ""maxQty"": 999
                            },
                            {
                              ""displayPrice"": ""0.3551"",
                              ""price"": 0.3551,
                              ""minQty"": 1000,
                              ""maxQty"": 999999999
                            }
                          ]
                        },
                        ""Availability"": [
                          {
                            ""fohQty"": 2010,
                            ""availabilityCd"": ""INSTK"",
                            ""availabilityMessage"": ""In Stock"",
                            ""pipeline"": []
                          }
                        ],
                        ""customerSpecificPricing"": [],
                        ""customerSpecificInventory"": [],
                        ""dateCode"": ""2218"",
                        ""resources"": [
                          {
                            ""type"": ""detail"",
                            ""uri"": ""https://www.verical.com/pd/national-semiconductor-amplifier---operational-LM358ADGKR-8096450""
                          },
                          {
                            ""type"": ""add_to_cart"",
                            ""uri"": ""https://www.verical.com/pd/national-semiconductor-amplifier---operational-LM358ADGKR-8096450""
                          }
                        ],
                        ""inStock"": true,
                        ""mfrLeadTime"": 0,
                        ""tariffFlag"": false,
                        ""arrowLeadTime"": """",
                        ""isNcnr"": false,
                        ""isNpi"": false,
                        ""productCode"": ""LM358ADGKR"",
                        ""eccnCode"": ""EAR99"",
                        ""htsCode"": ""8542.33.00.01"",
                        ""countryOfOrigin"": ""CN"",
                        ""containerType"": """"
                      }
                    ]
                  }
                ]
              },
              {
                ""code"": ""arrow.com"",
                ""name"": ""arrow.com"",
                ""sources"": [
                  {
                    ""currency"": ""USD"",
                    ""sourceCd"": ""ACNA"",
                    ""displayName"": ""Arrow Americas"",
                    ""sourceParts"": [
                      {
                        ""packSize"": 2500,
                        ""minimumOrderQuantity"": 2500,
                        ""sourcePartId"": ""V36:1790_07275137"",
                        ""Prices"": {
                          ""resaleList"": [
                            {
                              ""displayPrice"": ""0.0782"",
                              ""price"": 0.0782,
                              ""minQty"": 2500,
                              ""maxQty"": 9999
                            },
                            {
                              ""displayPrice"": ""0.0741"",
                              ""price"": 0.0741,
                              ""minQty"": 10000,
                              ""maxQty"": 24999
                            },
                            {
                              ""displayPrice"": ""0.0708"",
                              ""price"": 0.0708,
                              ""minQty"": 25000,
                              ""maxQty"": 49999
                            },
                            {
                              ""displayPrice"": ""0.0691"",
                              ""price"": 0.0691,
                              ""minQty"": 50000,
                              ""maxQty"": 99999999
                            }
                          ]
                        },
                        ""Availability"": [
                          {
                            ""fohQty"": 0,
                            ""availabilityCd"": ""QUOTE"",
                            ""availabilityMessage"": ""Call For Quote"",
                            ""pipeline"": []
                          }
                        ],
                        ""customerSpecificPricing"": [],
                        ""customerSpecificInventory"": [],
                        ""dateCode"": """",
                        ""resources"": [
                          {
                            ""type"": ""detail"",
                            ""uri"": ""https://www.arrow.com/en/products/lm358adgkr/texas-instruments?region=nac""
                          },
                          {
                            ""type"": ""add_to_cart"",
                            ""uri"": ""https://www.arrow.com/en/products/lm358adgkr/texas-instruments?region=nac""
                          }
                        ],
                        ""inStock"": false,
                        ""mfrLeadTime"": 6,
                        ""tariffFlag"": false,
                        ""shipsFrom"": ""United States of America"",
                        ""shipsIn"": ""Ships today"",
                        ""arrowLeadTime"": """",
                        ""isNcnr"": false,
                        ""isNpi"": false,
                        ""productCode"": ""LM358ADGKR"",
                        ""eccnCode"": ""EAR99"",
                        ""htsCode"": ""8542.33.00.01"",
                        ""containerType"": """"
                      },
                      {
                        ""packSize"": 2500,
                        ""minimumOrderQuantity"": 2500,
                        ""sourcePartId"": ""V39:1801_07275137"",
                        ""Prices"": {
                          ""resaleList"": [
                            {
                              ""displayPrice"": ""0.1029"",
                              ""price"": 0.1029,
                              ""minQty"": 2500,
                              ""maxQty"": 4999
                            },
                            {
                              ""displayPrice"": ""0.0957"",
                              ""price"": 0.0957,
                              ""minQty"": 5000,
                              ""maxQty"": 12499
                            },
                            {
                              ""displayPrice"": ""0.0887"",
                              ""price"": 0.0887,
                              ""minQty"": 12500,
                              ""maxQty"": 24999
                            },
                            {
                              ""displayPrice"": ""0.0856"",
                              ""price"": 0.0856,
                              ""minQty"": 25000,
                              ""maxQty"": 99999999
                            }
                          ]
                        },
                        ""Availability"": [
                          {
                            ""fohQty"": 2500,
                            ""availabilityCd"": ""INSTK"",
                            ""availabilityMessage"": ""In Stock"",
                            ""pipeline"": []
                          }
                        ],
                        ""customerSpecificPricing"": [],
                        ""customerSpecificInventory"": [],
                        ""dateCode"": """",
                        ""resources"": [
                          {
                            ""type"": ""detail"",
                            ""uri"": ""https://www.arrow.com/en/products/lm358adgkr/texas-instruments?region=nac""
                          },
                          {
                            ""type"": ""add_to_cart"",
                            ""uri"": ""https://www.arrow.com/en/products/lm358adgkr/texas-instruments?region=nac""
                          }
                        ],
                        ""inStock"": true,
                        ""mfrLeadTime"": 6,
                        ""tariffFlag"": false,
                        ""shipsFrom"": ""United States of America"",
                        ""shipsIn"": ""Ships today"",
                        ""arrowLeadTime"": """",
                        ""isNcnr"": false,
                        ""isNpi"": false,
                        ""productCode"": ""LM358ADGKR"",
                        ""eccnCode"": ""EAR99"",
                        ""htsCode"": ""8542.33.00.01"",
                        ""containerType"": """"
                      },
                      {
                        ""packSize"": 1,
                        ""minimumOrderQuantity"": 1,
                        ""sourcePartId"": ""V72:2272_07275137"",
                        ""Prices"": {
                          ""resaleList"": [
                            {
                              ""displayPrice"": ""0.3765"",
                              ""price"": 0.3765,
                              ""minQty"": 1,
                              ""maxQty"": 9
                            },
                            {
                              ""displayPrice"": ""0.3034"",
                              ""price"": 0.3034,
                              ""minQty"": 10,
                              ""maxQty"": 24
                            },
                            {
                              ""displayPrice"": ""0.2759"",
                              ""price"": 0.2759,
                              ""minQty"": 25,
                              ""maxQty"": 99
                            },
                            {
                              ""displayPrice"": ""0.2053"",
                              ""price"": 0.2053,
                              ""minQty"": 100,
                              ""maxQty"": 249
                            },
                            {
                              ""displayPrice"": ""0.1847"",
                              ""price"": 0.1847,
                              ""minQty"": 250,
                              ""maxQty"": 499
                            },
                            {
                              ""displayPrice"": ""0.152"",
                              ""price"": 0.152,
                              ""minQty"": 500,
                              ""maxQty"": 999
                            },
                            {
                              ""displayPrice"": ""0.1134"",
                              ""price"": 0.1134,
                              ""minQty"": 1000,
                              ""maxQty"": 999999999
                            }
                          ]
                        },
                        ""Availability"": [
                          {
                            ""fohQty"": 2010,
                            ""availabilityCd"": ""CSTRP"",
                            ""availabilityMessage"": ""Cut Strips"",
                            ""pipeline"": []
                          }
                        ],
                        ""customerSpecificPricing"": [],
                        ""customerSpecificInventory"": [],
                        ""dateCode"": ""2218"",
                        ""resources"": [
                          {
                            ""type"": ""detail"",
                            ""uri"": ""https://www.arrow.com/en/products/lm358adgkr/texas-instruments?region=nac""
                          },
                          {
                            ""type"": ""add_to_cart"",
                            ""uri"": ""https://www.arrow.com/en/products/lm358adgkr/texas-instruments?region=nac""
                          }
                        ],
                        ""inStock"": true,
                        ""mfrLeadTime"": 6,
                        ""tariffFlag"": false,
                        ""shipsFrom"": ""United States of America"",
                        ""shipsIn"": ""Ships today"",
                        ""arrowLeadTime"": """",
                        ""isNcnr"": false,
                        ""isNpi"": false,
                        ""productCode"": ""LM358ADGKR"",
                        ""eccnCode"": ""EAR99"",
                        ""htsCode"": ""8542.33.00.01"",
                        ""countryOfOrigin"": ""CN"",
                        ""containerType"": ""Cut Strips""
                      }
                    ]
                  }
                ]
              }
            ]
          },
          ""hasDatasheet"": false,
          ""categoryName"": ""Operational Amplifiers - Op Amps"",
          ""status"": ""Active""
        },
        {
          ""itemId"": 41646742,
          ""partNum"": ""LM358LVIDR"",
          ""manufacturer"": {
            ""mfrCd"": ""TI"",
            ""mfrName"": ""Texas Instruments""
          },
          ""desc"": ""Op Amp Dual Low Voltage Amplifier ±2.75V/5.5V 8-Pin SOIC T/R"",
          ""packageType"": """",
          ""resources"": [
            {
              ""type"": ""image_small"",
              ""uri"": ""https://static6.arrow.com/aropdfconversion/arrowimages/3f304a36ac6e93000334d1a2cf03ec6bd35c733b/d0008a.jpg""
            },
            {
              ""type"": ""image_large"",
              ""uri"": ""https://static6.arrow.com/aropdfconversion/arrowimages/e5533f271a03578b303601b99b62d8a183cba66a/d0008a.jpg""
            },
            {
              ""type"": ""cloud_part_detail"",
              ""uri"": ""https://www.arrow.com/en/products/lm358lvidr/texas-instruments""
            },
            {
              ""type"": ""api_part_detail"",
              ""uri"": ""http://api.arrow.com/itemservice/v2/en/detail?mfrCd=TI&partNum=LM358LVIDR""
            },
            {
              ""type"": ""part_detail"",
              ""uri"": ""http://api.arrow.com/itemservice/v2/en/detail?pkey=k-zjBRjt7go_ObKqe5ec2Q1fhyVvniQqVDeFzXaR8FM""
            }
          ],
          ""EnvData"": {
            ""compliance"": [
              {
                ""displayLabel"": ""eurohs"",
                ""displayValue"": ""Compliant""
              },
              {
                ""displayLabel"": ""cnrohs"",
                ""displayValue"": ""Compliant""
              }
            ]
          },
          ""InvOrg"": {
            ""webSites"": [
              {
                ""code"": ""arrow.com"",
                ""name"": ""arrow.com"",
                ""sources"": [
                  {
                    ""currency"": ""USD"",
                    ""sourceCd"": ""ACNA"",
                    ""displayName"": ""Arrow Americas"",
                    ""sourceParts"": [
                      {
                        ""packSize"": 2500,
                        ""minimumOrderQuantity"": 2500,
                        ""sourcePartId"": ""V36:1790_22822371"",
                        ""Prices"": {
                          ""resaleList"": [
                            {
                              ""displayPrice"": ""0.0624"",
                              ""price"": 0.0624,
                              ""minQty"": 2500,
                              ""maxQty"": 9999
                            },
                            {
                              ""displayPrice"": ""0.059"",
                              ""price"": 0.059,
                              ""minQty"": 10000,
                              ""maxQty"": 24999
                            },
                            {
                              ""displayPrice"": ""0.0569"",
                              ""price"": 0.0569,
                              ""minQty"": 25000,
                              ""maxQty"": 49999
                            },
                            {
                              ""displayPrice"": ""0.0552"",
                              ""price"": 0.0552,
                              ""minQty"": 50000,
                              ""maxQty"": 99999999
                            }
                          ]
                        },
                        ""Availability"": [
                          {
                            ""fohQty"": 0,
                            ""availabilityCd"": ""QUOTE"",
                            ""availabilityMessage"": ""Call For Quote"",
                            ""pipeline"": []
                          }
                        ],
                        ""customerSpecificPricing"": [],
                        ""customerSpecificInventory"": [],
                        ""dateCode"": """",
                        ""resources"": [
                          {
                            ""type"": ""detail"",
                            ""uri"": ""https://www.arrow.com/en/products/lm358lvidr/texas-instruments?region=nac""
                          },
                          {
                            ""type"": ""add_to_cart"",
                            ""uri"": ""https://www.arrow.com/en/products/lm358lvidr/texas-instruments?region=nac""
                          }
                        ],
                        ""inStock"": false,
                        ""mfrLeadTime"": 12,
                        ""tariffFlag"": false,
                        ""shipsFrom"": ""United States of America"",
                        ""shipsIn"": ""Ships today"",
                        ""arrowLeadTime"": """",
                        ""isNcnr"": false,
                        ""isNpi"": false,
                        ""productCode"": ""LM358LVIDR"",
                        ""eccnCode"": ""EAR99"",
                        ""htsCode"": ""8542.33.00.01"",
                        ""containerType"": """"
                      },
                      {
                        ""packSize"": 1,
                        ""minimumOrderQuantity"": 1,
                        ""sourcePartId"": ""V72:2272_22822371"",
                        ""Prices"": {
                          ""resaleList"": [
                            {
                              ""displayPrice"": ""0.4652"",
                              ""price"": 0.4652,
                              ""minQty"": 1,
                              ""maxQty"": 9
                            },
                            {
                              ""displayPrice"": ""0.3517"",
                              ""price"": 0.3517,
                              ""minQty"": 10,
                              ""maxQty"": 24
                            },
                            {
                              ""displayPrice"": ""0.3074"",
                              ""price"": 0.3074,
                              ""minQty"": 25,
                              ""maxQty"": 999999999
                            }
                          ]
                        },
                        ""Availability"": [
                          {
                            ""fohQty"": 67,
                            ""availabilityCd"": ""CSTRP"",
                            ""availabilityMessage"": ""Cut Strips"",
                            ""pipeline"": []
                          }
                        ],
                        ""customerSpecificPricing"": [],
                        ""customerSpecificInventory"": [],
                        ""dateCode"": ""2220"",
                        ""resources"": [
                          {
                            ""type"": ""detail"",
                            ""uri"": ""https://www.arrow.com/en/products/lm358lvidr/texas-instruments?region=nac""
                          },
                          {
                            ""type"": ""add_to_cart"",
                            ""uri"": ""https://www.arrow.com/en/products/lm358lvidr/texas-instruments?region=nac""
                          }
                        ],
                        ""inStock"": true,
                        ""mfrLeadTime"": 12,
                        ""tariffFlag"": false,
                        ""shipsFrom"": ""United States of America"",
                        ""shipsIn"": ""Ships today"",
                        ""arrowLeadTime"": """",
                        ""isNcnr"": false,
                        ""isNpi"": false,
                        ""productCode"": ""LM358LVIDR"",
                        ""eccnCode"": ""EAR99"",
                        ""htsCode"": ""8542.33.00.01"",
                        ""countryOfOrigin"": ""PH"",
                        ""containerType"": ""Cut Strips""
                      },
                      {
                        ""packSize"": 2500,
                        ""minimumOrderQuantity"": 2500,
                        ""sourcePartId"": ""V39:1801_22822371"",
                        ""Prices"": {
                          ""resaleList"": [
                            {
                              ""displayPrice"": ""0.0624"",
                              ""price"": 0.0624,
                              ""minQty"": 2500,
                              ""maxQty"": 9999
                            },
                            {
                              ""displayPrice"": ""0.059"",
                              ""price"": 0.059,
                              ""minQty"": 10000,
                              ""maxQty"": 24999
                            },
                            {
                              ""displayPrice"": ""0.0569"",
                              ""price"": 0.0569,
                              ""minQty"": 25000,
                              ""maxQty"": 49999
                            },
                            {
                              ""displayPrice"": ""0.0552"",
                              ""price"": 0.0552,
                              ""minQty"": 50000,
                              ""maxQty"": 99999999
                            }
                          ]
                        },
                        ""Availability"": [
                          {
                            ""fohQty"": 0,
                            ""availabilityCd"": ""QUOTE"",
                            ""availabilityMessage"": ""Call For Quote"",
                            ""pipeline"": []
                          }
                        ],
                        ""customerSpecificPricing"": [],
                        ""customerSpecificInventory"": [],
                        ""dateCode"": """",
                        ""resources"": [
                          {
                            ""type"": ""detail"",
                            ""uri"": ""https://www.arrow.com/en/products/lm358lvidr/texas-instruments?region=nac""
                          },
                          {
                            ""type"": ""add_to_cart"",
                            ""uri"": ""https://www.arrow.com/en/products/lm358lvidr/texas-instruments?region=nac""
                          }
                        ],
                        ""inStock"": false,
                        ""mfrLeadTime"": 12,
                        ""tariffFlag"": false,
                        ""shipsFrom"": ""United States of America"",
                        ""shipsIn"": ""Ships today"",
                        ""arrowLeadTime"": """",
                        ""isNcnr"": false,
                        ""isNpi"": false,
                        ""productCode"": ""LM358LVIDR"",
                        ""eccnCode"": ""EAR99"",
                        ""htsCode"": ""8542.33.00.01"",
                        ""containerType"": """"
                      }
                    ]
                  }
                ]
              }
            ]
          },
          ""hasDatasheet"": false,
          ""categoryName"": ""Operational Amplifiers - Op Amps"",
          ""status"": ""Active""
        },
        {
          ""itemId"": 42212691,
          ""partNum"": ""LM358F-GE2"",
          ""manufacturer"": {
            ""mfrCd"": ""ROHM"",
            ""mfrName"": ""Rohm Semiconductor""
          },
          ""desc"": ""Op Amp Dual GP ±16V/32V 8-Pin SOP T/R"",
          ""packageType"": """",
          ""resources"": [
            {
              ""type"": ""datasheet"",
              ""uri"": ""https://static6.arrow.com/aropdfconversion/77e8fa2c6444fce1b82c8951fb3b40f288401546/lm358f-e.pdf""
            },
            {
              ""type"": ""image_small"",
              ""uri"": ""https://static6.arrow.com/aropdfconversion/arrowimages/ce6bd87d901b62c0f0111bc71aebc1ee55d978f0/sop8.jpg""
            },
            {
              ""type"": ""image_large"",
              ""uri"": ""https://static6.arrow.com/aropdfconversion/arrowimages/5d3e95c85d75636ab6f3c70d5f34da26729f5f61/sop8.jpg""
            },
            {
              ""type"": ""cloud_part_detail"",
              ""uri"": ""https://www.arrow.com/en/products/lm358f-ge2/rohm-semiconductor""
            },
            {
              ""type"": ""api_part_detail"",
              ""uri"": ""http://api.arrow.com/itemservice/v2/en/detail?mfrCd=ROHM&partNum=LM358FGE2""
            },
            {
              ""type"": ""part_detail"",
              ""uri"": ""http://api.arrow.com/itemservice/v2/en/detail?pkey=aFgteqA_5rql1WQzNHn8m40RzK4-t6_aoaWeYdJWWn0""
            }
          ],
          ""EnvData"": {
            ""compliance"": [
              {
                ""displayLabel"": ""eurohs"",
                ""displayValue"": ""Compliant""
              },
              {
                ""displayLabel"": ""cnrohs"",
                ""displayValue"": ""Compliant""
              }
            ]
          },
          ""InvOrg"": {
            ""webSites"": [
              {
                ""code"": ""Verical.com"",
                ""name"": ""Verical.com"",
                ""sources"": [
                  {
                    ""currency"": ""USD"",
                    ""sourceCd"": ""VERICAL"",
                    ""displayName"": ""Verical"",
                    ""sourceParts"": [
                      {
                        ""packSize"": 1,
                        ""minimumOrderQuantity"": 118,
                        ""sourcePartId"": ""66395083"",
                        ""Prices"": {
                          ""resaleList"": [
                            {
                              ""displayPrice"": ""0.2703"",
                              ""price"": 0.2703,
                              ""minQty"": 118,
                              ""maxQty"": 199
                            },
                            {
                              ""displayPrice"": ""0.269"",
                              ""price"": 0.269,
                              ""minQty"": 200,
                              ""maxQty"": 499
                            },
                            {
                              ""displayPrice"": ""0.2282"",
                              ""price"": 0.2282,
                              ""minQty"": 500,
                              ""maxQty"": 999
                            },
                            {
                              ""displayPrice"": ""0.2155"",
                              ""price"": 0.2155,
                              ""minQty"": 1000,
                              ""maxQty"": 999999999
                            }
                          ]
                        },
                        ""Availability"": [
                          {
                            ""fohQty"": 2500,
                            ""availabilityCd"": ""INSTK"",
                            ""availabilityMessage"": ""In Stock"",
                            ""pipeline"": []
                          }
                        ],
                        ""customerSpecificPricing"": [],
                        ""customerSpecificInventory"": [],
                        ""dateCode"": """",
                        ""resources"": [
                          {
                            ""type"": ""detail"",
                            ""uri"": ""https://www.verical.com/pd/rohm-semiconductor-amplifier---operational-LM358F-GE2-4484509""
                          },
                          {
                            ""type"": ""add_to_cart"",
                            ""uri"": ""https://www.verical.com/pd/rohm-semiconductor-amplifier---operational-LM358F-GE2-4484509""
                          }
                        ],
                        ""inStock"": true,
                        ""mfrLeadTime"": 0,
                        ""tariffFlag"": false,
                        ""arrowLeadTime"": """",
                        ""isNcnr"": false,
                        ""isNpi"": false,
                        ""productCode"": ""LM358FGE2"",
                        ""eccnCode"": ""EAR99"",
                        ""containerType"": """"
                      }
                    ]
                  }
                ]
              }
            ]
          },
          ""hasDatasheet"": true,
          ""categoryName"": """",
          ""status"": ""Active""
        },
        {
          ""itemId"": 51776106,
          ""partNum"": ""LM358FV-GE2"",
          ""manufacturer"": {
            ""mfrCd"": ""ROHM"",
            ""mfrName"": ""Rohm Semiconductor""
          },
          ""desc"": ""Op Amp Dual GP ±16V/32V 8-Pin SSOP-B T/R"",
          ""packageType"": """",
          ""resources"": [
            {
              ""type"": ""cloud_part_detail"",
              ""uri"": ""https://www.arrow.com/en/products/lm358fv-ge2/rohm-semiconductor""
            },
            {
              ""type"": ""api_part_detail"",
              ""uri"": ""http://api.arrow.com/itemservice/v2/en/detail?mfrCd=ROHM&partNum=LM358FVGE2""
            },
            {
              ""type"": ""part_detail"",
              ""uri"": ""http://api.arrow.com/itemservice/v2/en/detail?pkey=ke-dQeV3kfTQbXzr7XaCvX3mwrZIZaptQ_qZL-wefSc""
            }
          ],
          ""EnvData"": {
            ""compliance"": [
              {
                ""displayLabel"": ""eurohs"",
                ""displayValue"": """"
              },
              {
                ""displayLabel"": ""cnrohs"",
                ""displayValue"": """"
              }
            ]
          },
          ""InvOrg"": {
            ""webSites"": [
              {
                ""code"": ""Verical.com"",
                ""name"": ""Verical.com"",
                ""sources"": [
                  {
                    ""currency"": ""USD"",
                    ""sourceCd"": ""VERICAL"",
                    ""displayName"": ""Verical"",
                    ""sourceParts"": [
                      {
                        ""packSize"": 1,
                        ""minimumOrderQuantity"": 291,
                        ""sourcePartId"": ""66354009"",
                        ""Prices"": {
                          ""resaleList"": [
                            {
                              ""displayPrice"": ""1.3111"",
                              ""price"": 1.3111,
                              ""minQty"": 66,
                              ""maxQty"": 99
                            },
                            {
                              ""displayPrice"": ""1.2525"",
                              ""price"": 1.2525,
                              ""minQty"": 100,
                              ""maxQty"": 249
                            },
                            {
                              ""displayPrice"": ""1.2022"",
                              ""price"": 1.2022,
                              ""minQty"": 250,
                              ""maxQty"": 499
                            },
                            {
                              ""displayPrice"": ""1.1588"",
                              ""price"": 1.1588,
                              ""minQty"": 500,
                              ""maxQty"": 999
                            },
                            {
                              ""displayPrice"": ""1.121"",
                              ""price"": 1.121,
                              ""minQty"": 1000,
                              ""maxQty"": 2499
                            },
                            {
                              ""displayPrice"": ""1.0879"",
                              ""price"": 1.0879,
                              ""minQty"": 2500,
                              ""maxQty"": 999999999
                            }
                          ]
                        },
                        ""Availability"": [
                          {
                            ""fohQty"": 2500,
                            ""availabilityCd"": ""INSTK"",
                            ""availabilityMessage"": ""In Stock"",
                            ""pipeline"": []
                          }
                        ],
                        ""customerSpecificPricing"": [],
                        ""customerSpecificInventory"": [],
                        ""dateCode"": ""2201"",
                        ""resources"": [
                          {
                            ""type"": ""detail"",
                            ""uri"": ""https://www.verical.com/pd/rohm-semiconductor-amplifier---operational-LM358FV-GE2-7409251""
                          },
                          {
                            ""type"": ""add_to_cart"",
                            ""uri"": ""https://www.verical.com/pd/rohm-semiconductor-amplifier---operational-LM358FV-GE2-7409251""
                          }
                        ],
                        ""inStock"": true,
                        ""mfrLeadTime"": 0,
                        ""tariffFlag"": false,
                        ""arrowLeadTime"": """",
                        ""isNcnr"": false,
                        ""isNpi"": false,
                        ""productCode"": ""LM358FVGE2"",
                        ""countryOfOrigin"": ""PH"",
                        ""containerType"": """"
                      }
                    ]
                  }
                ]
              }
            ]
          },
          ""hasDatasheet"": false,
          ""categoryName"": """",
          ""status"": """"
        }
      ]
    }
  ]
}}";
        }
    }
}
