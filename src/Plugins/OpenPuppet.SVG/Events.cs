using OpenPuppet.SDK;
using OpenPuppet.SDK.Events;
using ProjectEvents = OpenPuppet.SDK.Projects.Events.ProjectEvents;

namespace OpenPuppet.SVG
{
    public static class Events
    {
        public static void Subscribe()
        {
            IEvent<ProjectEvents.ProjectLoaded>.Subscribe("project.loaded", ProjectLoaded);
            IEvent<ProjectEvents.ProjectUnloaded>.Subscribe("project.loaded", ProjectUnloaded);
        }

        public static void ProjectLoaded(object? sender, ProjectEvents.ProjectLoaded e)
        {
            ContextMenu.SetEnabled("File.Import.Svg", true);
        }

        public static void ProjectUnloaded(object? sender, ProjectEvents.ProjectUnloaded e)
        {
            ContextMenu.SetEnabled("File.Import.Svg", false);
        }
    }
}
