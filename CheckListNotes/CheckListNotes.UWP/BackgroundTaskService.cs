using System;
using Windows.Foundation;
using System.Threading.Tasks;
using Windows.ApplicationModel.Activation;
using Windows.ApplicationModel.Background;

namespace CheckListNotes.UWP
{
    internal class BackgroundTaskService : IDisposable
    {
        public bool IsDisposing { get; private set; } = false;

        #region Register Methods

        internal async Task<BackgroundTaskRegistration> RegisterInProcessBackgroundTask(string taskName, IBackgroundTrigger trigger, IBackgroundCondition condition = null, BackgroundTaskCompletedEventHandler taskCompleted = null)
        {
            if (IsDisposing) return null;
            BackgroundExecutionManager.RemoveAccess();
            await BackgroundExecutionManager.RequestAccessAsync();

            var builder = new BackgroundTaskBuilder { Name = taskName };
            builder.SetTrigger(trigger);
            if (condition != null) builder.AddCondition(condition);
            var task = builder.Register();
            if (taskCompleted != null) task.Completed += taskCompleted;
            return task;
        }

        internal async Task<BackgroundTaskRegistration> RegisterOutOfProcessBackgroundTask(string taskName, string taskEntryPoint, IBackgroundTrigger trigger, IBackgroundCondition condition = null, BackgroundTaskCompletedEventHandler taskCompleted = null)
        {
            if (IsDisposing) return null;
            BackgroundExecutionManager.RemoveAccess();
            await BackgroundExecutionManager.RequestAccessAsync();

            var builder = new BackgroundTaskBuilder
            { Name = taskName, TaskEntryPoint = taskEntryPoint };
            builder.SetTrigger(trigger);
            if (condition != null) builder.AddCondition(condition);
            var task = builder.Register();
            if (taskCompleted != null) task.Completed += taskCompleted;
            return task;
        }

        internal BackgroundTaskRegistrationGroup RegisterBackgroundGroup(string groupId, string groupFriendlyName, TypedEventHandler<BackgroundTaskRegistrationGroup, BackgroundActivatedEventArgs> backgroundActivate = null)
        {
            if (IsDisposing) return null;
            var group = BackgroundTaskRegistration.GetTaskGroup(groupId);
            if (group != null) return group;
            else
            {
                group = new BackgroundTaskRegistrationGroup(groupId, groupFriendlyName);
                if (backgroundActivate != null) group.BackgroundActivated += backgroundActivate;
                return group;
            }
        }

        internal BackgroundTaskRegistration RegisterBackgroundTaskInAGroup(BackgroundTaskRegistrationGroup group, string taskName, IBackgroundTrigger trigger, string taskEntyPoint = null, IBackgroundCondition condition = null)
        {
            foreach (var taskKeyValue in group.AllTasks)
                if (taskKeyValue.Value.Name == taskName) return taskKeyValue.Value;

            var builder = new BackgroundTaskBuilder
            {
                Name = taskName,
                TaskGroup = group,
                TaskEntryPoint = taskEntyPoint // Entry point optional for in-process task
            };
            builder.SetTrigger(trigger);
            if (condition != null) builder.AddCondition(condition);
            return builder.Register();
        }

        #endregion

        #region Unregister Methods

        internal void UnregisterBackgroundTask(string taskName)
        {
            if (IsDisposing) return;
            // Unregister a task that is part of a group
            foreach (var groupKeyValue in BackgroundTaskRegistration.AllTaskGroups)
            {
                foreach (var groupedTask in groupKeyValue.Value.AllTasks)
                    if (groupedTask.Value.Name == taskName) groupedTask.Value.Unregister(true);
            }

            // Unregister a task that isn't part of a group
            foreach (var taskKeyValue in BackgroundTaskRegistration.AllTasks)
                if (taskKeyValue.Value.Name == taskName) taskKeyValue.Value.Unregister(true);
        }

        internal void UnregisterAllBackgroundTasks()
        {
            if (IsDisposing) return;
            // Unregister tasks that are part of a group
            foreach (var groupKeyValue in BackgroundTaskRegistration.AllTaskGroups)
            {
                foreach (var groupedTask in groupKeyValue.Value.AllTasks)
                    groupedTask.Value.Unregister(true);
            }

            // Unregister tasks that aren't part of a group
            foreach (var taskKeyValue in BackgroundTaskRegistration.AllTasks)
                taskKeyValue.Value.Unregister(true);
        }

        #endregion

        #region Auxiliary Methods

        public void Dispose()
        {
            IsDisposing = true;
        }

        #endregion
    }

    #region Auxiliary Class

    internal static class BackgroundDataTemplate
    {
        #region Out Of Process Background Tasks

        public static readonly string OutOfProcessGroupName = 
            "CeckListNotesAlarms";

        public static readonly string OutOfProcessGroupId = 
            "890fd9-342u3-42342-f4sg53-dh4s5hgf";

        public static readonly string OutOfProcessNotificationName = 
            "Notification";

        public static readonly string OutOfProcessNotificationEntryPoint =
            "CeckListNotes.Alarms.Notification";

        public static readonly string OutOfProcessResetTasksName =
            "ResetDailyTasks";

        public static readonly string OutOfProcessResetTasksEntryPoint =
            "CeckListNotes.Alarms.ResetDailyTasks";

        #endregion

        #region Out Of Process Background Tasks

        public static readonly string InProcessGroupName = 
            "CeckListNotesTriggers";

        public static readonly string InProcessGroupId = 
            "890fd9-342u3-42342-f4sg53-ji690m8n";

        public static readonly string InProcessTaskUpdateName =
            "UpdateTasks";

        public static readonly string ResetDailyTasksTimeTriggerName =
            "ResetDailyTasks";
        public static readonly string ResetDailyTasksSystemTriggerName =
            "ResetDailyTasks";

        #endregion
    }

    #endregion
}

#region Implementation

/* /-------------------------/ In Process Task /-----------------------------/
 * // 1) Add a new task
 * using (var service = new BackgroundTaskService()) 
 * {
 *     var task = await service.RegisterInProcessBackgroundTask(
 *         BackgroundDataTemplate.TaskName, // Task Name
 *         new TimeTrigger(15, false), // Task Trigger
 *     );
 * }
 * 
 * // 2.1) Add a new task with condition
 * using (var service = new BackgroundTaskService()) 
 * {
 *     var task = await service.RegisterInProcessBackgroundTask(
 *         BackgroundDataTemplate.TaskName, // Task Name
 *         new TimeTrigger(15, false), // Task Trigger
 *         new SystemCondition(SystemConditionType.UserPresent), // Task Condition
 *     );
 * }
 * 
 * // 2.2) Add a new task with condition
 * using (var service = new BackgroundTaskService()) 
 * {
 *     var task = await service.RegisterInProcessBackgroundTask(
 *         BackgroundDataTemplate.TaskName, // Task Name
 *         new TimeTrigger(15, false), // Task Trigger
 *     );
 *     task.AddCondition(new SystemCondition(SystemConditionType.UserPresent)); // Condition
 * }
 * 
 * // 3.1) Add a new task with completition event
 * using (var service = new BackgroundTaskService()) 
 * {
 *     var task = await service.RegisterInProcessBackgroundTask(
 *         BackgroundDataTemplate.TaskName, // Task Name
 *         new TimeTrigger(15, false), // Task Trigger
 *         new SystemCondition(SystemConditionType.UserPresent), // Task Condition
 *         TaskCompleted // Event Handler
 *     );
 * }
 * 
 * // 3.2) Add a new task with completition event
 * using (var service = new BackgroundTaskService()) 
 * {
 *     var task = await service.RegisterInProcessBackgroundTask(
 *         BackgroundDataTemplate.TaskName, // Task Name
 *         new TimeTrigger(15, false), // Task Trigger
 *         new SystemCondition(SystemConditionType.UserPresent), // Task Condition
 *     );
 *     task.TaskCompleted += TaskCompleted; // Event Handler
 * }
 */

/* /-------------------------/ Out of Process Task /-----------------------------/
 * // 1) Add a new task
 * using (var service = new BackgroundTaskService()) 
 * {
 *     var task = await service.RegisterOutOfProcessBackgroundTask(
 *         BackgroundDataTemplate.TaskName, // Task Name
 *         BackgroundDataTemplate.TaskEntyPoint, // Task Enty Point
 *         new TimeTrigger(15, false), // Task Trigger
 *     );
 * }
 * 
 * // 2.1) Add a new task with condition
 * using (var service = new BackgroundTaskService()) 
 * {
 *     var task = await service.RegisterOutOfProcessBackgroundTask(
 *         BackgroundDataTemplate.TaskName, // Task Name
 *         BackgroundDataTemplate.TaskEntyPoint, // Task Enty Point
 *         new TimeTrigger(15, false), // Task Trigger
 *         new SystemCondition(SystemConditionType.UserPresent), // Task Condition
 *     );
 * }
 * 
 * // 2.2) Add a new task with condition
 * using (var service = new BackgroundTaskService()) 
 * {
 *     var task = await service.RegisterOutOfProcessBackgroundTask(
 *         BackgroundDataTemplate.TaskName, // Task Name
 *         BackgroundDataTemplate.TaskEntyPoint, // Task Enty Point
 *         new TimeTrigger(15, false), // Task Trigger
 *     );
 *     task.AddCondition(new SystemCondition(SystemConditionType.UserPresent)); // Condition
 * }
 * 
 * // 3.1) Add a new task with completition event
 * using (var service = new BackgroundTaskService()) 
 * {
 *     var task = await service.RegisterOutOfProcessBackgroundTask(
 *         BackgroundDataTemplate.TaskName, // Task Name
 *         BackgroundDataTemplate.TaskEntyPoint, // Task Enty Point
 *         new TimeTrigger(15, false), // Task Trigger
 *         new SystemCondition(SystemConditionType.UserPresent), // Task Condition
 *         TaskCompleted // Event Handler
 *     );
 * }
 * 
 * // 3.2) Add a new task with completition event
 * using (var service = new BackgroundTaskService()) 
 * {
 *     var task = await service.RegisterOutOfProcessBackgroundTask(
 *         BackgroundDataTemplate.TaskName, // Task Name
 *         BackgroundDataTemplate.TaskEntyPoint, // Task Enty Point
 *         new TimeTrigger(15, false), // Task Trigger
 *         new SystemCondition(SystemConditionType.UserPresent), // Task Condition
 *     );
 *     task.TaskCompleted += TaskCompleted; // Event Handler
 * }
 */

/* /-------------------------/ Background Task Group /-----------------------------/
 * // 1.1) Add background task group
 * using (var service = new BackgroundTaskService()) 
 * {
 *     var goup = await service.RegisterBackgroundTaskInAGroup(
 *         BackgroundDataTemplate.GroupId, // Group Id
 *         BackgroundDataTemplate.GroupName, // Group Name
 *     );
 * }
 * 
 * // 1.2) Add background task group with Activation Event
 * using (var service = new BackgroundTaskService()) 
 * {
 *     var goup = await service.RegisterBackgroundTaskInAGroup(
 *         BackgroundDataTemplate.GroupId, // Group Id
 *         BackgroundDataTemplate.GroupName, // Group Name
 *         BackgroundActivated // Event Handler
 *     );
 * }
 * 
 * // 1.3) Add background task group with Activation Event
 * using (var service = new BackgroundTaskService()) 
 * {
 *     var goup = await service.RegisterBackgroundTaskInAGroup(
 *         BackgroundDataTemplate.GroupId, // Group Id
 *         BackgroundDataTemplate.GroupName, // Group Name
 *     );
 *     group.BackgroundActivated += BackgroundActivated; // Event Handler
 * }
 * 
 * // 2.1) Add in procces background task group
 * using (var service = new BackgroundTaskService()) 
 * {
 *     var goup = await service.RegisterBackgroundTaskInAGroup(
 *         BackgroundDataTemplate.GroupId, // Group Id
 *         BackgroundDataTemplate.GroupName, // Group Name
 *         BackgroundActivated // Event Handler
 *     );
 *     
 *     var task = await service.RegisterBackgroundTaskInAGroup(
 *         goup, // Task Group
 *         new TimeTrigger(15, false), // Task Trigger
 *     );
 * }
 * 
 * // 2.2) Add out of procces background task group
 * using (var service = new BackgroundTaskService()) 
 * {
 *     var goup = await service.RegisterBackgroundTaskInAGroup(
 *         BackgroundDataTemplate.GroupId, // Group Id
 *         BackgroundDataTemplate.GroupName, // Group Name
 *         BackgroundActivated // Event Handler
 *     );
 *     
 *     var task = await service.RegisterBackgroundTaskInAGroup(
 *         goup, // Task Group
 *         new TimeTrigger(15, false), // Task Trigger
 *         BackgroundDataTemplate.TaskEntyPoint, // Task Enty Point
 *         new SystemCondition(SystemConditionType.UserPresent), // Task Condition
 *     );
 * }
 * 
 * // 3.1) Add in procces background task group with condition
 * using (var service = new BackgroundTaskService()) 
 * {
 *     var goup = await service.RegisterBackgroundTaskInAGroup(
 *         BackgroundDataTemplate.GroupId, // Group Id
 *         BackgroundDataTemplate.GroupName, // Group Name
 *         BackgroundActivated // Event Handler
 *     );
 *     
 *     var task = await service.RegisterBackgroundTaskInAGroup(
 *         goup, // Task Group
 *         new TimeTrigger(15, false), // Task Trigger
 *         null,
 *         new SystemCondition(SystemConditionType.UserPresent), // Task Condition
 *     );
 * }
 * 
 * // 3.2) Add out of procces background task group with condition
 * using (var service = new BackgroundTaskService()) 
 * {
 *     var goup = await service.RegisterBackgroundTaskInAGroup(
 *         BackgroundDataTemplate.GroupId, // Group Id
 *         BackgroundDataTemplate.GroupName, // Group Name
 *         BackgroundActivated // Event Handler
 *     );
 *     
 *     var task = await service.RegisterBackgroundTaskInAGroup(
 *         goup, // Task Group
 *         new TimeTrigger(15, false), // Task Trigger
 *         BackgroundDataTemplate.TaskEntyPoint, // Task Enty Point
 *         new SystemCondition(SystemConditionType.UserPresent), // Task Condition
 *     );
 * }
 */

/* /-------------------------/ Unregister Background Tasks /-----------------------------/
 * // Unregister a background task
 * using (var service = new BackgroundTaskService()) 
 * {
 *     service.UnregisterBackgroundTask(BackgroundDataTemplate.TaskName);
 * }
 * 
 * // Unregister all background task
 * using (var service = new BackgroundTaskService()) 
 * {
 *     service.UnregisterAllBackgroundTasks();
 * }
 */

#endregion