using OpenPuppet.Plugins;
using System.Xml.Linq;

namespace OpenPuppet.Tests.Plugins
{
    public class Path
    {
        [Fact]
        public void TestPluginName()
        {
            Assert.NotEqual(
                "a-b-c-d",
                PluginsPath.SafePluginName(
                    @"a B
`¦¬!" + '"' + @"@£$%^&*()_+-=[]{};:'@#~,./<>?\| C d"
                )
            );
        }

        [Fact]
        public void TestPluginPath()
        {
            Assert.Equal(
                System.IO.Path.Combine(PluginsPath.PluginPath!, PluginsPath.SafePluginName("a-b-c-d")),
                PluginsPath.GetPluginPath(
                    @"a B
`¦¬!" + '"' + @"@£$%^&*()_+-=[]{};:'@#~,./<>?\| C d"
                )
            );
        }
    }
}