using BenchmarkDotNet.Columns;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Environments;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Validators;

namespace Qwiq.Benchmark
{
    public class BenchmarkConfig : ManualConfig
    {
        public BenchmarkConfig()
        {
            AddJob(Job.Default.WithRuntime(BenchmarkDotNet.Jobs.CoreRuntime.Core80).WithPlatform(Platform.X64).WithGcServer(true));
            AddJob(Job.Default.WithRuntime(BenchmarkDotNet.Jobs.CoreRuntime.Core80).WithPlatform(Platform.X86).WithGcServer(true));
            AddJob(Job.Default.WithRuntime(BenchmarkDotNet.Jobs.CoreRuntime.Core80).WithPlatform(Platform.AnyCpu).WithGcServer(true));

            // GC and Memory Allocation
            AddDiagnoser(BenchmarkDotNet.Diagnosers.MemoryDiagnoser.Default);

            // Checks whether any of the referenced assemblies is non-optimized
            Add(JitOptimizationsValidator.FailOnError);

            Add(StatisticColumn.AllStatistics);


        }
    }
}
