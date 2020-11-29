using System;
using System.Diagnostics;
using Foundation;

namespace SelectionMenu.iOS
{
    [DebuggerDisplay("Item = {" + nameof(Item) + "}")]
    public class IdentifierType<T> : NSObject
    {
        public IdentifierType(T item)
        {
            Item = item;
        }

        public readonly T Item;


        // pass-through the .NET hash
        public override nuint GetNativeHash() => (nuint) Item.GetHashCode();

        public override bool IsEqual(NSObject anObject)
        {
            if (anObject is IdentifierType<T> identifierType)
            {
                return identifierType.Item.Equals(Item);
            }

            return false;
        }
    }
}