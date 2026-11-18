# Plugin system overview

This section of the documentation outlines how the plugin system works,
how plugin loading, unloading, and management is handled.

## Everything is a service

When a plugin is initialised, it gets passed a `IServicesCollection` which
is from the `Microsoft.Extensions.DependencyInjection` NuGet package.

**Every** service that the plugin registers must be registered through the
provided `IServicesCollection`, otherwise unloading doesn't work correctly.
When a plugin is unloaded/destroyed, all of the services it has registered
must be loaded.