using System;
using System.Linq;
using Xamarin.Forms;
using PropertyChanged;
using System.Diagnostics;
using PortableClasses.Enums;
using System.Collections.Generic;

namespace CheckListNotes.Models
{
    [AddINotifyPropertyChangedInterfaceAttribute]
    public class CheckTaskViewModel : BaseModel, ICardModel, IDisposable
    {
        #region Atributes

        public bool? isAnimating = false;
        internal ToastTypesTime? notifyOn = ToastTypesTime.None;
        private string selectedNotificationTimeIndex;
        internal DateTime? expirationDate;
        internal TimeSpan? expiration;
        internal bool? isDaily = false;
        private bool? isDisposing = false;

        #endregion

        #region Base Atributes

        public string Id { get; set; }
        public string Name { get; set; }
        public int? Position { get; set; }
        public string ToastId { get; set; }
        public bool? IsTaskGroup { get; set; }
        public bool IsChecked { get => IsCompleted; set => IsCompleted = value; } 
        public DateTime? ExpirationDate { get => expirationDate; set => expirationDate = value; }
        public TimeSpan? Expiration { get => expiration; set => expiration = value; }
        public ToastTypesTime? NotifyOn { get => notifyOn; set => notifyOn = value; }
        public bool? IsDaily { get => isDaily; set => isDaily = value; }
        public DateTime? CompletedDate { get; set; }
        public DateTime? ReminderTime { get; set; }
        public bool? HasExpiration { get; set; }
        public int? CompletedTasks { get; set; }
        public int? TotalTasks { get; set; }

        #endregion

        #region Funtion Atributes

        public List<string> Errors { get; set; } = new List<string>();

        private bool IsCompleted
        {
            get => (CompletedDate != null);
            set
            {
                if (value && CompletedDate == null) CompletedDate = DateTime.Now;
                else if (!value && CompletedDate != null) CompletedDate = null;
            }
        }

        public string ExpirationDateText
        {
            get
            {
                var language = AppResourcesLisener.Languages;
                string text = "";
                if (HasExpiration == true)
                {
                    var dateText = ExpirationDate?.ToString("dd/MM/yyyy");
                    var timeText = ExpirationDate?.ToString("hh:mm tt");
                    if (IsCompleted)
                    {
                        if (IsDaily == true) text = string.Format(language
                            ["TaskListTaskCellCompletedNormalExpirationText"], timeText);
                        else text = string.Format(language["TaskListTaskCellCompletedDailyExpirationText"], dateText, timeText);
                    }
                    else
                    {
                        if (IsDaily == true)
                        {
                            if (ExpirationDate?.TimeOfDay > DateTime.Now.TimeOfDay)
                                text = string.Format(language["TaskListTaskCellDailyExpirationText"], timeText);
                            if (ExpirationDate?.TimeOfDay < DateTime.Now.TimeOfDay)
                                text = string.Format(language["TaskListTaskCellDailyLateExpirationText"], timeText);
                        }
                        else
                        {
                            if (ExpirationDate?.TimeOfDay > DateTime.Now.TimeOfDay)
                                text = string.Format(language["TaskListTaskCellNormalExpirationText"], dateText, timeText);
                            if (ExpirationDate?.TimeOfDay < DateTime.Now.TimeOfDay)
                                text = string.Format(language["TaskListTaskCellNormalLateExpirationText"], dateText, timeText);
                        }
                    }
                }
                return text;
            }
        }

        public string CompletedDateText
        {
            get
            {
                string text = "";
                var language = AppResourcesLisener.Languages;
                if (IsCompleted)
                {
                    var dateText = CompletedDate?.ToString("dd/MM/yyyy");
                    var timeText = CompletedDate?.ToString("hh:mm tt");
                    if (IsDaily == true)
                        text = string.Format(language["TaskListTaskCellDailyCompletionText"], timeText);
                    else text = string.Format(language["TaskListTaskCellNormalCompletionText"],
                        dateText, timeText);
                }
                return text;
            }
        }

        #endregion

        #region Auxiliary Atributes

        public bool? IsAnimating { get => isAnimating; set => isAnimating = value; }

        public SelectedFor? SelectedReason { get; set; }

        public List<string> NotifyOnDisplayNames
        {
            get
            {
                var language = AppResourcesLisener.Languages;
                return new List<string>
                {
                    language["TaskDetailsNotification1HourBefore"],
                    language["TaskDetailsNotification30MinutesBefore"],
                    language["TaskDetailsNotification15MinutesBefore"],
                    language["TaskDetailsNotificationNone"],
                    language["TaskDetailsNotification15MinutesAfter"],
                    language["TaskDetailsNotification30MinutesAfter"],
                    language["TaskDetailsNotification1HourAfter"]
                };
            }
        }

        public string SelectedNotificationTimeIndex
        {
            get
            {
                return selectedNotificationTimeIndex ?? (selectedNotificationTimeIndex = NotifyOnDisplayNames.ElementAtOrDefault((int)NotifyOn));
            }
            set
            {
                selectedNotificationTimeIndex = value;
                NotifyOn = (ToastTypesTime)NotifyOnDisplayNames.IndexOf(value);
            }
        }

        public bool IsLate
        {
            get
            {
                if (IsDaily == true) return (DateTime.Now.TimeOfDay > ExpirationDate?.TimeOfDay);
                else return (DateTime.Now > ExpirationDate);
            }
        }

        public bool IsUrgent
        {
            get
            {
                if (IsDaily == true) return (DateTime.Now.TimeOfDay > ExpirationDate?.AddHours(-1).TimeOfDay);
                else return (DateTime.Now > ExpirationDate?.AddHours(-1));
            }
        }

        public string Detail
        {
            get => string.Format(AppResourcesLisener.Languages["TaskListTaskCellDeails"],
                    CompletedTasks, TotalTasks);
        }

        public bool IsValid
        {
            get
            {
                var language = AppResourcesLisener.Languages;
                Errors = new List<string>();
                if (IsDaily == true && HasExpiration == false)
                    throw new Exception(language["TaskFormErrorMessageNullExpiration"]);
                if (IsChecked && CompletedDate == null)
                    throw new Exception(language["TaskFormErrorMessageNullCompletion"]);
                if (string.IsNullOrEmpty(Name) || string.IsNullOrWhiteSpace(Name))
                    Errors.Add(language["TaskFormErrorMessageNullName"]);
                if (!string.IsNullOrEmpty(Name) && Name.Length > 500)
                    Errors.Add(language["TaskFormErrorMessageNameTooLong"]);
                return (Errors.Count <= 0);
            }
        }

        public string ErrorMessage
        {
            get
            {
                var errorMessage = "";
                foreach (var message in Errors) errorMessage += message;
                return errorMessage;
            }
        }

        #endregion

        #region Visuals

        public double PendientPercentage
        {
            get
            {
                if (TotalTasks <= 0 || CompletedTasks <= 0) return 1;
                else if (TotalTasks == CompletedTasks) return 0;
                var totalTasks = Convert.ToDouble(TotalTasks);
                var completedTasks = Convert.ToDouble(CompletedTasks);
                return ((totalTasks - completedTasks) / totalTasks) * 10d;
            }
        }

        public double CompletedPercentage
        {
            get
            {
                if (TotalTasks <= 0 || CompletedTasks <= 0) return 0;
                double totalTasks = Convert.ToDouble(TotalTasks);
                var completedTask = Convert.ToDouble(CompletedTasks);
                return (completedTask / totalTasks) * 10d;
            }
        }

        public Color CellBackgroundColor
        {
            get
            {
                var cellColor = AppResourcesLisener.Themes["ContentDP02"];
                if (HasExpiration == true)
                {
                    if (IsCompleted)
                    {
                        if (IsDaily == true && CompletedDate?.TimeOfDay <= Expiration)
                            return Color.FromHex(Config.Current.CompletedTaskColor);
                        else if (CompletedDate <= ExpirationDate)
                            return Color.FromHex(Config.Current.CompletedTaskColor);
                        else return cellColor;
                    }
                    else
                    {
                        if (IsLate) return Color.FromHex(Config.Current.LateTaskColor);
                        else if (IsUrgent) return Color.FromHex(Config.Current.UrgentTaskColor);
                        else return cellColor;
                    }
                }
                else return cellColor;
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Evalua la igualdad de dos listas.
        /// </summary>
        /// <param name="obj">Lista a ser evaluada.</param>
        /// <returns>Retorna verdadero o falso segun el resultado.</returns>
        public override bool Equals(object obj)
        {
            if (!(obj is CheckTaskViewModel value)) return false;

            if (value.Name != this.Name) return false;
            if (value.ToastId != this.ToastId) return false;
            if (value.NotifyOn != this.NotifyOn) return false;
            if (value.Position != this.Position) return false;
            if (value.IsTaskGroup != this.IsTaskGroup) return false;
            if (value.HasExpiration != this.HasExpiration) return false;
            if (value.ExpirationDate != this.ExpirationDate) return false;
            if (value.CompletedDate != this.CompletedDate) return false;
            if (value.ReminderTime != this.ReminderTime) return false;
            if (value.Expiration != this.Expiration) return false;
            if (value.IsChecked != this.IsChecked) return false;
            if (value.IsDaily != this.IsDaily) return false;

            return base.Equals(obj);
        }

        public override int GetHashCode() => base.GetHashCode();

        #endregion

        #region Dispose

        ~CheckTaskViewModel()
        {
            if (isDisposing == false) Dispose(true);
#if DEBUG
            Debug.WriteLine($"Object destroyect: Name: {nameof(CheckTaskViewModel)}, Id: {this.GetHashCode()}].");
#endif
        }

        public void Dispose(bool isDisposing)
        {
            this.isDisposing = isDisposing;
            if (this.isDisposing == false) Dispose(true);
        }

        public void Dispose()
        {
            Id = null;
            Name = null;
            isDaily = null;
            ToastId = null;
            Position = null;
            notifyOn = null;
            TotalTasks = null;
            expiration = null;
            IsTaskGroup = null;
            isAnimating = null;
            ReminderTime = null;
            CompletedDate = null;
            HasExpiration = null;
            CompletedTasks = null;
            SelectedReason = null;
            expirationDate = null;
            selectedNotificationTimeIndex = null;
        }

        #endregion
    }

    [AddINotifyPropertyChangedInterfaceAttribute]
    public class CheckListViewModel : BaseModel, ICardModel, IDisposable
    {
        #region Atributes

        public bool? isAnimating = false;
        public SelectedFor? selectedReason = SelectedFor.Create;
        private bool? isDisposing = false;

        #endregion

        public CheckListViewModel() { }
        public CheckListViewModel(string name, bool isTask = false)
        { Name = name; IsTaskGroup = isTask; }

        #region Base Atributes

        public string Id { get; set; }
        public int? LastId { get; set; }
        public int? Position { get; set; }
        public string OldName { get; set; }
        public bool? HasExpiration { get; set; }
        public string CompletedDateText { get; }
        public string ExpirationDateText { get; }
        public int? CompletedTasks { get; set; }
        public bool? IsTaskGroup { get; set; }
        public int? TotalTasks { get; set; }
        public string Name { get; set; }

        #endregion

        #region Funtion Atributes

        public bool? IsAnimating { get => isAnimating; set => isAnimating = value; }

        public SelectedFor? SelectedReason { get => selectedReason; set => selectedReason = value; }

        public string Detail
        {
            get => string.Format(AppResourcesLisener.Languages["ListOfListTaskCellDeails"],
                    CompletedTasks, TotalTasks);
        }

        public bool NameHasChange { get => (OldName != Name); }

        #endregion

        #region Visuals

        public double PendientPercentage
        {
            get
            {
                if (TotalTasks <= 0 || CompletedTasks <= 0) return 1;
                else if (TotalTasks == CompletedTasks) return 0;
                var totalTasks = Convert.ToDouble(TotalTasks);
                var completedTasks = Convert.ToDouble(CompletedTasks);
                return ((totalTasks - completedTasks) / totalTasks) * 10d;
            }
        }

        public double CompletedPercentage
        {
            get
            {
                if (TotalTasks <= 0 || CompletedTasks <= 0) return 0;
                double totalTasks = Convert.ToDouble(TotalTasks);
                var completedTask = Convert.ToDouble(CompletedTasks);
                return (completedTask / totalTasks) * 10d;
            }
        }

        public Color CellBackgroundColor
        {
            get => AppResourcesLisener.Themes["ContentDP02"];
        }

        #endregion

        #region Dispose

        ~CheckListViewModel()
        {
            if (isDisposing == false) Dispose(true);
#if DEBUG
            Debug.WriteLine($"Object destroyect: Name: {nameof(CheckListViewModel)}, Id: {this.GetHashCode()}].");
#endif
        }

        public void Dispose(bool isDisposing)
        {
            this.isDisposing = isDisposing;
            if (this.isDisposing == false) Dispose(true);
        }

        public void Dispose()
        {
            CompletedTasks = null;
            selectedReason = null;
            HasExpiration = null;
            IsTaskGroup = null;
            isAnimating = null;
            isDisposing = null;
            TotalTasks = null;
            Position = null;
            OldName = null;
            LastId = null;
            Name = null;
            Id = null;
        }

        #endregion
    }
}
