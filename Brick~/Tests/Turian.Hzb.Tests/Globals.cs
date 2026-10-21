global using System.Numerics;
global using Gaya.Packages;
global using Microsoft.Extensions.Logging.Abstractions;
global using Silk.NET.Vulkan;
global using Turian.Editor.Core;
global using Turian.Engine.Core;
global using Xunit;

// Test classes run in parallel; those sharing process-wide state opt into the serial SerialTests collection.
[assembly: Xunit.v3.Parallelization(Mode = Xunit.Sdk.ParallelMode.Collections)]
