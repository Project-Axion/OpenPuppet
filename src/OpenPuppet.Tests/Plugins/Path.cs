using OpenPuppet.SDK;

namespace OpenPuppet.Tests.Plugins
{
    public class Path
    {
        [Fact]
        public void TestIPluginPluginPath()
        {
            Assert.NotNull(IPlugin.PluginPath);
        }

        [Fact]
        public void TestPluginName()
        {
            Assert.NotEqual(
                "a-b-c-d",
                IPlugin.SafePluginName(
                    @"a B
`¦¬!" + '"' + @"@£$%^&*()_+-=[]{};:'@#~,/<>?\| C d"
                )
            );
        }

        [Fact]
        public void TestPluginPath()
        {
            Assert.Equal(
                System.IO.Path.Combine(IPlugin.PluginPath!, IPlugin.SafePluginName("a-b-c-d")),
                IPlugin.GetPluginPath(
                    @"a B
`¦¬!" + '"' + @"@£$%^&*()_+-=[]{};:'@#~,/<>?\| C d"
                )
            );
        }

        [Fact]
        public void TestInternetSourceToLocalSourceConversion()
        {
            SDK.Plugin.InternetInstallSource internet = new(
                "https://example.com/plugin.zip",
                "plugin.zip"
            );

            SDK.Plugin.LocalInstallSource local = new(internet);

            Assert.Equal(internet.Path, local.Path);
        }
    }
}