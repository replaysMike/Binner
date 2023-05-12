using Ganss.Xss;

namespace Binner.StorageProvider.EntityFrameworkCore
{
    public static class SvgSanitizer
    {
        /// <summary>
        /// Sanitize an SVG string
        /// </summary>
        /// <param name="svg"></param>
        /// <returns></returns>
        public static string Sanitize(string svg)
        {
            var sanitizer = new HtmlSanitizer();
            sanitizer.AllowedTags.Clear();
            sanitizer.AllowedTags.Add("svg");
            sanitizer.AllowedTags.Add("path");
            sanitizer.AllowedTags.Add("pattern");
            sanitizer.AllowedTags.Add("polygon");
            sanitizer.AllowedTags.Add("polyline");
            sanitizer.AllowedTags.Add("rect");
            sanitizer.AllowedTags.Add("text");
            sanitizer.AllowedTags.Add("mpath");
            sanitizer.AllowedTags.Add("mask");
            sanitizer.AllowedTags.Add("line");
            sanitizer.AllowedTags.Add("ellipse");
            sanitizer.AllowedTags.Add("circle");
            sanitizer.AllowedTags.Add("clipPath");
            sanitizer.AllowedTags.Add("ellipse");
            sanitizer.AllowedTags.Add("animate");
            sanitizer.AllowedTags.Add("g");
            sanitizer.AllowedTags.Add("radialGradient");
            sanitizer.AllowedTags.Add("use");
            sanitizer.AllowedTags.Add("stop");
            sanitizer.AllowedTags.Add("defs");
            sanitizer.AllowedTags.Add("filter");
            sanitizer.AllowedTags.Add("feGaussianBlur");
            sanitizer.AllowedTags.Add("feDropShadow");
            sanitizer.AllowedTags.Add("feBlend");
            sanitizer.AllowedTags.Add("feOffset");
            sanitizer.AllowedTags.Add("clipPath");
            sanitizer.AllowedTags.Add("set");

            sanitizer.AllowedAttributes.Clear();
            sanitizer.AllowedAttributes.Add("xmlns");
            sanitizer.AllowedAttributes.Add("xmlns:xlink");
            sanitizer.AllowedAttributes.Add("version");
            sanitizer.AllowedAttributes.Add("baseProfile");
            sanitizer.AllowedAttributes.Add("viewBox");
            sanitizer.AllowedAttributes.Add("d");
            sanitizer.AllowedAttributes.Add("pathLength");
            sanitizer.AllowedAttributes.Add("stroke");
            sanitizer.AllowedAttributes.Add("stroke-width");
            sanitizer.AllowedAttributes.Add("fill");
            sanitizer.AllowedAttributes.Add("fill-rule");
            sanitizer.AllowedAttributes.Add("id");
            sanitizer.AllowedAttributes.Add("mask");
            sanitizer.AllowedAttributes.Add("stroke-linejoin");
            sanitizer.AllowedAttributes.Add("stroke-linecap");
            sanitizer.AllowedAttributes.Add("transform");
            sanitizer.AllowedAttributes.Add("cx");
            sanitizer.AllowedAttributes.Add("cy");
            sanitizer.AllowedAttributes.Add("dx");
            sanitizer.AllowedAttributes.Add("dy");
            sanitizer.AllowedAttributes.Add("rx");
            sanitizer.AllowedAttributes.Add("ry");
            sanitizer.AllowedAttributes.Add("r");
            sanitizer.AllowedAttributes.Add("x");
            sanitizer.AllowedAttributes.Add("y");
            sanitizer.AllowedAttributes.Add("width");
            sanitizer.AllowedAttributes.Add("height");
            sanitizer.AllowedAttributes.Add("font-family");
            sanitizer.AllowedAttributes.Add("font-size");
            sanitizer.AllowedAttributes.Add("attributeName");
            sanitizer.AllowedAttributes.Add("calcMode");
            sanitizer.AllowedAttributes.Add("additive");
            sanitizer.AllowedAttributes.Add("accumulate");
            sanitizer.AllowedAttributes.Add("from");
            sanitizer.AllowedAttributes.Add("to");
            sanitizer.AllowedAttributes.Add("begin");
            sanitizer.AllowedAttributes.Add("in");
            sanitizer.AllowedAttributes.Add("in2");
            sanitizer.AllowedAttributes.Add("mode");
            sanitizer.AllowedAttributes.Add("dur");
            sanitizer.AllowedAttributes.Add("opacity");
            sanitizer.AllowedAttributes.Add("xlink:href");
            sanitizer.AllowedAttributes.Add("gradientUnits");
            sanitizer.AllowedAttributes.Add("offset");
            sanitizer.AllowedAttributes.Add("stop-color");
            sanitizer.AllowedAttributes.Add("stdDeviation");
            sanitizer.AllowedAttributes.Add("filterRes");
            sanitizer.AllowedAttributes.Add("filterUnits");
            sanitizer.AllowedAttributes.Add("primitiveUnits");
            sanitizer.AllowedAttributes.Add("flood-color");
            sanitizer.AllowedAttributes.Add("flood-opacity");
            return sanitizer.Sanitize(svg);
            
        }
    }
}
