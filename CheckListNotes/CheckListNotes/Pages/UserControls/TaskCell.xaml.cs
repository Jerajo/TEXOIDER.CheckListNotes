using System;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using System.Diagnostics;
using PortableClasses.Enums;
using System.Threading.Tasks;

namespace CheckListNotes.Pages.UserControls
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class TaskCell : ViewCell
    {
        private double x = 0, y = 0;

        public TaskCell()
        {
            InitializeComponent();

            var panGestureReconizer = new PanGestureRecognizer();
            panGestureReconizer.PanUpdated += Animate;

            FrameCell.GestureRecognizers.Clear();
            FrameCell.GestureRecognizers.Add(panGestureReconizer);

            this.SetBinding(IsLockedProperty, "IsAnimating");
            this.SetBinding(SelectedReasonForProperty, "SelectedReason");

            SwitchIsCompleted.Toggled += SwitchToggled;

            Resice(); // Resise cell heght
        }

        #region SETTERS AND GETTERS

        public bool IsLocked
        {
            get => (bool)GetValue(IsLockedProperty);
            set => SetValue(IsLockedProperty, value);
        }

        public static readonly BindableProperty IsLockedProperty = BindableProperty.Create(nameof(IsLocked), typeof(bool), typeof(CheckListCell), false, BindingMode.TwoWay);

        public double SwipeDistance
        {
            get => (double)GetValue(SwipeDistanceProperty);
            set => SetValue(SwipeDistanceProperty, value);
        }

        public static readonly BindableProperty SwipeDistanceProperty = BindableProperty.Create(nameof(SwipeDistance), typeof(double), typeof(CheckListCell), 100d, BindingMode.TwoWay);

        public SelectedFor SelectedReasonFor
        {
            get => (SelectedFor)GetValue(SelectedReasonForProperty);
            set => SetValue(SelectedReasonForProperty, value);
        }

        public static readonly BindableProperty SelectedReasonForProperty = BindableProperty.Create(nameof(SelectedReasonFor), typeof(SelectedFor), typeof(CheckListCell), SelectedFor.Insert, BindingMode.TwoWay, propertyChanged: SelectedReasonPropertyChanged);

        #endregion

        #region Events

        private async void SwitchToggled(object sender, ToggledEventArgs e)
        {
            SwitchIsCompleted.IsEnabled = false;
            SetValue(IsLockedProperty, true);
            this.ForceUpdateSize();
            await Task.Delay(TimeSpan.FromSeconds(1));
            SetValue(IsLockedProperty, false);
            SwitchIsCompleted.IsEnabled = true;
        }

        #endregion

        #region Auxiliary Methods

        private async void Resice()
        {
            await Task.Delay(TimeSpan.FromSeconds(1));
            ForceUpdateSize();
        }

        private async void Animate(object sender, PanUpdatedEventArgs e)
        {
            if (e.StatusType == GestureStatus.Running)
            {
                FrameCell.TranslationX = x = e.TotalX;

                if (x > 0 && x < SwipeDistance)
                {
                    GridEdit.TranslationX = (x - SwipeDistance);
                    GridEdit.Opacity = (x / SwipeDistance);
                }
                if (x < 0 && x > -SwipeDistance)
                {
                    GridDelete.TranslationX = (x + SwipeDistance);
                    GridDelete.Opacity = (x / -SwipeDistance);
                }
            }
            else if (e.StatusType == GestureStatus.Completed)
            {
                // cach the tab event and shose an action
                IsEnabled = !(IsLocked = true);
                if (x > 0 && x > SwipeDistance) // Update
                {
                    SelectedReasonFor = SelectedFor.Update;
                    await FrameCell.TranslateTo(FrameCell.Width, y);
                }
                else if (x < 0 && x < -SwipeDistance) // Delete
                {
                    SelectedReasonFor = SelectedFor.Delete;
                    await FrameCell.TranslateTo(-FrameCell.Width, y);
                }
                else
                {
                    ResetAnimations();
                    await Task.Delay(TimeSpan.FromMilliseconds(250));
                }
                IsEnabled = !(IsLocked = false);
            }
        }

        public void ResetAnimations()
        {
            Parallel.Invoke(
                async () => await FrameCell.TranslateTo(0, y),
                async () =>
                {
                    if (x > 0) await GridEdit.TranslateTo(-SwipeDistance, y);
                    else await GridDelete.TranslateTo(SwipeDistance, y);
                },
                async () =>
                {
                    if (x > 0) await GridEdit.FadeTo(0);
                    else await GridDelete.FadeTo(0);
                }
            );
            x = 0;
        }

        private static void SelectedReasonPropertyChanged(BindableObject bindable, object oldValue, object newValue)
        {
            var reasonFor = (SelectedFor)newValue;
            var binging = bindable as TaskCell;
            if (reasonFor == SelectedFor.Create) binging.ResetAnimations();
        }

        #endregion

        #region Dispose

        ~TaskCell()
        {
#if DEBUG
            Debug.WriteLine($"Object destroyect: Name: {nameof(TaskCell)}, Id: {this.GetHashCode()}].");
#endif
        }

        #endregion
    }
}