using FolderDog.Models;

namespace FolderDog.Interfaces
{
    /// <summary>
    /// Sender interface
    /// </summary>
    public interface IMessageSender
    {
        /// <summary>
        /// Send Message
        /// </summary>
        /// <param name="message">Message body</param>
        /// <param name="pathToAttachment">Optional path to the atachment file</param>
        /// <returns><see cref="Result"/></returns>
        Result SendMessage(string message, string? pathToAttachment = null);
    }
}