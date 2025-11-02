namespace CroweAlumniPortal.Helper
{
    public static class SessionExtensions
    {
        public static int? GetUserId(this ISession session)
        {
            var s = session.GetString("UserId");
            return int.TryParse(s, out var id) ? id : (int?)null;
        }
    }
}
