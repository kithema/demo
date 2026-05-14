using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using FranchisorApp.Models;
using FranchisorApp.Services;
using System.Collections.Generic;

namespace FranchisorApp.Forms
{
    public partial class MachineMonitorForm : Form
    {
        private DataGridView dgvMonitors;
        private ComboBox cbStatusFilter, cbConnectionFilter, cbExtraStatusFilter;
        private Button btnApplyFilter, btnResetFilter, btnExport;
        private Label lblTotalMachines, lblTotalCash, lblLastUpdate;
        private Panel emptyPanel;
        private List<VendingMachine> _allMachines;
        private List<VendingMachine> _filteredMachines;
        private ApiService _apiService;
        private Random _random;

        public MachineMonitorForm()
        {
            InitializeComponent();
            _apiService = new ApiService();
            _random = new Random();
            LoadData();
        }

        private void InitializeComponent()
        {
            this.Text = "Монитор ТА - VendoMatic";
            this.Size = new Size(1400, 750);
            this.StartPosition = FormStartPosition.CenterParent;
            this.BackColor = Color.FromArgb(240, 242, 245);

            var topPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 170,
                Padding = new Padding(20),
                BackColor = Color.White
            };

            var lblTitle = new Label
            {
                Text = "ООО Торговые Автоматы",
                Font = new Font("Segoe UI", 18, FontStyle.Bold),
                Location = new Point(20, 15),
                Size = new Size(400, 35),
                ForeColor = Color.FromArgb(44, 62, 80)
            };
            topPanel.Controls.Add(lblTitle);

            var lblSubtitle = new Label
            {
                Text = "Монитор торговых автоматов",
                Font = new Font("Segoe UI", 12),
                Location = new Point(20, 55),
                Size = new Size(300, 25),
                ForeColor = Color.FromArgb(108, 117, 125)
            };
            topPanel.Controls.Add(lblSubtitle);

            lblLastUpdate = new Label
            {
                Text = "данные актуальны на " + DateTime.Now.ToString("HH:mm:ss"),
                Font = new Font("Segoe UI", 9),
                Location = new Point(20, 82),
                Size = new Size(300, 20),
                ForeColor = Color.FromArgb(108, 117, 125)
            };
            topPanel.Controls.Add(lblLastUpdate);

            var filterPanel = new Panel
            {
                Location = new Point(20, 110),
                Size = new Size(1340, 50)
            };

            var lblStatus = new Label { Text = "Общее состояние:", Location = new Point(0, 5), Size = new Size(100, 20), Font = new Font("Segoe UI", 9, FontStyle.Bold) };
            cbStatusFilter = new ComboBox { Location = new Point(0, 25), Size = new Size(150, 25), DropDownStyle = ComboBoxStyle.DropDownList };
            cbStatusFilter.Items.AddRange(new object[] { "Все", "Работает", "Не работает", "На обслуживании" });
            cbStatusFilter.SelectedIndex = 0;

            var lblConnection = new Label { Text = "Подключение:", Location = new Point(170, 5), Size = new Size(100, 20), Font = new Font("Segoe UI", 9, FontStyle.Bold) };
            cbConnectionFilter = new ComboBox { Location = new Point(170, 25), Size = new Size(150, 25), DropDownStyle = ComboBoxStyle.DropDownList };
            cbConnectionFilter.Items.AddRange(new object[] { "Все", "Online", "Offline" });
            cbConnectionFilter.SelectedIndex = 0;

            var lblExtra = new Label { Text = "Дополнительные статусы:", Location = new Point(340, 5), Size = new Size(140, 20), Font = new Font("Segoe UI", 9, FontStyle.Bold) };
            cbExtraStatusFilter = new ComboBox { Location = new Point(340, 25), Size = new Size(150, 25), DropDownStyle = ComboBoxStyle.DropDownList };
            cbExtraStatusFilter.Items.AddRange(new object[] { "Все", "Норма", "Требуется внимание", "Низкий запас", "Ошибка" });
            cbExtraStatusFilter.SelectedIndex = 0;

            btnApplyFilter = new Button { Text = "Применить", Location = new Point(520, 22), Size = new Size(100, 28), BackColor = Color.FromArgb(52, 152, 219), ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
            btnApplyFilter.FlatAppearance.BorderSize = 0;
            btnApplyFilter.Click += BtnApplyFilter_Click;

            btnResetFilter = new Button { Text = "Очистить", Location = new Point(630, 22), Size = new Size(100, 28), BackColor = Color.FromArgb(108, 117, 125), ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
            btnResetFilter.FlatAppearance.BorderSize = 0;
            btnResetFilter.Click += (s, e) => { cbStatusFilter.SelectedIndex = 0; cbConnectionFilter.SelectedIndex = 0; cbExtraStatusFilter.SelectedIndex = 0; ApplyFilters(); };

            btnExport = new Button { Text = "Экспорт в Excel", Location = new Point(750, 22), Size = new Size(120, 28), BackColor = Color.FromArgb(40, 167, 69), ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
            btnExport.FlatAppearance.BorderSize = 0;
            btnExport.Click += (s, e) => MessageBox.Show("Экспорт выполнен", "Успех");

            filterPanel.Controls.Add(lblStatus);
            filterPanel.Controls.Add(cbStatusFilter);
            filterPanel.Controls.Add(lblConnection);
            filterPanel.Controls.Add(cbConnectionFilter);
            filterPanel.Controls.Add(lblExtra);
            filterPanel.Controls.Add(cbExtraStatusFilter);
            filterPanel.Controls.Add(btnApplyFilter);
            filterPanel.Controls.Add(btnResetFilter);
            filterPanel.Controls.Add(btnExport);
            topPanel.Controls.Add(filterPanel);

            var summaryPanel = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 45,
                BackColor = Color.FromArgb(248, 249, 250),
                Padding = new Padding(20, 8, 20, 8)
            };

            lblTotalMachines = new Label { Text = "Итого автоматов: 0", Location = new Point(20, 12), Size = new Size(350, 25), Font = new Font("Segoe UI", 10, FontStyle.Bold) };
            lblTotalCash = new Label { Text = "Денег в автоматах: 0 руб.", Location = new Point(380, 12), Size = new Size(300, 25), Font = new Font("Segoe UI", 10, FontStyle.Bold) };

            summaryPanel.Controls.Add(lblTotalMachines);
            summaryPanel.Controls.Add(lblTotalCash);

            dgvMonitors = new DataGridView
            {
                Dock = DockStyle.Fill,
                AllowUserToAddRows = false,
                ReadOnly = true,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                BackgroundColor = Color.White,
                BorderStyle = BorderStyle.None,
                RowHeadersVisible = false,
                Font = new Font("Segoe UI", 9),
                AlternatingRowsDefaultCellStyle = { BackColor = Color.FromArgb(248, 249, 250) },
                RowTemplate = { Height = 60 }
            };

            emptyPanel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.White,
                Visible = false
            };
            var lblEmpty = new Label
            {
                Text = "Нет активных торговых автоматов, соответствующих заданному фильтру",
                Font = new Font("Segoe UI", 12),
                ForeColor = Color.Gray,
                TextAlign = ContentAlignment.MiddleCenter,
                Dock = DockStyle.Fill
            };
            emptyPanel.Controls.Add(lblEmpty);

            var tableContainer = new Panel { Dock = DockStyle.Fill };
            tableContainer.Controls.Add(dgvMonitors);
            tableContainer.Controls.Add(emptyPanel);

            this.Controls.Add(tableContainer);
            this.Controls.Add(summaryPanel);
            this.Controls.Add(topPanel);
        }

        private async void LoadData()
        {
            try
            {
                _allMachines = await _apiService.GetVendingMachinesAsync();

                if (_allMachines == null || _allMachines.Count == 0)
                {
                    _allMachines = GenerateTestData();
                }
                else
                {
                    DataGenerator.EmulateDynamicData(_allMachines);
                }
                ApplyFilters();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки данных: {ex.Message}\nИспользую тестовые данные", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                _allMachines = GenerateTestData();
                ApplyFilters();
            }
        }

        private List<VendingMachine> GenerateTestData()
        {
            var list = new List<VendingMachine>();
            var statuses = new[] { "Работает", "Вышел из строя", "В ремонте/на обслуживании" };
            var connections = new[] { "Online", "Offline" };
            var extraStatuses = new[] { "Норма", "Требуется внимание", "Низкий запас", "Ошибка" };
            var companyNames = new[] { "ООО КофеАвтомат", "ИП Иванов", "ООО Снэки", "ООО Напитки", "ИП Петрова" };
            var models = new[] { "Sesa Cristal 400", "Unicom Ross", "Bianchi BVM 972", "Necta Kikko Max", "Jofemar Conferma" };
            var locations = new[] { "ул. Московская 121", "Академическая ул. 15", "Баррикад 174", "Грабцевское шоссе 174", "пер. Воскресенский 28" };

            for (int i = 1; i <= 12; i++)
            {
                var status = statuses[_random.Next(statuses.Length)];
                list.Add(new VendingMachine
                {
                    machine_id = 900000 + i,
                    model = models[_random.Next(models.Length)],
                    location = locations[_random.Next(locations.Length)],
                    status_name = status,
                    company_name = companyNames[_random.Next(companyNames.Length)],
                    modem_id = $"MODEM_{900000 + i}",
                    current_cash = _random.Next(1000, 30000),
                    commissioning_date = new DateTime(2023, _random.Next(1, 13), _random.Next(1, 29)),
                    connection_status = connections[_random.Next(connections.Length)],
                    extra_status = extraStatuses[_random.Next(extraStatuses.Length)]
                });
            }
            return list;
        }

        private void BtnApplyFilter_Click(object? sender, EventArgs e)
        {
            ApplyFilters();
        }

        private void ApplyFilters()
        {
            if (_allMachines == null) return;

            var query = _allMachines.AsEnumerable();

            if (cbStatusFilter.SelectedIndex != 0)
            {
                string statusValue = cbStatusFilter.SelectedItem.ToString();
                if (statusValue == "Работает")
                    query = query.Where(m => m.status_name == "Работает");
                else if (statusValue == "Не работает")
                    query = query.Where(m => m.status_name == "Вышел из строя");
                else if (statusValue == "На обслуживании")
                    query = query.Where(m => m.status_name == "В ремонте/на обслуживании");
            }

            if (cbConnectionFilter.SelectedIndex != 0)
            {
                string conn = cbConnectionFilter.SelectedItem.ToString();
                query = query.Where(m => m.connection_status == conn);
            }

            if (cbExtraStatusFilter.SelectedIndex != 0)
            {
                string extra = cbExtraStatusFilter.SelectedItem.ToString();
                query = query.Where(m => m.extra_status == extra);
            }

            _filteredMachines = query.ToList();

            var workingCount = _filteredMachines.Count(m => m.status_name == "Работает");
            var brokenCount = _filteredMachines.Count(m => m.status_name == "Вышел из строя");
            var maintenanceCount = _filteredMachines.Count(m => m.status_name == "В ремонте/на обслуживании");
            lblTotalMachines.Text = $"Итого автоматов: {_filteredMachines.Count} ({workingCount}/ {brokenCount}/ {maintenanceCount})";

            var totalCash = _filteredMachines.Sum(m => m.current_cash);
            lblTotalCash.Text = $"Денег в автоматах: {totalCash:N0} руб.";

            if (_filteredMachines.Count == 0)
            {
                dgvMonitors.Visible = false;
                emptyPanel.Visible = true;
                return;
            }

            dgvMonitors.Visible = true;
            emptyPanel.Visible = false;

            dgvMonitors.Rows.Clear();
            dgvMonitors.Columns.Clear();

            dgvMonitors.Columns.Add(new DataGridViewTextBoxColumn { Name = "Number", HeaderText = "№", Width = 45 });
            dgvMonitors.Columns.Add(new DataGridViewTextBoxColumn { Name = "TP", HeaderText = "ТП", Width = 200 });
            dgvMonitors.Columns.Add(new DataGridViewTextBoxColumn { Name = "Connection", HeaderText = "Связь", Width = 80 });
            dgvMonitors.Columns.Add(new DataGridViewTextBoxColumn { Name = "Load", HeaderText = "Загрузка", Width = 80 });
            dgvMonitors.Columns.Add(new DataGridViewTextBoxColumn { Name = "Cash", HeaderText = "Денежные средства", Width = 150 });
            dgvMonitors.Columns.Add(new DataGridViewTextBoxColumn { Name = "Equipment", HeaderText = "Оборудование", Width = 120 });
            dgvMonitors.Columns.Add(new DataGridViewTextBoxColumn { Name = "Info", HeaderText = "Информация", Width = 150 });

            int index = 1;
            foreach (var m in _filteredMachines)
            {
                int load = _random.Next(10, 100);
                int cashInMachine = (int)m.current_cash;
                string connectionText = m.connection_status == "Online" ? "Online" : "Offline";
                string loadText = load + "%";
                string cashText = $"{cashInMachine:N0} руб.";
                string equipmentText = m.modem_id ?? "-";
                string infoText = m.extra_status ?? "Норма";

                dgvMonitors.Rows.Add(
                    index++,
                    $"{m.machine_id} - \"{m.company_name}\"\n{m.model}\n{m.location}",
                    connectionText,
                    loadText,
                    cashText,
                    equipmentText,
                    infoText
                );
            }

            dgvMonitors.CellFormatting += DgvMonitors_CellFormatting;
        }

        private void DgvMonitors_CellFormatting(object? sender, DataGridViewCellFormattingEventArgs e)
        {
            if (e.RowIndex < 0) return;

            if (dgvMonitors.Columns[e.ColumnIndex].Name == "Connection" && e.Value != null)
            {
                string conn = e.Value.ToString();
                if (conn == "Online")
                {
                    e.CellStyle.ForeColor = Color.FromArgb(46, 204, 113);
                    e.CellStyle.Font = new Font("Segoe UI", 9, FontStyle.Bold);
                }
                else
                {
                    e.CellStyle.ForeColor = Color.FromArgb(231, 76, 60);
                    e.CellStyle.Font = new Font("Segoe UI", 9, FontStyle.Bold);
                }
            }

            if (dgvMonitors.Columns[e.ColumnIndex].Name == "Info" && e.Value != null)
            {
                string info = e.Value.ToString();
                if (info == "Норма")
                {
                    e.CellStyle.ForeColor = Color.FromArgb(46, 204, 113);
                }
                else if (info == "Требуется внимание")
                {
                    e.CellStyle.ForeColor = Color.FromArgb(241, 196, 15);
                }
                else if (info == "Низкий запас")
                {
                    e.CellStyle.ForeColor = Color.FromArgb(52, 152, 219);
                }
                else if (info == "Ошибка")
                {
                    e.CellStyle.ForeColor = Color.FromArgb(231, 76, 60);
                }
            }
        }
    }
}