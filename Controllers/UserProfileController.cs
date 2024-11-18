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
        private readonly ProfileVM _pvm;        

        public UserProfileController(ClinicalContext context, IConfiguration config)
        {
            _context = context;
            _config = config;
            _staffUserData = new StaffUserData(_context);
            _titleData = new TitleData(_context);
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

                    if(curPassword == _pvm.staffMember.PASSWORD && newPassword == newPasswordConf)
                    {
                        _crud.CallStoredProcedure("Password", "Change", 0,0,0,curPassword, newPassword, newPasswordConf, "", User.Identity.Name);
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
        public IActionResult Edit(string title, string forename, string surname, string position, string telephone, string email)
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

                    _crud.CallStoredProcedure("StaffMember", "Edit", 0, 0, 0, title, forename, surname, position, User.Identity.Name, null, null, false, false, 0, 0, 0, email, telephone);

                    return RedirectToAction("ProfileDetails", "UserProfile", new { message = "Success - details have been updated", isSuccess = true });
                }
            }
            catch (Exception ex)
            {
                return RedirectToAction("ErrorHome", "Error", new { error = ex.Message, formName = "ChangePassword" });
            }


        }
    }
}
