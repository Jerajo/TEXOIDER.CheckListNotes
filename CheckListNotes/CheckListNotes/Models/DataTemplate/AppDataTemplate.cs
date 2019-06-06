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

    public class LanguagesDataTemplate : IEnumerable
    {
        public const string Español = "es";
        public const string English = "en";
        public const string French = "fr";

        public IEnumerator GetEnumerator()
        {
            yield return Español;
            yield return English;
            yield return French;
        }

        public static bool Contains(string value)
        {
            foreach (var item in (new LanguagesDataTemplate()))
                if ((string)item == value) return true;
            return false;
        }
    }

    public class ToastDataTemplate
    {
        public const string AlarmToastArguments = "command=CompleteTask&taskId=";
        public const string NotificationToastArguments = "command=ShowTaskDetails&taskId=";
    }

    public class BackgroundDataTemplate
    {
        public const string ResetDaylyTasksTimeTriggerName = "ResetDailyTaskTimeTrigger";
        public const string ResetDaylyTasksSystemTriggerName = "ResetDailyTaskSystemTrigger";
        public const string ResetDaylyTasksEntryPoint = "CheckListNotes.Tasks.ResetDailyTasks";

        public const string CompleteTaskName = "CompleteTask";
        public const string ShowTaskDetailsName = "ShowTaskDetails";
        public const string CompleteTaskEntryPoint = "CheckListNotes.Tasks.CompleteTask";

        public const string SnoozeToastName = "SnoozeToast";
        public const string SnoozeToastEntryPoint = "CheckListNotes.Tasks.SnoozeToast";

        //public const string BackgroundTasksGroupName = "CheckListNotesTasks";
        //public const string BackgroundTasksGroupId = "890fd9-342u3-42342-f4sg53-dh4s5hgf";
    }
}
