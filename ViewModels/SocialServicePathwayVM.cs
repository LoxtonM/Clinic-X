using Microsoft.EntityFrameworkCore;
using ClinicalXPDataConnections.Models;
using System.Security.Cryptography.Xml;

namespace ClinicX.ViewModels
{
    [Keyless]
    public class SocialServicePathwayVM
    {
        public List<SocialServicePathway> socialServicePathwayList { get; set; }
        public SocialServicePathway socialServicePathway { get; set; }
        public List<SocialServicePathwayOutcome> socialServicePathwayOutcomeList { get; set; }
        public List<SocialServicePathwayStatus> socialServicePathwayStatusList { get; set; }
        public List<SocialWorker> socialWorkerList { get; set; }
        public List<SocialService> socialServiceList { get; set; }
        public bool isLive { get; set; }
        public string? message { get; set; }
        public bool success { get; set; }
    }
}
