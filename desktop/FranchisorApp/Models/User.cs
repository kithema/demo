using System;

namespace FranchisorApp.Models
{
    public class User
    {
        public int user_id { get; set; }
        public string full_name { get; set; } = string.Empty;
        public string? email { get; set; }
        public string? phone { get; set; }
        public int role_id { get; set; }
        public string? role_name { get; set; }

        public string ShortName
        {
            get
            {
                var parts = full_name.Split(' ');
                if (parts.Length >= 1)
                {
                    string lastName = parts[0];
                    string initials = "";
                    for (int i = 1; i < parts.Length && i < 3; i++)
                    {
                        if (parts[i].Length > 0)
                            initials += parts[i][0] + ".";
                    }
                    return $"{lastName} {initials}";
                }
                return full_name;
            }
        }
    }
}