using System;
using CoreGraphics;
using Foundation;
using UIKit;

namespace SelectionMenu.iOS
{
    public abstract class SelectionMenuTableViewCell<T> : UITableViewCell
    {
        protected SelectionMenuTableViewCell(IntPtr handle) : base(handle)
        {
        }

        protected SelectionMenuTableViewCell()
        {
        }

        protected SelectionMenuTableViewCell(CGRect frame) : base(frame)
        {
        }

        protected SelectionMenuTableViewCell(UITableViewCellStyle style, NSString reuseIdentifier) : base(style,
            reuseIdentifier)
        {
        }

        internal void BindItem(SelectionMenuItem selectionMenuItem)
        {
            OnItemSelected(selectionMenuItem.IsSelected);
            BindItem((T) selectionMenuItem.Item);
        }

        protected abstract void BindItem(T selectionMenuItem);

        protected abstract void OnItemSelected(bool isSelected);
    }
}