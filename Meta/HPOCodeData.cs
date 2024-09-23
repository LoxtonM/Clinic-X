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
        public List<HPOTerm> GetHPOTermsList();
        public List<HPOTermDetails> GetExistingHPOTermsList(int id);
        public List<HPOExtractVM> GetExtractedTermsList(int noteID, IConfiguration _config);
        public HPOTerm GetHPOTermByTermCode(string hpoTermCode);
        public void AddHPOTermToDatabase(string hpoTermCode, string term, string userName, IConfiguration _config);
        public void AddHPOSynonymToDatabase(int hpoTermID, string synonym, string userName, IConfiguration _config);

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
            IQueryable<HPOTermDetails> notes = from n in _clinContext.HPOTermDetails
                        where n.MPI == id
                        select n;            

            return notes.ToList();
        }

        public List<HPOTerm> GetHPOTermsList() //Get list of all possible HPO codes
        {
            IQueryable<HPOTerm> terms = from t in _clinContext.HPOTerms
                        select t;

            return terms.ToList();
        }

        public List<HPOTermDetails> GetExistingHPOTermsList(int id) //Get list of all HPO codes added to a clinical note, by the ClinicalNoteID
        {
            IQueryable<HPOTermDetails> terms = from t in _clinContext.HPOTermDetails
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

        public HPOTerm GetHPOTermByTermCode(string hpoTermCode) //Get list of all possible HPO codes
        {
            HPOTerm term = _clinContext.HPOTerms.FirstOrDefault(t => t.TermCode == hpoTermCode);

            return term;
        }

        public void AddHPOTermToDatabase(string hpoTermCode, string term, string userName, IConfiguration _config)
        {
            SqlConnection conn = new SqlConnection(_config.GetConnectionString("ConString"));
            conn.Open(); //TODO - turn this into a SP
            SqlCommand cmd = new SqlCommand($"INSERT INTO HPOTerm (Term, TermCode, CreatedDate, CreatedBy) " +
                $"values ('{term}','{hpoTermCode}','{DateTime.Now.ToString("yyyy-MM-dd")}','{userName}')", conn);
            cmd.ExecuteNonQuery();
        }

        public void AddHPOSynonymToDatabase(int hpoTermID, string synonym, string userName, IConfiguration _config)
        {
            SqlConnection conn = new SqlConnection(_config.GetConnectionString("ConString"));
            conn.Open(); //TODO - turn this into a SP
            SqlCommand cmd = new SqlCommand($"INSERT INTO HPOTermSynonym (HPOTermId, TermSynonym, CreatedDate, CreatedBy) " +
                $"values ({hpoTermID},'{synonym}','{DateTime.Now.ToString("yyyy-MM-dd")}','{userName}')", conn);
            cmd.ExecuteNonQuery();
        }
    }
}
