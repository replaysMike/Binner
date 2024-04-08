using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Binner.Model.Integrations.Tme
{
    public enum DocumentTypes
    {
        /// <summary>
        /// Manual
        /// </summary>
        INS,
        /// <summary>
        /// Documentation
        /// </summary>
        DTE,
        /// <summary>
        /// Safety Data Sheet
        /// </summary>
        KCH,
        /// <summary>
        /// Warranty
        /// </summary>
        GWA,
        /// <summary>
        /// Safety Instruction
        /// </summary>
        INB,
        /// <summary>
        /// Video
        /// </summary>
        MOV,
        /// <summary>
        /// YouTube video
        /// YouTube videos are text files that contain the YouTube video id
        /// </summary>
        YTB,
        /// <summary>
        /// Presentation
        /// </summary>
        PRE,
        /// <summary>
        /// Software
        /// </summary>
        SFT
    }
}
