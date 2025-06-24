using ClinicalXPDataConnections.Data;
using ClinicalXPDataConnections.Models;
using ClinicalXPDataConnections.Meta;
using PdfSharpCore.Drawing;
using PdfSharpCore.Drawing.Layout;
using ClinicX.Models;
using ClinicX.Meta;
using ClinicX.Data;



namespace ClinicX.Controllers
{
    public class BloodFormController 
    {
        private readonly ClinicalContext _clinContext;
        private readonly ClinicXContext _cxContext;
        private readonly IPatientData _patientData;
        private readonly IBloodFormData _bloodFormData;
        private readonly IStaffUserData _staffUser;
        private readonly ITestData _testData;
        private readonly IClinicData _clinicData;
        

        public BloodFormController(ClinicalContext clinContext, ClinicXContext cxContext)
        {
            _clinContext = clinContext;  
            _cxContext = cxContext;            
            _patientData = new PatientData(_clinContext);
            _bloodFormData = new BloodFormData(_cxContext);   
            _staffUser = new StaffUserData(_clinContext);
            _testData = new TestData(_clinContext, _cxContext);
            _clinicData = new ClinicData(_clinContext);
        }



        public void CreateBloodForm(int bloodFormID, string user, string? altPatName = "", bool? isPreview = false)
        {
            StaffMember staffMember = _staffUser.GetStaffMemberDetails(user);
            BloodForm bloodForm = _bloodFormData.GetBloodFormDetails(bloodFormID);
            Test test = _testData.GetTestDetails(bloodForm.TestID);
            Patient patient = _patientData.GetPatientDetails(test.MPI);
            
            //creates a new PDF document
            PdfSharpCore.Pdf.PdfDocument document = new PdfSharpCore.Pdf.PdfDocument();
            document.Info.Title = "My PDF";
            PdfSharpCore.Pdf.PdfPage page = document.AddPage();
            PdfSharpCore.Pdf.PdfPage page2 = document.AddPage();
            XGraphics gfx = XGraphics.FromPdfPage(page);
            XGraphics gfx2 = XGraphics.FromPdfPage(page2);
            var tf = new XTextFormatter(gfx);
            var tf2 = new XTextFormatter(gfx2);

            XFont font = new XFont("Arial", 12, XFontStyle.Regular);
            XFont fontSmall = new XFont("Arial", 8, XFontStyle.Regular);
            XFont fontBold = new XFont("Arial", 12, XFontStyle.Bold);
            XFont fontSmallBold = new XFont("Arial", 8, XFontStyle.Bold);
            XFont fontSmallUnderlined = new XFont("Arial", 8, XFontStyle.Underline);
            XFont fontLarge = new XFont("Arial", 16, XFontStyle.Regular);
            XFont fontLargeBold = new XFont("Arial", 16, XFontStyle.Bold);
            XFont symbols = new XFont("Segoe UI Symbol", 12, XFontStyle.Bold);
            string tick = "\u2713";
            
            //measures etc
            int totalLength = 15;
            int pageEdge = 12;
            int pageWidth = 570;

            //title, addresses
            gfx.DrawRectangle(XBrushes.Black, new XRect(pageEdge, totalLength, pageWidth, 60));
            totalLength += 1;
            gfx.DrawRectangle(XBrushes.White, new XRect(pageEdge + 1, totalLength, pageWidth - 2, 58));
            totalLength += 4;
            tf.DrawString("West Midlands Regional Genetics Laboratory", font, XBrushes.Teal, new XRect(25, totalLength, page.Width, 50));            
            totalLength += 15;
            tf.DrawString("Birmingham Women's Hospital", fontSmallBold, XBrushes.Black, new XRect(25, totalLength, page.Width, 50));
            totalLength += 10;
            tf.DrawString("Mindelsohn Way | Edgbaston | Birmingham | B15 2TG", fontSmall, XBrushes.Black, new XRect(25, totalLength, page.Width, 50));
            totalLength += 10;
            tf.DrawString("Tel:", fontSmallBold, XBrushes.Black, new XRect(25, totalLength, page.Width, 50));
            tf.DrawString("0121 335 8036", fontSmall, XBrushes.Black, new XRect(50, totalLength, page.Width, 50));
            totalLength += 10;
            tf.DrawString("Email:", fontSmallBold, XBrushes.Black, new XRect(25, totalLength, page.Width, 50));
            tf.DrawString("bwc.genetics.lab@nhs.net", fontSmallUnderlined, XBrushes.Blue, new XRect(50, totalLength, page.Width, 50));
            totalLength += 10;

            gfx.DrawRectangle(XBrushes.Green, new XRect(pageEdge, totalLength, pageWidth, 20));
            totalLength += 2;
            tf.DrawString("RARE DISEASE AND REPRODUCTIVE GENOMICS TEST REQUEST", font, XBrushes.White, new XRect(100, totalLength, page.Width, 20));
            totalLength += 18;
            gfx.DrawRectangle(XBrushes.Black, new XRect(pageEdge, totalLength, pageWidth, 20));
            totalLength += 1;
            gfx.DrawRectangle(XBrushes.White, new XRect(pageEdge + 1, totalLength, pageWidth - 2, 18));
            totalLength += 19;
            gfx.DrawRectangle(XBrushes.Green, new XRect(pageEdge, totalLength, pageWidth, 20));
            totalLength += 2;
            tf.DrawString("PATIENT DETAILS", fontBold, XBrushes.White, new XRect(100, totalLength, 200, 20));
            tf.DrawString("CLINICIAN DETAILS", fontBold, XBrushes.White, new XRect(350, totalLength, 200, 20));
            totalLength += 18;
            //firstname/cliniican
            gfx.DrawRectangle(XBrushes.Black, new XRect(pageEdge, totalLength, pageWidth/2, 20));
            gfx.DrawRectangle(XBrushes.Black, new XRect(pageEdge + pageWidth/2, totalLength, pageWidth/2, 20));
            totalLength += 1;
            gfx.DrawRectangle(XBrushes.White, new XRect(pageEdge + 1, totalLength, pageWidth / 2 - 2, 18));
            gfx.DrawRectangle(XBrushes.White, new XRect(pageEdge + pageWidth / 2 + 1, totalLength, pageWidth / 2 - 2, 18));
            totalLength += 1;
            tf.DrawString("Forename:", fontSmallBold, XBrushes.Black, new XRect(pageEdge + 5, totalLength, 100, 20));
            if (altPatName == "" || altPatName == null)
            {
                tf.DrawString(patient.FIRSTNAME, font, XBrushes.Black, new XRect(pageEdge + 60, totalLength, 100, 20));
            }
            else
            {
                tf.DrawString(altPatName, font, XBrushes.Black, new XRect(pageEdge + 60, totalLength, 100, 20));
            }
            tf.DrawString("Consultant/Clinician:", fontSmallBold, XBrushes.Black, new XRect(pageEdge + pageWidth / 2 + 5, totalLength, 100, 20));
            tf.DrawString(staffMember.NAME, font, XBrushes.Black, new XRect(pageEdge + pageWidth / 2 + 100, totalLength, 100, 20));
            totalLength += 17;
            //surname/phone number
            gfx.DrawRectangle(XBrushes.Black, new XRect(pageEdge, totalLength, pageWidth / 2, 20));
            gfx.DrawRectangle(XBrushes.Black, new XRect(pageEdge + pageWidth / 2, totalLength, pageWidth / 2, 20));
            totalLength += 1;
            gfx.DrawRectangle(XBrushes.White, new XRect(pageEdge + 1, totalLength, pageWidth / 2 - 2, 18));
            gfx.DrawRectangle(XBrushes.White, new XRect(pageEdge + pageWidth / 2 + 1, totalLength, pageWidth / 2 - 2, 18));
            totalLength += 1;
            tf.DrawString("Surname:", fontSmallBold, XBrushes.Black, new XRect(pageEdge + 5, totalLength, 100, 20));
            tf.DrawString(patient.LASTNAME, font, XBrushes.Black, new XRect(pageEdge + 60, totalLength, 100, 20));
            tf.DrawString("Tel number/Bleep:", fontSmallBold, XBrushes.Black, new XRect(pageEdge + pageWidth / 2 + 5, totalLength, 100, 20));
            tf.DrawString("0121 335 8024", font, XBrushes.Black, new XRect(pageEdge + pageWidth / 2 + 100, totalLength, 100, 20));
            totalLength += 17;
            //DOB etc
            gfx.DrawRectangle(XBrushes.Black, new XRect(pageEdge, totalLength, pageWidth / 2 - 100, 20));
            gfx.DrawRectangle(XBrushes.Black, new XRect(pageEdge + pageWidth / 2 - 100, totalLength, 100, 20));
            gfx.DrawRectangle(XBrushes.Black, new XRect(pageEdge + pageWidth / 2, totalLength, pageWidth / 2, 40));
            totalLength += 1;
            gfx.DrawRectangle(XBrushes.White, new XRect(pageEdge + 1, totalLength, pageWidth / 2 - 102, 18));
            gfx.DrawRectangle(XBrushes.White, new XRect(pageEdge + pageWidth / 2 - 99, totalLength, 98, 18));
            gfx.DrawRectangle(XBrushes.White, new XRect(pageEdge + pageWidth / 2 + 1, totalLength, pageWidth / 2 - 2, 38));
            totalLength += 1;
            tf.DrawString("Date of birth:", fontSmallBold, XBrushes.Black, new XRect(pageEdge + 5, totalLength, 100, 20));
            tf.DrawString(patient.DOB.Value.ToString("dd/MM/yyyy"), font, XBrushes.Black, new XRect(pageEdge + 60, totalLength, 100, 20));
            tf.DrawString("Sex:", fontSmallBold, XBrushes.Black, new XRect(pageEdge + pageWidth / 2 - 95, totalLength, 200, 20));
            tf.DrawString(patient.SEX, font, XBrushes.Black, new XRect(pageWidth / 2 - 50, totalLength, 50, 20));
            tf.DrawString("Email address:", fontSmallBold, XBrushes.Black, new XRect(pageEdge + pageWidth / 2 + 5, totalLength, 200, 20));
            totalLength += 10;
            tf.DrawString("genetics.ipt@nhs.net", fontSmallUnderlined, XBrushes.Blue, new XRect(pageEdge + pageWidth / 2 + 50, totalLength, 200, 20));
            totalLength += 8;
            gfx.DrawRectangle(XBrushes.Black, new XRect(pageEdge, totalLength, pageWidth / 2, 20));
            totalLength += 1;
            gfx.DrawRectangle(XBrushes.White, new XRect(pageEdge + 1, totalLength, pageWidth / 2 - 2, 18));
            totalLength += 1;
            tf.DrawString("Ethnicity:", fontSmallBold, XBrushes.Black, new XRect(pageEdge + 5, totalLength, 100, 20));
            totalLength += 1;
            tf.DrawString(patient.Ethnic, font, XBrushes.Black, new XRect(pageEdge + 50, totalLength, 250, 20));
            totalLength += 16;

            gfx.DrawRectangle(XBrushes.Black, new XRect(pageEdge, totalLength, pageWidth / 2, 20));
            gfx.DrawRectangle(XBrushes.Black, new XRect(pageEdge + pageWidth / 2, totalLength, pageWidth / 2, 61));
            totalLength += 1;
            gfx.DrawRectangle(XBrushes.White, new XRect(pageEdge + 1, totalLength, pageWidth / 2 - 2, 18));
            gfx.DrawRectangle(XBrushes.White, new XRect(pageEdge + pageWidth / 2 + 1, totalLength, pageWidth / 2 - 2, 59));
            totalLength += 1;
            tf.DrawString("Address:", fontSmallBold, XBrushes.Black, new XRect(pageEdge + 5, totalLength, 100, 20));
            tf.DrawString($"{patient.ADDRESS1}, {patient.ADDRESS3}, {patient.POSTCODE}" , fontSmall, XBrushes.Black, new XRect(pageEdge + 50, totalLength, 250, 40));
            tf.DrawString("Hospital (please specify hospital site within Trust):", fontSmallBold, XBrushes.Black, new XRect(pageEdge + pageWidth / 2 + 5, totalLength, 300, 20));
            totalLength += 18;
            gfx.DrawRectangle(XBrushes.Black, new XRect(pageEdge, totalLength, pageWidth / 2, 20));
            totalLength += 1;
            gfx.DrawRectangle(XBrushes.White, new XRect(pageEdge + 1, totalLength, pageWidth / 2 - 2, 18));
            totalLength += 1;
            tf.DrawString("Hospital Number:", fontSmallBold, XBrushes.Black, new XRect(pageEdge + 5, totalLength, 100, 20));
            tf.DrawString(patient.CGU_No, font, XBrushes.Black, new XRect(pageEdge + 100, totalLength, 200, 20));
            tf.DrawString("Birmingham Women's Hospital" + Environment.NewLine + "Clinical Genetics", font, XBrushes.Black, new XRect(pageEdge + pageWidth / 2 + 50, totalLength, 200, 50));
            totalLength += 18;
            gfx.DrawRectangle(XBrushes.Black, new XRect(pageEdge, totalLength, pageWidth / 2, 21));
            totalLength += 1;
            gfx.DrawRectangle(XBrushes.White, new XRect(pageEdge + 1, totalLength, pageWidth / 2 - 2, 19));
            //NHS no boxes
            gfx.DrawRectangle(XBrushes.DarkGray, new XRect(pageEdge + 49, totalLength, 20, 19));
            gfx.DrawRectangle(XBrushes.DarkGray, new XRect(pageEdge + 69, totalLength, 20, 19));
            gfx.DrawRectangle(XBrushes.DarkGray, new XRect(pageEdge + 89, totalLength, 20, 19));

            gfx.DrawRectangle(XBrushes.DarkGray, new XRect(pageEdge + 109, totalLength, 18, 19));

            gfx.DrawRectangle(XBrushes.DarkGray, new XRect(pageEdge + 127, totalLength, 20, 19));
            gfx.DrawRectangle(XBrushes.DarkGray, new XRect(pageEdge + 147, totalLength, 20, 19));
            gfx.DrawRectangle(XBrushes.DarkGray, new XRect(pageEdge + 167, totalLength, 20, 19));

            gfx.DrawRectangle(XBrushes.DarkGray, new XRect(pageEdge + 187, totalLength, 18, 19));

            gfx.DrawRectangle(XBrushes.DarkGray, new XRect(pageEdge + 205, totalLength, 20, 19));
            gfx.DrawRectangle(XBrushes.DarkGray, new XRect(pageEdge + 225, totalLength, 20, 19));
            gfx.DrawRectangle(XBrushes.DarkGray, new XRect(pageEdge + 245, totalLength, 20, 19));
            gfx.DrawRectangle(XBrushes.DarkGray, new XRect(pageEdge + 265, totalLength, 20, 19));

            totalLength += 1;
            //inside of NHS no boxes
            gfx.DrawRectangle(XBrushes.White, new XRect(pageEdge + 50, totalLength, 18, 17));
            gfx.DrawRectangle(XBrushes.White, new XRect(pageEdge + 70, totalLength, 18, 17));
            gfx.DrawRectangle(XBrushes.White, new XRect(pageEdge + 90, totalLength, 18, 17));
            gfx.DrawRectangle(XBrushes.White, new XRect(pageEdge + 128, totalLength, 18, 17));
            gfx.DrawRectangle(XBrushes.White, new XRect(pageEdge + 148, totalLength, 18, 17));
            gfx.DrawRectangle(XBrushes.White, new XRect(pageEdge + 168, totalLength, 18, 17));
            gfx.DrawRectangle(XBrushes.White, new XRect(pageEdge + 206, totalLength, 18, 17));
            gfx.DrawRectangle(XBrushes.White, new XRect(pageEdge + 226, totalLength, 18, 17));
            gfx.DrawRectangle(XBrushes.White, new XRect(pageEdge + 246, totalLength, 18, 17));
            gfx.DrawRectangle(XBrushes.White, new XRect(pageEdge + 266, totalLength, 18, 17));

            tf.DrawString("NHS No:", fontSmallBold, XBrushes.Black, new XRect(pageEdge + 5, totalLength, 100, 20));
            totalLength += 1;
            //to split the NHS number into individual boxes
            tf.DrawString(patient.SOCIAL_SECURITY.Substring(0,1), font, XBrushes.Black, new XRect(pageEdge + 55, totalLength, 20, 20));
            tf.DrawString(patient.SOCIAL_SECURITY.Substring(1, 1), font, XBrushes.Black, new XRect(pageEdge + 75, totalLength, 20, 20));
            tf.DrawString(patient.SOCIAL_SECURITY.Substring(2, 1), font, XBrushes.Black, new XRect(pageEdge + 95, totalLength, 20, 20));
            tf.DrawString(patient.SOCIAL_SECURITY.Substring(3, 1), font, XBrushes.Black, new XRect(pageEdge + 133, totalLength, 20, 20));
            tf.DrawString(patient.SOCIAL_SECURITY.Substring(4, 1), font, XBrushes.Black, new XRect(pageEdge + 153, totalLength, 20, 20));
            tf.DrawString(patient.SOCIAL_SECURITY.Substring(5, 1), font, XBrushes.Black, new XRect(pageEdge + 173, totalLength, 20, 20));
            tf.DrawString(patient.SOCIAL_SECURITY.Substring(6, 1), font, XBrushes.Black, new XRect(pageEdge + 211, totalLength, 20, 20));
            tf.DrawString(patient.SOCIAL_SECURITY.Substring(7, 1), font, XBrushes.Black, new XRect(pageEdge + 231, totalLength, 20, 20));
            tf.DrawString(patient.SOCIAL_SECURITY.Substring(8, 1), font, XBrushes.Black, new XRect(pageEdge + 251, totalLength, 20, 20));
            tf.DrawString(patient.SOCIAL_SECURITY.Substring(9, 1), font, XBrushes.Black, new XRect(pageEdge + 271, totalLength, 20, 20));
            totalLength += 18;
            gfx.DrawRectangle(XBrushes.Black, new XRect(pageEdge, totalLength, 110, 20));
            gfx.DrawRectangle(XBrushes.Black, new XRect(pageEdge + 110, totalLength, 80, 20));
            gfx.DrawRectangle(XBrushes.Black, new XRect(pageEdge + 190, totalLength, 95, 20));
            gfx.DrawRectangle(XBrushes.Black, new XRect(pageEdge + 285, totalLength, 285, 20));
            totalLength += 1;
            gfx.DrawRectangle(XBrushes.White, new XRect(pageEdge + 1, totalLength, 108, 18));
            gfx.DrawRectangle(XBrushes.White, new XRect(pageEdge + 111, totalLength, 78, 18));
            gfx.DrawRectangle(XBrushes.White, new XRect(pageEdge + 191, totalLength, 93, 18));
            gfx.DrawRectangle(XBrushes.White, new XRect(pageEdge + 286, totalLength, 283, 18));
            totalLength += 3;
            gfx.DrawRectangle(XBrushes.Black, new XRect(pageEdge + 5, totalLength, 10, 10));
            gfx.DrawRectangle(XBrushes.Black, new XRect(pageEdge + 60, totalLength, 10, 10));
            gfx.DrawRectangle(XBrushes.Black, new XRect(pageEdge + 115, totalLength, 10, 10));
            gfx.DrawRectangle(XBrushes.Black, new XRect(pageEdge + 145, totalLength, 10, 10));
            gfx.DrawRectangle(XBrushes.Black, new XRect(pageEdge + 194, totalLength, 10, 10));
            gfx.DrawRectangle(XBrushes.Black, new XRect(pageEdge + 235, totalLength, 10, 10));
            totalLength += 1;
            gfx.DrawRectangle(XBrushes.White, new XRect(pageEdge + 6, totalLength, 8, 8));
            //Tick boxes (NHS/Private, In/Outpatient, etc)
            tf.DrawString("Inpatient", fontSmall, XBrushes.Black, new XRect(pageEdge + 16, totalLength, 40, 20));
            gfx.DrawRectangle(XBrushes.White, new XRect(pageEdge + 61, totalLength, 8, 8));
            if(bloodForm.IsInpatient)
            {
                gfx.DrawString(tick, symbols, XBrushes.Black, pageEdge + 8, totalLength + 6);
            }
            tf.DrawString("Outpatient", fontSmall, XBrushes.Black, new XRect(pageEdge + 71, totalLength, 40, 20));
            if (bloodForm.IsOutpatient)
            {
                gfx.DrawString(tick, symbols, XBrushes.Black, pageEdge + 62, totalLength + 6);
            }
            gfx.DrawRectangle(XBrushes.White, new XRect(pageEdge + 116, totalLength, 8, 8));
            tf.DrawString("NHS", fontSmall, XBrushes.Black, new XRect(pageEdge + 127, totalLength, 40, 20));
            if (bloodForm.IsNHS)
            {
                gfx.DrawString(tick, symbols, XBrushes.Black, pageEdge + 118, totalLength + 6);
            }
            gfx.DrawRectangle(XBrushes.White, new XRect(pageEdge + 146, totalLength, 8, 8));
            tf.DrawString("Private", fontSmall, XBrushes.Black, new XRect(pageEdge + 157, totalLength, 40, 20));
            if (bloodForm.IsPrivate)
            {
                gfx.DrawString(tick, symbols, XBrushes.Black, pageEdge + 148, totalLength + 6);
            }
            gfx.DrawRectangle(XBrushes.White, new XRect(pageEdge + 195, totalLength, 8, 8));
            tf.DrawString("Urgent", fontSmall, XBrushes.Black, new XRect(pageEdge + 205, totalLength, 40, 20));
            if (bloodForm.IsUrgent)
            {
                gfx.DrawString(tick, symbols, XBrushes.Black, pageEdge + 195, totalLength + 6);
            }
            gfx.DrawRectangle(XBrushes.White, new XRect(pageEdge + 236, totalLength, 8, 8));
            tf.DrawString("Routine", fontSmall, XBrushes.Black, new XRect(pageEdge + 248, totalLength, 40, 20));
            if (bloodForm.IsRoutine)
            {
                gfx.DrawString(tick, symbols, XBrushes.Black, pageEdge + 238, totalLength + 6);
            }
            tf.DrawString("Date of next appointment:", fontSmallBold, XBrushes.Black, new XRect(pageEdge + 290, totalLength, 150, 20));

            string dtString="N/A";
            List<Appointment> apptList = _clinicData.GetClinicByPatientsList(patient.MPI).Where(a => a.Attendance == "NOT RECORDED").OrderBy(a => a.BOOKED_DATE).ToList();
            apptList = apptList.Where(a => a.BOOKED_DATE > DateTime.Now).ToList();
            
            
            if (apptList.Count > 0)
            {
                Appointment appt = apptList.First();
                DateTime bookedDate = appt.BOOKED_DATE.GetValueOrDefault();
                dtString = bookedDate.ToString("dd/MM/yyyy");
            }

            tf.DrawString(dtString, font, XBrushes.Black, new XRect(pageEdge + 430, totalLength, 150, 20));
            totalLength += 15;
            gfx.DrawRectangle(XBrushes.Green, new XRect(pageEdge, totalLength, pageWidth, 20));
            totalLength += 2;
            tf.DrawString("CLINICAL DETAILS", fontBold, XBrushes.White, new XRect(pageEdge + 10, totalLength, 200, 20));
            totalLength += 1;
            tf.DrawString("Please provide detailed information. Incomplete referral forms will not be processed.", fontSmallBold, XBrushes.White, new XRect(pageEdge + 160, totalLength, 350, 20));
            totalLength += 17;
            gfx.DrawRectangle(XBrushes.Black, new XRect(pageEdge, totalLength, pageWidth, 50));
            totalLength += 1;
            gfx.DrawRectangle(XBrushes.White, new XRect(pageEdge + 1, totalLength, pageWidth - 2, 48));
            totalLength += 10;
            if(bloodForm.ClinicalDetails != null)
            {
                tf.DrawString(bloodForm.ClinicalDetails, font, XBrushes.Black, new XRect(pageEdge + 20, totalLength, 400, 50));
            }
            totalLength += 38;
            gfx.DrawRectangle(XBrushes.Green, new XRect(pageEdge, totalLength, pageWidth, 35));
            totalLength += 2;
            tf.DrawString("GENOMIC TESTING REQUIRED", fontBold, XBrushes.White, new XRect(pageEdge + 10, totalLength, 200, 20));
            totalLength += 1;
            tf.DrawString("Please refer to NHSE Test Directory (TD) code (where appropriate)", fontSmallBold, XBrushes.White, new XRect(pageEdge + 220, totalLength, 350, 20));
            totalLength += 14;
            tf.DrawString("https://www.england.nhs.uk/publication/national-genomic-test-directories/", fontSmallUnderlined, XBrushes.White, new XRect(pageEdge + 10, totalLength, 350, 20));
            totalLength += 11;
            gfx.DrawRectangle(XBrushes.Black, new XRect(pageEdge, totalLength, pageWidth, 50));
            totalLength += 1;
            gfx.DrawRectangle(XBrushes.White, new XRect(pageEdge + 1, totalLength, pageWidth - 2, 48));
            totalLength += 4;
            tf.DrawString("TD code(s) and indication:", fontSmallBold, XBrushes.Black, new XRect(pageEdge + 10, totalLength, 120, 20));
            totalLength += 10;
            if (bloodForm.TestingRequirements != null)
            {
                tf.DrawString(bloodForm.TestingRequirements, font, XBrushes.Black, new XRect(pageEdge + 20, totalLength, 400, 50));
            }
            totalLength += 34;
            //tick boxes for testing requirements
            gfx.DrawRectangle(XBrushes.Black, new XRect(pageEdge, totalLength, pageWidth / 5, 20));
            gfx.DrawRectangle(XBrushes.Black, new XRect(pageEdge + pageWidth / 5, totalLength, pageWidth / 5, 20));
            gfx.DrawRectangle(XBrushes.Black, new XRect(pageEdge + (pageWidth / 5) * 2, totalLength, pageWidth / 5, 20));
            gfx.DrawRectangle(XBrushes.Black, new XRect(pageEdge + (pageWidth / 5) * 3, totalLength, pageWidth / 5, 20));
            gfx.DrawRectangle(XBrushes.Black, new XRect(pageEdge + (pageWidth / 5) * 4, totalLength, pageWidth / 5, 20));

            totalLength += 1;
            gfx.DrawRectangle(XBrushes.White, new XRect(pageEdge + 1, totalLength, pageWidth / 5 - 2, 18));
            gfx.DrawRectangle(XBrushes.White, new XRect(pageEdge + pageWidth / 5 + 1, totalLength, pageWidth / 5 - 2, 18));
            gfx.DrawRectangle(XBrushes.White, new XRect(pageEdge + (pageWidth / 5) * 2 + 1, totalLength, pageWidth / 5 - 2, 18));
            gfx.DrawRectangle(XBrushes.White, new XRect(pageEdge + (pageWidth / 5) * 3 + 1, totalLength, pageWidth / 5 - 2, 18));
            gfx.DrawRectangle(XBrushes.White, new XRect(pageEdge + (pageWidth / 5) * 4 + 1, totalLength, pageWidth / 5 - 2, 18));
            totalLength += 4;
            tf.DrawString("Status:", fontSmallBold, XBrushes.Black, new XRect(pageEdge + 10, totalLength, 120, 20));
            tf.DrawString("(please tick)", fontSmall, XBrushes.Black, new XRect(pageEdge + 45, totalLength, 120, 20));

            gfx.DrawRectangle(XBrushes.Black, new XRect(pageEdge + pageWidth / 5 + 10, totalLength, 10, 10));
            gfx.DrawRectangle(XBrushes.Black, new XRect(pageEdge + (pageWidth / 5) * 2 + 10, totalLength, 10, 10));
            gfx.DrawRectangle(XBrushes.Black, new XRect(pageEdge + (pageWidth / 5) * 3 + 10, totalLength, 10, 10));
            gfx.DrawRectangle(XBrushes.Black, new XRect(pageEdge + (pageWidth / 5) * 4 + 10, totalLength, 10, 10));
            totalLength += 1;
            gfx.DrawRectangle(XBrushes.White, new XRect(pageEdge + pageWidth / 5 + 11, totalLength, 8, 8));
            tf.DrawString("Prenatal", fontSmall, XBrushes.Black, new XRect(pageEdge + pageWidth / 5 + 25, totalLength, 100, 20));            
            if (bloodForm.IsPrenatal)
            {
                gfx.DrawString(tick, symbols, XBrushes.Black, pageEdge + pageWidth / 5 + 12, totalLength + 6);
            }
            gfx.DrawRectangle(XBrushes.White, new XRect(pageEdge + (pageWidth / 5) * 2 + 11, totalLength, 8, 8));
            tf.DrawString("Presymptomatic", fontSmall, XBrushes.Black, new XRect(pageEdge + (pageWidth / 5) * 2 + 25, totalLength, 100, 20));
            if (bloodForm.IsPresymptomatic)
            {
                gfx.DrawString(tick, symbols, XBrushes.Black, pageEdge + (pageWidth / 5) * 2 + 12, totalLength + 6);
            }
            gfx.DrawRectangle(XBrushes.White, new XRect(pageEdge + (pageWidth / 5) * 3 + 11, totalLength, 8, 8));
            tf.DrawString("Diagnostic", fontSmall, XBrushes.Black, new XRect(pageEdge + (pageWidth / 5) * 3 + 25, totalLength, 100, 20));
            if (bloodForm.IsDiagnostic)
            {
                gfx.DrawString(tick, symbols, XBrushes.Black, pageEdge + (pageWidth / 5) * 3 + 12, totalLength + 6);
            }
            gfx.DrawRectangle(XBrushes.White, new XRect(pageEdge + (pageWidth / 5) * 4 + 11, totalLength, 8, 8));
            tf.DrawString("Carrier Testing", fontSmall, XBrushes.Black, new XRect(pageEdge + (pageWidth / 5) * 4 + 25, totalLength, 100, 20));
            if (bloodForm.IsCarrier)
            {
                gfx.DrawString(tick, symbols, XBrushes.Black, pageEdge + (pageWidth / 5) * 4 + 12, totalLength + 6);
            }
            totalLength += 14;
            gfx.DrawRectangle(XBrushes.Black, new XRect(pageEdge, totalLength, pageWidth / 4 + 2, 50)); //because they don't quite line up for some reason!!
            gfx.DrawRectangle(XBrushes.Black, new XRect(pageEdge + pageWidth / 4 + 2, totalLength, pageWidth / 4, 50));
            gfx.DrawRectangle(XBrushes.Black, new XRect(pageEdge + (pageWidth / 4) * 2 + 2, totalLength, pageWidth / 4, 50));
            gfx.DrawRectangle(XBrushes.Black, new XRect(pageEdge + (pageWidth / 4) * 3 + 2, totalLength, pageWidth / 4, 50));
            totalLength += 1;
            gfx.DrawRectangle(XBrushes.White, new XRect(pageEdge + 1, totalLength, pageWidth / 4 , 48));
            gfx.DrawRectangle(XBrushes.White, new XRect(pageEdge + pageWidth / 4 + 3, totalLength, pageWidth / 4 - 2, 48));
            gfx.DrawRectangle(XBrushes.White, new XRect(pageEdge + (pageWidth / 4) * 2 + 3, totalLength, pageWidth / 4 - 2, 48));
            gfx.DrawRectangle(XBrushes.White, new XRect(pageEdge + (pageWidth / 4) * 3 + 3, totalLength, pageWidth / 4 - 2, 48));
            totalLength += 4;
            tf.DrawString("Details of prenatal screening:", fontSmallBold, XBrushes.Black, new XRect(pageEdge + 10, totalLength, 200, 20));
            tf.DrawString("Current gestation:", fontSmallBold, XBrushes.Black, new XRect(pageEdge + pageWidth / 4 + 10, totalLength, 100, 20));
            tf.DrawString("Type of screening:", fontSmallBold, XBrushes.Black, new XRect(pageEdge + (pageWidth / 4) * 2 + 10, totalLength, 100, 20));
            tf.DrawString("Screening risk:", fontSmallBold, XBrushes.Black, new XRect(pageEdge + (pageWidth / 4) * 3 + 10, totalLength, 100, 20));
            totalLength += 45;
            gfx.DrawRectangle(XBrushes.Green, new XRect(pageEdge, totalLength, pageWidth, 25));
            totalLength += 2;
            tf.DrawString("DETAILS OF AFFECTED FAMILY MEMBERS IF RELEVANT", fontBold, XBrushes.White, new XRect(pageEdge + 10, totalLength, 500, 20));
            tf.DrawString("(please state relationship to the patient)", fontSmallBold, XBrushes.White, new XRect(pageEdge + 350, totalLength, 500, 20));
            totalLength += 18;
            gfx.DrawRectangle(XBrushes.Black, new XRect(pageEdge, totalLength, pageWidth / 2, 20));
            gfx.DrawRectangle(XBrushes.Black, new XRect(pageEdge + pageWidth / 2, totalLength, pageWidth / 4, 20));
            gfx.DrawRectangle(XBrushes.Black, new XRect(pageEdge + pageWidth / 2 + pageWidth / 4, totalLength, pageWidth / 4 + 1, 20));
            totalLength += 1;
            gfx.DrawRectangle(XBrushes.White, new XRect(pageEdge + 1, totalLength, pageWidth / 2 - 2, 18));
            gfx.DrawRectangle(XBrushes.White, new XRect(pageEdge + pageWidth / 2 + 1, totalLength, pageWidth / 4 - 2, 18));
            gfx.DrawRectangle(XBrushes.White, new XRect(pageEdge + pageWidth / 2 + pageWidth / 4 + 1, totalLength, pageWidth / 4 - 2, 18));
            totalLength += 2;
            tf.DrawString("Forename and Surname:", fontSmallBold, XBrushes.Black, new XRect(pageEdge + 10, totalLength, 200, 20));
            if (bloodForm.RelativeName != null)
            {
                tf.DrawString(bloodForm.RelativeName, font, XBrushes.Black, new XRect(pageEdge + 110, totalLength, 200, 20));
            }
            tf.DrawString("DOB:", fontSmallBold, XBrushes.Black, new XRect(pageEdge + pageWidth / 2 + 10, totalLength, 200, 20));
            if (bloodForm.RelativeDOB != null)
            {
                tf.DrawString(bloodForm.RelativeDOB.Value.ToString("dd/MM/yyyy"), font, XBrushes.Black, new XRect(pageEdge + pageWidth / 2 + 50, totalLength, 200, 20));
            }            
            tf.DrawString("Lab/NHS No:", fontSmallBold, XBrushes.Black, new XRect(pageEdge + pageWidth / 2 + pageWidth / 4 + 10, totalLength, 200, 20));
            if (bloodForm.RelativeNo != null)
            {
                tf.DrawString(bloodForm.RelativeNo, font, XBrushes.Black, new XRect(pageEdge + pageWidth / 2 + pageWidth / 4 + 60, totalLength, 200, 20));
            }
            totalLength += 16;
            gfx.DrawRectangle(XBrushes.Black, new XRect(pageEdge, totalLength, pageWidth, 50));
            totalLength += 1;
            gfx.DrawRectangle(XBrushes.White, new XRect(pageEdge + 1, totalLength, pageWidth - 2, 48));
            totalLength += 2;
            tf.DrawString("Details of previous genomic testing:", fontSmallBold, XBrushes.Black, new XRect(pageEdge + 10, totalLength, 150, 20));
            totalLength += 10;
            if (bloodForm.RelativeDetails != null)
            {
                tf.DrawString(bloodForm.RelativeDetails, font, XBrushes.Black, new XRect(pageEdge + 10, totalLength, 500, 20));
            }
            totalLength += 20;
            gfx.DrawRectangle(XBrushes.Green, new XRect(pageEdge, totalLength, pageWidth, 20));
            totalLength += 2;
            tf.DrawString("SPECIMEN DETAILS", fontBold, XBrushes.White, new XRect(pageEdge + 10, totalLength, 500, 20));
            totalLength += 18;
            gfx.DrawRectangle(XBrushes.Black, new XRect(pageEdge, totalLength, pageWidth / 2, 40));
            gfx.DrawRectangle(XBrushes.Black, new XRect(pageEdge + pageWidth / 2, totalLength, pageWidth / 2, 40));
            totalLength += 1;
            gfx.DrawRectangle(XBrushes.Yellow, new XRect(pageEdge + 1, totalLength, pageWidth / 2 - 2, 38));
            gfx.DrawRectangle(XBrushes.Yellow, new XRect(pageEdge + pageWidth / 2 + 1, totalLength, pageWidth / 2 - 2, 38));
            totalLength += 2;
            tf.DrawString("If a specimen is known to present an infection hazard it must be clearly labeled 'DANGER OF INFECTION' and the infection hazard stated:", fontSmall, XBrushes.Red, new XRect(pageEdge + 10, totalLength, 250, 20));
            totalLength += 37;
            gfx.DrawRectangle(XBrushes.Black, new XRect(pageEdge, totalLength, pageWidth / 6, 40));
            gfx.DrawRectangle(XBrushes.Black, new XRect(pageEdge + pageWidth / 6, totalLength, pageWidth / 6, 20));
            gfx.DrawRectangle(XBrushes.Black, new XRect(pageEdge + (pageWidth / 6) * 2, totalLength, pageWidth / 6, 20));
            gfx.DrawRectangle(XBrushes.Black, new XRect(pageEdge + (pageWidth / 6) * 3, totalLength, pageWidth / 6, 20));
            gfx.DrawRectangle(XBrushes.Black, new XRect(pageEdge + (pageWidth / 6) * 4, totalLength, pageWidth / 6, 20));
            gfx.DrawRectangle(XBrushes.Black, new XRect(pageEdge + (pageWidth / 6) * 5, totalLength, pageWidth / 6, 20));
            totalLength += 1;
            gfx.DrawRectangle(XBrushes.White, new XRect(pageEdge + 1, totalLength, pageWidth / 6 - 2, 38));
            gfx.DrawRectangle(XBrushes.White, new XRect(pageEdge + pageWidth / 6 + 1, totalLength, pageWidth / 6 - 2, 18));
            gfx.DrawRectangle(XBrushes.White, new XRect(pageEdge + (pageWidth / 6) * 2 + 1, totalLength, pageWidth / 6 - 2, 18));
            gfx.DrawRectangle(XBrushes.White, new XRect(pageEdge + (pageWidth / 6) * 3 + 1, totalLength, pageWidth / 6 - 2, 18));
            gfx.DrawRectangle(XBrushes.White, new XRect(pageEdge + (pageWidth / 6) * 4 + 1, totalLength, pageWidth / 6 - 2, 18));
            gfx.DrawRectangle(XBrushes.White, new XRect(pageEdge + (pageWidth / 6) * 5 + 1, totalLength, pageWidth / 6 - 2, 18));
            totalLength += 2;
            //tick boxes for sample/specimen type
            tf.DrawString("Sample type:", fontSmallBold, XBrushes.Black, new XRect(pageEdge + 10, totalLength, 150, 20));
            gfx.DrawRectangle(XBrushes.Black, new XRect(pageEdge + pageWidth / 6 + 10, totalLength, 10, 10));
            gfx.DrawRectangle(XBrushes.Black, new XRect(pageEdge + (pageWidth / 6) * 2 + 10, totalLength, 10, 10));
            gfx.DrawRectangle(XBrushes.Black, new XRect(pageEdge + (pageWidth / 6) * 3 + 10, totalLength, 10, 10));
            gfx.DrawRectangle(XBrushes.Black, new XRect(pageEdge + (pageWidth / 6) * 4 + 10, totalLength, 10, 10));
            gfx.DrawRectangle(XBrushes.Black, new XRect(pageEdge + (pageWidth / 6) * 5 + 10, totalLength, 10, 10));
            totalLength += 1;
            gfx.DrawRectangle(XBrushes.White, new XRect(pageEdge + pageWidth / 6 + 11, totalLength, 8, 8));
            tf.DrawString("Venous blood", fontSmall, XBrushes.Black, new XRect(pageEdge + pageWidth / 6 + 30, totalLength, 100, 20));            
            if (bloodForm.IsVenousBlood)
            {
                gfx.DrawString(tick, symbols, XBrushes.Black, pageEdge + pageWidth / 6 + 11, totalLength + 6);
            }
            gfx.DrawRectangle(XBrushes.White, new XRect(pageEdge + (pageWidth / 6) * 2 + 11, totalLength, 8, 8));
            tf.DrawString("Saliva", fontSmall, XBrushes.Black, new XRect(pageEdge + (pageWidth / 6) * 2 + 30, totalLength, 100, 20));
            if (bloodForm.IsSaliva)
            {
                gfx.DrawString(tick, symbols, XBrushes.Black, pageEdge + (pageWidth / 6) * 2 + 11, totalLength + 6);
            }
            gfx.DrawRectangle(XBrushes.White, new XRect(pageEdge + (pageWidth / 6) * 3 + 11, totalLength, 8, 8));
            tf.DrawString("Buccal swab", fontSmall, XBrushes.Black, new XRect(pageEdge + (pageWidth / 6) * 3 + 30, totalLength, 100, 20));
            if (bloodForm.IsBuccalSwab)
            {
                gfx.DrawString(tick, symbols, XBrushes.Black, pageEdge + (pageWidth / 6) * 3 + 11, totalLength + 6);
            }
            gfx.DrawRectangle(XBrushes.White, new XRect(pageEdge + (pageWidth / 6) * 4 + 11, totalLength, 8, 8));
            tf.DrawString("CVS", fontSmall, XBrushes.Black, new XRect(pageEdge + (pageWidth / 6) * 4 + 30, totalLength, 100, 20));
            if (bloodForm.IsCVS)
            {
                gfx.DrawString(tick, symbols, XBrushes.Black, pageEdge + (pageWidth / 6) * 4 + 11, totalLength + 6);
            }
            gfx.DrawRectangle(XBrushes.White, new XRect(pageEdge + (pageWidth / 6) * 5 + 11, totalLength, 8, 8));
            tf.DrawString("Amniotic fluid", fontSmall, XBrushes.Black, new XRect(pageEdge + (pageWidth / 6) * 5 + 30, totalLength, 100, 20));
            if (bloodForm.IsAmnioticFluid)
            {
                gfx.DrawString(tick, symbols, XBrushes.Black, pageEdge + (pageWidth / 6) * 5 + 11, totalLength + 6);
            }
            totalLength += 16;
            gfx.DrawRectangle(XBrushes.Black, new XRect(pageEdge + pageWidth / 6, totalLength, (pageWidth / 6) * 3, 20));
            gfx.DrawRectangle(XBrushes.Black, new XRect(pageEdge + (pageWidth / 6) * 4, totalLength, pageWidth / 6, 20));
            gfx.DrawRectangle(XBrushes.Black, new XRect(pageEdge + (pageWidth / 6) * 5, totalLength, pageWidth / 6, 20));
            totalLength += 1;
            gfx.DrawRectangle(XBrushes.White, new XRect(pageEdge + pageWidth / 6 + 1, totalLength, (pageWidth / 6) * 3 - 2, 18));
            gfx.DrawRectangle(XBrushes.White, new XRect(pageEdge + (pageWidth / 6) * 4 + 1, totalLength, pageWidth / 6 - 2, 18));
            gfx.DrawRectangle(XBrushes.White, new XRect(pageEdge + (pageWidth / 6) * 5 + 1, totalLength, pageWidth / 6 - 2, 18));
            totalLength += 2;
            gfx.DrawRectangle(XBrushes.Black, new XRect(pageEdge + pageWidth / 6 + 10, totalLength, 10, 10));
            gfx.DrawRectangle(XBrushes.Black, new XRect(pageEdge + (pageWidth / 6) * 4 + 10, totalLength, 10, 10));
            gfx.DrawRectangle(XBrushes.Black, new XRect(pageEdge + (pageWidth / 6) * 5 + 10, totalLength, 10, 10));
            totalLength += 1;
            gfx.DrawRectangle(XBrushes.White, new XRect(pageEdge + pageWidth / 6 + 11, totalLength, 8, 8));
            tf.DrawString("Tissue (please specify)", fontSmall, XBrushes.Black, new XRect(pageEdge + pageWidth / 6 + 30, totalLength, 100, 20));
            if (bloodForm.IsTissue)
            {
                gfx.DrawString(tick, symbols, XBrushes.Black, pageEdge + pageWidth / 6 + 11, totalLength + 6);
            }
            gfx.DrawRectangle(XBrushes.White, new XRect(pageEdge + (pageWidth / 6) * 4 + 11, totalLength, 8, 8));
            tf.DrawString("Cord blood", fontSmall, XBrushes.Black, new XRect(pageEdge + (pageWidth / 6) * 4 + 30, totalLength, 100, 20));
            if (bloodForm.IsCordBlood)
            {
                gfx.DrawString(tick, symbols, XBrushes.Black, pageEdge + (pageWidth / 6) * 4 + 11, totalLength + 6);
            }
            gfx.DrawRectangle(XBrushes.White, new XRect(pageEdge + (pageWidth / 6) * 5 + 11, totalLength, 8, 8));
            tf.DrawString("Fetal blood", fontSmall, XBrushes.Black, new XRect(pageEdge + (pageWidth / 6) * 5 + 30, totalLength, 100, 20));
            if (bloodForm.IsFetalBlood)
            {
                gfx.DrawString(tick, symbols, XBrushes.Black, pageEdge + (pageWidth / 6) * 5 + 11, totalLength + 6);
            }
            totalLength += 15;
            gfx.DrawRectangle(XBrushes.Green, new XRect(pageEdge, totalLength, pageWidth, 20));
            totalLength += 2;
            tf.DrawString("FOR INTERNAL GLH LAB USE ONLY", fontBold, XBrushes.White, new XRect(pageEdge + 10, totalLength, 500, 20));
            totalLength += 18;
            

            gfx.DrawRectangle(XBrushes.Black, new XRect(pageEdge, totalLength, (pageWidth / 6) * 2, 60));
            gfx.DrawRectangle(XBrushes.Black, new XRect(pageEdge + (pageWidth / 6) * 2, totalLength, (pageWidth / 6) * 3, 60));
            gfx.DrawRectangle(XBrushes.Black, new XRect(pageEdge + (pageWidth / 6) * 5, totalLength, pageWidth / 6, 60));

            totalLength += 1;
            gfx.DrawRectangle(XBrushes.White, new XRect(pageEdge + 1, totalLength, (pageWidth / 6) * 2 - 2, 58));
            gfx.DrawRectangle(XBrushes.White, new XRect(pageEdge + (pageWidth / 6) * 2 + 1, totalLength, (pageWidth / 6) * 3 - 2, 58));
            gfx.DrawRectangle(XBrushes.White, new XRect(pageEdge + (pageWidth / 6) * 5 + 1, totalLength, pageWidth / 6 - 2, 58));
            totalLength += 1;
            tf.DrawString("Date of receipt:", fontSmallBold, XBrushes.Black, new XRect(pageEdge + 10, totalLength, 100, 20));
            tf.DrawString("Number & volume of specific sample(s) received:", fontSmallBold, XBrushes.Black, new XRect(pageEdge + (pageWidth / 6) * 2 + 10, totalLength, 300, 20));
            tf.DrawString("Place lab reference sticker here:", fontSmallBold, XBrushes.Black, new XRect(pageEdge + (pageWidth / 6) * 5 + 10, totalLength, 80, 20));


            

            //PAGE 2

            int totalLength2 = 15;
            string sampleReqs = "";
            if(bloodForm.SampleRequirements != null)
            {
                sampleReqs = bloodForm.SampleRequirements;
            }

            gfx2.DrawRectangle(XBrushes.Green, new XRect(pageEdge, totalLength2, pageWidth, 20));
            totalLength2 += 2;
            tf2.DrawString("SAMPLE REQUIREMENTS", fontBold, XBrushes.White, new XRect(pageEdge + 10, totalLength2, 500, 20));
            totalLength2 += 18;
            gfx2.DrawRectangle(XBrushes.Black, new XRect(pageEdge, totalLength2, pageWidth, 40));
            totalLength2 += 1;
            gfx2.DrawRectangle(XBrushes.WhiteSmoke, new XRect(pageEdge + 1, totalLength2, pageWidth - 2, 38));
            totalLength2 += 6;
            
            tf2.DrawString("Venous blood", fontSmallUnderlined, XBrushes.Black, new XRect(pageEdge + 10, totalLength2, 100, 20));
            totalLength2 += 9;
            if (sampleReqs.Contains("Molecular"))
            {
                gfx2.DrawRectangle(XBrushes.Yellow, new XRect(pageEdge + 10, totalLength2, 380, 10));
            }
            totalLength2 += 1;
            tf2.DrawString("For molecular genetic testing (e.g. NGS, SNP array, QF-PCR) please send DNA or 3-5ml VB in EDTA.", fontSmall, XBrushes.Black, new XRect(pageEdge + 10, totalLength2, 500, 20));
            totalLength2 += 9;
            if (sampleReqs.Contains("Cytogenetics"))
            {
                gfx2.DrawRectangle(XBrushes.Yellow, new XRect(pageEdge + 10, totalLength2, 350, 10));
            }
            totalLength2 += 1;
            tf2.DrawString("For conventional cytogenetics (e.g. karyotype, FISH) please send 3-5ml VB in lithium heparin.", fontSmall, XBrushes.Black, new XRect(pageEdge + 10, totalLength2, 500, 20));
            totalLength2 += 12;

            gfx2.DrawRectangle(XBrushes.Black, new XRect(pageEdge, totalLength2, pageWidth, 60));
            totalLength2 += 1;
            gfx2.DrawRectangle(XBrushes.WhiteSmoke, new XRect(pageEdge + 1, totalLength2, pageWidth - 2, 58));
            totalLength2 += 6;
            tf2.DrawString("Prenatal", fontSmallUnderlined, XBrushes.Black, new XRect(pageEdge + 10, totalLength2, 100, 20));
            totalLength2 += 12;
            tf2.DrawString("CVS:", fontSmallBold, XBrushes.Black, new XRect(pageEdge + 10, totalLength2, 100, 20));
            if (sampleReqs.Contains("CVS"))
            {
                gfx2.DrawRectangle(XBrushes.Yellow, new XRect(pageEdge + 250, totalLength2, 120, 10));
            }
            totalLength2 += 1;
            tf2.DrawString("10-30mg in transport medium.", fontSmall, XBrushes.Black, new XRect(pageEdge + 250, totalLength2, 150, 20));
            totalLength2 += 9;

            tf2.DrawString("Amniotic fluid:", fontSmallBold, XBrushes.Black, new XRect(pageEdge + 10, totalLength2, 100, 20));
            if (sampleReqs.Contains("Amniotic"))
            {
                gfx2.DrawRectangle(XBrushes.Yellow, new XRect(pageEdge + 250, totalLength2, 120, 10));
            }
            totalLength2 += 1;
            tf2.DrawString("10-20ml in universal container.", fontSmall, XBrushes.Black, new XRect(pageEdge + 250, totalLength2, 150, 20));
            totalLength2 += 9;

            tf2.DrawString("Fetal blood:", fontSmallBold, XBrushes.Black, new XRect(pageEdge + 10, totalLength2, 100, 20));
            if (sampleReqs.Contains("Fetal"))
            {
                gfx2.DrawRectangle(XBrushes.Yellow, new XRect(pageEdge + 250, totalLength2, 150, 10));
            }
            totalLength2 += 1;
            tf2.DrawString("Lithium heparin and EDTA (min 0.5ml).", fontSmall, XBrushes.Black, new XRect(pageEdge + 250, totalLength2, 150, 20));
            totalLength2 += 9;

            tf2.DrawString("Maternal/paternal blood:", fontSmallBold, XBrushes.Black, new XRect(pageEdge + 10, totalLength2, 100, 20));
            if (sampleReqs.Contains("CVS"))
            {
                gfx2.DrawRectangle(XBrushes.Yellow, new XRect(pageEdge + 250, totalLength2, 80, 10));
            }
            totalLength2 += 1;
            tf2.DrawString("3-5 VB in EDTA.", fontSmall, XBrushes.Black, new XRect(pageEdge + 250, totalLength2, 100, 20));
            totalLength2 += 9;

            gfx2.DrawRectangle(XBrushes.Black, new XRect(pageEdge, totalLength2, pageWidth, 60));
            totalLength2 += 1;
            gfx2.DrawRectangle(XBrushes.WhiteSmoke, new XRect(pageEdge + 1, totalLength2, pageWidth - 2, 58));
            totalLength2 += 6;

            tf2.DrawString("Non-invasive prenatal", fontSmallUnderlined, XBrushes.Black, new XRect(pageEdge + 10, totalLength2, 100, 20));
            totalLength2 += 12;

            tf2.DrawString("NIPT", fontSmallBold, XBrushes.Black, new XRect(pageEdge + 10, totalLength2, 100, 20));
            tf2.DrawString("(trisomy 13, 18, 21):", fontSmall, XBrushes.Black, new XRect(pageEdge + 50, totalLength2, 100, 20));
            if (sampleReqs.Contains("NIPT"))
            {
                gfx2.DrawRectangle(XBrushes.Yellow, new XRect(pageEdge + 250, totalLength2, 150, 10));
            }
            totalLength2 += 1;
            tf2.DrawString("10ml maternal blood in Streck BCT tube.", fontSmall, XBrushes.Black, new XRect(pageEdge + 250, totalLength2, 150, 20));
            totalLength2 += 9;
            tf2.DrawString("NIPD", fontSmallBold, XBrushes.Black, new XRect(pageEdge + 10, totalLength2, 100, 20));
            tf2.DrawString("(fetal sexing/single gene disorder):", fontSmall, XBrushes.Black, new XRect(pageEdge + 50, totalLength2, 150, 20));
            if (sampleReqs.Contains("NIPD"))
            {
                gfx2.DrawRectangle(XBrushes.Yellow, new XRect(pageEdge + 250, totalLength2, 180, 10));
            }
            totalLength2 += 1;
            tf2.DrawString("10-20ml maternal blood in Streck BCT tube.", fontSmall, XBrushes.Black, new XRect(pageEdge + 250, totalLength2, 200, 20));
            totalLength2 += 9;
            if (sampleReqs.Contains("NIPD"))
            {
                gfx2.DrawRectangle(XBrushes.Yellow, new XRect(pageEdge + 250, totalLength2, 200, 10));
            }
            tf2.DrawString("Invert Streck tubes x10 and store at room temperature.", fontSmall, XBrushes.Black, new XRect(pageEdge + 250, totalLength2, 200, 20));
            totalLength2 += 10;
            tf2.DrawString("NIPD familial control samples:", fontSmallBold, XBrushes.Black, new XRect(pageEdge + 10, totalLength2, 150, 20));            
            if (sampleReqs.Contains("NIPD fam"))
            {
                gfx2.DrawRectangle(XBrushes.Yellow, new XRect(pageEdge + 250, totalLength2, 120, 10));
            }
            totalLength2 += 1;
            tf2.DrawString("DNA or 3-5ml VB in EDTA.", fontSmall, XBrushes.Black, new XRect(pageEdge + 250, totalLength2, 150, 20));
            totalLength2 += 9;

            gfx2.DrawRectangle(XBrushes.Black, new XRect(pageEdge, totalLength2, pageWidth, 60));
            totalLength2 += 1;
            gfx2.DrawRectangle(XBrushes.WhiteSmoke, new XRect(pageEdge + 1, totalLength2, pageWidth - 2, 58));
            totalLength2 += 6;
            tf2.DrawString("Tissue", fontSmallUnderlined, XBrushes.Black, new XRect(pageEdge + 10, totalLength2, 100, 20));
            totalLength2 += 10;
            tf2.DrawString("Fresh in a sterile container and NOT fixed in formalin.", fontSmall, XBrushes.Black, new XRect(pageEdge + 10, totalLength2, 300, 20));
            totalLength2 += 10;
            tf2.DrawString("POC/Placental biopsy (containing chorionic villi):", fontSmallBold, XBrushes.Black, new XRect(pageEdge + 10, totalLength2, 300, 20));
            if (sampleReqs.Contains("POC"))
            {
                gfx2.DrawRectangle(XBrushes.Yellow, new XRect(pageEdge + 300, totalLength2, 180, 10));
            }
            totalLength2 += 1;
            tf2.DrawString("15mm² in tissue culture medium or sterile saline.", fontSmall, XBrushes.Black, new XRect(pageEdge + 300, totalLength2, 180, 20));
            totalLength2 += 9;
            tf2.DrawString("Fetal or postnatal tissue biopsy (e.g. skin, muscle, cord):", fontSmallBold, XBrushes.Black, new XRect(pageEdge + 10, totalLength2, 300, 20));
            if (sampleReqs.Contains("Fetal"))
            {
                gfx2.DrawRectangle(XBrushes.Yellow, new XRect(pageEdge + 300, totalLength2, 180, 10));
            }
            totalLength2 += 1;
            tf2.DrawString("5mm² in tissue culture medium or sterile saline.", fontSmall, XBrushes.Black, new XRect(pageEdge + 300, totalLength2, 180, 20));
            totalLength2 += 9;
            tf2.DrawString("Cardiac/cord blood:", fontSmallBold, XBrushes.Black, new XRect(pageEdge + 10, totalLength2, 300, 20));
            if (sampleReqs.Contains("Cardiac"))
            {
                gfx2.DrawRectangle(XBrushes.Yellow, new XRect(pageEdge + 300, totalLength2, 60, 10));
            }
            totalLength2 += 1;
            tf2.DrawString("1-2ml in EDTA.", fontSmall, XBrushes.Black, new XRect(pageEdge + 300, totalLength2, 60, 20));
            totalLength2 += 9;

            gfx2.DrawRectangle(XBrushes.Black, new XRect(pageEdge, totalLength2, pageWidth, 30));
            totalLength2 += 1;
            gfx2.DrawRectangle(XBrushes.WhiteSmoke, new XRect(pageEdge + 1, totalLength2, pageWidth - 2, 28));
            totalLength2 += 6;
            tf2.DrawString("SAMPLES SHOULD BE SENT TO THE LABORATORY WITHIN 24 HOURS OR RISK BEING COMPROMISED", fontSmallBold, XBrushes.Black, new XRect(pageEdge + 10, totalLength2, pageWidth, 20));
            totalLength2 += 10;
            tf2.DrawString("Please send via hospital transport, courier or 1st class post.", fontSmall, XBrushes.Black, new XRect(pageEdge + 10, totalLength2, pageWidth, 20));
            totalLength2 += 13;

            gfx2.DrawRectangle(XBrushes.Green, new XRect(pageEdge, totalLength2, pageWidth, 20));
            totalLength2 += 2;
            tf2.DrawString("GENOMICS LAB CONTACT DETAILS", fontBold, XBrushes.White, new XRect(pageEdge + 10, totalLength2, 500, 20));
            totalLength2 += 18;
            gfx2.DrawRectangle(XBrushes.Black, new XRect(pageEdge, totalLength2, pageWidth / 3, 70));
            gfx2.DrawRectangle(XBrushes.Black, new XRect(pageEdge + pageWidth / 3, totalLength2, pageWidth / 3, 70));
            gfx2.DrawRectangle(XBrushes.Black, new XRect(pageEdge + (pageWidth / 3) * 2, totalLength2, pageWidth / 3, 70));
            totalLength2 += 1;
            gfx2.DrawRectangle(XBrushes.WhiteSmoke, new XRect(pageEdge + 1, totalLength2, pageWidth/3 - 2, 68));
            gfx2.DrawRectangle(XBrushes.WhiteSmoke, new XRect(pageEdge + pageWidth / 3 + 1, totalLength2, pageWidth/3 - 2, 68));
            gfx2.DrawRectangle(XBrushes.WhiteSmoke, new XRect(pageEdge + (pageWidth / 3) * 2 + 1, totalLength2, pageWidth/3 - 2, 68));
            totalLength2 += 10;
            tf2.DrawString("SHIPPING ADDRESS", fontSmallBold, XBrushes.Black, new XRect(pageEdge + pageWidth / 3 + 10, totalLength2, 150, 20));
            tf2.DrawString("LABORATORY OPENING TIMES", fontSmallBold, XBrushes.Black, new XRect(pageEdge + (pageWidth / 3) * 2 + 10, totalLength2, 150, 20));
            totalLength2 += 10;
            tf2.DrawString("Tel:", fontSmallBold, XBrushes.Black, new XRect(pageEdge + 10, totalLength2, 150, 20));
            tf2.DrawString("0121 335 8036", fontSmall, XBrushes.Black, new XRect(pageEdge + 50, totalLength2, 150, 20));
            tf2.DrawString("Birmingham Women's Hospital", fontSmall, XBrushes.Black, new XRect(pageEdge + pageWidth / 3 + 10, totalLength2, 200, 20));
            tf2.DrawString("Monday to Friday: 07:00 - 18:00", fontSmall, XBrushes.Black, new XRect(pageEdge + (pageWidth / 3) * 2 + 10, totalLength2, 200, 20));
            totalLength2 += 10;
            tf2.DrawString("Email:", fontSmallBold, XBrushes.Black, new XRect(pageEdge + 10, totalLength2, 150, 20));
            tf2.DrawString("bcw.genetics.lab@nhs.net", fontSmallUnderlined, XBrushes.Blue, new XRect(pageEdge + 50, totalLength2, 150, 20));
            tf2.DrawString("Genetics Laboratory - Ground Floor", fontSmall, XBrushes.Black, new XRect(pageEdge + pageWidth / 3 + 10, totalLength2, 200, 20));
            tf2.DrawString("Saturday: 09:00 - 14:00", fontSmall, XBrushes.Black, new XRect(pageEdge + (pageWidth / 3) * 2 + 10, totalLength2, 200, 20));
            totalLength2 += 10;
            tf2.DrawString("Mindelsohn Way | Edgbaston | Birmingham |", fontSmall, XBrushes.Black, new XRect(pageEdge + pageWidth / 3 + 10, totalLength2, 200, 20));
            totalLength2 += 10;
            tf2.DrawString("B15 2TG", fontSmall, XBrushes.Black, new XRect(pageEdge + pageWidth / 3 + 10, totalLength2, 200, 20));
            totalLength2 += 19;
            gfx2.DrawRectangle(XBrushes.Green, new XRect(pageEdge, totalLength2, pageWidth, 20));
            totalLength2 += 2;
            tf2.DrawString("TRIAGE DETAILS (FOR INTERNAL GLH LAB USE ONLY)", fontBold, XBrushes.White, new XRect(pageEdge + 10, totalLength2, 500, 20));
            totalLength2 += 18;
            gfx2.DrawRectangle(XBrushes.Black, new XRect(pageEdge, totalLength2, pageWidth / 6, 16));
            gfx2.DrawRectangle(XBrushes.Black, new XRect(pageEdge + pageWidth / 6, totalLength2, pageWidth / 6, 16));
            gfx2.DrawRectangle(XBrushes.Black, new XRect(pageEdge + (pageWidth / 6) * 2, totalLength2, pageWidth / 6, 16));
            gfx2.DrawRectangle(XBrushes.Black, new XRect(pageEdge + (pageWidth / 6) * 3, totalLength2, pageWidth / 6, 16));
            gfx2.DrawRectangle(XBrushes.Black, new XRect(pageEdge + (pageWidth / 6) * 4, totalLength2, pageWidth / 6, 16));
            gfx2.DrawRectangle(XBrushes.Black, new XRect(pageEdge + (pageWidth / 6) * 5, totalLength2, pageWidth / 6, 16));
            totalLength2 += 1;
            gfx2.DrawRectangle(XBrushes.LightGray, new XRect(pageEdge + 1, totalLength2, pageWidth / 6 - 2, 14));
            gfx2.DrawRectangle(XBrushes.WhiteSmoke, new XRect(pageEdge + pageWidth / 6 + 1, totalLength2, pageWidth / 6 - 2, 14));
            gfx2.DrawRectangle(XBrushes.WhiteSmoke, new XRect(pageEdge + (pageWidth / 6) * 2 + 1, totalLength2, pageWidth / 6 - 2, 14));
            gfx2.DrawRectangle(XBrushes.WhiteSmoke, new XRect(pageEdge + (pageWidth / 6) * 3 + 1, totalLength2, pageWidth / 6 - 2, 14));
            gfx2.DrawRectangle(XBrushes.WhiteSmoke, new XRect(pageEdge + (pageWidth / 6) * 4 + 1, totalLength2, pageWidth / 6 - 2, 14));
            gfx2.DrawRectangle(XBrushes.WhiteSmoke, new XRect(pageEdge + (pageWidth / 6) * 5 + 1, totalLength2, pageWidth / 6 - 2, 14));
            totalLength2 += 3;
            tf2.DrawString("Priority", fontSmallBold, XBrushes.Black, new XRect(pageEdge + 10, totalLength2, 100, 20));
            tf2.DrawString("Prenatal", fontSmallBold, XBrushes.Black, new XRect(pageEdge + pageWidth / 6 + 25, totalLength2, 100, 20));
            tf2.DrawString("Urgent", fontSmallBold, XBrushes.Black, new XRect(pageEdge + (pageWidth / 6) * 2 + 25, totalLength2, 100, 20));
            tf2.DrawString("Priority", fontSmallBold, XBrushes.Black, new XRect(pageEdge + (pageWidth / 6) * 3 + 25, totalLength2, 100, 20));
            tf2.DrawString("Routine", fontSmallBold, XBrushes.Black, new XRect(pageEdge + (pageWidth / 6) * 4 + 25, totalLength2, 100, 20));
            tf2.DrawString("Research", fontSmallBold, XBrushes.Black, new XRect(pageEdge + (pageWidth / 6) * 5 + 25, totalLength2, 100, 20));
            totalLength2 += 10;
            gfx2.DrawRectangle(XBrushes.Black, new XRect(pageEdge, totalLength2, pageWidth / 6, 16));
            gfx2.DrawRectangle(XBrushes.Black, new XRect(pageEdge + pageWidth / 6, totalLength2, (pageWidth / 6) * 5, 16));
            totalLength2 += 1;
            gfx2.DrawRectangle(XBrushes.LightGray, new XRect(pageEdge + 1, totalLength2, pageWidth / 6 - 2, 14));
            gfx2.DrawRectangle(XBrushes.WhiteSmoke, new XRect(pageEdge + pageWidth / 6 + 1, totalLength2, (pageWidth / 6) * 5 - 2, 14));
            totalLength2 += 3;
            tf2.DrawString("Culture indication", fontSmallBold, XBrushes.Black, new XRect(pageEdge + 10, totalLength2, 100, 20));
            totalLength2 += 11;
            gfx2.DrawRectangle(XBrushes.Black, new XRect(pageEdge, totalLength2, pageWidth / 6, 16));
            gfx2.DrawRectangle(XBrushes.Black, new XRect(pageEdge + pageWidth / 6, totalLength2, pageWidth / 6, 16));
            gfx2.DrawRectangle(XBrushes.Black, new XRect(pageEdge + (pageWidth / 6) * 2, totalLength2, pageWidth / 12, 16));
            gfx2.DrawRectangle(XBrushes.Black, new XRect(pageEdge + (pageWidth / 6) * 2 + pageWidth / 12, totalLength2, pageWidth / 12, 16));
            gfx2.DrawRectangle(XBrushes.Black, new XRect(pageEdge + (pageWidth / 6) * 3 - 1, totalLength2, pageWidth / 6 + 1, 16)); //because there has to be a gap!!
            gfx2.DrawRectangle(XBrushes.Black, new XRect(pageEdge + (pageWidth / 6) * 4, totalLength2, pageWidth / 6 + 1, 16));
            gfx2.DrawRectangle(XBrushes.Black, new XRect(pageEdge + (pageWidth / 6) * 5 + 1, totalLength2, pageWidth / 12, 16));
            gfx2.DrawRectangle(XBrushes.Black, new XRect(pageEdge + (pageWidth / 6) * 5 + pageWidth / 12 + 1, totalLength2, pageWidth / 12, 16));
            totalLength2 += 1;
            gfx2.DrawRectangle(XBrushes.LightGray, new XRect(pageEdge + 1, totalLength2, pageWidth / 6 - 2, 14));
            gfx2.DrawRectangle(XBrushes.WhiteSmoke, new XRect(pageEdge + pageWidth / 6 + 1, totalLength2, pageWidth / 6 - 2, 14));
            gfx2.DrawRectangle(XBrushes.WhiteSmoke, new XRect(pageEdge + (pageWidth / 6) * 2 + 1, totalLength2, pageWidth / 12 - 2, 14));
            gfx2.DrawRectangle(XBrushes.WhiteSmoke, new XRect(pageEdge + (pageWidth / 6) * 2 + pageWidth / 12 + 1, totalLength2, pageWidth / 12 - 1, 14));
            gfx2.DrawRectangle(XBrushes.LightGray, new XRect(pageEdge + (pageWidth / 6) * 3 + 1, totalLength2, pageWidth / 6 - 2, 14));
            gfx2.DrawRectangle(XBrushes.WhiteSmoke, new XRect(pageEdge + (pageWidth / 6) * 4 + 1, totalLength2, pageWidth / 6 - 2, 14));
            gfx2.DrawRectangle(XBrushes.WhiteSmoke, new XRect(pageEdge + (pageWidth / 6) * 5 + 1, totalLength2, pageWidth / 12 - 2, 14));
            gfx2.DrawRectangle(XBrushes.WhiteSmoke, new XRect(pageEdge + (pageWidth / 6) * 5 + pageWidth / 12 + 1, totalLength2, pageWidth / 12 - 2, 14));
            totalLength2 += 3;
            tf2.DrawString("Culture 1", fontSmallBold, XBrushes.Black, new XRect(pageEdge + 10, totalLength2, 100, 20));
            tf2.DrawString("S", fontSmall, XBrushes.Black, new XRect(pageEdge + pageWidth / 6 + 40, totalLength2, 100, 20));
            tf2.DrawString("48", fontSmall, XBrushes.Black, new XRect(pageEdge + (pageWidth / 6) * 2 + 20, totalLength2, 100, 20));
            tf2.DrawString("72", fontSmall, XBrushes.Black, new XRect(pageEdge + (pageWidth / 6) * 2 + pageWidth / 12 + 20, totalLength2, 100, 20));
            tf2.DrawString("Culture 2", fontSmallBold, XBrushes.Black, new XRect(pageEdge + (pageWidth / 6) * 3 + 10, totalLength2, 100, 20));
            tf2.DrawString("S", fontSmall, XBrushes.Black, new XRect(pageEdge + (pageWidth / 6) * 4 + 40, totalLength2, 100, 20));
            tf2.DrawString("48", fontSmall, XBrushes.Black, new XRect(pageEdge + (pageWidth / 6) * 5 + 20, totalLength2, 100, 20));
            tf2.DrawString("72", fontSmall, XBrushes.Black, new XRect(pageEdge + (pageWidth / 6) * 5 + pageWidth / 12 + 20, totalLength2, 100, 20));
            totalLength2 += 11;
            gfx2.DrawRectangle(XBrushes.Black, new XRect(pageEdge, totalLength2, pageWidth / 6, 32));
            gfx2.DrawRectangle(XBrushes.Black, new XRect(pageEdge + pageWidth / 6, totalLength2, pageWidth / 6, 16));
            gfx2.DrawRectangle(XBrushes.Black, new XRect(pageEdge + (pageWidth / 6) * 2, totalLength2, pageWidth / 6, 16));
            gfx2.DrawRectangle(XBrushes.Black, new XRect(pageEdge + (pageWidth / 6) * 3, totalLength2, pageWidth / 6, 32));
            gfx2.DrawRectangle(XBrushes.Black, new XRect(pageEdge + (pageWidth / 6) * 4, totalLength2, pageWidth / 6, 16));
            gfx2.DrawRectangle(XBrushes.Black, new XRect(pageEdge + (pageWidth / 6) * 5, totalLength2, pageWidth / 6, 16));
            totalLength2 += 1;
            gfx2.DrawRectangle(XBrushes.LightGray, new XRect(pageEdge + 1, totalLength2, pageWidth / 6 - 2, 30));
            gfx2.DrawRectangle(XBrushes.WhiteSmoke, new XRect(pageEdge + pageWidth / 6 + 1, totalLength2, pageWidth / 6 - 2, 14));
            gfx2.DrawRectangle(XBrushes.WhiteSmoke, new XRect(pageEdge + (pageWidth / 6) * 2 + 1, totalLength2, pageWidth / 6 - 2, 14));
            gfx2.DrawRectangle(XBrushes.LightGray, new XRect(pageEdge + (pageWidth / 6) * 3 + 1, totalLength2, pageWidth / 6 - 2, 30));
            gfx2.DrawRectangle(XBrushes.WhiteSmoke, new XRect(pageEdge + (pageWidth / 6) * 4 + 1, totalLength2, pageWidth / 6 - 2, 14));
            gfx2.DrawRectangle(XBrushes.WhiteSmoke, new XRect(pageEdge + (pageWidth / 6) * 5 + 1, totalLength2, pageWidth / 6 - 2, 14));
            totalLength2 += 3;
            tf2.DrawString("G band", fontSmall, XBrushes.Black, new XRect(pageEdge + pageWidth / 6 + 30, totalLength2, 100, 20));
            tf2.DrawString("Banking", fontSmall, XBrushes.Black, new XRect(pageEdge + (pageWidth / 6) * 2 + 30, totalLength2, 100, 20));
            tf2.DrawString("G band", fontSmall, XBrushes.Black, new XRect(pageEdge + (pageWidth / 6) * 4 + 30, totalLength2, 100, 20));
            tf2.DrawString("Banking", fontSmall, XBrushes.Black, new XRect(pageEdge + (pageWidth / 6) * 5 + 30, totalLength2, 100, 20));
            totalLength2 += 6;
            tf2.DrawString("Staining", fontSmallBold, XBrushes.Black, new XRect(pageEdge + 10, totalLength2, 100, 20));
            tf2.DrawString("Staining", fontSmallBold, XBrushes.Black, new XRect(pageEdge + (pageWidth / 6) * 3 + 10, totalLength2, 100, 20));
            totalLength2 += 6;
            gfx2.DrawRectangle(XBrushes.Black, new XRect(pageEdge + pageWidth / 6, totalLength2, pageWidth / 6, 16));
            gfx2.DrawRectangle(XBrushes.Black, new XRect(pageEdge + (pageWidth / 6) * 2, totalLength2, pageWidth / 6, 16));            
            gfx2.DrawRectangle(XBrushes.Black, new XRect(pageEdge + (pageWidth / 6) * 4, totalLength2, pageWidth / 6, 16));
            gfx2.DrawRectangle(XBrushes.Black, new XRect(pageEdge + (pageWidth / 6) * 5, totalLength2, pageWidth / 6, 16));
            totalLength += 1;
            gfx2.DrawRectangle(XBrushes.WhiteSmoke, new XRect(pageEdge + pageWidth / 6 + 1, totalLength2, pageWidth / 6 - 2, 14));
            gfx2.DrawRectangle(XBrushes.WhiteSmoke, new XRect(pageEdge + (pageWidth / 6) * 2 + 1, totalLength2, pageWidth / 6 - 2, 14));            
            gfx2.DrawRectangle(XBrushes.WhiteSmoke, new XRect(pageEdge + (pageWidth / 6) * 4 + 1, totalLength2, pageWidth / 6 - 2, 14));
            gfx2.DrawRectangle(XBrushes.WhiteSmoke, new XRect(pageEdge + (pageWidth / 6) * 5 + 1, totalLength2, pageWidth / 6 - 2, 14));
            totalLength2 += 3;
            tf2.DrawString("No Cult", fontSmall, XBrushes.Black, new XRect(pageEdge + pageWidth / 6 + 30, totalLength2, 100, 20));
            tf2.DrawString("FISH", fontSmall, XBrushes.Black, new XRect(pageEdge + (pageWidth / 6) * 2 + 30, totalLength2, 100, 20));
            tf2.DrawString("No Cult", fontSmall, XBrushes.Black, new XRect(pageEdge + (pageWidth / 6) * 4 + 30, totalLength2, 100, 20));
            tf2.DrawString("FISH", fontSmall, XBrushes.Black, new XRect(pageEdge + (pageWidth / 6) * 5 + 30, totalLength2, 100, 20));
            totalLength2 += 12;
            gfx2.DrawRectangle(XBrushes.Black, new XRect(pageEdge, totalLength2, pageWidth / 6, 16));
            gfx2.DrawRectangle(XBrushes.Black, new XRect(pageEdge + pageWidth / 6, totalLength2, (pageWidth / 6) * 2, 16));
            gfx2.DrawRectangle(XBrushes.Black, new XRect(pageEdge + (pageWidth / 6) * 3, totalLength2, pageWidth / 6, 16));
            gfx2.DrawRectangle(XBrushes.Black, new XRect(pageEdge + (pageWidth / 6) * 4, totalLength2, (pageWidth / 6) * 2, 16));
            totalLength2 += 1;
            gfx2.DrawRectangle(XBrushes.LightGray, new XRect(pageEdge + 1, totalLength2, pageWidth / 6 - 2, 14));
            gfx2.DrawRectangle(XBrushes.WhiteSmoke, new XRect(pageEdge + pageWidth / 6 + 1, totalLength2, (pageWidth / 6) * 2 - 2, 14));
            gfx2.DrawRectangle(XBrushes.LightGray, new XRect(pageEdge + (pageWidth / 6) * 3 + 1, totalLength2, pageWidth / 6 - 2, 14));
            gfx2.DrawRectangle(XBrushes.WhiteSmoke, new XRect(pageEdge + (pageWidth / 6) * 4 + 1, totalLength2, (pageWidth / 6) * 2 - 2, 14));
            totalLength2 += 3;
            tf2.DrawString("DNA indication 1", fontSmall, XBrushes.Black, new XRect(pageEdge + 10, totalLength2, 100, 20));
            tf2.DrawString("Reason", fontSmall, XBrushes.Black, new XRect(pageEdge + (pageWidth / 6) * 3 + 10, totalLength2, 100, 20));
            totalLength2 += 11;
            gfx2.DrawRectangle(XBrushes.Black, new XRect(pageEdge, totalLength2, pageWidth / 6, 16));
            gfx2.DrawRectangle(XBrushes.Black, new XRect(pageEdge + pageWidth / 6, totalLength2, (pageWidth / 6) * 2, 16));
            gfx2.DrawRectangle(XBrushes.Black, new XRect(pageEdge + (pageWidth / 6) * 3, totalLength2, pageWidth / 6, 16));
            gfx2.DrawRectangle(XBrushes.Black, new XRect(pageEdge + (pageWidth / 6) * 4, totalLength2, (pageWidth / 6) * 2, 16));
            totalLength2 += 1;
            gfx2.DrawRectangle(XBrushes.LightGray, new XRect(pageEdge + 1, totalLength2, pageWidth / 6 - 2, 14));
            gfx2.DrawRectangle(XBrushes.WhiteSmoke, new XRect(pageEdge + pageWidth / 6 + 1, totalLength2, (pageWidth / 6) * 2 - 2, 14));
            gfx2.DrawRectangle(XBrushes.LightGray, new XRect(pageEdge + (pageWidth / 6) * 3 + 1, totalLength2, pageWidth / 6 - 2, 14));
            gfx2.DrawRectangle(XBrushes.WhiteSmoke, new XRect(pageEdge + (pageWidth / 6) * 4 + 1, totalLength2, (pageWidth / 6) * 2 - 2, 14));
            totalLength2 += 3;
            tf2.DrawString("R code", fontSmall, XBrushes.Black, new XRect(pageEdge + 10, totalLength2, 100, 20));
            tf2.DrawString("Testing lab", fontSmall, XBrushes.Black, new XRect(pageEdge + (pageWidth / 6) * 3 + 10, totalLength2, 100, 20));
            totalLength2 += 11;
            gfx2.DrawRectangle(XBrushes.Black, new XRect(pageEdge, totalLength2, pageWidth / 6, 16));
            gfx2.DrawRectangle(XBrushes.Black, new XRect(pageEdge + pageWidth / 6, totalLength2, (pageWidth / 6) * 2, 16));
            gfx2.DrawRectangle(XBrushes.Black, new XRect(pageEdge + (pageWidth / 6) * 3, totalLength2, pageWidth / 6, 16));
            gfx2.DrawRectangle(XBrushes.Black, new XRect(pageEdge + (pageWidth / 6) * 4, totalLength2, (pageWidth / 6) * 2, 16));
            totalLength2 += 1;
            gfx2.DrawRectangle(XBrushes.LightGray, new XRect(pageEdge + 1, totalLength2, pageWidth / 6 - 2, 14));
            gfx2.DrawRectangle(XBrushes.WhiteSmoke, new XRect(pageEdge + pageWidth / 6 + 1, totalLength2, (pageWidth / 6) * 2 - 2, 14));
            gfx2.DrawRectangle(XBrushes.LightGray, new XRect(pageEdge + (pageWidth / 6) * 3 + 1, totalLength2, pageWidth / 6 - 2, 14));
            gfx2.DrawRectangle(XBrushes.WhiteSmoke, new XRect(pageEdge + (pageWidth / 6) * 4 + 1, totalLength2, (pageWidth / 6) * 2 - 2, 14));
            totalLength2 += 3;
            tf2.DrawString("DNA indication 2", fontSmall, XBrushes.Black, new XRect(pageEdge + 10, totalLength2, 100, 20));
            tf2.DrawString("Reason", fontSmall, XBrushes.Black, new XRect(pageEdge + (pageWidth / 6) * 3 + 10, totalLength2, 100, 20));
            totalLength2 += 11;
            gfx2.DrawRectangle(XBrushes.Black, new XRect(pageEdge, totalLength2, pageWidth / 6, 16));
            gfx2.DrawRectangle(XBrushes.Black, new XRect(pageEdge + pageWidth / 6, totalLength2, (pageWidth / 6) * 2, 16));
            gfx2.DrawRectangle(XBrushes.Black, new XRect(pageEdge + (pageWidth / 6) * 3, totalLength2, pageWidth / 6, 16));
            gfx2.DrawRectangle(XBrushes.Black, new XRect(pageEdge + (pageWidth / 6) * 4, totalLength2, (pageWidth / 6) * 2, 16));
            totalLength2 += 1;
            gfx2.DrawRectangle(XBrushes.LightGray, new XRect(pageEdge + 1, totalLength2, pageWidth / 6 - 2, 14));
            gfx2.DrawRectangle(XBrushes.WhiteSmoke, new XRect(pageEdge + pageWidth / 6 + 1, totalLength2, (pageWidth / 6) * 2 - 2, 14));
            gfx2.DrawRectangle(XBrushes.LightGray, new XRect(pageEdge + (pageWidth / 6) * 3 + 1, totalLength2, pageWidth / 6 - 2, 14));
            gfx2.DrawRectangle(XBrushes.WhiteSmoke, new XRect(pageEdge + (pageWidth / 6) * 4 + 1, totalLength2, (pageWidth / 6) * 2 - 2, 14));
            totalLength2 += 3;
            tf2.DrawString("R code", fontSmall, XBrushes.Black, new XRect(pageEdge + 10, totalLength2, 100, 20));
            tf2.DrawString("Testing lab", fontSmall, XBrushes.Black, new XRect(pageEdge + (pageWidth / 6) * 3 + 10, totalLength2, 100, 20));
            totalLength2 += 11;
            gfx2.DrawRectangle(XBrushes.Black, new XRect(pageEdge, totalLength2, pageWidth / 6, 16));
            gfx2.DrawRectangle(XBrushes.Black, new XRect(pageEdge + pageWidth / 6, totalLength2, (pageWidth / 6) * 5, 16));            
            totalLength2 += 1;
            gfx2.DrawRectangle(XBrushes.LightGray, new XRect(pageEdge + 1, totalLength2, pageWidth / 6 - 2, 14));
            gfx2.DrawRectangle(XBrushes.WhiteSmoke, new XRect(pageEdge + pageWidth / 6 + 1, totalLength2, (pageWidth / 6) * 5 - 2, 14));            
            totalLength2 += 3;
            tf2.DrawString("Extraction/OD type", fontSmall, XBrushes.Black, new XRect(pageEdge + 10, totalLength2, 100, 20));            
            totalLength2 += 11;
            gfx2.DrawRectangle(XBrushes.Black, new XRect(pageEdge, totalLength2, pageWidth / 6, 30));
            gfx2.DrawRectangle(XBrushes.Black, new XRect(pageEdge + pageWidth / 6, totalLength2, (pageWidth / 6) * 5, 30));
            totalLength2 += 1;
            gfx2.DrawRectangle(XBrushes.LightGray, new XRect(pageEdge + 1, totalLength2, pageWidth / 6 - 2, 28));
            gfx2.DrawRectangle(XBrushes.WhiteSmoke, new XRect(pageEdge + pageWidth / 6 + 1, totalLength2, (pageWidth / 6) * 5 - 2, 28));
            totalLength2 += 3;
            tf2.DrawString("Comments", fontSmall, XBrushes.Black, new XRect(pageEdge + 10, totalLength2, 100, 20));             
            totalLength2 += 11;


            document.Save(Path.Combine(Directory.GetCurrentDirectory(), $"wwwroot\\StandardLetterPreviews\\bloodform-{user}.pdf"));

            if(!isPreview.GetValueOrDefault()) //send to clinicians' VOT (depending on name and job title)
            {
                string votFolder = "";

                if(user == "mnln") { votFolder = "Martin Loxton (Test)"; }
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
                System.IO.File.Copy($"wwwroot\\StandardLetterPreviews\\bloodform-{user}.pdf", $@"\\zion.matrix.local\dfsrootbwh\cling\Virtual Out-trays (letter enclosures)\{votFolder}\Blood Form_{patient.CGU_No}.pdf");
            }
        }
    }
}
    

