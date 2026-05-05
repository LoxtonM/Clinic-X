using ClinicalXPDataConnections.Data;
using ClinicalXPDataConnections.Meta;
using ClinicalXPDataConnections.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ClinicX.Controllers
{
    public class LetterPreviewController : Controller
    {
        private readonly IConfiguration _config;
        //private readonly ClinicalContext _context;
        //private readonly DocumentContext _documentContext;
        private readonly LetterController _lc;

        public LetterPreviewController(IConfiguration config, LetterController lc, ClinicalContext context, DocumentContext documentContext)
        {
            _config = config;
            _lc = lc;
            //_context = context;
           // _documentContext = documentContext;
        }

        public async Task<IActionResult> DoLetter(int docID, int mpi, int refID, string referrerCode)
        {
            await _lc.DoPDF(docID, mpi, refID, User.Identity.Name, referrerCode, "", "", 0, "", false, false, 0, "", "", 0, "",
                        "", null, true, "", 0, true);

            return File($"~/StandardLetterPreviews/preview-{User.Identity.Name}.pdf", "Application/PDF");
        }
    }
}
