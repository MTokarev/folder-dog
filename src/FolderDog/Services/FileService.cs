using FolderDog.Interfaces;
using FolderDog.Options;
using Serilog;

namespace FolderDog.Services
{
	public class FileService : IFileService
	{
        private readonly FileServiceOptions _fileOptions;
        private readonly ILogger _logger;

        public FileService(FileServiceOptions fileOptions, ILogger logger)
		{
            _fileOptions = fileOptions;
            _logger = logger;
        }

        public bool TryGetFileStream(string filePath, out FileStream fileStream)
        {
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

            fileStream = null;
            return false;
        }
    }
}
