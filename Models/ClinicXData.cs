using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ClinicX.Models
{
    [Table("ViewPatientClinicalNoteDetails", Schema = "dbo" )] //Clinical notes (including patient data)
    public class Note
    {
        [Key]
        public int? ClinicalNoteID { get; set; }
        public int? RefID { get; set; }
        public int? MPI { get; set; }
        public string? ClinicalNote { get; set; }
        public int? CN_DCTM_sts { get; set; }
        [Display(Name = "Created Date")]
        [DataType(DataType.Date)]
        public DateTime? CreatedDate { get; set; }
        [Display(Name = "Created Time")]
        [DataType(DataType.Time)]
        public DateTime? CreatedTime { get; set; }
        public string? FIRSTNAME { get; set; }
        public string? LASTNAME { get; set; }
        public string? CGU_No { get; set; }
        public string? CreatedBy { get; set; }
        public string? NoteType { get; set; }
    }

    [Table("ClinicalNotes", Schema = "dbo")] //Clinical notes (just the note data)
    public class NoteItem
    {
        [Key]
        public int ClinicalNoteID { get; set; }
        public int? RefID { get; set; }
        public int? MPI { get; set; }
        public string? ClinicalNote { get; set; }
        public int CN_DCTM_sts { get; set; }        
        public string? NoteType { get; set; }
    }    

    [Table("ViewPatientTestDetails", Schema = "dbo")] //Patients' tests requested
    public class Test
    {
        [Key]
        public int TestID { get; set; }
        public int MPI { get; set; }
        public string CGU_No { get; set; }
        public string? Title { get; set; }
        public string? FIRSTNAME { get; set; }
        public string? LASTNAME { get; set; }
        public string? TEST { get; set; }
        public string? ORDEREDBY { get; set; }
        public string? LOCATION { get; set; }
        public string? NAME { get; set; }
        [Display(Name = "Requested Date")]
        [DataType(DataType.Date)]
        public DateTime? DATE_REQUESTED { get; set; }
        public DateTime? StandardTAT { get; set; }
        [Display(Name = "Expected Date")]
        [DataType(DataType.Date)]
        public DateTime? ExpectedDate { get; set; }
        [Display(Name = "Received Date")]
        [DataType(DataType.Date)]
        public DateTime? DATE_RECEIVED { get; set; }
        public string? RESULT { get; set; }
        [Display(Name = "Given to Patient")]
        [DataType(DataType.Date)]
        public DateTime? ResultGivenDate { get; set; }
        public string COMPLETE { get; set; }
        public string? COMMENTS { get; set; }
    }    

    [Table("ClinicalNoteTypes", Schema = "dbo")]//List of types of clinical note
    public class  ClinicalNoteType
    {
        [Key]
        public int NoteTypeID { get; set; }
        public string NoteType { get; set; }
        public bool NoteInUse { get; set; }
    }
        

    [Table("PAT_TESTTYPE", Schema = "dbo")] //List of all tests
    public class TestType
    {
        [Key]
        public string TEST { get; set; }
        public Int16? T_O { get; set; }
    }        
    
        

    [Table("ListRisk", Schema = "dbo")]
    public class RiskCodes
    {
        [Key]
        public string RiskCode { get; set; }
        public string Risk { get; set; }
        //public int? RiskOrder { get; set; }
    }

    [Table("ListSurvSite", Schema = "dbo")]
    public class SurvSiteCodes
    {
        [Key]
        public string SurvSiteCode { get; set; }
        public string SurvSite { get; set; }
    }

    [Table("ListSurvType", Schema = "dbo")]
    public class SurvTypeCodes
    {
        [Key]
        public string SurvTypeCode { get; set; }
        public string SurvType { get; set; }
    }

    [Table("ListSurvFreq", Schema = "dbo")]
    public class  SurvFreqCodes
    {
        [Key]
        public string SurvFreqCode { get; set; }
        public string SurvFreq { get; set; }
    }

    [Table("ListSurvDiscReason", Schema = "dbo")]
    public class DiscontinuedReasonCodes
    {
        [Key]
        public string SurvDiscReasonCode { get; set; }
        public string SurvDiscReason { get; set; }
    }
    
    [Keyless]
    [Table("ListCalculationTools", Schema = "dbo")]
    public class  CalculationTool
    {
        public string CalculationToolCode { get; set; }
    }

    [Table("ListICPCancerRequests", Schema = "dbo")]
    public class CancerRequests
    {
        [Key]
        public int ID { get; set; }
        public string Action { get; set; }
        public bool InUse { get; set; }
        public int? DocContentID { get; set; }
        public int? DocContentID2 { get; set; }
        public int? DocContentID3 { get; set; }
    }

    [Table("ListSurvRecGeneChange", Schema = "dbo")]
    public class GeneChange
    {
        [Key]
        public int GeneChangeID { get; set; }
        public string GeneChangeDescription { get; set; }
        public bool Inuse { get; set; }
    }

    [Table("ListScreeningServiceDetails", Schema = "dbo")]
    public class ScreeningService
    {
        [Key]
        public string ScreeningOfficeCode { get; set; }
        public string BreastScreeningService { get; set; }
        public string Contact {  get; set; }
        public string Telephone { get; set; }
        public string Add1 { get; set; }
        public string Add2 { get; set; }
        public string Add3 { get; set; }
        public string Add4 { get; set; }
        public string? Add5 { get; set; }
        public string? Add6 { get; set; }
        public string? Add7 { get; set; }
        public string? Add8 { get; set; }
        public string? Add9 { get; set; }
        public string? Add10 { get; set; }
    }

    [Table("ListScreeningServiceGPCodes", Schema = "dbo")]
    public class ScreeningServiceGPCode
    {
        [Key]
        public string GPCode { get; set; }
        public string ScreeningOfficeCode { get; set; }        
    }

    [Table("PatientBreastSurgeryImplantsHistory", Schema = "dbo")]
    public class BreastSurgeryHistory
    {
        [Key]
        public int ID { get; set; }
        public int MPI { get; set; }
        public int BreastCancerHistory { get; set; }
        public int BreastTissueRight { get; set; }
        public int BreastTissueLeft { get; set; }
        public int ImplantsRight { get; set; }
        public int ImplantsLeft { get; set; }
    }

    [Table("GeneticTestingEligibilityCodes", Schema = "dbo")]
    public class GeneCode
    {
        [Key]
        public int ID { get; set; }
        public string TestCode { get; set; }
        public string TestType { get; set; }
        public bool InUse { get; set; }
    }

    [Table("PatientBloodFormDetails", Schema = "dbo")]
    public class  BloodForm
    {
        [Key]
        public int BloodFormID { get; set; }
        public int TestID { get; set; }
        public bool IsNHS { get; set; }
        public bool IsPrivate { get; set; }
        public bool IsOutpatient { get; set; }
        public bool IsInpatient { get; set; }
        public bool IsUrgent { get; set; }
        public bool IsRoutine { get; set; }
        public DateTime? DateOfNextAppointment { get; set; }
        public string? ClinicalDetails { get; set; }
        public string? TestingRequirements { get; set; }
        public bool IsPrenatal { get; set; }
        public bool IsPresymptomatic { get; set; }
        public bool IsDiagnostic { get; set; }
        public bool IsCarrier { get; set; }
        public string? PrenatalType { get; set; }
        public int? PrenatalGestation { get; set; }
        public string? PrenatalRisk { get; set; }
        public bool IsVenousBlood { get; set; }
        public bool IsSaliva { get; set; }
        public bool IsBuccalSwab { get; set; }
        public bool IsCVS { get; set; }
        public bool IsAmnioticFluid { get; set; }
        public bool IsTissue { get; set; }
        public bool IsCordBlood { get; set; }
        public bool IsFetalBlood { get; set; }
        public string? RelativeName { get; set; }
        public DateTime? RelativeDOB { get; set; }
        public string? RelativeNo { get; set; }
        public string? RelativeDetails { get; set; }
        public string? SampleRequirements { get; set; }

    }

    [Table("ListSampleType", Schema = "dbo")]
    public class SampleTypes
    {
        [Key]
        public string SampleTypeCode { get; set; }
        public string SampleType { get; set; }
    }

    [Table("ListSampleRequirements", Schema = "dbo")]
    public class SampleRequirements
    {
        [Key] public int ID { get; set; }
        public string SampleRequirement { get; set; }
    }

    [Table("PatientRelativesRisk", Schema = "dbo")]
    public class UntestedVHRGroup
    {
        [Key]
        public int RelativeRiskID { get; set; }
        public int? RefID { get; set; }
        public bool FirstDegreeBelow30 { get; set; }
        public bool FirstDegreeBelow50 { get; set; }
        public bool OtherUntested { get; set; }
        public bool CauseNotIdentified { get; set; }
        public int? ThresholdAge8pct { get; set; }
    }

    [Table("Staff_Options", Schema = "dbo")]
    public class StaffOptions
    {
        [Key]
        public int ID { get; set; }
        public string Staff_Code { get; set; }
        public bool Automated_GEMR_Open { get; set; }
        public string GEMR_browser { get; set; }
    }
}
