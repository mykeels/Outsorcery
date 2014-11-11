/*
    License: http://www.apache.org/licenses/LICENSE-2.0
 */
namespace Outsorcery
{
    using System.Threading.Tasks;

    /// <summary>
    /// Interface for workload benchmark
    /// </summary>
    public interface IWorkloadBenchmark
    {
        /// <summary>
        /// Gets the benchmark score asynchronously.
        /// </summary>
        /// <param name="workCategoryId">The work category identifier.</param>
        /// <returns>
        /// The benchmark score.
        /// </returns>
        Task<int> GetScoreAsync(int workCategoryId);
    }
}