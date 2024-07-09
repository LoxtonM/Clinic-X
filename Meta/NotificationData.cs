using ClinicX.Data;
using ClinicX.Models;


namespace ClinicX.Meta
{
    interface INotificationData
    {
        public string GetMessage();
    }
    public class NotificationData : INotificationData
    {        
        private readonly ClinicalContext _context;
        
        public NotificationData(ClinicalContext context)
        {            
            _context = context;
        }        
       

        public string GetMessage()
        {
            string message = ""; 

            IQueryable<Notification> messageNotifications = _context.Notifications.Where(n => n.MessageCode == "ClinicXOutage" && n.IsActive == true);

            if (messageNotifications.Count() > 0) 
            { 
                message = messageNotifications.First().Message;
            }

            return message;
        }
    }
}
