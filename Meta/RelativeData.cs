﻿using ClinicX.Data;
using ClinicX.Models;
using System.Data;

namespace ClinicX.Meta
{
    interface IRelativeData
    {
        public List<Relative> GetRelativesList(int id);
        public Relative GetRelativeDetails(int relID);
        public List<Relation> GetRelationsList();
        public List<Gender> GetGenderList();
    }
    public class RelativeData : IRelativeData
    {
        private readonly ClinicalContext _clinContext;      

        public RelativeData(ClinicalContext context)
        {
            _clinContext = context;
        }
                

        public List<Relative> GetRelativesList(int id) //Get list of relatives of patient by MPI
        {
            Patient patient = _clinContext.Patients.FirstOrDefault(i => i.MPI == id);
            int wmfacsID = patient.WMFACSID;

            IQueryable<Relative> relative = from r in _clinContext.Relatives
                           where r.WMFACSID == wmfacsID
                           select r;           

            return relative.ToList();
        }

        public Relative GetRelativeDetails(int relID)
        {
            Relative rel = _clinContext.Relatives.FirstOrDefault(r => r.relsid == relID);

            return rel;
        }

        public List<Relation> GetRelationsList()
        {
            IQueryable<Relation> item = from i in _clinContext.Relations
                       select i;

            return item.ToList();
        }

        public List<Gender> GetGenderList()
        {
            IQueryable<Gender> item = from i in _clinContext.Genders
                       select i;

            return item.ToList();
        }
    }
}
