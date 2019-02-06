using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace TT.XBRLProcessor
{
    public enum ZipAction
    {
        Add,
        Extract
    }

    public static class ZipExtensions
    {
        /// <summary>Gets an existing zip entry, if a zip entry with the specified name
        /// exists; otherwise creates a zip entry with the specified name.</summary>
        /// <remarks>The <b>ZipArchive</b> must be open in <b>ZipArchiveMode.Update</b> mode.</remarks>
        public static ZipArchiveEntry CreateOrOpenEntry(this ZipArchive zip, string entryName)
        {
            var zipEntry = zip.GetEntry(entryName);

            if (zipEntry == null)
                zipEntry = zip.CreateEntry(entryName, CompressionLevel.Optimal);

            return zipEntry;
        }

        /// <summary>An async alternative to the <see cref="ZipArchiveEntry.ExtractToFile"/> extension method, using asynchronous IO.</summary>
        /// <remarks>The <b>overwrite</b> parameter was added to be consistent with <b>ZipArchiveEntry.ExtractToFile</b>, although my own
        /// preference is always to overwrite; hence my <b>ExtractEntriesToDirectoryAsync</b> implementation defaults to passing <b>true</b>.</remarks>
        public static async Task ExtractToFileAsync(this ZipArchiveEntry source, string destinationFileName, bool overwrite)
        {
            if (overwrite || !File.Exists(destinationFileName))
            {
                Directory.CreateDirectory(Path.GetDirectoryName(destinationFileName));

                using (var destStream = new FileStream(destinationFileName, FileMode.Create, FileAccess.Write, FileShare.None, XBRLConstants.LargeBufferSize, true))
                {
                    await source.ExtractToStreamAsync(destStream);
                }
                File.SetLastWriteTime(destinationFileName, source.LastWriteTime.DateTime);
            }
        }

        /// <summary>Extract the <b>ZipArchiveEntry</b> to the specified
        /// <b>Stream</b> asynchronously, using asynchronous IO.</summary>
        public static async Task ExtractToStreamAsync(this ZipArchiveEntry source, Stream destination)
        {
            if (source == null)
                throw new ArgumentNullException("source");

            if (destination == null)
                throw new ArgumentNullException("destination");

            using (var entryStream = source.Open())
            {
                using (var stream = new MemoryStream())
                {
                    await entryStream.CopyToAsync(stream, XBRLConstants.LargeBufferSize);
                    stream.Position = 0;

                    await stream.CopyToAsync(destination, XBRLConstants.LargeBufferSize);
                }
            }
        }
    }

    public class ZipErrorEventArgs : EventArgs
    {
        public ZipErrorEventArgs(string message)
        {
            Message = message;
        }

        public string Message { get; private set; }
    }

    /// <summary>Async methods for working with zip files. This type implements async methods to create, update and
    /// delete from zip archives, and raises events during each method to give some progress information.</summary>
    /// <remarks>Microsoft have made it easy to compliment their new zip file support with async extension methods,
    /// since they expose <see cref="System.IO.Stream"/> types for both the Zip archive and its entries.</remarks>
    public class ZipFileHelper
    {
        private bool cancel;

        private Lazy<PauseTokenSource> pauseTokenSource = new Lazy<PauseTokenSource>(() => new PauseTokenSource());

        public event EventHandler<ZipErrorEventArgs> Error;

        public event EventHandler<ZipProgressEventArgs> Progress;

        /// <summary>The Cancel and Pause properties here are used by
        /// my ProgressDialog, and do not apply to all methods.</summary>
        public bool Cancel
        {
            get { return cancel; }
            set
            {
                cancel = value;
                if (cancel)
                    Pause = false;
            }
        }

        public bool Pause
        {
            get { return PauseTokenSource.IsPaused; }
            set { PauseTokenSource.IsPaused = value; }
        }

        protected PauseTokenSource PauseTokenSource
        {
            get { return pauseTokenSource.Value; }
        }

        /// <summary>Save a single file or directory to zip asynchronously, using asynchronous IO.</summary>
        /// <remarks>This can be used as an async alternative to the static method,
        /// <see cref="System.IO.Compression.ZipFile.CreateFromDirectory()"/>.</remarks>
        /// <param name="path">The file or directory to zip.</param>
        /// <param name="zipFileName">The path of the zip file to create.</param>
        /// <param name="filesToExclude">(Optional) Any file names that should be excluded.</param>
        /// <returns>A Task that represents completion of the method.</returns>
        public Task CreateZipArchiveAsync(string path, string zipFileName, params string[] filesToExclude)
        {
            return CreateZipArchiveAsync(new string[] { path }, zipFileName, filesToExclude);
        }

        /// <summary>Save a file (or files) to zip asynchronously, using asynchronous IO.</summary>
        /// <param name="paths">The files and/or directories to create a zip file.</param>
        /// <param name="zipFileName">The path of the zip file to create.</param>
        /// <param name="filesToExclude">(Optional) Any file names that should be excluded.</param>
        /// <returns>A Task that represents completion of the method.</returns>
        public async Task CreateZipArchiveAsync(IEnumerable<string> paths, string zipFileName, params string[] filesToExclude)
        {
            var files = await BuildFileCollectionAsync(paths, filesToExclude);

            using (var stream = new FileStream(zipFileName, FileMode.Create, FileAccess.ReadWrite, FileShare.None, XBRLConstants.LargeBufferSize, true))
            {
                // ZipArchiveMode.Update mode is required here to allow setting entries' last write time, after creating and writing them.
                using (var zip = new ZipArchive(stream, ZipArchiveMode.Update))
                {
                    var count = 0;
                    var total = files.Count();

                    foreach (var kvp in files)
                    {
                        try
                        {
                            using (var memoryStream = new MemoryStream(await FileAsync.ReadAllBytesAsync(kvp.Key)))
                            {
                                if (memoryStream.Length > 0)
                                {
                                    ZipArchiveEntry entry;

                                    using (var zipStream = (entry = zip.CreateEntry(kvp.Value, CompressionLevel.Optimal)).Open())
                                    {
                                        await memoryStream.CopyToAsync(zipStream, XBRLConstants.LargeBufferSize);
                                    }

                                    var fileInfo = new FileInfo(kvp.Key);
                                    entry.LastWriteTime = new DateTimeOffset(fileInfo.LastWriteTime);

                                    OnProgress(new ZipProgressEventArgs(kvp.Key, kvp.Value, ++count, total, ZipAction.Add));
                                }
                                else
                                    total--;
                            }
                        }
                        catch (Exception ex)
                        {
                            OnError(new ZipErrorEventArgs(string.Format("Error adding '{0}'. {1}.", Path.GetFileName(kvp.Key), ex.Message)));
                        }
                    }
                }
            }
        }

        /// <summary>Asynchronously deletes all the <b>ZipArchiveEntries</b> specified from the zip file.</summary>
        public async Task DeleteFromZipArchiveAsync(IEnumerable<string> paths, string zipFileName)
        {
            using (var memoryStream = new MemoryStream(await FileAsync.ReadAllBytesAsync(zipFileName)))
            {
                using (var zip = new ZipArchive(memoryStream, ZipArchiveMode.Update))
                {
                    foreach (var entry in zip.Entries.Where(e => paths.Contains(e.FullName)).ToArray())
                    {
                        if (entry != null)
                        {
                            try { entry.Delete(); }

                            catch (Exception ex)
                            {
                                string entryDescription = entry.FullName.IndexOfAny(XBRLConstants.DelimiterChars) == -1 ?
                                    entry.FullName :
                                    entry.FullName.Substring(entry.FullName.LastIndexOfAny(XBRLConstants.DelimiterChars) + 1);

                                OnError(new ZipErrorEventArgs(
                                    string.Format("Error deleting '{0}' from '{1}'. {2}.",
                                    entryDescription,
                                    Path.GetFileName(zipFileName),
                                    ex.Message)));
                            }
                        }
                    }
                }
                await FileAsync.WriteAllBytesAsync(zipFileName, memoryStream.ToArray());
            }
        }

        /// <summary>Extract the specified entries to the directory specified, using asynchronous IO.</summary>
        /// <remarks>I normally use a progress dialog with a Cancel button here, so this method is cancellable.</remarks>
        public async Task ExtractEntriesToDirectoryAsync(string sourceArchiveFileName, IEnumerable<string> entryNames, string destinationDirectoryName, bool overwrite = true)
        {
            using (var stream = new FileStream(sourceArchiveFileName, FileMode.Open, FileAccess.Read, FileShare.None, XBRLConstants.LargeBufferSize, true))
            {
                using (var zipArchive = new ZipArchive(stream, ZipArchiveMode.Read))
                {
                    var entries = new List<ZipArchiveEntry>();

                    foreach (var f in entryNames.
                        Where(n => !n.EndsWith(Path.AltDirectorySeparatorChar.ToString()) && !n.EndsWith(Path.DirectorySeparatorChar.ToString())).ToArray())
                    {
                        var entry = zipArchive.Entries.FirstOrDefault(e => string.Compare(e.FullName, f, StringComparison.InvariantCultureIgnoreCase) == 0);

                        if (entry != null)
                            entries.Add(entry);
                    }

                    var count = 0;
                    var total = entries.Count();

                    foreach (var entry in entries)
                    {
                        if (Pause)
                            await PauseTokenSource.Token.WaitWhilePausedAsync();

                        /* Note that we don't do any cleanup of whatever was already
                         * extracted. That is left to the user who clicked Cancel. */
                        if (Cancel)
                            return;

                        string destFilename = Path.Combine(destinationDirectoryName, entry.FullName.Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar));

                        await entry.ExtractToFileAsync(destFilename, overwrite);

                        OnProgress(new ZipProgressEventArgs(destFilename, destinationDirectoryName, ++count, total, ZipAction.Extract));
                    }
                }
            }
        }

        /// <summary>Asynchronously extracts all zip entries contained by a directory
        /// inside the zip file with the name specified, to the directory specified.</summary>
        public async Task ExtractEntriesToDirectoryAsync(string sourceArchiveFileName, string entryDirectoryNameFilter, string destinationDirectoryName, bool overwrite = true)
        {
            using (var stream = new FileStream(sourceArchiveFileName, FileMode.Open, FileAccess.Read, FileShare.None, XBRLConstants.LargeBufferSize, true))
            {
                using (var zipArchive = new ZipArchive(stream, ZipArchiveMode.Read))
                {
                    var entries = zipArchive.Entries.Where(e => e.FullName.ToLowerInvariant().StartsWith(entryDirectoryNameFilter.ToLowerInvariant()));
                    var count = 0;
                    var total = entries.Count();

                    foreach (var entry in entries)
                    {
                        if (Pause)
                            await PauseTokenSource.Token.WaitWhilePausedAsync();

                        if (Cancel)
                            return;

                        var destFilename = Path.Combine(destinationDirectoryName, entry.FullName.Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar));

                        await entry.ExtractToFileAsync(destFilename, overwrite);

                        OnProgress(new ZipProgressEventArgs(destFilename, destinationDirectoryName, count, total, ZipAction.Extract));
                    }
                }
            }
        }

        /// <summary>An async alternative to <b>ZipFile.ExtractToDirectory</b>, using asynchronous IO.</summary>
        /// <remarks>I normally use a progress dialog with a Cancel button here, so this method is cancellable.</remarks>
        public async Task ExtractToDirectoryAsync(string sourceArchiveFileName, string destinationDirectoryName)
        {
            using (var stream = new FileStream(sourceArchiveFileName, FileMode.Open, FileAccess.Read, FileShare.None, XBRLConstants.LargeBufferSize, true))
            {
                using (var zip = new ZipArchive(stream, ZipArchiveMode.Read))
                {
                    var count = 0;
                    var total = zip.Entries.Count;

                    foreach (var entry in zip.Entries)
                    {
                        if (Pause)
                            await PauseTokenSource.Token.WaitWhilePausedAsync();

                        /* Note that we don't do any cleanup of whatever was already
                         * extracted. That is left to the user who clicked Cancel. */
                        if (Cancel)
                            return;

                        var destFilename = Path.Combine(destinationDirectoryName, entry.FullName.Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar));

                        if (destFilename.EndsWith(Path.AltDirectorySeparatorChar.ToString()) || destFilename.EndsWith(Path.DirectorySeparatorChar.ToString()))
                        {
                            /* Zip Entries for a directory are rare, but we will find them sometimes. Just create
                             * the directory, and continue to the next entry. There is nothing to extract here. */
                            Directory.CreateDirectory(destFilename);
                            continue;
                        }

                        await entry.ExtractToFileAsync(destFilename, true);

                        OnProgress(new ZipProgressEventArgs(destFilename, destinationDirectoryName, ++count, total, ZipAction.Extract));
                    }
                }
            }
        }

        /// <summary>Add a file or directory to a zip asynchronously, using asynchronous IO.</summary>
        /// <remarks>This is the same as <b>CreateZipArchiveAsync</b> except it updates an existing zip file.</remarks>
        /// <param name="path">The file or directory to zip.</param>
        /// <param name="zipFileName">The path of the zip file to update.</param>
        /// <param name="filesToExclude">(Optional) Any file names that should be excluded.</param>
        /// <returns>A Task that represents completion of the method.</returns>
        public Task UpdateZipArchiveAsync(string path, string zipFileName, params string[] filesToExclude)
        {
            return UpdateZipArchiveAsync(new string[] { path }, zipFileName, filesToExclude);
        }

        /// <summary>Add a file (or files) to a zip asynchronously, using asynchronous IO.</summary>
        /// <remarks>This is exactly the same as the <see cref="CreateZipArchiveAsync"/> method,
        /// except that it updates an existing zip file.</remarks>
        /// <param name="paths">The files and/or directories to add to a zip file.</param>
        /// <param name="zipFileName">The path of the zip file to update.</param>
        /// <param name="filesToExclude">(Optional) Any file names that should be excluded.</param>
        /// <returns>A Task that represents completion of the method.</returns>
        public async Task UpdateZipArchiveAsync(IEnumerable<string> paths, string zipFileName, params string[] filesToExclude)
        {
            var files = await BuildFileCollectionAsync(paths, filesToExclude);

            /* Note the FileShare parameter needs to be FileShare.Read here. Otherwise, if the zip file
             * is already open in another process, the FileStream constructor will throw an exception. */
            using (var stream = new FileStream(zipFileName, FileMode.Open, FileAccess.ReadWrite, FileShare.Read, XBRLConstants.LargeBufferSize, true))
            {
                using (var zip = new ZipArchive(stream, ZipArchiveMode.Update))
                {
                    var count = 0;

                    foreach (var kvp in files)
                    {
                        try
                        {
                            using (var memoryStream = new MemoryStream(await FileAsync.ReadAllBytesAsync(kvp.Key)))
                            {
                                if (memoryStream.Length > 0)
                                {
                                    ZipArchiveEntry entry;

                                    using (var zipStream = (entry = zip.CreateOrOpenEntry(kvp.Value)).Open())
                                    {
                                        await memoryStream.CopyToAsync(zipStream, XBRLConstants.LargeBufferSize);
                                    }

                                    var fileInfo = new FileInfo(kvp.Key);
                                    entry.LastWriteTime = new DateTimeOffset(fileInfo.LastWriteTime);

                                    OnProgress(new ZipProgressEventArgs(kvp.Key, kvp.Value, ++count, files.Count(), ZipAction.Add));
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            OnError(new ZipErrorEventArgs(string.Format("Error updating '{0}' with '{1}'. {2}.", Path.GetFileName(zipFileName), Path.GetFileName(kvp.Key), ex.Message)));
                        }
                    }
                }
            }
        }

        protected virtual void OnError(ZipErrorEventArgs e)
        {
            var error = Error;

            if (error != null)
                error(this, e);
        }

        protected virtual void OnProgress(ZipProgressEventArgs e)
        {
            var progress = Progress;

            if (progress != null)
                progress(this, e);
        }

        private async Task<IEnumerable<KeyValuePair<string, string>>> BuildFileCollectionAsync(IEnumerable<string> paths, string[] filesToExclude)
        {
            var files = await BuildFileCollectionFromPathsAsync(paths);

            if (filesToExclude.Length > 0)
            {
                files = files.Where(kvp =>
                {
                    var include = true;

                    foreach (var exclude in filesToExclude)
                    {
                        include = !Wildcard.IsMatch(Path.GetFileName(kvp.Key).ToLowerInvariant(), exclude.ToLowerInvariant());

                        if (include)
                        {
                            var directory = kvp.Key;
                            var sub = directory;

                            while (include && (sub = Path.GetDirectoryName(sub)) != null && sub.IndexOf(Path.DirectorySeparatorChar) != -1) 
                            {
                                var part = sub.Substring(sub.LastIndexOf(Path.DirectorySeparatorChar)+1);
                                include &= !Wildcard.IsMatch(part.ToLowerInvariant(), exclude.ToLowerInvariant());
                            }
                        }

                        if (!include)
                            break;
                    }

                    return include;
                });
            }
            return files;
        }

        /// <summary>Builds a collection of key-value pairs, where the key is the input file path and the
        /// value is the relative path to be stored in the zip file. This is done in parallel.</summary>
        private async Task<IEnumerable<KeyValuePair<string, string>>> BuildFileCollectionFromPathsAsync(IEnumerable<string> paths)
        {
            var fileEntries = paths.ToArray();
            var pathToRelativePathMap = new ConcurrentDictionary<string, string>();

            var rootLength = fileEntries.Length == 1 && Directory.Exists(fileEntries[0]) ? fileEntries[0].Length + 1 : Path.GetDirectoryName(fileEntries[0]).Length + 1;

            Func<string, bool> addEntry = (f) =>
            {
                /* Remove the root directory to create a relative path string.
                 * The built-in methods to create zip files use backslashes in the entries,
                 * but I prefer to use forward-slashes, the same as the rest of the world. */
                return pathToRelativePathMap.TryAdd(f, f.Substring(rootLength).Replace(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar));
            };

            var degreeOfParallelism = Environment.ProcessorCount;

            /* The input collection can contain files as well as directories. For each
             * item, if it is a file, add it. If it is a directory, add its contents. */
            await fileEntries.ForEachAsync(degreeOfParallelism, async path =>
            {
                try
                {
                    if (Directory.Exists(path))
                    {
                        var filesFound = Directory.GetFiles(path, "*", SearchOption.AllDirectories);

                        if (filesFound.Length > 0)
                            await filesFound.ForEachAsync(degreeOfParallelism, async found => await Task.FromResult(addEntry(found)));
                    }
                    else if (File.Exists(path))
                        await Task.FromResult(addEntry(path));
                }
                catch (IOException) { }
                catch (UnauthorizedAccessException) { }
                catch (Exception) { throw; }
                await Task.FromResult(false);
            });

            var pathsArray = pathToRelativePathMap.ToArray();

            // Sort the results in parallel.
            System.Threading.Algorithms.ParallelAlgorithms.Sort(pathsArray,
                Comparer<KeyValuePair<string, string>>.Create((x, y) => string.Compare(x.Key, y.Key, StringComparison.InvariantCultureIgnoreCase)));

            return pathsArray;
        }
    }

    /// <summary>Event arguments when extracting a single ZipArchiveEntry from a
    /// ZipArchive. Indicates rudimentary progress info for display in UI.</summary>
    public class ZipProgressEventArgs : EventArgs
    {
        public ZipProgressEventArgs(string filename, string destinationDirectoryName, int filesCompletedCount, int filesInZipCount, ZipAction zipAction)
        {
            Filename = filename;
            DestinationPathName = destinationDirectoryName;
            FilesCompletedCount = filesCompletedCount;
            FilesInZipCount = filesInZipCount;
            ZipAction = zipAction;
        }

        public int FilesCompletedCount { get; private set; }

        public int FilesInZipCount { get; private set; }

        public string DestinationPathName { get; private set; }

        public string Filename { get; private set; }

        public ZipAction ZipAction { get; private set; }
    }
}
