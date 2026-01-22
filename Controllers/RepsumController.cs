//using ClinicalXPDataConnections.Data;
using ClinicalXPDataConnections.Meta;
using ClinicalXPDataConnections.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MigraDoc.DocumentObjectModel;
using MigraDoc.Rendering;

namespace ClinicX.Controllers
{
    public class RepsumController : Controller
    {
        //private readonly ClinicalContext _clinContext;
        //private readonly DocumentContext _documentContext;
        private readonly IConfiguration _config;
        private readonly IStaffUserDataAsync _staffUser;
        private readonly IPatientDataAsync _patientData;
        private readonly IReferralDataAsync _referralData;
        private readonly IConstantsDataAsync _constantsData;
        private readonly IRiskDataAsync _riskData;
        private readonly ITriageDataAsync _triageData;
        private readonly ISurveillanceDataAsync _survData;
        private readonly IStudyDataAsync _studyData;
        private readonly ITestEligibilityDataAsync _teData;
        private readonly IClinicDataAsync _appData;
        private readonly IWaitingListDataAsync _wlData;
        private readonly IDiaryDataAsync _dData;
        private readonly IRelativeDataAsync _relData;
        private readonly IRelativeDiaryDataAsync _relDiaryData;
        private readonly IFHSummaryDataAsync _famData;

        public RepsumController(IConfiguration config, IStaffUserDataAsync staffUserData, IPatientDataAsync patientData, IReferralDataAsync referralData, IConstantsDataAsync constantsData, IRiskDataAsync riskData,
            ITriageDataAsync triageData, ISurveillanceDataAsync surveillanceData, IStudyDataAsync studyData, ITestEligibilityDataAsync testEligibilityData, IClinicDataAsync clinicData, IWaitingListDataAsync waitingListData,
            IDiaryDataAsync diaryData, IRelativeDataAsync relativeData, IRelativeDiaryDataAsync relativeDiaryData, IFHSummaryDataAsync fHSummaryData)
        {
            //_clinContext = context;
            //_documentContext = documentContext;
            _staffUser = staffUserData;
            _patientData = patientData;
            _referralData = referralData;
            _constantsData = constantsData;
            _riskData = riskData;      
            _triageData = triageData;
            _survData = surveillanceData;
            _studyData = studyData;
            _teData = testEligibilityData;
            _appData = clinicData;
            _wlData = waitingListData;
            _dData = diaryData;
            _relData = relativeData;
            _relDiaryData = relativeDiaryData;
            _famData = fHSummaryData;
        }

        [Authorize]
        public async Task<IActionResult> PrepareRepsum(int id, int diaryID)
        {            
            await DoRepsum(id, diaryID, User.Identity.Name);

            return RedirectToAction("CancerReview", "Triage", new { id = id });
        }


        public async Task DoRepsum(int icpID, int diaryID, string user)
        {
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

            section.PageSetup.LeftMargin = 20;
            section.PageSetup.RightMargin = 20;
            section.PageSetup.TopMargin = 20;
            section.PageSetup.BottomMargin = 20;

            MigraDoc.DocumentObjectModel.Tables.Table table = section.AddTable();
            MigraDoc.DocumentObjectModel.Tables.Column reportHeader = table.AddColumn();
            MigraDoc.DocumentObjectModel.Tables.Column logo = table.AddColumn();
            reportHeader.Format.Alignment = ParagraphAlignment.Left;
            logo.Format.Alignment = ParagraphAlignment.Right;
        
            MigraDoc.DocumentObjectModel.Tables.Row row1 = table.AddRow();
            row1.VerticalAlignment = MigraDoc.DocumentObjectModel.Tables.VerticalAlignment.Top;
            row1.Cells[0].AddParagraph().AddFormattedText($"Summary of reports for referral: {referral.refid} / {patient.CGU_No}", TextFormat.Bold);

            MigraDoc.DocumentObjectModel.Tables.Row row2 = table.AddRow();
            row2.VerticalAlignment = MigraDoc.DocumentObjectModel.Tables.VerticalAlignment.Center;


            string redHeaderText = "***SENSITIVE INFORMATION - DO NOT SHARE***" + Environment.NewLine + "This summary contains information that may have been requested as part of another family members' referral";

            row2.Cells[0].AddParagraph().AddFormattedText(redHeaderText, TextFormat.Bold);
            row2.Cells[0].Format.Font.Color = Colors.Red;

            MigraDoc.DocumentObjectModel.Shapes.Image imgLogo = row2.Cells[1].AddImage(@"wwwroot\Letterhead.jpg");
            imgLogo.ScaleWidth = new Unit(0.5, UnitType.Point);
            imgLogo.ScaleHeight = new Unit(0.5, UnitType.Point);

            table.Columns.Width = 240;
            reportHeader.Width = 300;
            logo.Width = 180;
            row1.Height = 10;
            row2.Height = 100;

            Paragraph spacer = section.AddParagraph();

            MigraDoc.DocumentObjectModel.Tables.Table table2 = section.AddTable();
            MigraDoc.DocumentObjectModel.Tables.Column boldTextColumn = table2.AddColumn();
            MigraDoc.DocumentObjectModel.Tables.Column normalTextColumn = table2.AddColumn();
            boldTextColumn.Width = 120;
            normalTextColumn.Width = 500;
            MigraDoc.DocumentObjectModel.Tables.Row row2_1 = table2.AddRow();
            MigraDoc.DocumentObjectModel.Tables.Row row2_2 = table2.AddRow();
            MigraDoc.DocumentObjectModel.Tables.Row row2_3 = table2.AddRow();
            MigraDoc.DocumentObjectModel.Tables.Row row2_4 = table2.AddRow();
            table2.Rows.Height = 15;

            row2_1.Cells[0].AddParagraph().AddFormattedText("Referred By:", TextFormat.Bold);
            row2_1.Cells[1].AddParagraph($"{referral.RefType} - {referral.RefDate.Value.ToString("dd/MM/yyyy")} - {referral.ReferringClinician} at {referral.ReferringFacility}");
            row2_2.Cells[0].AddParagraph().AddFormattedText("Action on Referral:", TextFormat.Bold);
            row2_2.Cells[1].AddParagraph(icpc.ActRefInfo);
            row2_3.Cells[0].AddParagraph().AddFormattedText("Provisional Review:", TextFormat.Bold);
            string reviewByDetails = "";
            if (icpc.ReviewedBy != null)
            {
                var staffmember = await _staffUser.GetStaffMemberDetailsByStaffCode(icpc.ReviewedBy);
                reviewByDetails = $"{staffmember.NAME} on {icpc.ReviewedDate.Value.ToString("dd/MM/yyyy")}";
            }
            row2_3.Cells[1].AddParagraph(reviewByDetails);
            row2_4.Cells[0].AddParagraph().AddFormattedText("Final Review:", TextFormat.Bold);
            string finalReviewDetails = "";
            if (icpc.FinalReviewedBy != null)
            {
                var staffmember = await _staffUser.GetStaffMemberDetailsByStaffCode(icpc.ReviewedBy);
                finalReviewDetails = $"{staffmember.NAME} on {icpc.FinalReviewedDate.Value.ToString("dd/MM/yyyy")}";
            }
            row2_4.Cells[1].AddParagraph(finalReviewDetails);
            Paragraph p1 = section.AddParagraph();
            p1.AddFormattedText("Additional Pre-clinic Review Notes:", TextFormat.Bold);
            string additionalNotes = "";
            if (icpc.Comments != null)
            {
                additionalNotes = icpc.Comments;
            }
            Paragraph p2 = section.AddParagraph(additionalNotes);
            p2.Format.Borders.Width = 1;
            p2.Format.Borders.Visible = true;
            spacer = section.AddParagraph();


            //RiskData riskData = new RiskData(_clinContext);
            List<Risk> riskList = await _riskData.GetRiskListForPatient(patient.MPI);
            if (riskList.Count > 0)
            {
                Paragraph pRiskHeader = section.AddParagraph();
                pRiskHeader.AddFormattedText("Risk Summary", TextFormat.Bold);
                pRiskHeader.Format.Font.Size = 12;

                MigraDoc.DocumentObjectModel.Tables.Table tableRisk = section.AddTable();
                MigraDoc.DocumentObjectModel.Tables.Column rDate = tableRisk.AddColumn();
                MigraDoc.DocumentObjectModel.Tables.Column rRisk = tableRisk.AddColumn();
                MigraDoc.DocumentObjectModel.Tables.Column rLifetime = tableRisk.AddColumn();
                MigraDoc.DocumentObjectModel.Tables.Column r30_40 = tableRisk.AddColumn();
                MigraDoc.DocumentObjectModel.Tables.Column r40_50 = tableRisk.AddColumn();
                MigraDoc.DocumentObjectModel.Tables.Column rCalc = tableRisk.AddColumn();
                MigraDoc.DocumentObjectModel.Tables.Column rSite = tableRisk.AddColumn();
                MigraDoc.DocumentObjectModel.Tables.Row rRowHead = tableRisk.AddRow();
                rRowHead.Cells[0].AddParagraph().AddFormattedText("Date", TextFormat.Bold);
                rRowHead.Cells[1].AddParagraph().AddFormattedText("Risk", TextFormat.Bold);
                rRowHead.Cells[2].AddParagraph().AddFormattedText("Lifetime Risk", TextFormat.Bold);
                rRowHead.Cells[3].AddParagraph().AddFormattedText("Ten Year Risk (30-40)", TextFormat.Bold);
                rRowHead.Cells[4].AddParagraph().AddFormattedText("Ten Year Risk (40-50)", TextFormat.Bold);
                rRowHead.Cells[5].AddParagraph().AddFormattedText("Calculation Tool", TextFormat.Bold);
                rRowHead.Cells[6].AddParagraph().AddFormattedText("Surv Site", TextFormat.Bold);
                rRowHead.Borders.Top.Width = 0.25;

                int rowCount = 0;

                foreach (var r in riskList)
                {
                    rowCount += 1;
                    //tableRisk.AddRow();
                    MigraDoc.DocumentObjectModel.Tables.Row rRow = tableRisk.AddRow();

                    string calcTool = "Unknown"; //because of course there's fucking nulls!!!
                    if(r.CalculationToolUsed != null) { calcTool = r.CalculationToolUsed;  }

                    rRow.Cells[0].AddParagraph(r.RiskDate.Value.ToString("dd/MM/yyyy"));
                    if (r.RiskName != null)
                    {
                        rRow.Cells[1].AddParagraph(r.RiskName);
                    }
                    else
                    {
                        rRow.Cells[1].AddParagraph(r.RiskCode);
                    }
                    rRow.Cells[2].AddParagraph(r.LifetimeRiskPercentage.ToString());
                    rRow.Cells[3].AddParagraph(r.R30_40.ToString());
                    rRow.Cells[4].AddParagraph(r.R40_50.ToString());
                    rRow.Cells[5].AddParagraph(calcTool);
                    rRow.Cells[6].AddParagraph(r.SurvSite);
                    //rRow.Borders.Bottom.Visible = false;
                    if (rowCount == riskList.Count)
                    {
                        rRow.Borders.Bottom.Width = 0.25;
                    }
                }

                tableRisk.Columns[0].Borders.Left.Width = 0.25;
                tableRisk.Columns[6].Borders.Right.Width = 0.25;

            }
            spacer = section.AddParagraph();


            
            List<Surveillance> survList = await _survData.GetSurveillanceList(patient.MPI);
            if (survList.Count > 0)
            {
                Paragraph pSurvHeader = section.AddParagraph();
                pSurvHeader.AddFormattedText("Surveillance Summary", TextFormat.Bold);
                pSurvHeader.Format.Font.Size = 12;

                MigraDoc.DocumentObjectModel.Tables.Table tableSurv = section.AddTable();
                MigraDoc.DocumentObjectModel.Tables.Column sSite = tableSurv.AddColumn();
                MigraDoc.DocumentObjectModel.Tables.Column sStaA = tableSurv.AddColumn();
                MigraDoc.DocumentObjectModel.Tables.Column sStoA = tableSurv.AddColumn();
                MigraDoc.DocumentObjectModel.Tables.Column sFreq = tableSurv.AddColumn();
                sFreq.Width = 150;
                MigraDoc.DocumentObjectModel.Tables.Column sType = tableSurv.AddColumn();
                MigraDoc.DocumentObjectModel.Tables.Row sRowHead = tableSurv.AddRow();
                sRowHead.Cells[0].AddParagraph().AddFormattedText("Site", TextFormat.Bold);
                sRowHead.Cells[1].AddParagraph().AddFormattedText("Start Age", TextFormat.Bold);
                sRowHead.Cells[2].AddParagraph().AddFormattedText("Stop Age", TextFormat.Bold);
                sRowHead.Cells[3].AddParagraph().AddFormattedText("Frequency", TextFormat.Bold);
                sRowHead.Cells[4].AddParagraph().AddFormattedText("Type", TextFormat.Bold);
                sRowHead.Borders.Top.Width = 0.25;

                int rowCount = 0;

                if (survList.Count > 0)
                {
                    foreach (var s in survList)
                    {
                        rowCount += 1;
                        MigraDoc.DocumentObjectModel.Tables.Row sRow = tableSurv.AddRow();                        
                        sRow.Cells[0].AddParagraph(s.SurvSite);
                        if (s.SurvStartAge != null) { sRow.Cells[1].AddParagraph(s.SurvStartAge.ToString()); }
                        if (s.SurvStopAge != null) { sRow.Cells[2].AddParagraph(s.SurvStopAge.ToString()); }
                        sRow.Cells[3].AddParagraph(s.SurvFreq);
                        sRow.Cells[4].AddParagraph(s.SurvType);

                        if (rowCount == survList.Count)
                        {
                            sRow.Borders.Bottom.Width = 0.25;
                        }
                    }
                }

                tableSurv.Columns[0].Borders.Left.Width = 0.25;
                tableSurv.Columns[4].Borders.Right.Width = 0.25;
            }
            spacer = section.AddParagraph();


            
            List<Study> studyList = await _studyData.GetStudiesList(patient.MPI);
            if (studyList.Count > 0)
            {
                Paragraph pStudiesHeader = section.AddParagraph();
                pStudiesHeader.AddFormattedText("Studies Summary", TextFormat.Bold);
                pStudiesHeader.Format.Font.Size = 12;

                MigraDoc.DocumentObjectModel.Tables.Table tableStudy = section.AddTable();
                MigraDoc.DocumentObjectModel.Tables.Column sDat = tableStudy.AddColumn();
                MigraDoc.DocumentObjectModel.Tables.Column sNam = tableStudy.AddColumn();
                MigraDoc.DocumentObjectModel.Tables.Column sSta = tableStudy.AddColumn();
                MigraDoc.DocumentObjectModel.Tables.Row sRowHead = tableStudy.AddRow();
                sNam.Width = 300;
                sRowHead.Cells[0].AddParagraph().AddFormattedText("Date Identified", TextFormat.Bold);
                sRowHead.Cells[1].AddParagraph().AddFormattedText("Study Code/Name", TextFormat.Bold);
                sRowHead.Cells[2].AddParagraph().AddFormattedText("Status", TextFormat.Bold);
                sRowHead.Borders.Top.Width = 0.25;

                int rowCount = 0;

                foreach (var s in studyList)
                {
                    rowCount += 1;
                    MigraDoc.DocumentObjectModel.Tables.Row sRow = tableStudy.AddRow();
                    if (s.IdentifiedDate != null)
                    {
                        sRow.Cells[0].AddParagraph(s.IdentifiedDate.Value.ToString("dd/MM/yyyy"));
                    }
                    sRow.Cells[1].AddParagraph(s.StudyCode + "-" + s.StudyName);
                    if (s.Status != null)
                    {
                        sRow.Cells[2].AddParagraph(s.Status);
                    }

                    if (rowCount == studyList.Count)
                    {
                        sRow.Borders.Bottom.Width = 0.25;
                    }
                }

                tableStudy.Columns[0].Borders.Left.Width = 0.25;
                tableStudy.Columns[2].Borders.Right.Width = 0.25;

            }

            spacer = section.AddParagraph();

            
            List<Eligibility> teList = await _teData.GetTestingEligibilityList(patient.MPI);
            if (teList.Count > 0)
            {
                Paragraph pTestHeader = section.AddParagraph();
                pTestHeader.AddFormattedText("Test Eligibility Summary", TextFormat.Bold);
                pTestHeader.Format.Font.Size = 12;

                MigraDoc.DocumentObjectModel.Tables.Table tableTE = section.AddTable();
                MigraDoc.DocumentObjectModel.Tables.Column sGen = tableTE.AddColumn();
                MigraDoc.DocumentObjectModel.Tables.Column sSco = tableTE.AddColumn();
                MigraDoc.DocumentObjectModel.Tables.Column sSta = tableTE.AddColumn();
                MigraDoc.DocumentObjectModel.Tables.Column sOff = tableTE.AddColumn();
                MigraDoc.DocumentObjectModel.Tables.Column sRel = tableTE.AddColumn();
                MigraDoc.DocumentObjectModel.Tables.Column sNam = tableTE.AddColumn();

                MigraDoc.DocumentObjectModel.Tables.Row sRowHead = tableTE.AddRow();
                sRowHead.Cells[0].AddParagraph().AddFormattedText("Gene", TextFormat.Bold);
                sRowHead.Cells[1].AddParagraph().AddFormattedText("Calc Tool", TextFormat.Bold);
                sRowHead.Cells[2].AddParagraph().AddFormattedText("Score", TextFormat.Bold);
                sRowHead.Cells[3].AddParagraph().AddFormattedText("Offer Testing", TextFormat.Bold);
                sRowHead.Cells[4].AddParagraph().AddFormattedText("Relative", TextFormat.Bold);
                sRowHead.Cells[5].AddParagraph().AddFormattedText("Name", TextFormat.Bold);
                sRowHead.Borders.Top.Width = 0.25;

                int rowCount = 0;

                foreach (var s in teList)
                {
                    rowCount += 1;
                    MigraDoc.DocumentObjectModel.Tables.Row sRow = tableTE.AddRow();
                    if (s.Gene != null) { sRow.Cells[0].AddParagraph(s.Gene.ToString()); }
                    sRow.Cells[1].AddParagraph(s.CalcTool);
                    sRow.Cells[2].AddParagraph(s.Score);
                    sRow.Cells[3].AddParagraph(s.OfferTesting);
                    if (s.Relative)
                    {
                        sRow.Cells[4].AddParagraph("Yes");
                    }
                    else
                    {
                        sRow.Cells[4].AddParagraph("No");
                    }
                    if (s.RelSurname != null)
                    {
                        sRow.Cells[5].AddParagraph($"{s.RelTitle} {s.RelForename1} {s.RelSurname}");
                    }

                    if (rowCount == teList.Count)
                    {
                        sRow.Borders.Bottom.Width = 0.25;
                    }
                }

                tableTE.Columns[0].Borders.Left.Width = 0.25;
                tableTE.Columns[5].Borders.Right.Width = 0.25;

            }


            spacer = section.AddParagraph();

            
            List<Appointment> appList = await _appData.GetClinicByPatientsList(patient.MPI);
            if (appList.Count > 0)
            {
                Paragraph pAppHeader = section.AddParagraph();
                pAppHeader.AddFormattedText("Appointment Summary", TextFormat.Bold);
                pAppHeader.Format.Font.Size = 12;

                MigraDoc.DocumentObjectModel.Tables.Table tableApp = section.AddTable();
                MigraDoc.DocumentObjectModel.Tables.Column aDate = tableApp.AddColumn();
                MigraDoc.DocumentObjectModel.Tables.Column aTime = tableApp.AddColumn();
                MigraDoc.DocumentObjectModel.Tables.Column aType = tableApp.AddColumn();
                aType.Width = 100;
                MigraDoc.DocumentObjectModel.Tables.Column aWith = tableApp.AddColumn();
                aWith.Width = 100;
                MigraDoc.DocumentObjectModel.Tables.Column aCounseled = tableApp.AddColumn();
                aCounseled.Width = 200;
                MigraDoc.DocumentObjectModel.Tables.Row aRowHead = tableApp.AddRow();
                aRowHead.Cells[0].AddParagraph().AddFormattedText("Date", TextFormat.Bold);
                aRowHead.Cells[1].AddParagraph().AddFormattedText("Time", TextFormat.Bold);
                aRowHead.Cells[2].AddParagraph().AddFormattedText("Appt Type", TextFormat.Bold);
                aRowHead.Cells[3].AddParagraph().AddFormattedText("Appt With", TextFormat.Bold);
                aRowHead.Cells[4].AddParagraph().AddFormattedText("Attended", TextFormat.Bold);
                aRowHead.Borders.Top.Width = 0.25;

                int rowCount = 0;

                foreach (var a in appList)
                {
                    rowCount += 1;
                    MigraDoc.DocumentObjectModel.Tables.Row sRow = tableApp.AddRow();
                    sRow.Cells[0].AddParagraph(a.BOOKED_DATE.Value.ToString("dd/MM/yyyy"));
                    sRow.Cells[1].AddParagraph(a.BOOKED_TIME.Value.ToString("HH:mm"));
                    sRow.Cells[2].AddParagraph(a.AppType);
                    sRow.Cells[3].AddParagraph(a.Clinician);
                    sRow.Cells[4].AddParagraph(a.Attendance);

                    if (rowCount == appList.Count)
                    {
                        sRow.Borders.Bottom.Width = 0.25;
                    }
                }

                tableApp.Columns[0].Borders.Left.Width = 0.25;
                tableApp.Columns[4].Borders.Right.Width = 0.25;
            }

            spacer = section.AddParagraph();

            
            List<WaitingList> wlList = await _wlData.GetWaitingListByCGUNo(patient.CGU_No);
            if (wlList.Count > 0)
            {
                Paragraph pWaitHeader = section.AddParagraph();
                pWaitHeader.AddFormattedText("Waiting List Summary", TextFormat.Bold);
                pWaitHeader.Format.Font.Size = 12;

                MigraDoc.DocumentObjectModel.Tables.Table tableWL = section.AddTable();
                MigraDoc.DocumentObjectModel.Tables.Column wClin = tableWL.AddColumn();
                MigraDoc.DocumentObjectModel.Tables.Column wVen = tableWL.AddColumn();
                MigraDoc.DocumentObjectModel.Tables.Column wAddedBy = tableWL.AddColumn();
                MigraDoc.DocumentObjectModel.Tables.Column wAddedDate = tableWL.AddColumn();
                MigraDoc.DocumentObjectModel.Tables.Column wToBeSeenBy = tableWL.AddColumn();
                MigraDoc.DocumentObjectModel.Tables.Column wComments = tableWL.AddColumn();

                MigraDoc.DocumentObjectModel.Tables.Row wRowHead = tableWL.AddRow();
                wRowHead.Cells[0].AddParagraph().AddFormattedText("Clinician", TextFormat.Bold);
                wRowHead.Cells[1].AddParagraph().AddFormattedText("Clinic", TextFormat.Bold);
                wRowHead.Cells[2].AddParagraph().AddFormattedText("Added By", TextFormat.Bold);
                wRowHead.Cells[3].AddParagraph().AddFormattedText("Added Date", TextFormat.Bold);
                wRowHead.Cells[4].AddParagraph().AddFormattedText("To Be Seen By", TextFormat.Bold);
                wRowHead.Cells[5].AddParagraph().AddFormattedText("Comments", TextFormat.Bold);
                wRowHead.Borders.Top.Width = 0.25;

                int rowCount = 0;

                foreach (var w in wlList)
                {
                    rowCount += 1;
                    MigraDoc.DocumentObjectModel.Tables.Row sRow = tableWL.AddRow();
                    sRow.Cells[0].AddParagraph(w.ClinicianName);
                    sRow.Cells[1].AddParagraph(w.ClinicName);
                    sRow.Cells[2].AddParagraph();
                    sRow.Cells[3].AddParagraph(w.AddedDate.Value.ToString("dd/MM/yyyy"));
                    sRow.Cells[4].AddParagraph();
                    sRow.Cells[5].AddParagraph(w.Comment);

                    if (rowCount == wlList.Count)
                    {
                        sRow.Borders.Bottom.Width = 0.25;
                    }
                }

                tableWL.Columns[0].Borders.Left.Width = 0.25;
                tableWL.Columns[5].Borders.Right.Width = 0.25;
            }



            spacer = section.AddParagraph();

            
            List<Diary> dList = await _dData.GetDiaryList(patient.MPI);
            List<Relative> rList = await _relData.GetRelativesList(patient.MPI);
            rList = rList.DistinctBy(r => r.relsid).ToList();

            int rdCount = 0;

            if (rList.Count > 0)
            {                
                foreach (var rel in rList)
                {
                    List<RelativeDiary> rdList = await _relDiaryData.GetRelativeDiaryList(rel.relsid);

                    if (rdList.Count > 0)
                    {
                        rdCount += 1; //I can see NO OTHER WAY to do this!!! - just to get the bottom border to appear based on whether the relatives have any diary entries or not!!!
                    }
                }
            }


            if (dList.Count > 0)
            {
                Paragraph pDiaryHeader = section.AddParagraph();
                pDiaryHeader.AddFormattedText("Diary Summary", TextFormat.Bold);
                pDiaryHeader.Format.Font.Size = 12;

                Paragraph dPatient = section.AddParagraph();
                
                MigraDoc.DocumentObjectModel.Tables.Table tableDiary = section.AddTable();
                MigraDoc.DocumentObjectModel.Tables.Column dDate = tableDiary.AddColumn();
                MigraDoc.DocumentObjectModel.Tables.Column dAction = tableDiary.AddColumn();
                dAction.Width = 50;
                MigraDoc.DocumentObjectModel.Tables.Column dText = tableDiary.AddColumn();
                dText.Width = 250;
                MigraDoc.DocumentObjectModel.Tables.Column dDocCode = tableDiary.AddColumn();
                dDocCode.Width = 50;
                MigraDoc.DocumentObjectModel.Tables.Column dReceived = tableDiary.AddColumn();
                MigraDoc.DocumentObjectModel.Tables.Column dRetExpected = tableDiary.AddColumn();

                MigraDoc.DocumentObjectModel.Tables.Row wTitle = tableDiary.AddRow();
                wTitle.Cells[0].MergeRight = 5;
                wTitle.Cells[0].AddParagraph().AddFormattedText($"Patient - {patient.FIRSTNAME} {patient.LASTNAME}", TextFormat.Bold);
                wTitle.Format.Font.Color = Colors.Blue;
                wTitle.Format.Font.Size = 12;
                wTitle.Borders.Top.Width = 0.25;

                MigraDoc.DocumentObjectModel.Tables.Row wRowHead = tableDiary.AddRow();
                wRowHead.Cells[0].AddParagraph().AddFormattedText("Diary Date", TextFormat.Bold);
                wRowHead.Cells[1].AddParagraph().AddFormattedText("Diary Action", TextFormat.Bold);
                wRowHead.Cells[2].AddParagraph().AddFormattedText("Diary Text", TextFormat.Bold);
                wRowHead.Cells[3].AddParagraph().AddFormattedText("DocCode", TextFormat.Bold);
                wRowHead.Cells[4].AddParagraph().AddFormattedText("Received", TextFormat.Bold);
                wRowHead.Cells[5].AddParagraph().AddFormattedText("No Return Expected", TextFormat.Bold);
                //wRowHead.Borders.Top.Width = 0.25;

                int rowCount = 0;

                foreach (var d in dList)
                {
                    rowCount += 1;
                    MigraDoc.DocumentObjectModel.Tables.Row sRow = tableDiary.AddRow();
                    if (d.DiaryDate.HasValue)
                    {
                        sRow.Cells[0].AddParagraph(d.DiaryDate.Value.ToString("dd/MM/yyyy"));
                    }
                    else
                    {
                        sRow.Cells[0].AddParagraph("Unknown");
                    }
                    sRow.Cells[1].AddParagraph(d.DiaryAction);
                    if (d.DiaryText != null)
                    {
                        sRow.Cells[2].AddParagraph(d.DiaryText.Substring(0, Math.Min(d.DiaryText.Length,  1 + d.DiaryText.IndexOf(Environment.NewLine))));
                    }
                    if (d.DocCode != null)
                    {
                        sRow.Cells[3].AddParagraph(d.DocCode);
                    }
                    sRow.Cells[4].AddParagraph();
                    sRow.Cells[5].AddParagraph();

                    if (rowCount == dList.Count && rdCount == 0)
                    {
                        sRow.Borders.Bottom.Width = 0.25;
                    }
                }

                if (rList.Count > 0)
                {
                    foreach (var rel in rList)
                    {
                        List<RelativeDiary> rdList = await _relDiaryData.GetRelativeDiaryList(rel.relsid);

                        if (rdList.Count > 0)
                        {
                            MigraDoc.DocumentObjectModel.Tables.Row wRelTitle = tableDiary.AddRow();
                            wRelTitle.Cells[0].MergeRight = 5;
                            wRelTitle.Cells[0].AddParagraph().AddFormattedText($"Relative - {rel.RelTitle}  {rel.RelForename1} {rel.RelSurname}", TextFormat.Bold);
                            wRelTitle.Format.Font.Color = Colors.Blue;
                            wRelTitle.Format.Font.Size = 12;

                            foreach (var d in rdList)
                            {
                                rowCount += 1;
                                MigraDoc.DocumentObjectModel.Tables.Row sRow = tableDiary.AddRow();
                                if (d.DiaryDate != null)
                                {
                                    sRow.Cells[0].AddParagraph(d.DiaryDate.Value.ToString("dd/MM/yyyy"));
                                }
                                if (d.DiaryAction != null)
                                {
                                    sRow.Cells[1].AddParagraph(d.DiaryAction);
                                }
                                if (d.DiaryText != null)
                                {
                                    sRow.Cells[2].AddParagraph(d.DiaryText);
                                }
                                if (d.DocCode != null)
                                {
                                    sRow.Cells[3].AddParagraph(d.DocCode);
                                }
                                if (d.DiaryRec != null)
                                {
                                    sRow.Cells[4].AddParagraph(d.DiaryRec.Value.ToString("dd/MM/yyyy"));
                                }
                                if (d.NotReturned)
                                {
                                    sRow.Cells[5].AddParagraph("Yes");
                                }
                                else
                                {
                                    sRow.Cells[5].AddParagraph("No");
                                }

                                if (rowCount == dList.Count + rdList.Count)
                                {
                                    sRow.Borders.Bottom.Width = 0.25;
                                }
                            }

                        }
                    }
                }

                tableDiary.Columns[0].Borders.Left.Width = 0.25;
                tableDiary.Columns[5].Borders.Right.Width = 0.25;
            }

            spacer = section.AddParagraph();

            spacer = section.AddParagraph();
            
            List<FHSummary> famList = await _famData.GetFHSummaryList(patient.MPI);

            if (famList.Count > 0)
            {
                Paragraph pFamHeader = section.AddParagraph();
                pFamHeader.AddFormattedText("Family History Summary", TextFormat.Bold);
                pFamHeader.Format.Font.Size = 12;

                MigraDoc.DocumentObjectModel.Tables.Table tableFam = section.AddTable();
                tableFam.Format.Font.Size = 9;
                tableFam.Columns.Width = 100;
                MigraDoc.DocumentObjectModel.Tables.Column c1 = tableFam.AddColumn(); //name
                MigraDoc.DocumentObjectModel.Tables.Column c2 = tableFam.AddColumn(); //sex
                c2.Width = 30;
                MigraDoc.DocumentObjectModel.Tables.Column c3 = tableFam.AddColumn(); //alive
                c3.Width = 30;
                MigraDoc.DocumentObjectModel.Tables.Column c4 = tableFam.AddColumn(); //dob
                c4.Width = 50;
                MigraDoc.DocumentObjectModel.Tables.Column c5 = tableFam.AddColumn(); //dod
                c5.Width = 40;
                MigraDoc.DocumentObjectModel.Tables.Column c6 = tableFam.AddColumn(); //diag age etc
                c6.Width = 200;
                MigraDoc.DocumentObjectModel.Tables.Column c7 = tableFam.AddColumn(); //info req/why not
                c7.Width = 60;
                MigraDoc.DocumentObjectModel.Tables.Column c8 = tableFam.AddColumn(); //info awaiting
                c8.Width = 60;
                MigraDoc.DocumentObjectModel.Tables.Column c9 = tableFam.AddColumn(); //Spacer
                c9.Width = 5;
                /*
                MigraDoc.DocumentObjectModel.Tables.Column c9 = tableFam.AddColumn(); //notes
                //c9.Width = 100;
                MigraDoc.DocumentObjectModel.Tables.Column c10 = tableFam.AddColumn(); //conf
                //c10.Width = 50;
                MigraDoc.DocumentObjectModel.Tables.Column c11 = tableFam.AddColumn(); //wmfacsid
                //c11.Width = 50;
                */

                MigraDoc.DocumentObjectModel.Tables.Row wRowHead = tableFam.AddRow();
                wRowHead.Cells[0].AddParagraph().AddFormattedText("Relative", TextFormat.Bold);
                wRowHead.Cells[1].AddParagraph().AddFormattedText("Sex", TextFormat.Bold);
                wRowHead.Cells[2].AddParagraph().AddFormattedText("Alive", TextFormat.Bold);
                wRowHead.Cells[3].AddParagraph().AddFormattedText("DOB", TextFormat.Bold);
                wRowHead.Cells[4].AddParagraph().AddFormattedText("DOD", TextFormat.Bold);
                wRowHead.Cells[5].AddParagraph().AddFormattedText("Family history reported at referral", TextFormat.Bold);
                wRowHead.Cells[6].AddParagraph().AddFormattedText("Ca registry request", TextFormat.Bold);
                wRowHead.Cells[7].AddParagraph().AddFormattedText("CA registry info obtained?", TextFormat.Bold);
                //wRowHead.Cells[8].AddParagraph().AddFormattedText("Notes", TextFormat.Bold);
                //wRowHead.Cells[9].AddParagraph().AddFormattedText("Conf", TextFormat.Bold);
                //wRowHead.Cells[10].AddParagraph().AddFormattedText("WMFACSID", TextFormat.Bold);
                wRowHead.Borders.Top.Width = 0.25;

                int rowCount = 0;

                foreach (var f in famList)
                {
                    rowCount += 1;

                    MigraDoc.DocumentObjectModel.Tables.Row sRow = tableFam.AddRow();
                    sRow.Cells[0].AddParagraph(f.RelSurname + ", " + f.RelForename1 + " " + f.RelForename2);
                    sRow.Cells[1].AddParagraph(f.RelSex);
                    sRow.Cells[2].AddParagraph(f.Alive);
                    sRow.Cells[3].AddParagraph(f.RelDOB);
                    sRow.Cells[4].AddParagraph(f.RelDOD);
                    sRow.Cells[5].AddParagraph(f.Diagnosis + " age " + f.AgeDiag + ", treated at " + f.Hospital);
                    sRow.Cells[6].AddParagraph(f.InfoReq + " " + f.WhyNot);
                    if (f.Conf == "Other")
                    {
                        sRow.Cells[7].AddParagraph("No");
                    }
                    else if (f.Conf == "No")
                    {
                        if (f.InfoReq == "No")
                        {
                            sRow.Cells[7].AddParagraph("N/A");
                        }
                        else if (f.InfoReq == "Yes")
                        {
                            sRow.Cells[7].AddParagraph("No");
                        }
                    }
                    else if (f.Conf == null)
                    {
                        if (f.InfoReq == "No")
                        {
                            sRow.Cells[7].AddParagraph("N/A");
                        }
                        else if (f.InfoReq == "Yes")
                        {
                            sRow.Cells[7].AddParagraph("Awaited");
                        }
                    }
                    else if (f.Conf == "Yes")
                    {
                        sRow.Cells[7].AddParagraph("Yes");
                        sRow.Cells[7].Format.Font.Color = Colors.Blue;
                    }
                }

                MigraDoc.DocumentObjectModel.Tables.Row wKeyHeader = tableFam.AddRow();
                wKeyHeader.Cells[5].MergeRight = 2;
                wKeyHeader.Cells[5].AddParagraph().AddFormattedText("* Key to CA registry request fields:", TextFormat.Bold);
                wKeyHeader.Height = 20;
                wKeyHeader.VerticalAlignment = MigraDoc.DocumentObjectModel.Tables.VerticalAlignment.Bottom;

                MigraDoc.DocumentObjectModel.Tables.Row wKeyToStuff = tableFam.AddRow();
                wKeyToStuff.Cells[5].MergeRight = 2;

                string keyToStuffText = "Reason not requested:" + Environment.NewLine +
                "C = consent known to be declined" + Environment.NewLine +
                "E = too early for cancer registry records" + Environment.NewLine +
                "N = consent form not returned to department" + Environment.NewLine +
                "O = overseas registry; unable to request details" + Environment.NewLine +
                "P = patient did not forward consent form to relative" + Environment.NewLine +
                "R = details registered on database only (older files)" + Environment.NewLine +
                "S = information obtained from a different (non cancer registry) source" + Environment.NewLine +
                "X = insufficient information available to request details" + Environment.NewLine +
                "Z = other reason, not specified above";

                wKeyToStuff.Cells[5].AddParagraph(keyToStuffText);
                wKeyToStuff.Cells[5].Borders.Top.Width = 0.25;
                wKeyToStuff.Cells[5].Borders.Bottom.Width = 0.25;
                wKeyToStuff.Cells[5].Borders.Left.Width = 0.25;
                wKeyToStuff.Cells[5].Borders.Right.Width = 0.25;

                MigraDoc.DocumentObjectModel.Tables.Row wSpacer = tableFam.AddRow();
                wSpacer.Height = 2.5;
                wSpacer.Borders.Bottom.Width = 0.25;


                tableFam.Columns[0].Borders.Left.Width = 0.25;
                tableFam.Columns[8].Borders.Right.Width = 0.25;
            }


            PdfDocumentRenderer pdf = new PdfDocumentRenderer();
            pdf.Document = document;
            pdf.RenderDocument();
            pdf.PdfDocument.Save(Path.Combine(Directory.GetCurrentDirectory(), $"wwwroot\\StandardLetterPreviews\\preview-{user}.pdf"));

            

            string fileCGU = patient.CGU_No.Replace(".", "-");
            string docCode = "REPSUM";
            string refIDString = icp.REFID.ToString();
            string mpiString = patient.MPI.ToString();
            string dateTimeString = DateTime.Now.ToString("yyyyMMddHHmmss");
            string diaryIDString = diaryID.ToString();

            //if (!isPreview.GetValueOrDefault())
            //{
            //System.IO.File.Copy($"wwwroot\\StandardLetterPreviews\\preview-{user}.pdf", $@"C:\CGU_DB\Letters\CaStdLetter-{fileCGU}-{docCode}-{mpiString}-0-{refIDString}-0-{dateTimeString}-{diaryIDString}.pdf");
            string edmsPath = await _constantsData.GetConstant("PrintPathEDMS", 1);

            System.IO.File.Copy($"wwwroot\\StandardLetterPreviews\\preview-{user}.pdf", $@"{edmsPath}\CaStdLetter-{fileCGU}-{docCode}-{mpiString}-0-{refIDString}-0-{dateTimeString}-{diaryIDString}.pdf");
            //}
        }
    }
}

