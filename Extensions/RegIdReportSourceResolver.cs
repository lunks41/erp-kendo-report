using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Telerik.Reporting;
using Telerik.Reporting.Services;

namespace my_report.Extensions
{
    /// <summary>
    /// Applies the tenant connection string from X-Reg-Id to SqlDataSource components before rendering.
    /// </summary>
    public class RegIdReportSourceResolver : IReportSourceResolver
    {
        private readonly IReportSourceResolver _parentResolver;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ReportConnectionResolver _connectionResolver;

        public RegIdReportSourceResolver(
            IReportSourceResolver parentResolver,
            IHttpContextAccessor httpContextAccessor,
            ReportConnectionResolver connectionResolver)
        {
            _parentResolver = parentResolver;
            _httpContextAccessor = httpContextAccessor;
            _connectionResolver = connectionResolver;
        }

        public ReportSource Resolve(
            string report,
            OperationOrigin operationOrigin,
            IDictionary<string, object> currentParameterValues)
        {
            var reportSource = _parentResolver.Resolve(report, operationOrigin, currentParameterValues);
            if (reportSource == null)
                return null!;

            var connectionString = _connectionResolver.ResolveConnectionString(
                _httpContextAccessor.HttpContext?.Request.Headers);

            if (string.IsNullOrEmpty(connectionString))
                return reportSource;

            if (reportSource is InstanceReportSource instanceReportSource)
                ApplyConnectionString(instanceReportSource.ReportDocument, connectionString);

            return reportSource;
        }

        private static void ApplyConnectionString(IReportDocument reportDocument, string connectionString)
        {
            switch (reportDocument)
            {
                case Report report:
                    SetSqlDataSourceConnectionStrings(report, connectionString);
                    break;
                case ReportBook reportBook:
                    foreach (var bookReport in reportBook.Reports)
                        SetSqlDataSourceConnectionStrings(bookReport, connectionString);
                    break;
            }
        }

        private static void SetSqlDataSourceConnectionStrings(Report report, string connectionString)
        {
            foreach (var sqlDataSource in report.GetDataSources().OfType<SqlDataSource>())
                sqlDataSource.ConnectionString = connectionString;
        }
    }
}
