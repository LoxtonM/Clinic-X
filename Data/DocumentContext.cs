using Microsoft.EntityFrameworkCore;
using ClinicX.Models;

namespace ClinicX.Data
{
    public class DocumentContext : DbContext
    { 
        public DocumentContext(DbContextOptions<DocumentContext> options) : base(options) { }
        public DbSet<StaffMember> StaffMember{ get; set; }
        public DbSet<Patient> Patient { get; set; }
        public DbSet<DocumentsContent> DocumentsContent { get; set; }
        public DbSet<Referrer> Referrer { get; set; }
        public DbSet<Facility> Facility { get; set; }
        public DbSet<Constants> Constants { get; set; }
    }
}