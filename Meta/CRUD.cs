using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.Data;

namespace ClinicX.Meta
{
    public class CRUD
    {
        private readonly IConfiguration _config;

        public CRUD(IConfiguration config)
        {
            _config = config;
        }       

        public void CallStoredProcedure(string sType, string sOperation, int int1, int int2, int int3, 
            string string1, string string2, string string3, string text, string sLogin,
            DateTime? dDate1 = null, DateTime? dDate2 = null, bool? bool1 = false, bool? bool2 = false,
            int? int4 = 0, int? int5 = 0, int? int6 = 0, string? string4 = "", string? string5 = "", string? string6 = "")
        {   
            if (dDate1 == null)
            {
                dDate1 = DateTime.Parse("1900-01-01");
            }
            if (dDate2 == null)
            {
                dDate2 = DateTime.Parse("1900-01-01");
            }

            SqlConnection conn = new SqlConnection(_config.GetConnectionString("ConString"));
            conn.Open();
            SqlCommand cmd = new SqlCommand("dbo.sp_CXCRUD", conn);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.Add("@ItemType", SqlDbType.VarChar).Value = sType; //thing you want to create/update (Clinic, review, letter etc)
            cmd.Parameters.Add("@Operation", SqlDbType.VarChar).Value = sOperation; //task you want to perform (create, update, etc)
            cmd.Parameters.Add("@int1", SqlDbType.Int).Value = int1;
            cmd.Parameters.Add("@int2", SqlDbType.Int).Value = int2;
            cmd.Parameters.Add("@int3", SqlDbType.Int).Value = int3;
            cmd.Parameters.Add("@string1", SqlDbType.VarChar).Value = string1; //varchar 30 - for small text fields
            cmd.Parameters.Add("@string2", SqlDbType.VarChar).Value = string2; //varchar 750 - for stuff that has more text
            cmd.Parameters.Add("@string3", SqlDbType.VarChar).Value = string3; //varchar max - for those who need to write War and Peace
            cmd.Parameters.Add("@text", SqlDbType.VarChar).Value = text; //varchar max - to be used for long text
            cmd.Parameters.Add("@login", SqlDbType.VarChar).Value = sLogin; //login name of user
            //Optional parameters - will pass default value to SQL sp if not supplied
            cmd.Parameters.Add("@date1", SqlDbType.DateTime).Value = dDate1;
            cmd.Parameters.Add("@date2", SqlDbType.DateTime).Value = dDate2;
            cmd.Parameters.Add("@bool1", SqlDbType.VarChar).Value = bool1;
            cmd.Parameters.Add("@bool2", SqlDbType.VarChar).Value = bool2;
            cmd.Parameters.Add("@int4", SqlDbType.Int).Value = int4;
            cmd.Parameters.Add("@int5", SqlDbType.Int).Value = int5;
            cmd.Parameters.Add("@int6", SqlDbType.Int).Value = int6;
            cmd.Parameters.Add("@string4", SqlDbType.VarChar).Value = string4;
            cmd.Parameters.Add("@string5", SqlDbType.VarChar).Value = string5;
            cmd.Parameters.Add("@string6", SqlDbType.VarChar).Value = string6;

            cmd.ExecuteNonQuery();
            conn.Close();           
        }

        
    }
}
