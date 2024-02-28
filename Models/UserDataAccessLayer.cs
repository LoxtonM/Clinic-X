using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Configuration;

namespace ClinicX.Models
{
    public class UserDataAccessLayer : Controller //used for user login, has no function beyond this.
    {
        public static IConfiguration Configuration { get; set; }
        private static string GetConnectionString()
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json");

            Configuration = builder.Build();
            string connectionString = Configuration["ConnectionStrings:ConString"];
           
            return connectionString;
        }

        string connectionString = GetConnectionString();


        public string ValidateLogin(UserDetails user)
        {
            using (SqlConnection con = new SqlConnection(connectionString))
            {
                SqlCommand cmd = new SqlCommand("sp_CXValidateUserLogin", con);
                cmd.CommandType = System.Data.CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@LoginID", user.EMPLOYEE_NUMBER);
                cmd.Parameters.AddWithValue("@LoginPassword", user.PASSWORD);
                
                con.Open();
                string result = cmd.ExecuteScalar().ToString();
                con.Close();
                
                return result;
            }
        }
    }
}
