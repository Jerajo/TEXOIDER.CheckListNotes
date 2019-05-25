using System;
using System.Linq;
using Xamarin.Forms;
using PropertyChanged;
using PortableClasses.Enums;
using System.ComponentModel;
using PortableClasses.Extensions;
using System.Collections.Generic;
using PortableClasses.Implementations;

namespace CheckListNotes.Models
{
    [AddINotifyPropertyChangedInterfaceAttribute]
    public class CheckTaskViewModel : BaseModel
    {
        #region Atributes

        private TimeSpan? expiration;
        private DateTime? expirationDate;
        public bool isAnimating = false;
        private string selectedNotificationTimeIndex;
        private ToastTypesTime notifyOn;

        #endregion

        #region Base Atributes

        public int Id { get; set; }
        public string Name { get; set; }
        public int LastSubId { get; set; }
        public string ToastId { get; set; }
        public DateTime? ReminderTime { get; set; }
        public DateTime? CompletedDate { get; set; }
        public List<CheckTaskModel> SubTasks { get; set; }
        public bool IsChecked { get => IsCompleted; set => IsCompleted = value; }
        public bool IsDaily { get; set; }
        public bool IsTaskGroup
        {
            get => (SubTasks != null);
            set
            {
                if (value && SubTasks == null)
                    SubTasks = new List<CheckTaskModel>();
                else if (!value) SubTasks = null;
            }
        }
        public DateTime? ExpirationDate
        {
            get => expirationDate;
            set
            {
                if (value != null && expiration != null) expirationDate = value?.ChangeTime(expiration.Value);
                else expirationDate = value;
            }
        }
        public TimeSpan? Expiration
        {
            get => expiration;
            set
            {
                expiration = value;
                if (expiration == null) ExpirationDate = null;
                else if (ExpirationDate == null) ExpirationDate = DateTime.Now;
                else ExpirationDate = expirationDate?.Date;
            }
        }
        public ToastTypesTime NotifyOn
        {
            get => (ReminderTime != null) ? notifyOn : ToastTypesTime.None;
            set
            {
                notifyOn = value;
                switch (notifyOn)
                {
                    case ToastTypesTime.AHourBefore:
                        ReminderTime = ExpirationDate?.AddHours(-1);
                        break;
                    case ToastTypesTime.HalfHourBefore:
                        ReminderTime = ExpirationDate?.AddMinutes(-30);
                        break;
                    case ToastTypesTime.FifteenMinutesBefore:
                        ReminderTime = ExpirationDate?.AddMinutes(-15);
                        break;
                    case ToastTypesTime.FifteenMinutesAfter:
                        ReminderTime = ExpirationDate?.AddMinutes(15);
                        break;
                    case ToastTypesTime.HalfHourAfter:
                        ReminderTime = ExpirationDate?.AddHours(1);
                        break;
                    case ToastTypesTime.AHourAfter:
                        ReminderTime = ExpirationDate?.AddMinutes(30);
                        break;
                    case ToastTypesTime.None:
                    default:
                        ReminderTime = null;
                        break;
                }
            }
        }

        #endregion

        #region Funtion Atributes

        public List<string> Errors { get; set; } = new List<string>();

        public bool HasExpiration
        {
            get => (ExpirationDate != null);
            set
            {
                if (value && expiration == null) Expiration = DateTime.Now.TimeOfDay;
                else if (!value && expiration != null)
                {
                    NotifyOn = ToastTypesTime.None;
                    Expiration = null;
                    IsDaily = false;
                }
            }
        }

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
                if (HasExpiration)
                {
                    if (IsCompleted)
                    {
                        if (IsDaily) return $"Expira mañana a las: {ExpirationDate?.ToString("hh:mm tt")}";
                        else return $"Fecha de expiración: {ExpirationDate?.ToString("dd/MM/yyyy")} a las {ExpirationDate?.ToString("hh:mm tt")}";
                    }
                    else
                    {
                        if (IsDaily)
                        {
                            switch (ExpirationDate)
                            {
                                case var time when ExpirationDate?.TimeOfDay > DateTime.Now.TimeOfDay:
                                    return $"Expira hoy a las: {ExpirationDate?.ToString("hh:mm tt")}";
                                case var time when ExpirationDate?.TimeOfDay < DateTime.Now.TimeOfDay:
                                    return $"Expiró hoy a las: {ExpirationDate?.ToString("hh:mm tt")}";
                                default:
                                    return "Error t.t";
                            }
                        }
                        else
                        {
                            switch (ExpirationDate)
                            {
                                case var time when ExpirationDate > DateTime.Now:
                                    return $"Expira el: {ExpirationDate?.ToString("dd/MM/yyyy")} a las {ExpirationDate?.ToString("hh:mm tt")}";
                                case var time when ExpirationDate < DateTime.Now:
                                    return $"Expiró el: {ExpirationDate?.ToString("dd/MM/yyyy")} a las {ExpirationDate?.ToString("hh:mm tt")}";
                                default:
                                    return "Error t.t";
                            }
                        }
                    }
                }
                else return "";
            }
        }

        public string CompletedDateText
        {
            get
            {
                if (IsCompleted)
                {
                    if (IsDaily) return $"Completado hoy a las: {CompletedDate?.ToString("hh:mm tt")}";
                    else return $"Completado el: {CompletedDate?.ToString("dd/MM/yyyy")} a las {CompletedDate?.ToString("hh:mm tt")}";
                }
                else return "";
            }
        }

        #endregion

        #region Auxiliary Atributes

        public bool IsAnimating 
        {
            get => isAnimating;
            set => isAnimating = value;
        }

        public SelectedFor SelectedReason { get; set; }

        public List<string> NotifyOnDisplayNames
        {
            get
            {
                return new List<string>
                {
                    "Una hora antes",
                    "Media hora antes",
                    "Quince minutos antes",
                    "Ninguna",
                    "Quince minutos despues",
                    "Media hora despues",
                    "Una hora despues"
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
                if (IsDaily) return (DateTime.Now.TimeOfDay > ExpirationDate?.TimeOfDay);
                else return (DateTime.Now > ExpirationDate);
            }
        }

        public bool IsUrgent
        {
            get
            {
                if (IsDaily) return (DateTime.Now.TimeOfDay > ExpirationDate?.AddHours(-1).TimeOfDay);
                else return (DateTime.Now > ExpirationDate?.AddHours(-1));
            }
        }

        public int PendientTasks
        {
            get
            {
                if (SubTasks != null)
                    return SubTasks.Where(m => !m.IsChecked).ToList().Count;
                else return 0;
            }
        }

        public double PendientPercentage
        {
            get
            {
                if (SubTasks != null)
                {
                    if (SubTasks.Count <= 0) return 1;
                    var pendientTask = Convert.ToDouble(PendientTasks);
                    var completedTask = Convert.ToDouble(CompletedTasks);
                    var allTask = pendientTask + completedTask;
                    return (pendientTask / allTask) * 10f;
                }
                else return 1;
            }
        }

        public int CompletedTasks
        {
            get
            {
                if (SubTasks != null)
                    return SubTasks.Where(m => m.IsChecked).ToList().Count;
                else return 0;
            }
        }

        public double CompletedPercentage
        {
            get
            {
                if (SubTasks != null)
                {
                    if (SubTasks.Count <= 0) return 0;
                    var pendientTask = Convert.ToDouble(PendientTasks);
                    var completedTask = Convert.ToDouble(CompletedTasks);
                    var allTask = pendientTask + completedTask;
                    return (completedTask / allTask) * 10f;
                }
                else return 0;
            }
        }

        public string Detail
        {
            get
            {
                return $"Tareas finalizadas: {CompletedTasks} | Tareas pendientes: {PendientTasks}";
            }
        }

        public bool IsValid
        {
            get
            {
                // Reinicia la lista de errores
                Errors = new List<string>();

                // Verifica los atributos
                if (string.IsNullOrEmpty(Name) || string.IsNullOrWhiteSpace(Name)) Errors.Add("Debe ingresar el nombre de la tarea.\n");
                if (IsDaily && !HasExpiration) Errors.Add("Debe seleccionar la hora de expiración.\n");
                if (IsChecked && !IsCompleted) Errors.Add("Error fecha de completado no encontrada.\n");
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

        #region Theme

        public Color AppFontColor
        {
            get => Color.FromHex(Config.Current.AppTheme?.AppFontColor);
        }

        public Color CellBackgroundColor
        {
            get
            {
                if (HasExpiration)
                {
                    if (IsCompleted)
                    {
                        if (IsDaily && CompletedDate?.TimeOfDay <= Expiration)
                            return Color.FromHex(Config.Current.CompletedTaskColor);
                        else if (CompletedDate <= ExpirationDate)
                            return Color.FromHex(Config.Current.CompletedTaskColor);
                        else return Color.FromHex(Config.Current.AppTheme?.CellBackgroundColor);
                    }
                    else
                    {
                        if (IsLate) return Color.FromHex(Config.Current.LateTaskColor);
                        else if (IsUrgent) return Color.FromHex(Config.Current.UrgentTaskColor);
                        else return Color.FromHex(Config.Current.AppTheme?.CellBackgroundColor);
                    }
                }
                else return Color.FromHex(Config.Current.AppTheme?.CellBackgroundColor);
            }
        }

        public Color CellBorderColor
        {
            get => Color.FromHex(Config.Current.AppTheme?.CellBorderColor);
        }

        public Color EditorBackgroundColor
        {
            get => Color.FromHex(Config.Current.AppTheme?.EditorBackgroundColor);
        }

        public Color EditorBorderColor
        {
            get => Color.FromHex(Config.Current.AppTheme?.EditorBorderColor);
        }

        public Color ViewBoxColor
        {
            get => Color.FromHex(Config.Current.AppTheme?.ViewBoxColor);
        }

        public Color CompletedPercentageColor //TODO:
        {
            get => Color.FromHex("#0F0");
        }

        public Color PendientPercentageColor //TODO:
        {
            get => Color.FromHex("#888");
        }

        #endregion
    }

    [AddINotifyPropertyChangedInterfaceAttribute]
    public class CheckListViewModel : BaseModel
    {
        public bool isAnimating = false;
        public SelectedFor selectedReason = SelectedFor.Create;

        public CheckListViewModel()
        {
            Config.Current.PropertyChanged += ConfigChanged;
        }

        #region Base Atributes

        public int LastId { get; set; }
        public bool IsTask { get; set; }
        public string Name { get; set; }
        public string OldName { get; set; }
        public int SelectedTabIndex { get; set; }
        public List<CheckTaskModel> CheckListTasks { get; set; }

        #endregion

        #region Funtion Atributes

        public bool IsAnimating
        {
            get => isAnimating;
            set => isAnimating = value;
        }

        public SelectedFor SelectedReason
        {
            get => selectedReason;
            set => selectedReason = value;
        }

        public int CompletedTasks
        {
            get
            {
                if (CheckListTasks != null)
                    return CheckListTasks.Where(m => m.IsChecked).ToList().Count;
                else return 0;
            }
        }

        public int PendientTasks
        {
            get
            {
                if (CheckListTasks != null)
                    return CheckListTasks.Where(m => !m.IsChecked).ToList().Count;
                else return 0;
            }
        }

        public string Detail
        {
            get
            {
                return $"Tareas finalizadas: {CompletedTasks} | Tareas pendientes: {PendientTasks}";
            }
        }

        public double PendientPercentage
        {
            get
            {
                if (CheckListTasks != null)
                {
                    if (CheckListTasks.Count <= 0) return 1;
                    var pendientTask = Convert.ToDouble(PendientTasks);
                    var completedTask = Convert.ToDouble(CompletedTasks);
                    var allTask = pendientTask + completedTask;
                    return (pendientTask / allTask) * 10f;
                }
                else return 1;
            }
        }

        public double CompletedPercentage
        {
            get
            { 
                if (CheckListTasks != null)
                {
                    if (CheckListTasks.Count <= 0) return 0;
                    var pendientTask = Convert.ToDouble(PendientTasks);
                    var completedTask = Convert.ToDouble(CompletedTasks);
                    var allTask = pendientTask + completedTask;
                    return (completedTask / allTask) * 10f;
                }
                else return 0;
            }
        }

        public bool NameHasChange
        {
            get => (OldName != Name);
        }

        #endregion

        #region Theme

        public Color AppFontColor
        {
            get => Color.FromHex(Config.Current.AppTheme?.AppFontColor ?? "#FF000000");
        }

        public Color CellBackgroundColor
        {
            get => Color.FromHex(Config.Current.AppTheme?.CellBackgroundColor ?? "#FF000000");
        }

        public Color CellBorderColor
        {
            get => Color.FromHex(Config.Current.AppTheme?.CellBorderColor ?? "#FF000000");
        }

        public Color CompletedPercentageColor //TODO:
        {
            get => Color.FromHex("#0F0");
        }

        public Color PendientPercentageColor //TODO:
        {
            get => Color.FromHex("#888");
        }

        #endregion

        #region Methods

        private void ConfigChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName != nameof(Config.Current.AppTheme)) return;
            OnPropertyChanged(nameof(AppFontColor));
            OnPropertyChanged(nameof(CellBackgroundColor));
            OnPropertyChanged(nameof(CellBorderColor));
            OnPropertyChanged(nameof(CompletedPercentageColor));
            OnPropertyChanged(nameof(PendientPercentageColor));
        }

        ~CheckListViewModel()
        {
            Config.Current.PropertyChanged -= ConfigChanged;
        }

        #endregion
    }
}
