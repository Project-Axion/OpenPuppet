namespace OpenPuppet.Tests.Plugins
{
    public class Path
    {
        [Fact]
        public void TestPluginName()
        {
            Assert.NotEqual(
                "a-b-c-d",
                OpenPuppet.Plugins.Path.SafePluginName(
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
                OpenPuppet.Plugins.Path.GetPluginPath(
                    @"a B
`¦¬!" + '"' + @"@£$%^&*()_+-=[]{};:'@#~,./<>?\| C d"
                )
            );
        }
    }
}