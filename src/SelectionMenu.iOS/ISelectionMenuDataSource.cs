using System;
using UIKit;

namespace SelectionMenu.iOS
{
    public interface ISelectionMenuDataSource<T> : IUITableViewDelegate, IUITableViewDataSource
    {
        T[] Items { get; set; }

        T[] SelectedItems { get; }

        void SelectItem(T item);

        void SelectItems(params T[] items);

        void UnSelectItem(T item);

        void UnSelectItems(params T[] items);

        void ApplyFilter(Func<T, bool> filter);

        void ClearFilter();
    }
}