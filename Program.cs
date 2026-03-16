using APIControllers.Controllers;
using APIControllers.Data;
using ClinicalXPDataConnections.Data;
using ClinicalXPDataConnections.Meta;
using ClinicX.Controllers;
using ClinicX.Data;
using ClinicX.Meta;
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

builder.Services.AddScoped<ICaseloadDataAsync, CaseloadDataAsync>();
builder.Services.AddScoped<IStaffUserDataAsync, StaffUserDataAsync>();
builder.Services.AddScoped<INotificationDataAsync, NotificationDataAsync>();
builder.Services.AddSingleton<IVersionData, VersionData>();
builder.Services.AddScoped<IPatientDataAsync, PatientDataAsync>();
builder.Services.AddScoped<IReferralDataAsync, ReferralDataAsync>();
builder.Services.AddScoped<IActivityDataAsync, ActivityDataAsync>();
builder.Services.AddScoped<IClinicDataAsync, ClinicDataAsync>();
builder.Services.AddScoped<IOutcomeDataAsync, OutcomeDataAsync>();
builder.Services.AddScoped<IClinicVenueDataAsync, ClinicVenueDataAsync>();
builder.Services.AddScoped<IActivityTypeDataAsync, ActivityTypeDataAsync>();
builder.Services.AddScoped<IClinicalNoteDataAsync, ClinicalNoteDataAsync>();
builder.Services.AddScoped<IMiscData, MiscData>();
builder.Services.AddScoped<IDiseaseDataAsync, DiseaseDataAsync>();
builder.Services.AddScoped<IDictatedLetterDataAsync, DictatedLetterDataAsync>();
builder.Services.AddScoped<IExternalClinicianDataAsync, ExternalClinicianDataAsync>();
builder.Services.AddScoped<IExternalFacilityDataAsync, ExternalFacilityDataAsync>();
builder.Services.AddScoped<IConstantsDataAsync, ConstantsDataAsync>();
builder.Services.AddScoped<IHPOCodeDataAsync, HPOCodeDataAsync>();
builder.Services.AddScoped<ILabDataAsync, LabReportDataAsync>();
builder.Services.AddScoped<IDocumentsDataAsync, DocumentsDataAsync>();
builder.Services.AddScoped<IDiaryDataAsync, DiaryDataAsync>();
builder.Services.AddScoped<ILeafletDataAsync, LeafletDataAsync>();
builder.Services.AddScoped<ISupervisorDataAsync, SupervisorDataAsync>();
builder.Services.AddScoped<IAreaNamesDataAsync, AreaNamesDataAsync>();
builder.Services.AddScoped<IPathwayDataAsync, PathwayDataAsync>();
builder.Services.AddScoped<IRelativeDataAsync, RelativeDataAsync>();
builder.Services.AddScoped<IRelativeDiaryDataAsync, RelativeDiaryDataAsync>();
builder.Services.AddScoped<IRelativeDiagnosisDataAsync, RelativeDiagnosisDataAsync>();
builder.Services.AddScoped<IAlertDataAsync, AlertDataAsync>();
builder.Services.AddScoped<IAgeCalculator, AgeCalculator>();
builder.Services.AddScoped<ITriageDataAsync, TriageDataAsync>();
builder.Services.AddScoped<IPhenotipsMirrorDataAsync, PhenotipsMirrorDataAsync>();
builder.Services.AddScoped<IPatientSearchDataAsync, PatientSearchDataAsync>();
builder.Services.AddScoped<IRiskDataAsync, RiskDataAsync>();
builder.Services.AddScoped<IRiskCodesDataAsync, RiskCodesDataAsync>();
builder.Services.AddScoped<ISurveillanceDataAsync, SurveillanceDataAsync>();
builder.Services.AddScoped<ISurveillanceCodesDataAsync, SurveillanceCodesDataAsync>();
builder.Services.AddScoped<IStudyDataAsync, StudyDataAsync>();
builder.Services.AddScoped<ITestEligibilityDataAsync, TestEligibilityDataAsync>();
builder.Services.AddScoped<IWaitingListDataAsync, WaitingListDataAsync>();
builder.Services.AddScoped<IFHSummaryDataAsync, FHSummaryDataAsync>();
builder.Services.AddScoped<IReviewDataAsync, ReviewDataAsync>();
builder.Services.AddScoped<IGeneChangeDataAsync, GeneChangeDataAsync>();
builder.Services.AddScoped<IGeneCodeDataAsync, GeneCodeDataAsync>();
builder.Services.AddScoped<ITestDataAsync, TestDataAsync>();
builder.Services.AddScoped<IBloodFormDataAsync, BloodFormDataAsync>();
builder.Services.AddScoped<ISampleDataAsync, SampleDataAsync>();
builder.Services.AddScoped<IICPActionDataAsync, ICPActionDataAsync>();
builder.Services.AddScoped<ICancerRequestDataAsync, CancerRequestDataAsync>();
builder.Services.AddScoped<IStaffOptionsDataAsync, StaffOptionsDataAsync>();
builder.Services.AddScoped<ITitleDataAsync, TitleDataAsync>();
builder.Services.AddScoped<IScreeningServiceDataAsync, ScreeningServiceDataAsync>();
builder.Services.AddScoped<IBreastHistoryDataAsync, BreastHistoryDataAsync>();
builder.Services.AddScoped<IUntestedVHRGroupDataAsync, UntestedVHRGroupDataAsync>();
builder.Services.AddScoped<IPriorityDataAsync, PriorityDataAsync>();
builder.Services.AddScoped<IAppointmentDataAsync, AppointmentDataAsync>();
builder.Services.AddScoped<IAuditService, AuditService>();
builder.Services.AddScoped<ICRUD, CRUD>();
builder.Services.AddScoped<VHRController>();
builder.Services.AddScoped<LetterController>();
builder.Services.AddScoped<IApiController, APIController>();
builder.Services.AddScoped<BloodFormController>();
builder.Services.AddScoped<ISocialServicePathwayDataAsync, SocialServicePathwayDataAsync>();

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
