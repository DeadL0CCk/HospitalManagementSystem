using System.Configuration;
using System.Data.SqlClient;

namespace HospitalManagementSystem.DAL
{
    public static class DbHelper
    {
        public static string GetConnectionString()
        {
            var cs = ConfigurationManager.ConnectionStrings["HospitalDB"];
            if (cs == null)
                throw new System.Exception("Connection string 'HospitalDB' not found in App.config");
            return cs.ConnectionString;
        }
    }
}