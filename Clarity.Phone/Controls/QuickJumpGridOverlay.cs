using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using Clarity.Phone.Extensions;

namespace Clarity.Phone.Controls
{
    public class QuickJumpGridOverlay : ListBox
    {

        private static readonly string FlipStoryboard = @"<Storyboard xmlns=""http://schemas.microsoft.com/winfx/2006/xaml/presentation"">
			<DoubleAnimation Duration=""0:0:0.45"" To=""0"" Storyboard.TargetProperty=""RotationX"" >
				<DoubleAnimation.EasingFunction>
					<ExponentialEase EasingMode=""EaseIn"" Exponent=""6""/>
				</DoubleAnimation.EasingFunction>
			</DoubleAnimation>
			<DoubleAnimation Duration=""0:0:0"" To=""0"" Storyboard.TargetProperty=""Opacity"" />
		</Storyboard>";

        private QuickJumpGrid _owner;
        private ListBoxItem _listboxItem;
        private KeyValuePair<string, int> _kvp;
        private PlaneProjection _planeProjection;
        private static Storyboard _storyboard;
        private static DoubleAnimation _flipAnimation;
        private static DoubleAnimation _opacityAnimation;

        public delegate void TileSelectedEventHandler(object sender, string key);
        public event TileSelectedEventHandler TileSelected;

        static QuickJumpGridOverlay()
        {
            _storyboard = System.Windows.Markup.XamlReader.Load(FlipStoryboard) as Storyboard;
            _flipAnimation = (_storyboard.Children[0] as DoubleAnimation);
            _opacityAnimation = (_storyboard.Children[1] as DoubleAnimation);
        }

        public QuickJumpGridOverlay()
        {
            base.DefaultStyleKey = typeof(QuickJumpGridOverlay);
            this.SelectionMode = System.Windows.Controls.SelectionMode.Single;
            this.SelectionChanged += new SelectionChangedEventHandler(OnSelectionChanged);

            _planeProjection = new PlaneProjection { RotationX = -90 };

            _storyboard.Stop();
            _planeProjection.RotationX = -90;
            Opacity = 0;
        }

        public void Show()
        {
            SelectedIndex = -1;
            _planeProjection.RotationX = -45;
            _opacityAnimation.To = 1;
            _opacityAnimation.BeginTime = TimeSpan.FromMilliseconds(0);
            _flipAnimation.To = 0;
            _flipAnimation.BeginTime = TimeSpan.FromMilliseconds(100);
            _flipAnimation.Duration = TimeSpan.FromMilliseconds(350);
            (_flipAnimation.EasingFunction as ExponentialEase).EasingMode = EasingMode.EaseOut;
            Storyboard.SetTarget(_flipAnimation, _planeProjection);
            Storyboard.SetTarget(_opacityAnimation, this);


            Dispatcher.BeginInvoke(() =>
            {
                _storyboard.Begin();
            });
        }

        void OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (SelectedItem != null)
                Close();
        }

        internal void Close()
        {
            _storyboard.Stop();
            Opacity = 1;
            _planeProjection.RotationX = 0;
            _flipAnimation.To = 60.0;
            _flipAnimation.Duration = TimeSpan.FromMilliseconds(250);
            (_flipAnimation.EasingFunction as ExponentialEase).EasingMode = EasingMode.EaseIn;
            _opacityAnimation.To = 0;
            _opacityAnimation.BeginTime = TimeSpan.FromMilliseconds(267);
            Storyboard.SetTarget(_flipAnimation, _planeProjection);
            Storyboard.SetTarget(_opacityAnimation, this);
            _storyboard.Completed += new EventHandler(OnOutroStoryboardCompleted);
            _storyboard.Begin();
        }

        void OnOutroStoryboardCompleted(object sender, EventArgs e)
        {
            _storyboard.Completed -= new EventHandler(OnOutroStoryboardCompleted);
            _storyboard.Stop();
            _planeProjection.RotationX = -90;
            Opacity = 0;
            if (null != TileSelected)
            {
                if (SelectedIndex > -1)
                    TileSelected(this, ((KeyValuePair<string, int>)SelectedItem).Key);
                else
                    TileSelected(this, string.Empty);
            }
        }

        public QuickJumpGridOverlay(QuickJumpGrid owner)
            : this()
        {
            _owner = owner;
        }
 
        protected override void PrepareContainerForItemOverride(DependencyObject element, object item)
        {
            base.PrepareContainerForItemOverride(element, item);


            //TODO: could do this through a binding converter. See if there is any perf diff
            _listboxItem = element as ListBoxItem;
            if (_listboxItem != null)
            {
                _listboxItem.Projection = _planeProjection;
                _kvp = (KeyValuePair<string, int>)item;
                _listboxItem.IsEnabled = _kvp.Value > 0;

                //Could also set these styles through a binding converter based on the count property
                if (_owner != null)
                {
                    if (_listboxItem.IsEnabled)
                    {
                        _listboxItem.Foreground = _owner.OverlayTileForeground;
                        _listboxItem.Background = _owner.OverlayTileBackground;
                    }
                    else
                    {
                        _listboxItem.Foreground = _owner.OverlayTileDisabledForeground;
                        _listboxItem.Background = _owner.OverlayTileDisabledBackground;
                    }
                }
            }
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            if (_owner != null)
            {
                if (_owner.IsAlphaNumeric)
                {
                    ItemsPanel = AlphaNumericItemsPanel;
                    ItemTemplate = AlphaNumericItemTemplate;
                }
            }
        }

        public static readonly DependencyProperty AlphaNumericItemsPanelProperty = DependencyProperty.Register("AlphaNumericItemsPanel", typeof(ItemsPanelTemplate), typeof(QuickJumpGridOverlay), new PropertyMetadata(null));
        public ItemsPanelTemplate AlphaNumericItemsPanel
        {
            get
            {
                return (GetValue(AlphaNumericItemsPanelProperty) as ItemsPanelTemplate);
            }
            set
            {
                SetValue(AlphaNumericItemsPanelProperty, value);
            }
        }

        public static readonly DependencyProperty AlphaNumericItemTemplateProperty = DependencyProperty.Register("AlphaNumericItemTemplate", typeof(DataTemplate), typeof(QuickJumpGridOverlay), new PropertyMetadata(null));
        public DataTemplate AlphaNumericItemTemplate
        {
            get
            {
                return (GetValue(AlphaNumericItemTemplateProperty) as DataTemplate);
            }
            set
            {
                SetValue(AlphaNumericItemTemplateProperty, value);
            }
        }
    }
}
