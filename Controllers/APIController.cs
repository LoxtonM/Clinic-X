using ClinicX.Data;
using ClinicX.Meta;
using ClinicX.Models;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RestSharp;
using static System.Runtime.CompilerServices.RuntimeHelpers;

namespace ClinicX.Controllers
{
    public class APIController : Controller
    {
        private readonly ClinicalContext _clinContext;
        private readonly IConfiguration _config;
        private readonly IPatientData _patientData;
        private readonly IConstantsData _constants;
        private readonly IHPOCodeData _hpo;
        private string apiURL;
        private string authKey;
        private string apiKey;

        public APIController(ClinicalContext clinContext, IConfiguration config)
        {
            _clinContext = clinContext;            
            _patientData = new PatientData(_clinContext);
            _constants = new ConstantsData(_clinContext);
            _hpo = new HPOCodeData(_clinContext);
            apiURL = _constants.GetConstant("PhenotipsURL", 2).Trim();
            authKey = "Basic bW5sbjpFZGVuUHJpbWUxOTg0";
            apiKey = "T-Si8nmMjT8SxJGIxgZ2oMw0135TnPUQ0XeA8Nva";
            _config = config;
        }

        public async Task<IActionResult> PushPtToPhenotips(int id)
        {
            var patient = _patientData.GetPatientDetails(id);
            DateTime DOB = patient.DOB.GetValueOrDefault();
            int yob = DOB.Year;
            int mob = DOB.Month;
            int dob = DOB.Day;

            apiURL = apiURL + ":443/rest/patients";
            var options = new RestClientOptions(apiURL);
            var client = new RestClient(options);
            var request = new RestRequest("");
            request.AddHeader("authorization", $"Basic {authKey}");
            request.AddHeader("X-Gene42-Secret", apiKey);
            string apiCall = "{\"patient_name\":{\"first_name\":\"" + $"{patient.FIRSTNAME}" + "\",\"last_name\":\"" + $"{patient.LASTNAME}";
            apiCall = apiCall + "\"},\"date_of_birth\":{\"year\":" + yob.ToString() + ",\"month\":" + mob.ToString() + ",\"day\":" + dob.ToString();
            apiCall = apiCall + "},\"sex\":\"" + $"{patient.SEX.Substring(0, 1)}" + "\",\"external_id\":\"" + $"{patient.CGU_No}" + "\"}";

            request.AddJsonBody(apiCall, false);
            var response = await client.PostAsync(request);

            SetPhenotipsOwner(patient.CGU_No, User.Identity.Name);

            bool isSuccess = false;

            if (response.ToString().Contains("StatusCode: OK"))
            {
                isSuccess = true;
            }

            return RedirectToAction("PatientDetails", "Patient", new { id = patient.MPI, success = isSuccess });
        }

        public async Task<IActionResult> ImportRelativesFromPhenotips(int id)
        {
            var patient = _patientData.GetPatientDetails(id);

            //placeholder - cool stuff goes here

            return RedirectToAction("PatientDetails", "Patient", new { id = patient.MPI });
        }


        public void SetPhenotipsOwner(string externalID, string userName)
        {
            if (externalID != null && externalID != "")
            {
                apiURL = apiURL + ":443/rest/patients";
                var options = new RestClientOptions($"{apiURL}/eid/{externalID}");
                var client = new RestClient(options);
                var request = new RestRequest("");
                request.AddHeader("accept", "application/json");
                request.AddHeader("authorization", authKey);
                request.AddHeader("X-Gene42-Secret", apiKey);
                var response = client.GetAsync(request);

                string phenotipsID;

                var obj = JObject.Parse(response.ToString());
                phenotipsID = (string)obj.SelectToken("id");

                var options2 = new RestClientOptions($"{apiURL}/{phenotipsID}/permissions");
                var client2 = new RestClient(options);
                var request2 = new RestRequest("");
                request2.AddHeader("content-type", "application/json");
                request2.AddHeader("authorization", authKey);
                request2.AddHeader("X-Gene42-Secret", apiKey);
                request2.AddJsonBody("{\"owner\":{\"id\":\"" + userName + "\"}}", false);
                var response2 = client2.PutAsync(request2);
            }
        }

        public async Task<List<HPOTerm>> GetHPOCodes(string searchTerm)
        {
            List<HPOTerm> hpoCodes = new List<HPOTerm>();

            searchTerm = searchTerm.Replace(" ", "%20");

            apiURL = $"https://ontology.jax.org/api/hp/search?q={searchTerm}&page=1&limit=50";  //I don't know what the page and limit actually means,
                                                                                                //but anything more than 50 seems to result in no results
            var options = new RestClientOptions(apiURL);
            var client = new RestClient(options);
            var request = new RestRequest("");
            request.AddHeader("accept", "application/json");
            
            var response = await client.GetAsync(request);

            
            dynamic dynJson = JsonConvert.DeserializeObject(response.Content);
            int i = 0;
            foreach (var item in dynJson.terms)
            {
                i += 1;                
                hpoCodes.Add(new HPOTerm { TermCode = item.id, Term = item.name, ID = i });
            }

            return hpoCodes.OrderBy(c => c.TermCode).ToList();
        }

        public async Task<HPOTerm> GetHPODataByTermCode(string hpoTermCode)
        {
            HPOTerm hPOTerm;

            apiURL = $"https://ontology.jax.org/api/hp/terms/{hpoTermCode.Replace(":", "%3A")}";  
                                                                                                
            var options = new RestClientOptions(apiURL);
            var client = new RestClient(options);
            var request = new RestRequest("");
            request.AddHeader("accept", "application/json");

            var response = await client.GetAsync(request);


            dynamic dynJson = JsonConvert.DeserializeObject(response.Content);
            
            hPOTerm = new HPOTerm{ ID = 1, TermCode = dynJson.id, Term = dynJson.name };


            return hPOTerm;
        }

        public async Task<List<string>> GetHPOSynonymsByTermCode(string hpoTermCode)
        {
            List<string> hpoSynonyms = new List<string>();

            apiURL = $"https://ontology.jax.org/api/hp/terms/{hpoTermCode.Replace(":", "%3A")}";

            var options = new RestClientOptions(apiURL);
            var client = new RestClient(options);
            var request = new RestRequest("");
            request.AddHeader("accept", "application/json");

            var response = await client.GetAsync(request);


            dynamic dynJson = JsonConvert.DeserializeObject(response.Content);

            foreach (var item in dynJson.synonyms)
            {                
                hpoSynonyms.Add(item.ToString()); 
            }

            return hpoSynonyms.ToList();
        }


        public async Task<IActionResult> GetAllHPOTerms()
        {
            List<string> hpoSynonyms = new List<string>();

            apiURL = $"https://ontology.jax.org/api/hp/terms";

            var options = new RestClientOptions(apiURL);
            var client = new RestClient(options);
            var request = new RestRequest("");
            request.AddHeader("accept", "application/json");

            var response = await client.GetAsync(request);


            dynamic dynJson = JsonConvert.DeserializeObject(response.Content);

            //string json = "";

            List<HPOTerm> hpoTerms = new List<HPOTerm>();
            
            foreach (var item in dynJson)
            {   
                string hpoID = item.id;
                string hpoName = item.name;
                if (_hpo.GetHPOTermByTermCode(hpoID) == null)
                {
                    //_hpo.AddHPOTermToDatabase(hpoID, hpoName, User.Identity.Name, _config);
                    hpoTerms.Add(new HPOTerm { TermCode = hpoID, Term = hpoName });
                }                
            }       

            foreach (var term in hpoTerms)
            {
                _hpo.AddHPOTermToDatabase(term.TermCode, term.Term.Replace("'", "''"), User.Identity.Name, _config);                 
            }

            return RedirectToAction("Index", "Home");
        }

        
    }
}
