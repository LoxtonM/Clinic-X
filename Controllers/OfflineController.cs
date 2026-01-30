using ClinicalXPDataConnections.Meta;
using ClinicalXPDataConnections.Models;
using ClinicX.Meta;
using ClinicX.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Data.OleDb;

namespace ClinicX.Controllers
{
    public class OfflineController : Controller
    {
        private readonly IConfiguration _config;
        private readonly ICRUD _crud;
        private readonly OfflineDBVM _ovm;
        private readonly IConstantsDataAsync _constants;
        private readonly IStaffUserDataAsync _staffUserData;
        private readonly IClinicDataAsync _clinicData;
        private readonly IPatientDataAsync _patientData;

        public OfflineController(IConfiguration config, ICRUD crud, IConstantsDataAsync constantsData, IStaffUserDataAsync staffUserData, IClinicDataAsync clinicData, IPatientDataAsync patientData)
        {
            _config = config;
            _crud = crud;
            _ovm = new OfflineDBVM();
            _constants = constantsData;
            _staffUserData = staffUserData;
            _clinicData = clinicData;
            _patientData = patientData;
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> Index(DateTime? dateFrom, DateTime? dateTo, string? message = "", bool? success = false)
        {
            if (message != "")
            {
                _ovm.message = message;
                _ovm.success = success.GetValueOrDefault();
            }

            if (dateFrom != null) { _ovm.dateFrom = dateFrom.GetValueOrDefault(); }
            else { _ovm.dateFrom = DateTime.Now; }
            if (dateTo != null) { _ovm.dateTo = dateTo.GetValueOrDefault(); }
            else { _ovm.dateTo = DateTime.Now.AddDays(14); }

            ModelState.Clear();

            return View(_ovm);
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Index(DateTime dateFrom, DateTime dateTo)
        {
            try
            {
                int success = _crud.SetupOfflineDatabase(User.Identity.Name, dateFrom, dateTo);

                bool isSuccess = false;
                string message = "Database setup failed";

                if (success == 1)
                {
                    await SetupDB(User.Identity.Name, dateFrom, dateTo);

                    isSuccess = true;
                    message = "Success";
                }

                return RedirectToAction("Index", new { dateFrom = dateFrom, dateTo = dateTo, success = isSuccess, message = message });
            }
            catch (Exception ex)
            {
                return RedirectToAction("ErrorHome", "Error", new { error = ex.Message, formName = "OfflineDB" });
            }
        }


        private async Task SetupDB(string username, DateTime dateFrom, DateTime dateTo)
        {
            string dbSourcePath = await _constants.GetConstant("OfflineDBMasterPath", 1);
            string dbDestPath = await _constants.GetConstant("OfflineDBDestPath", 1);
            string dbName = await _constants.GetConstant("OfflineDBName", 1);
            string dbSource = dbSourcePath + dbName;
            string dbDest = dbDestPath + dbName;

            if (!System.IO.File.Exists(dbDest))
            {
                System.IO.File.Copy(dbSource, dbDest);
            }
            //code to populate DB file from shadow tables, etc
            string staffCode = await _staffUserData.GetStaffCode(User.Identity.Name);

            List<Appointment> clinics = await _clinicData.GetClinicListByDate(dateFrom, dateTo);
            clinics = clinics.Where(c => c.STAFF_CODE_1 == staffCode).ToList();

            List<Patient> patients = new List<Patient>();
            
            var cn = new OleDbConnection("Provider=Microsoft.ACE.OLEDB.12.0;Data Source=C:\\OfflineDB\\OfflineMasterDB.mdb;Persist Security Info=False;");

            cn.Open();
            OleDbCommand cmd = new OleDbCommand("delete from MasterActivityTable", cn); //clear existing data            
            cmd.ExecuteNonQuery();
            cmd.CommandText = "delete from MasterPatientTable";
            cmd.ExecuteNonQuery();
            cn.Close();

            cn.Open();
            OleDbCommand cmdNotes = new OleDbCommand("select * from clinicalnotes", cn); //clear existing data
            OleDbDataReader reader = cmdNotes.ExecuteReader();

            if (reader.HasRows)
            {
                while (reader.Read())
                {
                    _crud.CallStoredProcedure("Clinical Note", "Create", reader.GetInt32(6), reader.GetInt32(1), 0, reader.GetString(4), "", "", reader.GetString(5), username,null,null);

                    OleDbCommand cmdDelNote = new OleDbCommand("delete from clinicalnotes where clinicalnoteid = " + reader.GetInt32(0), cn); //delete the local note once it's uploaded
                    cmdDelNote.ExecuteNonQuery();
                }
            }

            cn.Close();

            foreach (var item in clinics) //add new data for each clinic within the period
            {
                Patient pat = await _patientData.GetPatientDetails(item.MPI);
                patients.Add(pat);
                cn.Open();
                OleDbCommand cmd1 = new OleDbCommand($"Insert into MasterActivityTable (RefID, MPI, Type, BOOKED_DATE, BOOKED_TIME, FACILITY, STAFF_CODE_1, ClockStop, LogicalDelete) " +
                    $"values ({item.RefID}, {item.MPI}, '{item.AppType}', '{item.BOOKED_DATE}', '{item.BOOKED_TIME}', '{item.FACILITY}', '{item.STAFF_CODE_1}', 0,0)", cn);
                //so we CAN insert directly to the Access DB table... now we just need to figure out how to get the data
                cmd1.ExecuteNonQuery();
                

                OleDbCommand cmd2 = new OleDbCommand($"Insert into MasterPatientTable (MPI, CGU_No, IntID, WMFACSID, FIRSTNAME, LASTNAME, DOB, SOCIAL_SECURITY) " +
                    $"values ({pat.MPI}, '{pat.CGU_No}', {pat.INTID}, {pat.WMFACSID}, '{pat.FIRSTNAME}', '{pat.LASTNAME}', '{pat.DOB}', '{pat.SOCIAL_SECURITY}')", cn);
                cmd2.ExecuteNonQuery();
                cn.Close();
            }            

            cn.Close();
        }
    }
}