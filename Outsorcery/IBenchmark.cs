/*
    License: http://www.apache.org/licenses/LICENSE-2.0
 */
namespace Outsorcery
{
    using System.Threading.Tasks;

    /// <summary>
    /// Benchmark interface
    /// </summary>
    public interface IBenchmark
    {
        /// <summary>
        /// Gets the benchmark score asynchronously.
        /// </summary>
        /// <returns>The benchmark score.</returns>
        Task<int> GetScoreAsync();
    }
}
