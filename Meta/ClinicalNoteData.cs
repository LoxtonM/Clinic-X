using ClinicX.Data;
using ClinicX.Models;
using System.Data;

namespace ClinicX.Meta
{
    public class ClinicalNoteData
    {
        private readonly ClinicalContext _clinContext;
        
        public ClinicalNoteData(ClinicalContext context)
        {
            _clinContext = context;
        }

        public ClinicalNotes GetClinicalNoteDetails(int? noteID) //Get details of a clinical note by ClinicalNotesID
        {
            var note = _clinContext.ClinicalNotes.FirstOrDefault(i => i.ClinicalNoteID == noteID);

            return note;
        }

        public List<ClinicalNotes> GetClinicalNoteList(int? mpi) //Get list of clinical notes by MPI
        {
            var notes = from n in _clinContext.ClinicalNotes
                        where n.MPI == mpi
                        select n;

            return notes.ToList();
        }

        public List<NoteTypeList> GetNoteTypesList() //Get list of possible types of clinical note
        {
            var notetypes = from t in _clinContext.NoteTypes
                            where t.NoteInUse == true
                            select t;

            return notetypes.ToList();
        }
    }
}
