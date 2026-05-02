namespace codessentials.CGM.Elements
{
    /// <summary>
    /// Represents a CGM Application Structure LINKURI attribute.
    /// </summary>
    public sealed class LinkUriElement
    {
        public string Destination { get; }
        public string? Title { get; }
        public string? Behavior { get; }

        public LinkUriElement(string destination, string? title, string? behavior)
        {
            Destination = destination;
            Title = title;
            Behavior = behavior;
        }

        public override string ToString() => Destination;
    }
}
