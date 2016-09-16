using ICSharpCode.NRefactory.CSharp.TypeSystem;
using ICSharpCode.NRefactory.TypeSystem;
using Mirrored.SharpKit.JavaScript;
using SharpKit.Compiler;
using SharpKit.UnobtrusiveFeatures.Exports.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;

namespace SharpKit.UnobtrusiveFeatures.Exports
{
    /// <summary>
    /// Export for exporting types based on the XML definition.
    /// </summary>
    public class ExportExtension : ExtensionBase
    {
        private ExportService service;

        public ExportExtension(string[] additionalNameFileNames, bool debug = false)
            : base("Export", debug)
        {
            service = new ExportService(additionalNameFileNames);
        }

        public void Process(IEnumerable<IAssembly> assemblies, ICustomAttributeProvider attributeProvider)
        {
            if (assemblies == null)
                throw new ArgumentNullException("assemblies");

            if (attributeProvider == null)
                throw new ArgumentNullException("attributeProvider");
            
            foreach (IAssembly assembly in assemblies.OfType<CSharpAssembly>())
            {
                AssemblyExport registry = CreateRegistry(assembly);
                if (registry.IsExportAssembly)
                    ProcessRegistry(assembly, attributeProvider, registry);
            }
        }

        private AssemblyExport CreateRegistry(IAssembly assembly)
        {
            return service.Load(assembly);
        }

        private void ProcessRegistry(IAssembly assembly, ICustomAttributeProvider attributeProvider, AssemblyExport registry)
        {
            foreach (string typeName in registry.GetExternalTypes())
                attributeProvider.AddCustomAttribute(assembly, new JsTypeAttribute { TargetTypeName = typeName, Export = true });

            foreach (ITypeDefinition type in assembly.GetAllTypeDefinitions())
            {
                LogDebug("Processing type '{0}'", type.FullName);
                TypeExport typeItem = registry.GetType(type);
                ProcessType(type, attributeProvider, typeItem);
            }

            foreach (MergeFile item in registry.GetMergeItems())
                attributeProvider.AddCustomAttribute(assembly, new JsMergedFileAttribute { Filename = item.FileName, Sources = item.Sources, Minify = item.Minify });
        }

        private void ProcessType(ITypeDefinition type, ICustomAttributeProvider attributeProvider, TypeExport typeItem)
        {
            JsTypeAttribute typeAttribute = attributeProvider.GetCustomAttributes<JsTypeAttribute>(type).FirstOrDefault();
            if (typeAttribute == null)
            {
                typeAttribute = new JsTypeAttribute();
                attributeProvider.AddCustomAttribute(type, typeAttribute);
            }

            typeAttribute.Filename = typeItem.Filename;
            typeAttribute.Export = typeItem.Export;
            typeAttribute.Mode = typeItem.Mode;

            if (typeItem.AutomaticPropertiesAsFields != null)
                typeAttribute.AutomaticPropertiesAsFields = typeItem.AutomaticPropertiesAsFields.Value;

            if (typeItem.Name != null)
                typeAttribute.Name = typeItem.Name;

            if (typeItem.PropertiesAsFields != null)
                typeAttribute.PropertiesAsFields = typeItem.PropertiesAsFields.Value;

            //TODO: Methods...
        }
    }
}
