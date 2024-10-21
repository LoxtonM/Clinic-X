using Microsoft.AspNetCore.Mvc;
using ClinicX.ViewModels;
using ClinicX.Data;
using ClinicX.Meta;
using PdfSharpCore.Pdf;
using PdfSharpCore.Drawing;
using PdfSharpCore.Drawing.Layout;
using ClinicX.Models;
using System.Text.RegularExpressions;
using System.Numerics;
using System.Reflection.Metadata;

namespace ClinicX.Controllers;

public class VHRController : Controller
{
    private readonly ClinicalContext _clinContext;
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

    public VHRController(ClinicalContext clinContext, DocumentContext docContext)
    {
        _clinContext = clinContext;
        _docContext = docContext;
        _lvm = new LetterVM();        
        _patientData = new PatientData(_clinContext);
        _staffUser = new StaffUserData(_clinContext);
        _documentsData = new DocumentsData(_docContext);
        _externalClinicianData = new ExternalClinicianData(_clinContext);
        _externalFacilityData = new ExternalFacilityData(_clinContext);
        _screenData = new ScreeningServiceData(_clinContext);
        _triageData = new TriageData(_clinContext);
        _riskData = new RiskData(_clinContext);
        _survData = new SurveillanceData(_clinContext);
        _bhsData = new BreastHistoryData(_clinContext);
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
    public void DoPDF(int id, int mpi, int icpCancerID, string user, string referrer, string? additionalText = "")
    {
        try
        {
            _lvm.staffMember = _staffUser.GetStaffMemberDetails(user);
            _lvm.patient = _patientData.GetPatientDetails(mpi);
            _lvm.documentsContent = _documentsData.GetDocumentDetails(id);
            _lvm.referrer = _externalClinicianData.GetClinicianDetails(referrer);

            int icpID = _triageData.GetCancerICPDetails(icpCancerID).ICPID;
            int refID = _triageData.GetICPDetails(icpID).REFID;

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
            int diaryID = 0;
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
            int totalLengthVHR = 150;
            int totalLengthVHR2 = 50;
            int totalLengthVHR3 = 50;
            int rotation = 90;

            vhrTf.DrawString("Referral to the NHSBSP for very high-risk screening", fontBold, XBrushes.Black, new XRect(50, totalLengthVHR, 500, 20));
            vhrGfx.RotateTransform(rotation);
            vhrTf.DrawString("Section A: To be completed by the Referrer", fontBold, XBrushes.Black, new XRect(150, -30, 500, 500));
            vhrGfx.RotateTransform(-rotation);
            totalLengthVHR += 20;
            vhrTf.DrawString("Patient Details", fontSmallBold, XBrushes.Black, new XRect(50, totalLengthVHR, 500, 20));
            totalLengthVHR += 20;
            content1 = patName + System.Environment.NewLine + System.Environment.NewLine + patAddress + System.Environment.NewLine + System.Environment.NewLine +
                "Tel Home: " + _lvm.patient.TEL + System.Environment.NewLine +                     
                "Mobile: " + _lvm.patient.PtTelMobile;

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
            vhrTf.DrawString(content2, fontSmall, XBrushes.Black, new XRect(300, totalLengthVHR, 500, 150));


            totalLengthVHR += 120;
            vhrTf.DrawString("Referrer Details", fontSmallBold, XBrushes.Black, new XRect(50, totalLengthVHR, 500, 20));
            totalLengthVHR += 10;
            content3 = "Referrer Name: " + _lvm.staffMember.NAME + System.Environment.NewLine +
                "Referrer Role: " + _lvm.staffMember.POSITION + System.Environment.NewLine +
                "Address:" + System.Environment.NewLine +
                _lvm.documentsContent.OurAddress;

            vhrTf.DrawString(content3, fontSmall, XBrushes.Black, new XRect(50, totalLengthVHR, 500, 120));
            totalLengthVHR += 120;

            ScreeningService sServ = _screenData.GetScreeningServiceDetails(_lvm.patient.GP_Facility_Code);

            vhrTf.DrawString("Referee Details", fontSmallBold, XBrushes.Black, new XRect(50, totalLengthVHR, 500, 20));
            totalLengthVHR += 10;
            content4 = sServ.Contact + System.Environment.NewLine + 
                sServ.Telephone + System.Environment.NewLine +
                sServ.Add1 + System.Environment.NewLine +
                sServ.Add2 + System.Environment.NewLine +
                sServ.Add3 + System.Environment.NewLine +
                sServ.Add4 + System.Environment.NewLine;

            if(sServ.Add5 != null){content4 = content4 + sServ.Add5 + System.Environment.NewLine; }
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
            totalLengthVHR += 20;
            if (additionalText != null)
            {
                vhrTf.DrawString(additionalText, fontSmall, XBrushes.Black, new XRect(50, totalLengthVHR, 500, 120));
            }
            totalLengthVHR += 40;
            content5 = "Please attach a copy of the genetics letter indicating levels of risk to this form in support of the information below." +
                    System.Environment.NewLine + "1.IBIS risk print out (report pts 10yr risk if they are aged between 25 - 29 - calculate at their " +
                    "current age, or if no mutation identified, or no genetic testing undertaken)" +
                    System.Environment.NewLine + "2.Lab report(if testing has been completed and pathogenic mutation identified)";

            vhrTf.DrawString(content5, fontSmall, XBrushes.Black, new XRect(50, totalLengthVHR, 500, 120));
            totalLengthVHR += 50;

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

            vhrGfx2.RotateTransform(rotation);
            vhrTf2.DrawString("Section B: To be completed by Genetics/Oncology", fontBold, XBrushes.Black, new XRect(50, -30, 500, 500));
            vhrGfx2.RotateTransform(-rotation);

            vhrTf2.DrawString("Risk", fontSmall, XBrushes.Black, new XRect(50, totalLengthVHR2, 500, 50));
            vhrTf2.DrawString("Age", fontSmall, XBrushes.Black, new XRect(300, totalLengthVHR2, 500, 50));
            vhrTf2.DrawString("Surveillance Protocol", fontSmall, XBrushes.Black, new XRect(400, totalLengthVHR2, 500, 50));

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

            vhrTf2.DrawString("Please give 10 year risk using the most appropriate age range", fontSmall, XBrushes.Black, new XRect(50, totalLengthVHR2, 500, 50));
            totalLengthVHR2 += 40;
            vhrTf2.DrawString("Age Category for Patient (years)", fontSmall, XBrushes.Black, new XRect(300, totalLengthVHR2, 500, 50));
            vhrTf2.DrawString("10 Year Risk", fontSmall, XBrushes.Black, new XRect(500, totalLengthVHR2, 500, 50));

            vhrTf2.DrawString("25-29", fontSmall, XBrushes.Black, new XRect(350, totalLengthVHR2 + 15, 500, 50));
            vhrTf2.DrawString("30-39", fontSmall, XBrushes.Black, new XRect(350, totalLengthVHR2 + 30, 500, 50));
            vhrTf2.DrawString("40-49", fontSmall, XBrushes.Black, new XRect(350, totalLengthVHR2 + 45, 500, 50));
            vhrTf2.DrawString("50-60", fontSmall, XBrushes.Black, new XRect(350, totalLengthVHR2 + 60, 500, 50));

            List<Risk> risk = _riskData.GetRiskList(icpCancerID);

            if (risk.Count > 0)
            {
                foreach (var item in risk)
                {
                    vhrTf2.DrawString(item.R25_29.ToString(), fontSmall, XBrushes.Black, new XRect(550, totalLengthVHR2 + 15, 500, 50));
                    vhrTf2.DrawString(item.R30_40.ToString(), fontSmall, XBrushes.Black, new XRect(550, totalLengthVHR2 + 30, 500, 50));
                    vhrTf2.DrawString(item.R40_50.ToString(), fontSmall, XBrushes.Black, new XRect(550, totalLengthVHR2 + 45, 500, 50));
                    vhrTf2.DrawString(item.R50_60.ToString(), fontSmall, XBrushes.Black, new XRect(550, totalLengthVHR2 + 60, 500, 50));

                    totalLengthVHR2 += 20;
                    vhrTf2.DrawString("Risk Date: " + item.RiskDate.Value.ToString("dd/MM/yyyy"), fontSmall, XBrushes.Black, new XRect(50, totalLengthVHR2, 500, 50));
                    totalLengthVHR2 += 15;
                    vhrTf2.DrawString("Calculation Tool: " + item.CalculationToolUsed, fontSmall, XBrushes.Black, new XRect(50, totalLengthVHR2, 500, 50));
                    totalLengthVHR2 += 20;
                }
            }
            totalLengthVHR2 += 40;
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
            totalLengthVHR2 += 50;
            vhrTf2.DrawString("Radiotherapy to breast tissue - irradiated between 20-29", fontSmall, XBrushes.Black, new XRect(50, totalLengthVHR2, 200, 50));
            vhrTf2.DrawString("30 to 39", fontSmall, XBrushes.Black, new XRect(300, totalLengthVHR2, 500, 50));
            vhrTf2.DrawString("MRI", fontSmall, XBrushes.Black, new XRect(350, totalLengthVHR2, 500, 50));
            vhrGfx2.DrawRectangle(XBrushes.Black, new XRect(500, totalLengthVHR2, 10, 10));
            vhrGfx2.DrawRectangle(XBrushes.White, new XRect(501, totalLengthVHR2 + 1, 8, 8));
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
            totalLengthVHR2 += 20;
            vhrTf2.DrawString("If radiotherapy to breast was this for...", fontSmall, XBrushes.Black, new XRect(50, totalLengthVHR2, 500, 50));
            vhrTf2.DrawString("Hodgkins Lymphoma", fontSmall, XBrushes.Black, new XRect(300, totalLengthVHR2, 500, 50));            
            vhrGfx2.DrawRectangle(XBrushes.Black, new XRect(500, totalLengthVHR2, 10, 10));
            vhrGfx2.DrawRectangle(XBrushes.White, new XRect(501, totalLengthVHR2 + 1, 8, 8));
            totalLengthVHR2 += 20;
            vhrTf2.DrawString("Non-Hodgkins Lymphoma", fontSmall, XBrushes.Black, new XRect(300, totalLengthVHR2, 500, 50));
            vhrGfx2.DrawRectangle(XBrushes.Black, new XRect(500, totalLengthVHR2, 10, 10));
            vhrGfx2.DrawRectangle(XBrushes.White, new XRect(501, totalLengthVHR2 + 1, 8, 8));
            totalLengthVHR2 += 20;

            content5 = "I can confirm that the woman has been informed that her details will be shared with the NHS Breast Screening Programme for the purpose of screening invitations when she becomes eligible.";

            vhrTf2.DrawString(content5, fontSmall, XBrushes.Black, new XRect(50, totalLengthVHR2, 500, 50));

            totalLengthVHR2 += 80;

            sigFilename = _lvm.staffMember.StaffForename + _lvm.staffMember.StaffSurname.Replace("'","").Replace(" ", "") + ".jpg";

            if (!System.IO.File.Exists(@"wwwroot\Signatures\" + sigFilename)) { sigFilename = "empty.jpg"; } //this only exists because we can't define the image if it's null.
            
            XImage imageSig = XImage.FromFile(@"wwwroot\Signatures\" + sigFilename);
            int len = imageSig.PixelWidth;
            int hig = imageSig.PixelHeight;

            if (sigFilename != "empty.jpg")
            {
                vhrGfx2.DrawImage(imageSig, 50, totalLengthVHR2, len, hig);
            }
            signOff = _lvm.staffMember.NAME + System.Environment.NewLine + _lvm.staffMember.POSITION;
            vhrTf2.DrawString(signOff, font, XBrushes.Black, new XRect(400, totalLengthVHR2+20, 250, 50));
            totalLengthVHR2 = totalLengthVHR2 + hig + 20;

            vhrTf3.DrawString("Section C: To be completed by Breast Screening Service", fontBold, XBrushes.Black, new XRect(50, totalLengthVHR3, 250, 50));
            totalLengthVHR3 += 30;
            vhrTf3.DrawString("Please tick the relevant box", fontTinyItalic, XBrushes.Black, new XRect(50, totalLengthVHR3, 250, 50));
            totalLengthVHR3 += 30;
            vhrTf3.DrawString("Referral accepted for high risk screening", fontSmall, XBrushes.Black, new XRect(50, totalLengthVHR3, 250, 50));
            vhrGfx3.DrawRectangle(XBrushes.Black, new XRect(500, totalLengthVHR3, 10, 10));
            vhrGfx3.DrawRectangle(XBrushes.White, new XRect(501, totalLengthVHR3 + 1, 8, 8));
            totalLengthVHR3 += 40;
            vhrTf3.DrawString("Referral rejected for high risk screening", fontSmall, XBrushes.Black, new XRect(50, totalLengthVHR3, 250, 50));
            vhrGfx3.DrawRectangle(XBrushes.Black, new XRect(500, totalLengthVHR3, 10, 10));
            vhrGfx3.DrawRectangle(XBrushes.White, new XRect(501, totalLengthVHR3 + 1, 8, 8));
            totalLengthVHR3 += 40;
            vhrTf3.DrawString("If referral rejected please specify reason for rejection:", fontTinyItalic, XBrushes.Black, new XRect(50, totalLengthVHR3, 250, 50));
            totalLengthVHR3 += 100;
            vhrTf3.DrawString("Section Radiologist Signature", fontTiny, XBrushes.Black, new XRect(50, totalLengthVHR3, 250, 50));
            totalLengthVHR3 += 40;            
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

    string RemoveHTML(string text)
    {
        
        text = text.Replace("&nbsp;", System.Environment.NewLine);
        text = text.Replace(System.Environment.NewLine, "newline");
        text = Regex.Replace(text, @"<[^>]+>", "").Trim();
        //text = Regex.Replace(text, @"\n{2,}", " ");
        text = text.Replace("&lt;", "<");
        text = text.Replace("&gt;", ">"); //because sometimes clinicians like to actually use those symbols
        text = text.Replace("newlinenewline", System.Environment.NewLine);
        text = text.Replace("newline", "");
        //this is the ONLY way to strip out the excessive new lines!! (and still can't remove all of them)

        return text;
    }
}

    
