using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;

namespace ChilliCoreTemplate.Models
{
    /// <summary>
    /// Extension methods for System.IO.Stream.
    /// </summary>
    public static class StreamExtensions
    {
        /// <summary>
        /// Converts System.IO.Stream to byte array.
        /// </summary>
        /// <param name="s">The System.IO.Stream</param>
        /// <returns>A byte array.</returns>
        public static byte[] ReadToByteArray(this Stream s)
        {
            if (s is MemoryStream)
                return (s as MemoryStream).ToArray();

            using (MemoryStream memoryStream = new MemoryStream())
            {
                s.CopyTo(memoryStream);
                return memoryStream.ToArray();
            }
        }

        public static async Task<byte[]> ReadToByteArrayAsync(this Stream s)
        {
            if (s is MemoryStream)
                return (s as MemoryStream).ToArray();

            using (MemoryStream memoryStream = new MemoryStream())
            {
                await s.CopyToAsync(memoryStream);
                return memoryStream.ToArray();
            }
        }

        public static T DeserializeTo<T>(this Stream stream, StreamSerializationOptions options = null)
        {
            options = options ?? new StreamSerializationOptions();

            try
            {
                if (stream == null)
                    return default(T);

                if (options.SkipFormatterForStrings && typeof(T) == typeof(string))
                {
                    using (var streamReader = new StreamReader(stream, Encoding.UTF8, false, 4096, leaveOpen: true))
                    {
                        object value = streamReader.ReadToEnd();
                        return (T)value;
                    }
                }

                var formatter = options.FormatterFactory();
                var obj = formatter.Deserialize(stream);
                return (T)obj;
            }
            finally
            {
                if (!options.LeaveOpen && stream != null)
                {
                    stream.Dispose();
                }
            }
        }
    }

    public class StreamSerializationOptions
    {
        public StreamSerializationOptions()
        {
            this.LeaveOpen = false;
            this.FormatterFactory = DefaultFormatter;
        }

        private static IFormatter DefaultFormatter()
        {
            return new BinaryFormatter();
        }

        public bool LeaveOpen { get; set; }

        public bool SkipFormatterForStrings { get; set; }

        public Func<IFormatter> FormatterFactory { get; set; }
    }
}
