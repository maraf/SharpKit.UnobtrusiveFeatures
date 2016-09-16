using Mirrored.SharpKit.JavaScript;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharpKit.UnobtrusiveFeatures.Exports.Models
{
    /// <summary>
    /// Defines export for concreate type.
    /// </summary>
    public class TypeExport : ExportBase
    {
        public string Name { get; set; }
        public int? OrderInFile { get; set; }

        public IEnumerable<MethodExport> Methods { get; set; }
    }
}
