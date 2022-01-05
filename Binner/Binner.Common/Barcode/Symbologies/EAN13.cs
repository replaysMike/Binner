using System;
using System.Collections;

namespace Binner.Common.Barcode.Symbologies
{
    /// <summary>
    /// EAN-13 encoding
    /// </summary>
    public class Ean13 : BarcodeSymbology
    {
        private readonly string[] _eanCodeA = { "0001101", "0011001", "0010011", "0111101", "0100011", "0110001", "0101111", "0111011", "0110111", "0001011" };
        private readonly string[] _eanCodeB = { "0100111", "0110011", "0011011", "0100001", "0011101", "0111001", "0000101", "0010001", "0001001", "0010111" };
        private readonly string[] _eanCodeC = { "1110010", "1100110", "1101100", "1000010", "1011100", "1001110", "1010000", "1000100", "1001000", "1110100" };
        private readonly string[] _eanPattern = { "aaaaaa", "aababb", "aabbab", "aabbba", "abaabb", "abbaab", "abbbaa", "ababab", "ababba", "abbaba" };
        private readonly Hashtable _countryCodes = new();
        private string _countryAssigningManufacturerCode = "N/A";

        public string CountryAssigningManufacturerCode { get => _countryAssigningManufacturerCode; set => _countryAssigningManufacturerCode = value; }

        public Ean13(string input, bool disableCountryCode = false)
        {
            RawData = input;
            DisableCountryCode = disableCountryCode;

            CheckDigit();
        }

        /// <summary>
        /// Disables EAN13 country code parsing
        /// </summary>
        public bool DisableCountryCode { get; set; } = false;

        /// <summary>
        /// Encode the raw data using the EAN-13 algorithm. (Can include the checksum already.  If it doesnt exist in the data then it will calculate it for you.  Accepted data lengths are 12 + 1 checksum or just the 12 data digits)
        /// </summary>
        private string EncodeEAN13()
        {
            //check length of input
            if (RawData.Length < 12 || RawData.Length > 13)
                Error("EEAN13-1: Data length invalid. (Length must be 12 or 13)");

            if (!CheckNumericOnly(RawData))
                Error("EEAN13-2: Numeric Data Only");

            var patterncode = _eanPattern[int.Parse(RawData[0].ToString())];
            var result = "101";

            //first
            //result += EAN_CodeA[Int32.Parse(RawData[0].ToString())];

            //second
            var pos = 0;
            while (pos < 6)
            {
                if (patterncode[pos] == 'a')
                    result += _eanCodeA[int.Parse(RawData[pos + 1].ToString())];
                if (patterncode[pos] == 'b')
                    result += _eanCodeB[int.Parse(RawData[pos + 1].ToString())];
                pos++;
            }


            // add divider bars
            result += "01010";

            // get the third
            pos = 1;
            while (pos <= 5)
            {
                result += _eanCodeC[int.Parse(RawData[(pos++) + 6].ToString())];
            }

            // checksum digit
            var cs = int.Parse(RawData[RawData.Length - 1].ToString());

            // add checksum
            result += _eanCodeC[cs];

            // add ending bars
            result += "101";

            if (!DisableCountryCode)
            {
                ParseCountryCode();
            }

            return result;
        }

        private void ParseCountryCode()
        {
            //get the manufacturer assigning country
            InitCountryCodes();
            CountryAssigningManufacturerCode = "N/A";
            var twodigitCode = RawData.Substring(0, 2);
            var threedigitCode = RawData.Substring(0, 3);

            var cc = _countryCodes[threedigitCode];
            if (cc == null)
            {
                cc = _countryCodes[twodigitCode].ToString();
                if (cc == null)
                {
                    Error("EEAN13-3: Country assigning manufacturer code not found.");
                }
                else
                {
                    CountryAssigningManufacturerCode = cc.ToString();
                }
            }
            else
            {
                CountryAssigningManufacturerCode = cc.ToString();
            }

            _countryCodes.Clear();
        }

        private void CreateCountryCodeRange(int startingNumber, int endingNumber, string countryDescription)
        {
            for (var i = startingNumber; i <= endingNumber; i++)
            {
                _countryCodes.Add(i.ToString("00"), countryDescription);
            }
        }

        private void InitCountryCodes()
        {
            _countryCodes.Clear();

            // Source: https://en.wikipedia.org/wiki/List_of_GS1_country_codes
            CreateCountryCodeRange(0, 19, "US / CANADA");
            CreateCountryCodeRange(20, 29, "IN STORE");
            CreateCountryCodeRange(30, 39, "US DRUGS");
            CreateCountryCodeRange(40, 49, "Used to issue restricted circulation numbers within a geographic region (MO defined)");
            CreateCountryCodeRange(50, 59, "GS1 US reserved for future use");
            CreateCountryCodeRange(60, 99, "US / CANADA");
            CreateCountryCodeRange(100, 139, "UNITED STATES");
            CreateCountryCodeRange(200, 299, "Used to issue GS1 restricted circulation number within a geographic region (MO defined)");
            CreateCountryCodeRange(300, 379, "FRANCE AND MONACO");

            CreateCountryCodeRange(380, 380, "BULGARIA");
            CreateCountryCodeRange(383, 383, "SLOVENIA");
            CreateCountryCodeRange(385, 385, "CROATIA");
            CreateCountryCodeRange(387, 387, "BOSNIA AND HERZEGOVINA");
            CreateCountryCodeRange(389, 389, "MONTENEGRO");
            CreateCountryCodeRange(400, 440, "GERMANY");
            CreateCountryCodeRange(450, 459, "JAPAN");
            CreateCountryCodeRange(460, 469, "RUSSIA");
            CreateCountryCodeRange(470, 470, "KYRGYZSTAN");
            CreateCountryCodeRange(471, 471, "TAIWAN");
            CreateCountryCodeRange(474, 474, "ESTONIA");
            CreateCountryCodeRange(475, 475, "LATVIA");
            CreateCountryCodeRange(476, 476, "AZERBAIJAN");
            CreateCountryCodeRange(477, 477, "LITHUANIA");
            CreateCountryCodeRange(478, 478, "UZBEKISTAN");
            CreateCountryCodeRange(479, 479, "SRI LANKA");
            CreateCountryCodeRange(480, 480, "PHILIPPINES");
            CreateCountryCodeRange(481, 481, "BELARUS");
            CreateCountryCodeRange(482, 482, "UKRAINE");
            CreateCountryCodeRange(483, 483, "TURKMENISTAN");
            CreateCountryCodeRange(484, 484, "MOLDOVA");
            CreateCountryCodeRange(485, 485, "ARMENIA");
            CreateCountryCodeRange(486, 486, "GEORGIA");
            CreateCountryCodeRange(487, 487, "KAZAKHSTAN");
            CreateCountryCodeRange(488, 488, "TAJIKISTAN");
            CreateCountryCodeRange(489, 489, "HONG KONG");
            CreateCountryCodeRange(490, 499, "JAPAN");
            CreateCountryCodeRange(500, 509, "UNITED KINGDOM");
            CreateCountryCodeRange(520, 521, "GREECE");
            CreateCountryCodeRange(528, 528, "LEBANON");
            CreateCountryCodeRange(529, 529, "CYPRUS");
            CreateCountryCodeRange(530, 530, "ALBANIA");
            CreateCountryCodeRange(531, 531, "MACEDONIA");
            CreateCountryCodeRange(535, 535, "MALTA");
            CreateCountryCodeRange(539, 539, "REPUBLIC OF IRELAND");
            CreateCountryCodeRange(540, 549, "BELGIUM AND LUXEMBOURG");
            CreateCountryCodeRange(560, 560, "PORTUGAL");
            CreateCountryCodeRange(569, 569, "ICELAND");
            CreateCountryCodeRange(570, 579, "DENMARK, FAROE ISLANDS AND GREENLAND");
            CreateCountryCodeRange(590, 590, "POLAND");
            CreateCountryCodeRange(594, 594, "ROMANIA");
            CreateCountryCodeRange(599, 599, "HUNGARY");
            CreateCountryCodeRange(600, 601, "SOUTH AFRICA");
            CreateCountryCodeRange(603, 603, "GHANA");
            CreateCountryCodeRange(604, 604, "SENEGAL");
            CreateCountryCodeRange(608, 608, "BAHRAIN");
            CreateCountryCodeRange(609, 609, "MAURITIUS");
            CreateCountryCodeRange(611, 611, "MOROCCO");
            CreateCountryCodeRange(613, 613, "ALGERIA");
            CreateCountryCodeRange(615, 615, "NIGERIA");
            CreateCountryCodeRange(616, 616, "KENYA");
            CreateCountryCodeRange(618, 618, "IVORY COAST");
            CreateCountryCodeRange(619, 619, "TUNISIA");
            CreateCountryCodeRange(620, 620, "TANZANIA");
            CreateCountryCodeRange(621, 621, "SYRIA");
            CreateCountryCodeRange(622, 622, "EGYPT");
            CreateCountryCodeRange(623, 623, "BRUNEI");
            CreateCountryCodeRange(624, 624, "LIBYA");
            CreateCountryCodeRange(625, 625, "JORDAN");
            CreateCountryCodeRange(626, 626, "IRAN");
            CreateCountryCodeRange(627, 627, "KUWAIT");
            CreateCountryCodeRange(628, 628, "SAUDI ARABIA");
            CreateCountryCodeRange(629, 629, "UNITED ARAB EMIRATES");
            CreateCountryCodeRange(640, 649, "FINLAND");
            CreateCountryCodeRange(690, 699, "CHINA");
            CreateCountryCodeRange(700, 709, "NORWAY");
            CreateCountryCodeRange(729, 729, "ISRAEL");
            CreateCountryCodeRange(730, 739, "SWEDEN");
            CreateCountryCodeRange(740, 740, "GUATEMALA");
            CreateCountryCodeRange(741, 741, "EL SALVADOR");
            CreateCountryCodeRange(742, 742, "HONDURAS");
            CreateCountryCodeRange(743, 743, "NICARAGUA");
            CreateCountryCodeRange(744, 744, "COSTA RICA");
            CreateCountryCodeRange(745, 745, "PANAMA");
            CreateCountryCodeRange(746, 746, "DOMINICAN REPUBLIC");
            CreateCountryCodeRange(750, 750, "MEXICO");
            CreateCountryCodeRange(754, 755, "CANADA");
            CreateCountryCodeRange(759, 759, "VENEZUELA");
            CreateCountryCodeRange(760, 769, "SWITZERLAND AND LIECHTENSTEIN");
            CreateCountryCodeRange(770, 771, "COLOMBIA");
            CreateCountryCodeRange(773, 773, "URUGUAY");
            CreateCountryCodeRange(775, 775, "PERU");
            CreateCountryCodeRange(777, 777, "BOLIVIA");
            CreateCountryCodeRange(778, 779, "ARGENTINA");
            CreateCountryCodeRange(780, 780, "CHILE");
            CreateCountryCodeRange(784, 784, "PARAGUAY");
            CreateCountryCodeRange(786, 786, "ECUADOR");
            CreateCountryCodeRange(789, 790, "BRAZIL");
            CreateCountryCodeRange(800, 839, "ITALY, SAN MARINO AND VATICAN CITY");
            CreateCountryCodeRange(840, 849, "SPAIN AND ANDORRA");
            CreateCountryCodeRange(850, 850, "CUBA");
            CreateCountryCodeRange(858, 858, "SLOVAKIA");
            CreateCountryCodeRange(859, 859, "CZECH REPUBLIC");
            CreateCountryCodeRange(860, 860, "SERBIA");
            CreateCountryCodeRange(865, 865, "MONGOLIA");
            CreateCountryCodeRange(867, 867, "NORTH KOREA");
            CreateCountryCodeRange(868, 869, "TURKEY");
            CreateCountryCodeRange(870, 879, "NETHERLANDS");
            CreateCountryCodeRange(880, 880, "SOUTH KOREA");
            CreateCountryCodeRange(884, 884, "CAMBODIA");
            CreateCountryCodeRange(885, 885, "THAILAND");
            CreateCountryCodeRange(888, 888, "SINGAPORE");
            CreateCountryCodeRange(890, 890, "INDIA");
            CreateCountryCodeRange(893, 893, "VIETNAM");
            CreateCountryCodeRange(896, 896, "PAKISTAN");
            CreateCountryCodeRange(899, 899, "INDONESIA");
            CreateCountryCodeRange(900, 919, "AUSTRIA");
            CreateCountryCodeRange(930, 939, "AUSTRALIA");
            CreateCountryCodeRange(940, 949, "NEW ZEALAND");
            CreateCountryCodeRange(950, 950, "GS1 GLOBAL OFFICE SPECIAL APPLICATIONS");
            CreateCountryCodeRange(951, 951, "EPC GLOBAL SPECIAL APPLICATIONS");
            CreateCountryCodeRange(955, 955, "MALAYSIA");
            CreateCountryCodeRange(958, 958, "MACAU");
            CreateCountryCodeRange(960, 961, "GS1 UK OFFICE: GTIN-8 ALLOCATIONS");
            CreateCountryCodeRange(962, 969, "GS1 GLOBAL OFFICE: GTIN-8 ALLOCATIONS");
            CreateCountryCodeRange(977, 977, "SERIAL PUBLICATIONS (ISSN)");
            CreateCountryCodeRange(978, 979, "BOOKLAND (ISBN) – 979-0 USED FOR SHEET MUSIC (ISMN-13, REPLACES DEPRECATED ISMN M- NUMBERS)");
            CreateCountryCodeRange(980, 980, "REFUND RECEIPTS");
            CreateCountryCodeRange(981, 984, "GS1 COUPON IDENTIFICATION FOR COMMON CURRENCY AREAS");
            CreateCountryCodeRange(990, 999, "GS1 COUPON IDENTIFICATION");
        }
        private void CheckDigit()
        {
            try
            {
                var rawDataHolder = RawData.Substring(0, 12);

                var even = 0;
                var odd = 0;

                for (var i = 0; i < rawDataHolder.Length; i++)
                {
                    if (i % 2 == 0)
                        odd += int.Parse(rawDataHolder.Substring(i, 1));
                    else
                        even += int.Parse(rawDataHolder.Substring(i, 1)) * 3;
                }

                var total = even + odd;
                var cs = total % 10;
                cs = 10 - cs;
                if (cs == 10)
                    cs = 0;

                RawData = rawDataHolder + cs.ToString()[0];
            }
            catch
            {
                Error("EEAN13-4: Error calculating check digit.");
            }
        }

        #region IBarcode Members

        public override string EncodedValue => EncodeEAN13();

        #endregion
    }
}
