using ClinicX.Data;
using ClinicX.Models;
using System.Data;

namespace ClinicX.Meta
{
    interface IDiaryData
    {
        public List<Diary> GetDiaryList(int id);
    }
    public class DiaryData : IDiaryData
    {
        private readonly ClinicalContext _clinContext;

        public DiaryData(ClinicalContext context)
        {
            _clinContext = context;
        }
        
        public List<Diary> GetDiaryList(int id) //Get list of diary entries for patient by MPI
        {
            var pat = _clinContext.Patients.FirstOrDefault(p => p.MPI == id);

            var diary = from d in _clinContext.Diary
                        where d.WMFACSID == pat.WMFACSID
                        orderby d.DiaryDate
                        select d;

            return diary.ToList();
        }        
    }
}
