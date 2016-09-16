﻿using ICSharpCode.NRefactory.TypeSystem;
using Mirrored.SharpKit.JavaScript;
using Neptuo.Text.Tokens;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharpKit.UnobtrusiveFeatures.Exports.Models
{
    /// <summary>
    /// Defines export behavior for particular assembly.
    /// If <see cref="Assembly"/> is <c>null</c>, it represents configuration used in the parent directories, shared for multiple projects.
    /// </summary>
    public class AssemblyExport
    {
        private readonly AssemblyExport parent;

        private Dictionary<string, TypeExport> typeItems;
        private List<NamespaceExport> namespaceItems;
        private List<MergeFile> mergeFiles;
        private HashSet<string> externalTypes;

        /// <summary>
        /// Bound assembly definition.
        /// </summary>
        public IAssembly Assembly { get; set; }

        /// <summary>
        /// Identifiers whether anything is exporting from this assembly.
        /// </summary>
        public bool IsExportAssembly { get; set; }

        /// <summary>
        /// Default export settings.
        /// </summary>
        public DefaultExport DefaultExport { get; set; }

        public AssemblyExport()
        { }

        public AssemblyExport(AssemblyExport parent)
        {
            this.parent = parent;
        }

        /// <summary>
        /// Adds registration for namespace.
        /// </summary>
        /// <param name="item">Namespace registration item.</param>
        public void AddNamespace(NamespaceExport item)
        {
            if (item.Target == null)
                throw new ArgumentNullException("item.Target");

            if (String.IsNullOrEmpty(item.Target))
                throw new ArgumentOutOfRangeException("item.Target", "Namespace name can't be empty.");

            if (item.Target.EndsWith("."))
                throw new ArgumentOutOfRangeException("item.Target", "Namespace name can't ends with '.' (letter, number or * is expected).");

            if (item.Target.EndsWith("*"))
                item.Target = item.Target.Substring(0, item.Target.Length - 1);

            if (namespaceItems == null)
                namespaceItems = new List<NamespaceExport>();

            namespaceItems.Add(item);
        }

        /// <summary>
        /// Adds registration for type.
        /// </summary>
        /// <param name="item">Type registration item.</param>
        public void AddType(TypeExport item)
        {
            if (typeItems == null)
                typeItems = new Dictionary<string, TypeExport>();

            if (item.Target == null)
                item.Target = String.Empty;

            typeItems[item.Target] = item;
        }

        /// <summary>
        /// Adds definition for merging files.
        /// </summary>
        /// <param name="item">Merge file definition.</param>
        public void AddMerge(MergeFile item)
        {
            if (mergeFiles == null)
                mergeFiles = new List<MergeFile>();

            mergeFiles.Add(item);
        }

        /// <summary>
        /// Adds external type that should be included in the compilation.
        /// </summary>
        /// <param name="externalType">The external type fullname.</param>
        public void AddExternalType(string externalType)
        {
            if (externalTypes == null)
                externalTypes = new HashSet<string>();

            externalTypes.Add(externalType);
        }

        /// <summary>
        /// Builds registration structures after items are added.
        /// </summary>
        public void BuildUp()
        {
            if (namespaceItems != null)
                namespaceItems = new List<NamespaceExport>(namespaceItems.OrderByDescending(item => item.Target.Length));
        }

        /// <summary>
        /// Returns registration for <paramref name="targetType"/>.
        /// </summary>
        /// <param name="targetType">Type definition.</param>
        /// <returns>Registration for <paramref name="targetType"/>.</returns>
        public TypeExport GetType(ITypeDefinition targetType)
        {
            return GetType(targetType.FullTypeName.ReflectionName);
        }

        /// <summary>
        /// Tries to find type registration from type registrations (including <see cref="parent"/>).
        /// </summary>
        /// <param name="targetTypeName">The target type full name.</param>
        /// <returns>Registration for <paramref name="targetTypeName"/> or <c>null</c>.</returns>
        private TypeExport FindTypeFromTypeRegistry(string targetTypeName)
        {
            if (typeItems != null)
            {
                TypeExport item;
                if (typeItems.TryGetValue(targetTypeName, out item))
                    return item;
            }

            if (parent != null)
                return parent.FindTypeFromTypeRegistry(targetTypeName);

            return null;
        }

        /// <summary>
        /// Tries to find type registration from namespace registrations (including <see cref="parent"/>).
        /// </summary>
        /// <param name="targetTypeName">The target type full name.</param>
        /// <returns>Registration for <paramref name="targetTypeName"/> or <c>null</c>.</returns>
        private TypeExport FindTypeFromNamespaceRegistry(string targetTypeName)
        {
            if (namespaceItems != null)
            {
                foreach (NamespaceExport item in namespaceItems)
                {
                    if (IsInNamespace(targetTypeName, item.Target))
                    {
                        TypeExport typeItem = new TypeExport
                        {
                            AutomaticPropertiesAsFields = item.AutomaticPropertiesAsFields,
                            Export = item.Export,
                            Filename = item.Filename,
                            Mode = item.Mode,
                            PropertiesAsFields = item.PropertiesAsFields
                        };

                        if (typeItems == null)
                            typeItems = new Dictionary<string, TypeExport>();

                        typeItems[targetTypeName] = typeItem;
                        return typeItem;
                    }
                }
            }

            if (parent != null)
                return parent.FindTypeFromNamespaceRegistry(targetTypeName);

            return null;
        }

        /// <summary>
        /// Returns registration for <paramref name="targetTypeName"/>.
        /// </summary>
        /// <param name="targetTypeName">Type name.</param>
        /// <returns>Registration for <paramref name="targetTypeName"/>.</returns>
        public TypeExport GetType(string targetTypeName)
        {
            if (targetTypeName == null)
                targetTypeName = String.Empty;

            TypeExport result = FindTypeFromTypeRegistry(targetTypeName);
            if (result != null)
                return result;

            result = FindTypeFromNamespaceRegistry(targetTypeName);
            if (result != null)
                return result;

            // Applied also when only DefaultExport is used.
            if (IsExportAssembly)
            {
                return new TypeExport
                {
                    Export = true,
                    Filename = GetFilenameWithDefaultExport(null),
                    Mode = JsMode.Clr,
                };
            }

            // Return empty item.
            return new TypeExport();
        }

        /// <summary>
        /// Determins whether <paramref name="targetTypeName"/> is in <paramref name="namespaceName"/>.
        /// If <paramref name="namespaceName"/> ends with dot ('.'), <paramref name="targetTypeName"/> can be in that namespace or any subnamespace.
        /// </summary>
        /// <param name="targetTypeName"></param>
        /// <param name="namespaceName"></param>
        /// <returns></returns>
        private bool IsInNamespace(string targetTypeName, string namespaceName)
        {
            if (targetTypeName == null)
                throw new ArgumentNullException("targetTypeName");

            if (namespaceName == null)
                throw new ArgumentNullException("namespaceName");

            if (namespaceName.EndsWith(".") && targetTypeName.StartsWith(namespaceName.Substring(0, namespaceName.Length - 1)))
                return true;

            int lastIndexOfDot = targetTypeName.LastIndexOf('.');
            if (lastIndexOfDot == -1)
                return namespaceName == String.Empty;

            string typeNamespaceName = targetTypeName.Substring(0, lastIndexOfDot);
            return typeNamespaceName == namespaceName;
        }

        /// <summary>
        /// Returns filename with DefaultExport applied to it.
        /// </summary>
        /// <param name="filename">File name.</param>
        /// <returns>Filename with DefaultExport applied to it.</returns>
        private string GetFilenameWithDefaultExport(string filename)
        {
            if (String.IsNullOrEmpty(filename))
            {
                filename = FindDefaultFileName();
                filename = TryApplyTokens(filename, false);
            }

            return ApplyFilenameFormat(filename);
        }

        private string FindDefaultFileName()
        {
            if (DefaultExport == null)
            {
                if (parent == null)
                    return null;

                return parent.FindDefaultFileName();
            }

            return DefaultExport.Filename;
        }

        /// <summary>
        /// Applies file name format from DefaultExport if used.
        /// </summary>
        /// <param name="filename">File name.</param>
        /// <returns>File name format from DefaultExport if used.</returns>
        public string ApplyFilenameFormat(string filename)
        {
            if (filename == null)
                return null;

            string format = FindDefaultFileNameFormat();
            if (format == null)
                return filename;

            format = TryApplyTokens(format, true);
            return String.Format(format, filename);
        }

        private string FindDefaultFileNameFormat()
        {
            string format = null;

            if (DefaultExport != null && !String.IsNullOrEmpty(DefaultExport.FilenameFormat))
                format = DefaultExport.FilenameFormat;
            else if (parent != null)
                format = parent.FindDefaultFileNameFormat();

            if (String.IsNullOrEmpty(format))
                return null;

            return format;
        }

        private Dictionary<string, TokenWriter> formatWriterCache = new Dictionary<string, TokenWriter>();

        private string TryApplyTokens(string template, bool isZeroParameterSupported)
        {
            if (Assembly == null)
                return template;

            TokenWriter writer;
            if (!formatWriterCache.TryGetValue(template, out writer))
                formatWriterCache[template] = writer = new TokenWriter(template);

            template = writer.Format(name =>
            {
                if (name == "0" && isZeroParameterSupported)
                    return "{0}";
                else if (name == "AssemblyName")
                    return Assembly.AssemblyName;

                return null;
            });

            return template;
        }

        /// <summary>
        /// Returns definitions for merging files.
        /// </summary>
        /// <returns>Definitions for merging files.</returns>
        public IEnumerable<MergeFile> GetMergeItems()
        {
            IEnumerable<MergeFile> result = Enumerable.Empty<MergeFile>();
            if (parent != null)
                result = Enumerable.Concat(result, parent.GetMergeItems());

            if (mergeFiles != null)
                result = Enumerable.Concat(result, mergeFiles);

            return result;
        }

        /// <summary>
        /// Returns enumeration of external types that should be included in the compilation.
        /// </summary>
        /// <returns></returns>
        public IEnumerable<string> GetExternalTypes()
        {
            IEnumerable<string> result = Enumerable.Empty<string>();
            if (parent != null)
                result = Enumerable.Concat(result, parent.GetExternalTypes());

            if (externalTypes != null)
                result = Enumerable.Concat(result, externalTypes);

            return result;
        }
    }
}
