using ClinicX.Data;
using ClinicX.Models;
using System.Data;

namespace ClinicX.Meta
{
    interface IDiseaseData
    {
        public List<DiseaseList> GetDiseaseList();
        public List<Diagnosis> GetDiseaseListByPatient(int mpi);
        public List<DiseaseStatusList> GetStatusList();
        public Diagnosis GetDiagnosisDetails(int id);
    }
    public class DiseaseData : IDiseaseData
    {
        private readonly ClinicalContext _clinContext;        
        public DiseaseData(ClinicalContext context)
        {
            _clinContext = context;
        }
        
        public List<DiseaseList> GetDiseaseList() //Get list of all diseases
        {
            var items = from i in _clinContext.Diseases
                        orderby i.DESCRIPTION
                        select i;           

            return items.ToList();
        } 

        public List<Diagnosis> GetDiseaseListByPatient(int mpi) //Get list of all diseases recorded against a patient
        {            

            var items = from i in _clinContext.Diagnosis
                        where i.MPI == mpi
                        orderby i.DESCRIPTION
                        select i;

            return items.ToList();
        }

        public List<DiseaseStatusList> GetStatusList() //Get list of all possible disease statuses
        {
            var items = from i in _clinContext.DiseaseStatusList
                        select i;
            
            return items.ToList();
        }
        
        public Diagnosis GetDiagnosisDetails(int id) //Get details of diagnosis by the diagnosis ID
        {
            var diagnosis = _clinContext.Diagnosis.FirstOrDefault(i => i.ID == id);

            return diagnosis;
        }  

    }
}
