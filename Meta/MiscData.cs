using ClinicX.Data;
using ClinicX.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace ClinicX.Meta
{
    public class MiscData
    {
        private readonly IConfiguration _config;
        private readonly SqlConnection conn;

        public MiscData(IConfiguration config)
        {           
            _config = config;
            conn = new SqlConnection(_config.GetConnectionString("ConString"));
        }

        public int GetClinicalNoteID(int iRefID)
        {
            int iNoteID;            
            conn.Open();
            SqlCommand cmd = new SqlCommand("select top 1 clinicalnoteid from clinicalnotes " +
                "where refid = " + iRefID.ToString() + " order by createddate desc, createdtime desc", conn);

            iNoteID = (int)(cmd.ExecuteScalar());

            conn.Close();
            return iNoteID;        
        }

        public int GetNoteIDFromHPOTerm(int iID)
        {            
            conn.Open();

            SqlCommand cmd = new SqlCommand("select ClinicalNoteID from ClinicalNotesHPOTerm where ID=" + iID, conn);
            
            int iNoteID = 0;
            SqlDataReader reader = cmd.ExecuteReader();
            
            while (reader.Read())
            {
                iNoteID = reader.GetInt32(0);
            }
            
            conn.Close();
            return iNoteID;            
        }
    }
}
