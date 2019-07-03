using System;
using System.Linq;
using Xamarin.Forms;
using System.Diagnostics;
using Xamarin.Forms.Xaml;
using System.Collections;
using System.Windows.Input;
using CheckListNotes.Models;
using System.Threading.Tasks;
using System.Collections.Specialized;
using PortableClasses.Implementations;
using PortableClasses.Enums;
using TouchTracking.Forms;
using TouchTracking;
using System.ComponentModel;
using PortableClasses.Extensions;
using System.Collections.Generic;

namespace CheckListNotes.Pages.UserControls
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    class ViewBoadCardList : ScrollView
    {
        #region Atributes

        double x, y;
        int position;
        CardView card;
        bool? isDisposing;
        StackLayout Container;
        TouchEffect touchEffet;
        SelectedFor? selectedFor;
        Point distance, mousePoint, controlPoint;
        TapGestureRecognizer tapGestureRecognizer;
        View backgroundContent, previousContent, nextContent;

        #endregion

        public ViewBoadCardList() : base() => InitializeComponents();

        #region SETTERS AND GETTERS

        public int ContainerCount
        {
            get => Container?.Children.Count ?? -1;
        }

        #endregion

        #region Bindable Properties

        public FulyObservableCollection<CheckTaskViewModel> ItemsSource
        {
            get => (FulyObservableCollection<CheckTaskViewModel>)GetValue(ItemsSourceProperty);
            set => SetValue(ItemsSourceProperty, value);
        }

        public static readonly BindableProperty ItemsSourceProperty =
            BindableProperty.Create(nameof(ItemsSource),
            typeof(FulyObservableCollection<CheckTaskViewModel>),
            typeof(ViewBoadCardList), null, BindingMode.TwoWay,
            propertyChanged: ItemsSourcePropertyChanged);

        public CheckTaskViewModel SelectedItem
        {
            get => (CheckTaskViewModel)GetValue(SelectedItemProperty);
            set => SetValue(SelectedItemProperty, value);
        }

        public static readonly BindableProperty SelectedItemProperty =
            BindableProperty.Create(nameof(SelectedItem),
            typeof(CheckTaskViewModel), typeof(ViewBoadCardList), null, BindingMode.OneWayToSource);

        public double SwipeDistance
        {
            get => (double)GetValue(SwipeDistanceProperty);
            set => SetValue(SwipeDistanceProperty, value);
        }

        public static readonly BindableProperty SwipeDistanceProperty = BindableProperty.Create(nameof(SwipeDistance), typeof(double), typeof(CheckListCell), 100d, BindingMode.TwoWay);

        #endregion

        #region Bindable Commands

        public ICommand UpdatedListCommand
        {
            get { return (ICommand)GetValue(UpdatedListCommandProperty); }
            set { SetValue(UpdatedListCommandProperty, value); }
        }

        public static readonly BindableProperty UpdatedListCommandProperty =
            BindableProperty.Create(nameof(UpdatedListCommand), typeof(ICommand), typeof(ViewBoadCardList));

        public ICommand UpdatedOrDeleteItemCommand
        {
            get { return (ICommand)GetValue(UpdatedOrDeleteItemCommandProperty); }
            set { SetValue(UpdatedOrDeleteItemCommandProperty, value); }
        }


        public static readonly BindableProperty UpdatedOrDeleteItemCommandProperty =
            BindableProperty.Create(nameof(UpdatedOrDeleteItemCommand), typeof(ICommand), 
            typeof(ViewBoadCardList));

        #endregion

        #region Events

        #region Static

        private static void ItemsSourcePropertyChanged(BindableObject bindable, object oldValue, object newValue)
        {
            if (!(bindable is ViewBoadCardList control)) return;
            if (newValue is FulyObservableCollection<CheckTaskViewModel> newList)
            {
                newList.CollectionChanged += control.OnCollectionChanged;
                newList.ItemPropertyChanged += control.OnSelectedReasonPropertyChanged;
            }
            else if (oldValue is FulyObservableCollection<CheckTaskViewModel> oldList)
            {
                oldList.CollectionChanged -= control.OnCollectionChanged;
                oldList.ItemPropertyChanged -= control.OnSelectedReasonPropertyChanged;
                GC.Collect();
            }
        }

        #endregion

        #region Virtual

        protected virtual void OnItemSelected(object sender, EventArgs e)
        {
            if (!(sender is CardView control)) return;
            if (control.BindingContext.Equals(SelectedItem)) return;
            else
            {
                Device.BeginInvokeOnMainThread(async () =>
                { // ContentSecundary
                    control.ShadowColor = (Color)App.Current.Resources["ContentPrimary"];
                    await control.ScaleTo(.98, 300, Easing.SpringOut);
                    SelectedItem = control.BindingContext as CheckTaskViewModel;
                });
            }
        }

        protected virtual async void OnItemMoved(object sender, TouchActionEventArgs e)
        {
            if (isDisposing == true) return;
            if (!e.IsInContact && selectedFor == null) return;
            if (card == null && GetControlByPosition(e) == null) { ClearValues(); return; }
            if (!(card.BindingContext is CheckTaskViewModel item)) { ClearValues();  return; }
            if (selectedFor == null && !await SelectFor(item, e)) return;
            if (selectedFor == SelectedFor.Insert) await VerticalAnimation(item, e);
            else if (selectedFor != null) await HorizontalAnimation(item, e);
        }

        protected virtual void OnSelectedReasonPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (isDisposing == true || !(sender is CheckTaskViewModel item)) return;
            if (e.PropertyName == "SelectedReason" && item.SelectedReason == SelectedFor.Create)
                ResetAnimations();
        }

        protected virtual void OnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (sender is null) return;
            int? lastIndex = ItemsSource.Count - 1;
            int? ContainerCount = Container?.Children.Count;
            IList newItems = e.NewItems, oldItems = e.OldItems;
            int? newCount = e.NewItems?.Count, oldCount = e.OldItems?.Count;
            int newIndex = e.NewStartingIndex, oldIndex = e.OldStartingIndex;
            int? lastNewIndex = (newCount + newIndex)-1, lastOldIndex = (oldCount + oldIndex)-1;
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    if (newItems == null || newCount <= 0) return;
                    if (newCount > 1)
                    {
                        if (ContainerCount == 0) AddItems(newItems);
                        else
                        {
                            AddItems(newItems, newIndex);
                            if (newIndex < lastIndex) UpdateAfectedItems(lastNewIndex);
                        }
                    }
                    else
                    {
                        if (ContainerCount == 0) AddItem(newItems[0] as CheckTaskViewModel);
                        else
                        {
                            AddItem(newItems[0] as CheckTaskViewModel, newIndex);
                            if (newIndex < lastIndex) UpdateAfectedItems(newIndex);
                        }
                    }
                    break;
                case NotifyCollectionChangedAction.Remove:
                    if (oldItems == null || oldCount <= 0) return;
                    if (oldCount > 1)
                    {
                        if (oldCount == ContainerCount) RemoveItems(0);
                        else
                        {
                            RemoveItems(oldIndex, lastOldIndex);
                            if (lastOldIndex < lastIndex) UpdateAfectedItems(lastOldIndex);
                        }
                    }
                    else
                    {
                        RemoveItem(oldIndex);
                        if (oldIndex < lastIndex) UpdateAfectedItems(oldIndex);
                    }
                    break;
            }
            void UpdateAfectedItems(int? value = null)
            {
                if (e.Action == NotifyCollectionChangedAction.Add || 
                    e.Action == NotifyCollectionChangedAction.Remove 
                    || newCount > 1 || oldCount > 1) UpdateItems(value);
                else UpdateItem(ItemsSource[value.Value]);
                UpdatedListCommand?.Execute(null);
            }
        }

        #endregion

        #endregion

        #region Methods

        #region InitializeComponents

        private void InitializeComponents()
        {
            isDisposing = false;
            tapGestureRecognizer = new TapGestureRecognizer();
            tapGestureRecognizer.Tapped += OnItemSelected;
            touchEffet = new TouchEffect();
            touchEffet.TouchAction += OnItemMoved;

            Container = new StackLayout
            {
                VerticalOptions = LayoutOptions.FillAndExpand,
                Padding = new Thickness(0, 10, 0, 100),
                Spacing = 20
            };
            Container.Effects.Add(touchEffet);

            Content = Container;
        }

        #endregion

        #region Items Animations

        private Task VerticalAnimation(CheckTaskViewModel item, TouchActionEventArgs e)
        {
            if (isDisposing == true) return Task.CompletedTask;
            if (e.Type == TouchActionType.Moved && e.IsInContact)
            {
                var bounds = card.Bounds;
                y = e.Location.Y - (bounds.Y + bounds.Height / 2);
                card.TranslationY = y;
                var z = card.Y;
                distance.Y = Math.Abs(bounds.Y - Math.Abs(y));

                if (previousContent != null && previousContent.Y > distance.Y)
                {
                    MoveItem(previousContent, position);
                    nextContent = previousContent;
                    item.Position = --position;
                    MoveItem(backgroundContent, position);
                    previousContent = position > 0 ?
                        Container.Children.ElementAt(position - 1) : null;
                    UpdatedListCommand?.Execute(null);
                }
                else if (nextContent != null && nextContent.Y < distance.Y)
                {
                    MoveItem(previousContent, position);
                    previousContent = nextContent;
                    item.Position = ++position;
                    MoveItem(backgroundContent, position);
                    nextContent = position < ContainerCount - 2 ?
                        Container.Children.ElementAt(position + 1) : null;
                    UpdatedListCommand?.Execute(null);
                }
            }
            else if (isDisposing == false)
            {
                ResetAnimations();
                item.IsAnimating = false;
            }
            return Task.CompletedTask;
        }

        private async Task HorizontalAnimation(CheckTaskViewModel item, TouchActionEventArgs e)
        {
            if (isDisposing == true) return;
            if (e.Type == TouchActionType.Moved && e.IsInContact)
            {
                x = e.Location.X - (mousePoint.X - controlPoint.X);
                card.MaterialContainer.TranslationX = x;
                distance.X = Math.Abs(card.Bounds.X - Math.Abs(x));

                if (x > 0 && x < SwipeDistance)
                {
                    ((Grid)backgroundContent).Children.ElementAt(0).IsVisible = true;
                    ((Grid)backgroundContent).Children.ElementAt(1).IsVisible = false;
                    backgroundContent.BackgroundColor = AppResourcesLisener.Themes["Good"];
                    backgroundContent.TranslationX = (x - SwipeDistance);
                    backgroundContent.Opacity = (x / SwipeDistance);
                }
                if (x < 0 && x > -SwipeDistance)
                {
                    ((Grid)backgroundContent).Children.ElementAt(0).IsVisible = false;
                    ((Grid)backgroundContent).Children.ElementAt(1).IsVisible = true;
                    backgroundContent.BackgroundColor = AppResourcesLisener.Themes["Error"];
                    backgroundContent.TranslationX = (x + SwipeDistance);
                    backgroundContent.Opacity = (x / -SwipeDistance);
                }
            }
            else if (isDisposing == false)
            {
                isDisposing = true;
                if (x > 0 && x > SwipeDistance && item.SelectedReason != SelectedFor.Update)
                {
                    item.SelectedReason = SelectedFor.Update;
                    UpdatedOrDeleteItemCommand.Execute(item);
                    await card.MaterialContainer.TranslateTo(card.Width, y);
                }
                else if (x < 0 && x < -SwipeDistance && item.SelectedReason != SelectedFor.Delete)
                {
                    item.SelectedReason = SelectedFor.Delete;
                    UpdatedOrDeleteItemCommand.Execute(item);
                    await card.MaterialContainer.TranslateTo(-card.Width, y);
                }
                else
                {
                    ResetAnimations();
                    await Task.Delay(TimeSpan.FromMilliseconds(250));
                    item.IsAnimating = false;
                }
                isDisposing = false;
            }
        }

        public async void ResetAnimations()
        {
            if (card == null || backgroundContent == null) return;
            isDisposing = true;
            if (selectedFor == SelectedFor.Insert)
            {
                card.TranslationY = 0;
                Container.Children.Remove(backgroundContent);
                MoveItem(card, position);
                var count = ContainerCount;
            }
            else
            {
                Parallel.Invoke(
                    async () => await card.MaterialContainer.TranslateTo(0, 0),
                    async () =>
                    {
                        if (x > 0) await backgroundContent.TranslateTo(-SwipeDistance, 0);
                        else await backgroundContent.TranslateTo(SwipeDistance, 0);
                    },
                    async () => await backgroundContent.FadeTo(0)
                );
                await Task.Delay(250);
                if (backgroundContent == null) card?.InsertOrRemoveContent(backgroundContent);
            }
            ClearValues();
            isDisposing = false;
        }

        #endregion

        #region Public

        public void ClearValues()
        {
            backgroundContent = null;
            previousContent = null;
            nextContent = null;
            selectedFor = null;
            card = null;
            x = y = 0;
            GC.Collect();
        }

        #endregion

        #region Protedted

        protected virtual void AddItems(IList items, int? startIndex = null)
        {
            if (Container?.Children == null) return;
            foreach (var item in items)
            {
                if (startIndex != null) AddItem(item as CheckTaskViewModel, startIndex++);
                else AddItem(item as CheckTaskViewModel);
            }
        }

        protected virtual void AddItem(CheckTaskViewModel item, int? index = null)
        {
            Device.BeginInvokeOnMainThread(() =>
            {
                if (this.Container?.Children == null || item == null) return;
                var control = CreateCard(item);

                if (index is int i) this.Container.Children.Insert(i, control);
                else this.Container.Children.Add(control);

                item.Position = this.Container.Children.IndexOf(control);
            });
        }

        protected virtual void RemoveItems(int startIndex, int? endIndex = null)
        {
            if (Container?.Children == null) return;
            var containerLastIndex = Container.Children.Count - 1;
            var index = endIndex ?? containerLastIndex;
            var lastIndex = index <= containerLastIndex ? index : containerLastIndex;
            for (var i = lastIndex; i >= startIndex; i--) RemoveItem(i);
        }

        protected virtual void RemoveItem(int index)
        {
            Device.BeginInvokeOnMainThread(() =>
            {
                if (this.Container?.Children.Count <= 0) return;
                this.Container.Children[index].BindingContext = null;
                this.Container.Children.RemoveAt(index);
                GC.Collect();
            });
        }

        protected virtual void MoveItems(List<View> items, int startIndex)
        {
            if (Container?.Children == null) return;
            foreach (var item in items) MoveItem(item, startIndex++);
        }

        protected virtual void MoveItem(View item, int index)
        {
            if (item == null) return;
            if (this.Container?.Children.Contains(item) == false) return;
            this.Container.Children.Remove(item);
            this.Container.Children.Insert(index, item);
            if (item is CardView control && control.BindingContext is CheckTaskViewModel model) model.Position = index;
        }

        #endregion

        #region Auxiliary Methods

        private CardView CreateCard(CheckTaskViewModel item)
        {
            if (item == null) return null;
            var control = new CardView();
            control.BindingContext = item;
            control.GestureRecognizers.Clear();
            control.GestureRecognizers.Add(tapGestureRecognizer);
            return control;
        }

        private CardView GetControlByPosition(TouchActionEventArgs e)
        {
            mousePoint = new Point(e.Location.X, e.Location.Y);
            foreach (var item in Container.Children)
            {
                if (!(item is CardView control)) continue;
                if (control.Y < mousePoint.Y && control.Y + control.Height > mousePoint.Y)
                {
                    controlPoint = new Point(control.X, control.Y);
                    distance = new Point(control.X - mousePoint.X, control.Y - mousePoint.Y);
                    card = control;
                    card.BindingContext = control.BindingContext;
                    return card;
                }
            }
            return null;
        }

        private Task<bool> SelectFor(CheckTaskViewModel item, TouchActionEventArgs e)
        {
            var diferenceX = mousePoint.X - e.Location.X;
            var diferenceY = mousePoint.Y - e.Location.Y;
            if (diferenceX == diferenceY) return Task.FromResult(false);

            position = item.Position.Value;
            if (Math.Abs(diferenceX) > Math.Abs(diferenceY))
                selectedFor = (diferenceX > 0) ? SelectedFor.Update : SelectedFor.Delete;
            else selectedFor = SelectedFor.Insert;

            backgroundContent = CreateContent();
            if (selectedFor != SelectedFor.Insert)
                card.InsertOrRemoveContent(backgroundContent);
            else if (!Container.Children.Contains(backgroundContent))
            {
                previousContent = position > 0 ?
                    Container.Children.ElementAt(position - 1) : null;
                nextContent = position < ContainerCount - 1 ?
                    Container.Children.ElementAt(position + 1) : null;
                Container.Children.Insert(position, backgroundContent);
                Container.RaiseChild(card);
            }

            item.IsAnimating = true; return Task.FromResult(true);
        }

        private View CreateContent()
        {
            switch (selectedFor)
            {
                case SelectedFor.Insert:
                    return new Frame
                    {
                        Margin = new Thickness(10,0),
                        Padding = 10,
                        CornerRadius = 5,
                        BorderColor = AppResourcesLisener.Themes["FontColor"],
                        HasShadow = false,
                        Content = new Frame
                        {
                            Margin = 0,
                            CornerRadius = 5,
                            BorderColor = AppResourcesLisener.Themes["FontColor"],
                            HasShadow = false,
                        }
                    };
                case SelectedFor.Update:
                case SelectedFor.Delete:
                    var grid = new Grid();
                    var label = new Label
                    {
                        Margin = 10,
                        VerticalOptions = LayoutOptions.CenterAndExpand,
                        HorizontalOptions = LayoutOptions.Start,
                        Text = AppResourcesLisener.Languages["ButtonEditText"],
                        TextColor = AppResourcesLisener.Themes["FontColorInverse"],
                        FontAttributes = FontAttributes.Bold
                    };
                    grid.Children.Add(label);
                    var label2 = new Label
                    {
                        Margin = 10,
                        VerticalOptions = LayoutOptions.CenterAndExpand,
                        HorizontalOptions = LayoutOptions.End,
                        Text = AppResourcesLisener.Languages["ButtonDeleteText"],
                        TextColor = AppResourcesLisener.Themes["FontColorInverse"],
                        FontAttributes = FontAttributes.Bold
                    };
                    grid.Children.Add(label2); return grid;
                default: return null;
            }
        }

        private void UpdateItems(int? startIndex = null)
        {
            if (ItemsSource == null || ItemsSource.Count <= 0) return;
            var index = startIndex ?? 0;
            for (var i = index; i < ItemsSource.Count; i++) UpdateItem(ItemsSource[i]);
        }

        private void UpdateItem(CheckTaskViewModel item)
        {
            if (ItemsSource == null || ItemsSource.Count <= 0) return;
            var position = ItemsSource.IndexOf(item);
            if (position != -1) item.Position = position;
            else AddItem(item);
        }

        #endregion

        #endregion

        #region Dispose

        ~ViewBoadCardList()
        {
            isDisposing = true;
            tapGestureRecognizer = null;
            touchEffet = null;
            if (Container != null)
            {
                Container.Children.Clear();
                Container = null;
            }
#if DEBUG
            Debug.WriteLine($"Object destroyect: Name: {nameof(ViewBoadCardList)}, Id: {this.GetHashCode()}].");
#endif
            isDisposing = null;
            GC.Collect();
        }

        #endregion
    }
}
