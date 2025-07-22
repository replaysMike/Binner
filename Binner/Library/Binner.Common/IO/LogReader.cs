using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Binner.Common.IO
{
    /// <summary>
    /// Reads a log file line by line backwards
    /// </summary>
    public sealed class LogReader : IEnumerable<string>
    {
        private const int DefaultBufferSize = 4096;
        private readonly Stream _stream;
        private readonly Encoding _encoding;
        private readonly int _bufferSize;
        private Func<long, byte, bool> characterStartDetector;

        /// <summary>
        /// Reads a log file line by line backwards
        /// </summary>
        /// <param name="streamSource">Data source</param>
        public LogReader(Stream streamSource)
            : this(streamSource, Encoding.UTF8)
        {
        }

        /// <summary>
        /// Reads a log file line by line backwards
        /// </summary>
        /// <param name="filename">File to read from</param>
        public LogReader(string filename)
            : this(filename, Encoding.UTF8)
        {
        }

        /// <summary>
        /// Reads a log file line by line backwards
        /// </summary>
        /// <param name="filename">File to read from</param>
        /// <param name="encoding">Encoding to use to decode the file into text</param>
        public LogReader(string filename, Encoding encoding)
            : this(File.OpenRead(filename), encoding)
        {
        }

        /// <summary>
        /// Creates a LogReader from a stream source.
        /// </summary>
        /// <param name="stream">Data source</param>
        /// <param name="encoding">Encoding to use to decode the stream into text</param>
        public LogReader(Stream stream, Encoding encoding)
            : this(stream, encoding, DefaultBufferSize)
        {
        }

        internal LogReader(Stream stream, Encoding encoding, int bufferSize)
        {
            _stream = stream;
            _encoding = encoding;
            _bufferSize = bufferSize;
            if (encoding.IsSingleByte)
            {
                // For a single byte encoding, every byte is the start (and end) of a character
                characterStartDetector = (pos, data) => true;
            }
            else if (encoding is UnicodeEncoding)
            {
                // For UTF-16, even-numbered positions are the start of a character.
                // TODO: This assumes no surrogate pairs. More work required
                // to handle that.
                characterStartDetector = (pos, data) => (pos & 1) == 0;
            }
            else if (encoding is UTF8Encoding)
            {
                // For UTF-8, bytes with the top bit clear or the second bit set are the start of a character
                // See http://www.cl.cam.ac.uk/~mgk25/unicode.html
                characterStartDetector = (pos, data) => (data & 0x80) == 0 || (data & 0x40) != 0;
            }
            else
            {
                throw new ArgumentException("Only single byte, UTF-8 and Unicode encodings are permitted");
            }
        }

        /// <summary>
        /// Returns the enumerator reading strings backwards. If this method discovers that
        /// the returned stream is either unreadable or unseekable, a NotSupportedException is thrown.
        /// </summary>
        public IEnumerator<string> GetEnumerator()
        {
            var stream = _stream;
            if (!stream.CanSeek)
            {
                stream.Dispose();
                throw new NotSupportedException("Unable to seek within stream");
            }
            if (!stream.CanRead)
            {
                stream.Dispose();
                throw new NotSupportedException("Unable to read within stream");
            }
            return GetEnumeratorImpl(stream);
        }

        private IEnumerator<string> GetEnumeratorImpl(Stream stream)
        {
            try
            {
                var position = stream.Length;

                if (_encoding is UnicodeEncoding && (position & 1) != 0)
                {
                    throw new InvalidDataException("UTF-16 encoding provided, but stream has odd length.");
                }

                // Allow up to two bytes for data from the start of the previous
                // read which didn't quite make it as full characters
                var buffer = new byte[_bufferSize + 2];
                var charBuffer = new char[_encoding.GetMaxCharCount(buffer.Length)];
                var leftOverData = 0;
                string? previousEnd = null;
                var firstYield = true;
                var swallowCarriageReturn = false;

                while (position > 0)
                {
                    var bytesToRead = Math.Min(position > int.MaxValue ? _bufferSize : (int)position, _bufferSize);

                    position -= bytesToRead;
                    stream.Position = position;
                    StreamUtil.ReadExactly(stream, buffer, bytesToRead);
                    if (leftOverData > 0 && bytesToRead != _bufferSize)
                    {
                        Array.Copy(buffer, _bufferSize, buffer, bytesToRead, leftOverData);
                    }
                    bytesToRead += leftOverData;

                    var firstCharPosition = 0;
                    while (!characterStartDetector(position + firstCharPosition, buffer[firstCharPosition]))
                    {
                        firstCharPosition++;
                        if (firstCharPosition == 3 || firstCharPosition == bytesToRead)
                            throw new InvalidDataException("Invalid UTF-8 data");
                    }
                    leftOverData = firstCharPosition;

                    var charsRead = _encoding.GetChars(buffer, firstCharPosition, bytesToRead - firstCharPosition, charBuffer, 0);
                    var endExclusive = charsRead;

                    for (var i = charsRead - 1; i >= 0; i--)
                    {
                        var lookingAt = charBuffer[i];
                        if (swallowCarriageReturn)
                        {
                            swallowCarriageReturn = false;
                            if (lookingAt == '\r')
                            {
                                endExclusive--;
                                continue;
                            }
                        }
                        if (lookingAt != '\n' && lookingAt != '\r')
                            continue;
                        if (lookingAt == '\n')
                            swallowCarriageReturn = true;
                        var start = i + 1;
                        var  bufferContents = new string(charBuffer, start, endExclusive - start);
                        endExclusive = i;
                        var stringToYield = previousEnd == null ? bufferContents : bufferContents + previousEnd;
                        if (!firstYield || stringToYield.Length != 0)
                            yield return stringToYield;
                        firstYield = false;
                        previousEnd = null;
                    }

                    previousEnd = endExclusive == 0 ? null : (new string(charBuffer, 0, endExclusive) + previousEnd);

                    // If we didn't decode the start of the array, put it at the end for next time
                    if (leftOverData != 0)
                    {
                        Buffer.BlockCopy(buffer, 0, buffer, _bufferSize, leftOverData);
                    }
                }
                if (leftOverData != 0)
                {
                    // At the start of the final buffer, we had the end of another character.
                    throw new InvalidDataException("Invalid UTF-8 data at start of stream");
                }
                if (firstYield && string.IsNullOrEmpty(previousEnd))
                {
                    yield break;
                }
                yield return previousEnd ?? "";
            }
            finally
            {
                stream.Dispose();
            }
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }

    public static class StreamUtil
    {
        public static void ReadExactly(Stream input, byte[] buffer, int bytesToRead)
        {
            var index = 0;
            while (index < bytesToRead)
            {
                var  read = input.Read(buffer, index, bytesToRead - index);
                if (read == 0) throw new EndOfStreamException ($"End of stream reached with {bytesToRead - index} byte{(bytesToRead - index == 1 ? "s" : "")} left to read.");
                index += read;
            }
        }
    }
}

