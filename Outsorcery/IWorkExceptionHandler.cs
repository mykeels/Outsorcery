/*
    License: http://www.apache.org/licenses/LICENSE-2.0
 */
namespace Outsorcery
{
    /// <summary>
    /// Worker Exception Handler interface
    /// </summary>
    public interface IWorkExceptionHandler
    {
        /// <summary>
        /// Handles the work exception.
        /// </summary>
        /// <param name="exception">The exception.</param>
        /// <returns>A value indicating whether to suppress the exception.</returns>
        HandleWorkExceptionResult HandleWorkException(WorkException exception);
    }
}
