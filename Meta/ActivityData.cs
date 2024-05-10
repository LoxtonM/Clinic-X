﻿using ClinicX.Data;
using ClinicX.Models;

namespace ClinicX.Meta
{
    interface IActivityData
    {
        public ActivityItems GetActivityDetails(int id);
        public List<ActivityItems> GetClinicDetailsList(int refID);
    }
    public class ActivityData : IActivityData
    {
        private readonly ClinicalContext _clinContext;

        public ActivityData(ClinicalContext context)
        {
            _clinContext = context;
        }
        
        public ActivityItems GetActivityDetails(int id) //Get details of any activity item by RefID
        {
            var referral = _clinContext.ActivityItems?.FirstOrDefault(i => i.RefID == id);
            return referral;
        }

        public List<ActivityItems> GetClinicDetailsList(int refID) //Get details of an appointment by the RefID for editing
        {
            var cl = from c in _clinContext.ActivityItems
                     where c.RefID == refID
                     select c;

            List<ActivityItems> clinics = new List<ActivityItems>();

            foreach (var c in cl)
            {
                //Because the model can't handle spaces, we have to strip them out... but it bombs out if the value is null, so we have to
                //use this incredibly convoluted method to pass it to the HTML!!!
                string letterReq = "";
                string counseled = "";

                if (c.COUNSELED != null)
                {
                    counseled = c.COUNSELED.Replace(" ", "_");
                }
                else
                {
                    counseled = c.COUNSELED;
                }

                if (c.LetterReq != null)
                {
                    letterReq = c.LetterReq.Replace(" ", "_");
                }
                else
                {
                    letterReq = c.LetterReq;
                }

                clinics.Add(new ActivityItems()
                {
                    RefID = c.RefID,
                    BOOKED_DATE = c.BOOKED_DATE,
                    BOOKED_TIME = c.BOOKED_TIME,
                    TYPE = c.TYPE,
                    STAFF_CODE_1 = c.STAFF_CODE_1,
                    FACILITY = c.FACILITY,
                    COUNSELED = counseled,
                    SEEN_BY = c.SEEN_BY,
                    NOPATIENTS_SEEN = c.NOPATIENTS_SEEN,
                    LetterReq = letterReq,
                    ARRIVAL_TIME = c.ARRIVAL_TIME,
                    ClockStop = c.ClockStop
                });
            }

            return clinics;
        }
    }
}
