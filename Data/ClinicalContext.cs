using Microsoft.EntityFrameworkCore;
using ClinicX.Models;

namespace ClinicX.Data
{
    public class ClinicalContext : DbContext
    {
        public ClinicalContext (DbContextOptions<ClinicalContext> options) :base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<ActivityItems>().ToTable(ThreadPoolBoundHandle => ThreadPoolBoundHandle.HasTrigger("TriggerName"));
            base.OnModelCreating(modelBuilder);
        }

        public DbSet<Patients> Patients { get; set; }
        public DbSet<Ethnicity> Ethnicity { get; set; }
        public DbSet<Relatives> Relatives { get; set; }    
        public DbSet<RelativesDiagnosis> RelativesDiagnoses { get; set; }
        public DbSet<Clinics> Clinics { get; set; }
        public DbSet<Referrals> Referrals { get; set; }
        public DbSet<Triages> Triages { get; set; }
        public DbSet<ICPGeneral> ICPGeneral { get; set; }
        public DbSet<ICPCancer> ICPCancer { get; set; }
        public DbSet<StaffMemberList> StaffMembers { get; set; }
        public DbSet<ActivityItems> ActivityItems { get; set; }
        public DbSet<Diary> Diary { get; set; }
        public DbSet<ClinicalNotes> ClinicalNotes { get; set; }
        public DbSet<NoteItems> NoteItems { get; set; }
        public DbSet<Diagnosis> Diagnosis { get; set; }
        public DbSet<Test> Test { get; set; }
        public DbSet<OutcomeList> Outcomes { get; set; }
        public DbSet<NoteTypeList> NoteTypes { get; set; }        
        public DbSet<HPOTerms> HPOTerms { get; set; }
        public DbSet<ClinicalNoteHPOTerms> ClinicalNoteHPOTerms { get; set; }
        public DbSet<HPOTermDetails> HPOTermDetails { get; set; }
        public DbSet<ClinicalFacilityList> ClinicalFacilities { get; set; }
        public DbSet<TestList> Tests { get; set; }
        public DbSet<DiseaseList> Diseases { get; set; }
        public DbSet<DiseaseStatusList> DiseaseStatusList { get; set; }
        public DbSet<DictatedLetters> DictatedLetters { get; set; }
        public DbSet<DictatedLettersPatients> DictatedLettersPatients { get; set; }
        public DbSet<DictatedLettersCopies> DictatedLettersCopies { get; set; }
        public DbSet<PatientPathway> PatientPathway { get; set; }
        public DbSet<Caseload> Caseload { get; set; }
        public DbSet<Alert> Alert { get; set; }
        public DbSet<Reviews> Reviews { get; set; }
        public DbSet<ICPActionsList> ICPCancerActionsList { get; set; }
        public DbSet<ICPGeneralActionsList> ICPGeneralActionsList { get; set; }
        public DbSet<ICPGeneralActionsList2> ICPGeneralActionsList2 { get; set; }
        public DbSet<Risk> Risk { get; set; }
        public DbSet<Surveillance> Surveillance { get; set; }
        public DbSet<Eligibility> Eligibility { get; set; }
        public DbSet<ExternalFacility> ExternalFacility { get; set; }
        public DbSet<ExternalClinician> ExternalClinician { get; set; }
        public DbSet<CancerReg> CancerReg { get; set; }
        public DbSet<RequestStatus> RequestStatus { get; set; }
    }
}
