namespace FolderDog.Models
{
	/// <summary>
	/// File cache model
	/// </summary>
	public class FileCache
	{
		/// <summary>
		/// Time when the file was processed
		/// </summary>
		public DateTime LastProcessed { get; set; }

		/// <summary>
		/// File Size
		/// </summary>
		public long FileSize { get; set; }
	}
}
