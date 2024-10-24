﻿using ClinicX.Data;
using ClinicX.Models;
using System.Data;

namespace ClinicX.Meta
{
    interface IGeneChangeData
    {
        public List<GeneChange> GetGeneChangeList();
    }
    public class GeneChangeData : IGeneChangeData
    {
        private readonly ClinicalContext _clinContext;

        public GeneChangeData(ClinicalContext context)
        {
            _clinContext = context;
        }       

        public List<GeneChange> GetGeneChangeList()
        {
            IQueryable<GeneChange> geneChanges = from g in _clinContext.GeneChange
                                              where g.Inuse == true
                          orderby g.GeneChangeDescription
                         select g;

            return geneChanges.ToList();
        }

        
    }
}