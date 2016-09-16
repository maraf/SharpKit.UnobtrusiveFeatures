using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SharpKit.UnobtrusiveFeatures.Exports.Models
{
    /// <summary>
    /// Defines default export behavior for assembly.
    /// </summary>
    public class DefaultExport
    {
        /// <summary>
        /// Gets or sets default file name.
        /// </summary>
        public string Filename { get; set; }

        /// <summary>
        /// Gets or sets file name format used for all files in the export.
        /// </summary>
        public string FilenameFormat { get; set; }
    }
}
