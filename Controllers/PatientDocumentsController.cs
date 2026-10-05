using ClinicalXPDataConnections.Meta;
using ClinicX.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.StaticFiles;

namespace ClinicX.Controllers
{
    public class PatientDocumentsController : Controller
    {
        private readonly IConfiguration _config;
        private readonly IPatientDataAsync _patientData;
        private readonly IConstantsDataAsync _constantsData;
        private readonly PatientDocumentsVM _pdvm;
        private readonly IWebHostEnvironment _env;

        public PatientDocumentsController(IConfiguration config, IPatientDataAsync patientData, IConstantsDataAsync constantsData, IWebHostEnvironment env)
        {
            _config = config;
            _patientData = patientData;
            _constantsData = constantsData;
            _pdvm = new PatientDocumentsVM();
            _env = env;
        }

        public async Task<IActionResult> Index(int mpi)
        {
            _pdvm.patient = await _patientData.GetPatientDetails(mpi);
            string folderPath = await _constantsData.GetConstant("PatientDocsFldPath", 2);
            folderPath += _pdvm.patient.CGU_No.Replace(".", "_"); // + "\\Letters"; //because obviously they're not always in a sub folder called "Letters", why would anything be consistent here???

            _pdvm.folderPath = folderPath;

            string absolutePath = Path.GetFullPath(_pdvm.folderPath);

            if (Directory.Exists(absolutePath))
            {
                _pdvm.fileNames = new List<string>();
                var files = Directory.GetFiles(absolutePath);

                if (files.Count() > 0)
                {
                    foreach (var item in files)
                    {
                        _pdvm.fileNames.Add(item.Replace(folderPath, ""));
                    }
                    goto FolderFound;
                }
                else
                {
                    goto CheckLettersFolder;
                }
                
            }
            else //so we have to check for a "Letters" folder as well. Because consistency isn't a thing we do here.
            {
                goto CheckLettersFolder;   
            }

        CheckLettersFolder:
            folderPath += "\\Letters";

            _pdvm.folderPath = folderPath;
            absolutePath = Path.GetFullPath(_pdvm.folderPath);

            if (Directory.Exists(absolutePath))
            {
                _pdvm.fileNames = new List<string>();
                var files = Directory.GetFiles(absolutePath);

                foreach (var item in files)
                {
                    _pdvm.fileNames.Add(item.Replace(folderPath, ""));
                }
                goto FolderFound;
            }
            else
            {
                return RedirectToAction("PatientDetails", "Patient", new { id = mpi, success = false, message = "No patient folder found" });
            }


            FolderFound:
                return View(_pdvm);
        }

        [HttpGet]
        public IActionResult ViewFile(string subPath, string fileName)
        {
            //string fullPath = Path.Combine(subPath, fileName);
            string fullPath = subPath + fileName;
                        
            string canonicalPath = Path.GetFullPath(fullPath);
            
                        
            if (!System.IO.File.Exists(canonicalPath))
            {
                return NotFound("The requested file does not exist.");
            }
                        
            var provider = new FileExtensionContentTypeProvider();
            if (!provider.TryGetContentType(canonicalPath, out string contentType))
            {
                contentType = "application/octet-stream";
            }
            
            byte[] fileBytes = System.IO.File.ReadAllBytes(canonicalPath);

            if (Path.GetExtension(fullPath).Equals(".docx", StringComparison.OrdinalIgnoreCase))
            {
                return File(fileBytes, contentType, fileName);
            }

            return File(fileBytes, contentType);
        }
    }
}
