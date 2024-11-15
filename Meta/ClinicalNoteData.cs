using ClinicX.Data;
using ClinicX.Models;
using System.Data;

namespace ClinicX.Meta
{
    interface IClinicalNoteData
    {
        public Note GetClinicalNoteDetails(int? noteID);
        public List<Note> GetClinicalNoteList(int? mpi);
        public List<ClinicalNoteType> GetNoteTypesList();
    }
    public class ClinicalNoteData : IClinicalNoteData
    {
        private readonly ClinicXContext _clinContext;
        
        public ClinicalNoteData(ClinicXContext context)
        {
            _clinContext = context;
        }

        public Note GetClinicalNoteDetails(int? noteID) //Get details of a clinical note by ClinicalNotesID
        {
            Note note = _clinContext.ClinicalNotes.FirstOrDefault(i => i.ClinicalNoteID == noteID);

            return note;
        }

        public List<Note> GetClinicalNoteList(int? mpi) //Get list of clinical notes by MPI
        {
            IQueryable<Note> notes = from n in _clinContext.ClinicalNotes
                        where n.MPI == mpi
                        orderby n.CreatedDate
                        select n;

            return notes.ToList();
        }

        public List<ClinicalNoteType> GetNoteTypesList() //Get list of possible types of clinical note
        {
            IQueryable<ClinicalNoteType> notetypes = from t in _clinContext.NoteTypes
                            where t.NoteInUse == true
                            select t;

            return notetypes.ToList();
        }
    }
}
