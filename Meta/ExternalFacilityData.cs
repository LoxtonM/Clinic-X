﻿using ClinicX.Data;
using ClinicX.Models;
using System.Data;

namespace ClinicX.Meta
{
    interface IExternalFacilityData
    {
        public ExternalFacility GetFacilityDetails(string sref);
        public List<ExternalFacility> GetFacilityList();
    }
    public class ExternalFacilityData : IExternalFacilityData
    {
        private readonly ClinicalContext _clinContext;
      
        public ExternalFacilityData(ClinicalContext context)
        {
            _clinContext = context;
        }
        

        public ExternalFacility GetFacilityDetails(string sref) //Get details of external/referring facility
        {
            ExternalFacility item = _clinContext.ExternalFacility.FirstOrDefault(f => f.MasterFacilityCode == sref);
            return item;
        }        

        public List<ExternalFacility> GetFacilityList() //Get list of all external/referring facilities
        {
            IQueryable<ExternalFacility> facilities = from rf in _clinContext.ExternalFacility
                             where rf.NONACTIVE == 0
                             orderby rf.NAME
                             select rf;

            return facilities.ToList();
        }

    }
}
