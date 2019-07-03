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
        #region Atributes

        double x = 0, y = 0;
        bool? isDisposing;

        #endregion

        public TaskCell() : base() { InitializeComponent(); Init(); }

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
            IsLocked = true;
            await Task.Delay(300);
            IsLocked = false;
            SwitchIsCompleted.IsEnabled = true;
        }

        private static void SelectedReasonPropertyChanged(BindableObject bindable, object oldValue, object newValue)
        {
            if (!(bindable is TaskCell control)) return;
            var reasonFor = (SelectedFor)newValue;
            if (reasonFor == SelectedFor.Create) control.ResetAnimations();
        }

        protected virtual async void OnPadUpdated(object sender, PanUpdatedEventArgs e) =>
            await Animate(e);

        #endregion

        #region Auxiliary Methods

        #region Initialize Comonets

        private void Init()
        {
            isDisposing = false;
            var panGestureReconizer = new PanGestureRecognizer();
            panGestureReconizer.PanUpdated += OnPadUpdated;

            FrameCell.GestureRecognizers.Clear();
            FrameCell.GestureRecognizers.Add(panGestureReconizer);

            this.SetBinding(IsLockedProperty, "IsAnimating");
            this.SetBinding(SelectedReasonForProperty, "SelectedReason");

            SwitchIsCompleted.Toggled += SwitchToggled;
        }

        #endregion

        #region Animation

        private async Task Animate(PanUpdatedEventArgs e)
        {
            if (isDisposing == true) return;
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

        #endregion

        #endregion

        #region Dispose

        ~TaskCell()
        {
            isDisposing = false;
#if DEBUG
            Debug.WriteLine($"Object destroyect: Name: {nameof(TaskCell)}, Id: {this.GetHashCode()}].");
#endif
            isDisposing = null;
        }

        #endregion
    }
}