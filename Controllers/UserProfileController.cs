using ClinicalXPDataConnections.Data;
using ClinicalXPDataConnections.Meta;
using ClinicX.Meta;
using ClinicX.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace ClinicX.Controllers
{
    
    public class UserProfileController : Controller
    {
        private readonly ClinicalContext _context;
        private readonly IConfiguration _config;
        private readonly IStaffUserData _staffUserData;
        private readonly ITitleData _titleData;
        private readonly ICRUD _crud;
        private readonly IAuditService _auditService;
        private readonly ProfileVM _pvm;

        public UserProfileController(ClinicalContext context, IConfiguration config)
        {
            _context = context;
            _config = config;
            _staffUserData = new StaffUserData(_context);
            _titleData = new TitleData(_context);
            _pvm = new ProfileVM();
            _crud = new CRUD(_config);
            _auditService = new AuditService(_config);
        }

        public IActionResult ProfileDetails(string? message, bool? isSuccess)
        {
            try
            {
                if (!User.Identity.IsAuthenticated)
                {
                    return RedirectToAction("UserLogin", "Login");
                }
                else
                {
                    _pvm.staffMember = _staffUserData.GetStaffMemberDetails(User.Identity.Name);
                    _auditService.CreateUsageAuditEntry(_pvm.staffMember.STAFF_CODE, "Staff Profile", "Staffcode=" + _pvm.staffMember.STAFF_CODE);
                    if (message != null)
                    {
                        _pvm.message = message;
                        _pvm.success = isSuccess.GetValueOrDefault();
                    }

                    return View(_pvm);
                }
            }
            catch (Exception ex)
            {
                return RedirectToAction("ErrorHome", "Error", new { error = ex.Message, formName = "ProfileDetails" });
            }           
        }

        [HttpGet]
        public IActionResult ChangePassword()
        {
            try
            {
                if (!User.Identity.IsAuthenticated)
                {
                    return RedirectToAction("UserLogin", "Login");
                }
                else
                {
                    _pvm.staffMember = _staffUserData.GetStaffMemberDetails(User.Identity.Name);
                    _auditService.CreateUsageAuditEntry(_pvm.staffMember.STAFF_CODE, "Change Password", "");

                    return View(_pvm);
                }
            }
            catch (Exception ex)
            {
                return RedirectToAction("ErrorHome", "Error", new { error = ex.Message, formName = "ChangePassword" });
            }
        }

        [HttpPost]
        public IActionResult ChangePassword(string curPassword, string newPassword, string newPasswordConf)
        {
            try
            {
                if (!User.Identity.IsAuthenticated)
                {
                    return RedirectToAction("UserLogin", "Login");
                }
                else
                {
                    _pvm.staffMember = _staffUserData.GetStaffMemberDetails(User.Identity.Name);
                    _auditService.CreateUsageAuditEntry(_pvm.staffMember.STAFF_CODE, "Change Password", "Staffcode=" + _pvm.staffMember.STAFF_CODE);

                    if (curPassword == _pvm.staffMember.PASSWORD && newPassword == newPasswordConf)
                    {
                        int success = _crud.CallStoredProcedure("Password", "Change", 0,0,0,curPassword, newPassword, newPasswordConf, "", User.Identity.Name);

                        if (success == 0) { return RedirectToAction("ErrorHome", "Error", new { error = "Something went wrong with the database update.", formName = "Profile-ChangePassword(SQL)" }); }
                    }

                    return RedirectToAction("ProfileDetails", "UserProfile", new { message = "Success - password has been changed", isSuccess = true });
                }
            }
            catch (Exception ex)
            {
                return RedirectToAction("ErrorHome", "Error", new { error = ex.Message, formName = "ChangePassword" });
            }
        }

        [HttpGet]
        public IActionResult Edit()
        {
            try
            {
                if (!User.Identity.IsAuthenticated)
                {
                    return RedirectToAction("UserLogin", "Login");
                }
                else
                {
                    _pvm.staffMember = _staffUserData.GetStaffMemberDetails(User.Identity.Name);
                    _auditService.CreateUsageAuditEntry(_pvm.staffMember.STAFF_CODE, "Update Details", "");
                    _pvm.titles = _titleData.GetTitlesList();

                    return View(_pvm);
                }
            }
            catch (Exception ex)
            {
                return RedirectToAction("ErrorHome", "Error", new { error = ex.Message, formName = "Edit" });
            }
        }

        [HttpPost]
        public IActionResult Edit(string title, string forename, string surname, string position, string telephone, string email, string gmcnumber)
        {
            try
            {
                if (!User.Identity.IsAuthenticated)
                {
                    return RedirectToAction("UserLogin", "Login");
                }
                else
                {
                    _pvm.staffMember = _staffUserData.GetStaffMemberDetails(User.Identity.Name);
                    _auditService.CreateUsageAuditEntry(_pvm.staffMember.STAFF_CODE, "Update Details", "Staffcode=" + _pvm.staffMember.STAFF_CODE);

                    if (!email.Contains("@")) //we simply can NOT validate the email address in the front end, because there is no way to escape the @, so it has to be done here.
                    {
                        return RedirectToAction("ProfileDetails", "UserProfile", new { message = "Email verification failed.", isSuccess = false });
                    }
                    else
                    {
                        int success = _crud.CallStoredProcedure("StaffMember", "Edit", 0, 0, 0, title, forename, surname, position, User.Identity.Name, null, null, false, false, 0, 0, 0, email, telephone, gmcnumber);
                        
                        if (success == 0) { return RedirectToAction("ErrorHome", "Error", new { error = "Something went wrong with the database update.", formName = "Profile-UpdateDetails(SQL)" }); }
                        
                        return RedirectToAction("ProfileDetails", "UserProfile", new { message = "Success - details have been updated", isSuccess = true });
                    }
                }
            }
            catch (Exception ex)
            {
                return RedirectToAction("ErrorHome", "Error", new { error = ex.Message, formName = "ChangePassword" });
            }
        }
    }
}
