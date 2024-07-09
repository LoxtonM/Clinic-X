using ClinicX.Data;
using ClinicX.Models;
using System.Data;

namespace ClinicX.Meta
{
    interface IDiseaseData
    {
        public List<Disease> GetDiseaseList();
        public List<Diagnosis> GetDiseaseListByPatient(int mpi);
        public List<DiseaseStatus> GetStatusList();
        public Diagnosis GetDiagnosisDetails(int id);
    }
    public class DiseaseData : IDiseaseData
    {
        private readonly ClinicalContext _clinContext;        
        public DiseaseData(ClinicalContext context)
        {
            _clinContext = context;
        }
        
        public List<Disease> GetDiseaseList() //Get list of all diseases
        {
            IQueryable<Disease> items = from i in _clinContext.Diseases
                        orderby i.DESCRIPTION
                        select i;           

            return items.ToList();
        } 

        public List<Diagnosis> GetDiseaseListByPatient(int mpi) //Get list of all diseases recorded against a patient
        {            

            IQueryable<Diagnosis> items = from i in _clinContext.Diagnosis
                        where i.MPI == mpi
                        orderby i.DESCRIPTION
                        select i;

            return items.ToList();
        }

        public List<DiseaseStatus> GetStatusList() //Get list of all possible disease statuses
        {
            IQueryable<DiseaseStatus> items = from i in _clinContext.DiseaseStatusList
                        select i;
            
            return items.ToList();
        }
        
        public Diagnosis GetDiagnosisDetails(int id) //Get details of diagnosis by the diagnosis ID
        {
            Diagnosis diagnosis = _clinContext.Diagnosis.FirstOrDefault(i => i.ID == id);

            return diagnosis;
        }  

    }
}
