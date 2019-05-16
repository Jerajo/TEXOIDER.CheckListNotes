namespace CheckListNotes.Models
{
    using Windows.ApplicationModel.Background;
    public sealed class BackgroundTaskModel : PortableClasses.Interfaces.IBackgroundTask
    {
        public string Title { get; set; }
        public string EntryPoint { get; set; }
        public object Trigger { get; set; }
        public BackgroundTaskCompletedEventHandler OnComplete { get; set; }
    }
}
