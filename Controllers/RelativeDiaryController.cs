using ClinicalXPDataConnections.Meta;
using ClinicalXPDataConnections.Models;
using ClinicX.Meta;
using ClinicX.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;

namespace ClinicX.Controllers
{
    public class RelativeDiaryController : Controller
    {
        private readonly IRelativeDataAsync _relativeData;
        private readonly IRelativeDiaryDataAsync _relativeDiaryData;
        private readonly IPatientDataAsync _patientData;
        private readonly IRelativeDiarySourceDataAsync _diarySourceData;
        private readonly IStaffUserDataAsync _staffData;
        private readonly IDocumentsDataAsync _documentsData;
        private readonly ICRUD _crud;
        private readonly IDiaryActionDataAsync _diaryActionData;
        private readonly IDiaryClinicianDataAsync _diaryClinicianData;
        private readonly RelativeDiaryVM _rdvm;

        public RelativeDiaryController(IRelativeDataAsync relativeData, IRelativeDiaryDataAsync relativeDiaryData, IPatientDataAsync patientData, IRelativeDiarySourceDataAsync relativeDiarySourceData,
            IStaffUserDataAsync staffUserData, IDocumentsDataAsync documentsData, IDiaryActionDataAsync diaryActionData,  ICRUD crud, IDiaryClinicianDataAsync diaryClinicianData)
        {
            _relativeData = relativeData;
            _relativeDiaryData = relativeDiaryData;
            _patientData = patientData;
            _diarySourceData = relativeDiarySourceData;
            _documentsData = documentsData;
            _staffData = staffUserData;
            _diaryActionData = diaryActionData;
            _crud = crud;
            _rdvm = new RelativeDiaryVM();
            _diaryClinicianData = diaryClinicianData;
        }

        public async Task<IActionResult> Index(int relId)
        {
            try
            {
                _rdvm.relativeDetails = await _relativeData.GetRelativeDetails(relId);
                _rdvm.patient = await _patientData.GetPatientDetailsByWMFACSID(_rdvm.relativeDetails.WMFACSID);
                _rdvm.relativeDiaryList = await _relativeDiaryData.GetRelativeDiaryList(relId);
            }
            catch (Exception ex)
            {
                return RedirectToAction("ErrorHome", "Error", new { error = ex.Message, formName = "RelativeDiary" });
            }

            return View(_rdvm);
        }

        [HttpGet]
        public async Task<IActionResult> AddNew(int relId)
        {
            try
            {
                _rdvm.relativeDetails = await _relativeData.GetRelativeDetails(relId);
                _rdvm.patient = await _patientData.GetPatientDetailsByWMFACSID(_rdvm.relativeDetails.WMFACSID);
                _rdvm.sources = await _diarySourceData.GetRelativeDiarySourceList();
                _rdvm.staffMembersList = await _staffData.GetClinicalStaffList();
                _rdvm.docCodes = await _documentsData.GetDocumentsList();
                _rdvm.actionCodeList = await _diaryActionData.GetDiaryActions();
                _rdvm.diaryClinicianList = await _diaryClinicianData.GetDiaryClinicians();

                return View(_rdvm);
            }
            catch (Exception ex)
            {
                return RedirectToAction("ErrorHome", "Error", new { error = ex.Message, formName = "RelativeDiary-add" });
            }
        }

        [HttpPost]
        public async Task<IActionResult> AddNew(int id, string diaryWith, string actionCode, string? docCode, string? clinician, string? source, DateTime? diaryDate, string? extraText)
        {
            try
            {
                int success = await _crud.CallStoredProcedure("RelativeDiary", "Create", id, 0, 0, diaryWith, actionCode, docCode, extraText, User.Identity.Name,
                    diaryDate, null, false, false, 0, 0, 0, clinician, source);

                if (success == 0) { return RedirectToAction("ErrorHome", "Error", new { error = "Something went wrong with the database update.", formName = "RelativeDiary-add(SQL)" }); }

                return RedirectToAction("Index", "RelativeDiary", new { relID = id });
            }
            catch (Exception ex)
            {
                return RedirectToAction("ErrorHome", "Error", new { error = ex.Message, formName = "RelativeDiary-add" });
            }
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int diaryId)
        {
            try
            {
                _rdvm.relativeDiaryDetails = await _relativeDiaryData.GetRelativeDiaryDetails(diaryId);
                _rdvm.relativeDetails = await _relativeData.GetRelativeDetails(_rdvm.relativeDiaryDetails.RelsID);
                _rdvm.patient = await _patientData.GetPatientDetailsByWMFACSID(_rdvm.relativeDetails.WMFACSID);
                _rdvm.sources = await _diarySourceData.GetRelativeDiarySourceList();
                _rdvm.staffMembersList = await _staffData.GetClinicalStaffList();
                _rdvm.docCodes = await _documentsData.GetDocumentsList();
                _rdvm.actionCodeList = await _diaryActionData.GetDiaryActions();
                _rdvm.diaryClinicianList = await _diaryClinicianData.GetDiaryClinicians();

                return View(_rdvm);
            }
            catch (Exception ex)
            {
                return RedirectToAction("ErrorHome", "Error", new { error = ex.Message, formName = "RelativeDiary-edit" });
            }
        }

        [HttpPost]
        public async Task<IActionResult> Edit(int diaryId, string diaryWith, string actionCode, string? docCode, string? clinician, string? source, DateTime? diaryDate, 
            string? extraText, DateTime? firstRemDate, DateTime? recDate, bool returnNotExpected)
        {
            try
            {
                _rdvm.relativeDiaryDetails = await _relativeDiaryData.GetRelativeDiaryDetails(diaryId);
                _rdvm.relativeDetails = await _relativeData.GetRelativeDetails(_rdvm.relativeDiaryDetails.RelsID);

                int success = await _crud.CallStoredProcedure("RelativeDiary", "Edit", diaryId, 0, 0, diaryWith, actionCode, docCode, extraText, User.Identity.Name,
                    firstRemDate, recDate, returnNotExpected, false, 0, 0, 0, clinician, source);

                if (success == 0) { return RedirectToAction("ErrorHome", "Error", new { error = "Something went wrong with the database update.", formName = "RelativeDiary-add(SQL)" }); }

                return RedirectToAction("Index", "RelativeDiary", new { relID = _rdvm.relativeDetails.relsid });
            }
            catch (Exception ex)
            {
                return RedirectToAction("ErrorHome", "Error", new { error = ex.Message, formName = "RelativeDiary-edit" });
            }
        }
    }
}
