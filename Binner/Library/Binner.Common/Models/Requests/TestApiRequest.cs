using System.Collections;
using System.Collections.Generic;

namespace Binner.Common.Models.Requests
{
    public class TestApiRequest
    {
        /// <summary>
        /// The api name to test
        /// </summary>
        public string Name { get; set; } = null!;

        /// <summary>
        /// Api configuration
        /// </summary>
        public ICollection<ConfigValue> Configuration { get; set; } = new List<ConfigValue>();
    }

    public class ConfigValue
    {
        public string Key { get; set; } = null!;

        public string? Value { get; set; }
    }
}
