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
        public DbSet<TestType> Tests { get; set; }                                
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
        public DbSet<GeneCode> GeneCode { get; set; }
        public DbSet<BloodForm> BloodForm { get; set; }
        public DbSet<SampleTypes> SampleTypes { get; set; }
        public DbSet<SampleRequirements> SampleRequirements { get; set; }
        public DbSet<UntestedVHRGroup> UntestedVHRGroup { get; set; }
    }
}
