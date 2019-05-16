namespace PortableClasses.Interfaces
{
    public interface IBackgroundTask
    {
        /// <summary>
        /// The background task name. It has to be unique.
        /// </summary>
        string Title { get; set; }
        /// <summary>
        /// The name space and the class name of the out-of-prosess background task.
        /// </summary>
        string EntryPoint { get; set; }
        /// <summary>
        /// Execution trigget for the background task.
        /// </summary>
        object Trigger { get; set; }
    }
}
