/*
    License: http://www.apache.org/licenses/LICENSE-2.0
 */
namespace Outsorcery
{
    using System.Threading.Tasks;

    /// <summary>
    /// A workload benchmark based on current system performance.
    /// The higher then percentage CPU, memory and page file used, the higher
    /// the benchmark score for our current workload.
    /// </summary>
    public class SystemPerformanceWorkloadBenchmark : IBenchmark
    {
        /// <summary>
        /// Gets the benchmark score asynchronously.
        /// </summary>
        /// <returns>
        /// The benchmark score.
        /// </returns>
        public Task<int> GetScoreAsync()
        {
            // Todo: Implement performance counter based workload scoring
            return Task.FromResult(0);
        }
    }
}
