using Mirrored.SharpKit.JavaScript;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharpKit.UnobtrusiveFeatures.Exports.Models
{
    /// <summary>
    /// Defines collection of <see cref="TypeExport"/> type name.
    /// </summary>
    public class TypeExportCollection
    {
        private readonly Dictionary<string, TypeExport> storage = new Dictionary<string, TypeExport>();

        /// <summary>
        /// Adds export for <paramref name="typeName"/> to defined as <paramref name="model"/>.
        /// </summary>
        /// <param name="typeName"></param>
        /// <param name="model"></param>
        public void Add(string typeName, TypeExport model)
        {
            if (typeName == null)
                throw new ArgumentNullException("typeName");

            if (model == null)
                throw new ArgumentNullException("model");

            storage[typeName] = model;
        }

        public void AddDefault(TypeExport model)
        {
            if (model == null)
                throw new ArgumentNullException("model");

            storage[String.Empty] = model;
        }

        public TypeExport Get(string typeName)
        {
            if (typeName == null)
                throw new ArgumentNullException("typeName");

            TypeExport item;
            if (storage.TryGetValue(typeName, out item))
                return item;

            if (storage.TryGetValue(String.Empty, out item))
                return item;

            return null;
        }
    }
}
