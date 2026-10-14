using log4net;

namespace LegacyInventory.Core.Services
{
    public class AuditLogger
    {
        private readonly ILog _log;

        public AuditLogger(ILog log)
        {
            _log = log;
        }

        public void Record(string action, string details, string user)
        {
            if (string.IsNullOrEmpty(user))
            {
                user = "anonymous";
            }

            _log.InfoFormat("AUDIT user={0} action={1} details={2}", user, action, details);
        }
    }
}
