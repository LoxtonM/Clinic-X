using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace ClinicX.Meta
{
    interface IMiscData
    {
        public int GetClinicalNoteID(int refID);
        public int GetRiskID(int refID);
        public int GetNoteIDFromHPOTerm(int id);
    }
    public class MiscData : IMiscData //the MiscData class contains all data "get" methods where the data is a single variable rather than a data model.
                    //As such, the data is retrieved by way of a SQL "select". As such, it does not require a data context parameter.
    {
        private readonly IConfiguration _config;
        private readonly SqlConnection conn;

        public MiscData(IConfiguration config)
        {           
            _config = config;
            conn = new SqlConnection(_config.GetConnectionString("ConString"));
        }

        public int GetClinicalNoteID(int refID)
        {
            int noteID;            
            conn.Open();
            SqlCommand cmd = new SqlCommand("select top 1 clinicalnoteid from clinicalnotes " +
                "where refid = " + refID.ToString() + " order by createddate desc, createdtime desc", conn);

            noteID = (int)(cmd.ExecuteScalar());

            conn.Close();
            return noteID;        
        }

        public int GetRiskID(int refID)
        {
            int riskID;
            conn.Open();
            SqlCommand cmd = new SqlCommand("select top 1 RiskID from PatientRisk " +
                "where RefID = " + refID.ToString() + " order by RiskID desc", conn);

            riskID = (int)(cmd.ExecuteScalar());

            conn.Close();

            return riskID;
        }

        public int GetNoteIDFromHPOTerm(int id)
        {            
            conn.Open();

            SqlCommand cmd = new SqlCommand("select ClinicalNoteID from ClinicalNotesHPOTerm where ID=" + id, conn);
            
            int noteID = 0;
            SqlDataReader reader = cmd.ExecuteReader();
            
            while (reader.Read())
            {
                noteID = reader.GetInt32(0);
            }
            
            conn.Close();
            return noteID;            
        }
    }
}
