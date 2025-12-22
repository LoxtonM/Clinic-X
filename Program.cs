using APIControllers.Controllers;
using APIControllers.Data;
using ClinicalXPDataConnections.Data;
using ClinicalXPDataConnections.Meta;
using ClinicX.Controllers;
using ClinicX.Data;
using ClinicX.Meta;
using ClinicX.Models;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);
var config = new ConfigurationBuilder().AddJsonFile("appsettings.json", optional: false)
    .AddJsonFile("secrets.json", optional: false)
    .Build();
// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddDbContext<ClinicalContext>(options => options.UseSqlServer(config.GetConnectionString("ConString")));
builder.Services.AddDbContext<ClinicXContext>(options => options.UseSqlServer(config.GetConnectionString("ConString")));
builder.Services.AddDbContext<DocumentContext>(options => options.UseSqlServer(config.GetConnectionString("ConString")));
builder.Services.AddDbContext<LabContext>(options => options.UseSqlServer(config.GetConnectionString("ConStringLab")));
builder.Services.AddDbContext<APIContext>(options => options.UseSqlServer(config.GetConnectionString("ConString")));

builder.Services.AddScoped<ICaseloadData, CaseloadData>();
builder.Services.AddScoped<IStaffUserData, StaffUserData>();
builder.Services.AddScoped<INotificationData, NotificationData>();
builder.Services.AddSingleton<IVersionData, VersionData>();
builder.Services.AddScoped<IPatientData, PatientData>();
builder.Services.AddScoped<IReferralData, ReferralData>();
builder.Services.AddScoped<IActivityData, ActivityData>();
builder.Services.AddScoped<IClinicData, ClinicData>();
builder.Services.AddScoped<IOutcomeData, OutcomeData>();
builder.Services.AddScoped<IClinicVenueData, ClinicVenueData>();
builder.Services.AddScoped<IActivityTypeData, ActivityTypeData>();
builder.Services.AddScoped<IClinicalNoteData,  ClinicalNoteData>();
builder.Services.AddScoped<IMiscData, MiscData>();
builder.Services.AddScoped<IDiseaseData, DiseaseData>();
builder.Services.AddScoped<IDictatedLetterData, DictatedLetterData>();
builder.Services.AddScoped<IExternalClinicianData, ExternalClinicianData>();
builder.Services.AddScoped<IExternalFacilityData, ExternalFacilityData>();
builder.Services.AddScoped<IConstantsData, ConstantsData>();
builder.Services.AddScoped<IHPOCodeData, HPOCodeData>();
builder.Services.AddScoped<ILabData, LabReportData>();
builder.Services.AddScoped<IDocumentsData, DocumentsData>();
builder.Services.AddScoped<IDiaryData, DiaryData>();
builder.Services.AddScoped<ILeafletData, LeafletData>();
builder.Services.AddScoped<ISupervisorData, SupervisorData>();
builder.Services.AddScoped<IAreaNamesData, AreaNamesData>();
builder.Services.AddScoped<IPathwayData, PathwayData>();
builder.Services.AddScoped<IRelativeData, RelativeData>();
builder.Services.AddScoped<IRelativeDiagnosisData, RelativeDiagnosisData>();
builder.Services.AddScoped<IAlertData, AlertData>();
builder.Services.AddScoped<IAgeCalculator, AgeCalculator>();
builder.Services.AddScoped<ITriageData, TriageData>();
builder.Services.AddScoped<IPhenotipsMirrorData, PhenotipsMirrorData>();
builder.Services.AddScoped<IPatientSearchData, PatientSearchData>();
builder.Services.AddScoped<IRiskData, RiskData>();
builder.Services.AddScoped<IRiskCodesData, RiskCodesData>();
builder.Services.AddScoped<ISurveillanceData, SurveillanceData>();
builder.Services.AddScoped<ISurveillanceCodesData, SurveillanceCodesData>();
builder.Services.AddScoped<IStudyData, StudyData>();
builder.Services.AddScoped<ITestEligibilityData, TestEligibilityData>();
builder.Services.AddScoped<IWaitingListData, WaitingListData>();
builder.Services.AddScoped<IFHSummaryData, FHSummaryData>();
builder.Services.AddScoped<IReviewData, ReviewData>();
builder.Services.AddScoped<IGeneChangeData, GeneChangeData>();
builder.Services.AddScoped<IGeneCodeData, GeneCodeData>();
builder.Services.AddScoped<ITestData, TestData>();
builder.Services.AddScoped<IBloodFormData, BloodFormData>();
builder.Services.AddScoped<ISampleData, SampleData>();
builder.Services.AddScoped<IICPActionData, ICPActionData>();
builder.Services.AddScoped<ICancerRequestData, CancerRequestData>();
builder.Services.AddScoped<IStaffOptionsData, StaffOptionsData>();
builder.Services.AddScoped<ITitleData, TitleData>();
builder.Services.AddScoped<IScreeningServiceData, ScreeningServiceData>();
builder.Services.AddScoped<IBreastHistoryData, BreastHistoryData>();
builder.Services.AddScoped<IUntestedVHRGroupData, UntestedVHRGroupData>();
builder.Services.AddScoped<IScreeningServiceData, ScreeningServiceData>();
builder.Services.AddScoped<IPriorityData, PriorityData>();
builder.Services.AddScoped<IAppointmentData, AppointmentData>();
builder.Services.AddScoped<IAuditService, AuditService>();
builder.Services.AddScoped<ICRUD, CRUD>();
builder.Services.AddScoped<VHRController>();
builder.Services.AddScoped<LetterController>();
builder.Services.AddScoped<APIController>();

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
   .AddCookie(options =>
   {
       options.LoginPath = "/Login/UserLogin";
   });


builder.Services.AddMvc();
builder.Configuration.SetBasePath(Directory.GetCurrentDirectory()).AddJsonFile("secrets.json");

// this code is all for the shared authentication
var directoryInfo = new DirectoryInfo(@"C:\Websites\Authentication");
builder.Services.AddDataProtection()
    .PersistKeysToFileSystem(directoryInfo)
    .SetApplicationName("GeneticsWebAppHome");

builder.Services.ConfigureApplicationCookie(options => {
    options.Cookie.Name = ".AspNet.GeneticsWebAppHome";
    options.Cookie.Path = "/";
});
//
var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}


//app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    //pattern: "{controller=Login}/{action=UserLogin}/{id?}");
    pattern: "{controller=Home}/{action=Index}");

app.Run();
