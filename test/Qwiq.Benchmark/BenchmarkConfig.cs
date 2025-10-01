using BenchmarkDotNet.Columns;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Diagnosers;
using BenchmarkDotNet.Environments;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Validators;

namespace Qwiq.Benchmark
{
    public class BenchmarkConfig : ManualConfig
    {
        public BenchmarkConfig()
        {
            AddJob(Job.Default.WithRuntime(CoreRuntime.Core80).WithJit(Jit.RyuJit).WithPlatform(Platform.X64).WithGcMode(new GcMode { Server = true }));
            AddJob(Job.Default.WithRuntime(CoreRuntime.Core80).WithJit(Jit.RyuJit).WithPlatform(Platform.X86).WithGcMode(new GcMode { Server = true }));
            AddJob(Job.Default.WithRuntime(CoreRuntime.Core80).WithJit(Jit.RyuJit).WithPlatform(Platform.AnyCpu).WithGcMode(new GcMode { Server = true }));

            // GC and Memory Allocation
            AddDiagnoser(MemoryDiagnoser.Default);

            // Checks whether any of the referenced assemblies is non-optimized
            AddValidator(JitOptimizationsValidator.FailOnError);

            AddColumn(StatisticColumn.AllStatistics);
        }
    }
}