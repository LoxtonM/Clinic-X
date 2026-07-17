using ClinicalXPDataConnections.Meta;
using ClinicalXPDataConnections.Models;
using PdfSharpCore.Drawing;
using PdfSharpCore.Drawing.Layout;


namespace ClinicX.Controllers
{
    public class ConsentFormController
    {
        private readonly IPatientDataAsync _patientData;
        private readonly IStaffUserDataAsync _staffUserData;

        public ConsentFormController(IConfiguration config, IPatientDataAsync patientData, IStaffUserDataAsync staffUserData)
        {            
            _patientData = patientData;            
            _staffUserData = staffUserData;
        }

        public async Task<bool> CreateConsentForm(int mpi, string user, bool? isPreview = false)
        {
            StaffMember staffMember = await _staffUserData.GetStaffMemberDetails(user);
            var patient = await _patientData.GetPatientDetails(mpi);

            PdfSharpCore.Pdf.PdfDocument document = new PdfSharpCore.Pdf.PdfDocument();
            document.Info.Title = "My PDF";
            PdfSharpCore.Pdf.PdfPage page = document.AddPage();
            XGraphics gfx = XGraphics.FromPdfPage(page);
            var tf = new XTextFormatter(gfx);

            XFont font = new XFont("Arial", 11, XFontStyle.Regular);
            XFont fontSmall = new XFont("Arial", 10, XFontStyle.Regular);
            XFont fontBold = new XFont("Arial", 11, XFontStyle.Bold);
            XFont fontUnderlined = new XFont("Arial", 11, XFontStyle.Underline);
            XFont fontSmallBold = new XFont("Arial", 10, XFontStyle.Bold);
            XFont fontSmallUnderlined = new XFont("Arial", 8, XFontStyle.Underline);
            XFont fontLarge = new XFont("Arial", 16, XFontStyle.Regular);
            XFont fontLargeBold = new XFont("Arial", 16, XFontStyle.Bold);
            XFont symbols = new XFont("Segoe UI Symbol", 12, XFontStyle.Bold);
            string tick = "\u2713";

            //measures etc
            int totalLength = 35;
            int pageEdge = 12;
            int pageWidth = 570;

            ///////////////////////////////
            ///logic to do actual form
            //////////////////////////////
            ///Patient demographics
            /////////////////////////////
            XImage image = XImage.FromFile(@"wwwroot\Letterhead.jpg");
            gfx.DrawImage(image, 400, 20, image.PixelWidth / 3, image.PixelHeight / 3);

            gfx.DrawRectangle(XBrushes.Black, new XRect(pageEdge + 30, totalLength, 110, 16));
            gfx.DrawRectangle(XBrushes.Black, new XRect(pageEdge + 140, totalLength, 230, 16));
            totalLength += 1;
            gfx.DrawRectangle(XBrushes.White, new XRect(pageEdge + 31, totalLength, 108, 14));
            gfx.DrawRectangle(XBrushes.White, new XRect(pageEdge + 141, totalLength, 228, 14));
            totalLength += 2;
            tf.DrawString("Patient Name:", font, XBrushes.Black, new XRect(pageEdge + 32, totalLength, page.Width, 50));
            tf.DrawString(patient.FIRSTNAME + " " + patient.LASTNAME, font, XBrushes.Black, new XRect(pageEdge + 142, totalLength, page.Width, 50));
            totalLength += 12;
            gfx.DrawRectangle(XBrushes.Black, new XRect(pageEdge + 30, totalLength, 110, 16));
            gfx.DrawRectangle(XBrushes.Black, new XRect(pageEdge + 140, totalLength, 230, 16));
            totalLength += 1;
            gfx.DrawRectangle(XBrushes.White, new XRect(pageEdge + 31, totalLength, 108, 14));
            gfx.DrawRectangle(XBrushes.White, new XRect(pageEdge + 141, totalLength, 228, 14));
            totalLength += 2;
            tf.DrawString("DOB:", font, XBrushes.Black, new XRect(pageEdge + 32, totalLength, page.Width, 50));
            tf.DrawString(patient.DOB.Value.ToString("dd/MM/yyyy"), font, XBrushes.Black, new XRect(pageEdge + 142, totalLength, page.Width, 50));
            totalLength += 12;
            gfx.DrawRectangle(XBrushes.Black, new XRect(pageEdge + 30, totalLength, 110, 16));
            gfx.DrawRectangle(XBrushes.Black, new XRect(pageEdge + 140, totalLength, 230, 16));
            totalLength += 1;
            gfx.DrawRectangle(XBrushes.White, new XRect(pageEdge + 31, totalLength, 108, 14));
            gfx.DrawRectangle(XBrushes.White, new XRect(pageEdge + 141, totalLength, 228, 14));
            totalLength += 2;
            tf.DrawString("Patient Address:", font, XBrushes.Black, new XRect(pageEdge + 32, totalLength, page.Width, 50));
            tf.DrawString(patient.ADDRESS1.Replace(Environment.NewLine, ", ") + ", " + patient.POSTCODE.Trim(), font, XBrushes.Black, new XRect(pageEdge + 142, totalLength, 250, 50));
            totalLength += 12;
            gfx.DrawRectangle(XBrushes.Black, new XRect(pageEdge + 30, totalLength, 110, 16));
            gfx.DrawRectangle(XBrushes.Black, new XRect(pageEdge + 140, totalLength, 230, 16));
            totalLength += 1;
            gfx.DrawRectangle(XBrushes.White, new XRect(pageEdge + 31, totalLength, 108, 14));
            gfx.DrawRectangle(XBrushes.White, new XRect(pageEdge + 141, totalLength, 228, 14));
            totalLength += 2;
            tf.DrawString("Contact Number:", font, XBrushes.Black, new XRect(pageEdge + 32, totalLength, page.Width, 50));
            if (patient.PtTelMobile != null)
            {
                tf.DrawString(patient.PtTelMobile, font, XBrushes.Black, new XRect(pageEdge + 142, totalLength, page.Width, 50));
            }
            else if(patient.TEL != null)
            {
                tf.DrawString(patient.TEL, font, XBrushes.Black, new XRect(pageEdge + 142, totalLength, page.Width, 50));
            }
            totalLength += 12;
            gfx.DrawRectangle(XBrushes.Black, new XRect(pageEdge + 30, totalLength, 110, 16));
            gfx.DrawRectangle(XBrushes.Black, new XRect(pageEdge + 140, totalLength, 230, 16));
            totalLength += 1;
            gfx.DrawRectangle(XBrushes.White, new XRect(pageEdge + 31, totalLength, 108, 14));
            gfx.DrawRectangle(XBrushes.White, new XRect(pageEdge + 141, totalLength, 228, 14));
            totalLength += 2;
            tf.DrawString("Genetics Ref:", font, XBrushes.Black, new XRect(pageEdge + 32, totalLength, page.Width, 50));
            tf.DrawString(patient.CGU_No, font, XBrushes.Black, new XRect(pageEdge + 142, totalLength, page.Width, 50));
            totalLength += 25;
            //descriptions etc
            tf.DrawString("Record of Discussion Regarding Testing & Storage of Genetic Material", fontBold, XBrushes.Black, new XRect(pageEdge + 70, totalLength, page.Width, 10));
            totalLength += 20;            
            tf.DrawString("I have discussed genetic testing for _____________________________________________ and I understand that:", font, XBrushes.Black, new XRect(pageEdge + 10, totalLength, page.Width - 20, 40));
            totalLength += 20;
            tf.DrawString("Family Implications", fontSmallBold, XBrushes.Black, new XRect(pageEdge + 15, totalLength, page.Width - 20, 20));
            totalLength += 15;
            tf.DrawString("1.", font, XBrushes.Black, new XRect(pageEdge + 10, totalLength, 20, 40));
            tf.DrawString("The results of this test may have implications for my relatives. I acknowledge that the results may sometimes be used to inform the appropriate healthcare of relatives in a " +
                "way that is not personally identifiable.", font, XBrushes.Black, new XRect(pageEdge + 30, totalLength, page.Width - 60, 40));
            totalLength += 30;
            tf.DrawString("I am happy for results to be shared that identify me if necessary:", font, XBrushes.Black, new XRect(pageEdge + 30, totalLength, page.Width - 60, 40));
            tf.DrawString("Yes/No", fontSmallBold, XBrushes.Black, new XRect(pageEdge + 400, totalLength, 60, 40));
            totalLength += 20;
            tf.DrawString("Uncertainty", fontSmallBold, XBrushes.Black, new XRect(pageEdge + 15, totalLength, page.Width - 20, 20));
            totalLength += 15;
            tf.DrawString("2.", font, XBrushes.Black, new XRect(pageEdge + 10, totalLength, 20, 40));
            tf.DrawString("The results of this test may reveal genetic variants of uncertain significance. Establishing whether such variants are significant may require (inter)national comparisons. " +
                "I acknowledge that interpretation of the results may change over time as our understanding increases.", font, XBrushes.Black, new XRect(pageEdge + 30, totalLength, page.Width - 60, 60));
            totalLength += 45;
            tf.DrawString("Unexpected Information", fontSmallBold, XBrushes.Black, new XRect(pageEdge + 15, totalLength, page.Width - 20, 20));
            totalLength += 15;
            tf.DrawString("3.", font, XBrushes.Black, new XRect(pageEdge + 10, totalLength, 20, 40));
            tf.DrawString("The results of this test may reveal information relevant to other diseases that are not related to why this test is being done. These may be found by chance and further investigations " +
                "may be needed to assess their significance. If these additional findings are to be looked for, I will be given more information about this. The test may reveal " +
                "non-paternity/maternity.", font, XBrushes.Black, new XRect(pageEdge + 30, totalLength, page.Width - 60, 60));
            totalLength += 55;     
            tf.DrawString("DNA Storage", fontSmallBold, XBrushes.Black, new XRect(pageEdge + 15, totalLength, page.Width - 20, 20));
            totalLength += 15;
            tf.DrawString("4.", font, XBrushes.Black, new XRect(pageEdge + 10, totalLength, 20, 40));
            tf.DrawString("Normal laboratory practice is to store the DNA extracted from a sample even after the current testing is complete. " +
                "The sample might be used as a ‘quality control’ for other testing, for example, that of family members.", font, XBrushes.Black, new XRect(pageEdge + 30, totalLength, page.Width - 60, 40));           
            totalLength += 45;
            tf.DrawString("Data Storage", fontSmallBold, XBrushes.Black, new XRect(pageEdge + 15, totalLength, page.Width - 20, 20));
            totalLength += 15;
            tf.DrawString("5.", font, XBrushes.Black, new XRect(pageEdge + 10, totalLength, 20, 40));
            tf.DrawString("Data generated from this genetic test will be stored to allow possible future interpretations.", font, XBrushes.Black, new XRect(pageEdge + 30, totalLength, page.Width - 60, 40));
            totalLength += 20;            
            tf.DrawString("Health Records", fontSmallBold, XBrushes.Black, new XRect(pageEdge + 15, totalLength, page.Width - 20, 20));
            totalLength += 15;
            tf.DrawString("6.", font, XBrushes.Black, new XRect(pageEdge + 10, totalLength, 20, 40));
            tf.DrawString("Results from this and my test report will be part of my patient health record.", font, XBrushes.Black, new XRect(pageEdge + 30, totalLength, page.Width - 60, 40));
            totalLength += 20;            
            tf.DrawString("Insurance", fontSmallBold, XBrushes.Black, new XRect(pageEdge + 15, totalLength, page.Width - 20, 20));
            totalLength += 15;
            tf.DrawString("7.", font, XBrushes.Black, new XRect(pageEdge + 10, totalLength, 20, 40));
            tf.DrawString("The results of this test may have implications for insurance. Please refer to the UK Code on Genetic Testing and Insurance for further guidance.", font, XBrushes.Black, new XRect(pageEdge + 30, totalLength, page.Width - 60, 40));
            totalLength += 30;
            tf.DrawString("Note of other specific issues discussed (e.g. referral to particular research programmes):", font, XBrushes.Black, new XRect(pageEdge + 30, totalLength, page.Width - 60, 40));           
            totalLength += 20;
            tf.DrawString("__________________________________________________________________________________", font, XBrushes.Black, new XRect(pageEdge + 30, totalLength, page.Width - 60, 40));
            totalLength += 20;
            tf.DrawString("__________________________________________________________________________________", font, XBrushes.Black, new XRect(pageEdge + 30, totalLength, page.Width - 60, 40));
            totalLength += 30;

            //box of stuff at the bottom
            gfx.DrawRectangle(XBrushes.LightGray, new XRect(pageEdge + 30, totalLength, pageWidth - 60, 200));
            totalLength += 1;
            gfx.DrawRectangle(XBrushes.White, new XRect(pageEdge + 31, totalLength, pageWidth - 63, 198));            
            totalLength += 5;
            tf.DrawString("I will be informed of the results by:_________________________(telephone / post / in person)", fontSmallBold, XBrushes.Black, new XRect(pageEdge + 35, totalLength, page.Width - 60, 20));
            totalLength += 25;
            tf.DrawString("Name:_________________________ Signature: ______________________ Date: ____ / ____ / ______", fontSmallBold, XBrushes.Black, new XRect(pageEdge + 35, totalLength, page.Width - 60, 20));
            totalLength += 25;
            tf.DrawString("(Patient/Parent)", fontSmallBold, XBrushes.Black, new XRect(pageEdge + 35, totalLength, page.Width - 60, 20));
            totalLength += 25;
            tf.DrawString("If I am unable to receive the results (eg. due to permanent severe illness or death), I would like them to be given to", fontSmallBold, XBrushes.Black, new XRect(pageEdge + 35, totalLength, page.Width - 100, 40));
            totalLength += 40;
            tf.DrawString("Name:_________________________ Date of Birth: ____ / ____ / ______ Postcode: _________________", fontSmallBold, XBrushes.Black, new XRect(pageEdge + 35, totalLength, page.Width - 60, 20));
            totalLength += 25;
            tf.DrawString("Relationship:___________________________________________________________________________", fontSmallBold, XBrushes.Black, new XRect(pageEdge + 35, totalLength, page.Width - 60, 20));
            totalLength += 33;
            tf.DrawString(staffMember.NAME, fontSmall, XBrushes.Black, new XRect(pageEdge + 115, totalLength, page.Width - 60, 20));
            totalLength += 2;
            tf.DrawString("Clinician Name:_________________________ Clinician Signature: ______________________", fontSmallBold, XBrushes.Black, new XRect(pageEdge + 35, totalLength, page.Width - 60, 20));

            
            string sigFilename = $"{staffMember.StaffForename.Replace(" ", "")}{staffMember.StaffSurname.Replace("'", "").Replace(" ", "")}.jpg";

            if (File.Exists(@$"wwwroot\Signatures\{sigFilename}"))
            {
                XImage sig = XImage.FromFile(@$"wwwroot\Signatures\{sigFilename}");
                gfx.DrawImage(sig, 375, totalLength - 22, sig.PixelWidth/2, sig.PixelHeight/2);
            }


            document.Save(Path.Combine(Directory.GetCurrentDirectory(), $"wwwroot\\StandardLetterPreviews\\consentform-{user}.pdf"));

            if (!isPreview.GetValueOrDefault()) //send to clinicians' VOT (depending on name and job title)
            {
                string votFolder = "";

                if (user == "mnln") { votFolder = "Martin Loxton (Test)"; }
                else
                {
                    if (staffMember.CLINIC_SCHEDULER_GROUPS == "GC")
                    {
                        switch (staffMember.POSITION)
                        {
                            case "Genomics Associate":
                                votFolder = "GenA ";
                                break;
                            case "Genomics Practitioner":
                                votFolder = "GenP ";
                                break;
                            default:
                                votFolder = "GC ";
                                break;
                        }
                    }
                    else
                    {
                        votFolder = staffMember.StaffTitle + " ";
                    }
                    votFolder = votFolder + staffMember.StaffForename + " " + staffMember.StaffSurname;
                }
                if (File.Exists($@"\\zion.matrix.local\dfsrootbwh\cling\Virtual Out-trays (letter enclosures)\{votFolder}\Consent Form_{patient.CGU_No}.pdf"))
                {
                    File.Delete($@"\\zion.matrix.local\dfsrootbwh\cling\Virtual Out-trays (letter enclosures)\{votFolder}\Consent Form_{patient.CGU_No}.pdf");
                }
                System.IO.File.Copy($"wwwroot\\StandardLetterPreviews\\consentform-{user}.pdf", $@"\\zion.matrix.local\dfsrootbwh\cling\Virtual Out-trays (letter enclosures)\{votFolder}\Consent Form_{patient.CGU_No}.pdf");
            }

            return true;
        }
    }
}
