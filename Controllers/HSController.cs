using ClinicalXPDataConnections.Meta;
using ClinicalXPDataConnections.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Office.Interop.Outlook;
using Microsoft.SqlServer.Server;
using MigraDoc.DocumentObjectModel;
using MigraDoc.DocumentObjectModel.Tables;
using MigraDoc.Rendering;

namespace ClinicX.Controllers
{
    public class HSController : Controller
    {
        private readonly IConfiguration _config;
        private readonly IStaffUserDataAsync _staffUser;
        private readonly IPatientDataAsync _patientData;
        private readonly IReferralDataAsync _referralData;
        private readonly ITriageDataAsync _triageData;
        private readonly IConstantsDataAsync _constantsData;
        private readonly IRelativeDataAsync _relData;
        private readonly IRelativeDiagnosisDataAsync _relDiag;

        public HSController(IConfiguration config, IStaffUserDataAsync staffUserData, IPatientDataAsync patientData, IReferralDataAsync referralData, ITriageDataAsync triageData,
            IConstantsDataAsync constantsData, IRelativeDataAsync relData, IRelativeDiagnosisDataAsync relDiag)
        {
            _staffUser = staffUserData;
            _patientData = patientData;
            _referralData = referralData;
            _triageData = triageData;
            _constantsData = constantsData;
            _relData = relData;
            _relDiag = relDiag;
        }

        public async Task<IActionResult> PrepareHS(int id, int diaryID, bool isPreview)
        {
            var triage = await _triageData.GetCancerICPDetails(id);
            var patient = await _patientData.GetPatientDetails(triage.MPI);
            bool hsDone = await DoHS(id, diaryID, User.Identity.Name, isPreview); //should return success (or failure if something goes wrong in the creation process)

            string message = "";

            if (hsDone) { message = "HS has been created for filing in EDMS"; }
            else { message = "HS failed"; }

            if (isPreview)
            {
                return File($"~/StandardLetterPreviews/preview-{User.Identity.Name}.pdf", "Application/PDF");
            }
            else
            {
                return RedirectToAction("Index", "LetterMenu", new { mpi = patient.MPI, message = message, success = true });
            }
        }

        public async Task<bool> DoHS(int icpID, int diaryID, string user, bool isPreview)
        {
            bool success = false;

            StaffMember staffMember = new StaffMember();
            staffMember = await _staffUser.GetStaffMemberDetails(user);
            Patient patient = new Patient();

            //ITriageData triageData = new TriageData(_clinContext);
            ICPCancer icpc = await _triageData.GetCancerICPDetails(icpID);
            ICP icp = await _triageData.GetICPDetails(icpc.ICPID);
            Referral referral = await _referralData.GetReferralDetails(icp.REFID);
            patient = await _patientData.GetPatientDetails(referral.MPI);

            MigraDoc.DocumentObjectModel.Document document = new MigraDoc.DocumentObjectModel.Document();

            Section section = document.AddSection();

            section.PageSetup.LeftMargin = 60;
            section.PageSetup.RightMargin = 60;
            section.PageSetup.TopMargin = 40;
            section.PageSetup.BottomMargin = 40;

            MigraDoc.DocumentObjectModel.Tables.Table table = section.AddTable();
            MigraDoc.DocumentObjectModel.Tables.Column reportHeader = table.AddColumn();
            MigraDoc.DocumentObjectModel.Tables.Column logo = table.AddColumn();
            reportHeader.Format.Alignment = ParagraphAlignment.Left;
            logo.Format.Alignment = ParagraphAlignment.Right;

            MigraDoc.DocumentObjectModel.Tables.Row row1 = table.AddRow();
            row1.VerticalAlignment = MigraDoc.DocumentObjectModel.Tables.VerticalAlignment.Top;
            row1.Cells[0].AddParagraph().AddFormattedText($"West Midlands Family Cancer Strategy", TextFormat.Bold);

            MigraDoc.DocumentObjectModel.Tables.Row row2 = table.AddRow();
            row2.VerticalAlignment = MigraDoc.DocumentObjectModel.Tables.VerticalAlignment.Center;

            

            row2.Cells[0].AddParagraph().AddFormattedText("Cancer registry search results", TextFormat.Bold);
            row2.Cells[0].Format.Font.Color = Colors.Gray;

            //MigraDoc.DocumentObjectModel.Shapes.Image imgLogo = row1.Cells[1].AddImage(@"wwwroot\Letterhead.jpg");
            //imgLogo.ScaleWidth = new Unit(0.5, UnitType.Point);
            //imgLogo.ScaleHeight = new Unit(0.5, UnitType.Point);

            table.Columns.Width = 240;
            reportHeader.Width = 300;
            logo.Width = 180;
            row1.Height = 20;
            row2.Height = 20;
            row1.Format.Font.Size = 12;
            row2.Format.Font.Size = 14;

            Paragraph spacer = section.AddParagraph();

            string patientText = $"{patient.FIRSTNAME} {patient.LASTNAME} {patient.DOB.Value.ToString("dd/MM/yyyy")}";
            string patientText2 = $"CGU No: {patient.CGU_No}   WMFACS ID: {patient.WMFACSID}";

            Paragraph patientData = section.AddParagraph();
            patientData.AddFormattedText(patientText, TextFormat.Bold);
            patientData.Format.Font.Color = Colors.Blue;
            spacer = section.AddParagraph();
            Paragraph patientData2 = section.AddParagraph();
            patientData2.AddFormattedText(patientText2, TextFormat.Bold);
            spacer = section.AddParagraph();
            Paragraph consData = section.AddParagraph();
            consData.AddFormattedText($"{referral.LeadClinician}", TextFormat.Bold);
            spacer = section.AddParagraph();
            Paragraph headjfgf = section.AddParagraph();
            headjfgf.AddFormattedText("Cancer registry records for the following relatives of the above patient have been obtained:", TextFormat.Bold);

            List<Relative> relsList = new List<Relative>();

            /*
            relsList = await _relData.GetRelativesList(patient.MPI);

            List<Patient> patients = await _patientData.GetMatchingPatientsByPedNo(patient.PEDNO);

            foreach(var pat in patients)
            {
                if (pat.MPI != patient.MPI) //don't want to add the main patient's relatives twice!!!
                {
                    List<Relative> pedigreeRels = await _relData.GetRelativesListForPatient(pat.WMFACSID);

                    foreach (var pedRel in pedigreeRels)
                    {                        
                        
                        if (!relsList.Any(r => r.relsid == pedRel.relsid)) //only add if not already in the list
                        {
                            relsList.Add(pedRel);
                        }
                    }
                }
            }
            */

            relsList = await _relData.GetRelativesListForFamily(patient.PEDNO);

            relsList = relsList.OrderBy(r => r.RelSurname).ToList(); //apparently the CGU_DB one seems to be sorted by surname...

            foreach (var item in relsList)
            {
                List<RelativesDiagnosis> diagsForRel = await _relDiag.GetRelativeDiagnosisList(item.relsid);
                                
                diagsForRel = diagsForRel.Where(d => d.Confirmed == "Y").ToList();

                if (diagsForRel.Count > 0)
                {
                    MigraDoc.DocumentObjectModel.Tables.Table tableRelDiag = section.AddTable();
                    //tableRelDiag.Borders.Visible = true;
                    MigraDoc.DocumentObjectModel.Tables.Column col1 = tableRelDiag.AddColumn();
                    MigraDoc.DocumentObjectModel.Tables.Column col2 = tableRelDiag.AddColumn();
                    MigraDoc.DocumentObjectModel.Tables.Column col3 = tableRelDiag.AddColumn();
                    MigraDoc.DocumentObjectModel.Tables.Column col4 = tableRelDiag.AddColumn();

                    MigraDoc.DocumentObjectModel.Tables.Row r1 = tableRelDiag.AddRow();
                    MigraDoc.DocumentObjectModel.Tables.Row r2 = tableRelDiag.AddRow();
                    MigraDoc.DocumentObjectModel.Tables.Row r3 = tableRelDiag.AddRow();
                    MigraDoc.DocumentObjectModel.Tables.Row r4 = tableRelDiag.AddRow();
                    MigraDoc.DocumentObjectModel.Tables.Row r5 = tableRelDiag.AddRow();
                    MigraDoc.DocumentObjectModel.Tables.Row r6 = tableRelDiag.AddRow();
                    MigraDoc.DocumentObjectModel.Tables.Row r7 = tableRelDiag.AddRow();
                    MigraDoc.DocumentObjectModel.Tables.Row r8 = tableRelDiag.AddRow();

                    tableRelDiag.Rows.Height = 15;

                    col1.Width = 100;
                    col2.Width = 220;
                    col3.Width = 100;
                    col4.Width = 100;
                
                    col1.Format.Alignment = ParagraphAlignment.Right;
                    col3.Format.Alignment = ParagraphAlignment.Right;

                    r1.Cells[0].AddParagraph().AddFormattedText("Name:", TextFormat.Bold);
                    r1.Cells[1].AddParagraph().AddFormattedText($"{item.RelSurname}, {item.RelForename1}", TextFormat.Bold);
                    r1.Cells[2].AddParagraph().AddFormattedText("Date of birth:", TextFormat.Bold);
                    string dobString = "";
                    if(item.DOB != null) { dobString = item.DOB.Value.ToString("dd/MM/yy"); }
                    r1.Cells[3].AddParagraph(dobString);
    
                    r2.Cells[0].AddParagraph().AddFormattedText("Address:", TextFormat.Bold);
                    string addressString = "";
                    if(item.RelAdd1 != null) { addressString += item.RelAdd1; }
                    if (item.RelAdd2 != null) { addressString += ", " + item.RelAdd2; }
                    if (item.RelAdd3 != null) { addressString += ", " + item.RelAdd3; }
                    if (item.RelAdd4 != null) { addressString += ", " + item.RelAdd4; }
                    if (item.RelPC1 != null) { addressString += ", " + item.RelPC1; }
                    r2.Cells[1].AddParagraph(addressString);
                    r2.Cells[2].AddParagraph().AddFormattedText("Date of death:", TextFormat.Bold);
                    string dodString = "";
                    if (item.DOD != null) { dodString = item.DOD.Value.ToString("dd/MM/yy"); }
                    r2.Cells[3].AddParagraph(dodString);

                    r3.Cells[2].AddParagraph().AddFormattedText("Comments:", TextFormat.Bold);
                    if(item.Notes != null) { r3.Cells[3].AddParagraph(item.Notes); }

                    r4.Cells[0].AddParagraph().AddFormattedText("Diagnosis Date:", TextFormat.Bold);
                    r5.Cells[0].AddParagraph().AddFormattedText("Cancer Registry:", TextFormat.Bold);
                    r6.Cells[0].AddParagraph().AddFormattedText("Site", TextFormat.Bold);
                    r7.Cells[0].AddParagraph().AddFormattedText("Side:", TextFormat.Bold);
                    r8.Cells[0].AddParagraph().AddFormattedText("Morphology:", TextFormat.Bold);

                    string diagDate = item.ConfDiagDate ?? "";

                    r4.Cells[1].AddParagraph($"{diagDate} age: {item.ConfDiagAge}");
                    r5.Cells[1].AddParagraph(item.Registry ?? "");
                    r6.Cells[1].AddParagraph(item.Site ?? "");
                    r7.Cells[1].AddParagraph(item.Lat ?? "");
                    r8.Cells[1].AddParagraph(item.Morph ?? "");

                    tableRelDiag.SetEdge(0,
                        0,
                        tableRelDiag.Columns.Count,
                        tableRelDiag.Rows.Count,
                        Edge.Box,
                        BorderStyle.Single,
                        1.0,
                        Colors.Black);

                    spacer = section.AddParagraph();                   
                    
                }
            }

            string fileCGU = patient.CGU_No.Replace(".", "-");
            string docCode = "HS";
            string refIDString = icp.REFID.ToString();
            string mpiString = patient.MPI.ToString();
            string dateTimeString = DateTime.Now.ToString("yyyyMMddHHmmss");
            string diaryIDString = diaryID.ToString();


            PdfDocumentRenderer pdf = new PdfDocumentRenderer();
            pdf.Document = document;
            pdf.RenderDocument();
            pdf.PdfDocument.Save(Path.Combine(Directory.GetCurrentDirectory(), $"wwwroot\\StandardLetterPreviews\\preview-{user}.pdf"));

            if (!isPreview)
            {
                //System.IO.File.Copy($"wwwroot\\StandardLetterPreviews\\preview-{user}.pdf", $@"C:\CGU_DB\Letters\CaStdLetter-{fileCGU}-{docCode}-{mpiString}-0-{refIDString}-0-{dateTimeString}-{diaryIDString}.pdf");
                string edmsPath = await _constantsData.GetConstant("PrintPathEDMS", 1);

                System.IO.File.Copy($"wwwroot\\StandardLetterPreviews\\preview-{user}.pdf", $@"{edmsPath}\CaStdLetter-{fileCGU}-{docCode}-{mpiString}-0-{refIDString}-0-{dateTimeString}-{diaryIDString}.pdf");
            }

            success = true;

            return success;
        }
    }
}
