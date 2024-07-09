using ClinicX.Data;
using ClinicX.Models;


namespace ClinicX.Meta
{
    interface IConstantsData
    {
        public string GetConstant(string constantCode, int constantValue);
    }
    public class ConstantsData : IConstantsData
    {        
        private readonly DocumentContext? _docContext;
        
        public ConstantsData(DocumentContext docContext)
        {            
            _docContext = docContext;
        }        
       

        public string GetConstant(string constantCode, int constantValue)
        {
            Constant item = _docContext.Constants.FirstOrDefault(c => c.ConstantCode == constantCode);

            if (constantValue == 1)
            {
                return item.ConstantValue;
            }
            else
            {
                return item.ConstantValue2;
            }
        }
    }
}
