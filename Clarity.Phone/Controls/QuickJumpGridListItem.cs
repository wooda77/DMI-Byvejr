using System;
using System.Windows.Controls;
using System.Windows.Input;

namespace Clarity.Phone.Controls
{
    public class QuickJumpGridListItem : ListBoxItem
    {
        private bool _isHeader;
        private bool _isTap;
        private bool _inManipulation;
        private QuickJumpGrid _owner;

        public QuickJumpGridListItem()
            : this(null)
        {
            
        }

        public QuickJumpGridListItem(QuickJumpGrid owner)
            : base()
        {
            _owner = owner;
        }

        protected override void OnManipulationStarted(ManipulationStartedEventArgs e)
        {
            base.OnManipulationStarted(e);
            if (!e.Handled && IsHeader)
            {
                _isTap = true;
                _inManipulation = true;
            }
        }

        protected override void OnManipulationDelta(ManipulationDeltaEventArgs e)
        {
            base.OnManipulationDelta(e);

            if (Math.Abs(e.CumulativeManipulation.Translation.X) > 1 || Math.Abs(e.CumulativeManipulation.Translation.Y) > 1)
                _isTap = false;
        }

        protected override void OnManipulationCompleted(ManipulationCompletedEventArgs e)
        {
            base.OnManipulationCompleted(e);
            if (!e.Handled && IsHeader)
            {
                _inManipulation = false;
                if (_isTap && (this._owner != null))
                {
                    e.Handled = _owner.OnHeaderClicked();
                }
            }
        }

        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonDown(e);

            if ((!e.Handled && !_inManipulation) && (_owner != null) && IsHeader)
            {
                e.Handled = _owner.OnHeaderClicked();
            }
        }

        public bool IsHeader 
        {
            get { return _isHeader; }
            set 
            { 
                _isHeader = value;
                this.IsTabStop = !_isHeader;
            }
        }
    }
}
