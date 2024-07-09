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
    public class Document
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
     
    [Table("Constants", Schema = "dbo")]
    public class Constant
    {
        [Key]
        public string ConstantCode { get; set; }
        public string ConstantValue { get; set; }
        public string? ConstantValue2 { get; set; }
    }    
}