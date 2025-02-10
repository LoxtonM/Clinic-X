using Newtonsoft.Json;
using RestSharp;
using System.Data.SqlClient;
using APIControllers.Data;
using APIControllers.Meta;
using APIControllers.Models;
using Microsoft.Extensions.Configuration;

namespace APIControllers.Controllers
{
    public class PPQAPIController
    {
        private readonly APIContext _context;

        private readonly IConfiguration _config;
        private readonly IAPIPatientData _APIPatientData;
        private readonly IAPIConstantsData _constants;
        private readonly APIHPOCodeData _hpo;
        private string apiURLBase;
        private string apiURL;
        private string authKey;
        private string apiKey;

        public PPQAPIController(APIContext context, IConfiguration config)
        {
            _context = context;
            _APIPatientData = new APIPatientData(_context);
            _constants = new APIConstantsData(_context);
            _hpo = new APIHPOCodeData(_context);
            apiURLBase = _constants.GetConstant("PhenotipsURL", 1).Trim();
            authKey = _constants.GetConstant("PhenotipsAPIAuthKey", 1).Trim();
            apiKey = _constants.GetConstant("PhenotipsAPIAuthKey", 2).Trim();
            _config = config;
        }

        public bool CheckResponseValid(string response)
        {
            if (response.Contains("<html>")) //this seems to be the only way to check - if no results, it will return "<html>"
            {
                return false;
            }

            return true;
        }

        public async Task<string> GetPhenotipsPatientID(int id) //gets Phenotips ID from an existing patient (takes MPI)
        {
            //bool isExists = false;
            string phenotipsID = "";
            var patient = _APIPatientData.GetPatientDetails(id);
            apiURL = apiURLBase + $":443/rest/patients/eid/{patient.CGU_No}";
            var options = new RestClientOptions(apiURL);
            var client = new RestClient(options);
            var request = new RestRequest("");
            request.AddHeader("accept", "application/json");
            request.AddHeader("authorization", "Basic " + authKey);
            request.AddHeader("X-Gene42-Secret", apiKey);
            var response = await client.GetAsync(request);

            if (CheckResponseValid(response.Content))
            {
                dynamic dynJson = JsonConvert.DeserializeObject(response.Content);
                phenotipsID = dynJson.id;
            }

            return phenotipsID;
        }

        public async Task<string> GetPhenotipsFamilyID(int id) //gets Phenotips ID from an existing patient (takes MPI)
        {
            //bool isExists = false;
            string phenotipsID = "";
            var patient = _APIPatientData.GetPatientDetails(id);
            apiURL = apiURLBase + $":443/rest/patients/eid/{patient.CGU_No}";
            var options = new RestClientOptions(apiURL);
            var client = new RestClient(options);
            var request = new RestRequest("");
            request.AddHeader("accept", "application/json");
            request.AddHeader("authorization", "Basic " + authKey);
            request.AddHeader("X-Gene42-Secret", apiKey);
            var response = await client.GetAsync(request);

            if (CheckResponseValid(response.Content))
            {
                dynamic dynJson = JsonConvert.DeserializeObject(response.Content);
                phenotipsID = dynJson.family_id;
            }

            return phenotipsID;
        }

        public async Task<Int16> PushPtToPhenotips(int id)
        {
            var patient = _APIPatientData.GetPatientDetails(id);
            //bool isSuccess = false;
            //string sMessage = "";
            Int16 result = 0; //if patient exists, result will remain as 0

            if (GetPhenotipsPatientID(id).Result == "")
            {


                DateTime DOB = patient.DOB.GetValueOrDefault();
                int yob = DOB.Year;
                int mob = DOB.Month;
                int dob = DOB.Day;

                apiURL = apiURLBase + ":443/rest/patients";
                var options = new RestClientOptions(apiURL);
                var client = new RestClient(options);
                var request = new RestRequest("");
                request.AddHeader("authorization", $"Basic {authKey}");
                request.AddHeader("X-Gene42-Secret", apiKey);
                string apiCall = "{\"patient_name\":{\"first_name\":\"" + $"{patient.FIRSTNAME}" + "\",\"last_name\":\"" + $"{patient.LASTNAME}" + "\"}";
                apiCall = apiCall + ",\"date_of_birth\":{\"year\":" + yob.ToString() + ",\"month\":" + mob.ToString() + ",\"day\":" + dob.ToString() + "}";
                apiCall = apiCall + ",\"sex\":\"" + $"{patient.SEX.Substring(0, 1)}" + "\",\"external_id\":\"" + $"{patient.CGU_No}" + "\"}";

                request.AddJsonBody(apiCall, false);
                var response = await client.PostAsync(request);

                //SetPhenotipsOwner(patient.MPI, User.Identity.Name); - not currently necessary

                if (response.Content == "")
                {
                    //isSuccess = true;
                    //sMessage = "Push to Phenotips successful";
                    result = 1; //success result

                    string ptID = await GetPhenotipsPatientID(patient.MPI);
                    
                }
                else
                {
                    result = -1; //failure result
                    //sMessage = "Push to Phenotips failed :(";
                }
            }
            return result;
        }




        public async Task<List<Relative>> ImportRelativesFromPhenotips(int id)
        {
            var patient = _APIPatientData.GetPatientDetails(id);
            List<Relative> relatives = new List<Relative>(); //because it has to return something so need to make the empty list first

            string phenotipsID = GetPhenotipsFamilyID(id).Result;

            if (phenotipsID != null && phenotipsID != "")
            {
                phenotipsID = phenotipsID.Substring(phenotipsID.Length - 10);

                List<Relative> relListAll = new List<Relative>();

                apiURL = apiURLBase + $":443/get/PhenoTips/FamilyPedigreeInterface?action=familyinfo&document_id={phenotipsID}";

                var options = new RestClientOptions(apiURL);
                var client = new RestClient(options);
                var request = new RestRequest("");
                request.AddHeader("accept", "application/json");
                request.AddHeader("authorization", "Basic " + authKey);
                request.AddHeader("X-Gene42-Secret", apiKey);
                var response = await client.GetAsync(request);
                dynamic dynJson = JsonConvert.DeserializeObject(response.Content);
                //http://localhost:7168/Patient/PatientDetails?id=227775

                int relID = 0;

                foreach (var item in dynJson.pedigree.members)
                {
                    if ((item.properties.patient_name.first_name != null && item.properties.patient_name.first_name != "")
                            && (item.properties.patient_name.last_name != null && item.properties.patient_name.last_name != ""))
                    {
                        relID += 1;
                        DateTime dob = DateTime.Parse("1900-01-01");
                        DateTime dod = DateTime.Parse("1900-01-01");
                        string gender;
                        if (item.properties.date_of_birth.year != null && item.properties.date_of_birth.month != null && item.properties.date_of_birth.day != null)
                        {
                            dob = DateTime.Parse(item.properties.date_of_birth.year.ToString() + "-" +
                                            item.properties.date_of_birth.month.ToString() + "-" +
                                            item.properties.date_of_birth.day.ToString());
                        }
                        if (item.properties.date_of_death.year != null && item.properties.date_of_death.month != null && item.properties.date_of_death.day != null)
                        {
                            dod = DateTime.Parse(item.properties.date_of_death.year.ToString() + "-" +
                                            item.properties.date_of_death.month.ToString() + "-" +
                                            item.properties.date_of_death.day.ToString());
                        }

                        gender = item.properties.sex;

                        if (gender != "M" && gender != "F")
                        {
                            gender = "U";
                        }

                        relListAll.Add(new Relative
                        {
                            RelForename1 = item.properties.patient_name.first_name,
                            RelSurname = item.properties.patient_name.last_name,
                            DOB = dob,
                            DOD = dod,
                            RelSex = gender,
                            WMFACSID = patient.WMFACSID,
                            relsid = relID
                        });
                    }
                }

                APIRelativeData relData = new APIRelativeData(_context);

                foreach (var rel in relListAll)
                {
                    if (relData.GetRelativeDetailsByName(rel.RelForename1, rel.RelSurname).Count() == 0 &&
                            rel.RelForename1 != patient.FIRSTNAME && rel.RelSurname != patient.LASTNAME)
                    {
                        relatives.Add(new Relative
                        {
                            relsid = rel.relsid,
                            WMFACSID = rel.WMFACSID,
                            RelForename1 = rel.RelForename1,
                            RelSurname = rel.RelSurname,
                            DOB = rel.DOB,
                            DOD = rel.DOD,
                            RelSex = rel.RelSex
                        });
                    }
                }
            }

            return relatives.ToList();
        }

        public async Task<Int16> SchedulePPQ(int id, string ppqType)
        {
            var patient = _APIPatientData.GetPatientDetails(id);
            //bool isSuccess = false;
            //string sMessage = "";
            Int16 result = 0;

            string pID = "";
            string ppqID;
            if (ppqType == "Cancer")
            {
                ppqID = "bwch_cancer";
            }
            else
            {
                ppqID = "bwch_general";
            }

            pID = GetPhenotipsPatientID(id).Result;            

            if (!CheckPPQExists(id, ppqType).Result)
            {               
                if (pID != "" && pID != null)
                {
                    DateTime DOB = patient.DOB.GetValueOrDefault();
                    int yob = DOB.Year;
                    int mob = DOB.Month;
                    int dob = DOB.Day;

                    apiURL = apiURLBase + ":443/rest/questionnaire-scheduler/create";
                    var options = new RestClientOptions(apiURL);
                    var client = new RestClient(options);
                    var request = new RestRequest("");
                    request.AddHeader("authorization", $"Basic {authKey}");
                    request.AddHeader("X-Gene42-Secret", apiKey);
                    string apiCall = "{\"questionnaireId\":\"" + $"{ppqID}" + "\",";
                    apiCall = apiCall + "\"mrn\":\"" + $"{patient.CGU_No}" + "\","; //mrn needs to be the "External Identifier" - the CGU number in this case
                    apiCall = apiCall + "\"firstName\":\"" + $"{patient.FIRSTNAME}" + "\",\"lastName\":\"" + $"{patient.LASTNAME}" + "\"";
                    apiCall = apiCall + ",\"dateOfBirth\":{\"day\":" + dob.ToString() + ",\"month\":" + mob.ToString() + ",\"year\":" + yob.ToString() + "},";
                    apiCall = apiCall + "\"associatedStudy\":null}";
                    
                    request.AddJsonBody(apiCall, false);
                    var response = await client.PostAsync(request);

                    if (response.Content.Contains("{\"workflowStatus\""))
                    {
                        result = 1; //success result                    
                    }
                    else
                    {
                        result = -1; //failure result                    
                    }
                }
            }
            return result;
        }

        public async Task<bool> CheckPPQExists(int id, string ppqType)
        {
            var patient = _APIPatientData.GetPatientDetails(id);
            //bool isSuccess = false;
            //string sMessage = "";
            bool result = false;

            string pID = "";
            string ppqID;
            if (ppqType == "Cancer")
            {
                ppqID = "bwch_cancer";
            }
            else
            {
                ppqID = "bwch_general";
            }

            pID = GetPhenotipsPatientID(id).Result;

            if (pID != "" && pID != null)
            {
                DateTime DOB = patient.DOB.GetValueOrDefault();
                int yob = DOB.Year;
                int mob = DOB.Month;
                int dob = DOB.Day;

                apiURL = apiURLBase + ":443/rest/questionnaire-scheduler/search";
                var options = new RestClientOptions(apiURL);
                var client = new RestClient(options);
                var request = new RestRequest("");
                request.AddHeader("authorization", $"Basic {authKey}");
                request.AddHeader("X-Gene42-Secret", apiKey);
                string apiCall = "{\"search\":{\"questionnaireId\":\"" + $"{ppqID}" + "\",\"mrn\":\"" + $"{patient.CGU_No}" + "\",\"orderBy\":\"\",\"orderDir\":\"\",\"offset\":0,\"limit\":25}}";
                
                request.AddJsonBody(apiCall, false);
                var response = await client.PostAsync(request);
                
                string requestResult = "";


                if (!response.Content.Contains("{\"scheduleRequests\":[]"))
                {
                    result = true;
                }                
            }            
            return result;
        }

        public async Task<string> GetPPQUrl(int id, string ppqType)
        {
            var patient = _APIPatientData.GetPatientDetails(id);
            //bool isSuccess = false;
            //string sMessage = "";
            string result = "";

            string pID = "";
            string ppqID;
            if (ppqType == "Cancer")
            {
                ppqID = "bwch_cancer";
            }
            else
            {
                ppqID = "bwch_general";
            }

            pID = GetPhenotipsPatientID(id).Result;

            if (pID != "" && pID != null)
            {
                if (CheckPPQExists(id, ppqType).Result)
                {
                    DateTime DOB = patient.DOB.GetValueOrDefault();
                    int yob = DOB.Year;
                    int mob = DOB.Month;
                    int dob = DOB.Day;

                    apiURL = apiURLBase + ":443/rest/questionnaire-scheduler/search";
                    var options = new RestClientOptions(apiURL);
                    var client = new RestClient(options);
                    var request = new RestRequest("");
                    request.AddHeader("authorization", $"Basic {authKey}");
                    request.AddHeader("X-Gene42-Secret", apiKey);
                    string apiCall = "{\"search\":{\"questionnaireId\":\"" + $"{ppqID}" + "\",\"mrn\":\"" + $"{patient.CGU_No}" + "\",\"orderBy\":\"\",\"orderDir\":\"\",\"offset\":0,\"limit\":25}}";
                    

                    request.AddJsonBody(apiCall, false);
                    var response = await client.PostAsync(request);

                    if (response.Content.Contains("{\"scheduleRequests\""))
                    {
                        dynamic dynJson = JsonConvert.DeserializeObject(response.Content);
                        result = dynJson.scheduleRequests[0].questionnaireUri;
                    }
                }

            }
            return result;
        }

        public async Task<string> GetPPQQRCode(int id, string ppqType)
        {
            var patient = _APIPatientData.GetPatientDetails(id);
            //bool isSuccess = false;
            //string sMessage = "";
            string result = "";

            string pID = "";
            string ppqID;
            if (ppqType == "Cancer")
            {
                ppqID = "bwch_cancer";
            }
            else
            {
                ppqID = "bwch_general";
            }

            pID = GetPhenotipsPatientID(id).Result;

            if (pID != "" && pID != null)
            {
                if (!CheckPPQExists(id, ppqType).Result)
                {

                    DateTime DOB = patient.DOB.GetValueOrDefault();
                    int yob = DOB.Year;
                    int mob = DOB.Month;
                    int dob = DOB.Day;

                    apiURL = apiURLBase + ":443/rest/questionnaire-scheduler/search";
                    var options = new RestClientOptions(apiURL);
                    var client = new RestClient(options);
                    var request = new RestRequest("");
                    request.AddHeader("authorization", $"Basic {authKey}");
                    request.AddHeader("X-Gene42-Secret", apiKey);
                    string apiCall = "{\"search\":{\"questionnaireId\":\"" + $"{ppqID}" + "\",\"mrn\":\"" + $"{patient.CGU_No}" + "\",\"orderBy\":\"\",\"orderDir\":\"\",\"offset\":0,\"limit\":25}}";

                    request.AddJsonBody(apiCall, false);
                    var response = await client.PostAsync(request);

                    if (response.Content.Contains("{\"scheduleRequests\""))
                    {
                        dynamic dynJson = JsonConvert.DeserializeObject(response.Content);
                        result = dynJson.scheduleRequests[0].questionnaireQR;
                    }
                }
            }
            return result;
        }


        public async Task SetPhenotipsOwner(int id, string userName) //might not be necessary anymore...
        {
            if (id != null && id != 0)
            {
                string phenotipsID = GetPhenotipsPatientID(id).Result;

                apiURL = apiURLBase + $":443/rest/patients/{phenotipsID}/permissions";
                var options = new RestClientOptions(apiURL);
                var client = new RestClient(options);
                var request = new RestRequest("");
                request.AddHeader("content-type", "application/json");
                request.AddHeader("authorization", authKey);
                request.AddHeader("X-Gene42-Secret", apiKey);
                request.AddJsonBody("{\"owner\":{\"id\":\"xwiki:XWiki." + userName + "\"}}", false);
                var response = client.PutAsync(request);
            }
        }

    }
}