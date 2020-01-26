using System.IO;
using System.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Json.Serialization
{
    /// <summary>
    /// Tool to parse a stream of text into one or more JsonObjects.
    /// </summary>
    public class JsonStreamParser
    {
        private Stream _stream;
        private byte[] _Buffer;
        private int _Cursor = 0;
        private int _BufferContentLength = 0;
        private readonly int _BufferSize;

        /// <summary>
        /// Number of bytes read (but not necessarily processed yet) from the stream.
        /// </summary>
        public int BytesRead { get; private set; } = 0;

        /// <summary>
        /// Number of bytes processed into JsonObjects.
        /// </summary>
        public int BytesProcessed { get; private set; } = 0;

        /// <summary>
        /// Characters that will be skipped when skipping whitespace.
        /// </summary>
        private const string WHITESPACE = " \t\r\n";

        /// <summary>
        /// Valid escape codes to follow an escape character ('\').
        /// </summary>
        private const string ESCAPES = "bfnrt\"\\";

        /// <summary>
        /// For each character in ESCAPES, the character which the escape code (e.g., "\n") should be replaced with.
        /// </summary>
        private const string REPLACEMENTS = "\b\f\n\r\t\"\\";

        /// <summary>
        /// Characters with which a string representation of a number may begin.
        /// </summary>
        private const string NUMBER_STARTS = "-0123456789.";

        /// <summary>
        /// Prepare to parse incoming data from the specified stream into one or more JsonObjects.
        /// </summary>
        /// <param name="stream">Underlying stream that will supply the data from which one or more JsonObjects are constructed.</param>
        /// <param name="bufferSize">Async read operations should request this much data at once.</param>
        public JsonStreamParser(Stream stream, int bufferSize = 4096)
        {
            _stream = stream;
            _Buffer = new byte[bufferSize + 1]; // Plus one to allow un-reading one character without having to expand the buffer
            _BufferSize = bufferSize;
        }

        /// <summary>
        /// Create and return a Task that will complete once a full JsonObject has been read from the underlying stream.
        /// </summary>
        /// <exception cref="FormatException">The data in the stream did not conform to the JSON format.</exception>
        /// <exception cref="EndOfStreamException">The end of the stream was reached before the current JsonObject under construction could be completed.</exception>
        public async Task<JsonObject> ReadObject()
        {
            char c = await ReadSkippingWhiteSpace();
            if (c == '{')
            {
                return new JsonObject(await ReadDictionary());
            }
            else if (c == '"')
            {
                return new JsonObject(await ReadString());
            }
            else if (c == 't')
            {
                await ReadSequence("rue");
                return new JsonObject(true);
            }
            else if (c == 'f')
            {
                await ReadSequence("alse");
                return new JsonObject(false);
            }
            else if (NUMBER_STARTS.Contains(c))
            {
                return new JsonObject(await ReadNumber(c));
            }
            else if (c == 'n')
            {
                await ReadSequence("ull");
                return JsonObject.Null;
            }
            else if (c == '[')
            {
                return new JsonObject(await ReadArray());
            }
            else
            {
                throw new FormatException("Invalid JSON object starting with '" + c + "'");
            }
        }

        private async Task<char> ReadNextChar()
        {
            if (_Cursor >= _BufferContentLength)
            {
                if (_Buffer.Length > _BufferSize)
                    _Buffer = new byte[_BufferSize];
                _BufferContentLength = await _stream.ReadAsync(_Buffer, 0, _Buffer.Length);
                if (_BufferContentLength == 0)
                {
                    throw new EndOfStreamException();
                }
                BytesRead += _BufferContentLength;
                _Cursor = 0;
            }

            BytesProcessed++;
            return (char)_Buffer[_Cursor++];
        }

        private void UnreadChar(char c)
        {
            if (_Cursor > 0)
            {
                if (_Buffer[_Cursor - 1] == c)
                    _Cursor--;
                else
                    throw new Exception("Logic error: could not unread character because the buffered character does not match");
            }
            else
            {
                if (_BufferContentLength < _Buffer.Length)
                {
                    Array.Copy(_Buffer, 0, _Buffer, 1, _BufferContentLength);
                    _Buffer[0] = (byte)c;
                }
                else
                {
                    var newBuffer = new byte[_Buffer.Length + 1];
                    Array.Copy(_Buffer, 0, newBuffer, 1, _Buffer.Length);
                    _Buffer = newBuffer;
                }
            }
            BytesProcessed--;
        }

        private async Task<char> ReadSkippingWhiteSpace()
        {
            while (true)
            {
                char c = await ReadNextChar();
                if (WHITESPACE.Contains(c))
                    continue;
                return c;
            }
        }

        private async Task<Dictionary<string, JsonObject>> ReadDictionary()
        {
            var result = new Dictionary<string, JsonObject>();

            char c = await ReadSkippingWhiteSpace();
            if (c == '}')
                return result;
            UnreadChar(c);

            while (true)
            {
                KeyValuePair<string, JsonObject> kvp = await ParseKeyValuePair();
                result[kvp.Key] = kvp.Value;
                c = await ReadSkippingWhiteSpace();
                if (c == '}')
                    return result;
                else if (c != ',')
                    throw new FormatException("Unexpected key-value-pair delimiter in JSON object: '" + c + "'");
            }
        }

        private async Task<KeyValuePair<string, JsonObject>> ParseKeyValuePair()
        {
            string key;
            char c = await ReadSkippingWhiteSpace();
            if (c == '"')
            {
                key = await ReadString();
            }
            else
            {
                throw new FormatException("Unsupported key format; expected quoted string, found instead '" + c + "'");
            }

            c = await ReadSkippingWhiteSpace();
            if (c != ':')
                throw new FormatException("Expected key-value pair separated by ':', found instead '" + c + "' separator");

            JsonObject value = await ReadObject();

            return new KeyValuePair<string, JsonObject>(key, value);
        }

        private async Task<string> ReadString()
        {
            var sb = new StringBuilder();
            bool escape = false;
            while (true)
            {
                char c = await ReadNextChar();
                if (escape)
                {
                    if (c == 'u')
                    {
                        sb.Append((char)int.Parse(await ReadCharacters(4), System.Globalization.NumberStyles.HexNumber));
                        escape = false;
                    }
                    else if (c == 'U')
                    {
                        throw new NotImplementedException("Basic .NET chars do not support 32-bit Unicode characters");
                    }
                    else
                    {
                        int i = ESCAPES.IndexOf(c);
                        if (i >= 0)
                        {
                            sb.Append(REPLACEMENTS[i]);
                            escape = false;
                        }
                        else
                        {
                            throw new FormatException("Invalid escape character '" + c + "'");
                        }
                    }
                }
                else if (c == '\\')
                {
                    escape = true;
                }
                else if (c == '"')
                {
                    return sb.ToString();
                }
                else
                {
                    sb.Append(c);
                }
            }
        }

        private async Task<string> ReadCharacters(int nCharacters)
        {
            var sb = new StringBuilder();
            for (int i = 0; i < nCharacters; i++)
            {
                sb.Append(await ReadNextChar());
            }
            return sb.ToString();
        }

        private async Task ReadSequence(string sequence)
        {
            foreach (char c in sequence)
            {
                if (await ReadNextChar() != c)
                    throw new FormatException("Expected '" + c + "' in sequence '" + sequence + "'");
            }
        }

        private async Task<double> ReadNumber(char c0)
        {
            bool hasDecimal = false;
            const string NUMBERS = "0123456789.";
            var sb = new StringBuilder();
            char c = c0;
            while (true)
            {
                if (c == '.')
                {
                    if (hasDecimal)
                        throw new FormatException("Invalid number; only one decimal point is allowed");
                    else
                        hasDecimal = true;
                }

                sb.Append(c);

                try
                {
                    c = await ReadNextChar();
                }
                catch (EndOfStreamException)
                {
                    break;
                }
                if (!NUMBERS.Contains(c))
                {
                    UnreadChar(c);
                    break;
                }
            }

            return double.Parse(sb.ToString());
        }

        private async Task<List<JsonObject>> ReadArray()
        {
            var result = new List<JsonObject>();

            char c = await ReadSkippingWhiteSpace();
            if (c == ']')
                return result;
            UnreadChar(c);

            while (true)
            {
                JsonObject value = await ReadObject();
                result.Add(value);
                c = await ReadSkippingWhiteSpace();
                if (c == ']')
                    return result;
                else if (c != ',')
                    throw new FormatException("Unexpected array delimiter in JSON object: '" + c + "'");
            }
        }
    }
}
