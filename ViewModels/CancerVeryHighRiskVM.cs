using ClinicalXPDataConnections.Models;
using ClinicX.Models;

namespace ClinicX.ViewModels
{
    public class CancerVeryHighRiskVM
    {
        public ICPCancer icpCancer { get; set; }
        public Referral referral { get; set; }
        public List<ScreeningService> screeningCoordinators { get; set; }
        public string? defaultScreeningCo { get; set; }
        public UntestedVHRGroup untestedVHRGroup { get; set; }
        public string message { get; set; }
        public bool success { get; set; }
    }
}
