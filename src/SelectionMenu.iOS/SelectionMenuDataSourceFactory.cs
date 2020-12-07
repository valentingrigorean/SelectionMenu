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


    public static class SelectionMenuDataSourceFactory
    {
        public static ISelectionMenuDataSource<T> CreateDataSource<T>(UITableView tableView,
            ISelectionMenuDelegate<T> selectionMenuDelegate,
            UITableViewSelectionMenuDataSourceProvider<T> cellProvider)
        {
            if (UIDevice.CurrentDevice.CheckSystemVersion(13, 0))
                return new SelectionMenuDiffableDataSource<T>(tableView, selectionMenuDelegate, cellProvider);
            return new SelectionMenuDataSource<T>(tableView, selectionMenuDelegate, cellProvider);
        }

        private interface ISelectionMenuAdapterDelegate
        {
            void ApplySnapshot(IEnumerable<SelectionMenuItem> items);
            void NotifySelectedItem(SelectionMenuItem selectionMenuItem);
        }


        private class SelectionMenuAdapter<T>
        {
            private readonly List<SelectionMenuItem> _selectionMenuItems = new List<SelectionMenuItem>();
            private readonly List<SelectionMenuItem> _filterList = new List<SelectionMenuItem>();
            private readonly List<T> _selectedItem = new List<T>();
            private readonly IEqualityComparer _equalityComparerInternal;

            private readonly ISelectionMenuAdapterDelegate _menuAdapterDelegate;
            private readonly ISelectionMenuDelegate<T> _selectionMenuDelegate;

            private T[] _items;

            public SelectionMenuAdapter(ISelectionMenuAdapterDelegate menuAdapterDelegate,
                ISelectionMenuDelegate<T> selectionMenuDelegate)
            {
                _menuAdapterDelegate = menuAdapterDelegate;
                _selectionMenuDelegate = selectionMenuDelegate;

                _equalityComparerInternal = new EqualityComparerInternal(selectionMenuDelegate.EqualityComparer);
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
                _menuAdapterDelegate.ApplySnapshot(_filterList);
            }

            public void ClearFilter()
            {
                if (_filterList.Count == _selectionMenuItems.Count)
                    return;
                _filterList.Clear();
                _filterList.AddRange(_selectionMenuItems);
                _menuAdapterDelegate.ApplySnapshot(_selectionMenuItems);
            }


            public void RowSelected(NSIndexPath indexPath)
            {
                var item = _filterList[indexPath.Row];
                SelectItemInternal(item,!item.IsSelected);
                if (_selectionMenuDelegate.AllowMultipleSelection)
                    return;
                var currentSelected =
                    _filterList.FirstOrDefault(
                        i => !_equalityComparerInternal.Equals(item.Item, i.Item) && i.IsSelected);
                if (currentSelected == null)
                    return;
                NotifySelectedItem(currentSelected, false);
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

            private void SelectItemInternal(T item, bool isSelected)
            {
                var wrapper = _selectionMenuItems.FirstOrDefault(i => _equalityComparerInternal.Equals(i.Item, item));
                if (wrapper == null)
                    return;
                SelectItemInternal(wrapper, isSelected);
            }

            private void SelectItemInternal(SelectionMenuItem item, bool isSelected)
            {
                switch (isSelected)
                {
                    case true when !_selectionMenuDelegate.ShouldSelectItem((T) item.Item):
                    case false when !_selectionMenuDelegate.ShouldUnselectItem((T) item.Item):
                        return;
                }

                NotifySelectedItem(item, isSelected);
            }

            private void NotifySelectedItem(SelectionMenuItem selectionMenuItem, bool isSelected)
            {
                selectionMenuItem.IsSelected = isSelected;
                _menuAdapterDelegate.NotifySelectedItem(selectionMenuItem);
                NotifySelectedItem((T) selectionMenuItem.Item, isSelected);
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

                _menuAdapterDelegate.ApplySnapshot(_selectionMenuItems);
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


        private class SelectionMenuDataSource<T> : UITableViewSource, ISelectionMenuDataSource<T>,
            ISelectionMenuAdapterDelegate
        {
            private readonly List<SelectionMenuItem> _items = new List<SelectionMenuItem>();

            private readonly SelectionMenuAdapter<T> _adapter;
            private readonly UITableView _tableView;
            private readonly UITableViewSelectionMenuDataSourceProvider<T> _cellProvider;


            public SelectionMenuDataSource(UITableView tableView,
                ISelectionMenuDelegate<T> selectionMenuDelegate,
                UITableViewSelectionMenuDataSourceProvider<T> cellProvider)
            {
                _tableView = tableView;
                _tableView.Delegate = this;
                _cellProvider = cellProvider;
                _adapter = new SelectionMenuAdapter<T>(this, selectionMenuDelegate);
            }

            public T[] Items
            {
                get => _adapter.Items;
                set => _adapter.Items = value;
            }

            public T[] SelectedItems => _adapter.SelectedItems;

            public void SelectItem(T item) => _adapter.SelectItem(item);

            public void SelectItems(params T[] items) => _adapter.SelectItems(items);

            public void UnSelectItem(T item) => _adapter.UnSelectItem(item);

            public void UnSelectItems(params T[] items) => _adapter.UnSelectItems(items);

            public void ApplyFilter(Func<T, bool> filter) => _adapter.ApplyFilter(filter);

            public void ClearFilter() => _adapter.ClearFilter();

            public void ApplySnapshot(IEnumerable<SelectionMenuItem> items)
            {
                _items.Clear();
                _items.AddRange(items);
                _tableView.ReloadData();
            }

            public void NotifySelectedItem(SelectionMenuItem selectionMenuItem)
            {
                _tableView.ReloadData();
            }

            public override nint RowsInSection(UITableView tableview, nint section) => _items.Count;

            public override void RowSelected(UITableView tableView, NSIndexPath indexPath) =>
                _adapter.RowSelected(indexPath);

            public override UITableViewCell GetCell(UITableView tableView, NSIndexPath indexPath)
            {
                var item = _items[indexPath.Row];
                var cell = _cellProvider(tableView, (T) item.Item);
                cell.BindItem(item);
                return cell;
            }
        }


        private class
            SelectionMenuDiffableDataSource<T> : UITableViewDiffableDataSource<EmptyIdentifier,
                IdentifierType<SelectionMenuItem>>, ISelectionMenuDataSource<T>, ISelectionMenuAdapterDelegate

        {
            private readonly SelectionMenuAdapter<T> _adapter;

            protected SelectionMenuDiffableDataSource(NSObjectFlag t) : base(t)
            {
            }

            protected internal SelectionMenuDiffableDataSource(IntPtr handle) : base(handle)
            {
            }

            public SelectionMenuDiffableDataSource(UITableView tableView,
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
                tableView.Delegate = this;
                _adapter = new SelectionMenuAdapter<T>(this, selectionMenuDelegate);
            }

            public T[] Items
            {
                get => _adapter.Items;
                set => _adapter.Items = value;
            }

            public T[] SelectedItems => _adapter.SelectedItems;

            public void SelectItem(T item) => _adapter.SelectItem(item);

            public void SelectItems(params T[] items) => _adapter.SelectItems(items);

            public void UnSelectItem(T item) => _adapter.UnSelectItem(item);

            public void UnSelectItems(params T[] items) => _adapter.UnSelectItems(items);

            public void ApplyFilter(Func<T, bool> filter) => _adapter.ApplyFilter(filter);

            public void ClearFilter() => _adapter.ClearFilter();

            public void ApplySnapshot(IEnumerable<SelectionMenuItem> items)
            {
                var snapshot = new NSDiffableDataSourceSnapshot<EmptyIdentifier, IdentifierType<SelectionMenuItem>>();
                snapshot.AppendSections(new[] {EmptyIdentifier.Default});
                snapshot.AppendItems(items.Select(s => new IdentifierType<SelectionMenuItem>(s)).ToArray());
                ApplySnapshot(snapshot, true);
            }


            public void NotifySelectedItem(SelectionMenuItem selectionMenuItem)
            {
                var snapshot = Snapshot;
                snapshot.ReloadItems(new[] {new IdentifierType<SelectionMenuItem>(selectionMenuItem)});
                ApplySnapshot(snapshot, false);
            }


            [Export("tableView:didSelectRowAtIndexPath:")]
            // ReSharper disable once UnusedParameter.Local
            private void RowSelected(UITableView tableView, NSIndexPath indexPath) => _adapter.RowSelected(indexPath);
        }
    }
}