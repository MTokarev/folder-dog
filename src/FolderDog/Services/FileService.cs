using System.Collections.Concurrent;
using FolderDog.Interfaces;
using FolderDog.Models;
using FolderDog.Options;
using Serilog;

namespace FolderDog.Services
{
    /// <summary>
    /// <see cref="IFileService"/>
    /// </summary>
	public class FileService : IFileService
	{
        private readonly FileServiceOptions _fileOptions;
        private readonly ILogger _logger;
        private readonly ConcurrentDictionary<string, FileCache> _processedFilesCache = new();

        public FileService(FileServiceOptions fileOptions, ILogger logger)
		{
            _fileOptions = fileOptions;
            _logger = logger;
        }

        /// <summary>
        /// <see cref="IFileService.TryGetFileStream(string, out FileStream)"/>
        /// </summary>
        public bool TryGetFileStream(string filePath, out FileStream fileStream)
        {
            fileStream = null;
            if (_fileOptions.SkipProcessedFiles)
            {
                _logger.Debug("Option '{OptionName}' is enabled. Checking if file '{FilePath}' was processed before.",
                    nameof(_fileOptions.SkipProcessedFiles),
                    filePath);
                if(HasFileBeenProcessed(filePath))
                {
                    return false;
                }
            }

            // When external process write a big file usually it does that by batches
            // Adding delay could solve this case
            if (_fileOptions.WaitBeforeExecutionInMilliseconds > 0)
            {
                _logger.Information("Waiting '{Milliseconds}' miliseconds to allow external application to flush the memory for the '{FilePath}' to a disk",
                    _fileOptions.WaitBeforeExecutionInMilliseconds,
                    filePath);
            }

            // Trying access the file multiple time. To give opportunity to the external app to flush the data to the disk
            for (int i = 1; i <= _fileOptions.RepeatAccessAttempts; i++)
            {
                _logger.Information("Trying to access file '{FilePath}'. Attempt '{CurrentAttempt}' out of '{TotalAttempts}'...",
                    filePath, i, _fileOptions.RepeatAccessAttempts);

                try
                {
                    var fs = new FileStream(
                            filePath,
                            FileMode.Open,
                            FileAccess.Read,
                            FileShare.ReadWrite);
                    fileStream = fs;
                    return true;
                }
                catch (IOException)
                {
                    _logger.Information("Unable to access file '{FilePath}'.",
                        filePath);
                }

                Thread.Sleep(_fileOptions.WaitUntilNextRetryInMilliseconds);
            }

            _logger.Warning("File '{FilePath}' is used by another process and cannot be accessed.", filePath);
            return false;
        }

        /// <summary>
        /// Check if file has been processed.
        /// This function will use the file name and the size.
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns>True if file (compared by file name and size) has been already processed. Otherwise False.</returns>
        private bool HasFileBeenProcessed(string filePath)
        {
            var fileInfo = new FileInfo(filePath);
            if (_processedFilesCache.TryGetValue(filePath, out FileCache fileCache))
            {
                if (fileCache.FileSize == fileInfo.Length)
                {
                    _logger.Information("The file '{FileFullName}' was already processed on '{DateTime}' UTC.",
                        fileInfo.FullName,
                        fileCache.LastProcessed);
                    return true;
                }
            }

            _logger.Debug("Adding '{FileFullName}' to the cache...", fileInfo.FullName);
            _processedFilesCache[filePath] = new FileCache { FileSize = fileInfo.Length, LastProcessed = DateTime.UtcNow };

            return false;
        }
    }
}
