using ClinicX.Data;
using ClinicX.Models;
using ClinicX.ViewModels;
using Microsoft.Data.SqlClient;
using System.Data;

namespace ClinicX.Meta
{
    interface IHPOCodeData
    {
        public List<HPOTermDetails> GetHPOTermsAddedList(int id);
        public List<HPOTerms> GetHPOTermsList();
        public List<HPOTermDetails> GetExistingHPOTermsList(int id);
        public List<HPOExtractVM> GetExtractedTermsList(int noteID, IConfiguration _config);

    }
    public class HPOCodeData : IHPOCodeData
    {
        private readonly ClinicalContext _clinContext;

        public HPOCodeData(ClinicalContext context)
        {
            _clinContext = context;
        }
        
        public List<HPOTermDetails> GetHPOTermsAddedList(int id) //Get list of HPO codes added to a patient by MPI
        {
            var notes = from n in _clinContext.HPOTermDetails
                        where n.MPI == id
                        select n;            

            return notes.ToList();
        }

        public List<HPOTerms> GetHPOTermsList() //Get list of all possible HPO codes
        {
            var terms = from t in _clinContext.HPOTerms
                        select t;

            return terms.ToList();
        }

        public List<HPOTermDetails> GetExistingHPOTermsList(int id) //Get list of all HPO codes added to a clinical note, by the ClinicalNoteID
        {
            var terms = from t in _clinContext.HPOTermDetails
                        where t.ClinicalNoteID == id
                        select t;

            return terms.ToList();
        }

        public List<HPOExtractVM> GetExtractedTermsList(int noteID, IConfiguration _config) //Get list of HPO codes that can be extracted from a clinical note, by ClinicalNoteID
        {
            var model = new List<HPOExtractVM>();

            SqlConnection conn = new SqlConnection(_config.GetConnectionString("ConString"));
            conn.Open();
            SqlCommand cmd = new SqlCommand("dbo.sp_CXGetNewMatchingTermsForClinicalNote", conn);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.Add("@ClinicalNoteId", SqlDbType.Int).Value = noteID;


            using (var reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    // add items in the list
                    model.Add(new HPOExtractVM()
                    {
                        HPOTermID = reader.GetInt32(0),
                        TermCode = reader.GetString(1),
                        Term = reader.GetString(2)
                    });
                }
            }

            conn.Close();


            return (model);
        }
    }
}
