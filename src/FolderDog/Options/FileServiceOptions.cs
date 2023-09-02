namespace FolderDog.Options
{
	/// <summary>
	/// File service options
	/// </summary>
	public class FileServiceOptions
	{
		/// <summary>
		/// File can be used by the process.
		/// This param control how many attempts will be made accessing file.
		/// </summary>
		public int RepeatAccessAttempts { get; set; }

		/// <summary>
		/// The wait time in milliseconds before attempts
		/// <seealso cref="RepeatAccessAttempts"/>
		/// </summary>
		public int WaitUntilNextRetryInMilliseconds { get; set; }
	}
}
