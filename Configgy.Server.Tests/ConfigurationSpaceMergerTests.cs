using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Configgy.Server.Tests
{
    public class ConfigurationSpaceMergerTests
    {
        public class ConfigurationSpaceMergerTestsContext
        {
            protected IDictionary<string, object> validConfigurationSpace;
            protected IDictionary<string, object> invalidConfigurationSpace;

            public ConfigurationSpaceMergerTestsContext()
            {
                validConfigurationSpace = new Dictionary<string, object>
                {
                    { "set1", new Dictionary<string, object> { { "A", 1 } } },
                    { "set2", new Dictionary<string, object> { { "!_id_!", "custom_id" }, { "!_parent_!", "set1" }, { "B", 2 }, { "C", 3 } } },
                    { "set3", new Dictionary<string, object> { { "!_parent_!", "custom_id" }, { "C", 4 } } }
                };

                invalidConfigurationSpace = new Dictionary<string, object>
                {
                    { "set1", new Dictionary<string, object> { { "A", 1 } } },
                    { "set2", new Dictionary<string, object> { { "!_parent_!", "non_existant" }, { "B", 2 }, { "C", 3 } } },
                    { "set3", new Dictionary<string, object> { { "!_parent_!", "set2" }, { "C", 4 } } }
                };
            }
        }


        public class CreateNodes : ConfigurationSpaceMergerTestsContext
        {
            ConfigurationSpaceMerger merger = new ConfigurationSpaceMerger();

            [Fact]
            public void WhenPassingNullConfigurationSpace_ShouldThrow()
            {
                Assert.Throws<ArgumentNullException>(() => merger.CreateNodes(null));
            }

            [Fact]
            public void WhenParsingConfigurationSpaceWithInvalidParentIdentifier_ShouldThrow()
            {
                Assert.Throws<ConfigurationSpaceMergerException>(() => merger.CreateNodes(invalidConfigurationSpace));
            }

            [Fact]
            public void WhenParsingValidConfigurationSpace()
            {
                var nodes = merger.CreateNodes(validConfigurationSpace);

                Assert.Equal(validConfigurationSpace.Count, nodes.Count); //Should have as many nodes as entries in the configuration set
                Assert.Contains("custom_id", nodes.Keys); //Should consider custom IDs
                Assert.Equal("set1", nodes["custom_id"].ParentId); //Should consider parent IDs
                Assert.Contains(nodes["custom_id"], nodes["set1"].Children); //Should add nodes as childrens to ther parents (as specified in the parent ID property)
                Assert.Equal(0, nodes["set3"].Children.Count); //Should not add children to nodes thar are not referenced as parents
            }
        }

        public class CreateMergedSpace : ConfigurationSpaceMergerTestsContext
        {
            ConfigurationSpaceMerger merger = new ConfigurationSpaceMerger();

            [Fact]
            public void WhenPassinNull_ShouldThrow()
            {
                Assert.Throws<ArgumentNullException>(() => merger.CreateMergedSpace(null));
            }

            [Fact]
            public void WhenParsingValidNodesCollection()
            {
                var nodes = merger.CreateNodes(validConfigurationSpace);

                var mergedConfigurationSpace = merger.CreateMergedSpace(nodes);

                Assert.Equal(nodes.Count, mergedConfigurationSpace.Count); //Should have as many configuration sets as nodes
                Assert.Equal(1, (mergedConfigurationSpace["set3"] as IDictionary<string, object>)["A"]); //Should add parent properties to sets
                Assert.Equal(4, (mergedConfigurationSpace["set3"] as IDictionary<string, object>)["C"]); //Should override parent properties as many times as needed

                //Should have all own properties AND parent's properties
                var set = mergedConfigurationSpace["custom_id"] as IDictionary<string, object>;
                Assert.Equal(1, set["A"]);
                Assert.Equal(2, set["B"]);
                Assert.Equal(3, set["C"]);                
            }
        }
    }
}
