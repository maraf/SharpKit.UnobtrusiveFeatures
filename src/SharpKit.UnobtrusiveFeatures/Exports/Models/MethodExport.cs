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
    /// Defines export behavior for methods.
    /// Properties match those on the <see cref="JsMethodAttribute"/>.
    /// </summary>
    public class MethodExport
    {
        public string Code { get; set; }
        [DefaultValue(true)]
        public bool Export { get; set; }
        public bool? ExtensionImplementedInInstance { get; set; }
        public bool? Global { get; set; }
        public bool? InstanceImplementedAsExtension { get; set; }
        public string Name { get; set; }
        public string TargetMethod { get; set; }
    }
}
