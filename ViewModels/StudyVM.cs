using ClinicX.Models;
using ClinicalXPDataConnections.Models;

namespace ClinicX.ViewModels
{
    public class StudyVM
    {
        public List<Study> studies { get; set; }
        public Study study { get; set; }
        public List<Studies> studyList { get; set; }
        public List<StudyStatus> studyStatusList { get; set; }
        public Patient patient { get; set; }
        public bool isLive { get; set; }   
        public string message { get; set; }
        public bool success { get; set; }
    }
}
