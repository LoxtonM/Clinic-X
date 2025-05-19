using Microsoft.AspNetCore.Mvc;
using ClinicX.ViewModels;
using ClinicalXPDataConnections.Data;
using ClinicalXPDataConnections.Meta;
using ClinicX.Meta;
using PdfSharpCore.Pdf;
using PdfSharpCore.Drawing;
using PdfSharpCore.Drawing.Layout;
using ClinicX.Data;
using ClinicX.Models;
using ClinicalXPDataConnections.Models;


namespace ClinicX.Controllers;

public class VHRController : Controller
{
    private readonly ClinicalContext _clinContext;
    private readonly ClinicXContext _cXContext;
    private readonly DocumentContext _docContext;
    private readonly LetterVM _lvm;
    private readonly IPatientData _patientData;
    private readonly IStaffUserData _staffUser;
    private readonly IDocumentsData _documentsData;
    private readonly IExternalClinicianData _externalClinicianData;
    private readonly IExternalFacilityData _externalFacilityData;
    private readonly IScreeningServiceData _screenData;
    private readonly ITriageData _triageData;
    private readonly IRiskData _riskData;
    private readonly ISurveillanceData _survData;
    private readonly IBreastHistoryData _bhsData;
    private readonly IUntestedVHRGroupData _uVHRData;

    public VHRController(ClinicalContext clinContext, ClinicXContext cXContext, DocumentContext docContext)
    {
        _clinContext = clinContext;
        _cXContext = cXContext;
        _docContext = docContext;
        _lvm = new LetterVM();        
        _patientData = new PatientData(_clinContext);
        _staffUser = new StaffUserData(_clinContext);
        _documentsData = new DocumentsData(_docContext);
        _externalClinicianData = new ExternalClinicianData(_clinContext);
        _externalFacilityData = new ExternalFacilityData(_clinContext);
        _screenData = new ScreeningServiceData(_cXContext);
        _triageData = new TriageData(_clinContext);
        _riskData = new RiskData(_clinContext);
        _survData = new SurveillanceData(_clinContext);
        _bhsData = new BreastHistoryData(_cXContext);
        _uVHRData = new UntestedVHRGroupData(cXContext);
    }

    public async Task<IActionResult> Letter(int id, int mpi, string user, string referrer)
    {
        try
        {            
            _lvm.staffMember = _staffUser.GetStaffMemberDetails(user);
            _lvm.patient = _patientData.GetPatientDetails(mpi);
            _lvm.documentsContent = _documentsData.GetDocumentDetails(id);
            _lvm.referrer = _externalClinicianData.GetClinicianDetails(referrer);

            return View(_lvm);
        }
        catch (Exception ex)
        {
            return RedirectToAction("ErrorHome", "Error", new { error = ex.Message, formName = "Letter" });
        }
    }

    //Creates a preview of the DOT letter
    

    //Prints standard letter templates from the menu
    public void DoVHRPro(int id, int mpi, int icpCancerID, string user, string referrer, string screeningService, string? additionalText = "", int? diaryID = 0)
    {
        try
        {
            _lvm.staffMember = _staffUser.GetStaffMemberDetails(user);
            _lvm.patient = _patientData.GetPatientDetails(mpi);
            _lvm.documentsContent = _documentsData.GetDocumentDetails(id);
            _lvm.referrer = _externalClinicianData.GetClinicianDetails(referrer);

            int icpID = _triageData.GetCancerICPDetails(icpCancerID).ICPID;
            int refID = _triageData.GetICPDetails(icpID).REFID;

            //string docCode = _lvm.documentsContent.DocCode;
            string docCode = _lvm.documentsContent.DocCode;
            //creates a new PDF document            
            //set the fonts used for the letters
            XFont font = new XFont("Arial", 12, XFontStyle.Regular);
            XFont fontBold = new XFont("Arial", 12, XFontStyle.Bold);
            XFont fontItalic = new XFont("Arial", 12, XFontStyle.Italic);
            XFont fontSmall = new XFont("Arial", 10, XFontStyle.Regular);
            XFont fontSmallBold = new XFont("Arial", 10, XFontStyle.Bold);
            XFont fontSmallItalic = new XFont("Arial", 10, XFontStyle.Italic);            
            XFont fontTiny = new XFont("Arial", 8, XFontStyle.Regular);
            XFont fontTinyBold = new XFont("Arial", 8, XFontStyle.Bold);
            XFont fontTinyItalic = new XFont("Arial", 8, XFontStyle.Italic);
            //Load the image for the letter head
            XImage image = XImage.FromFile(@"wwwroot\Letterhead.jpg");
            
            string patName = "";
            string address = "";
            string patAddress = "";
            string salutation = "";            
            DateTime patDOB = DateTime.Now;

            string content1 = "";
            string content2 = "";
            string content3 = "";
            string content4 = "";
            string content5 = "";
            string content6 = "";           
            string quoteRef = "";
            string signOff = "";
            string sigFilename = "";
            int printCount = 1;

            string fileCGU = _lvm.patient.CGU_No.Replace(".", "-");
            string mpiString = _lvm.patient.MPI.ToString();
            string refIDString = refID.ToString();
            string dateTimeString = DateTime.Now.ToString("yyyyMMddHHmmss");            
            string diaryIDString = diaryID.ToString();
                        
            PdfDocument vhrDocument = new PdfDocument();
            vhrDocument.Info.Title = "VHRPro";
            PdfPage vhrPage = vhrDocument.AddPage();
            PdfPage vhrPage2 = vhrDocument.AddPage();
            PdfPage vhrPage3 = vhrDocument.AddPage();
            XGraphics vhrGfx = XGraphics.FromPdfPage(vhrPage);
            var vhrTf = new XTextFormatter(vhrGfx);
            XGraphics vhrGfx2 = XGraphics.FromPdfPage(vhrPage2);
            var vhrTf2 = new XTextFormatter(vhrGfx2);
            XGraphics vhrGfx3 = XGraphics.FromPdfPage(vhrPage3);
            var vhrTf3 = new XTextFormatter(vhrGfx3);
            vhrGfx.DrawImage(image, 350, 20, image.PixelWidth / 2, image.PixelHeight / 2);
            int totalLengthVHR = 140;
            int totalLengthVHR2 = 50;
            int totalLengthVHR3 = 50;
            int pageWidth = 500;
            
            //PdfSharpCore.Drawing.XRect rect1 = new PdfSharpCore.Drawing.XRect(15, totalLengthVHR, 25, 680);
            XPen pen = new XPen(XColors.Black, 1);
            XPen penLight = new XPen(XColors.Black, 0.5);
            vhrGfx.DrawRectangle(pen, new XRect(45, totalLengthVHR, pageWidth, 40));
            vhrGfx.DrawRectangle(XBrushes.DarkGray, new XRect(46, totalLengthVHR + 1, pageWidth - 2, 39));
            totalLengthVHR += 5;
            vhrTf.DrawString("Referral form to the NHSBSP for very high-risk screening (for completion by the referrer)", fontBold, XBrushes.Black, new XRect(50, totalLengthVHR, 500, 40));
            totalLengthVHR += 35;

            vhrGfx.DrawRectangle(pen, new XRect(45, totalLengthVHR, pageWidth, 80));
            vhrTf.DrawString("Patient Details", fontSmallBold, XBrushes.Black, new XRect(50, totalLengthVHR, 550, 20));
            totalLengthVHR += 15;


            patName = _lvm.patient.FIRSTNAME + " " + _lvm.patient.LASTNAME;
            patAddress = _lvm.patient.ADDRESS1;
            //if (_lvm.patient.ADDRESS2 != null) { patAddress = patAddress + _lvm.patient.ADDRESS2 + Environment.NewLine; }
            //if (_lvm.patient.ADDRESS3 != null) { patAddress = patAddress + _lvm.patient.ADDRESS3 + Environment.NewLine; }
            if (_lvm.patient.ADDRESS3 != null) { patAddress = patAddress + ", " + _lvm.patient.ADDRESS3; }
            //patAddress = patAddress + _lvm.patient.POSTCODE;

            var gp = _externalClinicianData.GetClinicianDetails(_lvm.patient.GP_Code);
            var gpFac = _externalFacilityData.GetFacilityDetails(_lvm.patient.GP_Facility_Code);            
            string gpDets = gp.TITLE + " " + gp.FIRST_NAME + " " + gp.NAME + ", " + gpFac.NAME;

            content1 = "Name:" + patName + Environment.NewLine + "Address:" + patAddress + Environment.NewLine +
                "Postcode: " + _lvm.patient.POSTCODE + Environment.NewLine +
                "Mobile number: " + _lvm.patient.PtTelMobile + Environment.NewLine + gpDets;

            vhrTf.DrawString(content1, fontSmall, XBrushes.Black, new XRect(50, totalLengthVHR, 500, 80));
            content2 = "NHS No: " + _lvm.patient.SOCIAL_SECURITY + Environment.NewLine +
                    "DOB: " + _lvm.patient.DOB.Value.ToString("dd/MM/yyyy") + Environment.NewLine + Environment.NewLine +
                    "Home number: " + _lvm.patient.TEL;

                   //"GP Practice Code: " + _lvm.patient.GP_Facility_Code + Environment.NewLine +
                   // "GP Address: ";

            
                        
            vhrTf.DrawString(content2, fontSmall, XBrushes.Black, new XRect(310, totalLengthVHR, 500, 80));
            totalLengthVHR += 65;
            
            vhrGfx.DrawRectangle(pen, new XRect(45, totalLengthVHR, 500, 125));            
            totalLengthVHR += 5;
            content3 = "Referrer Name: " + _lvm.staffMember.NAME + ", " + _lvm.staffMember.POSITION.Replace($"\r\n", ", ") + Environment.NewLine +
                "Address (with postcode): " + _lvm.documentsContent.OurAddress.Replace($"\r\n", ", ").Replace(" (Cancer Service)", "").Replace($"Tel:0121 335 8024", "").Replace("G,", "G");

            vhrTf.DrawString(content3, fontSmall, XBrushes.Black, new XRect(50, totalLengthVHR, 500, 120));
            totalLengthVHR += 20;

            ScreeningService sServ = _screenData.GetScreeningServiceDetails(_lvm.patient.GP_Facility_Code);

            //vhrTf.DrawString("Referee name:", fontSmallBold, XBrushes.Black, new XRect(50, totalLengthVHR, 500, 20));
            content4 = "Referee name:" + sServ.Contact + Environment.NewLine +
                "Address: " + sServ.Add1 + ", " + sServ.Add2 + ", " + sServ.Add3 + ", " + sServ.Add4;

            if (sServ.Add5 != null) { content4 = content4 + ", " + sServ.Add5; }
            if (sServ.Add6 != null) { content4 = content4 + ", " + sServ.Add6; }
            if (sServ.Add7 != null) { content4 = content4 + ", " + sServ.Add7; }
            if (sServ.Add8 != null) { content4 = content4 + ", " + sServ.Add8; }
            if (sServ.Add9 != null) { content4 = content4 + ", " + sServ.Add9; }
            if (sServ.Add10 != null) { content4 = content4 + ", " + sServ.Add10; }
            totalLengthVHR += 20;
            vhrTf.DrawString(content4, fontSmall, XBrushes.Black, new XRect(50, totalLengthVHR, 500, 120));
            totalLengthVHR += 40;            
            
            vhrGfx.DrawRectangle(pen, new XRect(45, totalLengthVHR, 500, 120)); //"Previous history of breast cancer" etc
            totalLengthVHR += 10;

            BreastSurgeryHistory bhd = _bhsData.GetBreastSurgeryHistory(mpi);
            
            if(bhd == null)
            {
                bhd = new BreastSurgeryHistory();
                bhd.BreastCancerHistory = 2;
                bhd.BreastTissueRight = 2;
                bhd.BreastTissueLeft = 2;
                bhd.ImplantsRight = 2;
                bhd.ImplantsLeft = 2;
            }

            vhrTf.DrawString("Previous history of breast cancer?", fontSmall, XBrushes.Black, new XRect(50, totalLengthVHR, 500, 120));
            switch (bhd.BreastCancerHistory)
            {
                case 0:
                    vhrTf.DrawString("No", fontSmall, XBrushes.Black, new XRect(400, totalLengthVHR, 500, 120));
                    break;
                case 1:
                    vhrTf.DrawString("Yes", fontSmall, XBrushes.Black, new XRect(400, totalLengthVHR, 500, 120));
                    break;
                default:
                    vhrTf.DrawString("Uncertain", fontSmall, XBrushes.Black, new XRect(400, totalLengthVHR, 500, 120));
                    break;
            }
            totalLengthVHR += 15;
            vhrTf.DrawString("If previous breast surgery, does tissue remain?", fontSmall, XBrushes.Black, new XRect(50, totalLengthVHR, 500, 120));
            totalLengthVHR += 15;
            vhrTf.DrawString("Right breast:", fontSmall, XBrushes.Black, new XRect(50, totalLengthVHR, 500, 120));
            switch (bhd.BreastTissueRight)
            {
                case 0:
                    vhrTf.DrawString("No", fontSmall, XBrushes.Black, new XRect(400, totalLengthVHR, 500, 120));
                    break;
                case 1:
                    vhrTf.DrawString("Yes", fontSmall, XBrushes.Black, new XRect(400, totalLengthVHR, 500, 120));
                    break;
                default:
                    vhrTf.DrawString("Uncertain", fontSmall, XBrushes.Black, new XRect(400, totalLengthVHR, 500, 120));
                    break;
            }
            totalLengthVHR += 15;
            vhrTf.DrawString("Left breast:", fontSmall, XBrushes.Black, new XRect(50, totalLengthVHR, 500, 120));
            switch (bhd.BreastTissueLeft)
            {
                case 0:
                    vhrTf.DrawString("No", fontSmall, XBrushes.Black, new XRect(400, totalLengthVHR, 500, 120));
                    break;
                case 1:
                    vhrTf.DrawString("Yes", fontSmall, XBrushes.Black, new XRect(400, totalLengthVHR, 500, 120));
                    break;
                default:
                    vhrTf.DrawString("Uncertain", fontSmall, XBrushes.Black, new XRect(400, totalLengthVHR, 500, 120));
                    break;
            }
            totalLengthVHR += 15;
            vhrTf.DrawString("Does the woman have implants?", fontSmall, XBrushes.Black, new XRect(50, totalLengthVHR, 500, 120));
            totalLengthVHR += 15;
            vhrTf.DrawString("Right breast:", fontSmall, XBrushes.Black, new XRect(50, totalLengthVHR, 500, 120));
            switch (bhd.ImplantsRight)
            {
                case 0:
                    vhrTf.DrawString("No", fontSmall, XBrushes.Black, new XRect(400, totalLengthVHR, 500, 120));
                    break;
                case 1:
                    vhrTf.DrawString("Yes", fontSmall, XBrushes.Black, new XRect(400, totalLengthVHR, 500, 120));
                    break;
                default:
                    vhrTf.DrawString("Uncertain", fontSmall, XBrushes.Black, new XRect(400, totalLengthVHR, 500, 120));
                    break;
            }
            totalLengthVHR += 15;
            vhrTf.DrawString("Left breast:", fontSmall, XBrushes.Black, new XRect(50, totalLengthVHR, 500, 120));
            switch (bhd.ImplantsLeft)
            {
                case 0:
                    vhrTf.DrawString("No", fontSmall, XBrushes.Black, new XRect(400, totalLengthVHR, 500, 120));
                    break;
                case 1:
                    vhrTf.DrawString("Yes", fontSmall, XBrushes.Black, new XRect(400, totalLengthVHR, 500, 120));
                    break;
                default:
                    vhrTf.DrawString("Uncertain", fontSmall, XBrushes.Black, new XRect(400, totalLengthVHR, 500, 120));
                    break;
            }

            UntestedVHRGroup uvg = new UntestedVHRGroup();
            uvg = _uVHRData.GetUntestedVHRGroupData(refID);
            bool isUntestedGroupTicked = false;
            if (uvg.FirstDegreeBelow30 || uvg.FirstDegreeBelow50 || uvg.OtherUntested || uvg.CauseNotIdentified)
            {
                isUntestedGroupTicked = true;
            }
            //Gene change, screening, etc                        
            vhrGfx.DrawRectangle(pen, new XRect(45, totalLengthVHR, pageWidth, 40));
            vhrGfx.DrawRectangle(XBrushes.DarkGray, new XRect(46, totalLengthVHR + 1, pageWidth - 2, 39));
            totalLengthVHR += 5;
            vhrTf.DrawString("For completion by referring clinical genetics services only", fontBold, XBrushes.Black, new XRect(50, totalLengthVHR, 500, 40));
            totalLengthVHR += 35;
            
            List<Surveillance> surv = _survData.GetSurveillanceList(mpi);

            vhrGfx.DrawRectangle(pen, new XRect(45, totalLengthVHR, pageWidth, 40 + surv.Count * 10));
            totalLengthVHR += 5;
            
            vhrTf.DrawString("Genetic status", fontSmall, XBrushes.Black, new XRect(50, totalLengthVHR, 500, 50));
            vhrTf.DrawString("Age", fontSmall, XBrushes.Black, new XRect(200, totalLengthVHR, 500, 50));
            vhrTf.DrawString("Imaging required", fontSmall, XBrushes.Black, new XRect(350, totalLengthVHR, 500, 50));
            totalLengthVHR += 15;

            vhrGfx.DrawLine(penLight, 50, totalLengthVHR, 500, totalLengthVHR);

            if (surv.Count() > 0 && !isUntestedGroupTicked)
            {
                foreach (var item in surv)
                {
                    totalLengthVHR += 10;
                    vhrTf.DrawString(item.GeneChangeDescription, fontSmall, XBrushes.Black, new XRect(50, totalLengthVHR, 500, 50));
                    string ageRange = item.SurvStartAge.ToString();
                    if(item.SurvStopAge != null) { ageRange = ageRange + " - " + item.SurvStopAge.ToString();  }
                    vhrTf.DrawString(ageRange, fontSmall, XBrushes.Black, new XRect(200, totalLengthVHR, 500, 50));
                    vhrTf.DrawString(item.SurvType + " (" + item.SurvFreqCode + ")", fontSmall, XBrushes.Black, new XRect(350, totalLengthVHR, 500, 50));
                }
            }
            totalLengthVHR += 20;

            vhrGfx.DrawRectangle(pen, new XRect(45, totalLengthVHR, pageWidth, 40));
            vhrGfx.DrawRectangle(XBrushes.DarkGray, new XRect(46, totalLengthVHR + 1, pageWidth - 2, 39));
            totalLengthVHR += 5;
                               

            vhrTf.DrawString("Risk equivalent, not tested for familial variant", fontBold, XBrushes.Black, new XRect(50, totalLengthVHR, 500, 40));
            totalLengthVHR += 35;
            vhrGfx.DrawRectangle(pen, new XRect(45, totalLengthVHR, pageWidth, 140));
            vhrTf.DrawString("Risk equivalent, not tested:", fontSmall, XBrushes.Black, new XRect(50, totalLengthVHR, 500, 50));            
            totalLengthVHR += 20;
            vhrTf.DrawString("50% risk BRCA1/2, PALB2 if aged 25 to <30", fontSmall, XBrushes.Black, new XRect(50, totalLengthVHR, 500, 50));
            vhrGfx.DrawRectangle(XBrushes.Black, new XRect(300, totalLengthVHR, 10, 10));
            vhrGfx.DrawRectangle(XBrushes.White, new XRect(301, totalLengthVHR + 1, 8, 8));
            if(uvg.FirstDegreeBelow30) { vhrTf.DrawString("X", fontSmall, XBrushes.Black, new XRect(301, totalLengthVHR, 10, 10)); }
            if (surv.Count() > 0 && isUntestedGroupTicked)
            {
                foreach (var item in surv)
                {                        
                    vhrTf.DrawString(item.SurvType + " every " + item.SurvFreq, fontSmall, XBrushes.Black, new XRect(350, totalLengthVHR, 500, 50));
                }                
            }
            totalLengthVHR += 20;
            vhrTf.DrawString("50% risk BRCA1/2 aged 30 to <51", fontSmall, XBrushes.Black, new XRect(50, totalLengthVHR, 500, 50));
            vhrGfx.DrawRectangle(XBrushes.Black, new XRect(300, totalLengthVHR, 10, 10));
            vhrGfx.DrawRectangle(XBrushes.White, new XRect(301, totalLengthVHR + 1, 8, 8));
            if (uvg.FirstDegreeBelow50 == true) { vhrTf.DrawString("X", fontSmall, XBrushes.Black, new XRect(301, totalLengthVHR, 10, 10)); }
            totalLengthVHR += 20;
            vhrTf.DrawString("Other untested (evidence of 10 year risk required)", fontSmall, XBrushes.Black, new XRect(50, totalLengthVHR, 500, 50));
            vhrGfx.DrawRectangle(XBrushes.Black, new XRect(300, totalLengthVHR, 10, 10));
            vhrGfx.DrawRectangle(XBrushes.White, new XRect(301, totalLengthVHR + 1, 8, 8));
            if (uvg.OtherUntested == true) { vhrTf.DrawString("X", fontSmall, XBrushes.Black, new XRect(301, totalLengthVHR, 10, 10)); }
            totalLengthVHR += 20;
            vhrTf.DrawString("Risk equivalent, genetic cause not identified", fontSmall, XBrushes.Black, new XRect(50, totalLengthVHR, 500, 50));
            vhrGfx.DrawRectangle(XBrushes.Black, new XRect(300, totalLengthVHR, 10, 10));
            vhrGfx.DrawRectangle(XBrushes.White, new XRect(301, totalLengthVHR + 1, 8, 8));
            if (uvg.CauseNotIdentified == true) { vhrTf.DrawString("X", fontSmall, XBrushes.Black, new XRect(301, totalLengthVHR, 10, 10)); }
            totalLengthVHR += 20;
            if(isUntestedGroupTicked)
            {                
                vhrTf.DrawString("Must have evidence of equivalent 8%, 10yr risk via CANRISK assessment:", fontSmall, XBrushes.Black, new XRect(50, totalLengthVHR, 500, 50));
                totalLengthVHR += 10;
                if(uvg.ThresholdAge8pct != null)
                { 
                    vhrTf.DrawString("Age at which the risk meets the 8% threshold: " + uvg.ThresholdAge8pct, fontSmall, XBrushes.Black, new XRect(50, totalLengthVHR, 500, 50)); 
                }
            }

            //totalLengthVHR += 60;


            //Second page
            vhrGfx2.DrawRectangle(pen, new XRect(45, totalLengthVHR2, pageWidth, 40));
            vhrGfx2.DrawRectangle(XBrushes.DarkGray, new XRect(46, totalLengthVHR2 + 1, pageWidth - 2, 39));
            totalLengthVHR2 += 5;
            vhrTf2.DrawString("For completion by BARD or oncology (tick as appropriate)", fontBold, XBrushes.Black, new XRect(50, totalLengthVHR2, 200, 50));
            totalLengthVHR2 += 35;
            vhrGfx2.DrawRectangle(pen, new XRect(45, totalLengthVHR2, pageWidth / 2, 20));
            vhrGfx2.DrawRectangle(XBrushes.LightGray, new XRect(46, totalLengthVHR2 + 1, pageWidth / 2 - 2, 19));
            vhrGfx2.DrawRectangle(pen, new XRect(pageWidth / 2 + 45, totalLengthVHR2, pageWidth / 4, 20));
            vhrGfx2.DrawRectangle(XBrushes.LightGray, new XRect(pageWidth / 2 + 46, totalLengthVHR2 + 1, (pageWidth / 4) - 2, 19));
            vhrGfx2.DrawRectangle(pen, new XRect(pageWidth / 2 + 45 + (pageWidth / 4), totalLengthVHR2, (pageWidth / 4), 20));
            vhrGfx2.DrawRectangle(XBrushes.LightGray, new XRect(pageWidth / 2 + 46 + (pageWidth / 4), totalLengthVHR2 + 1, (pageWidth / 4) - 2, 19));

            totalLengthVHR2 += 5;
            vhrTf2.DrawString("Age irradiated to breast tissue", fontSmallBold, XBrushes.Black, new XRect(50, totalLengthVHR2, 200, 50));
            vhrTf2.DrawString("Current age", fontSmallBold, XBrushes.Black, new XRect(300, totalLengthVHR2, 200, 50));
            vhrTf2.DrawString("Imaging required", fontSmallBold, XBrushes.Black, new XRect(440, totalLengthVHR2, 200, 50));
            totalLengthVHR2 += 15;
            vhrGfx2.DrawRectangle(pen, new XRect(45, totalLengthVHR2, pageWidth / 2, 60));
            vhrGfx2.DrawRectangle(pen, new XRect(45 + pageWidth / 2, totalLengthVHR2, pageWidth / 4, 20));
            vhrGfx2.DrawRectangle(pen, new XRect(45 + pageWidth / 2 + pageWidth / 4, totalLengthVHR2, pageWidth / 4, 20));
            totalLengthVHR2 += 5;
            vhrGfx2.DrawRectangle(XBrushes.Black, new XRect(80, totalLengthVHR2, 10, 10));
            vhrGfx2.DrawRectangle(XBrushes.White, new XRect(81, totalLengthVHR2 + 1, 8, 8));
            vhrTf2.DrawString("10 to <20 years", fontSmall, XBrushes.Black, new XRect(100, totalLengthVHR2, 200, 50));
            vhrGfx2.DrawRectangle(XBrushes.Black, new XRect(300, totalLengthVHR2, 10, 10));
            vhrGfx2.DrawRectangle(XBrushes.White, new XRect(301, totalLengthVHR2 + 1, 8, 8));
            vhrTf2.DrawString("30 to <40", fontSmall, XBrushes.Black, new XRect(320, totalLengthVHR2, 200, 50));
            vhrTf2.DrawString("MRI only", fontSmall, XBrushes.Black, new XRect(430, totalLengthVHR2, 200, 50));
            totalLengthVHR2 += 15;            
            vhrGfx2.DrawRectangle(pen, new XRect(45 + pageWidth / 2, totalLengthVHR2, pageWidth / 4, 20));
            vhrGfx2.DrawRectangle(pen, new XRect(45 + pageWidth / 2 + pageWidth / 4, totalLengthVHR2, pageWidth / 4, 20));
            totalLengthVHR2 += 5;
            vhrTf2.DrawString("Please state Date first irradiated (dd/mm/yyyy)", fontSmall, XBrushes.Black, new XRect(60, totalLengthVHR2, 250, 50));
            vhrTf2.DrawString("40 to <51", fontSmall, XBrushes.Black, new XRect(320, totalLengthVHR2, 200, 50));
            vhrTf2.DrawString("MRI + mammography", fontSmall, XBrushes.Black, new XRect(430, totalLengthVHR2, 200, 50));
            totalLengthVHR2 += 15;
            vhrGfx2.DrawRectangle(pen, new XRect(45 + pageWidth / 2, totalLengthVHR2, pageWidth / 4, 20));
            vhrGfx2.DrawRectangle(pen, new XRect(45 + pageWidth / 2 + pageWidth / 4, totalLengthVHR2, pageWidth / 4, 20));
            totalLengthVHR2 += 5;
            vhrTf2.DrawString("51 to <71", fontSmall, XBrushes.Black, new XRect(320, totalLengthVHR2, 200, 50));
            vhrTf2.DrawString("Mammography +/- MRI", fontSmall, XBrushes.Black, new XRect(430, totalLengthVHR2, 200, 50));
            totalLengthVHR2 += 15;
            vhrGfx2.DrawRectangle(pen, new XRect(45, totalLengthVHR2, pageWidth / 2, 60));
            vhrGfx2.DrawRectangle(pen, new XRect(45 + pageWidth / 2, totalLengthVHR2, pageWidth / 4, 20));
            vhrGfx2.DrawRectangle(pen, new XRect(45 + pageWidth / 2 + pageWidth / 4, totalLengthVHR2, pageWidth / 4, 20));
            totalLengthVHR2 += 5;
            vhrGfx2.DrawRectangle(XBrushes.Black, new XRect(80, totalLengthVHR2, 10, 10));
            vhrGfx2.DrawRectangle(XBrushes.White, new XRect(81, totalLengthVHR2 + 1, 8, 8));
            vhrTf2.DrawString("20 to <36 years", fontSmall, XBrushes.Black, new XRect(100, totalLengthVHR2, 200, 50));
            vhrGfx2.DrawRectangle(XBrushes.Black, new XRect(300, totalLengthVHR2, 10, 10));
            vhrGfx2.DrawRectangle(XBrushes.White, new XRect(301, totalLengthVHR2 + 1, 8, 8));
            vhrTf2.DrawString("30 to <40", fontSmall, XBrushes.Black, new XRect(320, totalLengthVHR2, 200, 50));
            vhrTf2.DrawString("MRI only", fontSmall, XBrushes.Black, new XRect(430, totalLengthVHR2, 200, 50));
            totalLengthVHR2 += 15;
            vhrGfx2.DrawRectangle(pen, new XRect(45 + pageWidth / 2, totalLengthVHR2, pageWidth / 4, 20));
            vhrGfx2.DrawRectangle(pen, new XRect(45 + pageWidth / 2 + pageWidth / 4, totalLengthVHR2, pageWidth / 4, 20));
            totalLengthVHR2 += 5;
            vhrTf2.DrawString("Please state Date first irradiated (dd/mm/yyyy)", fontSmall, XBrushes.Black, new XRect(60, totalLengthVHR2, 250, 50));
            vhrTf2.DrawString("40 to <51", fontSmall, XBrushes.Black, new XRect(320, totalLengthVHR2, 200, 50));
            vhrTf2.DrawString("MRI + mammography", fontSmall, XBrushes.Black, new XRect(430, totalLengthVHR2, 200, 50));
            totalLengthVHR2 += 15;
            vhrGfx2.DrawRectangle(pen, new XRect(45 + pageWidth / 2, totalLengthVHR2, pageWidth / 4, 20));
            vhrGfx2.DrawRectangle(pen, new XRect(45 + pageWidth / 2 + pageWidth / 4, totalLengthVHR2, pageWidth / 4, 20));
            totalLengthVHR2 += 5;
            vhrTf2.DrawString("51 to <71", fontSmall, XBrushes.Black, new XRect(320, totalLengthVHR2, 200, 50));
            vhrTf2.DrawString("Mammography +/- MRI", fontSmall, XBrushes.Black, new XRect(430, totalLengthVHR2, 200, 50));
            totalLengthVHR2 += 40;
            vhrGfx2.DrawRectangle(pen, new XRect(45, totalLengthVHR2, pageWidth, 50));
            vhrGfx2.DrawRectangle(XBrushes.DarkGray, new XRect(46, totalLengthVHR2 + 1, pageWidth - 2, 49));
            totalLengthVHR2 += 5;
            vhrTf2.DrawString("Women meeting VHR screening eligibility but aged 71 or over (for completion by clinical genetics, BARD or oncologists)", fontBold, XBrushes.Black, new XRect(50, totalLengthVHR2, 400, 50));
            totalLengthVHR2 += 45;
            vhrGfx2.DrawRectangle(pen, new XRect(45, totalLengthVHR2, pageWidth / 2, 120 + surv.Count() * 15));
            vhrGfx2.DrawRectangle(pen, new XRect(45 + pageWidth / 2, totalLengthVHR2, pageWidth / 2, 120 + surv.Count() * 15));
            totalLengthVHR2 += 5;
            vhrTf2.DrawString("Woman aged ≥71 years but meets VHR screening eligibility.", fontSmall, XBrushes.Black, new XRect(50, totalLengthVHR2, 200, 50));
            vhrTf2.DrawString("MRI + mammography density review, thereafter Mammography +/- MRI", fontSmall, XBrushes.Black, new XRect(330, totalLengthVHR2, 200, 50));
            totalLengthVHR2 += 40;
            vhrTf2.DrawString("Confirmed genetic mutation as below:", fontSmall, XBrushes.Black, new XRect(50, totalLengthVHR2, 200, 50));
            vhrTf2.DrawString("Note: Once women are accepted for VHR screening when aged over 70 years, they will need to self-refer to their local service for screening annually, if they wish to continue accessing screening", fontSmall, XBrushes.Black, new XRect(330, totalLengthVHR2, 200, 50));
            
            if (surv.Count() > 0)
            {
                foreach (var item in surv)
                {
                    totalLengthVHR2 += 15;
                    vhrTf2.DrawString(item.GeneChangeDescription, fontSmall, XBrushes.Black, new XRect(50, totalLengthVHR2, 500, 50));                    
                }
            }
            totalLengthVHR2 += 35;
            vhrGfx2.DrawRectangle(XBrushes.Black, new XRect(50, totalLengthVHR2, 10, 10));
            vhrGfx2.DrawRectangle(XBrushes.White, new XRect(51, totalLengthVHR2 + 1, 8, 8));
            vhrTf2.DrawString("Confirmed radiotherapy to breast tissue irradiated between ages 10 to <36 years", fontSmall, XBrushes.Black, new XRect(70, totalLengthVHR2, 200, 50));
            totalLengthVHR2 += 60;

            vhrGfx2.DrawRectangle(pen, new XRect(45, totalLengthVHR2, pageWidth, 50));
            vhrGfx2.DrawRectangle(XBrushes.DarkGray, new XRect(46, totalLengthVHR2 + 1, pageWidth - 2, 49));
            totalLengthVHR2 += 5;            
            vhrTf2.DrawString("I can confirm that the woman is 18 years of age or older and has been informed that her details will be shared with the NHS Breast Screening Programme for the purpose of screening invitations when she becomes eligible*.", fontSmall, XBrushes.Black, new XRect(60, totalLengthVHR2, pageWidth - 20, 80));

            //signatures etc
            totalLengthVHR2 += 100;
            vhrGfx2.DrawRectangle(pen, new XRect(45, totalLengthVHR2, pageWidth, 180));
            totalLengthVHR2 += 10;
            vhrTf2.DrawString("Signed:", fontSmall, XBrushes.Black, new XRect(60, totalLengthVHR2, 100, 20));


            sigFilename = _lvm.staffMember.StaffForename + _lvm.staffMember.StaffSurname.Replace("'","").Replace(" ", "") + ".jpg";

            if (!System.IO.File.Exists(@"wwwroot\Signatures\" + sigFilename)) { sigFilename = "empty.jpg"; } //this only exists because we can't define the image if it's null.
            
            XImage imageSig = XImage.FromFile(@"wwwroot\Signatures\" + sigFilename);
            int len = imageSig.PixelWidth;
            int hig = imageSig.PixelHeight;

            if (sigFilename != "empty.jpg")
            {
                vhrGfx2.DrawImage(imageSig, 100, totalLengthVHR2 - 5, len, hig);
            }

            vhrTf2.DrawString("Role: " + _lvm.staffMember.POSITION, fontSmall, XBrushes.Black, new XRect(350, totalLengthVHR2, 200, 40));
            totalLengthVHR2 += 60;
            vhrTf2.DrawString("Date: " + DateTime.Now.ToString("dd/MM/yyyy"), fontSmall, XBrushes.Black, new XRect(50, totalLengthVHR2, 100, 20));
            vhrTf2.DrawString("Print name: " + _lvm.staffMember.NAME, fontSmall, XBrushes.Black, new XRect(350, totalLengthVHR2, 200, 20));
            totalLengthVHR2 += 40;
            vhrTf2.DrawString("All referrers must keep copies of referral forms to support failsafe processes as per NHS BSP guidance", fontSmall, XBrushes.Black, new XRect(50, totalLengthVHR2, 400, 40));
            totalLengthVHR2 += 40;
            vhrTf2.DrawString("*BARD are excluded from directly informing the woman that her details will be shared for screening purposes", fontSmall, XBrushes.Black, new XRect(50, totalLengthVHR2, 400, 40));


            //Third page
            vhrGfx3.DrawRectangle(pen, new XRect(45, totalLengthVHR3, pageWidth, 40));
            vhrGfx3.DrawRectangle(XBrushes.DarkGray, new XRect(46, totalLengthVHR3 + 1, pageWidth - 2, 39));
            totalLengthVHR3 += 5;
            vhrTf3.DrawString("To be completed by Breast Screening Service", fontBold, XBrushes.Black, new XRect(50, totalLengthVHR3, 250, 50));
            totalLengthVHR3 += 35;
            vhrGfx3.DrawRectangle(pen, new XRect(45, totalLengthVHR3, pageWidth, 100));
            totalLengthVHR3 += 10;
            vhrGfx3.DrawRectangle(XBrushes.Black, new XRect(50, totalLengthVHR3, 10, 10));
            vhrGfx3.DrawRectangle(XBrushes.White, new XRect(51, totalLengthVHR3 + 1, 8, 8));
            vhrTf3.DrawString("Referral accepted for very high-risk screening", fontSmall, XBrushes.Black, new XRect(80, totalLengthVHR3, 250, 50));
            totalLengthVHR3 += 30;
            vhrGfx3.DrawRectangle(XBrushes.Black, new XRect(50, totalLengthVHR3, 10, 10));
            vhrGfx3.DrawRectangle(XBrushes.White, new XRect(51, totalLengthVHR3 + 1, 8, 8));
            vhrTf3.DrawString("Referral rejected for very high-risk screening", fontSmall, XBrushes.Black, new XRect(80, totalLengthVHR3, 250, 50));
            totalLengthVHR3 += 30;
            vhrTf3.DrawString("Reason for rejection:", fontSmall, XBrushes.Black, new XRect(80, totalLengthVHR3, 250, 50));
            totalLengthVHR3 += 30;
            vhrGfx3.DrawRectangle(pen, new XRect(45, totalLengthVHR3, pageWidth, 160));
            totalLengthVHR3 += 10;
            vhrTf3.DrawString("Please complete details below and copy form to the referrer as receipt of referral.", fontSmallBold, XBrushes.Black, new XRect(50, totalLengthVHR3, 350, 50));
            totalLengthVHR3 += 30;
            vhrTf3.DrawString("Radiotherapy referrals: chn-bard@nhs.net", fontSmall, XBrushes.Black, new XRect(50, totalLengthVHR3, 250, 50));
            totalLengthVHR3 += 30;
            vhrTf3.DrawString("Clinical genetics: referring clinical genetics service", fontSmall, XBrushes.Black, new XRect(50, totalLengthVHR3, 250, 50));
            totalLengthVHR3 += 30;
            vhrTf3.DrawString("Oncology referrals: to individual oncologist and chn-bard@nhs.net", fontSmall, XBrushes.Black, new XRect(50, totalLengthVHR3, 250, 50));
            totalLengthVHR3 += 30;
            vhrTf3.DrawString("Woman Invited", fontSmall, XBrushes.Black, new XRect(50, totalLengthVHR3, 250, 50));
            vhrTf3.DrawString("Yes / No", fontSmall, XBrushes.Black, new XRect(200, totalLengthVHR3, 250, 50));
            vhrTf3.DrawString("Woman Screened", fontSmall, XBrushes.Black, new XRect(350, totalLengthVHR3, 250, 50));
            vhrTf3.DrawString("Yes / No", fontSmall, XBrushes.Black, new XRect(500, totalLengthVHR3, 250, 50));
            totalLengthVHR3 += 30;
            vhrGfx3.DrawRectangle(pen, new XRect(45, totalLengthVHR3, pageWidth, 120));
            totalLengthVHR3 += 10;
            vhrTf3.DrawString("Authoriser's name:", fontSmallBold, XBrushes.Black, new XRect(50, totalLengthVHR3, 250, 50));
            totalLengthVHR3 += 30;
            vhrTf3.DrawString("Authoriser's signature:", fontSmallBold, XBrushes.Black, new XRect(50, totalLengthVHR3, 250, 50));
            totalLengthVHR3 += 30;
            vhrTf3.DrawString("Date:", fontSmallBold, XBrushes.Black, new XRect(50, totalLengthVHR3, 250, 50));
            totalLengthVHR3 += 30;
            vhrTf3.DrawString("(Referral forms can be authorised by a consultant radiologist, consultant practitioner or breast clinician.)", fontTiny, XBrushes.Black, new XRect(50, totalLengthVHR3, 400, 50));

            vhrDocument.Save($@"C:\CGU_DB\Letters\CaStdLetter-{fileCGU}-{docCode}-{mpiString}-0-{refIDString}-{printCount.ToString()}-{dateTimeString}-{diaryIDString}.pdf");
        }
        catch (Exception ex)
        {
            RedirectToAction("ErrorHome", "Error", new { error = ex.Message, formName = "StdLetter" });
        }
    }

    public void DoVHRProCoverLetter(int id, int mpi, int icpCancerID, string user, string referrer, string screeningService, string? additionalText = "", int? diaryID = 0)
    {
        try
        {
            _lvm.staffMember = _staffUser.GetStaffMemberDetails(user);
            _lvm.patient = _patientData.GetPatientDetails(mpi);
            _lvm.documentsContent = _documentsData.GetDocumentDetails(id);
            _lvm.gp = _externalClinicianData.GetClinicianDetails(_lvm.patient.GP_Code);
            _lvm.facility = _externalFacilityData.GetFacilityDetails(_lvm.gp.FACILITY);

            int icpID = _triageData.GetCancerICPDetails(icpCancerID).ICPID;
            int refID = _triageData.GetICPDetails(icpID).REFID;

            //string docCode = _lvm.documentsContent.DocCode;
            string docCode = _lvm.documentsContent.DocCode;
            //creates a new PDF document            
            //set the fonts used for the letters
            XFont font = new XFont("Arial", 12, XFontStyle.Regular);
            XFont fontBold = new XFont("Arial", 12, XFontStyle.Bold);
            XFont fontItalic = new XFont("Arial", 12, XFontStyle.Italic);
            XFont fontSmall = new XFont("Arial", 10, XFontStyle.Regular);
            XFont fontSmallBold = new XFont("Arial", 10, XFontStyle.Bold);
            XFont fontSmallItalic = new XFont("Arial", 10, XFontStyle.Italic);
            XFont fontTiny = new XFont("Arial", 8, XFontStyle.Regular);
            XFont fontTinyBold = new XFont("Arial", 8, XFontStyle.Bold);
            XFont fontTinyItalic = new XFont("Arial", 8, XFontStyle.Italic);
            //Load the image for the letter head
            XImage image = XImage.FromFile(@"wwwroot\Letterhead.jpg");

            string name = "";
            string patName = "";
            string address = "";
            string patAddress = "";
            string salutation = "";
            DateTime patDOB = DateTime.Now;

            string content = "";
            string quoteRef = "";
            string signOff = "";
            string sigFilename = "";
            int printCount = 1;

            string fileCGU = _lvm.patient.CGU_No.Replace(".", "-");
            string mpiString = _lvm.patient.MPI.ToString();
            string refIDString = refID.ToString();
            string dateTimeString = DateTime.Now.ToString("yyyyMMddHHmmss");
            string diaryIDString = diaryID.ToString();

            PdfDocument vhrDocument = new PdfDocument();
            vhrDocument.Info.Title = "VHRProC";
            PdfPage vhrPage = vhrDocument.AddPage();            
            XGraphics vhrGfx = XGraphics.FromPdfPage(vhrPage);
            var vhrTf = new XTextFormatter(vhrGfx);           
            vhrGfx.DrawImage(image, 350, 20, image.PixelWidth / 2, image.PixelHeight / 2);
            int totalLengthVHR = 150;
            int totalLengthVHR2 = 50;
            int totalLengthVHR3 = 50;

            vhrTf.Alignment = XParagraphAlignment.Right;
            //Our address and contact details
            vhrTf.DrawString(_lvm.documentsContent.OurAddress, font, XBrushes.Black, new XRect(-20, 150, vhrPage.Width, 200));
            if (_lvm.documentsContent.DirectLine != null) //because we have to trap them nulls!
            {
                vhrTf.DrawString(_lvm.documentsContent.DirectLine, fontBold, XBrushes.Black, new XRect(-20, 250, vhrPage.Width, 10));
            }
            if (_lvm.documentsContent.OurEmailAddress != null) //because obviously there's a null.
            {
                vhrTf.DrawString(_lvm.documentsContent.OurEmailAddress, font, XBrushes.Black, new XRect(-20, 270, vhrPage.Width, 10));
            }
            vhrTf.Alignment = XParagraphAlignment.Left;

            ScreeningService ss = _screenData.GetScreeningServiceDetailsByCode(screeningService);

            name = ss.Contact;
            address = ss.Add1 + Environment.NewLine;
            address = address + ss.Add2 + Environment.NewLine;
            address = address + ss.Add3 + Environment.NewLine;
            address = address + ss.Add4 + Environment.NewLine;
            if (ss.Add5 != null) { address = address + ss.Add5 + Environment.NewLine; }
            if (ss.Add6 != null) { address = address + ss.Add6 + Environment.NewLine; }
            if (ss.Add7 != null) { address = address + ss.Add7 + Environment.NewLine; }
            if (ss.Add8 != null) { address = address + ss.Add8 + Environment.NewLine; }

            string gpName = _lvm.gp.TITLE + " " + _lvm.gp.FIRST_NAME + " " + _lvm.gp.NAME;

            string gpAddress = _lvm.facility.NAME + Environment.NewLine;
            gpAddress = gpAddress + _lvm.facility.ADDRESS + Environment.NewLine;
            gpAddress = gpAddress + _lvm.facility.CITY + Environment.NewLine;
            gpAddress = gpAddress + _lvm.facility.ZIP + Environment.NewLine;

            totalLengthVHR += 100;
            vhrTf.DrawString(name, font, XBrushes.Black, new XRect(50, totalLengthVHR, 500, 15));
            totalLengthVHR += 15;
            vhrTf.DrawString(address, font, XBrushes.Black, new XRect(50, totalLengthVHR, 500, 100));
            totalLengthVHR += 120;
            vhrTf.DrawString("Dear " + name, font, XBrushes.Black, new XRect(50, totalLengthVHR, 500, 20));
            totalLengthVHR += 20;

            patName = _lvm.patient.FIRSTNAME + " " + _lvm.patient.LASTNAME;
            patAddress = _lvm.patient.ADDRESS1 + System.Environment.NewLine;
            if (_lvm.patient.ADDRESS2 != null) { patAddress = patAddress + _lvm.patient.ADDRESS2 + Environment.NewLine; }
            if (_lvm.patient.ADDRESS3 != null) { patAddress = patAddress + _lvm.patient.ADDRESS3 + Environment.NewLine; }
            if (_lvm.patient.ADDRESS4 != null) { patAddress = patAddress + _lvm.patient.ADDRESS4 + Environment.NewLine; }            
            patAddress = patAddress + _lvm.patient.POSTCODE;

            vhrTf.DrawString("Re: " + patName, fontBold, XBrushes.Black, new XRect(60, totalLengthVHR, 500, 15));
            vhrTf.DrawString(_lvm.patient.DOB.Value.ToString("dd/MM/yyyy"), fontBold, XBrushes.Black, new XRect(200, totalLengthVHR, 500, 15));
            vhrTf.DrawString("NHS number: " + _lvm.patient.SOCIAL_SECURITY, fontBold, XBrushes.Black, new XRect(350, totalLengthVHR, 500, 15));
            totalLengthVHR += 15;
            vhrTf.DrawString(patAddress, fontBold, XBrushes.Black, new XRect(80, totalLengthVHR, 500, 100));
            totalLengthVHR += 100;

            content = _lvm.documentsContent.Para1 + System.Environment.NewLine + System.Environment.NewLine;

            if (additionalText != null)
            {
                content = content + additionalText + System.Environment.NewLine + System.Environment.NewLine;
            }

            content = content + _lvm.documentsContent.Para2;

            vhrTf.DrawString(content, font, XBrushes.Black, new XRect(50, totalLengthVHR, 500, content.Length / 4));
            totalLengthVHR = totalLengthVHR + content.Length / 4 + 40;

            sigFilename = _lvm.staffMember.StaffForename + _lvm.staffMember.StaffSurname.Replace("'", "").Replace(" ", "") + ".jpg";

            if (!System.IO.File.Exists(@"wwwroot\Signatures\" + sigFilename)) { sigFilename = "empty.jpg"; } //this only exists because we can't define the image if it's null.

            XImage imageSig = XImage.FromFile(@"wwwroot\Signatures\" + sigFilename);
            int len = imageSig.PixelWidth;
            int hig = imageSig.PixelHeight;

            if (sigFilename != "empty.jpg")
            {
                vhrGfx.DrawImage(imageSig, 50, totalLengthVHR, len, hig);
            }
            totalLengthVHR += hig;

            signOff = _lvm.staffMember.NAME + Environment.NewLine + _lvm.staffMember.POSITION;
            vhrTf.DrawString(signOff, font, XBrushes.Black, new XRect(50, totalLengthVHR, 500, 100));

            PdfPage pageCC = vhrDocument.AddPage();
            printCount = printCount += 1;
            XGraphics gfxcc = XGraphics.FromPdfPage(pageCC);
            var tfcc = new XTextFormatter(gfxcc);

            int ccLength = 50;
            tfcc.DrawString("cc:", font, XBrushes.Black, new XRect(50, ccLength, 500, 100));
            //Add a page for all of the CC addresses (must be declared here or we can't use it)            
            tfcc.DrawString(gpName + System.Environment.NewLine + gpAddress, font, XBrushes.Black, new XRect(100, ccLength, 500, 100));            
            printCount = printCount += 1;

            vhrDocument.Save($@"C:\CGU_DB\Letters\CaStdLetter-{fileCGU}-{docCode}-{mpiString}-0-{refIDString}-{printCount.ToString()}-{dateTimeString}-{diaryIDString}.pdf");
        }
        catch (Exception ex)
        {
            RedirectToAction("ErrorHome", "Error", new { error = ex.Message, formName = "StdLetter" });
        }
    }


    public void DoVHRProOriginal(int id, int mpi, int icpCancerID, string user, string referrer, string screeningService, string? additionalText = "", int? diaryID = 0)
    {
        try
        {
            _lvm.staffMember = _staffUser.GetStaffMemberDetails(user);
            _lvm.patient = _patientData.GetPatientDetails(mpi);
            _lvm.documentsContent = _documentsData.GetDocumentDetails(id);
            _lvm.referrer = _externalClinicianData.GetClinicianDetails(referrer);

            int icpID = _triageData.GetCancerICPDetails(icpCancerID).ICPID;
            int refID = _triageData.GetICPDetails(icpID).REFID;

            //string docCode = _lvm.documentsContent.DocCode;
            string docCode = _lvm.documentsContent.DocCode;
            //creates a new PDF document            
            //set the fonts used for the letters
            XFont font = new XFont("Arial", 12, XFontStyle.Regular);
            XFont fontBold = new XFont("Arial", 12, XFontStyle.Bold);
            XFont fontItalic = new XFont("Arial", 12, XFontStyle.Italic);
            XFont fontSmall = new XFont("Arial", 10, XFontStyle.Regular);
            XFont fontSmallBold = new XFont("Arial", 10, XFontStyle.Bold);
            XFont fontSmallItalic = new XFont("Arial", 10, XFontStyle.Italic);
            XFont fontTiny = new XFont("Arial", 8, XFontStyle.Regular);
            XFont fontTinyBold = new XFont("Arial", 8, XFontStyle.Bold);
            XFont fontTinyItalic = new XFont("Arial", 8, XFontStyle.Italic);
            //Load the image for the letter head
            XImage image = XImage.FromFile(@"wwwroot\Letterhead.jpg");

            //Create the stuff that's common to all letters

            string name = "";
            string patName = "";
            string address = "";
            string patAddress = "";
            string salutation = "";
            DateTime patDOB = DateTime.Now;

            string content1 = "";
            string content2 = "";
            string content3 = "";
            string content4 = "";
            string content5 = "";
            string content6 = "";
            string quoteRef = "";
            string signOff = "";
            string sigFilename = "";
            int printCount = 1;

            string fileCGU = _lvm.patient.CGU_No.Replace(".", "-");
            string mpiString = _lvm.patient.MPI.ToString();
            string refIDString = refID.ToString();
            string dateTimeString = DateTime.Now.ToString("yyyyMMddHHmmss");
            string diaryIDString = diaryID.ToString();

            PdfDocument vhrDocument = new PdfDocument();
            vhrDocument.Info.Title = "VHRPro";
            PdfPage vhrPage = vhrDocument.AddPage();
            PdfPage vhrPage2 = vhrDocument.AddPage();
            PdfPage vhrPage3 = vhrDocument.AddPage();
            XGraphics vhrGfx = XGraphics.FromPdfPage(vhrPage);
            var vhrTf = new XTextFormatter(vhrGfx);
            XGraphics vhrGfx2 = XGraphics.FromPdfPage(vhrPage2);
            var vhrTf2 = new XTextFormatter(vhrGfx2);
            XGraphics vhrGfx3 = XGraphics.FromPdfPage(vhrPage3);
            var vhrTf3 = new XTextFormatter(vhrGfx3);
            vhrGfx.DrawImage(image, 350, 20, image.PixelWidth / 2, image.PixelHeight / 2);
            int totalLengthVHR = 140;
            int totalLengthVHR2 = 50;
            int totalLengthVHR3 = 50;
            int rotation = 90;


            //PdfSharpCore.Drawing.XRect rect1 = new PdfSharpCore.Drawing.XRect(15, totalLengthVHR, 25, 680);
            XPen pen = new XPen(XColors.Black, 1);
            XPen penLight = new XPen(XColors.Black, 0.5);

            vhrGfx.DrawRectangle(pen, new XRect(15, totalLengthVHR, 25, 680));

            vhrTf.DrawString("Referral to the NHSBSP for very high-risk screening", fontBold, XBrushes.Black, new XRect(50, totalLengthVHR, 500, 20));
            vhrGfx.RotateTransform(rotation);
            vhrTf.DrawString("Section A: To be completed by the Referrer", fontBold, XBrushes.Black, new XRect(150, -30, 500, 500));
            vhrGfx.RotateTransform(-rotation);
            totalLengthVHR += 20;


            vhrTf.DrawString("Patient Details", fontSmallBold, XBrushes.Black, new XRect(50, totalLengthVHR, 550, 20));
            totalLengthVHR += 15;
            //PdfSharpCore.Drawing.XRect rect2 = new PdfSharpCore.Drawing.XRect(45, totalLengthVHR, 250, 120);
            vhrGfx.DrawRectangle(pen, new XRect(45, totalLengthVHR, 250, 120));

            patName = _lvm.patient.FIRSTNAME + " " + _lvm.patient.LASTNAME;
            patAddress = _lvm.patient.ADDRESS1 + System.Environment.NewLine;
            if (_lvm.patient.ADDRESS2 != null) { patAddress = patAddress + _lvm.patient.ADDRESS2 + Environment.NewLine; }
            if (_lvm.patient.ADDRESS3 != null) { patAddress = patAddress + _lvm.patient.ADDRESS3 + Environment.NewLine; }
            if (_lvm.patient.ADDRESS4 != null) { patAddress = patAddress + _lvm.patient.ADDRESS4 + Environment.NewLine; }
            patAddress = patAddress + _lvm.patient.POSTCODE;

            content1 = patName + System.Environment.NewLine + System.Environment.NewLine + patAddress + System.Environment.NewLine + System.Environment.NewLine +
                "Tel Home: " + _lvm.patient.TEL + System.Environment.NewLine +
                "Mobile: " + _lvm.patient.PtTelMobile;

            //PdfSharpCore.Drawing.XRect rect3 = new PdfSharpCore.Drawing.XRect(305, totalLengthVHR, 245, 120);
            vhrGfx.DrawRectangle(pen, new XRect(305, totalLengthVHR, 245, 120));

            vhrTf.DrawString(content1, fontSmall, XBrushes.Black, new XRect(50, totalLengthVHR, 500, 150));
            content2 = "NHS Number: " + _lvm.patient.SOCIAL_SECURITY + System.Environment.NewLine +
                    "DOB: " + _lvm.patient.DOB.Value.ToString("dd/MM/yyyy") + System.Environment.NewLine +
                    "GP Practice Code: " + _lvm.patient.GP_Facility_Code + System.Environment.NewLine +
                    "GP Address: ";

            var gpFac = _externalFacilityData.GetFacilityDetails(_lvm.patient.GP_Facility_Code);
            string gpAddress = gpFac.NAME + Environment.NewLine +
                    gpFac.ADDRESS + Environment.NewLine +
                    gpFac.CITY + Environment.NewLine +
                    gpFac.STATE + Environment.NewLine +
                    gpFac.ZIP + Environment.NewLine;

            content2 = content2 + gpAddress;
            vhrTf.DrawString(content2, fontSmall, XBrushes.Black, new XRect(310, totalLengthVHR, 500, 150));
            totalLengthVHR += 125;
            //PdfSharpCore.Drawing.XRect rect4 = new PdfSharpCore.Drawing.XRect(45, totalLengthVHR, 500, 125);
            vhrGfx.DrawRectangle(pen, new XRect(45, totalLengthVHR, 500, 125));

            vhrTf.DrawString("Referrer Details", fontSmallBold, XBrushes.Black, new XRect(50, totalLengthVHR, 500, 20));
            totalLengthVHR += 10;
            content3 = "Referrer Name: " + _lvm.staffMember.NAME + System.Environment.NewLine +
                "Referrer Role: " + _lvm.staffMember.POSITION + System.Environment.NewLine +
                "Address:" + System.Environment.NewLine +
                _lvm.documentsContent.OurAddress;

            vhrTf.DrawString(content3, fontSmall, XBrushes.Black, new XRect(50, totalLengthVHR, 500, 120));
            totalLengthVHR += 120;

            ScreeningService sServ = _screenData.GetScreeningServiceDetails(_lvm.patient.GP_Facility_Code);

            //PdfSharpCore.Drawing.XRect rect5 = new PdfSharpCore.Drawing.XRect(45, totalLengthVHR, 500, 125);
            vhrGfx.DrawRectangle(pen, new XRect(45, totalLengthVHR, 500, 125));

            vhrTf.DrawString("Referee Details", fontSmallBold, XBrushes.Black, new XRect(50, totalLengthVHR, 500, 20));
            totalLengthVHR += 10;
            content4 = sServ.Contact + System.Environment.NewLine +
                sServ.Telephone + System.Environment.NewLine +
                sServ.Add1 + System.Environment.NewLine +
                sServ.Add2 + System.Environment.NewLine +
                sServ.Add3 + System.Environment.NewLine +
                sServ.Add4 + System.Environment.NewLine;

            if (sServ.Add5 != null) { content4 = content4 + sServ.Add5 + System.Environment.NewLine; }
            if (sServ.Add6 != null) { content4 = content4 + sServ.Add6 + System.Environment.NewLine; }
            if (sServ.Add7 != null) { content4 = content4 + sServ.Add7 + System.Environment.NewLine; }
            if (sServ.Add8 != null) { content4 = content4 + sServ.Add8 + System.Environment.NewLine; }
            if (sServ.Add9 != null) { content4 = content4 + sServ.Add9 + System.Environment.NewLine; }
            if (sServ.Add10 != null) { content4 = content4 + sServ.Add10 + System.Environment.NewLine; }

            vhrTf.DrawString(content4, fontSmall, XBrushes.Black, new XRect(50, totalLengthVHR, 500, 120));
            totalLengthVHR += 120;
            vhrTf.DrawString("Please indicate relevant family history members with age of diagnosis, " +
                    "relationship and attach copy of genetics letter indicating " +
                    "level of risk", fontSmallItalic, XBrushes.Black, new XRect(50, totalLengthVHR, 500, 120));
            totalLengthVHR += 25;

            //PdfSharpCore.Drawing.XRect rect6 = new PdfSharpCore.Drawing.XRect(45, totalLengthVHR, 500, 30);
            vhrGfx.DrawRectangle(pen, new XRect(45, totalLengthVHR, 500, 30));
            if (additionalText != null)
            {
                vhrTf.DrawString(additionalText, fontSmall, XBrushes.Black, new XRect(50, totalLengthVHR, 500, 120));
            }
            totalLengthVHR += 40;

            //PdfSharpCore.Drawing.XRect rect7 = new PdfSharpCore.Drawing.XRect(45, totalLengthVHR, 500, 60);
            vhrGfx.DrawRectangle(pen, new XRect(45, totalLengthVHR, 500, 60));

            content5 = "Please attach a copy of the genetics letter indicating levels of risk to this form in support of the information below." +
                    System.Environment.NewLine + "1.IBIS risk print out (report pts 10yr risk if they are aged between 25 - 29 - calculate at their " +
                    "current age, or if no mutation identified, or no genetic testing undertaken)" +
                    System.Environment.NewLine + "2.Lab report(if testing has been completed and pathogenic mutation identified)";

            vhrTf.DrawString(content5, fontSmall, XBrushes.Black, new XRect(50, totalLengthVHR, 490, 120));
            totalLengthVHR += 60;

            //PdfSharpCore.Drawing.XRect rect8 = new PdfSharpCore.Drawing.XRect(45, totalLengthVHR, 500, 105);
            vhrGfx.DrawRectangle(penLight, new XRect(45, totalLengthVHR, 500, 105));

            BreastSurgeryHistory bhd = _bhsData.GetBreastSurgeryHistory(mpi);

            if (bhd == null)
            {
                bhd = new BreastSurgeryHistory();
                bhd.BreastCancerHistory = 2;
                bhd.BreastTissueRight = 2;
                bhd.BreastTissueLeft = 2;
                bhd.ImplantsRight = 2;
                bhd.ImplantsLeft = 2;
            }

            vhrTf.DrawString("Previous history of breast cancer?", fontSmall, XBrushes.Black, new XRect(50, totalLengthVHR, 500, 120));
            switch (bhd.BreastCancerHistory)
            {
                case 0:
                    vhrTf.DrawString("No", fontSmall, XBrushes.Black, new XRect(400, totalLengthVHR, 500, 120));
                    break;
                case 1:
                    vhrTf.DrawString("Yes", fontSmall, XBrushes.Black, new XRect(400, totalLengthVHR, 500, 120));
                    break;
                default:
                    vhrTf.DrawString("Unclear", fontSmall, XBrushes.Black, new XRect(400, totalLengthVHR, 500, 120));
                    break;
            }
            totalLengthVHR += 15;
            vhrTf.DrawString("If previous breast surgery, does tissue remain?", fontSmall, XBrushes.Black, new XRect(50, totalLengthVHR, 500, 120));
            totalLengthVHR += 15;
            vhrTf.DrawString("Right breast:", fontSmall, XBrushes.Black, new XRect(50, totalLengthVHR, 500, 120));
            switch (bhd.BreastTissueRight)
            {
                case 0:
                    vhrTf.DrawString("No", fontSmall, XBrushes.Black, new XRect(400, totalLengthVHR, 500, 120));
                    break;
                case 1:
                    vhrTf.DrawString("Yes", fontSmall, XBrushes.Black, new XRect(400, totalLengthVHR, 500, 120));
                    break;
                default:
                    vhrTf.DrawString("Unclear", fontSmall, XBrushes.Black, new XRect(400, totalLengthVHR, 500, 120));
                    break;
            }
            totalLengthVHR += 15;
            vhrTf.DrawString("Left breast:", fontSmall, XBrushes.Black, new XRect(50, totalLengthVHR, 500, 120));
            switch (bhd.BreastTissueLeft)
            {
                case 0:
                    vhrTf.DrawString("No", fontSmall, XBrushes.Black, new XRect(400, totalLengthVHR, 500, 120));
                    break;
                case 1:
                    vhrTf.DrawString("Yes", fontSmall, XBrushes.Black, new XRect(400, totalLengthVHR, 500, 120));
                    break;
                default:
                    vhrTf.DrawString("Unclear", fontSmall, XBrushes.Black, new XRect(400, totalLengthVHR, 500, 120));
                    break;
            }
            totalLengthVHR += 15;
            vhrTf.DrawString("Does the patient have implants?", fontSmall, XBrushes.Black, new XRect(50, totalLengthVHR, 500, 120));
            totalLengthVHR += 15;
            vhrTf.DrawString("Right breast:", fontSmall, XBrushes.Black, new XRect(50, totalLengthVHR, 500, 120));
            switch (bhd.ImplantsRight)
            {
                case 0:
                    vhrTf.DrawString("No", fontSmall, XBrushes.Black, new XRect(400, totalLengthVHR, 500, 120));
                    break;
                case 1:
                    vhrTf.DrawString("Yes", fontSmall, XBrushes.Black, new XRect(400, totalLengthVHR, 500, 120));
                    break;
                default:
                    vhrTf.DrawString("Unclear", fontSmall, XBrushes.Black, new XRect(400, totalLengthVHR, 500, 120));
                    break;
            }
            totalLengthVHR += 15;
            vhrTf.DrawString("Left breast:", fontSmall, XBrushes.Black, new XRect(50, totalLengthVHR, 500, 120));
            switch (bhd.ImplantsLeft)
            {
                case 0:
                    vhrTf.DrawString("No", fontSmall, XBrushes.Black, new XRect(400, totalLengthVHR, 500, 120));
                    break;
                case 1:
                    vhrTf.DrawString("Yes", fontSmall, XBrushes.Black, new XRect(400, totalLengthVHR, 500, 120));
                    break;
                default:
                    vhrTf.DrawString("Unclear", fontSmall, XBrushes.Black, new XRect(400, totalLengthVHR, 500, 120));
                    break;
            }

            //PdfSharpCore.Drawing.XRect rect9 = new PdfSharpCore.Drawing.XRect(15, totalLengthVHR2, 25, 720);
            vhrGfx2.DrawRectangle(pen, new XRect(15, totalLengthVHR2, 25, 720));

            vhrGfx2.RotateTransform(rotation);
            vhrTf2.DrawString("Section B: To be completed by Genetics/Oncology", fontBold, XBrushes.Black, new XRect(50, -30, 500, 500));
            vhrGfx2.RotateTransform(-rotation);

            //PdfSharpCore.Drawing.XRect rect10 = new PdfSharpCore.Drawing.XRect(45, totalLengthVHR2, 500, 100);
            vhrGfx2.DrawRectangle(penLight, new XRect(45, totalLengthVHR2, 500, 100));
            vhrTf2.DrawString("Risk", fontSmall, XBrushes.Black, new XRect(50, totalLengthVHR2, 500, 50));
            vhrTf2.DrawString("Age", fontSmall, XBrushes.Black, new XRect(300, totalLengthVHR2, 500, 50));
            vhrTf2.DrawString("Surveillance Protocol", fontSmall, XBrushes.Black, new XRect(400, totalLengthVHR2, 500, 50));
            totalLengthVHR2 += 15;

            vhrGfx2.DrawLine(pen, 50, totalLengthVHR2, 490, totalLengthVHR2);

            List<Surveillance> surv = _survData.GetSurveillanceList(mpi);

            if (surv.Count() > 0)
            {
                foreach (var item in surv)
                {
                    totalLengthVHR2 += 20;
                    vhrTf2.DrawString(item.SurvSite, fontSmall, XBrushes.Black, new XRect(50, totalLengthVHR2, 500, 50));
                    vhrTf2.DrawString(item.SurvStartAge.ToString() + " - " + item.SurvStopAge.ToString(), fontSmall, XBrushes.Black, new XRect(300, totalLengthVHR2, 500, 50));
                    vhrTf2.DrawString(item.SurvType, fontSmall, XBrushes.Black, new XRect(400, totalLengthVHR2, 500, 50));
                }
            }
            totalLengthVHR2 += 50;

            //PdfSharpCore.Drawing.XRect rect11 = new PdfSharpCore.Drawing.XRect(45, totalLengthVHR2, 500, 180);
            vhrGfx2.DrawRectangle(penLight, new XRect(45, totalLengthVHR2, 500, 180));

            vhrTf2.DrawString("Please give 10 year risk using the most appropriate age range", fontSmall, XBrushes.Black, new XRect(50, totalLengthVHR2, 500, 50));
            totalLengthVHR2 += 20;
            vhrTf2.DrawString("Age Category for Patient (years)", fontSmall, XBrushes.Black, new XRect(200, totalLengthVHR2, 500, 50));
            vhrTf2.DrawString("10 Year Risk", fontSmall, XBrushes.Black, new XRect(400, totalLengthVHR2, 500, 50));

            vhrTf2.DrawString("25-29", fontSmall, XBrushes.Black, new XRect(250, totalLengthVHR2 + 15, 500, 50));
            vhrTf2.DrawString("30-39", fontSmall, XBrushes.Black, new XRect(250, totalLengthVHR2 + 30, 500, 50));
            vhrTf2.DrawString("40-49", fontSmall, XBrushes.Black, new XRect(250, totalLengthVHR2 + 45, 500, 50));
            vhrTf2.DrawString("50-60", fontSmall, XBrushes.Black, new XRect(250, totalLengthVHR2 + 60, 500, 50));

            List<Risk> risk = _riskData.GetRiskList(icpCancerID);

            if (risk.Count > 0)
            {
                foreach (var item in risk)
                {
                    vhrTf2.DrawString(item.R25_29.ToString(), fontSmall, XBrushes.Black, new XRect(425, totalLengthVHR2 + 15, 500, 50));
                    vhrTf2.DrawString(item.R30_40.ToString(), fontSmall, XBrushes.Black, new XRect(425, totalLengthVHR2 + 30, 500, 50));
                    vhrTf2.DrawString(item.R40_50.ToString(), fontSmall, XBrushes.Black, new XRect(425, totalLengthVHR2 + 45, 500, 50));
                    vhrTf2.DrawString(item.R50_60.ToString(), fontSmall, XBrushes.Black, new XRect(425, totalLengthVHR2 + 60, 500, 50));

                    totalLengthVHR2 += 20;
                    vhrTf2.DrawString("Risk Date: " + item.RiskDate.Value.ToString("dd/MM/yyyy"), fontSmall, XBrushes.Black, new XRect(50, totalLengthVHR2, 500, 50));
                    totalLengthVHR2 += 15;
                    vhrTf2.DrawString("Calculation Tool: " + item.CalculationToolUsed, fontSmall, XBrushes.Black, new XRect(50, totalLengthVHR2, 500, 50));
                    totalLengthVHR2 += 40;
                }
            }
            totalLengthVHR2 += 40;

            //PdfSharpCore.Drawing.XRect rect12 = new PdfSharpCore.Drawing.XRect(45, totalLengthVHR2, 500, 100);
            vhrGfx2.DrawRectangle(penLight, new XRect(45, totalLengthVHR2, 500, 100));

            vhrTf2.DrawString("Risk equivalent, not tested:", fontSmall, XBrushes.Black, new XRect(50, totalLengthVHR2, 500, 50));
            totalLengthVHR2 += 20;
            vhrTf2.DrawString("First degree relative (BRCA1/2, PALB2) aged <30 (evidence of 10 year risk required)", fontSmall, XBrushes.Black, new XRect(50, totalLengthVHR2, 500, 50));
            vhrGfx2.DrawRectangle(XBrushes.Black, new XRect(500, totalLengthVHR2, 10, 10));
            vhrGfx2.DrawRectangle(XBrushes.White, new XRect(501, totalLengthVHR2 + 1, 8, 8));
            totalLengthVHR2 += 20;
            vhrTf2.DrawString("First degree relative (BRCA1/2, PALB2) aged 30-50", fontSmall, XBrushes.Black, new XRect(50, totalLengthVHR2, 500, 50));
            vhrGfx2.DrawRectangle(XBrushes.Black, new XRect(500, totalLengthVHR2, 10, 10));
            vhrGfx2.DrawRectangle(XBrushes.White, new XRect(501, totalLengthVHR2 + 1, 8, 8));
            totalLengthVHR2 += 20;
            vhrTf2.DrawString("Other untested (including PALB2) (evidence of 10 year risk required)", fontSmall, XBrushes.Black, new XRect(50, totalLengthVHR2, 500, 50));
            vhrGfx2.DrawRectangle(XBrushes.Black, new XRect(500, totalLengthVHR2, 10, 10));
            vhrGfx2.DrawRectangle(XBrushes.White, new XRect(501, totalLengthVHR2 + 1, 8, 8));
            totalLengthVHR2 += 60;
            vhrTf2.DrawString("Radiotherapy to breast tissue - irradiated between 10-19", fontSmall, XBrushes.Black, new XRect(50, totalLengthVHR2, 200, 50));
            vhrTf2.DrawString("25 to 70", fontSmall, XBrushes.Black, new XRect(300, totalLengthVHR2, 500, 50));
            vhrTf2.DrawString("MRI", fontSmall, XBrushes.Black, new XRect(350, totalLengthVHR2, 500, 50));
            vhrGfx2.DrawRectangle(XBrushes.Black, new XRect(500, totalLengthVHR2, 10, 10));
            vhrGfx2.DrawRectangle(XBrushes.White, new XRect(501, totalLengthVHR2 + 1, 8, 8));
            totalLengthVHR2 += 25;
            vhrGfx2.DrawLine(penLight, 50, totalLengthVHR2, 550, totalLengthVHR2);
            totalLengthVHR2 += 10;
            vhrTf2.DrawString("Radiotherapy to breast tissue - irradiated between 20-29", fontSmall, XBrushes.Black, new XRect(50, totalLengthVHR2, 200, 50));
            vhrTf2.DrawString("30 to 39", fontSmall, XBrushes.Black, new XRect(300, totalLengthVHR2, 500, 50));
            vhrTf2.DrawString("MRI", fontSmall, XBrushes.Black, new XRect(350, totalLengthVHR2, 500, 50));
            vhrGfx2.DrawRectangle(XBrushes.Black, new XRect(500, totalLengthVHR2, 10, 10));
            vhrGfx2.DrawRectangle(XBrushes.White, new XRect(501, totalLengthVHR2 + 1, 8, 8)); //black rectangle with white rectangle, to simulate box (because PDF sharp is shit)
            totalLengthVHR2 += 20;
            vhrTf2.DrawString("40 to 50", fontSmall, XBrushes.Black, new XRect(300, totalLengthVHR2, 500, 50));
            vhrTf2.DrawString("MRI +/- mammography", fontSmall, XBrushes.Black, new XRect(350, totalLengthVHR2, 500, 50));
            vhrGfx2.DrawRectangle(XBrushes.Black, new XRect(500, totalLengthVHR2, 10, 10));
            vhrGfx2.DrawRectangle(XBrushes.White, new XRect(501, totalLengthVHR2 + 1, 8, 8));
            totalLengthVHR2 += 20;
            vhrTf2.DrawString("51 to 70", fontSmall, XBrushes.Black, new XRect(300, totalLengthVHR2, 500, 50));
            vhrTf2.DrawString("mammography +/- MRI", fontSmall, XBrushes.Black, new XRect(350, totalLengthVHR2, 500, 50));
            vhrGfx2.DrawRectangle(XBrushes.Black, new XRect(500, totalLengthVHR2, 10, 10));
            vhrGfx2.DrawRectangle(XBrushes.White, new XRect(501, totalLengthVHR2 + 1, 8, 8));
            totalLengthVHR2 += 15;
            vhrGfx2.DrawLine(penLight, 50, totalLengthVHR2, 550, totalLengthVHR2);
            totalLengthVHR2 += 15;
            vhrTf2.DrawString("If radiotherapy to breast was this for...", fontSmall, XBrushes.Black, new XRect(50, totalLengthVHR2, 500, 50));
            vhrTf2.DrawString("Hodgkins Lymphoma", fontSmall, XBrushes.Black, new XRect(300, totalLengthVHR2, 500, 50));
            vhrGfx2.DrawRectangle(XBrushes.Black, new XRect(500, totalLengthVHR2, 10, 10));
            vhrGfx2.DrawRectangle(XBrushes.White, new XRect(501, totalLengthVHR2 + 1, 8, 8));
            totalLengthVHR2 += 20;
            vhrTf2.DrawString("Non-Hodgkins Lymphoma", fontSmall, XBrushes.Black, new XRect(300, totalLengthVHR2, 500, 50));
            vhrGfx2.DrawRectangle(XBrushes.Black, new XRect(500, totalLengthVHR2, 10, 10));
            vhrGfx2.DrawRectangle(XBrushes.White, new XRect(501, totalLengthVHR2 + 1, 8, 8));
            totalLengthVHR2 += 15;
            vhrGfx2.DrawLine(penLight, 50, totalLengthVHR2, 550, totalLengthVHR2);
            totalLengthVHR2 += 15;

            content5 = "I can confirm that the woman has been informed that her details will be shared with the NHS Breast Screening Programme for the purpose of screening invitations when she becomes eligible.";

            vhrTf2.DrawString(content5, fontSmall, XBrushes.Black, new XRect(50, totalLengthVHR2, 500, 50));

            totalLengthVHR2 += 40;

            sigFilename = _lvm.staffMember.StaffForename + _lvm.staffMember.StaffSurname.Replace("'", "").Replace(" ", "") + ".jpg";

            if (!System.IO.File.Exists(@"wwwroot\Signatures\" + sigFilename)) { sigFilename = "empty.jpg"; } //this only exists because we can't define the image if it's null.

            XImage imageSig = XImage.FromFile(@"wwwroot\Signatures\" + sigFilename);
            int len = imageSig.PixelWidth;
            int hig = imageSig.PixelHeight;

            if (sigFilename != "empty.jpg")
            {
                vhrGfx2.DrawImage(imageSig, 50, totalLengthVHR2, len, hig);
            }
            signOff = _lvm.staffMember.NAME + System.Environment.NewLine + _lvm.staffMember.POSITION;
            vhrTf2.DrawString(signOff, font, XBrushes.Black, new XRect(300, totalLengthVHR2 + 20, 250, 50));
            totalLengthVHR2 = totalLengthVHR2 + hig + 20;

            vhrGfx3.DrawRectangle(penLight, new XRect(40, totalLengthVHR3, 520, 650));

            vhrTf3.DrawString("Section C: To be completed by Breast Screening Service", fontBold, XBrushes.Black, new XRect(50, totalLengthVHR3, 250, 50));
            totalLengthVHR3 += 30;
            vhrTf3.DrawString("Please tick the relevant box", fontTinyItalic, XBrushes.Black, new XRect(50, totalLengthVHR3, 250, 50));
            totalLengthVHR3 += 30;
            vhrGfx3.DrawRectangle(penLight, new XRect(45, totalLengthVHR3 - 10, 400, 30));
            vhrGfx3.DrawRectangle(penLight, new XRect(450, totalLengthVHR3 - 10, 50, 30));
            vhrTf3.DrawString("Referral accepted for high risk screening", fontSmall, XBrushes.Black, new XRect(50, totalLengthVHR3, 250, 50));
            //vhrGfx3.DrawRectangle(XBrushes.Black, new XRect(500, totalLengthVHR3, 10, 10));
            //vhrGfx3.DrawRectangle(XBrushes.White, new XRect(501, totalLengthVHR3 + 1, 8, 8));
            totalLengthVHR3 += 40;
            vhrGfx3.DrawRectangle(penLight, new XRect(45, totalLengthVHR3 - 10, 400, 30));
            vhrGfx3.DrawRectangle(penLight, new XRect(450, totalLengthVHR3 - 10, 50, 30));
            vhrTf3.DrawString("Referral rejected for high risk screening", fontSmall, XBrushes.Black, new XRect(50, totalLengthVHR3, 250, 50));
            //vhrGfx3.DrawRectangle(XBrushes.Black, new XRect(500, totalLengthVHR3, 10, 10));
            //vhrGfx3.DrawRectangle(XBrushes.White, new XRect(501, totalLengthVHR3 + 1, 8, 8));
            totalLengthVHR3 += 40;
            vhrTf3.DrawString("If referral rejected please specify reason for rejection:", fontTinyItalic, XBrushes.Black, new XRect(50, totalLengthVHR3, 250, 50));
            totalLengthVHR3 += 100;
            vhrTf3.DrawString("Section Radiologist Signature", fontTiny, XBrushes.Black, new XRect(50, totalLengthVHR3, 250, 50));
            totalLengthVHR3 += 40;
            vhrGfx3.DrawLine(penLight, 40, totalLengthVHR3 - 5, 560, totalLengthVHR3 - 5);
            vhrTf3.DrawString("Please complete details below and copy form to the referrer as receipt of referral.", fontSmallBold, XBrushes.Black, new XRect(50, totalLengthVHR3, 250, 50));
            totalLengthVHR3 += 40;
            vhrTf3.DrawString("Radiotherapy referrals: chn-bard@nhs.net", fontSmall, XBrushes.Black, new XRect(50, totalLengthVHR3, 250, 50));
            totalLengthVHR3 += 40;
            vhrTf3.DrawString("Clinical genetics: referring clinical genetics service", fontSmall, XBrushes.Black, new XRect(50, totalLengthVHR3, 250, 50));
            totalLengthVHR3 += 40;
            vhrTf3.DrawString("Oncology referrals: to individual oncologist and chn-bard@nhs.net", fontSmall, XBrushes.Black, new XRect(50, totalLengthVHR3, 250, 50));
            totalLengthVHR3 += 40;
            vhrTf3.DrawString("Woman Invited", fontSmall, XBrushes.Black, new XRect(50, totalLengthVHR3, 250, 50));
            vhrTf3.DrawString("Yes / No", fontSmall, XBrushes.Black, new XRect(200, totalLengthVHR3, 250, 50));
            vhrTf3.DrawString("Woman Screened", fontSmall, XBrushes.Black, new XRect(350, totalLengthVHR3, 250, 50));
            vhrTf3.DrawString("Yes / No", fontSmall, XBrushes.Black, new XRect(500, totalLengthVHR3, 250, 50));
            totalLengthVHR3 += 40;
            vhrGfx3.DrawLine(penLight, 40, totalLengthVHR3 - 5, 560, totalLengthVHR3 - 5);
            vhrTf3.DrawString("Authoriser's name:", fontSmall, XBrushes.Black, new XRect(50, totalLengthVHR3, 250, 50));
            totalLengthVHR3 += 40;
            vhrTf3.DrawString("Authoriser's signature:", fontSmall, XBrushes.Black, new XRect(50, totalLengthVHR3, 250, 50));
            totalLengthVHR3 += 40;
            vhrTf3.DrawString("Date:", fontSmall, XBrushes.Black, new XRect(50, totalLengthVHR3, 250, 50));
            totalLengthVHR3 += 40;
            vhrTf3.DrawString("(Referral forms can be authorised by a consultant radiologist, consultant practitioner or breast clinician.)", fontTiny, XBrushes.Black, new XRect(50, totalLengthVHR3, 250, 50));

            vhrDocument.Save($@"C:\CGU_DB\Letters\CaStdLetter-{fileCGU}-{docCode}-{mpiString}-0-{refIDString}-{printCount.ToString()}-{dateTimeString}-{diaryIDString}.pdf");
        }
        catch (Exception ex)
        {
            RedirectToAction("ErrorHome", "Error", new { error = ex.Message, formName = "StdLetter" });
        }
    }




}