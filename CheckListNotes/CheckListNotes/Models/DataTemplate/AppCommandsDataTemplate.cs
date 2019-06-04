namespace CheckListNotes.Models
{
    using System.Collections;
    public class AppCommandsDataTemplate : IEnumerable
    {
        public const string CompleteTask = "CompleteTask";
        public const string ShowTaskDetails = "ShowTaskDetails";

        public IEnumerator GetEnumerator()
        {
            yield return CompleteTask;
            yield return ShowTaskDetails;
        }

        public static bool Contains(string value)
        {
            foreach (var item in (new AppCommandsDataTemplate()))
                if ((string)item == value) return true;
            return false;
        }
    }
}
