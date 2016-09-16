using ICSharpCode.NRefactory.TypeSystem;
using Mirrored.SharpKit.JavaScript;
using SharpKit.UnobtrusiveFeatures.Exports.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;

namespace SharpKit.UnobtrusiveFeatures.Exports
{
    /// <summary>
    /// A loader of the XML based configuration.
    /// </summary>
    public class ExportService
    {
        private readonly Dictionary<string, AssemblyExport> folderCache = new Dictionary<string, AssemblyExport>();
        private readonly string[] additionalNameFileNames;

        public ExportService(string[] additionalNameFileNames)
        {
            this.additionalNameFileNames = additionalNameFileNames;
        }

        private AssemblyExport CreateInstance(IAssembly assembly)
        {
            List<string> filenames = GetConfigurationFilenames(assembly);

            Stack<string> applicableFiles = new Stack<string>();
            string directoryPath = Environment.CurrentDirectory;
            while (!String.IsNullOrEmpty(directoryPath))
            {
                IEnumerable<string> filePaths = filenames.Select(f => Path.Combine(directoryPath, f)).Where(File.Exists);
                foreach (string filePath in filePaths)
                    applicableFiles.Push(filePath);
                
                directoryPath = Path.GetDirectoryName(directoryPath);
            }

            AssemblyExport parent = null;
            while (applicableFiles.Count > 0)
            {
                string filePath = applicableFiles.Pop();
                AssemblyExport item;
                if (!folderCache.TryGetValue(filePath, out item))
                    item = LoadFile(filePath, parent);

                parent = item;
            }

            AssemblyExport result = new AssemblyExport(parent);
            foreach (string filePath in GetConfigurationFilenames(assembly).Where(File.Exists))
                LoadFileWithoutBuildUp(result, filePath);
            
            result.IsExportAssembly = true;
            result.Assembly = assembly;
            result.BuildUp();
            return result;
        }

        private AssemblyExport LoadFile(string filePath, AssemblyExport parent)
        {
            AssemblyExport result = new AssemblyExport(parent);
            LoadFileWithoutBuildUp(result, filePath);
            result.BuildUp();
            return result;
        }

        private void LoadFileWithoutBuildUp(AssemblyExport result, string filePath)
        {
            XmlDocument document = new XmlDocument();
            document.Load(filePath);

            foreach (XmlElement element in document.GetElementsByTagName("ExternalTypes"))
            {
                foreach (XmlElement typeElement in element.GetElementsByTagName("Type"))
                    result.AddExternalType(typeElement.GetAttribute("Target"));
            }

            foreach (XmlElement element in document.GetElementsByTagName("Export"))
                result.DefaultExport = LoadExport(element);

            foreach (XmlElement element in document.GetElementsByTagName("Namespace"))
                result.AddNamespace(LoadNamespace(element, result));

            foreach (XmlElement element in document.GetElementsByTagName("Type"))
                result.AddType(LoadType(element, result));

            foreach (XmlElement element in document.GetElementsByTagName("Merge"))
                result.AddMerge(LoadMerge(element));
        }

        /// <summary>
        /// Loads export registration for assembly.
        /// </summary>
        /// <param name="assembly">Assembly definition.</param>
        /// <returns>Export registration for assembly</returns>
        public AssemblyExport Load(IAssembly assembly)
        {
            if (!IsConfigurationFile(assembly))
            {
                return new AssemblyExport()
                {
                    IsExportAssembly = false,
                    Assembly = assembly
                };
            }

            AssemblyExport result = CreateInstance(assembly);
            return result;
        }

        #region Reading and preparing file

        /// <summary>
        /// Tests whether exists configuration file.
        /// </summary>
        /// <param name="assembly">Assembly definition.</param>
        /// <returns>True if file exists, false otherwise.</returns>
        public bool IsConfigurationFile(IAssembly assembly)
        {
            return GetConfigurationFilenames(assembly).Any(File.Exists);
        }

        /// <summary>
        /// Returns name of configuration file.
        /// </summary>
        /// <param name="assembly">Assembly definition.</param>
        /// <returns>Name of configuration file.</returns>
        public List<string> GetConfigurationFilenames(IAssembly assembly)
        {
            List<string> result = new List<string>();
            result.Add(assembly.AssemblyName + ".xml");
            result.AddRange(additionalNameFileNames);
            return result;
        }

        #endregion

        #region Loading from xml

        /// <summary>
        /// Loads default export settings.
        /// </summary>
        /// <param name="element">Xml element.</param>
        /// <returns>Default export settings.</returns>
        private DefaultExport LoadExport(XmlElement element)
        {
            return new DefaultExport
            {
                Filename = XmlUtil.GetAttributeString(element, "Filename"),
                FilenameFormat = XmlUtil.GetAttributeString(element, "FilenameFormat")
            };
        }

        /// <summary>
        /// Loads namespace export settings.
        /// </summary>
        /// <param name="element">Xml element.</param>
        /// <param name="registry">Current export registry.</param>
        /// <returns>Namespace export settings.</returns>
        private NamespaceExport LoadNamespace(XmlElement element, AssemblyExport registry)
        {
            NamespaceExport item = new NamespaceExport();
            LoadItemBase(element, item, registry);
            return item;
        }

        /// <summary>
        /// Loads type export settings.
        /// </summary>
        /// <param name="element">Xml element.</param>
        /// <param name="registry">Current export registry.</param>
        /// <returns>Type export settings.</returns>
        private TypeExport LoadType(XmlElement element, AssemblyExport registry)
        {
            TypeExport item = new TypeExport();
            item.Name = XmlUtil.GetAttributeString(element, "Name");
            item.OrderInFile = XmlUtil.GetAttributeInt(element, "OrderInFile");
            //TODO: Load methods...
            LoadItemBase(element, item, registry);
            return item;
        }

        /// <summary>
        /// Loads export settings for shared base class.
        /// </summary>
        /// <param name="element">Xml element.</param>
        /// <param name="item">Currently loading item.</param>
        /// <param name="registry">Current export registry.</param>
        private void LoadItemBase(XmlElement element, ExportBase item, AssemblyExport registry)
        {
            item.Target = XmlUtil.GetAttributeString(element, "Target") ?? String.Empty;
            item.AutomaticPropertiesAsFields = XmlUtil.GetAttributeBool(element, "AutomaticPropertiesAsFields") ?? registry.GetType(item.Target).AutomaticPropertiesAsFields;
            item.Export = XmlUtil.GetAttributeBool(element, "Export") ?? registry.GetType(item.Target).Export;
            item.Mode = XmlUtil.GetAttributeEnum<JsMode>(element, "Mode") ?? registry.GetType(item.Target).Mode;
            item.PropertiesAsFields = XmlUtil.GetAttributeBool(element, "PropertiesAsFields") ?? registry.GetType(item.Target).PropertiesAsFields;
            item.Filename = registry.ApplyFilenameFormat(XmlUtil.GetAttributeString(element, "Filename")) ?? registry.GetType(item.Target).Filename;
        }

        /// <summary>
        /// Loads merge file definition.
        /// </summary>
        /// <param name="element">Xml element.</param>
        /// <returns>Merge file definition.</returns>
        private MergeFile LoadMerge(XmlElement element)
        {
            MergeFile item = new MergeFile();
            item.FileName = XmlUtil.GetAttributeString(element, "Filename");
            item.Sources = (XmlUtil.GetAttributeString(element, "Sources") ?? "").Split(',');
            item.Minify = XmlUtil.GetAttributeBool(element, "Minify") ?? false;
            return item;
        }

        #endregion
    }
}
