using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace my_report.Extensions
{
    public class CompanyRegistration
    {
        public string RegId { get; set; } = string.Empty;
        public string CompanyName { get; set; } = string.Empty;
        public string ConnectionStringName { get; set; } = string.Empty;
    }

    public class DBGetConnection
    {
        public string? GetconnectionDB(string RegId)
        {
            var regCompany = LoadRegistrations();
            return regCompany?.FirstOrDefault(b => b.RegId == RegId)?.ConnectionStringName;
        }

        public bool ValidateRegId(string RegId)
        {
            return !string.IsNullOrEmpty(GetconnectionDB(RegId));
        }

        private static IEnumerable<CompanyRegistration>? LoadRegistrations()
        {
            var regCompanyData = File.ReadAllText("regCompany.json");
            return JsonConvert.DeserializeObject<IEnumerable<CompanyRegistration>>(regCompanyData);
        }
    }
}
