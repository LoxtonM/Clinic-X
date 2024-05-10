using ClinicX.Data;


namespace ClinicX.Meta
{
    public class NotificationData
    {        
        private readonly ClinicalContext _context;
        
        public NotificationData(ClinicalContext context)
        {            
            _context = context;
        }        
       

        public string GetMessage()
        {
            string message = ""; 

            var messageNotifications = _context.Notifications.Where(n => n.MessageCode == "ClinicXOutage" && n.IsActive == true);

            if (messageNotifications.Count() > 0) 
            { 
                message = messageNotifications.First().Message;
            }

            return message;
        }
    }
}
