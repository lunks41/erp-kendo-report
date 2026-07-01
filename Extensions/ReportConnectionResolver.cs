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

        /// <summary>
        /// Returns the appsettings connection string name (e.g. DbConnection) for Telerik SqlDataSource.
        /// Reports are authored with a named reference like Reporting.AHHA; swapping the name preserves provider resolution.
        /// </summary>
        public string ResolveConnectionStringName(IHeaderDictionary? headers)
        {
            string? regId = null;
            if (headers != null && headers.TryGetValue("X-Reg-Id", out var regIdValues))
                regId = regIdValues.FirstOrDefault();

            return ResolveConnectionStringName(regId);
        }

        public string? ResolveConnectionString(string? regId)
        {
            var name = ResolveConnectionStringName(regId);
            var resolved = _configuration.GetConnectionString(name);
            return !string.IsNullOrEmpty(resolved) ? resolved : null;
        }

        public string ResolveConnectionStringName(string? regId)
        {
            if (!string.IsNullOrWhiteSpace(regId))
            {
                try
                {
                    var connectionStringName = _dbGetConnection.GetconnectionDB(regId);
                    if (!string.IsNullOrEmpty(connectionStringName)
                        && !string.IsNullOrEmpty(_configuration.GetConnectionString(connectionStringName)))
                    {
                        return connectionStringName;
                    }
                }
                catch
                {
                    // regCompany.json lookup failed — fall through to defaults
                }
            }

            if (!string.IsNullOrEmpty(_configuration.GetConnectionString("DbConnection")))
                return "DbConnection";

            return "Reporting.AHHA";
        }
    }
}
