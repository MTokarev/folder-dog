using System.Collections.Concurrent;
using FolderDog.Interfaces;
using FolderDog.Models;
using FolderDog.Options;
using Serilog;

namespace FolderDog.Services
{
	public class FileService : IFileService
	{
        private readonly FileServiceOptions _fileOptions;
        private readonly ILogger _logger;
        private readonly ConcurrentDictionary<string, FileCache> _proccesedFilesCache = new();

        public FileService(FileServiceOptions fileOptions, ILogger logger)
		{
            _fileOptions = fileOptions;
            _logger = logger;
        }

        public bool TryGetFileStream(string filePath, out FileStream fileStream)
        {
            fileStream = null;
            long fileSize = new FileInfo(filePath).Length;

            // Return if the file was processed before
            if (_proccesedFilesCache.TryGetValue(filePath, out FileCache fileCache))
            {
                if (fileCache.FileSize == fileSize)
                {
                    _logger.Information("The file was already processed on '{DateTime}' UTC.", fileCache.LastProcessed);
                    return false;
                }
            }

            _proccesedFilesCache[filePath] = new FileCache { FileSize = fileSize, LastProcessed = DateTime.UtcNow };

            for (int i = 1; i <= _fileOptions.RepeatAccessAttempts; i++)
            {
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
                    _logger.Information("Unable to access file '{FilePath}'. Attempt '{CurrentAttempt}' out of '{TotalAttempts}'",
                        filePath, i, _fileOptions.RepeatAccessAttempts);
                }

                Thread.Sleep(_fileOptions.WaitUntilNextRetryInMilliseconds);
            }

            _logger.Warning("File '{FilePath}' is used by another process and cannot be accessed.", filePath);
            return false;
        }
    }
}
