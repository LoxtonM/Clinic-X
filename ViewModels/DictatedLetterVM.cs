﻿using ClinicX.Meta;
using ClinicX.Models;
using Microsoft.EntityFrameworkCore;

namespace ClinicX.ViewModels
{
    [Keyless]
    public class DictatedLetterVM
    {
        public StaffMember staffUser { get; set; }
        public List<DictatedLettersCopies> dictatedLettersCopies  { get; set; }
        public List<DictatedLettersPatients> dictatedLettersPatients { get; set; }
        public DictatedLetters dictatedLetters { get; set; }
        public List<DictatedLetters> dictatedLettersForApproval { get; set; }
        public List<DictatedLetters> dictatedLettersForPrinting { get; set; }
        public List<Patients> patients { get; set; }
        public List<StaffMember> staffMemberList { get; set;}
        public Patients patientDetails { get; set; }        
        public ActivityItems activityDetails { get; set; }
        public ExternalFacility referrerFacility { get; set; }
        public ExternalClinician referrer { get; set; }
        public ExternalFacility GPFacility { get; set; }
        public List<ExternalFacility> facilities { get; set; }
        public List<ExternalClinician> clinicians { get; set; }
        public List<ExternalClinician> cardio { get; set; }
        public List<ExternalClinician> genetics { get; set; }
        public List<ExternalClinician> gynae { get; set; }
        public List<ExternalClinician> histo { get; set; }
        public List<ExternalClinician> screeningco { get; set; }
        public List<ExternalClinician> gps_all { get; set; }
        //Relatives
        
        //Genetics lab
        
        //Clinician (all)
        public List<string> consultants { get; set; }
        public List<string> gcs { get; set; }
        public List<string> secteams { get; set; }
        public List<string> specialities { get; set; }
    }
}
