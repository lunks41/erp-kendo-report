using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
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
        private readonly ILogger<RegIdReportSourceResolver> _logger;

        public RegIdReportSourceResolver(
            IReportSourceResolver parentResolver,
            IHttpContextAccessor httpContextAccessor,
            ReportConnectionResolver connectionResolver,
            ILogger<RegIdReportSourceResolver> logger)
        {
            _parentResolver = parentResolver;
            _httpContextAccessor = httpContextAccessor;
            _connectionResolver = connectionResolver;
            _logger = logger;
        }

        public ReportSource Resolve(
            string report,
            OperationOrigin operationOrigin,
            IDictionary<string, object> currentParameterValues)
        {
            var reportSource = _parentResolver.Resolve(report, operationOrigin, currentParameterValues);
            if (reportSource == null)
                return null!;

            var regId = _connectionResolver.ResolveRegId(
                _httpContextAccessor.HttpContext?.Request.Headers,
                currentParameterValues);

            var connectionStringName = _connectionResolver.ResolveConnectionStringName(regId);

            var connectionInfo = _connectionResolver.DescribeConnection(
                _httpContextAccessor.HttpContext?.Request.Headers,
                currentParameterValues);

            _logger.LogInformation(
                "Report {Report} using connection name {ConnectionName} -> {DataSource}/{Catalog} (RegId header: {HeaderRegId}, RegId used: {RegIdUsed})",
                report,
                connectionInfo.ConnectionStringName,
                connectionInfo.DataSource ?? "?",
                connectionInfo.InitialCatalog ?? "?",
                connectionInfo.RegIdFromHeader ?? "(none)",
                connectionInfo.RegIdUsed ?? "(none)");

            if (string.IsNullOrEmpty(connectionStringName))
                return reportSource;

            if (reportSource is InstanceReportSource instanceReportSource)
                ApplyConnectionStringName(instanceReportSource.ReportDocument, connectionStringName);

            return reportSource;
        }

        private static void ApplyConnectionStringName(IReportDocument reportDocument, string connectionStringName)
        {
            switch (reportDocument)
            {
                case Report report:
                    SetSqlDataSourceConnectionStringNames(report, connectionStringName);
                    break;
                case ReportBook reportBook:
                    foreach (var bookReport in reportBook.Reports)
                        SetSqlDataSourceConnectionStringNames(bookReport, connectionStringName);
                    break;
            }
        }

        private static void SetSqlDataSourceConnectionStringNames(Report report, string connectionStringName)
        {
            foreach (var sqlDataSource in report.GetDataSources().OfType<SqlDataSource>())
                sqlDataSource.ConnectionString = connectionStringName;
        }
    }
}
