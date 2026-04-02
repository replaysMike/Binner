using Binner.Common.Extensions;
using Binner.Model.Requests;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using TypeSupport.Extensions;

namespace Binner.Common.Data
{
    public class ParsedPackage
    {
        public string Name { get; set; } = string.Empty;
        public int Pins { get; set; }
        public int MinPins { get; set; }
        public int MaxPins { get; set; }
        public double Confidence { get; set; }
        public ICollection<string> Features { get; set; } = new List<string>();
        public ICollection<string> AlternateNames { get; set; } = new List<string>();
        public string Original { get; set; } = string.Empty;
        public string AlternateNotation => Pins > 0 ? $"{Name}-{Pins}" : string.Empty;
        public override string ToString()
            => $"{Name}{(Pins > 0 ? $" ({Pins})" : "")}";
    }

    public class PinCountDefinition
    {
        public int Min { get; set; }
        public int Max { get; set; }
        public int Value { get; set; }
        public PinCountDefinition(int value)
        {
            Min = Max = Value = value;
        }
        public PinCountDefinition(int value, int min, int max)
        {
            Min = min;
            Max = max;
            Value = value;
        }
    }

    public static class PackageTypeFormatter
    {
        private const string AllowChars = @"[^a-zA-Zμ0-9 \-\/]";
        private static readonly string[] _normalizedPackageNames = ["SOT-#", "SOD-#", "SOJ-#", "TSOT-#", "TO-#", "SC-#", "SMD-#", "DO-#", "SIP", "HSIP", "DIP", "CDIP", "PDIP", "FDIP", "CERDIP", "QIP", "SKDIP", "SDIP", "ZIP", "MDIP", "PDIP", "CCGA", "CGA", "CERPACK", "CQGP", "LLP", "LGA", "LTCC", "MCM", "MICRO SMDXT", "BCC", "CLCC", "LCC", "DLCC", "PLCC", "OPGA", "FCPGA", "PGA", "PPGA", "CPGA", "CFP", "CQFP", "BQFP", "DFN", "UDFN", "VDFN", "QDFN", "TDFN", "WFDFN", "UFDFN", "ETQFP", "PQFN", "PQFP", "LQFP", "QFN", "QFP", "MQFP", "HVQFN", "SIDEBRAZE", "TQFP", "VQFP", "TQFN", "VQFN", "WQFN", "UQFN", "ODFN", "SOP", "CSOP", "DSOP", "HSOP", "HSSOP", "HTSSOP", "mini-SOIC", "MSOP", "PSOP", "PSON", "QSOP", "SOIC", "SOJ", "SON", "SSOP", "TSOP", "TSSOP", "TVSOP", "VSOP", "VSSOP", "WSON", "USON", "BL", "CSP", "TCSP", "TDSP", "WCSP", "WL-CSP", "WLCSP", "PMCP", "Fan-out WLCSP", "eWLB", "MICRO SMD", "COB", "COF", "COW", "COG", "TAB", "FBGA", "LBGA", "DSBGA", "TEPBGA", "CBGA", "OBGA", "TFBGA", "PBGA", "MAP-BGA", "UCSP", "μBGA", "uBGA", "LFBGA", "TBGA", "SBGA", "UFBGA", "MELF", "SOD", "SOT", "μSOP", "uSOP", "LFCSP", "TDFN", "US8", "PLCC", "DO-214AA", "DO-214AB", "DO-214AC", "SMA", "SMB", "SMC", "IPAK", "DPAK", "D2PAK", "D3PAK", "DPAK2", "DPAK3", "D²PAK", "DPAK²", "D³PAK", "DPAK³", "MLP", "MLF", "TFSOP", "WFBGA", "Super-247", "TSM", "Mini Mold", "S-MIN", "Power Mini Mold", "UPAK", "PW-Min", "DDPAK", "DDDPAK", "uSMD", "μSMD", "TLC-USMD", "PowerFLEX", "VFBGA", "HC-49/US", "SOJ", "HU3PACK", "EEP", "MELF", "MiniMELF", "DIL", "PDMU", "SPM23", "SMPC", "TSOC", "uSiP", "WOG", "SNAPHAT"];
        private static readonly Lazy<IEnumerable<string>> NormalizedPackageNames = new Lazy<IEnumerable<string>>(() =>
        {
            return _normalizedPackageNames.Distinct().OrderByDescending(x => x.Length).ToArray();
        });
        private static readonly Dictionary<string, string[]> KnownSynonyms = new Dictionary<string, string[]>
        {
            { "DIP",  ["PDIP", "DIPP", "FDIP", "N Package", "DIL"] },
            { "DFN",  ["SON"] },
            { "DO-204AA",  ["DO-7"] },
            { "DO-204AB",  ["DO-14"] },
            { "DO-204AC",  ["DO-15"] },
            { "DO-204AH",  ["DO-35"] },
            { "DO-204AL",  ["DO-41"] },
            { "DO-34",  ["ALF2", "SOD-27", "SC-40"] },
            { "DPAK",  ["TO-252", "SOT-428", "SC-63", "TO-252AA", "TO-252-3", "TO-252-5", "TO-252-5-P1", "TO-252-5-P2", "KVU (R-PSFM-G5)", "Decawatt"] },
            { "D2PAK",  ["TO-263", "TO-263AB", "TO-262", "TO-279", "SOT404", "SMD-220", "DDPAK", "Double Decawatt"] },
            { "D3PAK",  ["TO-268", "TO-268AA", "TO-247", "DDDPAK", "TO-268-3", "Triple Decawatt", "Decawatt Package 3"] },
            { "EEP",  ["R-PDSS-T5", "R-PDSS-T6"] },
            { "HC-49/US",  ["HC49/US", "HC49-US"] },
            { "IPAK",  ["TO-251", "SOT-428"] },
            { "MELF",  ["DO-103AB"] },
            { "MiniMELF",  ["DO-103AA"] },
            { "QFN",  ["MLF", "MLP", "VQFN"] },
            { "PGA",  ["PPGA"] },
            { "SMA",  ["DO-214AC"] },
            { "SMB",  ["DO-214AA"] },
            { "SMC",  ["DO-214AB"] },
            { "SOD-123",  ["PDMU","DO-219AA","M1F","Mini2-F4-B","SC-109D"] },
            { "SOD-128",  ["CFP5","PMDTM","SOD-128-2","SOD-128 FLAT"] },
            { "SOD-323",  ["SC-76"] },
            { "SOD-523",  ["CASE 502", "SC-79", "SSMini-F5-B"] },
            { "SOD-723",  ["SC-104A", "VMD2"] },
            { "SOD-923",  ["SC-116A"] },
            { "SOIC",  ["SOP", "SO", "DSO", "11-4M1S", "751EP", "R-PDSO", "S8", "S8E", "SOIC8E", "SOT162-1", "SSOP4", "LSOP04", "WSON"] },
            { "SOT-1571-1",  ["QFP"] },
            { "SOT-162-1",  ["SOIC-16W"] },
            { "SOT-1226-2",  ["X2SON5"] },
            { "SOT-1040-1",  ["Plastic Flip Chip Strap Package"] },
            { "SOT-23-5",  ["SC-74A","SOT-753","5-TSOP"] },
            { "SOT-23",  ["TO-236", "TO-236AB", "SC-59", "TSM", "S-MIN", "1-3G1G", "2-3AB1A", "318D-04", "526AG", "MPAK", "S3", "SMT3", "SOT-346", "SSOT3", "SuperSOT-3", "TO-236AA", "TO-236AB", "TO-236MOD", "RT-3"] },
            { "SOT-223",  ["TO-261AA", "TO-261", "DCY", "KC-3", "TO-261-4", "PG-SOT-223", "R-PDSO-G4", "MP04A"] },
            { "SOT-353",  ["CASE 419A","SC-70-5","SC-88A"] },
            { "SOT-363",  ["TSSOP6","SC-88"] },
            { "SOT-416",  ["2-2H1S","EMT3","SC-75","SC-75A","SOT-416-3"] },
            { "SOT-490",  ["463C-03","SC-89-3"] },
            { "SOT-563",  ["463C-03","DRL|6","DRL0006A","SC-89-6"] },
            { "SOT-583",  ["FCSOT"] },
            { "SOT-89-3",  ["TO-243-AA","3-HSIP","PW-MINI","SC-62"] },
            { "SOT-723",  ["VMT3","Case 631AA","SC-105AA"] },
            { "Super-247",  ["TO-274"] },
            { "TO-206",  ["H02A", "H03C", "H03H", "H04A", "H04D", "TO-18"] },
            { "TO-206-AA",  ["TO-18"] },
            { "TO-206-AB",  ["TO-46"] },
            { "TO-206-AC",  ["TO-52"] },
            { "TO-206-AF",  ["TO-72"] },
            { "TO-218",  ["SOT-93", "CASE 340D-02"] },
            { "TO-220",  ["TO-220AB"] },
            { "TO-220F",  ["SOT-186A", "TO-220FP"] },
            { "TO-247AC",  ["SOD-429", "CASE 340L-02"] },
            { "TO-252-3",  ["SC-63", "SOT-428"] },
            { "TO-277A",  ["SMPC"] },
            { "TO-3",  ["TO-204AA", "TO-204AD"] },
            { "TO-39",  ["TO-205AD"] },
            { "TO-5",  ["H-03B"] },
            { "TO-92",  ["TO-226AA", "Z Package", "TO-226-3", "TO-92-3", "Formed Leads", "29-11", "E-Line"] },
            { "TO-92MOD",  ["TO-226AB", "TO-92/18"] },
            { "TO-93",  ["TO-209AB"] },
            { "TO-PMOD-7",  ["NDW", "TZA07A"] },
            { "TSSOP",  ["PWP (PowerPAD)", "PWP", "SOT1171-2", "MO-153"] },
            { "uSiP",  ["MicroSiP", "SIL"] },
            { "USON",  ["PG-USON"] },
            { "WSON",  ["DMB", "PWSON", "DQD"] },
            { "VSSOP",  ["MSOP", "μMAX", "RM-10", "MSE"] },
            { "WLCSP",  ["LCSPW", "DSBGA"] },
            { "008004",  ["0201 (Metric)"] },
            { "01005",  ["0402 (Metric)"] },
            { "0201",  ["0603 (Metric)"] },
            { "0402",  ["1005 (Metric)"] },
            { "0603",  ["1608 (Metric)"] },
            { "0805",  ["2012 (Metric)"] },
            { "1206",  ["3216 (Metric)"] },
            { "1210",  ["3225 (Metric)"] },
            { "1515",  ["4040 (Metric)"] },
            { "1806",  ["4516 (Metric)"] },
            { "1812",  ["4532 (Metric)"] },
            { "2512",  ["6332 (Metric)"] },
        };
        private static readonly Dictionary<string, PinCountDefinition> KnownPinDefinitions = new Dictionary<string, PinCountDefinition>()
        {
            { "DPAK", new PinCountDefinition(3, 3, 5) },
            { "D2PAK", new PinCountDefinition(3, 3, 7) },
            { "D3PAK", new PinCountDefinition(3) },
            { "TO-9", new PinCountDefinition(3) },
            { "TO-12", new PinCountDefinition(4) },
            { "TO-16", new PinCountDefinition(3) },
            { "TO-33", new PinCountDefinition(4) },
            { "TO-39", new PinCountDefinition(3) },
            { "TO-42", new PinCountDefinition(3) },
            { "TO-73", new PinCountDefinition(12, 11, 12) },
            { "TO-74", new PinCountDefinition(10, 9, 10) },
            { "TO-75", new PinCountDefinition(6) },
            { "TO-76", new PinCountDefinition(8) },
            { "TO-77", new PinCountDefinition(8) },
            { "TO-78", new PinCountDefinition(8, 5, 8) },
            { "TO-79", new PinCountDefinition(8, 5, 8) },
            { "TO-80", new PinCountDefinition(8, 5, 8) },
            { "TO-92", new PinCountDefinition(3) },
            { "TO-96", new PinCountDefinition(10, 9, 10) },
            { "TO-97", new PinCountDefinition(10, 9, 10) },
            { "TO-99", new PinCountDefinition(8) },
            { "TO-100", new PinCountDefinition(10, 9, 10) },
            { "TO-101", new PinCountDefinition(12, 11, 12) },
            { "TO-126", new PinCountDefinition(3) },
            { "TO-220", new PinCountDefinition(3) },
            { "TO-247", new PinCountDefinition(3) },
            { "SOT-1571-1", new PinCountDefinition(48) },
            { "SOT-1226-2", new PinCountDefinition(8) },
            { "SOT-162-1", new PinCountDefinition(16) },
            { "SOT-143", new PinCountDefinition(4) },
            { "SOT-23", new PinCountDefinition(3, 3, 8) },
            { "SOT-23F", new PinCountDefinition(3) },
            { "SOT-223", new PinCountDefinition(3, 3, 5) },
            { "SOT-323", new PinCountDefinition(3, 3, 8) },
            { "SOT-353", new PinCountDefinition(5) },
            { "SOT-416", new PinCountDefinition(3) },
            { "SOT-428", new PinCountDefinition(3) },
            { "SOT-457", new PinCountDefinition(6) },
            { "SOT-490", new PinCountDefinition(3) },
            { "SOT-523", new PinCountDefinition(3) },
            { "SOT-563", new PinCountDefinition(6) },
            { "SOT-583", new PinCountDefinition(8) },
            { "SOT-665", new PinCountDefinition(5) },
            { "SOT-723", new PinCountDefinition(3) },
            { "SOT-753", new PinCountDefinition(5) },
            { "SOT-762-1", new PinCountDefinition(14) },
            { "SOT-883", new PinCountDefinition(3) },
            { "SOT-886", new PinCountDefinition(6) },
            { "SOT-902", new PinCountDefinition(8) },
            { "SOT-93", new PinCountDefinition(3) },
            { "SPM23", new PinCountDefinition(23) },
            { "SuperSOT-3", new PinCountDefinition(3) },
            { "SOD-110", new PinCountDefinition(2) },
            { "SOD-123", new PinCountDefinition(2) },
            { "SOD-128", new PinCountDefinition(2) },
            { "SOD-323", new PinCountDefinition(2) },
            { "SOD-429", new PinCountDefinition(3) },
            { "SOD-523", new PinCountDefinition(3) },
            { "SOD-57", new PinCountDefinition(2) },
            { "SOD-64", new PinCountDefinition(2) },
            { "SOD-723", new PinCountDefinition(2) },
            { "SOD-882", new PinCountDefinition(2) },
            { "SOD-923", new PinCountDefinition(2) },
        };

        /// <summary>
        /// Format a raw package name into a clean package name
        /// </summary>
        /// <param name="packageName"></param>
        /// <returns></returns>
        public static ParsedPackage ParsePackageName(string? packageName)
        {
            if (string.IsNullOrEmpty(packageName)) return new ParsedPackage() { Original = packageName ?? string.Empty };

            var partNames = packageName.ContextSensitiveSplit(",", StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            // if there are multiple part names, choose the key of the one we have an alternate definition for.
            // if we don't have it, then sort by name (to make sure choices are consistent with different array orders)
            var choices = new List<ParsedPackage>();
            foreach (var partName in partNames)
            {
                var original = partName.ToString();
                var parsedPackage = new ParsedPackage() { Original = packageName };
                var cleanPackageName = partName.ToString().Trim();

                // do all processing here
                if (TryGetPinCountFromReplacement(partName, cleanPackageName, delimiters: ["-"], out var pinCount))
                    parsedPackage.Pins = pinCount;
                var parenthesisMatch = Regex.Match(cleanPackageName, @"\(([^)]+)\)");
                if (parenthesisMatch != null && parenthesisMatch.Success)
                {
                    var feature = parenthesisMatch.Groups[0].Value.Trim('(', ')').Trim();
                    if (!parsedPackage.Features.Contains(feature))
                        parsedPackage.Features.Add(feature);
                }
                var cleanPackageNameOriginal = cleanPackageName.ToString();
                // look for (3 Leads + Tab) to indicate pin counts
                var pattern = @"(?<=\()\d+(?=\s*Leads \+)";
                var matches = Regex.Match(cleanPackageName, pattern);
                if (matches.Success)
                    parsedPackage.Pins = int.Parse(matches.Value) + 1; // tab includes a pin
                else
                {
                    // also look for (3 Leads) to indicate pin counts
                    pattern = @"(?<=\()\d+(?=\s*Leads)";
                    matches = Regex.Match(cleanPackageName, pattern);
                    if (matches.Success)
                        parsedPackage.Pins = int.Parse(matches.Value);
                }
                // formats package names such as: 8-Lead SOIC to SOIC
                cleanPackageName = Regex.Replace(cleanPackageName, "(?i)[0-9]+[- ](lead|pin)", "").Trim();
                if (TryGetPinCountFromReplacement(cleanPackageNameOriginal, cleanPackageName, delimiters: ["-", " "], out pinCount))
                    parsedPackage.Pins = pinCount;
                // remove everything between parenthesis (might not be complete)
                cleanPackageName = Regex.Replace(cleanPackageName, @"\(([^)]+)\)", "");
                var parenthesisIndex = cleanPackageName.IndexOf('(');
                if (parenthesisIndex >= 0)
                {
                    cleanPackageName = cleanPackageName.Substring(0, parenthesisIndex).Trim();
                }
                var parenthesisStartIndex = cleanPackageName.IndexOf(')');
                if (parenthesisStartIndex >= 0)
                {
                    cleanPackageName = cleanPackageName.Substring(parenthesisStartIndex + 1, cleanPackageName.Length - parenthesisStartIndex - 1).Trim();
                }
                // remove line endings, tabs
                cleanPackageName = cleanPackageName.Replace("\n", "");
                cleanPackageName = cleanPackageName.Replace("\r", "");
                cleanPackageName = cleanPackageName.Replace("\t", "");
                // allow numeric only packages to pass through
                if (int.TryParse(cleanPackageName, out var _))
                {
                    parsedPackage.Name = cleanPackageName.Trim();
                    parsedPackage.MinPins = parsedPackage.MaxPins = parsedPackage.Pins = 2;
                    // look for alternate names
                    parsedPackage = PopulateAlternateNames(parsedPackage);
                    parsedPackage.Confidence = 1.0;
                    choices.Add(parsedPackage);
                    continue;
                }

                // long form substitutions
                cleanPackageName = cleanPackageName.Replace("plastic dual in-line package", "DIP", StringComparison.InvariantCultureIgnoreCase);
                cleanPackageName = cleanPackageName.Replace("dual in-line package", "DIP", StringComparison.InvariantCultureIgnoreCase);
                cleanPackageName = cleanPackageName.Replace("single in-line package", "SIP", StringComparison.InvariantCultureIgnoreCase);
                cleanPackageName = cleanPackageName.Replace("quad in-line package", "QIP", StringComparison.InvariantCultureIgnoreCase);
                cleanPackageName = cleanPackageName.Replace("very small outline package", "VSOP", StringComparison.InvariantCultureIgnoreCase);
                cleanPackageName = cleanPackageName.Replace("small outline ic", "SOIC", StringComparison.InvariantCultureIgnoreCase);
                // remove commonly found extension words
                var commonFeatures = new[] { "exposed pad", "formed leads", "long body", "full pack", "metal can" };
                foreach (var commonFeature in commonFeatures)
                {
                    if (cleanPackageName.Contains(commonFeature, StringComparison.InvariantCultureIgnoreCase)) parsedPackage.Features.Add(commonFeature);
                    cleanPackageName = cleanPackageName.Replace(commonFeature, "", StringComparison.InvariantCultureIgnoreCase);
                }

                // attempt to match and normalize the package name
                cleanPackageName = NormalizePackageNames(cleanPackageName, out var matchedPackageNameWithValidNumber, out var removedPortions);
                if (matchedPackageNameWithValidNumber)
                    parsedPackage.Confidence += 0.4;
                cleanPackageName = NormalizeAlternateNames(cleanPackageName);
                if (!string.IsNullOrEmpty(removedPortions))
                {
                    if (TryGetPinCountFromReplacement(removedPortions, delimiters: [" "], out pinCount))
                    {
                        parsedPackage.Pins = pinCount;
                        parsedPackage.Confidence += 0.1;
                    }
                }

                // do a final replacement for removing leading/trailing pin counts
                if (!matchedPackageNameWithValidNumber)
                {
                    foreach (var officialName in NormalizedPackageNames.Value)
                    {
                        if (cleanPackageName.Contains(officialName, StringComparison.InvariantCultureIgnoreCase))
                        {
                            parsedPackage.Confidence += 0.1;
                            // check for 8-SOIC
                            pattern = $"^[\\d]+[-]{officialName}";
                            matches = Regex.Match(cleanPackageName, pattern, RegexOptions.IgnoreCase);
                            if (matches.Success)
                            {
                                parsedPackage.Confidence += 0.2;
                                original = cleanPackageName.ToString();
                                cleanPackageName = Regex.Replace(cleanPackageName, pattern, officialName, RegexOptions.IgnoreCase);
                                if (TryGetPinCountFromReplacement(original, cleanPackageName, delimiters: ["-"], out pinCount))
                                {
                                    parsedPackage.Confidence += 0.2;
                                    parsedPackage.Pins = pinCount;
                                }
                            }
                            // check for SOIC-8
                            pattern = $"^{officialName}[-][\\d]+";
                            matches = Regex.Match(cleanPackageName, pattern, RegexOptions.IgnoreCase);
                            if (matches.Success)
                            {
                                parsedPackage.Confidence += 0.2;
                                original = cleanPackageName.ToString();
                                cleanPackageName = Regex.Replace(cleanPackageName, pattern, officialName, RegexOptions.IgnoreCase);
                                if (TryGetPinCountFromReplacement(original, cleanPackageName, delimiters: ["-"], out pinCount))
                                {
                                    parsedPackage.Confidence += 0.2;
                                    parsedPackage.Pins = pinCount;
                                }
                            }
                        }
                    }
                }

                // for unknown package types that are prefixed with a pin count, such as 3-UNKNOWN, handle them.
                if (!string.IsNullOrEmpty(cleanPackageName))
                {
                    original = cleanPackageName.ToString();
                    cleanPackageName = Regex.Replace(cleanPackageName, @"^[\d]+[-]", "");
                    removedPortions = original.Replace(cleanPackageName, "");
                    if (TryGetPinCountFromReplacement(removedPortions, delimiters: ["-"], out pinCount))
                    {
                        parsedPackage.Pins = pinCount;
                        parsedPackage.Confidence += 0.1;
                    }
                }

                // replace all characters not in the pattern
                cleanPackageName = Regex.Replace(cleanPackageName, AllowChars, "");
                parsedPackage.Name = cleanPackageName.Trim();
                // look for alternate names
                parsedPackage = PopulateAlternateNames(parsedPackage);

                // finally if pins are not known, check if we have a known default
                PinCountDefinition? definition = null;
                if (KnownPinDefinitions.ContainsKey(parsedPackage.Name))
                {
                    definition = KnownPinDefinitions[parsedPackage.Name];
                    parsedPackage.Confidence += 0.2;
                }
                else if (KnownPinDefinitions.ContainsKey(parsedPackage.AlternateNotation))
                {
                    definition = KnownPinDefinitions[parsedPackage.AlternateNotation];
                    parsedPackage.Confidence += 0.2;
                }
                else
                {
                    // check alternate names
                    foreach (var alternateName in parsedPackage.AlternateNames)
                    {
                        if (KnownPinDefinitions.ContainsKey(alternateName))
                        {
                            definition = KnownPinDefinitions[alternateName];
                            parsedPackage.Confidence += 0.1;
                            break;
                        }
                    }
                }
                if (definition != null)
                {
                    if (definition.Value > parsedPackage.Pins)
                        parsedPackage.Pins = definition.Value;
                    parsedPackage.MinPins = definition.Min;
                    parsedPackage.MaxPins = definition.Max;
                }


                if (!string.IsNullOrEmpty(parsedPackage.Name))
                {
                    if (parsedPackage.Confidence > 1.0)
                        parsedPackage.Confidence = 1.0;
                    choices.Add(parsedPackage);
                }
            }

            // populate alternate names if they don't exist (and they were provided)
            if (choices.Count > 1)
            {
                foreach (var choice in choices)
                {
                    foreach (var choice2 in choices)
                    {
                        if (choice.Name != choice2.Name)
                        {
                            // merge data between choices
                            if (choice2.Pins > choice.Pins)
                                choice.Pins = choice2.Pins;
                            choice.Features = choice.Features.Union(choice2.Features).ToList();
                            if (!choice.AlternateNames.Contains(choice2.Name, StringComparer.InvariantCultureIgnoreCase))
                                choice.AlternateNames.Add(choice2.Name);
                            if (!choice.AlternateNames.Contains(choice2.AlternateNotation, StringComparer.InvariantCultureIgnoreCase))
                                choice.AlternateNames.Add(choice2.AlternateNotation);
                        }
                    }
                }
            }

            // finally, choose the known name if its an alternate name
            foreach (var choice in choices)
            {
                var key = KnownSynonyms.Where(x => x.Value.Contains(choice.Name, StringComparer.InvariantCultureIgnoreCase))
                    .Select(x => x.Key)
                    .FirstOrDefault();
                if (key != null && !choices.Any(x => x.Name == key))
                {
                    // since we are swapping out the name, make sure its in alternate names
                    if (!choice.AlternateNames.Contains(choice.Name)) choice.AlternateNames.Add(choice.Name);
                    if (!choice.AlternateNames.Contains(choice.AlternateNotation)) choice.AlternateNames.Add(choice.AlternateNotation);
                    choice.Name = key;
                }
            }

            // add alternate names to all choices
            foreach (var choice in choices)
            {
                var entity = KnownSynonyms.Where(x => x.Key.Equals(choice.Name, StringComparison.InvariantCultureIgnoreCase))
                    .Select(x => x.Key)
                    .FirstOrDefault();
                if (!string.IsNullOrEmpty(entity) && partNames.StartsWithAny(choice.Name, StringComparison.InvariantCultureIgnoreCase))
                {
                    return choice;
                }
            }
            foreach (var choice in choices)
            {
                // check for alternatives
                var entity = KnownSynonyms.Where(x => x.Value.Contains(choice.Name, StringComparer.InvariantCultureIgnoreCase))
                    .Select(x => x.Key)
                    .FirstOrDefault();
                if (!string.IsNullOrEmpty(entity) && partNames.StartsWithAny(entity, StringComparison.InvariantCultureIgnoreCase))
                {
                    return choice;
                }
            }

            // no alternatives found, choose the first one ordered by name
            if (choices.Any(x => x.Confidence > 0))
                return choices.Where(x => x.Confidence > 0).OrderBy(x => x.Name).First() ?? new ParsedPackage();
            return choices.OrderBy(x => x.Name).First() ?? new ParsedPackage();
        }

        /// <summary>
        /// Format a raw package name into a clean package name
        /// </summary>
        /// <param name="packageName"></param>
        /// <returns></returns>
        public static string FormatPackageName(string packageName)
        {
            if (string.IsNullOrEmpty(packageName)) return string.Empty;

            var partNames = packageName.ContextSensitiveSplit(",", StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            // if there are multiple part names, choose the key of the one we have an alternate definition for.
            // if we don't have it, then sort by name (to make sure choices are consistent with different array orders)
            var choices = new List<string>();
            foreach (var partName in partNames)
            {
                var cleanPackageName = partName.ToString().Trim();

                // formats package names such as: 8-SOIC (0.154",3.90mm Width) to SOIC, D2PAK (2 Leads + Tab) to D2PAK, TO-92-3 (TO-226AA) to TO-92-3
                cleanPackageName = Regex.Replace(cleanPackageName, "(?i)[0-9]+[- ](lead|pin)", "").Trim();
                // remove everything between parenthesis (might not be complete)
                cleanPackageName = Regex.Replace(cleanPackageName, @"\(([^)]+)\)", "");
                var parenthesisIndex = cleanPackageName.IndexOf('(');
                if (parenthesisIndex >= 0)
                {
                    cleanPackageName = cleanPackageName.Substring(0, parenthesisIndex).Trim();
                }
                var parenthesisStartIndex = cleanPackageName.IndexOf(')');
                if (parenthesisStartIndex >= 0)
                {
                    cleanPackageName = cleanPackageName.Substring(parenthesisStartIndex + 1, cleanPackageName.Length - parenthesisStartIndex - 1).Trim();
                }
                // remove line endings, tabs
                cleanPackageName = cleanPackageName.Replace("\n", "");
                cleanPackageName = cleanPackageName.Replace("\r", "");
                cleanPackageName = cleanPackageName.Replace("\t", "");
                // allow numeric only packages to pass through
                if (int.TryParse(cleanPackageName, out var _))
                {
                    choices.Add(cleanPackageName.Trim());
                    continue;
                }
                // long form substitutions
                cleanPackageName = cleanPackageName.Replace("plastic dual in-line package", "DIP", StringComparison.InvariantCultureIgnoreCase);
                cleanPackageName = cleanPackageName.Replace("dual in-line package", "DIP", StringComparison.InvariantCultureIgnoreCase);
                cleanPackageName = cleanPackageName.Replace("single in-line package", "SIP", StringComparison.InvariantCultureIgnoreCase);
                cleanPackageName = cleanPackageName.Replace("quad in-line package", "QIP", StringComparison.InvariantCultureIgnoreCase);
                cleanPackageName = cleanPackageName.Replace("very small outline package", "VSOP", StringComparison.InvariantCultureIgnoreCase);
                cleanPackageName = cleanPackageName.Replace("small outline ic", "SOIC", StringComparison.InvariantCultureIgnoreCase);
                // remove commonly found extension words
                var commonFeatures = new[] { "exposed pad", "formed leads", "long body", "full pack" };
                foreach (var commonFeature in commonFeatures)
                {
                    cleanPackageName = cleanPackageName.Replace(commonFeature, "", StringComparison.InvariantCultureIgnoreCase);
                }

                // attempt to match and normalize the package name
                cleanPackageName = NormalizePackageNames(cleanPackageName, out var matchedPackageNameWithValidNumber, out var removedPortions);
                cleanPackageName = NormalizeAlternateNames(cleanPackageName);
                // do a final replacement for removing leading/trailing pin counts
                if (!matchedPackageNameWithValidNumber)
                {
                    foreach (var officialName in NormalizedPackageNames.Value)
                    {
                        if (cleanPackageName.Contains(officialName, StringComparison.InvariantCultureIgnoreCase))
                        {
                            cleanPackageName = Regex.Replace(cleanPackageName, $"^[\\d]+[-]{officialName}", officialName, RegexOptions.IgnoreCase);
                            cleanPackageName = Regex.Replace(cleanPackageName, $"^{officialName}[-][\\d]+", officialName, RegexOptions.IgnoreCase);
                        }
                    }
                }
                // replace all characters not in the pattern
                cleanPackageName = Regex.Replace(cleanPackageName, AllowChars, "");
                if (!string.IsNullOrEmpty(cleanPackageName))
                    choices.Add(cleanPackageName.Trim());
            }

            // finally, choose the known name if its an alternate name
            for (var i = 0; i < choices.Count; i++)
            {
                var key = KnownSynonyms.Where(x => x.Value.Contains(choices[i], StringComparer.InvariantCultureIgnoreCase))
                    .Select(x => x.Key)
                    .FirstOrDefault();
                if (key != null && !choices.Any(x => x == key))
                {
                    choices[i] = key;
                }
            }

            // choose the appropriate package
            foreach (var choice in choices)
            {
                var entity = KnownSynonyms.Where(x => x.Key.Equals(choice, StringComparison.InvariantCultureIgnoreCase))
                    .Select(x => x.Key)
                    .FirstOrDefault();
                if (!string.IsNullOrEmpty(entity) && partNames.StartsWithAny(choice, StringComparison.InvariantCultureIgnoreCase))
                {
                    return choice;
                }
            }
            foreach (var choice in choices)
            {
                // check for alternatives
                var entity = KnownSynonyms.Where(x => x.Value.Contains(choice, StringComparer.InvariantCultureIgnoreCase))
                    .Select(x => x.Key)
                    .FirstOrDefault();
                if (!string.IsNullOrEmpty(entity) && partNames.StartsWithAny(entity, StringComparison.InvariantCultureIgnoreCase))
                {
                    return choice;
                }
            }

            // no alternatives found, choose the first one ordered by name
            if (choices.Any(x => x.Length > 2))
                return choices.Where(x => x.Length > 2).OrderBy(x => x).First();
            return choices.OrderBy(x => x).FirstOrDefault() ?? string.Empty;
        }

        /// <summary>
        /// Given a comma-delimited list of formatted package names, only return the root package names
        /// </summary>
        /// <param name="packageNames"></param>
        /// <returns></returns>
        public static ICollection<string> FilterPackageList(string packageNames)
        {
            if (string.IsNullOrEmpty(packageNames)) return Array.Empty<string>();
            return FilterPackageList(packageNames.Split(",", StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries));
        }

        /// <summary>
        /// Given a list of formatted package names, only return the root package names
        /// </summary>
        /// <param name="packageNames"></param>
        /// <returns></returns>
        public static ICollection<string> FilterPackageList(ICollection<string> packageNames)
        {
            if (!packageNames.Any()) return packageNames;
            var results = new List<string>();
            foreach(var packageName in packageNames)
            {
                var knownSynonymKey = KnownSynonyms.Where(x => x.Value.Contains(packageName))
                    .Select(x => x.Key)
                    .FirstOrDefault();
                if (!string.IsNullOrEmpty(knownSynonymKey))
                {
                    results.Add(knownSynonymKey);
                } else
                {
                    results.Add(packageName);
                }
            }
            return results.Distinct().OrderBy(x => x).ToList();
        }

        private static ParsedPackage PopulateAlternateNames(ParsedPackage parsedPackage)
        {
            var alternateNames = new List<string>();
            foreach (var alternateName in KnownSynonyms)
            {
                if (alternateName.Key.Equals(parsedPackage.Name))
                    alternateNames.AddRange(alternateName.Value);
                else if (alternateName.Value.Contains(parsedPackage.Name))
                {
                    alternateNames.AddRange(alternateName.Key);
                    alternateNames.AddRange(alternateName.Value);
                }
                if (!string.IsNullOrEmpty(parsedPackage.AlternateNotation))
                {
                    if (alternateName.Key.Equals(parsedPackage.AlternateNotation))
                        alternateNames.AddRange(alternateName.Value);
                    else if (alternateName.Value.Contains(parsedPackage.AlternateNotation))
                    {
                        alternateNames.AddRange(alternateName.Key);
                        alternateNames.AddRange(alternateName.Value);
                    }
                }
            }
            alternateNames = alternateNames.Where(x => !x.Equals(parsedPackage.Name)).Distinct().ToList();
            parsedPackage.AlternateNames = alternateNames;
            return parsedPackage;
        }

        private static string NormalizeAlternateNames(string output)
        {
            var replacements = new Dictionary<string, string>
            {
                { "D²PAK", "D2PAK" },
                { "DPAK²", "D2PAK" },
                { "D³PAK", "D3PAK" },
                { "DPAK³", "D3PAK" },
            };
            foreach (var replacement in replacements)
            {
                if (output.Contains(replacement.Key, StringComparison.InvariantCultureIgnoreCase))
                {
                    output = output.Replace(replacement.Key, replacement.Value, StringComparison.InvariantCultureIgnoreCase);
                }
            }
            return output;
        }

        private static string NormalizePackageNames(string output, out bool matchedPackageNameWithValidNumber, out string removedPortions)
        {
            removedPortions = string.Empty;
            matchedPackageNameWithValidNumber = false;
            var normalized = output.ToString();

            foreach (var officialName in NormalizedPackageNames.Value)
            {
                if (officialName.Contains("#") && normalized.Contains(officialName.Replace("#", string.Empty), StringComparison.InvariantCultureIgnoreCase))
                {
                    for (var i = 1300; i >= 2; i--)
                    {
                        if (normalized.Contains(officialName.Replace("#", $"{i}"), StringComparison.InvariantCultureIgnoreCase))
                        {
                            var officialType = officialName.Replace("#", $"{i}");
                            // match the simple type with optional letters at the end but nothing else.
                            // for types such as TO-252AA, we want to keep the AA but remove any other trailing characters
                            var matches = Regex.Match(normalized, $"{officialType}([A-Z]*)?", RegexOptions.IgnoreCase);
                            if (matches.Success)
                            {
                                // take the match, but use the capitalization of the official type
                                normalized = matches.Value.Replace(officialType, officialType, StringComparison.InvariantCultureIgnoreCase);
                            }
                            var parts = normalized.Split(" ");
                            // remove any other parts of the string that don't contain the type
                            normalized = parts.Where(x => x.Contains(officialType, StringComparison.InvariantCultureIgnoreCase)).FirstOrDefault() ?? officialType;
                            matchedPackageNameWithValidNumber = true;
                            removedPortions = output.Replace(normalized, string.Empty, StringComparison.InvariantCultureIgnoreCase);
                            return normalized;
                        }
                    }
                }
            }
            return normalized;
        }

        private static bool TryGetPinCountFromReplacement(string original, string replaceResult, string[] delimiters, out int pinCount)
        {
            pinCount = 0;
            if (delimiters.Length == 0) throw new ArgumentNullException(nameof(delimiters));
            if (string.IsNullOrEmpty(original) || string.IsNullOrEmpty(replaceResult)) return false;
            var removed = original.Replace(replaceResult, string.Empty);
            if (removed.Length > 0)
            {
                return TryGetPinCountFromReplacement(removed, delimiters, out pinCount);
            }
            return false;
        }

        private static bool TryGetPinCountFromReplacement(string removed, string[] delimiters, out int pinCount)
        {
            pinCount = 0;
            if (delimiters.Length == 0) throw new ArgumentNullException(nameof(delimiters));
            if (string.IsNullOrEmpty(removed)) return false;
            var parts = removed.Split(delimiters, StringSplitOptions.RemoveEmptyEntries);
            foreach (var part in parts)
            {
                if (int.TryParse(part, out int pins))
                {
                    pinCount = Math.Abs(pins);
                    return true;
                }
            }
            return false;
        }
    }
}
