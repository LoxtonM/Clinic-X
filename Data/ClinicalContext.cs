using Microsoft.EntityFrameworkCore;
using ClinicX.Models;

namespace ClinicX.Data
{
    public class ClinicalContext : DbContext //The CLinicalContext class is the data context for all clinical related data.
    {
        public ClinicalContext (DbContextOptions<ClinicalContext> options) :base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<ActivityItem>().ToTable(ThreadPoolBoundHandle => ThreadPoolBoundHandle.HasTrigger("TriggerName"));
            base.OnModelCreating(modelBuilder);
        }

        public DbSet<Patient> Patients { get; set; }
        public DbSet<Ethnicity> Ethnicity { get; set; }
        public DbSet<Relative> Relatives { get; set; }    
        public DbSet<RelativesDiagnosis> RelativesDiagnoses { get; set; }
        public DbSet<Appointment> Clinics { get; set; }
        public DbSet<Referral> Referrals { get; set; }
        public DbSet<Triage> Triages { get; set; }
        public DbSet<ICP> ICP { get; set; }
        public DbSet<ICPGeneral> ICPGeneral { get; set; }
        public DbSet<ICPCancer> ICPCancer { get; set; }
        public DbSet<StaffMember> StaffMembers { get; set; }
        public DbSet<ActivityItem> ActivityItems { get; set; }
        public DbSet<Diary> Diary { get; set; }
        public DbSet<Note> ClinicalNotes { get; set; }
        public DbSet<NoteItem> NoteItems { get; set; }
        public DbSet<Diagnosis> Diagnosis { get; set; }
        public DbSet<Test> Test { get; set; }
        public DbSet<Outcome> Outcomes { get; set; }
        public DbSet<ClinicalNoteType> NoteTypes { get; set; }        
        public DbSet<HPOTerm> HPOTerms { get; set; }
        public DbSet<ClinicalNoteHPOTerms> ClinicalNoteHPOTerms { get; set; }
        public DbSet<HPOTermDetails> HPOTermDetails { get; set; }
        public DbSet<ClinicalFacility> ClinicalFacilities { get; set; }
        public DbSet<TestType> Tests { get; set; }
        public DbSet<Disease> Diseases { get; set; }
        public DbSet<DiseaseStatus> DiseaseStatusList { get; set; }
        public DbSet<DictatedLetter> DictatedLetters { get; set; }
        public DbSet<DictatedLettersPatient> DictatedLettersPatients { get; set; }
        public DbSet<DictatedLettersCopy> DictatedLettersCopies { get; set; }
        public DbSet<PatientPathway> PatientPathway { get; set; }
        public DbSet<Caseload> Caseload { get; set; }
        public DbSet<Alert> Alert { get; set; }
        public DbSet<Review> Reviews { get; set; }
        public DbSet<ICPAction> ICPCancerActionsList { get; set; }
        public DbSet<ICPGeneralAction> ICPGeneralActionsList { get; set; }
        public DbSet<ICPGeneralAction2> ICPGeneralActionsList2 { get; set; }
        public DbSet <ICPCancerReviewAction> ICPCancerReviewActionsList { get; set; }
        public DbSet<Risk> Risk { get; set; }
        public DbSet<Surveillance> Surveillance { get; set; }
        public DbSet<Eligibility> Eligibility { get; set; }
        public DbSet<ExternalFacility> ExternalFacility { get; set; }
        public DbSet<ExternalClinician> ExternalClinician { get; set; }
        public DbSet<CancerReg> CancerReg { get; set; }
        public DbSet<RequestStatus> RequestStatus { get; set; }
        public DbSet<TumourSite> TumourSite { get; set; }
        public DbSet<TumourLat> TumourLat { get; set; }
        public DbSet<TumourMorph> TumourMorph { get; set; }
        public DbSet<RiskCodes> RiskCodes { get; set; }
        public DbSet<SurvSiteCodes> SurvSiteCodes { get; set ; }
        public DbSet<SurvTypeCodes> SurvTypeCodes { get; set; }
        public DbSet<SurvFreqCodes> SurvFreqCodes { get; set; }
        public DbSet<DiscontinuedReasonCodes> DiscontinuedReasonCodes { get; set; }
        public DbSet<CalculationTool> CalculationTools { get; set; }
        public DbSet<CancerRequests> CancerRequests { get; set; }
        public DbSet<Relation> Relations { get; set; }
        public DbSet<Gender> Genders { get; set; }
        public DbSet<Pathway> Pathways { get; set; }
        public DbSet<Priority> Priority { get; set; }
        public DbSet<GeneChange> GeneChange { get; set; }
        public DbSet<Notification> Notifications { get; set; }
        public DbSet<ScreeningService> ScreeningService { get; set; }
        public DbSet<ScreeningServiceGPCode> ScreeningServiceGPCode { get; set; }
        public DbSet<BreastSurgeryHistory> BreastSurgeryHistory { get; set; }
        public DbSet<Constant> Constants { get; set; }
        public DbSet<ExternalCliniciansAndFacilities> ExternalCliniciansAndFacilities { get; set; }
    }
}
