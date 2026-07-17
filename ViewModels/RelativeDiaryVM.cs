using Microsoft.EntityFrameworkCore;
using ClinicalXPDataConnections.Models;

namespace ClinicX.ViewModels
{
    [Keyless]
    public class RelativeDiaryVM
    {
        public Patient patient { get; set; }
        public Relative relativeDetails { get; set; }
        public List<RelativeDiary> relativeDiaryList { get; set; }
        public RelativeDiary relativeDiaryDetails { get; set; }
        public List<StaffMember> staffMembersList { get; set; }
        public List<DiaryAction> actionCodeList { get; set; }
        public List<Document> docCodes { get; set; }
        public List<RelativeDiarySource> sources { get; set; }
        public List<DiaryClinician> diaryClinicianList { get; set; }
        public bool isLive { get; set; }
    }
}
