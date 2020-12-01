﻿// This file has been autogenerated from a class added in the UI designer.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Foundation;
using SelectionMenu.iOS;
using UIKit;

namespace SelectionSample
{
    public partial class SelectionViewController : UIViewController, ISelectionMenuDelegate<SampleItem>
    {
        private ISelectionMenuDataSource<SampleItem> _dataSource;

        private TaskCompletionSource<SampleItem[]> _tcs;

        private SampleItem[] _items;

        private bool _didFinish;

        private bool _allowMultipleSelection;

        public SelectionViewController(IntPtr handle) : base(handle)
        {
        }

        public bool AllowMultipleSelection => _allowMultipleSelection;
        public IEqualityComparer<SampleItem> EqualityComparer => SampleItemEqualityComparer.Instance;

        public void OnItemSelected(SampleItem item)
        {
        }

        public void OnItemUnselected(SampleItem item)
        {
        }


        public static Task<SampleItem[]> GetSelectedItemsAsync(UIViewController viewController, SampleItem[] items,
            bool allowMultipleSelection)
        {
            var storyboard = UIStoryboard.FromName("Main", null);
            var vc = (SelectionViewController) storyboard.InstantiateViewController(nameof(SelectionViewController));
            vc._allowMultipleSelection = allowMultipleSelection;
            vc._tcs = new TaskCompletionSource<SampleItem[]>();
            vc._items = items;
            var nav = new UINavigationController(vc);

            if (UIDevice.CurrentDevice.CheckSystemVersion(13, 0))
            {
                nav.PresentationController.Delegate = new UIAdaptivePresentationController(vc._tcs);
            }

            viewController.PresentViewController(nav, true, null);

            return vc._tcs.Task;
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            _dataSource = SelectionMenuDataSourceFactory.CreateDataSource(TableView, this, (tableView, item) =>
                (SelectionTableViewCell) tableView.DequeueReusableCell(nameof(SelectionTableViewCell)));

            _dataSource.Items = _items;


            TableView.DataSource = _dataSource;
            TableView.TableFooterView = new UIView();


            var searchController = new UISearchController((UIViewController) null);
            searchController.ObscuresBackgroundDuringPresentation = false;
            searchController.SearchBar.Placeholder = "Search...";
            searchController.SearchBar.AutocapitalizationType = UITextAutocapitalizationType.None;
            searchController.SearchBar.TextChanged += (sender, args) =>
            {
                if (args.SearchText.Length >= 2)
                {
                    var query = args.SearchText.ToLowerInvariant();
                    _dataSource.ApplyFilter(i => i.Name.ToLowerInvariant().Contains(query));
                }
                else
                {
                    _dataSource.ClearFilter();
                }
            };

            DefinesPresentationContext = true;


            NavigationItem.LeftBarButtonItem = new UIBarButtonItem(UIBarButtonSystemItem.Close,
                (sender, args) => SetResult(Array.Empty<SampleItem>()));

            NavigationItem.RightBarButtonItem = new UIBarButtonItem(UIBarButtonSystemItem.Done,
                (sender, args) => SetResult(_dataSource.SelectedItems));

            NavigationItem.SearchController = searchController;
        }


        private void SetResult(SampleItem[] sampleItem)
        {
            if (_didFinish)
                return;
            _didFinish = true;
            _tcs.TrySetResult(sampleItem);
            NavigationController.DismissViewController(true, null);
        }

        private class UIAdaptivePresentationController : UIAdaptivePresentationControllerDelegate
        {
            private readonly TaskCompletionSource<SampleItem[]> _taskCompletionSource;

            public UIAdaptivePresentationController(TaskCompletionSource<SampleItem[]> taskCompletionSource)
            {
                _taskCompletionSource = taskCompletionSource;
            }

            public override void DidDismiss(UIPresentationController presentationController)
            {
                _taskCompletionSource.TrySetResult(Array.Empty<SampleItem>());
            }
        }
    }
}