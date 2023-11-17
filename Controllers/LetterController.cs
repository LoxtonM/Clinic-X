using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using ClinicX.Models;
using ClinicX.ViewModels;
using ClinicX.Data;
using ClinicX.Meta;
using PdfSharpCore.Pdf;
using PdfSharpCore.Drawing;
using PdfSharpCore.Drawing.Layout;


namespace ClinicX.Controllers;

public class LetterController : Controller
{
    private readonly DocumentContext _context;

    public LetterController(DocumentContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Letter(int id, int impi, string suser, string sref)
    {
        LetterVM lvm = new LetterVM();
        VMData vm = new VMData(_context);
        lvm.staffMember = vm.GetStaffMember(suser);
        lvm.patient = vm.GetPatient(impi);
        lvm.documentsContent = vm.GetDocument(id);
        lvm.referrer = vm.GetReferrer(sref);

        return View(lvm);
    }

    

    public void DoPDF(int id, int impi, int irefid, string suser, string sref, string? sAdditionalText = "")
    {
        LetterVM lvm = new LetterVM();
        VMData vm = new VMData(_context);
        lvm.staffMember = vm.GetStaffMember(suser);
        lvm.patient = vm.GetPatient(impi);
        lvm.documentsContent = vm.GetDocument(id);
        lvm.referrer = vm.GetReferrer(sref);
        

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
        tf.DrawString(lvm.documentsContent.OurAddress, font, XBrushes.Black, new XRect(-20, 150, page.Width, 200));
        if (lvm.documentsContent.DirectLine != null) //because we have to trap them nulls!
        {
            tf.DrawString(lvm.documentsContent.DirectLine, fontBold, XBrushes.Black, new XRect(-20, 250, page.Width, 10));
        }
        tf.DrawString(lvm.documentsContent.OurEmailAddress, font, XBrushes.Black, new XRect(-20, 270, page.Width, 10));

        //Note: Xrect parameters are: (Xpos, Ypos, Width, Depth) - use to position blocks of text
        //Depth of 10 seems sufficient for one line of text; 30 is sufficient for two lines. 7 lines needs 100.

        //patient's address
        tf.Alignment = XParagraphAlignment.Left;
        tf.DrawString(lvm.patient.PtLetterAddressee, font, XBrushes.Black, new XRect(50, 235, 500, 10));

        string strAddress = lvm.patient.ADDRESS1 + Environment.NewLine;
        if (lvm.patient.ADDRESS2 != null) //this is sometimes null
        {
            strAddress = strAddress + lvm.patient.ADDRESS2 + Environment.NewLine;
        }
        strAddress = strAddress + lvm.patient.ADDRESS3 + Environment.NewLine;
        strAddress = strAddress + lvm.patient.ADDRESS4 + Environment.NewLine;
        strAddress = strAddress + lvm.patient.POSTCODE;

        tf.DrawString(strAddress, font, XBrushes.Black, new XRect(50, 250, 490, 100));

        //Date letter created
        tf.DrawString(DateTime.Today.ToString("dd MMMM yyyy"), font, XBrushes.Black, new XRect(50, 350, 500, 10)); //today's date

        tf.DrawString("Dear " + lvm.patient.SALUTATION, font, XBrushes.Black, new XRect(50, 375, 500, 10)); //salutation

        //Content containers for all of the paragraphs, as well as other data required
        string strContent1 = "";
        string strContent2 = "";
        string strContent3 = "";
        string strContent4 = "";
        string strContent5 = "";
        string strQuoteRef = "";
        string strSignoff = "";
        string strSigFilename = "";
        string strDocCode = lvm.documentsContent.DocCode;
        string strReferrer = lvm.referrer.TITLE + " " + lvm.referrer.FIRST_NAME + " " + lvm.referrer.NAME;
        string strCC = "";
        string strCC2 = "";
        int iPrintCount = 1;
        int iTotalLength = 400; //used for spacing - so the paragraphs can dynamically resize

        ///////////////////////////////////////////////////////////////////////////////////////
        //////All letter templates need to be defined individually here////////////////////////
        //////Since some of them go over two pages, and this function requires/////////////////
        //////each page to be defined, we can't use a separate function for any of this!///////
        ///////////////////////////////////////////////////////////////////////////////////////
        ///
        //Ack letter
        if (strDocCode == "Ack")
        {
            strQuoteRef = "Please quote this reference on all correspondence: " + lvm.patient.CGU_No + Environment.NewLine;
            strQuoteRef = strQuoteRef + "NHS number: " + lvm.patient.SOCIAL_SECURITY;

            tf.Alignment = XParagraphAlignment.Right;
            tf.DrawString(strQuoteRef, font, XBrushes.Black, new XRect(-20, 300, page.Width, 200)); //"Please quote CGU number" etc

            tf.Alignment = XParagraphAlignment.Left;
            strContent1 = lvm.documentsContent.Para1;
            tf.DrawString(strContent1, font, XBrushes.Black, new XRect(50, iTotalLength, 500, strContent1.Length / 4));
            iTotalLength = iTotalLength + strContent1.Length / 4 - 40;
            strSignoff = lvm.staffMember.NAME + Environment.NewLine + lvm.staffMember.POSITION;
            strSigFilename = lvm.staffMember.StaffForename + lvm.staffMember.StaffSurname + ".jpg";
            iTotalLength = iTotalLength + 20;
            XImage imageSig = XImage.FromFile(@"wwwroot\Signatures\" + strSigFilename);
            int iLen = imageSig.PixelWidth;
            int iHig = imageSig.PixelHeight;
            gfx.DrawImage(imageSig, 50, iTotalLength, iLen, iHig);
            iTotalLength = iTotalLength + iHig + 20;
            tf.DrawString(strSignoff, font, XBrushes.Black, new XRect(50, iTotalLength, 500, 20));
            strCC = strReferrer;
        }

        //CTB Ack letter
        if (strDocCode == "CTBAck") //Referrer name must be included here
        {
            strQuoteRef = "Please quote this reference on all correspondence: " + lvm.patient.CGU_No + Environment.NewLine;
            strQuoteRef = strQuoteRef + "NHS number: " + lvm.patient.SOCIAL_SECURITY + Environment.NewLine;
            strQuoteRef = strQuoteRef + "Consultant: " + Environment.NewLine;
            strQuoteRef = strQuoteRef + "Genetic Counsellor: ";
            tf.DrawString(strQuoteRef, font, XBrushes.Black, new XRect(50, 150, page.Width, 150)); //"Please quote CGU number" etc

            strContent1 = strReferrer + " " + lvm.documentsContent.Para1;
            strContent2 = lvm.documentsContent.Para2;
            strContent3 = lvm.documentsContent.Para3 + Environment.NewLine + Environment.NewLine + lvm.documentsContent.Para4;

            tf.DrawString(strContent1, font, XBrushes.Black, new XRect(50, iTotalLength, 500, strContent1.Length / 4));
            iTotalLength = iTotalLength + strContent1.Length / 4 + 10;
            tf.DrawString(strContent2, fontBold, XBrushes.Black, new XRect(50, iTotalLength, 500, strContent2.Length / 4));
            iTotalLength = iTotalLength + strContent2.Length / 4 + 10;
            tf.DrawString(strContent3, font, XBrushes.Black, new XRect(50, iTotalLength, 500, strContent3.Length / 3));
            iTotalLength = iTotalLength + strContent3.Length / 3;
            tf.DrawString("Yours sincerely", font, XBrushes.Black, new XRect(50, iTotalLength, 500, 20));
            strSignoff = "CGU Booking Centre";
            iTotalLength = iTotalLength + 80;
            tf.DrawString(strSignoff, font, XBrushes.Black, new XRect(50, iTotalLength, 500, 20));
            strCC = strReferrer;            
        }

        //KC letter
        if (strDocCode == "Kc") //Paragraph 1 sometimes needs the referrer name
        {
            strQuoteRef = "Please quote this reference on all correspondence: " + lvm.patient.CGU_No + Environment.NewLine;
            strQuoteRef = strQuoteRef + "NHS number: " + lvm.patient.SOCIAL_SECURITY;

            tf.Alignment = XParagraphAlignment.Right;
            tf.DrawString(strQuoteRef, font, XBrushes.Black, new XRect(-20, 300, page.Width, 200)); //"Please quote CGU number" etc

            tf.Alignment = XParagraphAlignment.Left;
            strContent1 = lvm.documentsContent.Para1 + " " + strReferrer + " " + lvm.documentsContent.Para2 +
                Environment.NewLine + Environment.NewLine + lvm.documentsContent.Para3 +
                Environment.NewLine + Environment.NewLine + lvm.documentsContent.Para4;
            strContent2 = lvm.documentsContent.Para5;
            tf.DrawString(strContent1, font, XBrushes.Black, new XRect(50, iTotalLength, 500, strContent1.Length / 4));
            iTotalLength = iTotalLength + strContent1.Length / 4 - 40;
            PdfPage page2 = document.AddPage();
            XGraphics gfx2 = XGraphics.FromPdfPage(page2);
            var tf2 = new XTextFormatter(gfx2);
            tf2.DrawString(strContent2, font, XBrushes.Black, new XRect(50, 50, 500, strContent2.Length / 4));
            iTotalLength = 50 + strContent2.Length / 4;
            tf2.DrawString("Yours sincerely", font, XBrushes.Black, new XRect(50, iTotalLength, 500, 20));
            strSignoff = lvm.staffMember.NAME + Environment.NewLine + lvm.staffMember.POSITION;
            strSigFilename = lvm.staffMember.StaffForename + lvm.staffMember.StaffSurname + ".jpg";
            iTotalLength = iTotalLength + 20;
            XImage imageSig = XImage.FromFile(@"wwwroot\Signatures\" + strSigFilename);
            int iLen = imageSig.PixelWidth;
            int iHig = imageSig.PixelHeight;
            gfx2.DrawImage(imageSig, 50, iTotalLength, iLen, iHig);
            iTotalLength = iTotalLength + iHig + 20;
            tf2.DrawString(strSignoff, font, XBrushes.Black, new XRect(50, iTotalLength, 500, 20));
        }

        //O1 letter
        if (strDocCode == "O1")
        {
            strQuoteRef = "Please quote this reference on all correspondence: " + lvm.patient.CGU_No + Environment.NewLine;
            strQuoteRef = strQuoteRef + "NHS number: " + lvm.patient.SOCIAL_SECURITY + Environment.NewLine;
            strQuoteRef = strQuoteRef + "Consultant: " + Environment.NewLine;
            strQuoteRef = strQuoteRef + "Genetic Counsellor: ";
            tf.DrawString(strQuoteRef, font, XBrushes.Black, new XRect(50, 150, page.Width, 150)); //"Please quote CGU number" etc

            strContent1 = lvm.documentsContent.Para1 + Environment.NewLine + Environment.NewLine + lvm.documentsContent.Para2;
            strContent2 = sAdditionalText;
            strContent3 = lvm.documentsContent.Para4;
            strContent4 = lvm.documentsContent.Para7;
            strContent5 = lvm.documentsContent.Para9;

            tf.DrawString(strContent1, font, XBrushes.Black, new XRect(50, 400, 500, strContent1.Length / 5));
            iTotalLength = iTotalLength + strContent1.Length / 5;
            if (strContent2 != null)
            {
                tf.DrawString(strContent2, fontBold, XBrushes.Black, new XRect(50, iTotalLength, 500, strContent2.Length / 4));
                iTotalLength = iTotalLength + strContent2.Length / 4;
            }
            tf.DrawString(strContent3, font, XBrushes.Black, new XRect(50, iTotalLength, 500, strContent3.Length / 4));
            iTotalLength = iTotalLength + strContent3.Length / 4;
            tf.DrawString(strContent4, font, XBrushes.Black, new XRect(50, iTotalLength, 500, strContent4.Length / 4));
            iTotalLength = iTotalLength + strContent4.Length / 4;

            strSignoff = lvm.staffMember.NAME + Environment.NewLine + lvm.staffMember.POSITION;
            strSigFilename = lvm.staffMember.StaffForename + lvm.staffMember.StaffSurname + ".jpg";
            iTotalLength = iTotalLength + 20;
            XImage imageSig = XImage.FromFile(@"wwwroot\Signatures\" + strSigFilename);
            int iLen = imageSig.PixelWidth;
            int iHig = imageSig.PixelHeight;
            gfx.DrawImage(imageSig, 50, iTotalLength, iLen, iHig);
            iTotalLength = iTotalLength + iHig + 20;
            tf.DrawString(strSignoff, font, XBrushes.Black, new XRect(50, iTotalLength, 500, 20));
            strCC = strReferrer;
        }

        //Add a page for all of the CC addresses (must be declared here or we can't use it)
        PdfPage pageCC = document.AddPage();

        if (strCC != "")
        {
            iPrintCount = iPrintCount += 1;
            strCC = strCC + vm.GetCC(lvm.referrer);
            XGraphics gfxcc = XGraphics.FromPdfPage(pageCC);
            var tfcc = new XTextFormatter(gfxcc);
            tfcc.DrawString("cc:", font, XBrushes.Black, new XRect(50, 50, 500, 100));
            tfcc.DrawString(strCC, font, XBrushes.Black, new XRect(75, 50, 500, 100));
        }

        if (strCC2 != "")
        {
            iPrintCount = iPrintCount += 1;
            strCC = strCC + vm.GetCC(lvm.referrer);
            XGraphics gfxcc = XGraphics.FromPdfPage(pageCC);
            var tfcc = new XTextFormatter(gfxcc);
            tfcc.DrawString(strCC, font, XBrushes.Black, new XRect(75, 150, 500, 100));
        }

        //Finally we set the filename for the output PDF
        //needs to be: "CaStdLetter"-CGU number-DocCode-Patient/relative ID (usually "[MPI]-0")-RefID-"print count (if CCs present)"-date/time stamp-Diary ID
        string strFileCGU = lvm.patient.CGU_No.Replace(".", "_");
        string strMPI = lvm.patient.MPI.ToString();
        string strRefID = irefid.ToString();
        string strDateTime = DateTime.Now.ToString("yyyyMMddHHmmss");
        string strDiaryID = "00000"; //need to create diary entry first

        var par = _context.Constants.FirstOrDefault(p => p.ConstantCode == "FilePathEDMS");
        string strFilePath = par.ConstantValue;


        //EDMS filename - we have to strip out the spaces that keep inserting themselves into the backend data!
        //Also, we only have a constant value for the OPEX scanner, not the letters folder, for some reason!
        string strLetterFilename = strFilePath.Replace(" ", "") + "\\CaStdLetter-" + strFileCGU + "-" + strDocCode + "-" + strMPI + "-0-" + strRefID + "-" + iPrintCount.ToString() + "-" + strDateTime + "-" + strDiaryID;
        strLetterFilename = strLetterFilename.Replace("ScannerOPEX2", "Letters");

        document.Save(strLetterFilename + ".pdf");
    }

    

}

    
