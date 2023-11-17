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
        }

        public PatientPathway GetPathwayDetails(int id)
        {
            var pathway = _context.PatientPathway.OrderBy(i => i.REFERRAL_DATE).FirstOrDefault(i => i.MPI == id);
            return pathway;
        }

        public List<Alert> GetAlerts(int id)
        {
            var alerts = from a in _context.Alert
                        where a.MPI == id & a.EffectiveToDate == null
                        orderby a.AlertID
                        select a;            

            return alerts.ToList();
        }
        
        public List<Referrals> GetReferrals(int id)
        {
            var referrals = from r in _context.Referrals
                           where r.MPI == id & r.RefType.Contains("Referral") & r.COMPLETE != "Complete"
                           orderby r.RefDate
                           select r;            

            return referrals.ToList();
        }

        public List<Relatives> GetRelativesDetails(int id)
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
        }

        public List<DiseaseList> GetDisease()
        {
            var items = from i in _context.Diseases
                        orderby i.DESCRIPTION
                        select i;           

            return items.ToList();
        }

        public List<DiseaseStatusList> GetStatusList()
        {
            var items = from i in _context.DiseaseStatusList
                        select i;
            
            return items.ToList();
        }
        
        public Diagnosis GetDiagnosisDetails(int id)
        {
            var diagnosis = _context.Diagnosis.FirstOrDefault(i => i.ID == id);

            return diagnosis;
        }

        public DictatedLetters GetDictatedLetterDetails(int iDotID)
        {
            var letter = _context.DictatedLetters.FirstOrDefault(l => l.DoTID == iDotID);

            return letter;
        }

        public List<DictatedLettersPatients> GetDictatedLettersPatients(int iDotID)
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

        public List<DictatedLettersCopies> GetDictatedLettersCopies(int iDotID)
        {
            var copies = from c in _context.DictatedLettersCopies
                       where c.DotID == iDotID
                       select c;            

            return copies.ToList();
        }

        public List<Patients> GetPatients(int iDotID)
        {
            var mpi = from m in _context.DictatedLettersPatients
                      where m.DOTID == iDotID
                      select m;

            List<Patients> patients = new List<Patients>();

            foreach (var m in mpi.ToList())
            {

                var patient = _context.Patients.FirstOrDefault(p => p.MPI == m.MPI);
                int impi = patient.MPI;
                string name = patient.FIRSTNAME + " " + patient.LASTNAME;

                patients.Add(new Patients() { MPI = patient.MPI, FIRSTNAME = patient.FIRSTNAME, LASTNAME = patient.LASTNAME, DOB = patient.DOB, SOCIAL_SECURITY = patient.SOCIAL_SECURITY, CGU_No = patient.CGU_No });
            }
            return patients;
        }

        public List<HPOTerms> GetHPOTerms()
        {
            var terms = from t in _context.HPOTerms
                       select t;            

            return terms.ToList();
        }

        public List<HPOTermDetails> GetExistingHPOTerms(int id)
        {
            var terms = from t in _context.HPOTermDetails
                       where t.ClinicalNoteID == id
                       select t;           

            return terms.ToList();
        }

        public ClinicalNotes GetClinicalNoteDetails(int? iNoteID)
        {
            var note = _context.ClinicalNotes.FirstOrDefault(i => i.ClinicalNoteID == iNoteID);

            return note;
        }

        public List<HPOExtractVM> GetExtractedTerms(int iNoteID, IConfiguration _config)
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

        public List<TestList> GetTests()
        {
            var items = from i in _context.Tests
                        select i;           

            return items.ToList();
        }

        public Triages GetTriageDetails(int? iIcpID)
        {
            var icp = _context.Triages.FirstOrDefault(i => i.ICPID == iIcpID);
            return icp;
        }

        public List<Triages> GetTriageList(string sUsername)
        {
            var triages = from t in _context.Triages
                         where t.LoginDetails == sUsername
                         orderby t.RefDate descending
                         select t;           

            return triages.ToList();
        }

        public List<ICPCancer> GetCancerICPList(string sUsername)
        {
            var user = _context.StaffMembers.FirstOrDefault(s => s.EMPLOYEE_NUMBER == sUsername);
            string sStaffCode = user.STAFF_CODE;

            var icps = from i in _context.ICPCancer
                      where i.ActOnRefBy != null && i.FinalReviewed == null && i.GC_CODE == sStaffCode
                      select i;
            
            return icps.ToList();
        }

        public List<ICPActionsList> GetICPCancerActionsList()
        {
            var actions = from a in _context.ICPCancerActionsList
                         where a.InUse == true
                         orderby a.ID
                         select a;
           
            return actions.ToList();
        }

        public List<ICPGeneralActionsList> GetICPGeneralActionsList()
        {
            var actions = from a in _context.ICPGeneralActionsList
                         where a.InUse == true
                         orderby a.ID
                         select a;
           
            return actions.ToList();
        }

        public List<ICPGeneralActionsList2> GetICPGeneralActionsList2()
        {
            var actions = from a in _context.ICPGeneralActionsList2
                         where a.InUse == true
                         orderby a.ID
                         select a;
       
            return actions.ToList();
        }

        public List<ClinicalFacilityList> GetClinicalFacilities()
        {
            var facs = from f in _context.ClinicalFacilities
                      where f.NON_ACTIVE == 0
                      select f;
        
            return facs.ToList();
        }

        public List<StaffMemberList> GetClinicians()
        {
            var clinicians = from s in _context.StaffMembers
                        where s.InPost == true && (s.CLINIC_SCHEDULER_GROUPS == "GC" || s.CLINIC_SCHEDULER_GROUPS == "Consultant")
                        select s;
          
            return clinicians.ToList();
        }

        public ICPGeneral GetGeneralICPDetails(int? iIcpID)
        {
            var icp = _context.ICPGeneral.FirstOrDefault(c => c.ICPID == iIcpID);
            return icp;
        }

        public ICPCancer GetCancerICPDetails(int? iIcpID)
        {
            var icp = _context.ICPCancer.FirstOrDefault(c => c.ICP_Cancer_ID == iIcpID);
            return icp;
        }

        public List<Risk> GetRiskList(int? iIcpID)
        {
            var icp = _context.ICPCancer.FirstOrDefault(c => c.ICP_Cancer_ID == iIcpID);

            var risks = from r in _context.Risk
                       where r.MPI == icp.MPI
                       select r;
           
            return risks.ToList();
        }

        public List<Surveillance> GetSurveillanceList(int? iIcpID)
        {
            var icp = _context.ICPCancer.FirstOrDefault(c => c.ICP_Cancer_ID == iIcpID);

            var surveillances = from r in _context.Surveillance
                               where r.MPI == icp.MPI
                               select r;

            return surveillances.ToList();
        }

        public Risk GetRiskDetails(int? iRiskID)
        {
            var risk = _context.Risk.FirstOrDefault(c => c.RiskID == iRiskID);
            return risk;
        }

        public Surveillance GetSurvDetails(int? iRiskID)
        {
            var surv = _context.Surveillance.FirstOrDefault(c => c.RiskID == iRiskID);
            return surv;
        }

        public List<Eligibility> GetTestingEligibilityList(int? iIcpID)
        {
            var icp = _context.ICPCancer.FirstOrDefault(c => c.ICP_Cancer_ID == iIcpID);

            var eligibilities = from e in _context.Eligibility
                              where e.MPI == icp.MPI
                              select e;

            return eligibilities.ToList();
        }

        public ActivityItems GetClinicDetails(int iRefID)
        {
            var clinics = _context.ActivityItems.FirstOrDefault(c => c.RefID == iRefID);

            return clinics;
        }

        public List<NoteTypeList> GetNoteTypes()
        {
            var notetypes = from t in _context.NoteTypes
                           where t.NoteInUse == true
                           select t;

            return notetypes.ToList();
        }

        public List<StaffMemberList> GetStaffMember()
        {
            var sm = from s in _context.StaffMembers
                     where s.InPost.Equals(true) & (s.CLINIC_SCHEDULER_GROUPS.Equals("GC") || s.CLINIC_SCHEDULER_GROUPS.Equals("Consultant"))
                     orderby s.NAME
                     select s;

            return sm.ToList();
        }               

        public List<ActivityItems> GetClinicDetailsList(int iRefId)
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

        public List<OutcomeList> GetOutcomes()
        {
            var outcomes = from o in _context.Outcomes
                          where o.DEFAULT_CLINIC_STATUS.Equals("Active")
                          select o;

            return outcomes.ToList();
        }

        public List<Ethnicity> GetEthnicitiesList()
        {
            var ethnicities = from e in _context.Ethnicity
                         orderby e.Ethnic
                         select e;

            return ethnicities.ToList();
        }

        public StaffMember GetStaffMember(string suser)
        {
            var item = _docContext.StaffMember.FirstOrDefault(i => i.EMPLOYEE_NUMBER == suser);
            return item;
        }

        public Patient GetPatient(int impi)
        {
            var item = _docContext.Patient.FirstOrDefault(i => i.MPI == impi);
            return item;
        }

        public DocumentsContent GetDocument(int id)
        {
            var item = _docContext.DocumentsContent.FirstOrDefault(d => d.DocContentID == id);
            return item;
        }

        public Referrer GetReferrer(string sref)
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


    }
}
