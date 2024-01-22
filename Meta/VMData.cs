using ClinicX.Data;
using ClinicX.Models;
using ClinicX.ViewModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.Data.SqlClient;
using System.Data;

namespace ClinicX.Meta
{
    public class VMData
    {
        private readonly ClinicalContext _context;
        private readonly DocumentContext? _docContext;

        public VMData(ClinicalContext context)
        {
            _context = context;
        }
        
        public VMData(DocumentContext docContext)
        {            
            _docContext = docContext;
        }

        public Patients GetPatientDetails(int id)
        {
            var patient = _context.Patients?.FirstOrDefault(i => i.MPI == id);
            return patient;
        } //Get patient details from MPI

        public PatientPathway GetPathwayDetails(int id)
        {
            var pathway = _context.PatientPathway.OrderBy(i => i.REFERRAL_DATE).FirstOrDefault(i => i.MPI == id);
            return pathway;
        } //Get earliest active pathway for patient by MPI

        public List<Alert> GetAlerts(int id)
        {
            var alerts = from a in _context.Alert
                        where a.MPI == id & a.EffectiveToDate == null
                        orderby a.AlertID
                        select a;            

            return alerts.ToList();
        } //Get list of alerts for patient by MPI
        
        public Referrals GetReferralDetails(int id)
        {
            var referral = _context.Referrals?.FirstOrDefault(i => i.refid == id);
            return referral;
        } //Get details of referral by RefID

        public ActivityItems GetActivityDetails(int id) //Get details of any activity item by RefID
        {
            var referral = _context.ActivityItems?.FirstOrDefault(i => i.RefID == id);
            return referral;
        }                

        public List<Referrals> GetReferrals(int id) //Get list of active referrals for patient by MPI
        {
            var referrals = from r in _context.Referrals
                           where r.MPI == id & r.RefType.Contains("Referral") & r.COMPLETE != "Complete"
                           orderby r.RefDate
                           select r;            

            return referrals.ToList();
        }

        public List<Diary> GetDiaryList(int id) //Get list of diary entries for patient by MPI
        {
            var pat = _context.Patients.FirstOrDefault(p => p.MPI == id);

            var diary = from d in _context.Diary
                        where d.WMFACSID == pat.WMFACSID
                        orderby d.DiaryDate
                        select d;

            return diary.ToList();
        }

        public List<Relatives> GetRelativesDetails(int id) //Get list of relatives of patient by MPI
        {
            var patient = _context.Patients.FirstOrDefault(i => i.MPI == id);
            int iWmfacsID = patient.WMFACSID;

            var relative = from r in _context.Relatives
                           where r.WMFACSID == iWmfacsID
                           select r;           

            return relative.ToList();
        }

        public List<HPOTermDetails> GetHPOTerms(int id)
        {
            var notes = from n in _context.HPOTermDetails
                        where n.MPI == id
                        select n;            

            return notes.ToList();
        } //Get list of HPO codes added to a patient by MPI

        public List<DiseaseList> GetDisease()
        {
            var items = from i in _context.Diseases
                        orderby i.DESCRIPTION
                        select i;           

            return items.ToList();
        } //Get list of all diseases

        public List<DiseaseStatusList> GetStatusList()
        {
            var items = from i in _context.DiseaseStatusList
                        select i;
            
            return items.ToList();
        } //Get list of all possible disease statuses
        
        public Diagnosis GetDiagnosisDetails(int id) //Get details of diagnosis by the diagnosis ID
        {
            var diagnosis = _context.Diagnosis.FirstOrDefault(i => i.ID == id);

            return diagnosis;
        }  

        public DictatedLetters GetDictatedLetterDetails(int iDotID) //Get details of DOT letter by its DotID
        {
            var letter = _context.DictatedLetters.FirstOrDefault(l => l.DoTID == iDotID);

            return letter;
        }

        public List<DictatedLettersPatients> GetDictatedLettersPatients(int iDotID) //Get list of patients added to a DOT letter by the DotID
        {
            var patient = from p in _context.DictatedLettersPatients
                          where p.DOTID == iDotID
                          select p;

            List<DictatedLettersPatients> patients = new List<DictatedLettersPatients>();

            foreach (var p in patient)
            {
                patients.Add(new DictatedLettersPatients() { DOTID = p.DOTID });
            }

            return patients;
        }

        public List<DictatedLettersCopies> GetDictatedLettersCopies(int iDotID) //Get list of all CCs added to a DOT letter by DotID
        {
            var copies = from c in _context.DictatedLettersCopies
                       where c.DotID == iDotID
                       select c;            

            return copies.ToList();
        }        

        public List<Patients> GetPatients(int iDotID) //Get list of all patients in the family that can be added to a DOT, by the DotID
        {
            var letter = _context.DictatedLetters.FirstOrDefault(l => l.DoTID == iDotID);
            int? impi = letter.MPI;
            var pat = _context.Patients.FirstOrDefault(p => p.MPI == impi.GetValueOrDefault());

            var patients = from p in _context.Patients
                           where p.PEDNO == pat.PEDNO
                           select p;

            return patients.ToList();
        }

        public List<HPOTerms> GetHPOTerms() //Get list of all possible HPO codes
        {
            var terms = from t in _context.HPOTerms
                       select t;            

            return terms.ToList();
        }

        public List<HPOTermDetails> GetExistingHPOTerms(int id) //Get list of all HPO codes added to a clinical note, by the ClinicalNoteID
        {
            var terms = from t in _context.HPOTermDetails
                       where t.ClinicalNoteID == id
                       select t;           

            return terms.ToList();
        }

        public ClinicalNotes GetClinicalNoteDetails(int? iNoteID) //Get details of a clinical note by ClinicalNotesID
        {
            var note = _context.ClinicalNotes.FirstOrDefault(i => i.ClinicalNoteID == iNoteID);

            return note;
        }

        public List<HPOExtractVM> GetExtractedTerms(int iNoteID, IConfiguration _config) //Get list of HPO codes that can be extracted from a clinical note, by ClinicalNoteID
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

        public List<TestList> GetTests() //Get list of all available tests
        {
            var items = from i in _context.Tests
                        select i;           

            return items.ToList();
        }

        public Triages GetTriageDetails(int? iIcpID) //Get details of ICP from the IcpID
        {
            var icp = _context.Triages.FirstOrDefault(i => i.ICPID == iIcpID);
            return icp;
        }

        public List<Triages> GetTriageList(string sUsername) //Get list of all outstanding triages for a specific user (by login name)
        {
            var triages = from t in _context.Triages
                         where t.LoginDetails == sUsername
                         orderby t.RefDate descending
                         select t;           

            return triages.ToList();
        }

        public List<ICPCancer> GetCancerICPList(string sUsername) //Get list of all open Cancer ICP Reviews for a specific user (by login name)
        {
            var user = _context.StaffMembers.FirstOrDefault(s => s.EMPLOYEE_NUMBER == sUsername);
            string sStaffCode = user.STAFF_CODE;

            var icps = from i in _context.ICPCancer
                      where i.ActOnRefBy != null && i.FinalReviewed == null && i.GC_CODE == sStaffCode
                      select i;
            
            return icps.ToList();
        }

        public List<ICPActionsList> GetICPCancerActionsList() //Get list of all triage actions for Cancer ICPs
        {
            var actions = from a in _context.ICPCancerActionsList
                         where a.InUse == true
                         orderby a.ID
                         select a;
           
            return actions.ToList();
        }

        public List<ICPGeneralActionsList> GetICPGeneralActionsList() //Get list of all "treatpath" items for General ICPs
        {
            var actions = from a in _context.ICPGeneralActionsList
                         where a.InUse == true
                         orderby a.ID
                         select a;
           
            return actions.ToList();
        }

        public List<ICPGeneralActionsList2> GetICPGeneralActionsList2() //Get list of all "treatpath2" items for General ICPs
        {
            var actions = from a in _context.ICPGeneralActionsList2
                         where a.InUse == true
                         orderby a.ID
                         select a;
       
            return actions.ToList();
        }

        public List<ClinicalFacilityList> GetClinicalFacilities() //Get list of all clinic facilities where we hold clinics
        {
            var facs = from f in _context.ClinicalFacilities
                      where f.NON_ACTIVE == 0
                      select f;
        
            return facs.ToList();
        }

        public List<StaffMemberList> GetClinicians() //Get list of all clinical staff members currently in post
        {
            var clinicians = from s in _context.StaffMembers
                        where s.InPost == true && (s.CLINIC_SCHEDULER_GROUPS == "GC" || s.CLINIC_SCHEDULER_GROUPS == "Consultant")
                        orderby s.NAME
                        select s;
          
            return clinicians.ToList();
        }

        public List<StaffMemberList> GetStaffMember() //Get list of all staff members currently in post 
        {
            var sm = from s in _context.StaffMembers
                     where s.InPost.Equals(true)
                     orderby s.NAME
                     select s;

            return sm.ToList();
        }

        public ICPGeneral GetGeneralICPDetails(int? iIcpID) //Get details of a general ICP by the IcpID
        {
            var icp = _context.ICPGeneral.FirstOrDefault(c => c.ICPID == iIcpID);
            return icp;
        }

        public ICPCancer GetCancerICPDetails(int? iIcpID) //Get details of a cancer ICP by the IcpID
        {
            var icp = _context.ICPCancer.FirstOrDefault(c => c.ICP_Cancer_ID == iIcpID);
            return icp;
        }

        public List<Risk> GetRiskList(int? iIcpID) //Get list of all risk items for an ICP (by IcpID)
        {
            var icp = _context.ICPCancer.FirstOrDefault(c => c.ICP_Cancer_ID == iIcpID);

            var risks = from r in _context.Risk
                       where r.MPI == icp.MPI
                       select r;
           
            return risks.ToList();
        }

        public List<Surveillance> GetSurveillanceList(int? iIcpID) //Get list of all surveillance recommendations for an ICP (by IcpID)
        {
            var icp = _context.ICPCancer.FirstOrDefault(c => c.ICP_Cancer_ID == iIcpID);

            var surveillances = from r in _context.Surveillance
                               where r.MPI == icp.MPI
                               select r;

            return surveillances.ToList();
        }

        public Risk GetRiskDetails(int? iRiskID) //Get details of risk item by RiskID
        {
            var risk = _context.Risk.FirstOrDefault(c => c.RiskID == iRiskID);
            return risk;
        }

        public Surveillance GetSurvDetails(int? iRiskID) //Get details of surveillance recommendation by RiskID
        {
            var surv = _context.Surveillance.FirstOrDefault(c => c.RiskID == iRiskID);
            return surv;
        }

        public List<Eligibility> GetTestingEligibilityList(int? iIcpID) //Get list of testing aligibility codes by IcpID
        {
            var icp = _context.ICPCancer.FirstOrDefault(c => c.ICP_Cancer_ID == iIcpID);

            var eligibilities = from e in _context.Eligibility
                              where e.MPI == icp.MPI
                              select e;

            return eligibilities.ToList();
        }
        
        public List<NoteTypeList> GetNoteTypes() //Get list of possible types of clinical note
        {
            var notetypes = from t in _context.NoteTypes
                           where t.NoteInUse == true
                           select t;

            return notetypes.ToList();
        }

        public List<ActivityItems> GetClinicDetailsList(int iRefId) //Get details of an appointment by the RefID
        {
            var cl = from c in _context.ActivityItems
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

        public List<OutcomeList> GetOutcomes() //Get list of outcomes for clinic appointments
        {
            var outcomes = from o in _context.Outcomes
                          where o.DEFAULT_CLINIC_STATUS.Equals("Active")
                          select o;

            return outcomes.ToList();
        }

        public List<Ethnicity> GetEthnicitiesList() //Get list of ethnicities
        {
            var ethnicities = from e in _context.Ethnicity
                         orderby e.Ethnic
                         select e;

            return ethnicities.ToList();
        }

        public StaffMember GetStaffMember(string suser) //Get details of a staff member by login name
        {
            var item = _docContext.StaffMember.FirstOrDefault(i => i.EMPLOYEE_NUMBER == suser);
            return item;
        }

        public Patient GetPatient(int impi) //Get details about a patient (from the documents context) by MPI
        {
            var item = _docContext.Patient.FirstOrDefault(i => i.MPI == impi);
            return item;
        }

        public DocumentsContent GetDocument(int id) //Get content for a type of standard letter by its ID
        {
            var item = _docContext.DocumentsContent.FirstOrDefault(d => d.DocContentID == id);
            return item;
        }

        public Referrer GetReferrer(string sref) //Get details about the referring clinician (by its code)
        {
            var item = _docContext.Referrer.FirstOrDefault(d => d.MasterClinicianCode == sref);
            return item;
        }

        public string GetCC(Referrer referrer) //Get details of CC address
        {
            string cc = "";
            var facility = _docContext.Facility.FirstOrDefault(f => f.MasterFacilityCode == referrer.FACILITY);

            cc = cc + Environment.NewLine + facility.NAME + Environment.NewLine + facility.ADDRESS + Environment.NewLine
                + facility.CITY + Environment.NewLine + facility.STATE + Environment.NewLine + facility.ZIP;
            return cc;
        }

        public ExternalFacility GetFacilityDetails(string sref) //Get details of external/referring facility
        {
            var item = _context.ExternalFacility.FirstOrDefault(f => f.MasterFacilityCode == sref);
            return item;
        }        

        public List<ExternalFacility> GetFacilityList() //Get list of all external/referring facilities
        {
            var facilities = from rf in _context.ExternalFacility
                             where rf.NONACTIVE == 0
                             orderby rf.NAME
                             select rf;

            return facilities.ToList();
        }

        public ExternalClinician GetClinicianDetails(string sref) //Get details of external/referring clinician
        {
            var item = _context.ExternalClinician.FirstOrDefault(f => f.MasterClinicianCode == sref);
            return item;
        }

        public List<ExternalClinician> GetClinicianList() //Get list of all external/referring clinicians
        {
            var clinicians = from rf in _context.ExternalClinician
                             where rf.NON_ACTIVE == 0
                             orderby rf.NAME
                             select rf;

            return clinicians.ToList();
        }

        public List<string> GetConsultantsList() //Get list of all consultants
        {
            var clinicians = from rf in _context.StaffMembers
                             where rf.InPost == true && rf.CLINIC_SCHEDULER_GROUPS == "Consultant"
                             orderby rf.NAME
                             select rf.NAME;

            return clinicians.ToList();
        }

        public List<string> GetGCList() //Get list of all GCs
        {
            var clinicians = from rf in _context.StaffMembers
                             where rf.InPost == true && rf.CLINIC_SCHEDULER_GROUPS == "GC"
                             orderby rf.NAME
                             select rf.NAME;

            return clinicians.ToList();
        }

        public List<string> GetSecTeams() //Get list of all secretarial teams
        {
            var secteams = from rf in _context.StaffMembers
                             where rf.BILL_ID != null && rf.BILL_ID.Contains("Team")                            
                             select rf.BILL_ID;

            return secteams.Distinct().ToList();
        }

        public List<Caseload> GetCaseload(string sStaffCode) //Get caseload for clinician
        {
            var caseload = from c in _context.Caseload
                           where c.StaffCode == sStaffCode
                           select c;

            return caseload.ToList();
        }
    }
}
