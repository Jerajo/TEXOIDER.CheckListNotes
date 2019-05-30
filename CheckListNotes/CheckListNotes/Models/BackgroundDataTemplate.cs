namespace CheckListNotes.Models
{
    public class BackgroundDataTemplate
    {
        public const string ResetDaylyTasksTimeTriggerName = "ResetDailyTaskTimeTrigger";
        public const string ResetDaylyTasksSystemTriggerName = "ResetDailyTaskSystemTrigger";
        public const string ResetDaylyTasksEntryPoint = "CheckListNotes.Tasks.ResetDailyTasks";

        public const string CompleteTaskName = "CompleteTask";
        public const string CompleteTaskEntryPoint = "CheckListNotes.Tasks.CompleteTask";

        public const string ShowTaskDetailsName = "ShowTaskDetails";
        public const string ShowTaskDetailsEntryPoint = "CheckListNotes.Tasks.ShowTaskDetails";

        public const string SnoozeToastName = "SnoozeToast";
        public const string SnoozeToastEntryPoint = "CheckListNotes.Tasks.SnoozeToast";

        //public const string BackgroundTasksGroupName = "CheckListNotesTasks";
        //public const string BackgroundTasksGroupId = "890fd9-342u3-42342-f4sg53-dh4s5hgf";
    }
}
