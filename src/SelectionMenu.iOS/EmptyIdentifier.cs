namespace SelectionMenu.iOS
{
    public class EmptyIdentifier : IdentifierType<EmptyIdentifier.Unit>
    {
        private EmptyIdentifier(Unit item) : base(item)
        {
        }

        public static EmptyIdentifier Default { get; } = new EmptyIdentifier(new Unit());

        public struct Unit
        {
        }
    }
}