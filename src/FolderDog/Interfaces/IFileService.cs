using System;
namespace FolderDog.Interfaces
{
	public interface IFileService
	{
        bool TryGetFileStream(string filePath, out FileStream fileStream);
    }
}

