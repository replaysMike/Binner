using Binner.Model;
using EnumsNET;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Binner.Common.Integrations
{
    public static class CommonPartValueFormatter
    {
        public static double DetectValueFromText(string? value)
        {
            if (string.IsNullOrEmpty(value))
                return 0d;
            var resultString = Regex.Match(value, @"\d+").Value;
            if (double.TryParse(resultString, out var result))
                return result;
            return 0d;
        }

        public static ParametricUnits DetectUnitsFromText(string? value)
        {
            if (string.IsNullOrEmpty(value))
                return ParametricUnits.None;
            // if its a number, and matches the entire value then treat it as a count as there are no other details
            if (DetectValueFromText(value).ToString().Equals(value)) return ParametricUnits.Count;

            var result = new Dictionary<ParametricUnits, int>();

            Enum.GetValues<ParametricUnits>().ToList().ForEach(x =>
            {
                var t = x.GetAttributes();
                
                var att = t.FirstOrDefault() as AbbreviationAttribute;
                
                if (att == null) return;
                foreach(var val in att.Abbreviations)
                {
                    if (val == null) return;
                    var caseSensitiveMatch = value.Contains(val, StringComparison.InvariantCulture);
                    var caseInsensitiveMatch = value.Contains(val, StringComparison.InvariantCultureIgnoreCase);
                    if (caseSensitiveMatch)
                    {
                        if (result.ContainsKey(x))
                            result[x] = val.Length + 1;
                        else
                            result.Add(x, val.Length + 1);
                    }
                    if (caseInsensitiveMatch)
                    {
                        if (result.ContainsKey(x))
                            result[x] = val.Length;
                        else
                            result.Add(x, val.Length);
                    }
                }
            });
            if (!result.Any()) return ParametricUnits.None;
            return result.OrderByDescending(x => x.Value).First().Key;
        }
    }
}
