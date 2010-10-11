using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Media;
using Clarity.Phone.Extensions;
using Microsoft.Phone.Controls;
using System.Collections;
using System.Diagnostics;
using System.Collections.ObjectModel;

namespace Clarity.Phone.Controls
{
    public class QuickJumpGrid : ListBox
    {
        /// <summary>
        /// Stores a reference to the current Popup.
        /// </summary>
        private Popup _popup;

        /// <summary>
        /// Stores a reference to the current overlay.
        /// </summary>
        private Panel _overlayPanel;

        private QuickJumpGridOverlay _overlay;

        /// <summary>
        /// Stores a reference to the current root visual.
        /// </summary>
        private FrameworkElement _rootVisual;

        private PhoneApplicationPage _page;

        private Dictionary<string, int> _quickJumpLookup;

        private ItemsControlHelper _itemsControlHelper;

        public bool IsOverlayOpen { get; set; }


        public event RoutedEventHandler OverlayOpened;

        public event RoutedEventHandler OverlayClosed;

        public QuickJumpGrid()
        {
            base.DefaultStyleKey = typeof(QuickJumpGrid);

            _itemsControlHelper = new ItemsControlHelper(this);
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
        }

        protected override bool IsItemItsOwnContainerOverride(object item)
        {
            return item is QuickJumpGridListItem;
        }

        protected override DependencyObject GetContainerForItemOverride()
        {
            var item = new QuickJumpGridListItem(this);
            if (this.ItemContainerStyle != null)
            {
                item.Style = this.ItemContainerStyle;
            }
            return item;
        }

        internal bool OnHeaderClicked()
        {
            OpenPopup();
            return true;
        }

        /// <summary>
        /// Initialize the _rootVisual property (if possible and not already done).
        /// </summary>
        private void InitializeRootVisual()
        {
            if (null == _rootVisual)
            {
                // Try to capture the Application's RootVisual
                _rootVisual = Application.Current.RootVisual as FrameworkElement;

                if (null == _page)
                    _page = _rootVisual.GetVisualDescendants().OfType<PhoneApplicationPage>().First();
            }
        }

        private void HandleRootVisualSizeChanged(object sender, SizeChangedEventArgs e)
        {
            UpdateOverlayPlacement();
        }

        private void UpdateOverlayPlacement()
        {
            if ((null != _rootVisual) && (null != _overlayPanel))
            {
                // Size the overlay to match the new container
                _overlayPanel.Width = _rootVisual.ActualWidth;
                _overlayPanel.Height = _rootVisual.ActualHeight;
            }
        }

        /// <summary>
        /// Opens the Popup.
        /// </summary>
        /// <param name="position">Position to place the Popup.</param>
        private void OpenPopup()
        {

            if (IsOverlayOpen)
                return;
            else
                IsOverlayOpen = true;

            if (null != OverlayOpened)
                OverlayOpened(this, null);

            InitializeRootVisual();

            _overlayPanel = new Grid { Background = OverlayBackground };

            UpdateOverlayPlacement();

            if (_overlay == null)
            {
                _overlay = new QuickJumpGridOverlay(this);
                _overlay.ItemsSource = _quickJumpLookup;
                _overlay.TileSelected += new QuickJumpGridOverlay.TileSelectedEventHandler(OnOverlayTileSelected);
            }

            _overlayPanel.Children.Add(_overlay);

            _popup = new Popup { Child = _overlayPanel };

            if (null != _rootVisual)
            {
                _rootVisual.SizeChanged += new SizeChangedEventHandler(HandleRootVisualSizeChanged);
            }

            if (null != _page)
            {
                _page.BackKeyPress += new EventHandler<System.ComponentModel.CancelEventArgs>(OnBackKeyPress);
            }

            _popup.InvokeOnLayoutUpdated(() =>
                {
                    _overlay.Show();
                });
            _popup.IsOpen = true;
        }

        void OnBackKeyPress(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (_popup.IsOpen)
            {
                e.Cancel = true;
                ClosePopup(string.Empty);
            }
        }

        void OnOverlayTileSelected(object sender, string key)
        {
            ClosePopup(key);
        }

        /// <summary>
        /// Closes the Popup.
        /// </summary>
        private void ClosePopup(string key)
        {
            if (null != OverlayClosed)
                OverlayClosed(this, null);

            if (null != _page)
            {
                _page.BackKeyPress -= new EventHandler<System.ComponentModel.CancelEventArgs>(OnBackKeyPress);
            }

            if (null != _popup)
            {
                _popup.IsOpen = false;
                _popup.Child = null;
                _popup = null;
            }

            if (null != _overlayPanel)
            {
                _overlayPanel.Children.Clear();
                _overlayPanel = null;
            }

            if (_overlay != null)
            {
                _overlay.TileSelected -= new QuickJumpGridOverlay.TileSelectedEventHandler(OnOverlayTileSelected);
                _overlay = null;
            }

            if (null != _rootVisual)
            {
                _rootVisual.SizeChanged -= new SizeChangedEventHandler(HandleRootVisualSizeChanged);
            }

            Dispatcher.BeginInvoke(() =>
                {
                    try
                    {
                        if (!String.IsNullOrEmpty(key))
                        {
                            _itemsControlHelper.ScrollHost.ScrollToVerticalOffset(this.Items.IndexOf(key));
                        }
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine("ScrollToVerticalOffset Exception: {0}", ex);
                    }
                });

            IsOverlayOpen = false;
        }

        //TODO: get form datasource
        public Dictionary<string, int> Tiles { get; set; }

        protected override void PrepareContainerForItemOverride(DependencyObject element, object item)
        {
            base.PrepareContainerForItemOverride(element, item);

            var listItem = element as QuickJumpGridListItem;
            if (listItem != null)
            {
                if (item is String)
                {
                    listItem.ContentTemplate = this.GroupHeaderTemplate;
                    listItem.IsHeader = true;
                }
                else
                    listItem.IsHeader = false;
            }
        }

        public static readonly DependencyProperty GroupHeaderTemplateProperty = DependencyProperty.Register("GroupHeaderTemplate", typeof(DataTemplate), typeof(QuickJumpGrid), new PropertyMetadata(null));
        public DataTemplate GroupHeaderTemplate
        {
            get
            {
                return (GetValue(GroupHeaderTemplateProperty) as DataTemplate);
            }
            set
            {
                SetValue(GroupHeaderTemplateProperty, value);
            }
        }

        public static readonly DependencyProperty OverlayBackgroundProperty = DependencyProperty.Register("OverlayBackground", typeof(Brush), typeof(QuickJumpGrid), new PropertyMetadata(null));
        public Brush OverlayBackground
        {
            get
            {
                return (GetValue(OverlayBackgroundProperty) as Brush);
            }
            set
            {
                SetValue(OverlayBackgroundProperty, value);
            }
        }

        public static readonly DependencyProperty IsAlphaNumericProperty = DependencyProperty.Register("IsAlphaNumeric", typeof(bool), typeof(QuickJumpGrid), new PropertyMetadata(true));
        public bool IsAlphaNumeric
        {
            get
            {
                return (bool)GetValue(IsAlphaNumericProperty);
            }
            set
            {
                SetValue(IsAlphaNumericProperty, value);
            }
        }

        public static readonly DependencyProperty OverlayTileBackgroundProperty = DependencyProperty.Register("OverlayTileBackground", typeof(Brush), typeof(QuickJumpGrid), new PropertyMetadata(null));
        public Brush OverlayTileBackground
        {
            get
            {
                return (GetValue(OverlayTileBackgroundProperty) as Brush);
            }
            set
            {
                SetValue(OverlayTileBackgroundProperty, value);
            }
        }

        public static readonly DependencyProperty OverlayTileForegroundProperty = DependencyProperty.Register("OverlayTileForeground", typeof(Brush), typeof(QuickJumpGrid), new PropertyMetadata(null));
        public Brush OverlayTileForeground
        {
            get
            {
                return (GetValue(OverlayTileForegroundProperty) as Brush);
            }
            set
            {
                SetValue(OverlayTileForegroundProperty, value);
            }
        }

        public static readonly DependencyProperty OverlayTileDisabledBackgroundProperty = DependencyProperty.Register("OverlayTileDisabledBackground", typeof(Brush), typeof(QuickJumpGrid), new PropertyMetadata(null));
        public Brush OverlayTileDisabledBackground
        {
            get
            {
                return (GetValue(OverlayTileDisabledBackgroundProperty) as Brush);
            }
            set
            {
                SetValue(OverlayTileDisabledBackgroundProperty, value);
            }
        }

        public static readonly DependencyProperty OverlayTileDisabledForegroundProperty = DependencyProperty.Register("OverlayTileDisabledForeground", typeof(Brush), typeof(QuickJumpGrid), new PropertyMetadata(null));
        public Brush OverlayTileDisabledForeground
        {
            get
            {
                return (GetValue(OverlayTileDisabledForegroundProperty) as Brush);
            }
            set
            {
                SetValue(OverlayTileDisabledForegroundProperty, value);
            }
        }

        public static readonly DependencyProperty QuickJumpGridSelectorProperty = DependencyProperty.Register("QuickJumpGridSelector", typeof(IQuickJumpGridSelector), typeof(QuickJumpGrid), new PropertyMetadata(null));
        public IQuickJumpGridSelector QuickJumpGridSelector
        {
            get
            {
                return (GetValue(QuickJumpGridSelectorProperty) as IQuickJumpGridSelector);
            }
            set
            {
                SetValue(QuickJumpGridSelectorProperty, value);
            }
        }

        public static readonly DependencyProperty DataSourceProperty = DependencyProperty.Register("DataSource", typeof(object), typeof(QuickJumpGrid), new PropertyMetadata(null, OnDataSourceChanged));
        public object DataSource
        {
            get
            {
                return (GetValue(DataSourceProperty) as object);
            }
            set
            {
                SetValue(DataSourceProperty, value);
            }
        }

        private static void OnDataSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var sender = d as QuickJumpGrid;

            if (e.NewValue != null && e.NewValue != e.OldValue)
                sender.SetupDataSource(e.NewValue);
        }

        internal void SetupDataSource(object dataSource)
        {
            var items = dataSource as IEnumerable;

            if (items == null || items.Count() == 0)
                return;

            var groupedOrderedSequence = items.Cast<object>()
                .OrderBy(QuickJumpGridSelector.GetOrderByKeySelector())
                .ThenBy(QuickJumpGridSelector.GetThenByKeySelector())
                .GroupBy(QuickJumpGridSelector.GetGroupBySelector());

            _quickJumpLookup = new Dictionary<string, int>();
            var quickJumpItems = new ObservableCollection<object>();

            if (IsAlphaNumeric)
            {
                _quickJumpLookup["#"] = 0;
                for (int i = 97; i < 123; i++)
                {
                    _quickJumpLookup[((char)i).ToString()] = 0;
                }

                bool numericHeaderAdded = false;
                foreach (var group in groupedOrderedSequence)
                {
                    int numericKey;
                    bool isNumeric = int.TryParse(group.Key.ToString(), out numericKey);

                    if (isNumeric)
                    {
                        if (!numericHeaderAdded)
                        {
                            quickJumpItems.Add("#");
                            numericHeaderAdded = true;
                        }
                        _quickJumpLookup["#"] += group.Count();
                    }
                    else
                    {
                        string key = group.Key.ToString().ToLower();
                        int charValue = (int)key.First();

                        if (charValue < 97 || charValue > 122)
                        {
                            _quickJumpLookup["z"] += group.Count();
                        }
                        else
                        {
                            _quickJumpLookup[key] = group.Count();
                            quickJumpItems.Add(key);
                        }
                    }
                    quickJumpItems.AddRange(group);
                }
            }
            else
            {
                foreach (var group in groupedOrderedSequence)
                {
                    _quickJumpLookup[group.Key.ToString()] = group.Count();
                    quickJumpItems.Add(group.Key);
                    quickJumpItems.AddRange(group);
                }
            }

            if (_overlay != null)
                _overlay.ItemsSource = _quickJumpLookup;

            ItemsSource = quickJumpItems;
        }


        public void Remove(object toDelete)
        {
            var MyList = ItemsSource as ObservableCollection<object>;

            int index = MyList.IndexOf(toDelete);
            
            for(int i=index; i > -1; i--)
            {                
                if(_quickJumpLookup.ContainsKey(MyList[i].ToString()))
                {
                    _quickJumpLookup[MyList[i].ToString()]--;

                    if (_quickJumpLookup[MyList[i].ToString()] == 0)
                    {
                        _quickJumpLookup.Remove(MyList[i].ToString());
                        MyList.RemoveAt(i);                        
                    }

                    break;
                }
            }      

            MyList.Remove(toDelete);
        }
    }
}
