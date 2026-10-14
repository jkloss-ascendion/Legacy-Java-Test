using System.Web;
using log4net;

namespace LegacyInventory.Core.Services
{
    public static class AuditLogger
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(AuditLogger));

        public static void Record(string action, string details)
        {
            var user = "anonymous";
            var context = HttpContext.Current;
            if (context != null && context.User != null && context.User.Identity.IsAuthenticated)
            {
                user = context.User.Identity.Name;
            }

            Log.InfoFormat("AUDIT user={0} action={1} details={2}", user, action, details);
        }
    }
}
