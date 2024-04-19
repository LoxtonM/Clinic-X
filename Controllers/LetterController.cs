using Microsoft.AspNetCore.Mvc;
using ClinicX.ViewModels;
using ClinicX.Data;
using ClinicX.Meta;
using PdfSharpCore.Pdf;
using PdfSharpCore.Drawing;
using PdfSharpCore.Drawing.Layout;
using System;
using System.IO;
using Microsoft.EntityFrameworkCore;


namespace ClinicX.Controllers;

public class LetterController : Controller
{
    private readonly ClinicalContext _clinContext;
    private readonly DocumentContext _docContext;
    private readonly LetterVM _lvm;
    private readonly VMData _vmDoc;
    private readonly VMData _vmClin;

    public LetterController(ClinicalContext clinContext, DocumentContext docContext)
    {
        _clinContext = clinContext;
        _docContext = docContext;
        _lvm = new LetterVM();
        _vmClin = new VMData(_clinContext);
        _vmDoc = new VMData(_docContext);
    }

    public async Task<IActionResult> Letter(int id, int mpi, string user, string referrer)
    {
        try
        {
            
            _lvm.staffMember = _vmClin.GetStaffMemberDetails(user);
            _lvm.patient = _vmClin.GetPatientDetails(mpi);
            _lvm.documentsContent = _vmDoc.GetDocumentDetails(id);
            _lvm.referrer = _vmClin.GetClinicianDetails(referrer);

            return View(_lvm);
        }
        catch (Exception ex)
        {
            return RedirectToAction("ErrorHome", "Error", new { error = ex.Message });
        }
    }

    public void PreviewDOTPDF(int dID,string user)
    {
        try
        {
            _lvm.staffMember = _vmClin.GetStaffMemberDetails(user);
            _lvm.dictatedLetter = _vmClin.GetDictatedLetterDetails(dID);                        
            string ourAddress = _docContext.DocumentsContent.FirstOrDefault(d => d.OurAddress != null).OurAddress;            
            //creates a new PDF document
            PdfDocument document = new PdfDocument();
            document.Info.Title = "My PDF";
            PdfPage page = document.AddPage();
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
            tf.DrawString(_vmDoc.GetConstant("MainCGUEmail", 1), font, XBrushes.Black, new XRect(450, 250, page.Width, 200));

            tf.Alignment = XParagraphAlignment.Left;
            tf.DrawString($"Consultant: {_lvm.dictatedLetter.Consultant}", fontBold, XBrushes.Black, new XRect(20, 150, page.Width/2, 200));
            tf.DrawString($"Genetic Counsellor: {_lvm.dictatedLetter.GeneticCounsellor}", fontBold, XBrushes.Black, new XRect(20, 165, page.Width/2, 200));

            //Note: Xrect parameters are: (Xpos, Ypos, Width, Depth) - use to position blocks of text
            //Depth of 10 seems sufficient for one line of text; 30 is sufficient for two lines. 7 lines needs 100.

            string phoneNumbers = "Secretaries Direct Line:" + System.Environment.NewLine;

            var secretariesList = _vmClin.GetStaffMemberList().Where(s => s.BILL_ID == _lvm.dictatedLetter.SecTeam);
            foreach (var t in secretariesList)
            {
                phoneNumbers = phoneNumbers + $"{t.NAME} {t.TELEPHONE}" + System.Environment.NewLine;
            }
            
            tf.DrawString(phoneNumbers, font, XBrushes.Black, new XRect(20, 200, page.Width/2, 200));

            string datesInfo = $"Dictated Date: {_lvm.dictatedLetter.DateDictated.Value.ToString("dd/MM/yyyy")}" + System.Environment.NewLine +
                               $"Date Typed: {_lvm.dictatedLetter.CreatedDate.Value.ToString("dd/MM/yyyy")}";
                       
            _lvm.patient = _vmClin.GetPatientDetails(_lvm.dictatedLetter.MPI);
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
            
            
            if (_lvm.dictatedLetter.LetterRe != null)
            {
                tf.DrawString($"Re  {letterRe}", fontBold, XBrushes.Black, new XRect(20, totalLength, 500, letterRe.Length));
            }
            totalLength = totalLength + letterRe.Length / 4 + 20;
            tf.DrawString(summary, fontBold, XBrushes.Black, new XRect(20, totalLength, 500, summary.Length));
            totalLength = totalLength + summary.Length / 4 + 20;
            tf.DrawString(content, font, XBrushes.Black, new XRect(20, totalLength, 500, content.Length));
            totalLength = totalLength + content.Length+20;
            
            signOff = _lvm.staffMember.NAME + Environment.NewLine + _lvm.staffMember.POSITION;
            sigFilename = $"{_lvm.staffMember.StaffForename}{_lvm.staffMember.StaffSurname}.jpg";
            totalLength = totalLength + 20;
            if (System.IO.File.Exists(@$"wwwroot\Signatures\{sigFilename}"))
            {
                XImage imageSig = XImage.FromFile(@$"wwwroot\Signatures\{sigFilename}");

                int len = imageSig.PixelWidth;
                int hig = imageSig.PixelHeight;
                tf.DrawString("Yours sincerely,", font, XBrushes.Black, new XRect(20, totalLength, 500, 20));
                totalLength = totalLength + 20;
                gfx.DrawImage(imageSig, 50, totalLength, len, hig);
                totalLength = totalLength + hig + 20;
            }
            
            tf.DrawString(signOff, font, XBrushes.Black, new XRect(20, totalLength, 500, 20));

            document.Save($"c:\\projects\\VS Test\\ClinicX\\wwwroot\\preview-{user}.pdf");
        }

        catch (Exception ex)
        {
            RedirectToAction("ErrorHome", "Error", new { error = ex.Message });
        }
    }

    public void DoPDF(int id, int mpi, int refID, string user, string referrer, string? additionalText = "")
    {
        try
        {            
            _lvm.staffMember = _vmClin.GetStaffMemberDetails(user);
            _lvm.patient = _vmClin.GetPatientDetails(mpi);
            _lvm.documentsContent = _vmDoc.GetDocumentDetails(id);
            _lvm.referrer = _vmClin.GetClinicianDetails(referrer);


            //creates a new PDF document
            PdfDocument document = new PdfDocument();
            document.Info.Title = "My PDF";
            PdfPage page = document.AddPage();
            XGraphics gfx = XGraphics.FromPdfPage(page);
            var tf = new XTextFormatter(gfx);
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

            if (_lvm.documentsContent.LetterTo == "PT")
            {
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
                //placeholder
            }

            tf.DrawString(address, font, XBrushes.Black, new XRect(50, 250, 490, 100));

            //Date letter created
            tf.DrawString(DateTime.Today.ToString("dd MMMM yyyy"), font, XBrushes.Black, new XRect(50, 350, 500, 10)); //today's date

            tf.DrawString("Dear " + _lvm.patient.SALUTATION, font, XBrushes.Black, new XRect(50, 375, 500, 10)); //salutation

            //Content containers for all of the paragraphs, as well as other data required
            string content1 = "";
            string content2 = "";
            string content3 = "";
            string content4 = "";
            string content5 = "";
            string quoteRef = "";
            string signOff = "";
            string sigFlename = "";
            string docCode = _lvm.documentsContent.DocCode;
            string referrerName = _lvm.referrer.TITLE + " " + _lvm.referrer.FIRST_NAME + " " + _lvm.referrer.NAME;
            string cc = "";
            string cc2 = "";
            int printCount = 1;
            int totalLength = 400; //used for spacing - so the paragraphs can dynamically resize

            ///////////////////////////////////////////////////////////////////////////////////////
            //////All letter templates need to be defined individually here////////////////////////
            //////Since some of them go over two pages, and this function requires/////////////////
            //////each page to be defined, we can't use a separate function for any of this!///////
            ///////////////////////////////////////////////////////////////////////////////////////
            ///
            //Ack letter
            if (docCode == "Ack")
            {
                quoteRef = "Please quote this reference on all correspondence: " + _lvm.patient.CGU_No + Environment.NewLine;
                quoteRef = quoteRef + "NHS number: " + _lvm.patient.SOCIAL_SECURITY;

                tf.Alignment = XParagraphAlignment.Right;
                tf.DrawString(quoteRef, font, XBrushes.Black, new XRect(-20, 300, page.Width, 200)); //"Please quote CGU number" etc

                tf.Alignment = XParagraphAlignment.Left;
                content1 = _lvm.documentsContent.Para1;
                tf.DrawString(content1, font, XBrushes.Black, new XRect(50, totalLength, 500, content1.Length / 4));
                totalLength = totalLength + content1.Length / 4 - 40;
                signOff = _lvm.staffMember.NAME + Environment.NewLine + _lvm.staffMember.POSITION;
                sigFlename = _lvm.staffMember.StaffForename + _lvm.staffMember.StaffSurname + ".jpg";
                totalLength = totalLength + 20;
                XImage imageSig = XImage.FromFile(@"wwwroot\Signatures\" + sigFlename);
                int len = imageSig.PixelWidth;
                int hig = imageSig.PixelHeight;
                gfx.DrawImage(imageSig, 50, totalLength, len, hig);
                totalLength = totalLength + hig + 20;
                tf.DrawString(signOff, font, XBrushes.Black, new XRect(50, totalLength, 500, 20));
                cc = referrerName;
            }

            //CTB Ack letter
            if (docCode == "CTBAck") //Referrer name must be included here
            {
                quoteRef = "Please quote this reference on all correspondence: " + _lvm.patient.CGU_No + Environment.NewLine;
                quoteRef = quoteRef + "NHS number: " + _lvm.patient.SOCIAL_SECURITY + Environment.NewLine;
                quoteRef = quoteRef + "Consultant: " + Environment.NewLine;
                quoteRef = quoteRef + "Genetic Counsellor: ";
                tf.DrawString(quoteRef, font, XBrushes.Black, new XRect(50, 150, page.Width, 150)); //"Please quote CGU number" etc

                content1 = referrerName + " " + _lvm.documentsContent.Para1;
                content2 = _lvm.documentsContent.Para2;
                content3 = _lvm.documentsContent.Para3 + Environment.NewLine + Environment.NewLine + _lvm.documentsContent.Para4;

                tf.DrawString(content1, font, XBrushes.Black, new XRect(50, totalLength, 500, content1.Length / 4));
                totalLength = totalLength + content1.Length / 4 + 10;
                tf.DrawString(content2, fontBold, XBrushes.Black, new XRect(50, totalLength, 500, content2.Length / 4));
                totalLength = totalLength + content2.Length / 4 + 10;
                tf.DrawString(content3, font, XBrushes.Black, new XRect(50, totalLength, 500, content3.Length / 3));
                totalLength = totalLength + content3.Length / 3;
                tf.DrawString("Yours sincerely", font, XBrushes.Black, new XRect(50, totalLength, 500, 20));
                signOff = "CGU Booking Centre";
                totalLength = totalLength + 80;
                tf.DrawString(signOff, font, XBrushes.Black, new XRect(50, totalLength, 500, 20));
                cc = referrerName;
            }

            //KC letter
            if (docCode == "Kc") //Paragraph 1 sometimes needs the referrer name
            {
                quoteRef = "Please quote this reference on all correspondence: " + _lvm.patient.CGU_No + Environment.NewLine;
                quoteRef = quoteRef + "NHS number: " + _lvm.patient.SOCIAL_SECURITY;

                tf.Alignment = XParagraphAlignment.Right;
                tf.DrawString(quoteRef, font, XBrushes.Black, new XRect(-20, 300, page.Width, 200)); //"Please quote CGU number" etc

                tf.Alignment = XParagraphAlignment.Left;
                content1 = _lvm.documentsContent.Para1 + " " + referrerName + " " + _lvm.documentsContent.Para2 +
                    Environment.NewLine + Environment.NewLine + _lvm.documentsContent.Para3 +
                    Environment.NewLine + Environment.NewLine + _lvm.documentsContent.Para4;
                content2 = _lvm.documentsContent.Para5;
                tf.DrawString(content1, font, XBrushes.Black, new XRect(50, totalLength, 500, content1.Length / 4));
                totalLength = totalLength + content1.Length / 4 - 40;
                PdfPage page2 = document.AddPage();
                XGraphics gfx2 = XGraphics.FromPdfPage(page2);
                var tf2 = new XTextFormatter(gfx2);
                tf2.DrawString(content2, font, XBrushes.Black, new XRect(50, 50, 500, content2.Length / 4));
                totalLength = 50 + content2.Length / 4;
                tf2.DrawString("Yours sincerely", font, XBrushes.Black, new XRect(50, totalLength, 500, 20));
                signOff = _lvm.staffMember.NAME + Environment.NewLine + _lvm.staffMember.POSITION;
                sigFlename = _lvm.staffMember.StaffForename + _lvm.staffMember.StaffSurname + ".jpg";
                totalLength = totalLength + 20;
                XImage imageSig = XImage.FromFile(@"wwwroot\Signatures\" + sigFlename);
                int len = imageSig.PixelWidth;
                int hig = imageSig.PixelHeight;
                gfx2.DrawImage(imageSig, 50, totalLength, len, hig);
                totalLength = totalLength + hig + 20;
                tf2.DrawString(signOff, font, XBrushes.Black, new XRect(50, totalLength, 500, 20));
            }

            //O1 letter
            if (docCode == "O1")
            {
                quoteRef = "Please quote this reference on all correspondence: " + _lvm.patient.CGU_No + Environment.NewLine;
                quoteRef = quoteRef + "NHS number: " + _lvm.patient.SOCIAL_SECURITY + Environment.NewLine;
                quoteRef = quoteRef + "Consultant: " + Environment.NewLine;
                quoteRef = quoteRef + "Genetic Counsellor: ";
                tf.DrawString(quoteRef, font, XBrushes.Black, new XRect(50, 150, page.Width, 150)); //"Please quote CGU number" etc

                content1 = _lvm.documentsContent.Para1 + Environment.NewLine + Environment.NewLine + _lvm.documentsContent.Para2;
                content2 = additionalText;
                content3 = _lvm.documentsContent.Para4;
                content4 = _lvm.documentsContent.Para7;
                content5 = _lvm.documentsContent.Para9;

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
                sigFlename = _lvm.staffMember.StaffForename + _lvm.staffMember.StaffSurname + ".jpg";
                totalLength = totalLength + 20;
                XImage imageSig = XImage.FromFile(@"wwwroot\Signatures\" + sigFlename);
                int len = imageSig.PixelWidth;
                int hig = imageSig.PixelHeight;
                gfx.DrawImage(imageSig, 50, totalLength, len, hig);
                totalLength = totalLength + hig + 20;
                tf.DrawString(signOff, font, XBrushes.Black, new XRect(50, totalLength, 500, 20));
                cc = referrerName;
            }

                //Reject letter
            if (docCode == "RejectCMA")
            {
                quoteRef = "Please quote this reference on all correspondence: " + _lvm.patient.CGU_No + Environment.NewLine;
                quoteRef = quoteRef + "NHS number: " + _lvm.patient.SOCIAL_SECURITY + Environment.NewLine;
                quoteRef = quoteRef + "Consultant: " + Environment.NewLine;
                quoteRef = quoteRef + "Genetic Counsellor: ";
                tf.DrawString(quoteRef, font, XBrushes.Black, new XRect(50, 150, page.Width, 150)); //"Please quote CGU number" etc

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
                sigFlename = _lvm.staffMember.StaffForename + _lvm.staffMember.StaffSurname + ".jpg";
                totalLength = totalLength + 20;
                XImage imageSig = XImage.FromFile(@"wwwroot\Signatures\" + sigFlename);
                int len = imageSig.PixelWidth;
                int hig = imageSig.PixelHeight;
                gfx.DrawImage(imageSig, 50, totalLength, len, hig);
                totalLength = totalLength + hig + 20;
                tf.DrawString(signOff, font, XBrushes.Black, new XRect(50, totalLength, 500, 20));
                cc = referrerName;

            }

            //Add a page for all of the CC addresses (must be declared here or we can't use it)
            PdfPage pageCC = document.AddPage();

            if (cc != "")
            {
                printCount = printCount += 1;
                cc = cc + _vmClin.GetCCDetails(_lvm.referrer);
                XGraphics gfxcc = XGraphics.FromPdfPage(pageCC);
                var tfcc = new XTextFormatter(gfxcc);
                tfcc.DrawString("cc:", font, XBrushes.Black, new XRect(50, 50, 500, 100));
                tfcc.DrawString(cc, font, XBrushes.Black, new XRect(75, 50, 500, 100));
            }

            if (cc2 != "")
            {
                printCount = printCount += 1;
                cc = cc + _vmClin.GetCCDetails(_lvm.referrer);
                XGraphics gfxcc = XGraphics.FromPdfPage(pageCC);
                var tfcc = new XTextFormatter(gfxcc);
                tfcc.DrawString(cc, font, XBrushes.Black, new XRect(75, 150, 500, 100));
            }

            //Finally we set the flename for the output PDF
            //needs to be: "CaStdLetter"-CGU number-DocCode-Patient/relative ID (usually "[MPI]-0")-RefID-"print count (if CCs present)"-date/time stamp-Diary ID
            string fileCGU = _lvm.patient.CGU_No.Replace(".", "_");
            string mpiString = _lvm.patient.MPI.ToString();
            string refIDString = refID.ToString();
            string dateTimeString = DateTime.Now.ToString("yyyyMMddHHmmss");
            string diaryIDString = "00000"; //need to create diary entry first

            var par = _docContext.Constants.FirstOrDefault(p => p.ConstantCode == "FilePathEDMS");
            string filePath = par.ConstantValue;


            //EDMS flename - we have to strip out the spaces that keep inserting themselves into the backend data!
            //Also, we only have a constant value for the OPEX scanner, not the letters folder, for some reason!
            string letterFileName = filePath.Replace(" ", "") + "\\CaStdLetter-" + fileCGU + "-" + docCode + "-" + mpiString + "-0-" + refIDString + "-" + printCount.ToString() + "-" + dateTimeString + "-" + diaryIDString;
            letterFileName = letterFileName.Replace("ScannerOPEX2", "Letters");

            document.Save(letterFileName + ".pdf");
            
        }
        catch (Exception ex)
        {
            RedirectToAction("ErrorHome", "Error", new { error = ex.Message });
        }
    }

    

}

    
