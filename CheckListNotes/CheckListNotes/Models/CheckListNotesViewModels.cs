using System;
using System.Linq;
using Xamarin.Forms;
using PropertyChanged;
using PortableClasses.Enums;
using System.ComponentModel;
using System.Collections.Generic;

namespace CheckListNotes.Models
{
    [AddINotifyPropertyChangedInterfaceAttribute]
    public class CheckTaskViewModel : BaseModel
    {
        #region Atributes

        public bool isAnimating = false;
        internal ToastTypesTime notifyOn;
        private string selectedNotificationTimeIndex;
        internal DateTime? expirationDate;
        internal TimeSpan? expiration;
        internal bool isDaily;

        #endregion

        #region Base Atributes

        public int Id { get; set; }
        public string Name { get; set; }
        public int LastSubId { get; set; }
        public string ToastId { get; set; }
        public bool IsTaskGroup { get; set; }
        public bool IsChecked { get => IsCompleted; set => IsCompleted = value; }
        public DateTime? ExpirationDate { get => expirationDate; set => expirationDate = value; }
        public TimeSpan? Expiration { get => expiration; set => expiration = value; }
        public ToastTypesTime NotifyOn { get => notifyOn; set => notifyOn = value; }
        public bool IsDaily { get => isDaily; set => isDaily = value; }
        public List<CheckTaskModel> SubTasks { get; set; }
        public DateTime? CompletedDate { get; set; }
        public DateTime? ReminderTime { get; set; }
        public bool HasExpiration { get; set; }

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

        #region Methods

        /// <summary>
        /// Evalua la igualdad de dos listas.
        /// </summary>
        /// <param name="obj">Lista a ser evaluada.</param>
        /// <returns>Retorna verdadero o falso segun el resultado.</returns>
        public override bool Equals(object obj)
        {
            if (obj == null || GetType() != obj.GetType()) return false;

            var value = obj as CheckTaskViewModel;

            if (value.Name != this.Name) return false;
            if (value.ToastId != this.ToastId) return false;
            if (value.NotifyOn != this.NotifyOn) return false;
            if (value.LastSubId != this.LastSubId) return false;
            if (value.IsTaskGroup != this.IsTaskGroup) return false;
            if (value.ExpirationDate != this.ExpirationDate) return false;
            if (value.CompletedDate != this.CompletedDate) return false;
            if (value.ReminderTime != this.ReminderTime) return false;
            if (value.Expiration != this.Expiration) return false;
            if (value.IsChecked != this.IsChecked) return false;
            if (value.IsDaily != this.IsDaily) return false;

            return true;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
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
