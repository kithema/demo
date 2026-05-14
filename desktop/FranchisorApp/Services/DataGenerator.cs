using System;
using System.Collections.Generic;
using FranchisorApp.Models;

namespace FranchisorApp.Services
{
    public static class DataGenerator
    {
        private static readonly Random _random = new Random();
        private static readonly string[] _extraStatuses = { "Норма", "Требуется внимание", "Низкий запас", "Ошибка" };
        private static readonly string[] _connectionStatuses = { "Online", "Offline" };
        private static readonly string[] _companyNames = { "ООО КофеАвтомат", "ИП Иванов", "ООО Снэки", "ООО Напитки", "ИП Петрова" };

        public static void EmulateDynamicData(List<VendingMachine> machines)
        {
            for (int i = 0; i < machines.Count; i++)
            {
                var machine = machines[i];

                machine.company_name = _companyNames[machine.machine_id % _companyNames.Length];

                machine.modem_id = machine.modem_id == "-1" ? "-1" : $"MODEM_{machine.machine_id:000}";

                machine.current_cash = _random.Next(0, 30000);
                machine.connection_status = _random.Next(100) < 85 ? "Online" : "Offline";

                var extraIndex = _random.Next(100);
                if (extraIndex < 60) machine.extra_status = "Норма";
                else if (extraIndex < 80) machine.extra_status = "Требуется внимание";
                else if (extraIndex < 95) machine.extra_status = "Низкий запас";
                else machine.extra_status = "Ошибка";
            }
        }

        public static int GetRandomLoad()
        {
            return _random.Next(10, 100);
        }
    }
}