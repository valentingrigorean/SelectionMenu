using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UIKit;
using Foundation;
using SelectionMenu.iOS;

namespace SelectionSample
{
    [Register("SelectionViewController2")]
    public partial class SelectionViewController2 : UIViewController, ISelectionMenuDelegate<SampleItem>
    {
        private UITableView _tableView;

        private ISelectionMenuDataSource<SampleItem> _dataSource;

        private TaskCompletionSource<SampleItem[]> _tcs;

        private SampleItem[] _items;
        private SampleItem[] _itemsSelected;

        private bool _didFinish;

        private bool _allowMultipleSelection;
        private bool _allowFiltering;
        

        public bool AllowMultipleSelection => _allowMultipleSelection;
        public bool AllowFiltering => _allowFiltering;
        public IEqualityComparer<SampleItem> EqualityComparer => SampleItemEqualityComparer.Instance;

        public bool ShouldSelectItem(SampleItem item)
        {
            return true;
        }

        public bool ShouldUnselectItem(SampleItem item)
        {
            return true;
        }

        public void OnItemSelected(SampleItem item)
        {
            if (AllowMultipleSelection) return;
            SetResult(_dataSource.SelectedItems);
        }

        public void OnItemUnselected(SampleItem item)
        {
        }

        public static Task<SampleItem[]> GetSelectedItemsAsync(UIViewController viewController, SampleItem[] items,
            SampleItem[] itemsSelected, string title,
            bool allowMultipleSelection, bool allowFiltering)
        {
            //     var storyboard = UIStoryboard.FromName("Main", null);
            //     var vc = (SelectionViewController)storyboard.InstantiateViewController(nameof(SelectionViewController));

            var vc = new SelectionViewController2();
            vc.Title = title;
            vc._allowMultipleSelection = allowMultipleSelection;
            vc._allowFiltering = allowFiltering;
            vc._tcs = new TaskCompletionSource<SampleItem[]>();
            vc._items = items;
            vc._itemsSelected = itemsSelected;

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

            // NavigationController.NavigationBar.BackgroundColor = ColorHelper.FromType(ColorType.Navigationbar);
            // NavigationController.NavigationBar.BarTintColor = ColorHelper.FromType(ColorType.Navigationbar);
            // NavigationController.NavigationBar.TintColor = ColorHelper.FromType(ColorType.NavigationbarTint);
            // NavigationController.NavigationBar.TitleTextAttributes = new UIStringAttributes()
            // {
            //     ForegroundColor = ColorHelper.FromType(ColorType.NavigationbarLabel)
            // };

            _tableView = new UITableView
            {
                TranslatesAutoresizingMaskIntoConstraints = false
            };
            View?.AddSubview(_tableView);
            _tableView.RegisterClassForCellReuse(typeof(SelectionTableViewCell2), nameof(SelectionTableViewCell2));



            NSLayoutConstraint.ActivateConstraints(new[]
            {
                _tableView.LeadingAnchor.ConstraintEqualTo(View.LeadingAnchor),
                _tableView.TrailingAnchor.ConstraintEqualTo(View.TrailingAnchor),
                _tableView.BottomAnchor.ConstraintEqualTo(View.BottomAnchor),
                _tableView.TopAnchor.ConstraintEqualTo(View.TopAnchor)
            });


            _dataSource = SelectionMenuDataSourceFactory.CreateDataSource(_tableView, this, (tableView, item) =>
                (SelectionTableViewCell2) tableView.DequeueReusableCell(nameof(SelectionTableViewCell2)));

            _dataSource.Items = _items;
            if (AllowMultipleSelection && _itemsSelected != null)
                _dataSource.SelectItems(_itemsSelected);
            else if (_itemsSelected != null)
                _dataSource.SelectItem(_itemsSelected[0]);

            _tableView.DataSource = _dataSource;

            _tableView.TableFooterView = new UIView();

            var searchController = new UISearchController((UIViewController) null);


            NavigationItem.LeftBarButtonItem = new UIBarButtonItem(UIBarButtonSystemItem.Close,
                (sender, args) => SetResult(Array.Empty<SampleItem>()));

            NavigationItem.RightBarButtonItem = new UIBarButtonItem(UIBarButtonSystemItem.Done,
                (sender, args) => SetResult(_dataSource.SelectedItems));

            if (AllowFiltering)
            {
                searchController.ObscuresBackgroundDuringPresentation = false;
                searchController.HidesNavigationBarDuringPresentation = false;
                if (UIDevice.CurrentDevice.CheckSystemVersion(13, 0))
                    searchController.AutomaticallyShowsCancelButton = false;

                searchController.SearchBar.Placeholder = "Search ...";
                searchController.SearchBar.AutocapitalizationType = UITextAutocapitalizationType.None;
                searchController.SearchBar.TextChanged += (sender, args) =>
                {
                    if (args.SearchText.Length >= 1)
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

                NavigationItem.SearchController = searchController;
            }

            NavigationItem.HidesSearchBarWhenScrolling = false;
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