using ClinicX.Models;
using Microsoft.EntityFrameworkCore;

namespace ClinicX.ViewModels
{
    [Keyless]
    public class DictatedLetterVM
    {
        public List<DictatedLettersCopies> dictatedLettersCopies  { get; set; }
        public List<DictatedLettersPatients> dictatedLettersPatients { get; set; }
        public DictatedLetters dictatedLetters { get; set; }
        public List<Patients> patients { get; set; }
    }
}
