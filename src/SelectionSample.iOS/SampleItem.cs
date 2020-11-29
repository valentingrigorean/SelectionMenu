using System.Collections.Generic;

namespace SelectionSample
{
    public class SampleItem
    {
        public SampleItem(string name)
        {
            Name = name;
        }

        public string Name { get; }
    }

    public class SampleItemEqualityComparer : IEqualityComparer<SampleItem>
    {
        private SampleItemEqualityComparer()
        {
        }

        public static SampleItemEqualityComparer Instance { get; } = new SampleItemEqualityComparer();

        public bool Equals(SampleItem x, SampleItem y)
        {
            if (ReferenceEquals(x, y)) return true;
            if (ReferenceEquals(x, null)) return false;
            if (ReferenceEquals(y, null)) return false;
            if (x.GetType() != y.GetType()) return false;
            return x.Name == y.Name;
        }

        public int GetHashCode(SampleItem obj)
        {
            return (obj.Name != null ? obj.Name.GetHashCode() : 0);
        }
    }
}