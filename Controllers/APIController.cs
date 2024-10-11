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
            int pageNo = 1;
            int setSize = 10;

            apiURL = $"https://ontology.jax.org/api/hp/search?q={searchTerm}&page={pageNo.ToString()}&limit={setSize.ToString()}";  //I don't know what the page and limit actually means!

            var options = new RestClientOptions(apiURL);
            var client = new RestClient(options);
            var request = new RestRequest("");
            request.AddHeader("accept", "application/json");

            var response = await client.GetAsync(request);

            dynamic dynJson = JsonConvert.DeserializeObject(response.Content);
            int totalCount = dynJson.totalCount;

            int i = 0;

            if (totalCount > setSize) //because I can't just have a number that returns all results - that would be far too convenient!
            {
                var batch = BatchInteger(totalCount, setSize); 

                foreach (var item in batch)
                {
                    foreach (var term in dynJson.terms)
                    {
                        i += 1; //the model needs an ID field, but as this is not supplied by ontology themselves, we need to fabricate it here
                        string hpoID = term.id;
                        if (hpoCodes.Where(t => t.TermCode == hpoID).Count() == 0) //because it's duplicating them all for some reason!!!
                        {
                            hpoCodes.Add(new HPOTerm { TermCode = term.id, Term = term.name, ID = i });
                        }
                    }
                    pageNo++;
                    apiURL = $"https://ontology.jax.org/api/hp/search?q={searchTerm}&page={pageNo.ToString()}&limit={item.ToString()}";
                    options = new RestClientOptions(apiURL);
                    client = new RestClient(options);
                    request = new RestRequest("");
                    request.AddHeader("accept", "application/json");

                    response = await client.GetAsync(request);

                    dynJson = JsonConvert.DeserializeObject(response.Content);
                    

                }
            }
            else
            {
                foreach (var term in dynJson.terms)
                {
                    i += 1;
                    Console.WriteLine(term.id);
                    hpoCodes.Add(new HPOTerm { TermCode = term.id, Term = term.name, ID = i });
                }
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
            apiURL = $"https://ontology.jax.org/api/hp/terms";

            var options = new RestClientOptions(apiURL);
            var client = new RestClient(options);
            var request = new RestRequest("");
            request.AddHeader("accept", "application/json");

            var response = await client.GetAsync(request);

            dynamic dynJson = JsonConvert.DeserializeObject(response.Content);
            
            foreach (var item in dynJson)
            {   
                string hpoID = item.id;
                string hpoName = item.name;                

                if (_hpo.GetHPOTermByTermCode(hpoID) == null)
                {
                    _hpo.AddHPOTermToDatabase(hpoID, hpoName.Replace("'", "''"), User.Identity.Name, _config);

                    int hpocodeid = _hpo.GetHPOTermByTermCode(hpoID).ID;

                    foreach (var synonym in item.synonyms)
                    {
                        string syn = synonym.ToString();
                        syn = syn.Replace("{", "").Replace("}", "");
                        _hpo.AddHPOSynonymToDatabase(hpocodeid, syn, User.Identity.Name, _config);
                    }                    
                }                
            }           

            return RedirectToAction("Index", "Home");
        }

        private IEnumerable<int> BatchInteger(int total, int batchSize)
        { //this is supposed to split the total into batches of 10, so each one can be run as a separate page - but the API itself is a bit janky
            if (batchSize == 0)
            {
                yield return 0;
            }

            if (batchSize >= total)
            {
                yield return total;
            }
            else
            {
                int rest = total % batchSize;
                int divided = total / batchSize;
                if (rest > 0)
                {
                    divided += 1;
                }

                for (int i = 0; i < divided; i++)
                {
                    if (rest > 0 && i == divided - 1)
                    {
                        yield return rest;
                    }
                    else
                    {
                        yield return batchSize;
                    }
                }
            }
        }

    }
}
