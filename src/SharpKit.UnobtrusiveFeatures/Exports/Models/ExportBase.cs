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
    /// Defines shared properties for exporting types (either for whole namespace or concrete type).
    /// Properties match those on <see cref="JsTypeAttribute"/>.
    /// </summary>
    public class ExportBase
    {
        public bool? AutomaticPropertiesAsFields { get; set; }
        [DefaultValue(true)]
        public bool Export { get; set; }
        public JsMode Mode { get; set; }
        public bool? PropertiesAsFields { get; set; }
        public string Filename { get; set; }
        public string Target { get; set; }
    }
}
