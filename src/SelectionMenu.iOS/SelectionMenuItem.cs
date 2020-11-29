using System.Collections;

namespace SelectionMenu.iOS
{
    public class SelectionMenuItem
    {
        private readonly IEqualityComparer _equalityComparer;

        public SelectionMenuItem(object item, bool isSelected, IEqualityComparer equalityComparer)
        {
            Item = item;
            _equalityComparer = equalityComparer;
            IsSelected = isSelected;
        }

        public bool IsSelected { get; set; }

        public object Item { get; }

        public SelectionMenuItem Clone(bool isSelected) => new SelectionMenuItem(Item, isSelected, _equalityComparer);

        public override int GetHashCode()
        {
            return _equalityComparer.GetHashCode(Item);
        }

        public override bool Equals(object obj)
        {
            if (obj is SelectionMenuItem selectionMenuItem)
            {
                return _equalityComparer.Equals(Item, selectionMenuItem.Item);
            }

            return false;
        }
    }
}