using FolderDog.Models;

namespace FolderDog.Interfaces
{
    /// <summary>
    /// Interface for hook service
    /// </summary>
    public interface IHookService
    {
        /// <summary>
        /// Send hook to a service
        /// </summary>
        /// <param name="e"><see cref="FileSystemEventArgs"></param>
        /// <returns><see cref="Result"/></returns>
        Task<Result> SendHookAsync(FileSystemEventArgs e);
    }
}