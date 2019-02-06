using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TT.XBRLProcessor
{
    /// <summary>These are not extension methods, but are static methods
    /// similar to certain synchronous static methods on System.IO.File.
    /// (File.ReadAllBytes; File.WriteAllBytes; File.ReadAllLines etc.)</summary>
    /// <remarks>All these methods rely on my other extension methods,
    /// especially <b>StreamExtensions.CopyToStreamAsync</b>.</remarks>
    public static class FileAsync
    {
        private static readonly byte[] NoBytes = Enumerable.Empty<byte>().ToArray();

        /// <summary>Asynchronously appends lines to a file, and then closes the file.</summary>
        /// <param name="path">The file to append the lines to. The file is created if it does not already exist.</param>
        /// <param name="contents">The lines to append to the file.</param>
        /// <returns>A Task that represents completion of the method.</returns>
        public static async Task AppendAllLinesAsync(string path, IEnumerable<string> contents)
        {
            var lines = File.Exists(path) ? new List<string>(await ReadAllLinesAsync(path)) : new List<string>();
            lines.AddRange(contents);

            await WriteAllLinesAsync(path, lines.ToArray());
        }

        /// <summary>Asynchronously creates a new file, writes a collection
        /// of strings to the file, and then closes the file.</summary>
        /// <param name="path">The file to write to.</param>
        /// <param name="contents">The lines to write to the file.</param>
        /// <returns>A Task that represents completion of the method.</returns>
        public static async Task WriteAllLinesAsync(string path, IEnumerable<string> contents)
        {
            await WriteAllLinesAsync(path, contents, Encoding.UTF8);
        }

        /// <summary>Asynchronously creates a new file, writes the specified string to the file,
        /// and then closes the file. If the target file already exists, it is overwritten.
        /// </summary>
        /// <param name="path">The file to write to.</param>
        /// <param name="contents">The string to write to the file.</param>
        /// <returns>A Task that represents completion of the method.</returns>
        public static async Task WriteAllTextAsync(string path, string contents)
        {
            await WriteAllLinesAsync(path, new string[] { contents });
        }

        /// <summary>Asynchronously creates a new file by using the specified encoding,
        /// writes a collection of strings to the file, and then closes the file.</summary>
        /// <param name="path">The file to write to.</param>
        /// <param name="contents">The lines to write to the file.</param>
        /// <param name="encoding">The character encoding to use.</param>
        /// <returns>A Task that represents completion of the method.</returns>
        public static async Task WriteAllLinesAsync(string path, IEnumerable<string> contents, Encoding encoding)
        {
            using (var memoryStream = new MemoryStream(contents.SelectMany(s => encoding.GetBytes(s.EndsWith("\r\n") ? s : s.TrimEnd() + "\r\n")).ToArray()))
            {
                using (var stream = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.None, XBRLConstants.BufferSize, true))
                {
                    await memoryStream.CopyToAsync(stream, XBRLConstants.BufferSize);
                }
            }
        }

        /// <summary>Asynchronously opens a text file, reads all
        /// lines of the file, and then closes the file.</summary>
        /// <param name="path">The file to open for reading.</param>
        /// <returns>A Task that will, on completion of the method,
        /// contain a string array containing all lines of the file.</returns>
        public static async Task<string[]> ReadAllLinesAsync(string path)
        {
            var lines = new List<string>();
            var line = string.Empty;

            using (var reader = new StreamReader(path, true))
            {
                while ((line = await reader.ReadLineAsync()) != null)
                    lines.Add(line);
            }

            return lines.ToArray();
        }

        /// <summary>Asynchronously opens a binary file, reads the contents of
        /// the file into a byte array, and then closes the file.</summary>
        /// <param name="filename">The file to open for reading.</param>
        /// <returns>A Task that will, on completion of the method,
        /// contain a byte array of the contents of the file.</returns>
        public static Task<byte[]> ReadAllBytesAsync(string filename)
        {
            return ReadAllBytesAsync(filename, XBRLConstants.BufferSize);
        }

        public static async Task<byte[]> ReadAllBytesAsync(string filename, int bufferSize)
        {
            try
            {
                var length = (int)new FileInfo(filename).Length;
                byte[] bytes = NoBytes;

                using (var stream = new FileStream(filename, FileMode.Open, FileAccess.Read, FileShare.Read, bufferSize, true))
                {
                    using (var memoryStream = new MemoryStream(length))
                    {
                        await stream.CopyToAsync(memoryStream, bufferSize);
                        bytes = memoryStream.ToArray();
                    }
                }
                return bytes;
            }
            catch { return NoBytes; }
        }

        /// <summary>Creates a new file, writes the specified byte array to the file asynchronously,
        /// and then closes the file. If the target file already exists, it is overwritten.</summary>
        /// <param name="path">The file to write to.</param>
        /// <param name="bytes">The bytes to write to the file.</param>
        public static async Task WriteAllBytesAsync(string path, byte[] bytes)
        {
            using (var memoryStream = new MemoryStream(bytes))
            {
                using (var stream = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.None, XBRLConstants.BufferSize, true))
                {
                    await memoryStream.CopyToAsync(stream, XBRLConstants.BufferSize);
                }
            }
        }

        public static async Task WriteAllBytesAsync(string path, byte[] bytes, int bufferSize)
        {
            using (var memoryStream = new MemoryStream(bytes))
            {
                using (var stream = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.None, bufferSize, true))
                {
                    await memoryStream.CopyToAsync(stream, bufferSize);
                }
            }
        }

        public static Task AppendAllBytesAsync(string path, byte[] bytes)
        {
            return AppendAllBytesAsync(path, bytes, XBRLConstants.BufferSize);
        }

        public static async Task AppendAllBytesAsync(string path, byte[] bytes, int bufferSize)
        {
            using (var memoryStream = new MemoryStream(bytes))
            {
                using (var stream = new FileStream(path, FileMode.Append, FileAccess.Write, FileShare.None, bufferSize, true))
                {
                    await memoryStream.CopyToAsync(stream, bufferSize);
                }
            }
        }
    }
}
