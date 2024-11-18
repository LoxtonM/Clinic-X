using ClinicalXPDataConnections.Data;
using ClinicalXPDataConnections.Meta;
using ClinicalXPDataConnections.Models;
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
        private readonly ICRUD _crud;
        private readonly ProfileVM _pvm;        

        public UserProfileController(ClinicalContext context, IConfiguration config)
        {
            _context = context;
            _config = config;
            _staffUserData = new StaffUserData(_context);
            _pvm = new ProfileVM();
            _crud = new CRUD(_config);
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
                return RedirectToAction("ErrorHome", "Error", new { error = ex.Message, formName = "ClinicalNotes" });
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

                    return View(_pvm);
                }
            }
            catch (Exception ex)
            {
                return RedirectToAction("ErrorHome", "Error", new { error = ex.Message, formName = "ClinicalNotes" });
            }


        }

        [HttpPost]
        public IActionResult Edit(string curPassword, string newPassword, string newPasswordConf)
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

                    if(curPassword == _pvm.staffMember.PASSWORD && newPassword == newPasswordConf)
                    {
                        _crud.CallStoredProcedure("Password", "Change", 0,0,0,curPassword, newPassword, newPasswordConf, "", User.Identity.Name);
                    }

                    return RedirectToAction("ProfileDetails", "UserProfile", new { message = "Success - password has been changed", isSuccess = true });
                }
            }
            catch (Exception ex)
            {
                return RedirectToAction("ErrorHome", "Error", new { error = ex.Message, formName = "ClinicalNotes" });
            }


        }
    }
}
