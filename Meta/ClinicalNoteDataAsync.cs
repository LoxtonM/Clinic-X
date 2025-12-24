using ClinicX.Data;
using ClinicX.Models;
using Microsoft.EntityFrameworkCore;
using System.Data;

namespace ClinicX.Meta
{
    public interface IClinicalNoteDataAsync
    {
        public Task<Note> GetClinicalNoteDetails(int? noteID);
        public Task<List<Note>> GetClinicalNoteList(int? mpi);
        public Task<List<ClinicalNoteType>> GetNoteTypesList();
    }
    public class ClinicalNoteDataAsync : IClinicalNoteDataAsync
    {
        private readonly ClinicXContext _clinContext;
        
        public ClinicalNoteDataAsync(ClinicXContext context)
        {
            _clinContext = context;
        }

        public async Task<Note> GetClinicalNoteDetails(int? noteID) //Get details of a clinical note by ClinicalNotesID
        {
            Note note = await _clinContext.ClinicalNotes.FirstAsync(i => i.ClinicalNoteID == noteID);

            return note;
        }

        public async Task<List<Note>> GetClinicalNoteList(int? mpi) //Get list of clinical notes by MPI
        {
            IQueryable<Note> notes = from n in _clinContext.ClinicalNotes
                        where n.MPI == mpi
                        orderby n.CreatedDate
                        select n;

            return await notes.ToListAsync();
        }

        public async Task<List<ClinicalNoteType>> GetNoteTypesList() //Get list of possible types of clinical note
        {
            IQueryable<ClinicalNoteType> notetypes = from t in _clinContext.NoteTypes
                            where t.NoteInUse == true
                            select t;

            return await notetypes.ToListAsync();
        }
    }
}
