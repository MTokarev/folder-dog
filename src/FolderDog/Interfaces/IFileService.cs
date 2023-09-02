namespace FolderDog.Interfaces
{
    /// <summary>
    /// File service interface
    /// </summary>
    public interface IFileService
	{
        /// <summary>
        /// Tries to open a file stream
        /// </summary>
        /// <param name="filePath">File path</param>
        /// <param name="fileStream">Out: File stream</param>
        /// <returns>true if stream is instantiated, otherwise false</returns>
        bool TryGetFileStream(string filePath, out FileStream fileStream);
    }
}

