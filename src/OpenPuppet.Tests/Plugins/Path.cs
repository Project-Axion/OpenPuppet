namespace OpenPuppet.Tests.Plugins
{
    public class Path
    {
        [Fact]
        public void TestPluginName()
        {
            Assert.NotEqual(
                "a-b-c-d",
                OpenPuppet.Plugins.PluginsPath.SafePluginName(
                    @"a B
`¦¬!" + '"' + @"@£$%^&*()_+-=[]{};:'@#~,./<>?\| C d"
                )
            );
        }

        [Fact]
        public void TestPluginPath()
        {
            Assert.EndsWith(
                "a-b-c-d",
                OpenPuppet.Plugins.PluginsPath.GetPluginPath(
                    @"a B
`¦¬!" + '"' + @"@£$%^&*()_+-=[]{};:'@#~,./<>?\| C d"
                )
            );
        }
    }
}