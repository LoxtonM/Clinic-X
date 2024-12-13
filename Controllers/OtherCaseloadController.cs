using ClinicalXPDataConnections.Data;
using ClinicalXPDataConnections.Meta;
using ClinicX.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using ClinicalXPDataConnections.Models;

namespace ClinicX.Controllers
{
    public class OtherCaseloadController : Controller
    {
        private readonly ClinicalContext _clinContext;
        private readonly CaseloadVM _cvm;
        private readonly IConfiguration _config;
        private readonly IStaffUserData _staffUser;
        private readonly ICaseloadData _caseloadData;
        private readonly ISupervisorData _supervisorData;
        private readonly IReferralData _referralData;
        private readonly IAreaNamesData _areaNamesData;
        private readonly IPatientData _patientData;
        private IAuditService _audit;

        public OtherCaseloadController(ClinicalContext context, IConfiguration config)
        {
            _clinContext = context;
            _config = config;
            _cvm = new CaseloadVM();            
            _staffUser = new StaffUserData(_clinContext);
            _caseloadData = new CaseloadData(_clinContext);
            _supervisorData = new SupervisorData(_clinContext);
            _referralData = new ReferralData(_clinContext);
            _areaNamesData = new AreaNamesData(_clinContext);
            _patientData = new PatientData(_clinContext);
            _audit = new AuditService(_config);
        }

        [Authorize]
        public IActionResult Index(string? staffCode)
        {
            try
            {
                if (staffCode == null)
                {
                    staffCode = _staffUser.GetStaffMemberDetails(User.Identity.Name).STAFF_CODE;
                }
                _cvm.isSupervisor = false;
                string userStaffCode = _staffUser.GetStaffMemberDetails(User.Identity.Name).STAFF_CODE;
                _audit.CreateUsageAuditEntry(userStaffCode, "ClinicX - Caseloads", "StaffCode=" + staffCode);

                _cvm.staffCode = staffCode;
                _cvm.caseLoad = _caseloadData.GetCaseloadList(staffCode).OrderBy(c => c.BookedDate).ThenBy(c => c.BookedTime).ToList();
                _cvm.clinicians = _staffUser.GetClinicalStaffList();
                if (_cvm.caseLoad.Count() > 0)
                {
                    _cvm.name = _cvm.caseLoad.FirstOrDefault().Clinician;
                }

                if(_supervisorData.GetIsConsSupervisor(staffCode) || _supervisorData.GetIsGCSupervisor(staffCode))
                {
                    _cvm.isSupervisor = true;
                }
                

                return View(_cvm);
            }
            catch (Exception ex) 
            {
                return RedirectToAction("ErrorHome", "Error", new { error = ex.Message, formName = "OtherCaseload" });
            }
        }

        [Authorize]
        public IActionResult CaseloadDistribution(int? year)
        {
            try
            {
                List<Referral> referralList = _referralData.GetActiveReferralsList();

                if(year != null)
                {
                    referralList = referralList.Where(r => r.RefDate >= DateTime.Parse(year + "-01-01")).ToList();
                    _cvm.yearSelected = year.GetValueOrDefault();
                }

                _cvm.clinicians = _staffUser.GetClinicalStaffList();
                _cvm.areaNamesList = _areaNamesData.GetAreaNames();

                _cvm.TotalConsReferralCount = new Dictionary<string, int>();
                _cvm.TotalGCReferralCount = new Dictionary<string, int>();
                _cvm.TotalAreaReferralCount = new Dictionary<string, int>();

                _cvm.years = new List<int>();
                foreach(var r in referralList.Where(r => r.RefDate != null).OrderByDescending(r => r.RefDate))
                {
                    int y = r.RefDate.Value.Year;
                    if(!_cvm.years.Contains(y))
                    {
                        _cvm.years.Add(y);
                    }
                }
                                
                int i = 0;
                referralList = referralList.Where(r => r.COMPLETE == "Active" && r.logicaldelete == false).ToList();
                   
                foreach (var c in _cvm.clinicians.OrderBy(c => c.NAME))
                {
                    string staffType = c.CLINIC_SCHEDULER_GROUPS;

                    if (staffType == "Consultant")
                    {
                        i = referralList.Where(r => r.PATIENT_TYPE_CODE == c.STAFF_CODE).Count();
                            
                        if (!_cvm.TotalConsReferralCount.ContainsKey(c.NAME) && i > 0) //because of course there are ducplicates.
                        {
                            _cvm.TotalConsReferralCount.Add(c.NAME, i);
                        }
                    }

                    if (staffType == "GC")
                    {
                        i = referralList.Where(r => r.GC_CODE == c.STAFF_CODE).Count();
                
                        if (!_cvm.TotalGCReferralCount.ContainsKey(c.NAME) && i > 0)
                        {
                            _cvm.TotalGCReferralCount.Add(c.NAME, i);
                        }
                    }                        
                }

                foreach (var a in _cvm.areaNamesList.OrderBy(a => a.AreaName))
                {
                    referralList = referralList.Where(r => r.PtAreaName != null).ToList();

                    i = referralList.Where(r => r.PtAreaName == a.AreaName).Count();

                    if (!_cvm.TotalAreaReferralCount.ContainsKey(a.AreaName) && i > 0) 
                    {
                        _cvm.TotalAreaReferralCount.Add(a.AreaName, i);
                    }
                }

                return View(_cvm);
            }
            catch (Exception ex)
            {
                return RedirectToAction("ErrorHome", "Error", new { error = ex.Message, formName = "CaseloadDistribution" });
            }
        }
    }
}

