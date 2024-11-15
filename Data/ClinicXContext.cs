using Microsoft.EntityFrameworkCore;
using ClinicX.Models;

namespace ClinicX.Data
{
    public class ClinicXContext : DbContext 
    {
        public ClinicXContext (DbContextOptions<ClinicXContext> options) :base(options) { }
                
        public DbSet<Note> ClinicalNotes { get; set; }
        public DbSet<NoteItem> NoteItems { get; set; }
        public DbSet<Test> Test { get; set; }        
        public DbSet<ClinicalNoteType> NoteTypes { get; set; }        
        public DbSet<HPOTerm> HPOTerms { get; set; }
        public DbSet<ClinicalNoteHPOTerms> ClinicalNoteHPOTerms { get; set; }
        public DbSet<HPOTermDetails> HPOTermDetails { get; set; }        
        public DbSet<TestType> Tests { get; set; }        
        public DbSet<DictatedLetter> DictatedLetters { get; set; }
        public DbSet<DictatedLettersPatient> DictatedLettersPatients { get; set; }
        public DbSet<DictatedLettersCopy> DictatedLettersCopies { get; set; }        
        public DbSet<Caseload> Caseload { get; set; }        
        public DbSet<ICPAction> ICPCancerActionsList { get; set; }
        public DbSet<ICPGeneralAction> ICPGeneralActionsList { get; set; }
        public DbSet<ICPGeneralAction2> ICPGeneralActionsList2 { get; set; }
        public DbSet <ICPCancerReviewAction> ICPCancerReviewActionsList { get; set; }
        public DbSet<Risk> Risk { get; set; }
        public DbSet<Surveillance> Surveillance { get; set; }
        public DbSet<Eligibility> Eligibility { get; set; }        
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
        public DbSet<GeneChange> GeneChange { get; set; }        
        public DbSet<ScreeningService> ScreeningService { get; set; }
        public DbSet<ScreeningServiceGPCode> ScreeningServiceGPCode { get; set; }
        public DbSet<BreastSurgeryHistory> BreastSurgeryHistory { get; set; }        
    }
}
