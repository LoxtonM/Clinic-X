using ClinicX.Data;
using ClinicX.Models;
using ClinicX.ViewModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.Data.SqlClient;
using System.Data;

namespace ClinicX.Meta
{
    public class VMData //This class contains all of the data "get" functions that retrieve data from a data model.
                        //They each return either a list of a data model.
        //Note there are two constructors, because there are two possible "datacontext" parameters that can be passed to it.
    {
        private readonly ClinicalContext _clinContext;
        private readonly DocumentContext? _docContext;

        public VMData(ClinicalContext context)
        {
            _clinContext = context;
        }
        
        public VMData(DocumentContext docContext)
        {            
            _docContext = docContext;
        }

        public StaffMemberList GetCurrentStaffUser(string username)
        {
            var user = _clinContext.StaffMembers.FirstOrDefault(u => u.EMPLOYEE_NUMBER == username);

            return user;
        }

        public Patients GetPatientDetails(int id)
        {
            var patient = _clinContext.Patients?.FirstOrDefault(i => i.MPI == id);
            return patient;
        } //Get patient details from MPI

        public Patients GetPatientDetailsByWMFACSID(int id)
        {
            var patient = _clinContext.Patients?.FirstOrDefault(i => i.WMFACSID == id);
            return patient;
        } //Get patient details from WMFACSID

        public PatientPathway GetPathwayDetails(int id)
        {
            var pathway = _clinContext.PatientPathway.OrderBy(i => i.REFERRAL_DATE).FirstOrDefault(i => i.MPI == id);
            return pathway;
        } //Get earliest active pathway for patient by MPI

        public List<Alert> GetAlertsList(int id)
        {
            var alerts = from a in _clinContext.Alert
                        where a.MPI == id & a.EffectiveToDate == null
                        orderby a.AlertID
                        select a;            

            return alerts.ToList();
        } //Get list of alerts for patient by MPI
        
        public Referrals GetReferralDetails(int id)
        {
            var referral = _clinContext.Referrals?.FirstOrDefault(i => i.refid == id);
            return referral;
        } //Get details of referral by RefID

        public ActivityItems GetActivityDetails(int id) //Get details of any activity item by RefID
        {
            var referral = _clinContext.ActivityItems?.FirstOrDefault(i => i.RefID == id);
            return referral;
        }                

        public List<Referrals> GetReferralsList(int id) //Get list of active referrals for patient by MPI
        {
            var referrals = from r in _clinContext.Referrals
                           where r.MPI == id & r.RefType.Contains("Referral") & r.COMPLETE != "Complete"
                           orderby r.RefDate
                           select r;            

            return referrals.ToList();
        }

        public List<Diary> GetDiaryList(int id) //Get list of diary entries for patient by MPI
        {
            var pat = _clinContext.Patients.FirstOrDefault(p => p.MPI == id);

            var diary = from d in _clinContext.Diary
                        where d.WMFACSID == pat.WMFACSID
                        orderby d.DiaryDate
                        select d;

            return diary.ToList();
        }

        public List<Relatives> GetRelativesList(int id) //Get list of relatives of patient by MPI
        {
            var patient = _clinContext.Patients.FirstOrDefault(i => i.MPI == id);
            int iWmfacsID = patient.WMFACSID;

            var relative = from r in _clinContext.Relatives
                           where r.WMFACSID == iWmfacsID
                           select r;           

            return relative.ToList();
        }

        public Relatives GetRelativeDetails(int relID)
        {
            var rel = _clinContext.Relatives.FirstOrDefault(r => r.relsid == relID);

            return rel;
        }

        public List<RelativesDiagnosis> GetRelativeDiagnosisList(int id)
        {
            var reldiag = from r in _clinContext.RelativesDiagnoses
                           where r.RelsID == id
                           select r;
            
            return reldiag.ToList();
        }

        public RelativesDiagnosis GetRelativeDiagnosisDetails(int id)
        {
            var item = _clinContext.RelativesDiagnoses.FirstOrDefault(rd => rd.TumourID == id);
            return item;
        }

        public List<HPOTermDetails> GetHPOTermsAddedList(int id)
        {
            var notes = from n in _clinContext.HPOTermDetails
                        where n.MPI == id
                        select n;            

            return notes.ToList();
        } //Get list of HPO codes added to a patient by MPI

        public List<DiseaseList> GetDiseaseList()
        {
            var items = from i in _clinContext.Diseases
                        orderby i.DESCRIPTION
                        select i;           

            return items.ToList();
        } //Get list of all diseases

        public List<Diagnosis> GetDiseaseListByPatient(int iMPI)
        {            

            var items = from i in _clinContext.Diagnosis
                        where i.MPI == iMPI
                        orderby i.DESCRIPTION
                        select i;

            return items.ToList();
        } //Get list of all diseases

        public List<DiseaseStatusList> GetStatusList()
        {
            var items = from i in _clinContext.DiseaseStatusList
                        select i;
            
            return items.ToList();
        } //Get list of all possible disease statuses
        
        public Diagnosis GetDiagnosisDetails(int id) //Get details of diagnosis by the diagnosis ID
        {
            var diagnosis = _clinContext.Diagnosis.FirstOrDefault(i => i.ID == id);

            return diagnosis;
        }  

        public List<DictatedLetters> GetDictatedLettersList(string staffcode)
        {
            var letters = from l in _clinContext.DictatedLetters
                          where l.LetterFromCode == staffcode && l.MPI != null && l.RefID != null && l.Status != "Printed"
                          orderby l.DateDictated descending
                          select l;

            return letters.ToList();
        }
        
        public DictatedLetters GetDictatedLetterDetails(int iDotID) //Get details of DOT letter by its DotID
        {
            var letter = _clinContext.DictatedLetters.FirstOrDefault(l => l.DoTID == iDotID);

            return letter;
        }

        public List<DictatedLettersPatients> GetDictatedLettersPatientsList(int iDotID) //Get list of patients added to a DOT letter by the DotID
        {
            var patient = from p in _clinContext.DictatedLettersPatients
                          where p.DOTID == iDotID
                          select p;

            List<DictatedLettersPatients> patients = new List<DictatedLettersPatients>();

            foreach (var p in patient)
            {
                patients.Add(new DictatedLettersPatients() { DOTID = p.DOTID });
            }

            return patients;
        }

        public List<DictatedLettersCopies> GetDictatedLettersCopiesList(int iDotID) //Get list of all CCs added to a DOT letter by DotID
        {
            var copies = from c in _clinContext.DictatedLettersCopies
                       where c.DotID == iDotID
                       select c;            

            return copies.ToList();
        }        
        
        public DictatedLettersCopies GetDictatedLetterCopyDetails(int iID) 
        {
            var letter = _clinContext.DictatedLettersCopies.FirstOrDefault(x => x.CCID == iID);

            return letter;
        } //Get details of a CC on a letter for deletion

        public List<Patients> GetDictatedLetterPatientsList(int iDotID) //Get list of all patients in the family that can be added to a DOT, by the DotID
        {
            var letter = _clinContext.DictatedLetters.FirstOrDefault(l => l.DoTID == iDotID);
            int? impi = letter.MPI;
            var pat = _clinContext.Patients.FirstOrDefault(p => p.MPI == impi.GetValueOrDefault());

            var patients = from p in _clinContext.Patients
                           where p.PEDNO == pat.PEDNO
                           select p;

            return patients.ToList();
        }

        public List<HPOTerms> GetHPOTermsList() //Get list of all possible HPO codes
        {
            var terms = from t in _clinContext.HPOTerms
                       select t;            

            return terms.ToList();
        }

        public List<HPOTermDetails> GetExistingHPOTermsList(int id) //Get list of all HPO codes added to a clinical note, by the ClinicalNoteID
        {
            var terms = from t in _clinContext.HPOTermDetails
                       where t.ClinicalNoteID == id
                       select t;           

            return terms.ToList();
        }

        public ClinicalNotes GetClinicalNoteDetails(int? iNoteID) //Get details of a clinical note by ClinicalNotesID
        {
            var note = _clinContext.ClinicalNotes.FirstOrDefault(i => i.ClinicalNoteID == iNoteID);

            return note;
        }

        public List<ClinicalNotes> GetClinicalNoteList(int? iMPI) //Get list of clinical notes by MPI
        {
            var notes = from n in _clinContext.ClinicalNotes
                        where n.MPI == iMPI
                        select n;

            return notes.ToList();
        }

        public List<HPOExtractVM> GetExtractedTermsList(int iNoteID, IConfiguration _config) //Get list of HPO codes that can be extracted from a clinical note, by ClinicalNoteID
        {
            var model = new List<HPOExtractVM>();

            SqlConnection conn = new SqlConnection(_config.GetConnectionString("ConString"));
            conn.Open();
            SqlCommand cmd = new SqlCommand("dbo.sp_CXGetNewMatchingTermsForClinicalNote", conn);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.Add("@ClinicalNoteId", SqlDbType.Int).Value = iNoteID;


            using (var reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    // add items in the list
                    model.Add(new HPOExtractVM()
                    {
                        HPOTermID = reader.GetInt32(0),
                        TermCode = reader.GetString(1),
                        Term = reader.GetString(2)
                    });
                }
            }

            conn.Close();


            return (model);
        }

        public List<TestList> GetTestList() //Get list of all available tests
        {
            var items = from i in _clinContext.Tests
                        select i;           

            return items.ToList();
        }

        public ICP GetICPDetails(int iIcpID)
        {
            var icp = _clinContext.ICP.FirstOrDefault(i => i.ICPID == iIcpID);
            return icp;
        }
        public Triages GetTriageDetails(int? iIcpID) //Get details of ICP from the IcpID
        {
            var icp = _clinContext.Triages.FirstOrDefault(i => i.ICPID == iIcpID);
            return icp;
        }

        public List<Triages> GetTriageList(string sUsername) //Get list of all outstanding triages for a specific user (by login name)
        {
            var triages = from t in _clinContext.Triages
                         where t.LoginDetails == sUsername
                         orderby t.RefDate descending
                         select t;           

            return triages.ToList();
        }

        public List<ICPCancer> GetCancerICPList(string sUsername) //Get list of all open Cancer ICP Reviews for a specific user (by login name)
        {
            var user = _clinContext.StaffMembers.FirstOrDefault(s => s.EMPLOYEE_NUMBER == sUsername);
            string sStaffCode = user.STAFF_CODE;

            var icps = from i in _clinContext.ICPCancer
                      where i.ActOnRefBy != null && i.FinalReviewed == null && i.GC_CODE == sStaffCode
                      select i;
            
            return icps.ToList();
        }

        public List<ICPActionsList> GetICPCancerActionsList() //Get list of all triage actions for Cancer ICPs
        {
            var actions = from a in _clinContext.ICPCancerActionsList
                         where a.InUse == true
                         orderby a.ID
                         select a;
           
            return actions.ToList();
        }

        public List<ICPGeneralActionsList> GetICPGeneralActionsList() //Get list of all "treatpath" items for General ICPs
        {
            var actions = from a in _clinContext.ICPGeneralActionsList
                         where a.InUse == true
                         orderby a.ID
                         select a;
           
            return actions.ToList();
        }

        public List<ICPGeneralActionsList2> GetICPGeneralActionsList2() //Get list of all "treatpath2" items for General ICPs
        {
            var actions = from a in _clinContext.ICPGeneralActionsList2
                         where a.InUse == true
                         orderby a.ID
                         select a;
       
            return actions.ToList();
        }

        public List<ClinicalFacilityList> GetClinicalFacilitiesList() //Get list of all clinic facilities where we hold clinics
        {
            var facs = from f in _clinContext.ClinicalFacilities
                      where f.NON_ACTIVE == 0
                      select f;
        
            return facs.ToList();
        }

        public List<StaffMemberList> GetClinicalStaffList() //Get list of all clinical staff members currently in post
        {
            var clinicians = from s in _clinContext.StaffMembers
                        where s.InPost == true && (s.CLINIC_SCHEDULER_GROUPS == "GC" || s.CLINIC_SCHEDULER_GROUPS == "Consultant")
                        orderby s.NAME
                        select s;
          
            return clinicians.ToList();
        }

        public List<StaffMemberList> GetStaffMemberList() //Get list of all staff members currently in post 
        {
            var sm = from s in _clinContext.StaffMembers
                     where s.InPost.Equals(true)
                     orderby s.NAME
                     select s;

            return sm.ToList();
        }

        public ICPGeneral GetGeneralICPDetails(int? iIcpID) //Get details of a general ICP by the IcpID
        {
            var icp = _clinContext.ICPGeneral.FirstOrDefault(c => c.ICPID == iIcpID);
            return icp;
        }

        public ICPCancer GetCancerICPDetails(int? iIcpID) //Get details of a cancer ICP by the Cancer ID
        {
            var icp = _clinContext.ICPCancer.FirstOrDefault(c => c.ICP_Cancer_ID == iIcpID);
            return icp;
        }

        public ICPCancer GetCancerICPDetailsByICPID(int? iIcpID) //Get details of a cancer ICP by the IcpID
        {
            var icp = _clinContext.ICPCancer.FirstOrDefault(c => c.ICPID == iIcpID);
            return icp;
        }

        public List<Risk> GetRiskList(int? iIcpID) //Get list of all risk items for an ICP (by IcpID)
        {
            var icp = _clinContext.ICPCancer.FirstOrDefault(c => c.ICP_Cancer_ID == iIcpID);

            var risks = from r in _clinContext.Risk
                       where r.MPI == icp.MPI
                       select r;
           
            return risks.ToList();
        }

        public List<Surveillance> GetSurveillanceList(int? iMPI) //Get list of all surveillance recommendations for an ICP (by MPI)
        {
            //var icp = _clinContext.ICPCancer.FirstOrDefault(c => c.ICP_Cancer_ID == iIcpID);

            var surveillances = from r in _clinContext.Surveillance
                               where r.MPI == iMPI
                               select r;

            return surveillances.ToList();
        }

        public List<Surveillance> GetSurveillanceListByRiskID(int? iRiskID) //Get list of all surveillance recommendations for a  risk item (by RiskID)
        {
            //var icp = _clinContext.ICPCancer.FirstOrDefault(c => c.ICP_Cancer_ID == iIcpID);

            var surveillances = from r in _clinContext.Surveillance
                                where r.RiskID == iRiskID
                                select r;

            return surveillances.ToList();
        }

        public Risk GetRiskDetails(int? iRiskID) //Get details of risk item by RiskID
        {
            var risk = _clinContext.Risk.FirstOrDefault(c => c.RiskID == iRiskID);
            return risk;
        }

        public Surveillance GetSurvDetails(int? iRiskID) //Get details of surveillance recommendation by RiskID
        {
            var surv = _clinContext.Surveillance.FirstOrDefault(c => c.RiskID == iRiskID);
            return surv;
        }

        public List<Eligibility> GetTestingEligibilityList(int? iMPI) //Get list of testing aligibility codes by IcpID
        {
            //var icp = _clinContext.ICPCancer.FirstOrDefault(c => c.ICP_Cancer_ID == iIcpID);

            var eligibilities = from e in _clinContext.Eligibility
                              where e.MPI == iMPI
                              select e;

            return eligibilities.ToList();
        }
        
        public List<NoteTypeList> GetNoteTypesList() //Get list of possible types of clinical note
        {
            var notetypes = from t in _clinContext.NoteTypes
                           where t.NoteInUse == true
                           select t;

            return notetypes.ToList();
        }

        public List<Clinics> GetClinicList(string username) //Get list of your clinics
        {
            string sStaffCode = GetStaffMemberDetails(username).STAFF_CODE;

            var clinics = from c in _clinContext.Clinics
                          where c.AppType.Contains("App") && c.STAFF_CODE_1 == sStaffCode && c.Attendance != "Declined"
                          select c;

            return clinics.ToList();
        }

        public List<Clinics> GetClinicByPatientsList(int iMPI)
        {
            var appts = from c in _clinContext.Clinics
                        where c.MPI.Equals(iMPI)
                        orderby c.BOOKED_DATE descending
                        select c;

            return appts.ToList();
        }

        public Clinics GetClinicDetails(int iRefID) //Get details of an appointment for display only
        {
            var appt = _clinContext.Clinics.FirstOrDefault(a => a.RefID == iRefID);

            return appt;
        }

        public List<Reviews> GetReviewsList(string username) 
        {
            string sStaffCode = GetStaffMemberDetails(username).STAFF_CODE;

            var reviews = from r in _clinContext.Reviews
                          where r.Review_Recipient == sStaffCode && r.Review_Status == "Pending"
                          orderby r.Planned_Date
                          select r;

            return reviews.ToList();
        }

        public Reviews GetReviewDetails(int id)
        {
            var review = _clinContext.Reviews.FirstOrDefault(r => r.ReviewID == id);

            return review;
        }

        public List<Test> GetTestListByUser(string username)
        {
            string sStaffCode = _clinContext.StaffMembers.FirstOrDefault(s => s.EMPLOYEE_NUMBER == username).STAFF_CODE;
             
            var tests = from t in _clinContext.Test
                        where t.ORDEREDBY.Equals(sStaffCode) & t.COMPLETE == "No"
                        select t;

            return tests.ToList();
        }

        public List<Test> GetTestListByPatient(int iMPI)
        {
            var tests = from t in _clinContext.Test
                        where t.MPI.Equals(iMPI)
                        select t;

            return tests.ToList();
        }

        public Test GetTestDetails(int id)
        {
            var test = _clinContext.Test.FirstOrDefault(t => t.TestID == id);

            return test;
        }

        public List<ActivityItems> GetClinicDetailsList(int iRefId) //Get details of an appointment by the RefID for editing
        {
            var cl = from c in _clinContext.ActivityItems
                     where c.RefID == iRefId
                     select c;

            List<ActivityItems> clinics = new List<ActivityItems>();

            foreach (var c in cl)
            {
                //Because the model can't handle spaces, we have to strip them out... but it bombs out if the value is null, so we have to
                //use this incredibly convoluted method to pass it to the HTML!!!
                string sLetterReq = "";
                string sCounseled = "";

                if (c.COUNSELED != null)
                {
                    sCounseled = c.COUNSELED.Replace(" ", "_");
                }
                else
                {
                    sCounseled = c.COUNSELED;
                }

                if (c.LetterReq != null)
                {
                    sLetterReq = c.LetterReq.Replace(" ", "_");
                }
                else
                {
                    sLetterReq = c.LetterReq;
                }

                clinics.Add(new ActivityItems()
                {
                    RefID = c.RefID,
                    BOOKED_DATE = c.BOOKED_DATE,
                    BOOKED_TIME = c.BOOKED_TIME,
                    TYPE = c.TYPE,
                    STAFF_CODE_1 = c.STAFF_CODE_1,
                    FACILITY = c.FACILITY,
                    COUNSELED = sCounseled,
                    SEEN_BY = c.SEEN_BY,
                    NOPATIENTS_SEEN = c.NOPATIENTS_SEEN,
                    LetterReq = sLetterReq,
                    ARRIVAL_TIME = c.ARRIVAL_TIME,
                    ClockStop = c.ClockStop
                });
            }

            return clinics;
        }

        public List<OutcomeList> GetOutcomesList() //Get list of outcomes for clinic appointments
        {
            var outcomes = from o in _clinContext.Outcomes
                          where o.DEFAULT_CLINIC_STATUS.Equals("Active")
                          select o;

            return outcomes.ToList();
        }

        public List<Ethnicity> GetEthnicitiesList() //Get list of ethnicities
        {
            var ethnicities = from e in _clinContext.Ethnicity
                         orderby e.Ethnic
                         select e;

            return ethnicities.ToList();
        }

        public StaffMemberList GetStaffMemberDetails(string suser) //Get details of a staff member by login name
        {
            var item = _clinContext.StaffMembers.FirstOrDefault(i => i.EMPLOYEE_NUMBER == suser);
            return item;
        }
        
        public DocumentsContent GetDocumentDetails(int id) //Get content for a type of standard letter by its ID
        {
            var item = _docContext.DocumentsContent.FirstOrDefault(d => d.DocContentID == id);
            return item;
        }

        public DocumentsContent GetDocumentDetailsByDocCode(string docCode) //Get content for a type of standard letter by its ID
        {
            var item = _docContext.DocumentsContent.FirstOrDefault(d => d.DocCode == docCode);
            return item;
        }

        public List<Documents> GetDocumentsList() 
        {
            var docs = from d in _docContext.Documents
                       where d.TemplateInUseNow == true
                       select d;

            return docs.ToList();
        }
        
        public string GetCCDetails(ExternalClinician referrer) //Get details of CC address
        {
            string cc = "";
            var facility = _clinContext.ExternalFacility.FirstOrDefault(f => f.MasterFacilityCode == referrer.FACILITY);

            cc = cc + Environment.NewLine + facility.NAME + Environment.NewLine + facility.ADDRESS + Environment.NewLine
                + facility.CITY + Environment.NewLine + facility.STATE + Environment.NewLine + facility.ZIP;
            return cc;
        }

        public ExternalFacility GetFacilityDetails(string sref) //Get details of external/referring facility
        {
            var item = _clinContext.ExternalFacility.FirstOrDefault(f => f.MasterFacilityCode == sref);
            return item;
        }        

        public List<ExternalFacility> GetFacilityList() //Get list of all external/referring facilities
        {
            var facilities = from rf in _clinContext.ExternalFacility
                             where rf.NONACTIVE == 0
                             orderby rf.NAME
                             select rf;

            return facilities.ToList();
        }

        public ExternalClinician GetClinicianDetails(string sref) //Get details of external/referring clinician
        {
            var item = _clinContext.ExternalClinician.FirstOrDefault(f => f.MasterClinicianCode == sref);
            return item;
        }

        public List<ExternalClinician> GetClinicianList() //Get list of all external/referring clinicians
        {
            var clinicians = from rf in _clinContext.ExternalClinician
                             where rf.NON_ACTIVE == 0
                             orderby rf.NAME
                             select rf;

            return clinicians.ToList();
        }

        public List<string> GetConsultantsList() //Get list of all consultants
        {
            var clinicians = from rf in _clinContext.StaffMembers
                             where rf.InPost == true && rf.CLINIC_SCHEDULER_GROUPS == "Consultant"
                             orderby rf.NAME
                             select rf.NAME;

            return clinicians.ToList();
        }

        public List<string> GetGCList() //Get list of all GCs
        {
            var clinicians = from rf in _clinContext.StaffMembers
                             where rf.InPost == true && rf.CLINIC_SCHEDULER_GROUPS == "GC"
                             orderby rf.NAME
                             select rf.NAME;

            return clinicians.ToList();
        }

        public List<string> GetSecTeamsList() //Get list of all secretarial teams
        {
            var secteams = from rf in _clinContext.StaffMembers
                             where rf.BILL_ID != null && rf.BILL_ID.Contains("Team")                            
                             select rf.BILL_ID;

            return secteams.Distinct().ToList();
        }

        public List<Caseload> GetCaseloadList(string sStaffCode) //Get caseload for clinician
        {
            var caseload = from c in _clinContext.Caseload
                           where c.StaffCode == sStaffCode
                           select c;

            return caseload.ToList();
        }

        public List<CancerReg> GetCancerRegList()
        {
            var creg = from c in _clinContext.CancerReg
                           where c.Creg_InUse == true
                           select c;
            int dfs = creg.Count();
            return creg.ToList();
        }

        public List<RequestStatus> GetRequestStatusList()
        {
            var status = from s in _clinContext.RequestStatus                       
                       select s;
            int dfs = status.Count();
            return status.ToList();
        }

        public List<TumourSite> GetTumourSiteList()
        {
            var item = from i in _clinContext.TumourSite
                       select i;

            return item.ToList();
        }

        public List<TumourLat> GetTumourLatList()
        {
            var item = from i in _clinContext.TumourLat
                       select i;

            return item.ToList();
        }

        public List<TumourMorph> GetTumourMorphList()
        {
            var item = from i in _clinContext.TumourMorph
                       select i;

            return item.ToList();
        }

        public List<RiskCodes> GetRiskCodesList()
        {
            var item = from i in _clinContext.RiskCodes
                       orderby i.RiskCode
                       select i;

            return item.ToList();
        }

        public List<SurvSiteCodes> GetSurvSiteCodesList()
        {
            var item = from i in _clinContext.SurvSiteCodes
                       orderby i.SurvSite
                       select i;

            return item.ToList();
        }

        public List<SurvTypeCodes> GetSurvTypeCodesList()
        {
            var item = from i in _clinContext.SurvTypeCodes
                       orderby i.SurvType
                       select i;

            return item.ToList();
        }

        public List<SurvFreqCodes> GetSurvFreqCodesList()
        {
            var item = from i in _clinContext.SurvFreqCodes
                       orderby i.SurvFreq
                       select i;

            return item.ToList();
        }

        public List<DiscontinuedReasonCodes> GetDiscReasonCodesList()
        {
            var item = from i in _clinContext.DiscontinuedReasonCodes
                       orderby i.SurvDiscReason
                       select i;

            return item.ToList();
        }

        public List<CalculationTools> GetCalculationToolsList() 
        {
            var item = from i in _clinContext.CalculationTools
                       select i;

            return item.ToList();
        }
    }
}
