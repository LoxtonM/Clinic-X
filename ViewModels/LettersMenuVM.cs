using ClinicalXPDataConnections.Models;
using ClinicX.Models;

namespace ClinicX.ViewModels
{
    public class LettersMenuVM
    {
        public List<Document> docsListStandard {  get; set; }
        public List<Document> docsListMedRec { get; set; }
        public List<Document> docsListDNA { get; set; }
        public List<Document> docsListOutcome { get; set; }
        public List<DocumentsContent> docContentList { get; set; }
        public Patient patient {  get; set; }
        public Referral referral { get; set; }
        public List<Leaflet> leaflets { get; set; }
        public List<ExternalCliniciansAndFacilities> externalClinicians { get; set; }
        public List<ExternalCliniciansAndFacilities> clinicianList { get; set; }
        public List<ExternalCliniciansAndFacilities> histoList { get; set; }
        public List<ExternalCliniciansAndFacilities> breastList { get; set; }
        public List<ExternalCliniciansAndFacilities> geneticsList { get; set; }
        public ExternalCliniciansAndFacilities patGP { get; set; }
    }
}
