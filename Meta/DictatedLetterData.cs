using ClinicX.Data;
using ClinicX.Models;
using System.Data;

namespace ClinicX.Meta
{
    interface IDictatedLetterData
    {
        public List<DictatedLetters> GetDictatedLettersList(string staffcode);
        public DictatedLetters GetDictatedLetterDetails(int dotID);
        public List<DictatedLettersPatients> GetDictatedLettersPatientsList(int dotID);
        public List<DictatedLettersCopies> GetDictatedLettersCopiesList(int dotID);
        public DictatedLettersCopies GetDictatedLetterCopyDetails(int id);
        public List<Patients> GetDictatedLetterPatientsList(int dotID);
    }
    public class DictatedLetterData : IDictatedLetterData
    {
        private readonly ClinicalContext _clinContext;

        public DictatedLetterData(ClinicalContext context)
        {
            _clinContext = context;
        }
               

        public List<DictatedLetters> GetDictatedLettersList(string staffcode)
        {
            var letters = from l in _clinContext.DictatedLetters
                          where l.LetterFromCode == staffcode && l.MPI != null && l.RefID != null && l.Status != "Printed"
                          orderby l.DateDictated descending
                          select l;

            return letters.ToList();
        }
        
        public DictatedLetters GetDictatedLetterDetails(int dotID) //Get details of DOT letter by its DotID
        {
            var letter = _clinContext.DictatedLetters.FirstOrDefault(l => l.DoTID == dotID);

            return letter;
        }

        public List<DictatedLettersPatients> GetDictatedLettersPatientsList(int dotID) //Get list of patients added to a DOT letter by the DotID
        {
            var patient = from p in _clinContext.DictatedLettersPatients
                          where p.DOTID == dotID
                          select p;

            List<DictatedLettersPatients> patients = new List<DictatedLettersPatients>();

            foreach (var p in patient)
            {
                patients.Add(new DictatedLettersPatients() { DOTID = p.DOTID });
            }

            return patients;
        }

        public List<DictatedLettersCopies> GetDictatedLettersCopiesList(int dotID) //Get list of all CCs added to a DOT letter by DotID
        {
            var copies = from c in _clinContext.DictatedLettersCopies
                       where c.DotID == dotID
                       select c;            

            return copies.ToList();
        }        
        
        public DictatedLettersCopies GetDictatedLetterCopyDetails(int id)  //Get details of a CC on a letter for deletion
        {
            var letter = _clinContext.DictatedLettersCopies.FirstOrDefault(x => x.CCID == id);

            return letter;
        }

        public List<Patients> GetDictatedLetterPatientsList(int dotID) //Get list of all patients in the family that can be added to a DOT, by the DotID
        {
            var letter = _clinContext.DictatedLetters.FirstOrDefault(l => l.DoTID == dotID);
            int? mpi = letter.MPI;
            var pat = _clinContext.Patients.FirstOrDefault(p => p.MPI == mpi.GetValueOrDefault());

            var patients = from p in _clinContext.Patients
                           where p.PEDNO == pat.PEDNO
                           select p;

            return patients.ToList();
        }
    }
}
