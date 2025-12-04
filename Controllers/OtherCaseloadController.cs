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
        private readonly IPathwayData _pathwayData;
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
            _pathwayData = new PathwayData(_clinContext);
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
                IPAddressFinder _ip = new IPAddressFinder(HttpContext);                
                _audit.CreateUsageAuditEntry(userStaffCode, "ClinicX - Caseloads", "StaffCode=" + staffCode, _ip.GetIPAddress());

                _cvm.staffCode = staffCode;
                _cvm.caseLoad = _caseloadData.GetCaseloadList(staffCode).OrderBy(c => c.BookedDate).ThenBy(c => c.BookedTime).ToList();
                _cvm.clinicians = _staffUser.GetClinicalStaffList();
                if (_cvm.caseLoad.Count() > 0)
                {
                    _cvm.name = _cvm.caseLoad.FirstOrDefault().Clinician;
                }

                if(_supervisorData.GetIsConsSupervisor(staffCode) || _supervisorData.GetIsGCSupervisor(staffCode))
                {
                    _cvm.isSupervisor = true; //only supervisors should be able to see certain functions
                }
                
                return View(_cvm);
            }
            catch (Exception ex) 
            {
                return RedirectToAction("ErrorHome", "Error", new { error = ex.Message, formName = "OtherCaseload" });
            }
        }

        [Authorize]
        public IActionResult CaseloadDistribution(int? year, string? pathway, string? region) //telemetry - shows distribution of referrals across clinicians
        {
            try
            {
                string userStaffCode = _staffUser.GetStaffMemberDetails(User.Identity.Name).STAFF_CODE;
                IPAddressFinder _ip = new IPAddressFinder(HttpContext);
                _audit.CreateUsageAuditEntry(userStaffCode, "ClinicX - CaseloadDistribution", "", _ip.GetIPAddress());

                List<Referral> referralList = _referralData.GetActiveReferralsList();
                _cvm.years = new List<int>();

                IQueryable<Referral> referrals = referralList.Where(r => r.COMPLETE == "Active" && r.logicaldelete == false).AsQueryable(); 
                //converting to lists every time is slow, so best to do it as a IQueryable, then convert to list once at the end

                foreach (var r in referrals.Where(r => r.RefDate != null).OrderByDescending(r => r.RefDate))
                {
                    int y = r.RefDate.Value.Year;

                    if (!_cvm.years.Contains(y))
                    {
                        _cvm.years.Add(y);
                    }                   
                }
                
                _cvm.pathways = _pathwayData.GetPathwayList();

                if (year != null)
                {                    
                    referrals = referrals.Where(r => r.RefDate != null).Where(r => r.RefDate.Value.Year == year);
                    _cvm.yearSelected = year.GetValueOrDefault();
                }

                if(pathway != null)
                {                    
                    referrals = referrals.Where(r => r.PATHWAY == pathway);
                    _cvm.pathwaySelected = pathway;
                }

                referralList = referrals.Where(r => r.PATHWAY != null).ToList();                

                _cvm.clinicians = _staffUser.GetClinicalStaffList();
                List<AreaNames> areaNamesList = _areaNamesData.GetAreaNames();
                _cvm.geographicalRegions = new Dictionary<string, string>();

                foreach (var item in areaNamesList)
                {
                    string a = "";

                    switch(item.AreaCode)
                    {
                        case "A01":
                            a = "Bromsgrove and Redditch";
                            break;
                        case "A03":
                            a = "Kidderminster and Bewdley";
                            break;
                        case "A04":
                            a = "Worcestershire (inc. Rubery)";
                            break;
                        case "A05":
                            a = "Shropshire";
                            break;
                        case "A06":
                            a = "Staffordshire Central";
                            break;
                        case "A07":
                            a = "Staffordshire North";
                            break;
                        case "A08":
                            a = "Staffordshire South-East";
                            break;
                        case "A12":
                            a = "Birmingham South West";
                            break;
                        case "A13":
                            a = "Birmingham East";
                            break;
                        case "A16":
                            a = "Birmingham West";
                            break;
                        case "A23":
                            a = "Out of Region";
                            break;                        
                        case "B02":
                            a = "Herefordshire";
                            break;
                        case "B09":
                            a = "Rugby";
                            break;
                        case "B10":
                            a = "Warwickshire North";
                            break;
                        case "B11":
                            a = "Warwickshire South";
                            break;
                        case "B12":
                            a = "Birmingham Central";
                            break;
                        case "B14":
                            a = "Birmingham North";
                            break;
                        case "B15":
                            a = "Birmingham South";
                            break;
                        case "B16":
                            a = "Birmingham North Central";
                            break;
                        case "B17":
                            a = "Coventry";
                            break;
                        case "B18":
                            a = "Dudley and Halesowen";
                            break;
                        case "B19":
                            a = "Sandwell and West Bromwich";
                            break;
                        case "B20":
                            a = "Solihull";
                            break;
                        case "B21":
                            a = "Walsall";
                            break;
                        case "B22":
                            a = "Wolverhampton";
                            break;
                        case "B24":
                            a = "Uknown address";
                            break;                                                 
                    }

                    if (!_cvm.geographicalRegions.ContainsKey(item.AreaCode) && item.AreaID != 148 && a != "")
                    {
                        _cvm.geographicalRegions.Add(item.AreaCode, a);
                    }
                }

                if (region != null)
                {
                    string[] groupedRegions = new string[3];
                    groupedRegions[0] = region;
                    groupedRegions[1] = "";
                    groupedRegions[2] = "";

                    switch (region) //because of all the "DISTRICT: something" ones
                    {
                        case "A04":                            
                            groupedRegions[1] = "A30";
                            groupedRegions[2] = "A32";
                            break;
                        case "B12":                            
                            groupedRegions[1] = "A31";
                            break;
                        case "A03":
                            groupedRegions[1] = "A43";
                            break;
                        case "A06":
                            groupedRegions[1] = "A44";
                            groupedRegions[2] = "";
                            break;
                        case "A07":
                            groupedRegions[1] = "A45";
                            break;
                        case "A05":
                            groupedRegions[1] = "A46";
                            break;
                        case "A08":
                            groupedRegions[1] = "48";
                            break;
                        case "B10":
                            groupedRegions[1] = "B32";
                            break;
                        case "B17":
                            groupedRegions[1] = "B33";
                            break;
                        case "B20":
                            groupedRegions[1] = "B34";
                            break;
                        case "B02":
                            groupedRegions[1] = "B40";
                            break;
                        case "B21":
                            groupedRegions[1] = "B41";
                            break;
                        case "B18":
                            groupedRegions[1] = "B47";
                            break;
                        case "B14":
                            groupedRegions[1] = "B49";
                            break;

                    }

                    //referralList = referralList.Where(r => r.PtAreaCode == region).ToList();
                    referralList = referralList.Where(r => r.PtAreaCode == groupedRegions[0] || r.PtAreaCode == groupedRegions[1] || r.PtAreaCode == groupedRegions[2]).ToList();
                    _cvm.regionCodeSelected = region;
                    _cvm.regionNameSelected = _cvm.geographicalRegions.First(r => r.Key == region).Value;
                }

                _cvm.geographicalRegions = _cvm.geographicalRegions.OrderBy(a => a.Value).ToDictionary();
                _cvm.TotalConsReferralCount = new Dictionary<string, int>();
                _cvm.TotalConsReferralCountGeneral = new Dictionary<string, int>();
                _cvm.TotalConsReferralCountCancer = new Dictionary<string, int>();
                _cvm.TotalGCReferralCount = new Dictionary<string, int>();
                _cvm.TotalGCReferralCountGeneral = new Dictionary<string, int>();
                _cvm.TotalGCReferralCountCancer = new Dictionary<string, int>();
                _cvm.TotalAreaReferralCount = new Dictionary<string, int>();
                _cvm.TotalAreaReferralCountGeneral = new Dictionary<string, int>();
                _cvm.TotalAreaReferralCountCancer = new Dictionary<string, int>();

                int i = 0;
                int iGen = 0;
                int iCan = 0;
                   
                foreach (var c in _cvm.clinicians.OrderBy(c => c.NAME))
                {
                    string staffType = c.CLINIC_SCHEDULER_GROUPS;

                    if (staffType == "Consultant")
                    {
                        i = referralList.Where(r => r.PATIENT_TYPE_CODE == c.STAFF_CODE).Count();
                        iGen = referralList.Where(r => r.PATIENT_TYPE_CODE == c.STAFF_CODE & r.PATHWAY.Contains("General")).Count();
                        iCan = referralList.Where(r => r.PATIENT_TYPE_CODE == c.STAFF_CODE & r.PATHWAY.Contains("Cancer")).Count();

                        if (!_cvm.TotalConsReferralCount.ContainsKey(c.NAME) && i > 0) //because of course there are duplicates.
                        {
                            _cvm.TotalConsReferralCount.Add(c.NAME, i);
                            _cvm.TotalConsReferralCountGeneral.Add(c.NAME, iGen);
                            _cvm.TotalConsReferralCountCancer.Add(c.NAME, iCan);
                        }
                    }

                    if (staffType == "GC")
                    {
                        i = referralList.Where(r => r.GC_CODE == c.STAFF_CODE).Count();
                        iGen = referralList.Where(r => r.GC_CODE == c.STAFF_CODE & r.PATHWAY.Contains("General")).Count();
                        iCan = referralList.Where(r => r.GC_CODE == c.STAFF_CODE & r.PATHWAY.Contains("Cancer")).Count();

                        if (!_cvm.TotalGCReferralCount.ContainsKey(c.NAME) && i > 0)
                        {
                            _cvm.TotalGCReferralCount.Add(c.NAME, i);
                            _cvm.TotalGCReferralCountGeneral.Add(c.NAME, iGen);
                            _cvm.TotalGCReferralCountCancer.Add(c.NAME, iCan);
                        }
                    }                        
                }

                
                foreach (var a in areaNamesList.OrderBy(a => a.AreaName))
                {                
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

