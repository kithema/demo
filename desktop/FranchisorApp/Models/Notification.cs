using System;

namespace FranchisorApp.Models
{
    public class Notification
    {
        public int Id { get; set; }
        public string Message { get; set; } = string.Empty;
        public NotificationType Type { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public bool IsRead { get; set; }
        public int? MachineId { get; set; } 
        public string? MachineName { get; set; } 
    }

    public enum NotificationType
    {
        Critical,   
        Warning,    
        Info,       
        Success     
    }
}