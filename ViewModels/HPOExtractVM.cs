using Microsoft.AspNetCore.Mvc;
using ClinicX.Models;

namespace ClinicX.ViewModels
{
    public class HPOExtractVM
    {
        public int HPOTermID { get; set; }
        public string TermCode { get; set; }
        public String Term { get; set; }
    }
}
