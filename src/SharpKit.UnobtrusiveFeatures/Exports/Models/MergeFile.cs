using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharpKit.UnobtrusiveFeatures.Exports.Models
{
    /// <summary>
    /// Defines configuration for merging files.
    /// </summary>
    public class MergeFile
    {
        /// <summary>
        /// Gets or sets target file path.
        /// </summary>
        public string FileName { get; set; }

        /// <summary>
        /// Gets or sets list of source file paths.
        /// </summary>
        public string[] Sources { get; set; }

        /// <summary>
        /// Gets or sets whether to minify <see cref="FileName"/>.
        /// </summary>
        public bool Minify { get; set; }
    }
}
