using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace ClinicX.Models
{
    /*[Table("STAFF", Schema = "dbo")]
    public class StaffMember
    {
        [Key]
        public string STAFF_CODE { get; set; }
        public string? EMPLOYEE_NUMBER { get; set; }
        public string? NAME { get; set; }
        public string? POSITION { get; set; }
        public bool InPost { get; set; }
        public string CLINIC_SCHEDULER_GROUPS { get; set; }
        //public byte[]? SIGNATURE { get; set; }
        public string? StaffForename { get; set; }
        public string? StaffSurname { get; set; }
    }*/

    [Table("ListDocumentsNEW", Schema = "wmfacs_user")]
    public class Documents
    {
        [Key]
        public string DocCode { get; set; }
        public string? DocName { get; set; }
        public string? DocGroup { get; set; }
        public bool TemplateInUseNow { get; set; }
    }

    [Table("ListDocumentsContent", Schema = "wmfacs_user")]
    public class DocumentsContent
    {
        [Key]
        public int DocContentID { get; set; }
        public string DocCode { get; set; }
        public string LetterTo { get; set; }
        public string LetterFrom { get; set; }
        public string? Para1 { get; set; }
        public string? Para2 { get; set; }
        public string? Para3 { get; set; }
        public string? Para4 { get; set; }
        public string? Para5 { get; set; }
        public string? Para6 { get; set; }
        public string? Para7 { get; set; }
        public string? Para8 { get; set; }
        public string? Para9 { get; set; }
        public string? OurAddress { get; set; }
        public string? DirectLine { get; set; }
        public string? OurEmailAddress { get; set; }
    }

    /*[Table("MasterPatientTable", Schema = "dbo")]
    public class Patient
    {
        [Key]
        public int MPI { get; set; }
        public int INTID { get; set; }
        public string? Title { get; set; }
        public DateTime? DOB { get; set; }
        public string TITLE { get; set; }
        public string FIRSTNAME { get; set; }
        public string LASTNAME { get; set; }
        public string CGU_No { get; set; }
        public string? SOCIAL_SECURITY { get; set; }
        public string? ADDRESS1 { get; set; }
        public string? ADDRESS2 { get; set; }
        public string? ADDRESS3 { get; set; }
        public string? ADDRESS4 { get; set; }
        public string POSTCODE { get; set; }
        public string? PtLetterAddressee { get; set; }
        public string? SALUTATION { get; set; }
    }*/

    /*[Table("MasterClinicianTable", Schema = "dbo")]
    public class Referrer
    {
        [Key]
        public string MasterClinicianCode { get; set; }
        public string? TITLE { get; set; }
        public string? FIRST_NAME { get; set; }
        public string? NAME { get; set; }
        public string FACILITY { get; set; }
    }

    [Table("MasterFacilityTable", Schema = "dbo")]
    public class Facility
    {
        [Key]
        public string MasterFacilityCode { get; set; }
        public string NAME { get; set; }
        public string? ADDRESS { get; set; }
        public string? CITY { get; set; }
        public string? STATE { get; set; }
        public string? ZIP { get; set; }
        //public bool NONACTIVE { get; set; } 
    }*/

    [Table("Constants", Schema = "dbo")]
    public class Constants
    {
        [Key]
        public string ConstantCode { get; set; }
        public string ConstantValue { get; set; }
        public string? ConstantValue2 { get; set; }
    }

    /*[Table("DictatedLetters", Schema = "dbo")] //Dictated le'ahs
    public class DictatedLetter
    {
        [Key]
        public int DoTID { get; set; }
        public int MPI { get; set; }
        public int? RefID { get; set; }
        public string? LetterTo { get; set; }
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
    }

    [Table("DictatedLettersPatients", Schema = "dbo")] //Patients added to DOT
    public class DictatedLetterPatient
    {
        [Key]
        public int DOTPID { get; set; }
        public int DOTID { get; set; }
        public int MPI { get; set; }
        public int RefID { get; set; }
    }

    [Table("DictatedLettersCopies", Schema = "dbo")] //CC copies added to DOTs
    public class DictatedLetterCopy
    {
        [Key]
        public int CCID { get; set; }
        public int DotID { get; set; }
        public string CC { get; set; }
    }*/
}