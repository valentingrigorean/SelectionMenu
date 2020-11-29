using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Foundation;
using UIKit;

namespace SelectionMenu.iOS
{
    public delegate SelectionMenuTableViewCell<T> UITableViewSelectionMenuDataSourceProvider<T>(
        UITableView tableView,
        T item);


    public class
        SelectionMenuDataSource<T> : UITableViewDiffableDataSource<EmptyIdentifier,
            IdentifierType<SelectionMenuItem>>, IUITableViewDelegate

    {
        private readonly List<SelectionMenuItem> _selectionMenuItems = new List<SelectionMenuItem>();
        private readonly List<SelectionMenuItem> _filterList = new List<SelectionMenuItem>();
        private readonly List<T> _selectedItem = new List<T>();
        private readonly IEqualityComparer _equalityComparerInternal;
        private T[] _items;

        private readonly ISelectionMenuDelegate<T> _selectionMenuDelegate;

        protected SelectionMenuDataSource(NSObjectFlag t) : base(t)
        {
        }

        protected internal SelectionMenuDataSource(IntPtr handle) : base(handle)
        {
        }

        public SelectionMenuDataSource(UITableView tableView,
            ISelectionMenuDelegate<T> selectionMenuDelegate,
            UITableViewSelectionMenuDataSourceProvider<T> cellProvider) :
            base(tableView, (view, _, item) =>
            {
                var wrapper = (IdentifierType<SelectionMenuItem>) item;
                var cell = cellProvider(view, (T) wrapper.Item.Item);
                cell.BindItem(wrapper.Item);
                return cell;
            })
        {
            _selectionMenuDelegate = selectionMenuDelegate;
            _equalityComparerInternal = new EqualityComparerInternal(selectionMenuDelegate.EqualityComparer);

            tableView.Delegate = this;
        }

        public T[] Items
        {
            get => _items ?? Array.Empty<T>();
            set
            {
                if (_items == value)
                    return;
                _items = value;
                SyncWithMenuItems(Items);
            }
        }

        public T[] SelectedItems => _selectedItem.ToArray();

        public void SelectItem(T item)
        {
            SelectItemInternal(item, true);
        }

        public void SelectItems(params T[] items)
        {
            if (!_selectionMenuDelegate.AllowMultipleSelection)
            {
                throw new InvalidOperationException(
                    "Can't select multiple items because of ISelectionMenuDelegate -> false");
            }

            foreach (var item in items)
            {
                SelectItem(item);
            }
        }

        public void UnSelectItem(T item)
        {
            SelectItemInternal(item, false);
        }

        public void UnSelectItems(params T[] items)
        {
            if (!_selectionMenuDelegate.AllowMultipleSelection)
            {
                throw new InvalidOperationException(
                    "Can't select multiple items because of ISelectionMenuDelegate -> false");
            }

            foreach (var item in items)
            {
                UnSelectItem(item);
            }
        }


        public void ApplyFilter(Func<T, bool> filter)
        {
            _filterList.Clear();
            _filterList.AddRange(_selectionMenuItems.Where(i => filter((T) i.Item)));
            ApplySnapshot(_filterList);
        }

        public void ClearFilter()
        {
            if (_filterList.Count == _selectionMenuItems.Count)
                return;
            _filterList.Clear();
            _filterList.AddRange(_selectionMenuItems);
            ApplySnapshot(_selectionMenuItems);
        }

        [Export("tableView:didSelectRowAtIndexPath:")]
        private void RowSelected(UITableView tableView, NSIndexPath indexPath)
        {
            var item = _filterList[indexPath.Row];
            NotifySelectedItem(item, !item.IsSelected);
            if (_selectionMenuDelegate.AllowMultipleSelection)
                return;
            var currentSelected =
                _filterList.FirstOrDefault(i => !_equalityComparerInternal.Equals(item.Item, i.Item) && i.IsSelected);
            if (currentSelected == null)
                return;
            NotifySelectedItem(currentSelected, false);
        }

        private void ApplySnapshot(IEnumerable<SelectionMenuItem> items)
        {
            var snapshot = new NSDiffableDataSourceSnapshot<EmptyIdentifier, IdentifierType<SelectionMenuItem>>();
            snapshot.AppendSections(new[] {EmptyIdentifier.Default});
            snapshot.AppendItems(items.Select(s => new IdentifierType<SelectionMenuItem>(s)).ToArray());
            ApplySnapshot(snapshot, true);
        }

        private void SelectItemInternal(T item, bool isSelected)
        {
            var wrapper = _selectionMenuItems.FirstOrDefault(i => _equalityComparerInternal.Equals(i, item));
            if (wrapper == null)
                return;

            NotifySelectedItem(wrapper, isSelected);
        }

        private void NotifySelectedItem(SelectionMenuItem selectionMenuItem, bool isSelected)
        {
            selectionMenuItem.IsSelected = isSelected;
            var snapshot = Snapshot;
            snapshot.ReloadItems(new[] {new IdentifierType<SelectionMenuItem>(selectionMenuItem)});
            ApplySnapshot(snapshot, false);
            NotifySelectedItem((T) selectionMenuItem.Item, isSelected);
        }

        private void NotifySelectedItem(T item, bool isSelected)
        {
            _selectedItem.Remove(item);
            if (isSelected)
            {
                _selectedItem.Add(item);
                _selectionMenuDelegate.OnItemSelected(item);
            }
            else
            {
                _selectionMenuDelegate.OnItemUnselected(item);
            }
        }

        private void SyncWithMenuItems(IEnumerable<T> items)
        {
            _selectionMenuItems.Clear();
            _filterList.Clear();
            foreach (var item in items)
            {
                var selectionMenuItem = new SelectionMenuItem(item, false, _equalityComparerInternal);
                _selectionMenuItems.Add(selectionMenuItem);
                _filterList.Add(selectionMenuItem);
            }

            ApplySnapshot(_selectionMenuItems);
        }


        private class EqualityComparerInternal : IEqualityComparer
        {
            private readonly IEqualityComparer<T> _equalityComparer;

            public EqualityComparerInternal(IEqualityComparer<T> equalityComparer)
            {
                _equalityComparer = equalityComparer;
            }

            bool IEqualityComparer.Equals(object x, object y)
            {
                return _equalityComparer.Equals((T) x, (T) y);
            }

            public int GetHashCode(object obj) => _equalityComparer.GetHashCode((T) obj);
        }
    }
}