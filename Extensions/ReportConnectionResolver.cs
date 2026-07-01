using System.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;

namespace my_report.Extensions
{
    /// <summary>
    /// Resolves SQL connection strings from X-Reg-Id via regCompany.json (same pattern as api-core).
    /// </summary>
    public class ReportConnectionResolver
    {
        private readonly IConfiguration _configuration;
        private readonly DBGetConnection _dbGetConnection;

        public ReportConnectionResolver(IConfiguration configuration)
        {
            _configuration = configuration;
            _dbGetConnection = new DBGetConnection();
        }

        public string? ResolveConnectionString(IHeaderDictionary? headers)
        {
            string? regId = null;
            if (headers != null && headers.TryGetValue("X-Reg-Id", out var regIdValues))
                regId = regIdValues.FirstOrDefault();

            return ResolveConnectionString(regId);
        }

        public string? ResolveConnectionString(string? regId)
        {
            if (!string.IsNullOrWhiteSpace(regId))
            {
                try
                {
                    var connectionStringName = _dbGetConnection.GetconnectionDB(regId);
                    if (!string.IsNullOrEmpty(connectionStringName))
                    {
                        var resolved = _configuration.GetConnectionString(connectionStringName);
                        if (!string.IsNullOrEmpty(resolved))
                            return resolved;
                    }
                }
                catch
                {
                    // regCompany.json lookup failed — fall through to defaults
                }
            }

            return _configuration.GetConnectionString("DbConnection")
                ?? _configuration.GetConnectionString("Reporting.AHHA");
        }
    }
}
