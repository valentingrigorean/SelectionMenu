using System.Collections.Generic;

namespace SelectionMenu.iOS
{
    public interface ISelectionMenuDelegate<T>
    {
        bool AllowMultipleSelection { get; }

        IEqualityComparer<T> EqualityComparer { get; }

        void OnItemSelected(T item);

        void OnItemUnselected(T item);
    }
}