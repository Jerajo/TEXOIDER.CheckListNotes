using System;
using System.Linq;
using Xamarin.Forms;
using Xam.Plugin.TabView;
using System.ComponentModel;
using System.Collections.Generic;
using Xam.Plugin.TabView.Converters;
using System.Collections.ObjectModel;
using CarouselView.FormsPlugin.Abstractions;

namespace CheckListNotes.Pages.UserControls
{
    public class TabViewControl : ContentView
    {
        #region Atributes

        StackLayout _mainContainerSL;
        Grid _headerContainerGrid;
        CarouselViewControl _carouselView;
        bool _supressCarouselViewPositionChangedEvent;
        public event PositionChangingEventHandler PositionChanging;
        public event PositionChangedEventHandler PositionChanged;

        #endregion

        //Parameterless constructor required for xaml instantiation.
        public TabViewControl() => Init();

        public TabViewControl(IList<TabItem> tabItems, int selectedTabIndex = 0)
        {
            Init();
            foreach (var tab in tabItems) ItemSource.Add(tab);
            if (selectedTabIndex > 0) SelectedTabIndex = selectedTabIndex;
        }

        #region Bindalbe Properties

        public Color HeaderBackgroundColor
        {
            get => (Color)GetValue(HeaderBackgroundColorProperty);
            set => SetValue(HeaderBackgroundColorProperty, value);
        }

        public static readonly BindableProperty HeaderBackgroundColorProperty = BindableProperty.Create(nameof(HeaderBackgroundColor), typeof(Color), typeof(TabViewControl), Color.SkyBlue, BindingMode.Default, null, HeaderBackgroundColorChanged);

        public Color HeaderTabTextColor
        {
            get => (Color)GetValue(HeaderTabTextColorProperty);
            set => SetValue(HeaderTabTextColorProperty, value);
        }

        public static readonly BindableProperty HeaderTabTextColorProperty =
            BindableProperty.Create(nameof(HeaderTabTextColor), typeof(Color), typeof(TabViewControl), TabDefaults.DefaultColor, BindingMode.OneWay, null, HeaderTabTextColorChanged);

        public double ContentHeight
        {
            get => (double)GetValue(ContentHeightProperty);
            set => SetValue(ContentHeightProperty, value);
        }

        public static readonly BindableProperty ContentHeightProperty = BindableProperty.Create(nameof(ContentHeight), typeof(double), typeof(TabViewControl), (double)200, BindingMode.Default, null, ContentHeightChanged);

        public Color HeaderSelectionUnderlineColor
        {
            get => (Color)GetValue(HeaderSelectionUnderlineColorProperty);
            set => SetValue(HeaderSelectionUnderlineColorProperty, value);
        }

        public static readonly BindableProperty HeaderSelectionUnderlineColorProperty = BindableProperty.Create(nameof(HeaderSelectionUnderlineColor), typeof(Color), typeof(TabViewControl), Color.White, BindingMode.Default, null, HeaderSelectionUnderlineColorChanged);

        public double HeaderSelectionUnderlineThickness
        {
            get => (double)GetValue(HeaderSelectionUnderlineThicknessProperty);
            set => SetValue(HeaderSelectionUnderlineThicknessProperty, value);
        }

        public static readonly BindableProperty HeaderSelectionUnderlineThicknessProperty = BindableProperty.Create(nameof(HeaderSelectionUnderlineThickness), typeof(double), typeof(TabViewControl), TabDefaults.DefaultThickness, BindingMode.Default, null, HeaderSelectionUnderlineThicknessChanged);

        public double HeaderSelectionUnderlineWidth
        {
            get => (double)GetValue(HeaderSelectionUnderlineWidthProperty);
            set => SetValue(HeaderSelectionUnderlineWidthProperty, value);
        }

        public static readonly BindableProperty HeaderSelectionUnderlineWidthProperty = BindableProperty.Create(nameof(HeaderSelectionUnderlineWidth), typeof(double), typeof(TabViewControl), (double)0, BindingMode.Default, null, HeaderSelectionUnderlineWidthChanged);

        [Xamarin.Forms.TypeConverter(typeof(FontSizeConverter))]
        public double HeaderTabTextFontSize
        {
            get => (double)GetValue(HeaderTabTextFontSizeProperty);
            set => SetValue(HeaderTabTextFontSizeProperty, value);
        }

        public static readonly BindableProperty HeaderTabTextFontSizeProperty = BindableProperty.Create(nameof(HeaderTabTextFontSize), typeof(double), typeof(TabViewControl), (double)14, BindingMode.Default, null, HeaderTabTextFontSizeChanged);

        public string HeaderTabTextFontFamily
        {
            get => (string)GetValue(HeaderTabTextFontFamilyProperty);
            set => SetValue(HeaderTabTextFontFamilyProperty, value);
        }
        public static readonly BindableProperty HeaderTabTextFontFamilyProperty = BindableProperty.Create(nameof(HeaderTabTextFontFamily), typeof(string), typeof(TabViewControl), null, BindingMode.Default, null, HeaderTabTextFontFamilyChanged);

        public FontAttributes HeaderTabTextFontAttributes
        {
            get => (FontAttributes)GetValue(HeaderTabTextFontAttributesProperty);
            set => SetValue(HeaderTabTextFontAttributesProperty, value);
        }

        public static readonly BindableProperty HeaderTabTextFontAttributesProperty = BindableProperty.Create(nameof(HeaderTabTextFontAttributes), typeof(FontAttributes), typeof(TabViewControl), FontAttributes.None, BindingMode.Default, null, HeaderTabTextFontAttributesChanged);

        public ObservableCollection<TabItem> ItemSource
        {
            get => (ObservableCollection<TabItem>)GetValue(ItemSourceProperty);
            set => SetValue(ItemSourceProperty, value);
        }

        public static readonly BindableProperty ItemSourceProperty = BindableProperty.Create(nameof(ItemSource), typeof(ObservableCollection<TabItem>), typeof(TabViewControl));

        public GridLength TabSizeOption
        {
            get => (GridLength)GetValue(TabSizeOptionProperty);
            set => SetValue(TabSizeOptionProperty, value);
        }

        public static readonly BindableProperty TabSizeOptionProperty = BindableProperty.Create(nameof(TabSizeOption), typeof(GridLength), typeof(TabViewControl), default(GridLength), propertyChanged: OnTabSizeOptionChanged);

        public int SelectedTabIndex
        {
            get => (int)GetValue(SelectedTabIndexProperty);
            set => SetValue(SelectedTabIndexProperty, value);
        }

        public static readonly BindableProperty SelectedTabIndexProperty = BindableProperty.Create(nameof(SelectedTabIndex), typeof(int), typeof(TabViewControl), 0, propertyChanged: OnSelectedTabIndexChanged);

        #endregion

        #region Events

        #region Core Events
        void ItemSource_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems != null)
            {
                foreach (var tab in e.NewItems)
                {
                    if (tab is TabItem newTab)
                    {
                        SetTabProps(newTab);
                        AddTabToView(newTab);
                    }
                }
            }
        }

        private void _carouselView_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(_carouselView.Position) && !_supressCarouselViewPositionChangedEvent)
            {
                var positionChangingArgs = new PositionChangingEventArgs()
                {
                    Canceled = false,
                    NewPosition = _carouselView.Position,
                    OldPosition = SelectedTabIndex
                };

                OnPositionChanging(ref positionChangingArgs);

                if (positionChangingArgs != null && positionChangingArgs.Canceled)
                {
                    _supressCarouselViewPositionChangedEvent = true;
                    _carouselView.PositionSelected -= _carouselView_PositionSelected;
                    _carouselView.PropertyChanged -= _carouselView_PropertyChanged;
                    _carouselView.Position = SelectedTabIndex;
                    _carouselView.PositionSelected += _carouselView_PositionSelected;
                    _carouselView.PropertyChanged += _carouselView_PropertyChanged;
                    _supressCarouselViewPositionChangedEvent = false;
                }
            }
        }

        protected override void OnBindingContextChanged()
        {
            base.OnBindingContextChanged();
            if (BindingContext != null)
            {
                foreach (var tab in ItemSource)
                {
                    if (tab is TabItem view)
                    {
                        view.Content.BindingContext = BindingContext;
                    }
                }
            }
        }
        #endregion

        #region HeaderBackgroundColor
        private static void HeaderBackgroundColorChanged(BindableObject bindable, object oldValue, object newValue)
        {
            if (bindable is TabViewControl tabViewControl)
            {
                tabViewControl._headerContainerGrid.BackgroundColor = (Color)newValue;
            }
        }
        #endregion

        #region HeaderTabTextColor
        private static void HeaderTabTextColorChanged(BindableObject bindable, object oldValue, object newValue)
        {
            if (bindable is TabViewControl tabViewControl && tabViewControl.ItemSource != null)
            {
                foreach (var tab in tabViewControl.ItemSource)
                {
                    tab.HeaderTextColor = (Color)newValue;
                }
            }
        }
        #endregion

        #region ContentHeight
        private static void ContentHeightChanged(BindableObject bindable, object oldValue, object newValue)
        {
            if (bindable is TabViewControl tabViewControl && tabViewControl._carouselView != null)
            {
                tabViewControl._carouselView.HeightRequest = (double)newValue;
            }
        }
        #endregion

        #region HeaderSelectionUnderlineColor
        private static void HeaderSelectionUnderlineColorChanged(BindableObject bindable, object oldValue, object newValue)
        {
            if (bindable is TabViewControl tabViewControl && tabViewControl.ItemSource != null)
            {
                foreach (var tab in tabViewControl.ItemSource)
                {
                    tab.HeaderSelectionUnderlineColor = (Color)newValue;
                }
            }
        }
        #endregion

        #region HeaderSelectionUnderlineThickness
        private static void HeaderSelectionUnderlineThicknessChanged(BindableObject bindable, object oldValue, object newValue)
        {
            if (bindable is TabViewControl tabViewControl && tabViewControl.ItemSource != null)
            {
                foreach (var tab in tabViewControl.ItemSource)
                {
                    tab.HeaderSelectionUnderlineThickness = (double)newValue;
                }
            }
        }
        #endregion

        #region HeaderSelectionUnderlineWidth
        private static void HeaderSelectionUnderlineWidthChanged(BindableObject bindable, object oldValue, object newValue)
        {
            if (bindable is TabViewControl tabViewControl && tabViewControl.ItemSource != null)
            {
                foreach (var tab in tabViewControl.ItemSource)
                {
                    tab.HeaderSelectionUnderlineWidth = (double)newValue;
                }
            }
        }
        #endregion

        #region HeaderTabTextFontSize
        private static void HeaderTabTextFontSizeChanged(BindableObject bindable, object oldValue, object newValue)
        {
            if (bindable is TabViewControl tabViewControl && tabViewControl.ItemSource != null)
            {
                foreach (var tab in tabViewControl.ItemSource)
                {
                    tab.HeaderTabTextFontSize = (double)newValue;
                }
            }
        }
        #endregion

        #region HeaderTabTextFontFamily
        private static void HeaderTabTextFontFamilyChanged(BindableObject bindable, object oldValue, object newValue)
        {
            if (bindable is TabViewControl tabViewControl && tabViewControl.ItemSource != null)
            {
                foreach (var tab in tabViewControl.ItemSource)
                {
                    tab.HeaderTabTextFontFamily = (string)newValue;
                }
            }
        }
        #endregion

        #region HeaderTabTextFontAttributes
        private static void HeaderTabTextFontAttributesChanged(BindableObject bindable, object oldValue, object newValue)
        {
            if (bindable is TabViewControl tabViewControl && tabViewControl.ItemSource != null)
            {
                foreach (var tab in tabViewControl.ItemSource)
                {
                    tab.HeaderTabTextFontAttributes = (FontAttributes)newValue;
                }
            }
        }
        #endregion

        #region TabSizeOption
        private static void OnTabSizeOptionChanged(BindableObject bindable, object oldValue, object newValue)
        {
            if (bindable is TabViewControl tabViewControl && tabViewControl._headerContainerGrid != null && tabViewControl.ItemSource != null)
            {
                foreach (var tabContainer in tabViewControl._headerContainerGrid.ColumnDefinitions)
                {
                    tabContainer.Width = (GridLength)newValue;
                }
            }
        }
        #endregion

        #region SelectedTabIndex
        private static void OnSelectedTabIndexChanged(BindableObject bindable, object oldValue, object newValue)
        {
            if (bindable is TabViewControl tabViewControl && tabViewControl.ItemSource != null
                && tabViewControl._carouselView.Position != (int)newValue)
            {
                tabViewControl.SetPosition((int)newValue);
            }
        }
        protected virtual void OnPositionChanging(ref PositionChangingEventArgs e)
        {
            if (e.NewPosition < 0 || e.NewPosition >= ItemSource.Count) e.Canceled = true;
            else if (_carouselView.Position == e.NewPosition && 
                SelectedTabIndex == e.NewPosition) e.Canceled = true;
            else PositionChanging?.Invoke(this, e);
        }
            
        protected virtual void OnPositionChanged(PositionChangedEventArgs e) =>
            PositionChanged?.Invoke(this, e);
        private void _carouselView_PositionSelected(object sender, PositionSelectedEventArgs e)
        {
            if (_carouselView.Position != e.NewValue || SelectedTabIndex != e.NewValue)
                SetPosition(e.NewValue);
        }
        #endregion

        #endregion

        #region Methods

        #region Initialize Components

        private void Init()
        {
            ItemSource = new ObservableCollection<TabItem>();
            _headerContainerGrid = new MaterialGrid
            {
                HorizontalOptions = LayoutOptions.FillAndExpand,
                VerticalOptions = LayoutOptions.Start,
                BackgroundColor = HeaderBackgroundColor,
                CornerRadius = new CornerRadius(0),
                ShadowSize = new Thickness(0,0,0,10),
                MinimumHeightRequest = 50
            };

            _carouselView = new CarouselViewControl
            {
                HorizontalOptions = LayoutOptions.FillAndExpand,
                VerticalOptions = LayoutOptions.FillAndExpand,
                HeightRequest = ContentHeight,
                ShowArrows = false,
                ShowIndicators = false,
                BindingContext = this
            };

            _mainContainerSL = new StackLayout
            {
                HorizontalOptions = LayoutOptions.FillAndExpand,
                VerticalOptions = LayoutOptions.FillAndExpand,
                Children = { _headerContainerGrid, _carouselView },
                Spacing = 0
            };

            Content = _mainContainerSL;
            ItemSource.CollectionChanged += ItemSource_CollectionChanged;
            SetPosition(SelectedTabIndex, true);
        }

        #endregion

        #region Initialize tabs

        private void SetTabProps(TabItem tab)
        {
            //Set the tab properties from the main control only if not set in xaml at the individual tab level.
            if (tab.HeaderTextColor == TabDefaults.DefaultColor && HeaderTabTextColor != TabDefaults.DefaultColor)
                tab.HeaderTextColor = HeaderTabTextColor;
            if (tab.HeaderSelectionUnderlineColor == TabDefaults.DefaultColor && HeaderSelectionUnderlineColor != TabDefaults.DefaultColor)
                tab.HeaderSelectionUnderlineColor = HeaderSelectionUnderlineColor;
            if (tab.HeaderSelectionUnderlineThickness.Equals(TabDefaults.DefaultThickness) && !HeaderSelectionUnderlineThickness.Equals(TabDefaults.DefaultThickness))
                tab.HeaderSelectionUnderlineThickness = HeaderSelectionUnderlineThickness;
            if (tab.HeaderSelectionUnderlineWidth > 0)
                tab.HeaderSelectionUnderlineWidth = HeaderSelectionUnderlineWidth;
            if (tab.HeaderTabTextFontSize.Equals(TabDefaults.DefaultTextSize) && !HeaderTabTextFontSize.Equals(TabDefaults.DefaultTextSize))
                tab.HeaderTabTextFontSize = HeaderTabTextFontSize;
            if (tab.HeaderTabTextFontFamily is null && !string.IsNullOrWhiteSpace(HeaderTabTextFontFamily))
                tab.HeaderTabTextFontFamily = HeaderTabTextFontFamily;
            if (tab.HeaderTabTextFontAttributes == FontAttributes.None && HeaderTabTextFontAttributes != FontAttributes.None)
                tab.HeaderTabTextFontAttributes = HeaderTabTextFontAttributes;
        }

        private void AddTabToView(TabItem tab)
        {
            var tabSize = (TabSizeOption.IsAbsolute && TabSizeOption.Value.Equals(0)) ? new GridLength(1, GridUnitType.Star) : TabSizeOption;

            _headerContainerGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = tabSize });

            tab.IsCurrent = _headerContainerGrid.ColumnDefinitions.Count - 1 == SelectedTabIndex;

            var headerIcon = new Image
            {
                Margin = new Thickness(0, 10, 0, 0),
                BindingContext = tab,
                HorizontalOptions = LayoutOptions.CenterAndExpand,
                VerticalOptions = LayoutOptions.Center,
                WidthRequest = tab.HeaderIconSize,
                HeightRequest = tab.HeaderIconSize
            };
            headerIcon.SetBinding(Image.SourceProperty, nameof(TabItem.HeaderIcon));
            headerIcon.SetBinding(IsVisibleProperty, nameof(TabItem.HeaderIcon), converter: new NullToBoolConverter());

            var headerLabel = new Label
            {
                Margin = new Thickness(5, headerIcon.IsVisible ? 0 : 10, 5, 0),
                BindingContext = tab,
                VerticalTextAlignment = TextAlignment.Start,
                HorizontalTextAlignment = TextAlignment.Center,
                HorizontalOptions = LayoutOptions.CenterAndExpand,
                VerticalOptions = LayoutOptions.Center
            };
            headerLabel.SetBinding(Label.TextProperty, nameof(TabItem.HeaderText));
            headerLabel.SetBinding(Label.TextColorProperty, nameof(TabItem.HeaderTextColor));
            headerLabel.SetBinding(Label.FontSizeProperty, nameof(TabItem.HeaderTabTextFontSize));
            headerLabel.SetBinding(Label.FontFamilyProperty, nameof(TabItem.HeaderTabTextFontFamily));
            headerLabel.SetBinding(Label.FontAttributesProperty, nameof(TabItem.HeaderTabTextFontAttributes));
            headerLabel.SetBinding(IsVisibleProperty, nameof(TabItem.HeaderText), converter: new NullToBoolConverter());

            var selectionBarBoxView = new BoxView
            {
                VerticalOptions = LayoutOptions.EndAndExpand,
                BindingContext = tab,
                HeightRequest = HeaderSelectionUnderlineThickness,
                WidthRequest = HeaderSelectionUnderlineWidth
            };
            selectionBarBoxView.SetBinding(IsVisibleProperty, nameof(TabItem.IsCurrent));
            selectionBarBoxView.SetBinding(BoxView.ColorProperty, nameof(TabItem.HeaderSelectionUnderlineColor));
            selectionBarBoxView.SetBinding(WidthRequestProperty, nameof(TabItem.HeaderSelectionUnderlineWidth));
            selectionBarBoxView.SetBinding(HeightRequestProperty, nameof(TabItem.HeaderSelectionUnderlineThickness));
            selectionBarBoxView.SetBinding(HorizontalOptionsProperty,
                                           nameof(TabItem.HeaderSelectionUnderlineWidthProperty),
                                           converter: new DoubleToLayoutOptionsConverter());

            selectionBarBoxView.PropertyChanged += (object sender, PropertyChangedEventArgs e) =>
            {
                if (e.PropertyName == nameof(TabItem.IsCurrent))
                {
                    SetPosition(ItemSource.IndexOf((TabItem)((BoxView)sender).BindingContext));
                }
                if (e.PropertyName == nameof(WidthRequest))
                {
                    selectionBarBoxView.HorizontalOptions = tab.HeaderSelectionUnderlineWidth > 0 ? LayoutOptions.CenterAndExpand : LayoutOptions.FillAndExpand;
                }
            };

            var headerItemSL = new StackLayout
            {
                HorizontalOptions = LayoutOptions.Fill,
                VerticalOptions = LayoutOptions.FillAndExpand,
                Children = { headerIcon, headerLabel, selectionBarBoxView }
            };
            var tapRecognizer = new TapGestureRecognizer();
            tapRecognizer.Tapped += (object s, EventArgs e) =>
            {
                _supressCarouselViewPositionChangedEvent = true;
                var capturedIndex = _headerContainerGrid.Children.IndexOf((View)s);
                SetPosition(capturedIndex);
                _supressCarouselViewPositionChangedEvent = false;
            };
            headerItemSL.GestureRecognizers.Add(tapRecognizer);
            _headerContainerGrid.Children.Add(headerItemSL, _headerContainerGrid.ColumnDefinitions.Count - 1, 0);
            _carouselView.ItemsSource = ItemSource.Select(t => t.Content);
        }

        #endregion

        #region Add and Remove Tabs

        public void AddTab(TabItem tab, int position = -1, bool selectNewPosition = false)
        {
            if (position > -1)
            {
                ItemSource.Insert(position, tab);
            }
            else
            {
                ItemSource.Add(tab);
            }
            if (selectNewPosition)
            {
                SelectedTabIndex = position;
            }
        }

        public void RemoveTab(int position = -1)
        {
            if (position > -1)
            {
                ItemSource.RemoveAt(position);
                _headerContainerGrid.Children.RemoveAt(position);
                _headerContainerGrid.ColumnDefinitions.RemoveAt(position);

            }
            else
            {
                ItemSource.Remove(ItemSource.Last());
                _headerContainerGrid.Children.RemoveAt(_headerContainerGrid.Children.Count - 1);
                _headerContainerGrid.ColumnDefinitions.Remove(_headerContainerGrid.ColumnDefinitions.Last());
            }
            _carouselView.ItemsSource = ItemSource.Select(t => t.Content);
            SelectedTabIndex = position >= 0 && position < ItemSource.Count ? position : ItemSource.Count - 1;
        }

        #endregion

        #region Update Tab Positions

        public void SetPosition(int position, bool initialRun = false)
        {
            if (SelectedTabIndex == position && !initialRun) return;

            int oldPosition = SelectedTabIndex;

            var positionChangingArgs = new PositionChangingEventArgs()
            {
                Canceled = false,
                NewPosition = position,
                OldPosition = oldPosition
            };
            OnPositionChanging(ref positionChangingArgs);

            if (positionChangingArgs != null && positionChangingArgs.Canceled && 
                !initialRun) return;

            if ((position >= 0 && position < ItemSource.Count) || initialRun)
            {
                if (_carouselView.Position != position || initialRun)
                {
                    _carouselView.PositionSelected -= _carouselView_PositionSelected;
                    _carouselView.Position = position;
                    _carouselView.PositionSelected += _carouselView_PositionSelected;
                }
                if (oldPosition != position)
                {
                    if (oldPosition < ItemSource.Count)
                    {
                        ItemSource[oldPosition].IsCurrent = false;
                    }
                    ItemSource[position].IsCurrent = true;
                    SelectedTabIndex = position;
                }
            }

            var positionChangedArgs = new PositionChangedEventArgs()
            {
                NewPosition = SelectedTabIndex,
                OldPosition = oldPosition
            };
            OnPositionChanged(positionChangedArgs);
        }

        public void SelectNext() => SetPosition(SelectedTabIndex + 1);

        public void SelectPrevious() => SetPosition(SelectedTabIndex - 1);

        public void SelectFirst() => SetPosition(0);

        public void SelectLast() => SetPosition(ItemSource.Count - 1);

        #endregion

        #endregion
    }

    #region Auxiliary Classes

    public static class TabDefaults
    {
        public static readonly Color DefaultColor = Color.White;
        public const double DefaultThickness = 5;
        public const double DefaultTextSize = 14;
    }

    #endregion
}
