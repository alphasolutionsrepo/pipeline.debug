using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PipelineDebug.Model
{
    public class TaxonomyHierarchy
    {
        public TaxonomyHierarchy(string name, List<string> taxonomies)
        {
            Name = name;
            if (taxonomies.Contains(null))
            {
                Selected = true;
                taxonomies.RemoveAll(t => t == null);
            }
            var prefixes = taxonomies.Select(SplitTaxonomy).GroupBy(t => t.Item1).ToList();
            Children = prefixes.Select(pref => new TaxonomyHierarchy(pref.Key, pref.Select(t => t.Item2).ToList())).ToList();
        }

        public string Name { get; }
        public bool Selected { get; }
        public List<TaxonomyHierarchy> Children { get; set; }
        
        private Tuple<string, string> SplitTaxonomy(string taxonomy)
        {
            if (taxonomy.StartsWith(Constants.ContextName))
            {
                return new Tuple<string, string>(Constants.ContextName, taxonomy.Substring(Constants.ContextName.Length + 1));
            }

            var index = taxonomy.IndexOf('.');
            if (index < 0)
                return new Tuple<string, string>(taxonomy, null);

            return new Tuple<string, string>(taxonomy.Substring(0, index), taxonomy.Substring(index + 1));
        }
    }
}
