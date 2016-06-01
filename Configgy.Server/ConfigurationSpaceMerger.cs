using System;
using System.Collections.Generic;
using System.Linq;

namespace Configgy.Server
{
    internal class ConfigurationSpaceMerger
    {
        public IDictionary<string, object> CreateMergedConfigurationSpace(IDictionary<string, object> configurationSpace)
        {
            var nodes = CreateNodes(configurationSpace);
            return CreateMergedSpace(nodes);
        }

        internal IDictionary<string, ConfigurationSetNode> CreateNodes(IDictionary<string, object> configurationSpace)
        {
            if (configurationSpace == null) throw new ArgumentNullException("configurationSets");

            var nodes = new Dictionary<string, ConfigurationSetNode>();

            foreach (var kvp in configurationSpace)
            {
                var node = new ConfigurationSetNode(
                    kvp.Key,
                    kvp.Value as IDictionary<string, object>
                );

                nodes.Add(node.Id, node);
            }

            foreach (var node in nodes.Values.Where(n => n.ParentId != null))
            {
                ConfigurationSetNode parent;

                if (!nodes.TryGetValue(node.ParentId, out parent))
                {
                    throw new ConfigurationSpaceMergerException("Error merging configuration set [" + node.Id + "]: Invalid parent ID [" + node.ParentId + "]");
                }

                nodes[node.ParentId].Children.Add(node);
            }

            return nodes;
        }

        internal IDictionary<string, object> CreateMergedSpace(IDictionary<string, ConfigurationSetNode> nodes)
        {
            if (nodes == null) throw new ArgumentNullException("nodes");

            var mergedMetaset = new Dictionary<string, object>();

            foreach (var node in nodes.Values.Where(n => n.ParentId == null))
            {
                FillMergedSet(node, null, mergedMetaset);
            }

            return mergedMetaset;
        }

        private void FillMergedSet(ConfigurationSetNode node, IDictionary<string, object> parentSet, IDictionary<string, object> mergedSpace)
        {
            var resultingSet = new Dictionary<string, object>();

            if (parentSet != null)
            {
                foreach (var entry in parentSet)
                {
                    resultingSet[entry.Key] = entry.Value;
                }
            }

            foreach (var entry in node.ConfigurationSet)
            {
                resultingSet[entry.Key] = entry.Value;
            }

            mergedSpace.Add(node.Id, resultingSet);

            foreach (var child in node.Children)
            {
                FillMergedSet(child, resultingSet, mergedSpace);
            }
        }

        internal class ConfigurationSetNode
        {
            public string Id { get; set; }
            public string ParentId { get; set; }
            public IDictionary<string, object> ConfigurationSet { get; set; }
            public List<ConfigurationSetNode> Children { get; set; }

            public ConfigurationSetNode(string id, IDictionary<string, object> value)
            {
                Id =  GetCustomId(value) ?? id;
                ConfigurationSet = value;
                ParentId = GetParentId(value);
                Children = new List<ConfigurationSetNode>();
            }

            private static string GetCustomId(IDictionary<string, object> configurationSet)
            {
                object customId = null;

                if (!configurationSet.TryGetValue(CreateSpecialKey("id"), out customId)) return null;

                if (!(customId is string))
                    throw new ConfigurationSpaceMergerException(string.Format("The value for the \"{0}\" key must be a string.", CreateSpecialKey("id")));

                return customId as string;
            }

            private static string GetParentId(IDictionary<string, object> configurationSet)
            {
                object parentId = null;

                if (!configurationSet.TryGetValue(CreateSpecialKey("parent"), out parentId)) return null;

                if (!(parentId is string))
                    throw new ConfigurationSpaceMergerException(string.Format("The value for the \"{0}\" key must be a string.", CreateSpecialKey("parent")));

                return parentId as string;
            }

            private static string CreateSpecialKey(string name)
            {
                return "!_" + name + "_!";
            }
        }
    }


    [Serializable]
    public class ConfigurationSpaceMergerException : Exception
    {
        public ConfigurationSpaceMergerException() { }
        public ConfigurationSpaceMergerException(string message) : base(message) { }
        public ConfigurationSpaceMergerException(string message, Exception inner) : base(message, inner) { }
        protected ConfigurationSpaceMergerException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context)
            : base(info, context) { }
    }
}