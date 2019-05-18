using System;
using System.Linq;
using Xamarin.Forms;
using PropertyChanged;
using PortableClasses.Enums;
using PortableClasses.Extensions;
using System.Collections.Generic;

namespace CheckListNotes.Models
{
    [AddINotifyPropertyChangedInterfaceAttribute]
    public class CheckTaskVieModel : BaseModel
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
        public string ToastId { get; set; }
        public DateTime? ReminderTime { get; set; }
        public DateTime? CompletedDate { get; set; }
        public bool IsChecked { get; set; }
        public bool IsDaily { get; set; }
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

        public bool IsCompleted
        {
            get => (CompletedDate != null);
            set
            {
                if (value) CompletedDate = DateTime.Now;
                else CompletedDate = null;
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

        public Color TaskBackgroundColor
        {
            get
            {
                if (HasExpiration)
                {
                    if (IsCompleted)
                    {
                        if (CompletedDate <= ExpirationDate) return Color.DarkGreen;
                        else return Color.FromHex("#444");
                    }
                    else
                    {
                        if (IsLate) return Color.DarkRed;
                        else if (IsUrgent) return Color.DarkGoldenrod;
                        else return Color.FromHex("#444");
                    }
                }
                else return Color.FromHex("#444");
            }
        }

        public Color LavelFontColor
        {
            get => Color.FromHex(Config.Current.AppTheme.LavelFontColor);
        }

        public Color EditiorFontColor
        {
            get => Color.FromHex(Config.Current.AppTheme.EditiorFontColor);
        }

        public Color EditorBackgroundColor
        {
            get => Color.FromHex(Config.Current.AppTheme.EditorBackgroundColor);
        }

        public Color EditorBorderColor
        {
            get => Color.FromHex(Config.Current.AppTheme.EditorBorderColor);
        }

        public Color ViewBoxColor
        {
            get => Color.FromHex(Config.Current.AppTheme.ViewBoxColor);
        }

        #endregion
    }

    [AddINotifyPropertyChangedInterfaceAttribute]
    public class CheckListViewModel : BaseModel
    {
        public bool isAnimating = false;
        public SelectedFor selectedReason = SelectedFor.Create;

        #region Base Atributes

        public int LastId { get; set; }
        public string Name { get; set; }
        public string OldName { get; set; }
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
                else return 0;
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
    }
}
