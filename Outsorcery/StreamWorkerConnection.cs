/*
    License: http://www.apache.org/licenses/LICENSE-2.0
 */
namespace Outsorcery
{
    using System;
    using System.IO;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Outsourced worker connection implementation around a stream.
    /// </summary>
    public class StreamWorkerConnection : IWorkerConnection
    {
        /// <summary>The waiting for data delay</summary>
        private readonly TimeSpan _waitingForDataDelay = TimeSpan.FromSeconds(0.1);

        /// <summary>The stream</summary>
        private readonly Stream _stream;

        /// <summary>
        /// Initializes a new instance of the <see cref="StreamWorkerConnection"/> class.
        /// </summary>
        /// <param name="stream">The stream.</param>
        public StreamWorkerConnection(Stream stream)
        {
            Contract.IsNotNull(stream);

            _stream = stream;
        }

        /// <summary>
        /// Sends the object asynchronously.
        /// </summary>
        /// <param name="obj">The object.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>
        /// An awaitable task.
        /// </returns>
        /// <exception cref="System.InvalidOperationException">Invalid header size</exception>
        public async Task SendObjectAsync(object obj, CancellationToken cancellationToken)
        {
            if (obj == null)
            {
                await SendIntAsync(0, cancellationToken).ConfigureAwait(false);
            }
            else
            {
                var data = SerializationHelper.SerializeObject(obj);
                await SendIntAsync(data.Length, cancellationToken).ConfigureAwait(false);
                await _stream.WriteAsync(data, 0, data.Length, cancellationToken).ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Receives the object asynchronously.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>
        /// An awaitable task, result is the received object.
        /// </returns>
        public async Task<object> ReceiveObjectAsync(CancellationToken cancellationToken)
        {
            var dataLength = await ReceiveIntAsync(cancellationToken).ConfigureAwait(false);

            if (dataLength <= 0)
            {
                return null;
            }

            var data = await ReadToLengthAsync(dataLength, cancellationToken).ConfigureAwait(false);
            return SerializationHelper.DeserializeObject(data);
        }

        /// <summary>
        /// Sends the integer asynchronously.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>
        /// An awaitable task.
        /// </returns>
        public async Task SendIntAsync(int value, CancellationToken cancellationToken)
        {
            var intBytes = BitConverter.GetBytes(value);
            await _stream.WriteAsync(intBytes, 0, sizeof(int), cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Receives the integer asynchronously.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>
        /// An awaitable task, result is the received object.
        /// </returns>
        public async Task<int> ReceiveIntAsync(CancellationToken cancellationToken)
        {
            var intBytes = await ReadToLengthAsync(sizeof(int), cancellationToken).ConfigureAwait(false);
            return BitConverter.ToInt32(intBytes, 0);
        }
        
        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources.
        /// </summary>
        /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                _stream.Dispose();
            }
        }

        /// <summary>
        /// Asynchronously reads all bytes up to length.
        /// If less bytes than length are available, will wait for more bytes.
        /// </summary>
        /// <param name="length">The length.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>
        /// A byte array.
        /// </returns>
        private async Task<byte[]> ReadToLengthAsync(int length, CancellationToken cancellationToken)
        {
            var dataReceived = 0;
            var data = new byte[length];

            while (dataReceived < length)
            {
                dataReceived += await _stream.ReadAsync(
                                                data,
                                                dataReceived,
                                                length - dataReceived,
                                                cancellationToken)
                                            .ConfigureAwait(false);

                if (dataReceived == 0)
                {
                    // No data pending yet, delay a little to avoid thrashing
                    await Task.Delay(_waitingForDataDelay, cancellationToken).ConfigureAwait(false);
                }
            }

            return data;
        }
    }
}
