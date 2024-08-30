using Microsoft.AspNetCore.Mvc;
using ClinicX.ViewModels;
using ClinicX.Data;
using ClinicX.Meta;
using PdfSharpCore.Pdf;
using PdfSharpCore.Drawing;
using PdfSharpCore.Drawing.Layout;
using ClinicX.Models;
using System.Text.RegularExpressions;

namespace ClinicX.Controllers;

public class LetterController : Controller
{
    private readonly ClinicalContext _clinContext;
    private readonly DocumentContext _docContext;
    private readonly LetterVM _lvm;
    private readonly IPatientData _patientData;
    private readonly IReferralData _referralData;
    private readonly IDictatedLetterData _dictatedLetterData;
    private readonly IStaffUserData _staffUser;
    private readonly IDocumentsData _documentsData;
    private readonly IExternalClinicianData _externalClinicianData;
    private readonly IExternalFacilityData _externalFacilityData;
    private readonly IConstantsData _constantsData;
    //private readonly IConfiguration _config;
    //private readonly ICRUD _crud;

    public LetterController(ClinicalContext clinContext, DocumentContext docContext)
    {
        _clinContext = clinContext;
        _docContext = docContext;
       // _config = config;
        _lvm = new LetterVM();        
        _patientData = new PatientData(_clinContext);
        _referralData = new ReferralData(_clinContext);
        _staffUser = new StaffUserData(_clinContext);
        _dictatedLetterData = new DictatedLetterData(_clinContext);
        _documentsData = new DocumentsData(_docContext);
        _externalClinicianData = new ExternalClinicianData(_clinContext);
        _externalFacilityData = new ExternalFacilityData(_clinContext);
        _constantsData = new ConstantsData(_clinContext);
       // _crud = new CRUD(_config);
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
    public void PreviewDOTPDF(int dID,string user)
    {
        try
        {
            _lvm.staffMember = _staffUser.GetStaffMemberDetails(user);
            _lvm.dictatedLetter = _dictatedLetterData.GetDictatedLetterDetails(dID);                        
            string ourAddress = _docContext.DocumentsContent.FirstOrDefault(d => d.OurAddress != null).OurAddress;            
            //creates a new PDF document
            PdfDocument document = new PdfDocument();
            document.Info.Title = "DOT Letter Preview";
            PdfPage page = document.AddPage();
            PdfPage page2 = document.AddPage();
            PdfPage page3 = document.AddPage();
            PdfPage page4 = document.AddPage(); //we HAVE to add all four pages whether we want them or not, or we can't use them later!
            XGraphics gfx = XGraphics.FromPdfPage(page);
            var tf = new XTextFormatter(gfx);
            //set the fonts used for the letters
            XFont font = new XFont("Arial", 12, XFontStyle.Regular);
            XFont fontBold = new XFont("Arial", 12, XFontStyle.Bold);
            XFont fontItalic = new XFont("Arial", 12, XFontStyle.Italic);
            //Load the image for the letter head
            XImage image = XImage.FromFile(@"wwwroot\Letterhead.jpg");
            gfx.DrawImage(image, 350, 20, image.PixelWidth / 2, image.PixelHeight / 2);
            //Create the stuff that's common to all letters
            tf.Alignment = XParagraphAlignment.Right;
            //Our address and contact details
            tf.DrawString(ourAddress, font, XBrushes.Black, new XRect(-20, 150, page.Width, 200));
            //the email address just absolutely will not align right, for some stupid reason!!! So we have to force it.
            tf.DrawString(_constantsData.GetConstant("MainCGUEmail", 1), font, XBrushes.Black, new XRect(450, 250, page.Width, 200));

            tf.Alignment = XParagraphAlignment.Left;
            tf.DrawString($"Consultant: {_lvm.dictatedLetter.Consultant}", fontBold, XBrushes.Black, new XRect(20, 150, page.Width/2, 200));
            tf.DrawString($"Genetic Counsellor: {_lvm.dictatedLetter.GeneticCounsellor}", fontBold, XBrushes.Black, new XRect(20, 165, page.Width/2, 200));

            //Note: Xrect parameters are: (Xpos, Ypos, Width, Depth) - use to position blocks of text
            //Depth of 10 seems sufficient for one line of text; 30 is sufficient for two lines. 7 lines needs 100.

            string phoneNumbers = "Secretaries Direct Line:" + System.Environment.NewLine;

            var secretariesList = _staffUser.GetStaffMemberList().Where(s => s.BILL_ID == _lvm.dictatedLetter.SecTeam && s.CLINIC_SCHEDULER_GROUPS == "Admin");
            foreach (var t in secretariesList)
            {
                phoneNumbers = phoneNumbers + $"{t.NAME} {t.TELEPHONE}" + System.Environment.NewLine;
            }
            
            tf.DrawString(phoneNumbers, font, XBrushes.Black, new XRect(20, 200, page.Width/2, 200));

            string datesInfo = "";

            if (_lvm.dictatedLetter.DateDictated != null)
            {
                datesInfo = $"Dictated Date: {_lvm.dictatedLetter.DateDictated.Value.ToString("dd/MM/yyyy")}" + System.Environment.NewLine +
                                   $"Date Typed: {_lvm.dictatedLetter.CreatedDate.Value.ToString("dd/MM/yyyy")}";
            }         
            _lvm.patient = _patientData.GetPatientDetails(_lvm.dictatedLetter.MPI);
            tf.DrawString($"Please quote our reference on all correspondence: {_lvm.patient.CGU_No}", fontItalic, XBrushes.Black, new XRect(20, 270, page.Width, 200));

            tf.DrawString(datesInfo, font, XBrushes.Black, new XRect(20, 290, page.Width / 2, 200));

            //recipient's address
                       
            string address = "";            

            address = _lvm.dictatedLetter.LetterTo;

            tf.DrawString(address, font, XBrushes.Black, new XRect(50, 350, 490, 100));

            //Date letter created
            //tf.DrawString(DateTime.Today.ToString("dd MMMM yyyy"), font, XBrushes.Black, new XRect(50, 450, 500, 10)); //today's date

            tf.DrawString($"Dear {_lvm.dictatedLetter.LetterToSalutation}", font, XBrushes.Black, new XRect(20, 450, 500, 10)) ; //salutation

            //Content containers for all of the paragraphs, as well as other data required
            string letterRe = _lvm.dictatedLetter.LetterRe;
            string summary = _lvm.dictatedLetter.LetterContentBold;
            string content = _lvm.dictatedLetter.LetterContent;
            string quoteRef = "";
            string signOff = "";
            string sigFilename = "";
            string docCode = "DOT";
            //string referrerName = _lvm.referrer.TITLE + " " + _lvm.referrer.FIRST_NAME + " " + _lvm.referrer.NAME;
            string cc = "";
            string cc2 = "";
            int printCount = 1;
            int totalLength = 500; //used for spacing - so the paragraphs can dynamically resize
            int totalLength2 = 0;
            int totalLength3 = 0;
            int totalLength4 = 0; //for multiple pages
            XGraphics gfx2 = XGraphics.FromPdfPage(page2); //they have to be declared here or I can't use them later
            var tf2 = new XTextFormatter(gfx2);
            XGraphics gfx3 = XGraphics.FromPdfPage(page3);
            var tf3 = new XTextFormatter(gfx3);
            XGraphics gfx4 = XGraphics.FromPdfPage(page4);
            var tf4 = new XTextFormatter(gfx4);

            if (summary == null) { summary = ""; }

            if (letterRe == null) { letterRe = ""; }

            if (content.Contains("</")) { content = RemoveHTML(content); }

            if (_lvm.dictatedLetter.LetterRe != null)
            {
                tf.DrawString($"Re  {letterRe}", fontBold, XBrushes.Black, new XRect(20, totalLength, 500, letterRe.Length));
            }

            totalLength = totalLength + (letterRe.Length / 2) + 20;
            tf.DrawString(summary, fontBold, XBrushes.Black, new XRect(20, totalLength, 500, summary.Length));
            totalLength = totalLength + (summary.Length / 2) + 20;
            
            signOff = _lvm.staffMember.NAME + Environment.NewLine + _lvm.staffMember.POSITION;
            sigFilename = $"{_lvm.staffMember.StaffForename}{_lvm.staffMember.StaffSurname}.jpg";
            totalLength = totalLength + 20;

            if (content.Length < 200) //split the content so it goes over multiple pages if more than one page
            {                
                tf.DrawString(content, font, XBrushes.Black, new XRect(20, totalLength, 500, content.Length));
                totalLength = totalLength + content.Length + 20;                               
            }
            else
            {                
                string[] lines = content.Split(
                new string[] { Environment.NewLine },
                StringSplitOptions.None);                                
                                
                tf2 = new XTextFormatter(gfx2);
                totalLength2 = 20;
                                
                tf3 = new XTextFormatter(gfx3);
                totalLength3 = 20;

                tf4 = new XTextFormatter(gfx4);
                totalLength4 = 20;

                foreach (var line in lines)
                {
                    if(totalLength < 800)
                    {
                        tf.DrawString(line, font, XBrushes.Black, new XRect(20, totalLength, 500, lines[0].Length));
                        totalLength = totalLength + (lines[0].Length / 4) + 10;
                    }
                    else if (totalLength2 < 800)
                    {
                        tf2.DrawString(line, font, XBrushes.Black, new XRect(20, totalLength2, 500, line.Length));
                        totalLength2 = totalLength2 + (line.Length / 4) + 10;
                    }
                    else if (totalLength3 < 800)
                    {
                        tf3.DrawString(line, font, XBrushes.Black, new XRect(20, totalLength3, 500, line.Length));
                        totalLength3 = totalLength3 + (line.Length / 4) + 10;
                    }
                    else if (totalLength4 < 800)
                    {
                        tf4.DrawString(line, font, XBrushes.Black, new XRect(20, totalLength4, 500, line.Length));
                        totalLength4 = totalLength4 + (line.Length / 4) + 10;
                    }
                }
                totalLength2 = totalLength2 + 20;                             
            }

            if (System.IO.File.Exists(@$"wwwroot\Signatures\{sigFilename}"))
            {
                XImage imageSig = XImage.FromFile(@$"wwwroot\Signatures\{sigFilename}");

                int len = imageSig.PixelWidth;
                int hig = imageSig.PixelHeight;

                if (totalLength < 800)
                {
                    document.Pages.Remove(page2);
                    document.Pages.Remove(page3);
                    document.Pages.Remove(page4);
                    tf.DrawString("Yours sincerely,", font, XBrushes.Black, new XRect(20, totalLength, 500, 20));
                    totalLength = totalLength + 20;
                    gfx.DrawImage(imageSig, 50, totalLength, len, hig);
                    totalLength = totalLength + hig + 20;
                    tf.DrawString(signOff, font, XBrushes.Black, new XRect(20, totalLength, 500, 20));
                }
                else if (totalLength2 < 800)
                {
                    document.Pages.Remove(page3);
                    document.Pages.Remove(page4);
                    totalLength2 = totalLength2 + 20;
                    tf2.DrawString("Yours sincerely,", font, XBrushes.Black, new XRect(20, totalLength2, 500, 20));
                    totalLength2 = totalLength2 + 20;
                    gfx2.DrawImage(imageSig, 50, totalLength2, len, hig);
                    totalLength2 = totalLength2 + hig + 20;
                    tf2.DrawString(signOff, font, XBrushes.Black, new XRect(20, totalLength2, 500, 20));
                }
                else if(totalLength3 < 800)
                {
                    document.Pages.Remove(page4);
                    totalLength3 = totalLength3 + 20;
                    tf3.DrawString("Yours sincerely,", font, XBrushes.Black, new XRect(20, totalLength3, 500, 20));
                    totalLength3 = totalLength3 + 20;
                    gfx3.DrawImage(imageSig, 50, totalLength3, len, hig);
                    totalLength3 = totalLength3 + hig + 20;
                    tf3.DrawString(signOff, font, XBrushes.Black, new XRect(20, totalLength3, 500, 20));
                }
                else
                {
                    totalLength4 = totalLength4 + 20;
                    tf4.DrawString("Yours sincerely,", font, XBrushes.Black, new XRect(20, totalLength4, 500, 20));
                    totalLength4 = totalLength4 + 20;
                    gfx4.DrawImage(imageSig, 50, totalLength4, len, hig);
                    totalLength4 = totalLength4 + hig + 20;
                    tf4.DrawString(signOff, font, XBrushes.Black, new XRect(20, totalLength4, 500, 20));
                }                
            }

            //document.Save($"c:\\projects\\VS Test\\ClinicX\\wwwroot\\DOTLetterPreviews\\preview-{user}.pdf");
            document.Save($"~/DOTLetterPreviews/preview-{user}.pdf");
        }

        catch (Exception ex)
        {
            RedirectToAction("ErrorHome", "Error", new { error = ex.Message, formName = "DOTPreview" });
        }
    }

    //Prints standard letter templates from the menu
    public void DoPDF(int id, int mpi, int refID, string user, string referrer, string? additionalText = "", string? enclosures = "", int? reviewAtAge = 0,
        string? tissueType = "", bool? isResearchStudy = false, bool? isScreeningRels = false, int? diaryID = 0, string? freeText="")
    {
        try
        {
            _lvm.staffMember = _staffUser.GetStaffMemberDetails(user);
            _lvm.patient = _patientData.GetPatientDetails(mpi);
            _lvm.documentsContent = _documentsData.GetDocumentDetails(id);
            _lvm.referrer = _externalClinicianData.GetClinicianDetails(referrer);

            var referral = _referralData.GetReferralDetails(refID);

            //creates a new PDF document
            PdfDocument document = new PdfDocument();
            document.Info.Title = "My PDF";
            PdfPage page = document.AddPage();
            XGraphics gfx = XGraphics.FromPdfPage(page);
            var tf = new XTextFormatter(gfx);
            PdfPage page2 = document.AddPage();
            XGraphics gfx2 = XGraphics.FromPdfPage(page2);
            var tf2 = new XTextFormatter(gfx2);
            //set the fonts used for the letters
            XFont font = new XFont("Arial", 12, XFontStyle.Regular);
            XFont fontBold = new XFont("Arial", 12, XFontStyle.Bold);
            XFont fontItalic = new XFont("Arial", 12, XFontStyle.Bold);
            //Load the image for the letter head
            XImage image = XImage.FromFile(@"wwwroot\Letterhead.jpg");
            gfx.DrawImage(image, 350, 20, image.PixelWidth / 2, image.PixelHeight / 2);
            //Create the stuff that's common to all letters
            tf.Alignment = XParagraphAlignment.Right;
            //Our address and contact details
            tf.DrawString(_lvm.documentsContent.OurAddress, font, XBrushes.Black, new XRect(-20, 150, page.Width, 200));
            if (_lvm.documentsContent.DirectLine != null) //because we have to trap them nulls!
            {
                tf.DrawString(_lvm.documentsContent.DirectLine, fontBold, XBrushes.Black, new XRect(-20, 250, page.Width, 10));
            }
            tf.DrawString(_lvm.documentsContent.OurEmailAddress, font, XBrushes.Black, new XRect(-20, 270, page.Width, 10));

            //Note: Xrect parameters are: (Xpos, Ypos, Width, Depth) - use to position blocks of text
            //Depth of 10 seems sufficient for one line of text; 30 is sufficient for two lines. 7 lines needs 100.

            //patient's address
            tf.Alignment = XParagraphAlignment.Left;
            tf.DrawString(_lvm.patient.PtLetterAddressee, font, XBrushes.Black, new XRect(50, 235, 500, 10));

            string address = "";
            string salutation = "";

            if (_lvm.documentsContent.LetterTo == "PT")
            {
                salutation = _lvm.patient.SALUTATION;

                address = _lvm.patient.ADDRESS1 + Environment.NewLine;
                if (_lvm.patient.ADDRESS2 != null) //this is sometimes null
                {
                    address = address + _lvm.patient.ADDRESS2 + Environment.NewLine;
                }
                address = address + _lvm.patient.ADDRESS3 + Environment.NewLine;
                address = address + _lvm.patient.ADDRESS4 + Environment.NewLine;
                address = address + _lvm.patient.POSTCODE;
            }
            if (_lvm.documentsContent.LetterTo == "RD")
            {
                //placeholder
            }
            if (_lvm.documentsContent.LetterTo == "GP")
            {                
                var gpAddressee = _externalClinicianData.GetClinicianDetails(_lvm.patient.GP_Code);
                salutation = gpAddressee.TITLE + " " + gpAddressee.FIRST_NAME + " " + gpAddressee.NAME;
                var gpFac = _externalFacilityData.GetFacilityDetails(gpAddressee.FACILITY);
                address = gpFac.NAME + Environment.NewLine;
                address = address + gpFac.ADDRESS + Environment.NewLine;
                address += address + gpFac.CITY + Environment.NewLine;
                address += address + gpFac.STATE + Environment.NewLine;
                address += address + gpFac.ZIP + Environment.NewLine;
            }

            tf.DrawString(address, font, XBrushes.Black, new XRect(50, 250, 490, 100));

            //Date letter created
            tf.DrawString(DateTime.Today.ToString("dd MMMM yyyy"), font, XBrushes.Black, new XRect(50, 350, 500, 10)); //today's date

            tf.DrawString("Dear " + salutation, font, XBrushes.Black, new XRect(50, 375, 500, 10)); //salutation

            //Content containers for all of the paragraphs, as well as other data required
            string content1 = "";
            string content2 = "";
            string content3 = "";
            string content4 = "";
            string content5 = "";
            string content6 = "";
            string freetext = freeText;
            string quoteRef = "";
            string signOff = "";
            string sigFlename = "";
            string docCode = _lvm.documentsContent.DocCode;
            //WHY IS THERE ALWAYS A NULL SOMEWHWERE?????????
            string referrerName = "";
            if (_lvm.referrer != null) { referrerName = _lvm.referrer.TITLE + " " + _lvm.referrer.FIRST_NAME + " " + _lvm.referrer.NAME; }
            string cc = "";
            string cc2 = "";
            int printCount = 1;
            int totalLength = 400; //used for spacing - so the paragraphs can dynamically resize
            int pageCount = 1;

            quoteRef = "Please quote this reference on all correspondence: " + _lvm.patient.CGU_No + Environment.NewLine;
            quoteRef = quoteRef + "NHS number: " + _lvm.patient.SOCIAL_SECURITY + Environment.NewLine;
            quoteRef = quoteRef + "Consultant: " + referral.LeadClinician + Environment.NewLine;
            quoteRef = quoteRef + "Genetic Counsellor: " + referral.GC;
            tf.DrawString(quoteRef, font, XBrushes.Black, new XRect(50, 130, page.Width, 150)); //"Please quote CGU number" etc

            ///////////////////////////////////////////////////////////////////////////////////////
            //////All letter templates need to be defined individually here////////////////////////            
            ///////////////////////////////////////////////////////////////////////////////////////

            //Ack letter
            if (docCode == "Ack")
            {
                tf.Alignment = XParagraphAlignment.Left;
                content1 = _lvm.documentsContent.Para1;
                tf.DrawString(content1, font, XBrushes.Black, new XRect(50, totalLength, 500, content1.Length / 4));
                totalLength = totalLength + content1.Length / 4 - 40;
                signOff = _lvm.staffMember.NAME + Environment.NewLine + _lvm.staffMember.POSITION;               
                cc = referrerName;
            }

            //CTB Ack letter
            if (docCode == "CTBAck") 
            {
                content1 = referrerName + " " + _lvm.documentsContent.Para1;
                content2 = _lvm.documentsContent.Para2;
                content3 = _lvm.documentsContent.Para3 + Environment.NewLine + Environment.NewLine + _lvm.documentsContent.Para4;

                tf.DrawString(content1, font, XBrushes.Black, new XRect(50, totalLength, 500, content1.Length / 4));
                totalLength = totalLength + content1.Length / 4 + 10;
                tf.DrawString(content2, fontBold, XBrushes.Black, new XRect(50, totalLength, 500, content2.Length / 4));
                totalLength = totalLength + content2.Length / 4 + 10;
                tf.DrawString(content3, font, XBrushes.Black, new XRect(50, totalLength, 500, content3.Length / 3));
                totalLength = totalLength + content3.Length / 3;               
                signOff = "CGU Booking Centre";                
                cc = referrerName;
            }

            //KC letter
            if (docCode == "Kc")
            {
                pageCount = 2; //because this can't happen automatically, obviously, so we have to hard code it! Oh PDFSharp you are so wonderful.
                tf.Alignment = XParagraphAlignment.Left;
                content1 = _lvm.documentsContent.Para1 + " " + referrerName + " " + _lvm.documentsContent.Para2 +
                    Environment.NewLine + Environment.NewLine + _lvm.documentsContent.Para3 +
                    Environment.NewLine + Environment.NewLine + _lvm.documentsContent.Para4;
                content2 = _lvm.documentsContent.Para5;
                tf.DrawString(content1, font, XBrushes.Black, new XRect(50, totalLength, 500, content1.Length / 4));
                totalLength = totalLength + content1.Length / 4 - 40;                
                tf2.DrawString(content2, font, XBrushes.Black, new XRect(50, 50, 500, content2.Length / 4));
                totalLength = 50 + content2.Length / 4;
                signOff = _lvm.staffMember.NAME + Environment.NewLine + _lvm.staffMember.POSITION;
            }

            //PrC letter
            if (docCode == "PrC")
            {
                content1 = _lvm.documentsContent.Para1;
                content2 = _lvm.documentsContent.Para2;
                content3 = _lvm.documentsContent.Para3;
                content4 = _lvm.documentsContent.Para4;

                tf.DrawString(content1, font, XBrushes.Black, new XRect(50, totalLength, 500, content1.Length / 4));
                totalLength = totalLength + content1.Length / 4;
                tf.DrawString(content2, font, XBrushes.Black, new XRect(50, totalLength, 500, content2.Length / 4));
                totalLength = totalLength + content2.Length / 4;
                tf.DrawString(content3, font, XBrushes.Black, new XRect(50, totalLength, 500, content3.Length / 4));
                totalLength = totalLength + content3.Length / 4;
                tf.DrawString(content4, font, XBrushes.Black, new XRect(50, totalLength, 500, content4.Length / 4));
                totalLength = totalLength + content4.Length / 4;
                signOff = _lvm.staffMember.NAME + Environment.NewLine + _lvm.staffMember.POSITION;
            }

            //O1 letter
            if (docCode == "O1")
            {  
                content1 = _lvm.documentsContent.Para1 + Environment.NewLine + Environment.NewLine + _lvm.documentsContent.Para2;
                content2 = additionalText;
                content3 = _lvm.documentsContent.Para4;
                content4 = _lvm.documentsContent.Para7;
                content5 = _lvm.documentsContent.Para9;

                tf.DrawString(content1, font, XBrushes.Black, new XRect(50, 400, 500, content1.Length / 4));
                totalLength = totalLength + content1.Length / 4 + 20;
                if (content2 != null && content2 != "")
                {
                    tf.DrawString(content2, fontBold, XBrushes.Black, new XRect(50, totalLength, 500, content2.Length / 4));
                    totalLength = totalLength + content2.Length / 4;
                }
                tf.DrawString(content3, font, XBrushes.Black, new XRect(50, totalLength, 500, content3.Length / 4));
                totalLength = totalLength + content3.Length / 4;
                tf.DrawString(content4, font, XBrushes.Black, new XRect(50, totalLength, 500, content4.Length / 4));
                totalLength = totalLength + content4.Length / 4;
                signOff = _lvm.staffMember.NAME + Environment.NewLine + _lvm.staffMember.POSITION;
                cc = referrerName;
            }

            //O1a
            if (docCode == "O1A")
            {
                content1 = _lvm.documentsContent.Para1 + Environment.NewLine + Environment.NewLine + _lvm.documentsContent.Para2;
                content2 = additionalText;
                if (reviewAtAge > 0)
                {
                    content3 = "This advice is based upon the information currently available.  You may wish to contact us again around the age of " +
                        reviewAtAge.ToString() + " so we can update our advice.";
                }
                if (tissueType != "")
                {
                    content4 = "Further Investigations: "; //all these strings have been hard-coded in the Access front-end!
                    if (tissueType == "Blood")
                    {
                        content4 = content4 + "It may also be useful to store a sample of blood from one of your relatives who has had cancer.  This may enable genetic testing to be pursued in the future if there are further developments in knowledge or technology. If you are interested in discussing this further, please contact the department to discuss this with the genetic counsellor.";
                    }
                    else if (tissueType == "Tissue")
                    {
                        content4 = content4 + "We may be able to do further tests on samples of tumour tissue which may have been stored from your relatives who have had cancer. This could help to clarify whether the cancers in the family may be due to a family predisposition. In turn, we may then be able to give more accurate screening advice for you and your relatives. If you are interested in discussing this further, please contact the department to discuss this with the genetic counsellor.";
                    }
                    else if (tissueType == "Blood & Tissue")
                    {
                        content4 = content4 + "We may be able to do further tests on samples of tumour tissue which may have been stored from your relatives who have had cancer. This could help to clarify whether the cancers in the family may be due to a family predisposition. In turn, we may then be able to give more accurate screening advice for you and your relatives. It may also be useful to store a sample of blood from one of your relatives who has had cancer.  This may enable genetic testing to be pursued in the future if there are further developments in knowledge or technology. If you are interested in discussing this further, please contact the department to discuss this with the genetic counsellor.";
                    }
                }
                content5 = _lvm.documentsContent.Para3 + Environment.NewLine + Environment.NewLine + _lvm.documentsContent.Para4;
                if (isResearchStudy.GetValueOrDefault())
                {
                    content6 = _lvm.documentsContent.Para9;
                }
                tf.DrawString(content1, font, XBrushes.Black, new XRect(50, 400, 500, content1.Length / 4));
                totalLength = totalLength + content1.Length / 4 + 20;
                if (content2 != null && content2 != "")
                {
                    tf.DrawString(content2, font, XBrushes.Black, new XRect(50, totalLength, 500, content2.Length / 4));
                    totalLength = totalLength + content2.Length / 4 + 20;
                }
                if (content3 != null && content3 != "")
                {
                    tf.DrawString(content3, font, XBrushes.Black, new XRect(50, totalLength, 500, content3.Length / 4));
                    totalLength = totalLength + content3.Length / 4 + 20;
                }
                tf.DrawString(content4, font, XBrushes.Black, new XRect(50, totalLength, 500, content3.Length / 4));
                totalLength = totalLength + content3.Length / 4 + 20;
                tf.DrawString(content5, font, XBrushes.Black, new XRect(50, totalLength, 500, content4.Length / 4));
                totalLength = totalLength + content4.Length + 40;
                signOff = _lvm.staffMember.NAME + Environment.NewLine + _lvm.staffMember.POSITION;
                cc = referrerName;
            }

            //O1c
            if (docCode == "O1C")
            {
                content1 = _lvm.documentsContent.Para1 + Environment.NewLine + Environment.NewLine + _lvm.documentsContent.Para2 + Environment.NewLine +
                    Environment.NewLine + _lvm.documentsContent.Para3 + Environment.NewLine + Environment.NewLine + _lvm.documentsContent.Para4;
                if (reviewAtAge > 0)
                {
                    content2 = "This advice is based upon the information currently available.  You may wish to contact us again around the age of " +
                        reviewAtAge.ToString() + " so we can update our advice.";
                }
                if (isResearchStudy.GetValueOrDefault())
                {
                    content3 = _lvm.documentsContent.Para9;
                }

                tf.DrawString(content1, font, XBrushes.Black, new XRect(50, 400, 500, content1.Length / 4));
                totalLength = totalLength + content1.Length / 4;
                if (content2 != null && content2 != "")
                {
                    tf.DrawString(content2, font, XBrushes.Black, new XRect(50, totalLength, 500, content2.Length / 4));
                    totalLength = totalLength + content2.Length / 4;
                }
                if (content3 != null && content3 != "")
                {
                    tf.DrawString(content3, font, XBrushes.Black, new XRect(50, totalLength, 500, content3.Length / 4));
                    totalLength = totalLength + content3.Length / 4;
                }
                signOff = _lvm.staffMember.NAME + Environment.NewLine + _lvm.staffMember.POSITION;
                cc = referrerName;
            }

            //O2
            if (docCode == "O2")
            {                
                content1 = _lvm.documentsContent.Para1;
                content2 = _lvm.documentsContent.Para2 + " " + additionalText;
                if (isScreeningRels.GetValueOrDefault())
                {
                    content3 = _lvm.documentsContent.Para8;
                }
                if (reviewAtAge > 0)
                {
                    content4 = "This advice is based upon the information currently available.  You may wish to contact us again around the age of "
                        + reviewAtAge.ToString() + " so we can update our advice.";
                }
                if (tissueType != "")
                {
                    content5 = "Further Investigations: ";
                    if (tissueType == "Blood")
                    {
                        content5 = content5 + "It may also be useful to store a sample of blood from one of your relatives who has had cancer.  This may enable genetic testing to be pursued in the future if there are further developments in knowledge or technology. If you are interested in discussing this further, please contact the department to discuss this with the genetic counsellor.";
                    }
                    else if (tissueType == "Tissue")
                    {
                        content5 = content5 + "We may be able to do further tests on samples of tumour tissue which may have been stored from your relatives who have had cancer. This could help to clarify whether the cancers in the family may be due to a family predisposition. In turn, we may then be able to give more accurate screening advice for you and your relatives. If you are interested in discussing this further, please contact the department to discuss this with the genetic counsellor.";
                    }
                    else if (tissueType == "Blood & Tissue")
                    {
                        content5 = content5 + "We may be able to do further tests on samples of tumour tissue which may have been stored from your relatives who have had cancer. This could help to clarify whether the cancers in the family may be due to a family predisposition. In turn, we may then be able to give more accurate screening advice for you and your relatives. It may also be useful to store a sample of blood from one of your relatives who has had cancer.  This may enable genetic testing to be pursued in the future if there are further developments in knowledge or technology. If you are interested in discussing this further, please contact the department to discuss this with the genetic counsellor.";
                    }
                }

                tf.DrawString(content1, font, XBrushes.Black, new XRect(50, 400, 500, content1.Length / 4));
                totalLength = totalLength + content1.Length / 3;
                tf.DrawString(content2, font, XBrushes.Black, new XRect(50, totalLength, 500, content2.Length / 4));
                totalLength = totalLength + content2.Length / 4;
                if (content3 != "")
                { 
                    tf.DrawString(content3, font, XBrushes.Black, new XRect(50, totalLength, 500, content3.Length / 4));
                    totalLength = totalLength + content3.Length / 4;
                }
                if (content4 != "")
                {
                    tf.DrawString(content4, font, XBrushes.Black, new XRect(50, totalLength, 500, content4.Length / 4));
                    totalLength = totalLength + content4.Length / 4;
                }
                if (content5 != "")
                {
                    tf.DrawString(content5, fontBold, XBrushes.Black, new XRect(50, totalLength, 500, content5.Length / 4));
                    totalLength = totalLength + content5.Length / 4;
                }
                signOff = _lvm.staffMember.NAME + Environment.NewLine + _lvm.staffMember.POSITION;
                cc = referrerName;
            }

            //O2a
            if (docCode == "O2a")
            {
                content1 = _lvm.documentsContent.Para1 + Environment.NewLine + Environment.NewLine + _lvm.documentsContent.Para2 + " " + additionalText;
                if (tissueType != "")
                {
                    content2 = "Further Investigations: "; 
                    if (tissueType == "Blood")
                    {
                        content2 = content2 + "It may also be useful to store a sample of blood from one of your relatives who has had cancer.  This may enable genetic testing to be pursued in the future if there are further developments in knowledge or technology. If you are interested in discussing this further, please contact the department to discuss this with the genetic counsellor.";
                    }
                    else if (tissueType == "Tissue")
                    {
                        content2 = content2 + "We may be able to do further tests on samples of tumour tissue which may have been stored from your relatives who have had cancer. This could help to clarify whether the cancers in the family may be due to a family predisposition. In turn, we may then be able to give more accurate screening advice for you and your relatives. If you are interested in discussing this further, please contact the department to discuss this with the genetic counsellor.";
                    }
                    else if (tissueType == "Blood & Tissue")
                    {
                        content2 = content2 + "We may be able to do further tests on samples of tumour tissue which may have been stored from your relatives who have had cancer. This could help to clarify whether the cancers in the family may be due to a family predisposition. In turn, we may then be able to give more accurate screening advice for you and your relatives. It may also be useful to store a sample of blood from one of your relatives who has had cancer.  This may enable genetic testing to be pursued in the future if there are further developments in knowledge or technology. If you are interested in discussing this further, please contact the department to discuss this with the genetic counsellor.";
                    }
                }
                content3 = _lvm.documentsContent.Para3 + Environment.NewLine + Environment.NewLine + _lvm.documentsContent.Para4;
                if (isResearchStudy.GetValueOrDefault())
                {
                    content4 = _lvm.documentsContent.Para9;
                }
                if (reviewAtAge > 0)
                {
                    content5 = "This advice is based upon the information currently available.  You may wish to contact us again around the age of "
                        + reviewAtAge.ToString() + " so we can update our advice.";
                }                

                tf.DrawString(content1, font, XBrushes.Black, new XRect(50, 400, 500, content1.Length / 4));
                totalLength = totalLength + content1.Length /4 + 30;
                if (content2 != "")
                {
                    tf.DrawString(content2, font, XBrushes.Black, new XRect(50, totalLength, 500, content2.Length / 4));
                    totalLength = totalLength + content2.Length / 4;
                }
                tf.DrawString(content3, font, XBrushes.Black, new XRect(50, totalLength, 500, content3.Length / 4));
                totalLength = totalLength + content3.Length / 4;
                
                if (content4 != "")
                {
                    tf.DrawString(content4, font, XBrushes.Black, new XRect(50, totalLength, 500, content4.Length / 4));
                    totalLength = totalLength + content4.Length / 4;
                }
                if (content5 != "")
                {
                    tf.DrawString(content5, fontBold, XBrushes.Black, new XRect(50, totalLength, 500, content5.Length / 4));
                    totalLength = totalLength + content5.Length / 4;
                }
                signOff = _lvm.staffMember.NAME + Environment.NewLine + _lvm.staffMember.POSITION;
                cc = referrerName;                
            }

            //O2d
            if (docCode == "O2d")
            {
                content1 = _lvm.documentsContent.Para1;
                content2 = _lvm.documentsContent.Para2;
                content3 = additionalText;

                tf.DrawString(content1, font, XBrushes.Black, new XRect(50, 400, 500, content1.Length / 4));
                totalLength = totalLength + content1.Length / 4 + 20;
                tf.DrawString(content2, font, XBrushes.Black, new XRect(50, totalLength, 500, content2.Length / 4));
                totalLength = totalLength + content2.Length / 4;                
                tf.DrawString(content3, font, XBrushes.Black, new XRect(50, totalLength, 500, content3.Length / 4));
                totalLength = totalLength + content3.Length / 4;
                signOff = _lvm.staffMember.NAME + Environment.NewLine + _lvm.staffMember.POSITION;
                cc = referrerName;                
            }

            //O3
            if (docCode == "O3")
            {
                List<Risk> _riskList = new List<Risk>();
                RiskData _rData = new RiskData(_clinContext);
                Surveillance _surv = new Surveillance();
                SurveillanceData _survData = new SurveillanceData(_clinContext);
                _riskList = _rData.GetRiskListByRefID(refID);
                
                content1 = _lvm.documentsContent.Para1;
                content2 = _lvm.documentsContent.Para3;
                foreach (var item in _riskList)
                {
                    _surv = _survData.GetSurvDetails(item.RiskID);
                    content3 = item.SurvSite + " surveillance " + " by " + item.SurvType + " " + item.SurvFreq + " from the age of " + item.SurvStartAge.ToString(); //TODO - get this to display properly
                    if(item.SurvStopAge != null)
                    {
                        content3 = content3 + " to " + item.SurvStopAge.ToString();
                    }
                }
                
                content4 = _lvm.documentsContent.Para4;

                tf.DrawString(content1, font, XBrushes.Black, new XRect(50, 400, 500, content1.Length / 4));
                totalLength = totalLength + content1.Length / 4 + 40; //because it's just exactly the wrong size to work with just /3 or /4
                tf.DrawString(content3, fontBold, XBrushes.Black, new XRect(50, totalLength, 500, content3.Length / 4));
                totalLength = totalLength + content3.Length / 4 + 10;
                tf.DrawString(content2, font, XBrushes.Black, new XRect(50, totalLength, 500, content2.Length / 4));
                totalLength = totalLength + content2.Length / 4;
                tf.DrawString(content4, font, XBrushes.Black, new XRect(50, totalLength, 500, content4.Length / 4));
                totalLength = totalLength + content4.Length / 4;
                signOff = _lvm.staffMember.NAME + Environment.NewLine + _lvm.staffMember.POSITION;
                cc = referrerName;
            }

            //O3a
            if (docCode == "O3a")
            {
                content1 = _lvm.documentsContent.Para1;
                content2 = _lvm.documentsContent.Para2;
                content3 = _lvm.documentsContent.Para3;
                content4 = _lvm.documentsContent.Para9;

                tf.DrawString(content1, font, XBrushes.Black, new XRect(50, 400, 500, content1.Length / 4));
                totalLength = totalLength + content1.Length / 4;
                tf.DrawString(content2, font, XBrushes.Black, new XRect(50, totalLength, 500, content2.Length / 4));
                totalLength = totalLength + content2.Length / 4;
                tf.DrawString(content3, font, XBrushes.Black, new XRect(50, totalLength, 500, content3.Length / 4));
                totalLength = totalLength + content3.Length / 5;
                tf.DrawString(content4, font, XBrushes.Black, new XRect(50, totalLength, 500, content4.Length / 4));
                totalLength = totalLength + content4.Length / 4;
                signOff = _lvm.staffMember.NAME + Environment.NewLine + _lvm.staffMember.POSITION;
                cc = referrerName;
            }

            //O4
            if (docCode == "O4")
            {
                content1 = _lvm.documentsContent.Para1;
                content2 = _lvm.documentsContent.Para2;
                content3 = _lvm.documentsContent.Para3;
                content4 = _lvm.documentsContent.Para4;

                tf.DrawString(content1, font, XBrushes.Black, new XRect(50, 400, 500, content1.Length / 4));
                totalLength = totalLength + content1.Length / 4;
                tf.DrawString(content2, font, XBrushes.Black, new XRect(50, totalLength, 500, content2.Length / 4));
                totalLength = totalLength + content2.Length / 4;
                tf.DrawString(content3, font, XBrushes.Black, new XRect(50, totalLength, 500, content3.Length / 4));
                totalLength = totalLength + content3.Length / 4;
                tf.DrawString(content4, font, XBrushes.Black, new XRect(50, totalLength, 500, content4.Length / 4));
                totalLength = totalLength + content4.Length / 4;
                signOff = _lvm.staffMember.NAME + Environment.NewLine + _lvm.staffMember.POSITION;
                cc = referrerName;
            }

            //Reject letter
            if (docCode == "RejectCMA")
            {
                content1 = _lvm.documentsContent.Para1 + Environment.NewLine + Environment.NewLine + _lvm.documentsContent.Para2;

                tf.DrawString(content1, font, XBrushes.Black, new XRect(50, 400, 500, content1.Length / 5));
                totalLength = totalLength + content1.Length / 5;
                if (content2 != null)
                {
                    tf.DrawString(content2, fontBold, XBrushes.Black, new XRect(50, totalLength, 500, content2.Length / 4));
                    totalLength = totalLength + content2.Length / 4;
                }
                tf.DrawString(content3, font, XBrushes.Black, new XRect(50, totalLength, 500, content3.Length / 4));
                totalLength = totalLength + content3.Length / 4;
                tf.DrawString(content4, font, XBrushes.Black, new XRect(50, totalLength, 500, content4.Length / 4));
                totalLength = totalLength + content4.Length / 4;
                signOff = _lvm.staffMember.NAME + Environment.NewLine + _lvm.staffMember.POSITION;
                cc = referrerName;
            }

            sigFlename = _lvm.staffMember.StaffForename + _lvm.staffMember.StaffSurname.Replace("'","").Replace(" ", "") + ".jpg";

            if (!System.IO.File.Exists(@"wwwroot\Signatures\" + sigFlename)) { sigFlename = "empty.jpg"; } //this only exists because we can't define the image if it's null.
            
            XImage imageSig = XImage.FromFile(@"wwwroot\Signatures\" + sigFlename);
            int len = imageSig.PixelWidth;
            int hig = imageSig.PixelHeight;

            if (pageCount == 1)
            {
                document.Pages.Remove(page2);                
                tf.DrawString("Yours sincerely", font, XBrushes.Black, new XRect(50, totalLength, 500, 20));
                totalLength = totalLength + 20;
                
                if (signOff == "CGU Booking Centre")
                {
                    totalLength = totalLength + 50;
                }
                else
                {                    
                    if (sigFlename != "empty.jpg")
                    {
                        gfx.DrawImage(imageSig, 50, totalLength, len, hig);
                    }
                    totalLength = totalLength + hig + 20;
                }
                tf.DrawString(signOff, font, XBrushes.Black, new XRect(50, totalLength, 500, 20));
            }
            else
            {
                tf2.DrawString("Yours sincerely", font, XBrushes.Black, new XRect(50, totalLength, 500, 20));
                totalLength = totalLength + 20;
                
                if (signOff == "CGU Booking Centre")
                {
                    totalLength = totalLength + 50;
                }
                else
                {
                    if (sigFlename != "empty.jpg")
                    {
                        gfx2.DrawImage(imageSig, 50, totalLength, len, hig);
                    }
                    totalLength = totalLength + hig + 20;
                }
                tf2.DrawString(signOff, font, XBrushes.Black, new XRect(50, totalLength, 500, 20));
            }
            
            //Add a page for all of the CC addresses (must be declared here or we can't use it)            
            if (cc != "")
            {
                PdfPage pageCC = document.AddPage();
                printCount = printCount += 1;
                cc = cc + _externalClinicianData.GetCCDetails(_lvm.referrer);
                XGraphics gfxcc = XGraphics.FromPdfPage(pageCC);
                var tfcc = new XTextFormatter(gfxcc);
                tfcc.DrawString("cc:", font, XBrushes.Black, new XRect(50, 50, 500, 100));
                tfcc.DrawString(cc, font, XBrushes.Black, new XRect(75, 50, 500, 100));
                if (cc2 != "")
                {
                    printCount = printCount += 1;
                    cc = cc + _externalClinicianData.GetCCDetails(_lvm.referrer);
                    XGraphics gfxcc2 = XGraphics.FromPdfPage(pageCC);
                    var tfcc2 = new XTextFormatter(gfxcc);
                    tfcc2.DrawString(cc, font, XBrushes.Black, new XRect(75, 150, 500, 100));
                }
            }
            //Finally we set the filename for the output PDF
            //needs to be: "CaStdLetter"-CGU number-DocCode-Patient/relative ID (usually "[MPI]-0")-RefID-"print count (if CCs present)"-date/time stamp-Diary ID
            string fileCGU = _lvm.patient.CGU_No.Replace(".", "-");
            string mpiString = _lvm.patient.MPI.ToString();
            string refIDString = refID.ToString();
            string dateTimeString = DateTime.Now.ToString("yyyyMMddHHmmss");
            string diaryIDString = diaryID.ToString();

            /*
            var par = _docContext.Constants.FirstOrDefault(p => p.ConstantCode == "FilePathEDMS");
            string filePath = par.ConstantValue;

            //EDMS flename - we have to strip out the spaces that keep inserting themselves into the backend data!
            //Also, we only have a constant value for the OPEX scanner, not the letters folder!
            string letterFileName = filePath.Replace(" ", "") + "\\CaStdLetter-" + fileCGU + "-" + docCode + "-" + mpiString + "-0-" + refIDString + "-" + printCount.ToString() + "-" + dateTimeString + "-" + diaryIDString;
            letterFileName = letterFileName.Replace("ScannerOPEX2", "Letters");
            */
            //document.Save(letterFileName + ".pdf"); - the server can't save it to the watchfolder due to permission issues.
            //So we have to create it locally and have a scheduled job to move it instead.
            document.Save($@"C:\CGU_DB\Letters\CaStdLetter-{fileCGU}-{docCode}-{mpiString}-0-{refIDString}-{printCount.ToString()}-{dateTimeString}-{diaryIDString}.pdf");
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

    
