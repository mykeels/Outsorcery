/*
    License: http://www.apache.org/licenses/LICENSE-2.0
 */
namespace Outsorcery
{
    using System;
    using System.Diagnostics;
    using System.Threading.Tasks;

    /// <summary>
    /// A quick-fix Workload Benchmark that uses CPU percentage to determine how busy the system is.
    /// </summary>
    public class BasicCpuWorkloadBenchmark : IWorkloadBenchmark
    {
        /// <summary>The CPU counter</summary>
        private readonly PerformanceCounter _cpuCounter = new PerformanceCounter("Processor", "% Processor Time", "_Total");

        /// <summary>
        /// Gets the benchmark score asynchronously.
        /// </summary>
        /// <param name="workCategoryId">The work category identifier.</param>
        /// <returns>
        /// The benchmark score.
        /// </returns>
        public Task<int> GetScoreAsync(int workCategoryId)
        {
            return Task.FromResult((int)Math.Ceiling(Math.Pow(_cpuCounter.NextValue(), 3)));
        }
    }
}
