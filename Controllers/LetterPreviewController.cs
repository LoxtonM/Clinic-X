using APIControllers.Controllers;
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
        private readonly ClinicalContext _context;
        private readonly DocumentContext _documentContext;
        private readonly LetterController _lc;
        private readonly IConstantsDataAsync _constantsData;
        private readonly IDocumentsDataAsync _documentsData;
        private readonly IApiController _api;
        private readonly IReferralDataAsync _referralDataAsync;

        public LetterPreviewController(IConfiguration config, LetterController lc, ClinicalContext context, DocumentContext documentContext, 
            IConstantsDataAsync constantsData, IDocumentsDataAsync documentsData, IApiController api, IReferralDataAsync referralDataAsync)
        {
            _config = config;
            _lc = lc;
            _context = context;
            _documentContext = documentContext;
            _constantsData = constantsData;
            _documentsData = documentsData;
            _api = api;
            _referralDataAsync = referralDataAsync;
        }

        public async Task<IActionResult> DoLetter(int docID, int mpi, int refID, string referrerCode, string? recipient, string? freeText, string? enclosures, 
            string? siteText, int? relID)
        {
            string qrCodeText = ""; //check and set up the Phenotips PPQ QR code if required
            bool isPhenotipsAvailable = await _constantsData.GetConstant("PhenotipsURL", 2) == "1";

            if (isPhenotipsAvailable)
            {
                string docCode = _documentsData.GetDocumentDetails(docID).Result.DocCode;
                string pathway = _referralDataAsync.GetReferralDetails(refID).Result.PATHWAY;

                bool isPhenotipsQR = _documentsData.GetDocumentDetailsByDocCode(docCode).Result.hasPhenotipsPPQ;

                if (isPhenotipsQR)
                {
                    qrCodeText = await _api.GetPPQQRCode(mpi, pathway);
                }
            }

            LetterControllerLOCAL lc = new LetterControllerLOCAL(_context, _documentContext);

            await lc.DoPDF(docID, mpi, refID, User.Identity.Name, referrerCode, freeText, enclosures, 0, "", false, false, 0, "", "", relID, recipient,
                        siteText, null, true, qrCodeText, 0, true);

            return File($"~/StandardLetterPreviews/preview-{User.Identity.Name}.pdf", "Application/PDF");
        }
    }
}

/*
 * int id, int mpi, int refID, string user, string referrer, string? additionalText = "", string? enclosures = "", int? reviewAtAge = 0,
            string? tissueType = "", bool? isResearchStudy = false, bool? isScreeningRels = false, int? diaryID = 0, string? freeText1 = "", string? freeText2 = "",
            int? relID = 0, string? clinicianCode = "", string? siteText = "", DateTime? diagDate = null, bool? isPreview = false, string? qrCodeText = "", int? leafletID = 0,
            bool? adminToPrint = false
*/