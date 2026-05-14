using System;

namespace FranchisorApp.Models
{
    public class Maintenance
    {
        public int maintenance_id { get; set; }
        public int? machine_id { get; set; }
        public DateTime maintenance_date { get; set; }
        public string description { get; set; } = string.Empty;
        public string? problems { get; set; }
        public string executor { get; set; } = string.Empty;
    }
}