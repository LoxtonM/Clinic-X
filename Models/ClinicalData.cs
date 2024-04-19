using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Client;
using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.Intrinsics.X86;

namespace ClinicX.Models
{
    [Table("ViewPatientDemographicDetails", Schema="dbo")] //Patient demographic data
    public class Patients
    {
        [Key]
        public int MPI { get; set; }
        public int INTID { get; set; }
        public int WMFACSID { get; set; }
        public string? Title { get; set; }
        [Display(Name ="Date of Birth")]
        [DataType(DataType.Date)]
        public DateTime? DOB { get; set; }
        [Display(Name = "Forename")]
        public string? FIRSTNAME { get; set; }
        [Display(Name = "Surname")]
        public string? LASTNAME { get; set; }
        [Display(Name = "CGU Number")]
        public string? CGU_No { get; set; }
        public string? PEDNO { get; set; }
        [Display(Name = "NHS Number")]
        public string? SOCIAL_SECURITY { get; set; }
        public string? ADDRESS1 { get; set; }
        public string? ADDRESS2 { get; set; }
        public string? ADDRESS3 { get; set; }
        public string? ADDRESS4 { get; set; }
        public string? POSTCODE { get; set; }
        public string? TEL { get; set; }
        public string? PtTelMobile { get; set; }               
        public string? EmailCommsConsent { get; set; }
        public string? EmailAddress { get; set; }
        public string? PrimaryLanguage { get; set; }
        public string? IsInterpreterReqd { get; set; }
        public string? GP { get; set; }
        public string? GP_Code { get; set; }
        public string? GP_Facility { get; set; }
        public string? GP_Facility_Code { get; set; }
        public string? PtAKA { get; set; }
        public string? PtLetterAddressee { get; set; }
        public string? SALUTATION { get; set; }
        public string? Ethnic { get; set; }
        public string? EthnicCode { get; set; }
        public string? DCTM_Folder_ID { get; set; }
    }   

    [Table("ViewPatientRelativeDetails", Schema = "dbo")] //Patients' relatives
    public class Relatives
    {
        [Key]
        public int relsid { get; set; }
        public int WMFACSID { get; set; }
        public string? Name { get; set; }
        public string? RelTitle { get; set; }
        public string? RelSurname { get; set; }
        public string? RelSurnameBirth { get; set; }
        public string? RelForename1 { get; set; }
        public string? RelForename2 { get; set; }
        public string? RelAKA { get; set; }
        [Display(Name = "Date of Birth")]
        [DataType(DataType.Date)]
        public DateTime? DOB { get; set; }
        [Display(Name = "Date of Death")]
        [DataType(DataType.Date)]
        public DateTime? DOD { get; set; }
        public string? RelAffected { get; set; }
        public string? RelAlive { get; set; }
        public string? Sex { get; set; }
        public string? Diagnosis { get; set; }
        public string? Status { get; set; }
        public string? SiteCode { get; set; }
        public string? Relation { get; set; }
    }

    [Table("RelativesDiagnosis")]
    public class RelativesDiagnosis
    {
        [Key]
        public int TumourID { get; set; }
        public int RelsID { get; set; }
        //public int WMFACSID { get; set; }
        public string? Diagnosis {  get; set; }
        public string? AgeDiag {  get; set; }
        public string? Hospital { get; set; }
        public string? CRegCode { get; set; }
        [Column("Consent?")] //because some silly person named the column in the SQL table with a question mark!!
        public string? Consent { get; set; }
        public string? Confirmed { get; set; }
        public string? ConfDiagDate { get; set; }
        public Double? ConfDiagAge { get; set; }
        public string? SiteCode { get; set; }
        public string? LatCode { get; set; }
        public string? MorphCode { get; set; }
        public string? Status { get; set; }
        [DataType(DataType.Date)]
        public DateTime? DateReq { get; set; }
        [DataType(DataType.Date)]
        public DateTime? DateRec { get; set; }
        public string? Cons {  get; set; }
        public string? ReqBy { get; set; }
        public string? HistologyNumber { get; set; }
        public string? Grade {  get; set; }
        public string? Dukes { get; set; }
        public string? Notes {  get; set; }
    }

    [Table("ViewPatientAppointmentDetails", Schema="dbo")] //Appointment data
    public class Clinics
    {
        [Key]
        public int RefID { get; set; }
        public int MPI { get; set; }
        [Display(Name = "Booked Date")]
        [DataType(DataType.Date)]
        public DateTime? BOOKED_DATE { get; set; }
        [Display(Name = "Booked Time")]
        [DataType(DataType.Time)]
        public DateTime? BOOKED_TIME { get; set; }
        [Display(Name ="Appt With")]
        public string STAFF_CODE_1 { get; set; }
        public string AppType { get; set; }
        public string? Attendance { get; set; }
        public string Clinician { get; set; }
        public string Clinic { get; set; }
        public string FACILITY { get; set; }
        public string FIRSTNAME { get; set; }
        public string LASTNAME { get; set; }
        public string CGU_No { get; set; }
        [DataType(DataType.Time)]
        public DateTime? ArrivalTime { get; set; }
        public string? SeenBy { get; set; }
        public string? SeenByClinician { get; set; }
        public Int16? NoPatientsSeen { get; set; }
        public Int16? Duration { get; set; }
        public bool? isClockStop { get; set; }
        public string? LetterRequired { get; set; }
        public string LoginDetails { get; set; }
        public string? Notes { get; set; }
        public string? IndicationNotes { get; set; }
        public int ReferralRefID { get; set; }
    }

    [Table("ViewPatientReferralDetails", Schema ="dbo")] //Referral data
    public class Referrals
    {
        [Key]
        public int refid { get; set; }
        public int MPI { get; set; }
        public string CLINICNO { get; set; }
        public string? INDICATION { get; set; }
        public string? LeadClinician { get; set; }
        public string? GC { get; set; }
        public string? AdminContact { get; set; }
        public string? ReferringClinician { get; set; }
        public string? ReferrerCode { get; set; }
        public string? ReferringFacility { get; set; }
        public string? ReferringFacilityCode { get; set; }
        [DataType(DataType.Date)]
        public DateTime? RefDate { get; set; }
        public string? RefType { get; set; }
        public string? COMPLETE { get; set; }
        public DateTime? ClockStartDate { get; set; }
        public DateTime? ClockStopDate { get; set; }
        public string? PATHWAY { get; set; }
        public string? REASON_FOR_REFERRAL { get; set; }
    }

    [Table("MasterActivityTable", Schema="dbo")] //Any activity
    public class ActivityItems
    {
        [Key]
        public int RefID { get; set; }
        public int MPI { get; set; }            
        public string? COUNSELED { get; set; }        
        public DateTime? ARRIVAL_TIME { get; set; }
        public string? SEEN_BY { get; set; }
        public Int16? NOPATIENTS_SEEN { get; set; }
        public Int16? EST_DURATION_MINS { get; set; }
        public bool? ClockStop { get; set; }
        public string? LetterReq { get; set; }
        [Display(Name = "Booked Date")]
        [DataType(DataType.Date)]
        public DateTime? BOOKED_DATE { get; set; }
        [Display(Name = "Booked Time")]
        [DataType(DataType.Time)]
        public DateTime? BOOKED_TIME { get; set; }        
        public string? STAFF_CODE_1 { get; set; }
        [Display(Name = "Appointment Type")]
        public string TYPE { get; set; }
        [Display(Name = "Clinic Venue")]
        public string? FACILITY { get; set; }
        public string? PATHWAY { get; set; }
        public string? REF_PHYS { get; set; }
        public string? REF_FAC { get; set; }
    }

    [Table("PatientDiary", Schema = "dbo")]
    public class Diary
    {
        [Key]
        public int DiaryID { get; set; }
        public int WMFACSID { get; set; }
        public DateTime? DiaryDate { get; set; }
        public string? DiaryWith { get; set; }
        public string? DiaryAction { get; set; }
        public string? DiaryText { get; set; }
        public string? DocCode { get; set; }
        public int RefID { get; set; }
    }

    [Table("ViewTriageDetails", Schema = "dbo")] //Cases to be triaged
    public class Triages
    {
        [Key]
        public int ICPID { get; set; }        
        public int RefID { get; set; }
        public int MPI { get; set; }
        public string ReferralPathway { get; set; }
        public string RefType { get; set; }
        public string CGU_No { get; set; }
        public string ConsultantCode { get; set; }
        public string GCCode { get; set; }
        public string Name { get; set; }
        public bool? GCToTriage { get; set; }
        public bool? ConsToTriage { get; set; }
        public bool? GCTriaged { get; set; }
        public bool? ConsTriaged { get; set; }        
        [DataType(DataType.Date)]
        public DateTime? RefDate { get; set; }
        public string LoginDetails { get; set; }
        public string Clinician { get; set; }
        public int? TreatPath { get; set; }
        public int? TreatPath2 { get; set; }
        public bool? ConsWLForSPR { get; set; }
        public string ConsultantName { get; set; }
        public string GCName { get; set; }
    }

    [Table("ViewPatientReviews", Schema = "dbo")] //Requested reviews
    public class Reviews
    {
        [Key]
        public int ReviewID { get; set; }
        public int MPI {  get; set; }
        public string CGU_No { get; set; }
        public string? Title { get; set; }
        public string? FIRSTNAME { get; set; }
        public string? LASTNAME { get; set; }
        public string? Category { get; set; }
        public string? Comments { get; set; }
        [DataType(DataType.Date)]
        public DateTime? Created_Date { get; set; }
        [DataType(DataType.Date)]
        public DateTime? Planned_Date { get; set; }
        public string? Owner { get; set; }
        public string? Recipient { get; set; }
        [DataType(DataType.Date)]
        public DateTime? Completed_Date { get; set; }
        public string? Review_Status { get; set; }
        public string? Review_Recipient { get; set; }
        public string? RecipientLogin { get; set; }
    }
    [Table("ICP", Schema = "dbo")]
    public class ICP
    {
        [Key]
        public int ICPID { get; set; }
        public int REFID { get; set; }
        public int MPI { get; set; }
    }

    [Table("ICP_General", Schema = "dbo")] //General ICP
    public class ICPGeneral
    {
        [Key]
        public int ICP_General_ID { get; set; }
        public int ICPID { get; set; }
        public int? TreatPath { get; set; }
        public int? TreatPath2 { get; set; }        
        public bool? ConsWLForSPR { get; set; }
    }

    [Table("ViewPatientCancerICP", Schema = "dbo")] //Cancer ICP
    public class ICPCancer
    {
        [Key]
        public int ICP_Cancer_ID { get; set; }
        public int ICPID { get; set; }
        public int MPI { get; set; }
        public string? CGU_No {  get; set; }
        public string? FIRSTNAME { get; set; }
        public string? LASTNAME { get; set; }
        public DateTime? REFERRAL_DATE { get; set; }
        //public int ICPID { get; set; }
        public int? ActOnRef { get; set; }
        public int? ReviewedOption { get; set; }
        //public string ActRefInfo { get; set; }
        public string? ActOnRefBy { get; set; }
        //public int FHFNotRet { get; set; }
        public bool FHFRev { get; set; }
        public bool PedRev { get; set; }
        public bool ConfRev { get; set; }
        public bool PathRepRev { get; set; }
        public bool RiskAssessment { get; set; }
        //public int ReviewedOption { get; set; }
        public string? FinalReviewed { get; set; }
        public string? GC_CODE { get; set; }
        public string? WaitingListClinician { get; set; }
        public string? WaitingListVenue { get; set; }
        public string? WaitingListComments { get; set; }
        public string? ReferralAction { get; set; }
        public string? Comments { get; set; }
        public string? ToBeReviewedby { get; set; }

    }

    [Table("CLIN_FACILITIES", Schema = "dbo")] //Facilities where we hold clinics
    public class ClinicalFacilityList
    {
        [Key]
        public string FACILITY { get; set; }
        public string NAME { get; set; }
        public Int16 NON_ACTIVE { get; set; }
    }

    [Table("ViewPatientClinicalNoteDetails", Schema = "dbo" )] //Clinical notes (including patient data)
    public class ClinicalNotes
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
    public class NoteItems
    {
        [Key]
        public int ClinicalNoteID { get; set; }
        public int? RefID { get; set; }
        public int? MPI { get; set; }
        public string? ClinicalNote { get; set; }
        public int CN_DCTM_sts { get; set; }        
        public string? NoteType { get; set; }
    }

    [Table("STAFF", Schema = "dbo")] //Staff members
    public class StaffMemberList
    {
        [Key]
        public string STAFF_CODE { get; set; }
        public string EMPLOYEE_NUMBER { get; set; }
        public string NAME { get; set; }
        public string? StaffTitle { get; set; }
        public string? StaffForename { get; set; }
        public string? StaffSurname { get; set; }
        public string CLINIC_SCHEDULER_GROUPS { get; set; }
        public string? BILL_ID { get; set; }
        public string? TELEPHONE { get; set; }
        public string POSITION { get; set; }
        public bool InPost { get; set; }
    }

    [Table("ViewPatientDiagnosisDetails", Schema = "dbo")] //Patients' diagnoses
    public class Diagnosis
    {
        [Key]
        public int ID { get; set; }
        public int MPI { get; set; }
        public string CGU_No { get; set; }
        public string Title { get; set; }
        public string FIRSTNAME { get; set; }
        public string LASTNAME { get; set; }
        public string? DISEASE_CODE { get; set; }
        public string? DESCRIPTION { get; set; }        
        public string? STATUS { get; set; }
        public string? MAIN_SUB { get; set; }
        public string? NAME { get; set; }
        [Display(Name = "Date Diagnosed")]
        [DataType(DataType.Date)]
        public DateTime ENTEREDDATE { get; set; }
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

    [Table("CLIN_OUTCOMES", Schema = "dbo")] //List of possible outcomes for appointments
    public class OutcomeList
    {
        [Key]
        public string CLINIC_OUTCOME { get; set;}
        public string DEFAULT_CLINIC_STATUS { get; set; }
    }

    [Table("ClinicalNoteTypes", Schema = "dbo")]//List of types of clinical note
    public class  NoteTypeList
    {
        [Key]
        public int NoteTypeID { get; set; }
        public string NoteType { get; set; }
        public bool NoteInUse { get; set; }
    }

    [Table("HPOTerm", Schema = "dbo")] //List of all HPO codes
    public class HPOTerms
    {
        [Key]
        public int ID { get; set; }
        public string Term { get; set; }
        public string TermCode { get; set; }
    }

    [Table("ClinicalNotesHPOTerm", Schema = "dbo")] //HPO codes applied to a clinical note (just the IDs)
    public class ClinicalNoteHPOTerms
    {
        [Key]
        public int ID { get; set; }
        public int ClinicalNoteID { get; set; }
        public int HPOTermID { get; set; }
    }

    [Table("ViewClinicalNoteHPOTermDetails", Schema = "dbo")] //HPO codes applied to a clinical note (including MPI and HPO data)
    public class HPOTermDetails
    {
        [Key]
        public int ID { get; set; }
        public int ClinicalNoteID { get; set; }  
        public int MPI { get;set; }
        public string? Term { get; set; }
        public string? TermCode { get; set; }
    }

    [Table("DISEASE", Schema = "dbo")] //List of all diseases
    public class DiseaseList
    {
        [Key]
        public string DISEASE_CODE { get; set; }
        public string DESCRIPTION { get; set; }
    }

    [Table("PAT_TESTTYPE", Schema = "dbo")] //List of all tests
    public class TestList
    {
        [Key]
        public string TEST { get; set; }
        public Int16? T_O { get; set; }
    }

    [Table("DISEASE_STATUS", Schema = "dbo")] //List of all statuses for diagnoses
    public class  DiseaseStatusList
    {
        [Key]
        public string DISEASE_STATUS { get; set; }
    }

    [Table("View_ETHNICITY_as_ListEthnicOrigin", Schema = "dbo")] //List of ethnicities
    public class Ethnicity
    {
        [Key]
        public string EthnicCode { get; set; }
        public string Ethnic { get; set; }
        public string NHSEthnicCode { get; set; }
    }

    [Table("ListICPActions", Schema = "dbo")] //List of ICP actions (duh!)
    public class ICPActionsList
    {
        [Key]
        public int ID { get; set; }
        public string Action { get; set; }
        public bool InUse { get; set; }
    }

    [Table("ListICPGeneralActions", Schema = "dbo")] //List of treatpath actions
    public class ICPGeneralActionsList
    {
        [Key]
        public int ID { get; set; }
        public string Action { get; set; }
        public bool InUse { get; set; }
    }

    [Table("ListICPGeneralActions2", Schema = "dbo")] //List of trheatpath2 actions
    public class ICPGeneralActionsList2
    {
        [Key]
        public int ID { get; set; }
        public string Action { get; set; }
        public bool InUse { get; set; }
        public bool Clinic { get; set; }
        public bool NoClinic { get; set; }
    }

    [Table("ListICPCancerReviewActions", Schema = "dbo")]
    public class ICPCancerReviewActionsList
    {
        [Key]
        public int ID { get; set; }
        public string Action { get; set; }
        public string? description { get; set; }
        public string? DocCode { get; set; }
        public bool InUse { get; set; }
    }

    [Table("ViewPatientDictatedLetterDetails", Schema = "dbo")] //Dictated le'ahs
    public class DictatedLetters
    {
        [Key]
        public int DoTID { get; set; }
        public int MPI { get; set; }
        public string CGU_No { get; set; }
        public int? RefID { get; set; }
        public string? LetterTo { get; set; }
        public string? LetterToSalutation { get; set; }
        public string? LetterRe { get; set; }
        public string? LetterFrom { get; set; }
        public string? LetterFromCode { get; set; }
        public string? LetterContent { get; set; }
        public string? LetterContentBold { get; set; }
        public string? Status { get; set; }
        [DataType(DataType.Date)]
        public DateTime? DateDictated { get; set; }
        public DateTime? CreatedDate { get; set; }
        public string? SecTeam { get; set; }
        public string? Consultant { get; set; }
        public string? GeneticCounsellor { get; set; }
        public string? Comments { get; set; }
    }

    [Table("DictatedLettersPatients", Schema = "dbo")] //Patients added to DOT
    public class DictatedLettersPatients
    {
        [Key]
        public int DOTPID { get; set; }
        public int DOTID { get; set; }
        public int MPI { get; set; }
        public int RefID { get; set; }
    }

    [Table("DictatedLettersCopies", Schema = "dbo")] //CC copies added to DOTs
    public class DictatedLettersCopies
    {
        [Key]
        public int CCID { get; set; }
        public int DotID { get; set; }
        public string CC { get; set; }
    }

    [Table("ViewPatientPathwayAll", Schema = "dbo")] //Patient pathway overview
    [Keyless]
    public class PatientPathway
    {        
        public int MPI { get; set; }
        public string cgu_no { get; set; }
        public string firstname { get; set; }
        public string lastname { get; set; }
        public DateTime REFERRAL_DATE { get; set; }
        public int Triaged { get; set; }
        public int AppointmentBooked { get; set; }
        public int Seen { get; set; }
        public int LetterDictated { get; set; }
        public int LetterPrinted { get; set; }
        public int ReviewPlanned { get; set; }
        public string ClockStatus { get; set; }
        public string? PATHWAY { get; set; }
        public int? ToBeSeenByGC { get; set; }
        public int? ToBeSeenByCons {  get; set; }
    }

    [Table("ViewCaseloadOverview", Schema = "dbo")] //Caseload overview
    [Keyless]
    public class Caseload
    {
        public int MPI { get; set; }
        public int RecordPrimaryKey { get; set; }
        public string StaffCode { get; set; }
        public string Type  { get; set; }
        public DateTime? BookedDate { get; set; }
        public DateTime? BookedTime { get; set; }
        public string? State { get; set; }
        public string CGU_No { get; set; }
        public string Name { get; set; }
        public string Clinician { get; set; }
    }

    [Table("View_Alerts", Schema = "dbo")] //Alerts
    public class Alert
    {
        [Key]
        public int AlertID { get; set; }
        public int MPI { get; set; }
        public string CGU_No { get; set; }
        public bool ProtectedAddress { get; set; }
        public DateTime EffectiveFromDate { get; set; }
        public DateTime? EffectiveToDate { get; set; }
        public string AlertType { get; set; }
        public string Comments { get; set; }
    }

    [Table("ViewPatientRisk", Schema = "dbo")] //Cancer risk items
    public class Risk
    {
        [Key]
        public int RiskID { get; set; }
        public int RefID { get; set; }
        public int MPI { get; set; }
        public string CGU_No { get; set; }
        public string? FIRSTNAME { get; set; }
        public string? LASTNAME { get; set; }       
        [DataType(DataType.Date)]
        public DateTime? RiskDate { get; set; }
        public string? RiskCode { get; set; }
        public string? RiskClinCode { get; set; }
        public string? RiskComments { get; set; }
        public Int16? IncludeLetter { get; set; }
        public double? R25_29 { get; set; }
        public double? R30_40 { get; set; }
        public double? R40_50 { get; set; }
        public double? R50_60 { get; set; }
        public string? CalculationToolUsed { get; set; }
        public string? SurvSiteCode { get; set; }
        public double? LifetimeRiskPercentage { get; set; }
        public string? Clinician { get; set; }
        public int ICPID { get; set; }
    }

    [Table("ViewPatientSurveillance", Schema = "dbo")] //Surveillance recommendations
    public class Surveillance
    {
        [Key]
        public int SurvRecID { get; set; }
        public int RiskID { get; set; }
        public string? Clinician { get; set; }
        public int MPI { get; set; }
        public string? FIRSTNAME { get; set; }
        public string? LASTNAME { get; set; }
        public string? SurvFreqCode { get; set; }
        public int? SurvStartAge { get; set; }
        public int? SurvStopAge { get; set; }
        public string? SurvSiteCode { get; set; }
        public string? SurvRecHoCode { get; set; }
    }

    [Table("ViewTestingEligibility", Schema = "dbo")] //Testing eligibility
    public class Eligibility
    {
        [Key]
        public int ID { get; set; }
        public int MPI { get; set; }
        public string? FIRSTNAME { get; set; }
        public string? LASTNAME { get; set; }
        public string? CalcTool { get; set; }
        public int? Gene { get; set; }
        public string? Score { get; set; }
        public string? OfferTesting { get; set; }
    }

    [Table("MasterFacilityTable", Schema = "dbo")] //External clinical facilities
    public class ExternalFacility
    {
        [Key]
        public string MasterFacilityCode { get; set; }
        public string? NAME { get; set; }
        public string? ADDRESS { get; set; }
        public string? CITY { get; set; }
        public string? STATE { get; set; }
        public string? ZIP { get; set; }
        public Int16 NONACTIVE { get; set; }
        public Int16 IS_GP_SURGERY { get; set; }
    }

    [Table("MasterClinicianTable", Schema = "dbo")] //External clinicians
    public class ExternalClinician
    {
        [Key]
        public string MasterClinicianCode { get; set; }
        public string? TITLE { get; set; }
        public string? FIRST_NAME { get; set; }
        public string? NAME { get; set; }
        public string? SPECIALITY { get; set; }
        public string? FACILITY { get; set; }
        public Int16 NON_ACTIVE { get; set; }
        public Int16 Is_Gp { get; set; }
    }

    [Table("ListCReg", Schema = "dbo")]
    public class CancerReg
    {
        [Key]
        public string CRegCode { get; set; }
        public string Registry {  get; set; }
        public bool Creg_InUse { get; set; }
    }

    [Table("ListRequestStatus", Schema = "dbo")]
    public class RequestStatus
    {
        [Key]
        public string RelStatusCode { get; set;}
        public string RelStatus { get; set; }
    }

    [Table("ListTumourSite", Schema = "dbo")]
    public class  TumourSite
    {
        [Key]
        public string SiteCode { get; set; }
        public string Site { get; set; }
    }

    [Table("ListTumourLat", Schema = "dbo")]
    public class TumourLat
    {
        [Key]
        public string LatCode { get; set; }
        public string Lat { get; set; }
    }

    [Table("ListTumourMorph", Schema = "dbo")]
    public class TumourMorph
    {
        [Key]
        public string MorphCode { get; set; }
        public string? Morph { get; set; }
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
    public class  CalculationTools
    {
        public string CalculationToolCode { get; set; }
    }
}
