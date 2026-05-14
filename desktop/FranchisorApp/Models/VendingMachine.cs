using System;

namespace FranchisorApp.Models
{
    public class VendingMachine
    {
        public int machine_id { get; set; }
        public string serial_number { get; set; } = string.Empty;
        public string inventory_number { get; set; } = string.Empty;
        public string location { get; set; } = string.Empty;
        public string model { get; set; } = string.Empty;
        public string manufacturer { get; set; } = string.Empty;
        public DateTime manufacture_date { get; set; }
        public DateTime commissioning_date { get; set; }
        public DateTime? last_verification_date { get; set; }
        public int? verification_interval_months { get; set; }
        public int? resource_hours { get; set; }
        public DateTime? next_maintenance_date { get; set; }
        public int? maintenance_time_hours { get; set; }
        public int? status_id { get; set; }
        public string? status_name { get; set; }
        public int? country_id { get; set; }
        public string? country_name { get; set; }
        public DateTime? inventory_date { get; set; }
        public string? last_verifier_employee { get; set; }
        public decimal total_income { get; set; }
        public DateTime? next_verification_date { get; set; }

        public string? modem_id { get; set; } = string.Empty;
        public string? company_name { get; set; } = string.Empty;
        public decimal current_cash { get; set; }
        public string? connection_status { get; set; } = "Online";
        public string? extra_status { get; set; } = "Норма";
    }
}