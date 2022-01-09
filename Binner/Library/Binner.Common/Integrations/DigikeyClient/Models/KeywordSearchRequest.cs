//-----------------------------------------------------------------------
//
// THE SOFTWARE IS PROVIDED "AS IS" WITHOUT ANY WARRANTIES OF ANY KIND, EXPRESS, IMPLIED, STATUTORY, 
// OR OTHERWISE. EXPECT TO THE EXTENT PROHIBITED BY APPLICABLE LAW, DIGI-KEY DISCLAIMS ALL WARRANTIES, 
// INCLUDING, WITHOUT LIMITATION, ANY IMPLIED WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE, 
// SATISFACTORY QUALITY, TITLE, NON-INFRINGEMENT, QUIET ENJOYMENT, 
// AND WARRANTIES ARISING OUT OF ANY COURSE OF DEALING OR USAGE OF TRADE. 
// 
// DIGI-KEY DOES NOT WARRANT THAT THE SOFTWARE WILL FUNCTION AS DESCRIBED, 
// WILL BE UNINTERRUPTED OR ERROR-FREE, OR FREE OF HARMFUL COMPONENTS.
// 
//-----------------------------------------------------------------------

namespace ApiClient.Models
{
    /// <summary>
    ///     Very simple version of Keyword Search request for WebApp and IntegrationExams
    /// </summary>
    public class KeywordSearchRequest
    {
        /// <summary>
        ///     Gets or sets the keywords.
        /// </summary>
        /// <value>
        ///     The keywords.
        /// </value>
        public string Keywords { get; set; }

        /// <summary>
        ///     Gets or sets the record count.
        /// </summary>
        /// <value>
        ///     The record count.
        /// </value>
        public int RecordCount { get; set; }
    }
}
