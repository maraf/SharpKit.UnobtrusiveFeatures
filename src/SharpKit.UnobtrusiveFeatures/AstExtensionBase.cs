using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharpKit.UnobtrusiveFeatures
{
    public abstract class AstExtensionBase : ExtensionBase
    {
        /// <summary>
        /// Js clr helper.
        /// </summary>
        protected JsClrHelper Helper { get; private set; }

        public AstExtensionBase(string extensionName, bool debug)
            : base(extensionName, debug)
        {
            Helper = new JsClrHelper();
        }

    }
}
