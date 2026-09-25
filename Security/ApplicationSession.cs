namespace WholesaleHub.Security
{
    public static class ApplicationSession
    {
        public const string ClaimType = "WholesaleHub:ApplicationSession";
        public static string Id { get; } = Guid.NewGuid().ToString("N");
    }
}
