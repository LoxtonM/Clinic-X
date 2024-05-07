using ClinicX.Data;
using ClinicX.Models;
using System.Data;

namespace ClinicX.Meta
{
    public class DocumentsData
    {       
        private readonly DocumentContext? _docContext;
        
        public DocumentsData(DocumentContext docContext)
        {            
            _docContext = docContext;
        }        
        
        
        public DocumentsContent GetDocumentDetails(int id) //Get content for a type of standard letter by its ID
        {
            var item = _docContext.DocumentsContent.FirstOrDefault(d => d.DocContentID == id);
            return item;
        }

        public DocumentsContent GetDocumentDetailsByDocCode(string docCode) //Get content for a type of standard letter by its ID
        {
            var item = _docContext.DocumentsContent.FirstOrDefault(d => d.DocCode == docCode);
            return item;
        }

        public List<Documents> GetDocumentsList() 
        {
            var docs = from d in _docContext.Documents
                       where d.TemplateInUseNow == true
                       select d;

            return docs.ToList();
        }
        
    }
}
