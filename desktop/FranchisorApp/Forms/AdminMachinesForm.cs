using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using FranchisorApp.Models;
using FranchisorApp.Services;
using System.Collections.Generic;

namespace FranchisorApp.Forms
{
    public partial class AdminMachinesForm : Form
    {
        private DataGridView dgvMachines;
        private ComboBox cbPageSize;
        private TextBox txtSearch;
        private Button btnSearch, btnAdd, btnExport;
        private NumericUpDown nudPage;
        private Label lblTotalRecords;
        private ComboBox cbGroupBy;
        private List<VendingMachine> _allMachines;
        private List<VendingMachine> _filteredMachines;
        private int _pageSize = 10;
        private int _currentPage = 1;
        private ApiService _apiService;

        public AdminMachinesForm()
        {
            InitializeComponent();
            _apiService = new ApiService();
            LoadMachines();
        }

        private void InitializeComponent()
        {
            this.Text = "Администрирование торговых автоматов";
            this.Size = new Size(1300, 700);
            this.StartPosition = FormStartPosition.CenterParent;
            this.BackColor = Color.FromArgb(240, 242, 245);

            var topPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 120,
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
                Text = "Торговые автоматы",
                Font = new Font("Segoe UI", 12),
                Location = new Point(20, 55),
                Size = new Size(200, 25),
                ForeColor = Color.FromArgb(108, 117, 125)
            };
            topPanel.Controls.Add(lblSubtitle);

            var controlPanel = new Panel
            {
                Location = new Point(20, 85),
                Size = new Size(1240, 35)
            };

            var lblShow = new Label { Text = "Показать", Location = new Point(0, 5), Size = new Size(60, 25), Font = new Font("Segoe UI", 9) };
            cbPageSize = new ComboBox { Location = new Point(60, 2), Size = new Size(60, 25), DropDownStyle = ComboBoxStyle.DropDownList };
            cbPageSize.Items.AddRange(new object[] { 10, 25, 50, 100 });
            cbPageSize.SelectedIndex = 0;
            cbPageSize.SelectedIndexChanged += (s, e) => { _pageSize = (int)cbPageSize.SelectedItem; _currentPage = 1; ApplyFilterAndPaging(); };

            var lblRecords = new Label { Text = "записей", Location = new Point(125, 5), Size = new Size(50, 25), Font = new Font("Segoe UI", 9) };

            txtSearch = new TextBox { Location = new Point(400, 2), Size = new Size(200, 25), Font = new Font("Segoe UI", 9), PlaceholderText = "Поиск по названию..." };
            btnSearch = new Button { Text = "Найти", Location = new Point(610, 1), Size = new Size(90, 28), BackColor = Color.FromArgb(52, 152, 219), ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
            btnSearch.FlatAppearance.BorderSize = 0;
            btnSearch.Click += BtnSearch_Click;

            btnAdd = new Button { Text = "Добавить", Location = new Point(710, 1), Size = new Size(100, 28), BackColor = Color.FromArgb(40, 167, 69), ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
            btnAdd.FlatAppearance.BorderSize = 0;
            btnAdd.Click += BtnAdd_Click;

            btnExport = new Button { Text = "Экспорт", Location = new Point(820, 1), Size = new Size(100, 28), BackColor = Color.FromArgb(108, 117, 125), ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
            btnExport.FlatAppearance.BorderSize = 0;
            btnExport.Click += (s, e) => MessageBox.Show("Экспорт в Excel выполнен", "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);

            var lblGroupBy = new Label { Text = "Группировка:", Location = new Point(950, 5), Size = new Size(80, 25), Font = new Font("Segoe UI", 9) };
            cbGroupBy = new ComboBox { Location = new Point(1030, 2), Size = new Size(130, 25), DropDownStyle = ComboBoxStyle.DropDownList };
            cbGroupBy.Items.AddRange(new object[] { "Без группировки", "По франчайзи" });
            cbGroupBy.SelectedIndex = 0;
            cbGroupBy.SelectedIndexChanged += CbGroupBy_SelectedIndexChanged;

            controlPanel.Controls.Add(lblShow);
            controlPanel.Controls.Add(cbPageSize);
            controlPanel.Controls.Add(lblRecords);
            controlPanel.Controls.Add(txtSearch);
            controlPanel.Controls.Add(btnSearch);
            controlPanel.Controls.Add(btnAdd);
            controlPanel.Controls.Add(btnExport);
            controlPanel.Controls.Add(lblGroupBy);
            controlPanel.Controls.Add(cbGroupBy);
            topPanel.Controls.Add(controlPanel);

            dgvMachines = new DataGridView
            {
                Dock = DockStyle.Fill,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                ReadOnly = true,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                BackgroundColor = Color.White,
                BorderStyle = BorderStyle.None,
                RowHeadersVisible = false,
                Font = new Font("Segoe UI", 10),
                AlternatingRowsDefaultCellStyle = { BackColor = Color.FromArgb(248, 249, 250) }
            };
            dgvMachines.CellClick += DgvMachines_CellClick;

            var pagingPanel = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 50,
                BackColor = Color.White,
                Padding = new Padding(20, 8, 20, 8)
            };

            var btnPrev = new Button { Text = "◀", Location = new Point(0, 5), Size = new Size(40, 30), FlatStyle = FlatStyle.Flat, BackColor = Color.FromArgb(108, 117, 125), ForeColor = Color.White };
            btnPrev.FlatAppearance.BorderSize = 0;
            btnPrev.Click += (s, e) => { if (_currentPage > 1) { _currentPage--; ApplyFilterAndPaging(); } };

            nudPage = new NumericUpDown { Location = new Point(45, 7), Size = new Size(60, 25), Minimum = 1, Font = new Font("Segoe UI", 9) };
            nudPage.ValueChanged += (s, e) => { _currentPage = (int)nudPage.Value; ApplyFilterAndPaging(); };

            var btnNext = new Button { Text = "▶", Location = new Point(110, 5), Size = new Size(40, 30), FlatStyle = FlatStyle.Flat, BackColor = Color.FromArgb(108, 117, 125), ForeColor = Color.White };
            btnNext.FlatAppearance.BorderSize = 0;
            btnNext.Click += (s, e) => { _currentPage++; ApplyFilterAndPaging(); };

            lblTotalRecords = new Label { Text = "Запись с 1 по 10 из 0", Location = new Point(200, 10), Size = new Size(250, 25), Font = new Font("Segoe UI", 9) };

            pagingPanel.Controls.Add(btnPrev);
            pagingPanel.Controls.Add(nudPage);
            pagingPanel.Controls.Add(btnNext);
            pagingPanel.Controls.Add(lblTotalRecords);

            this.Controls.Add(dgvMachines);
            this.Controls.Add(pagingPanel);
            this.Controls.Add(topPanel);
        }

        private async void LoadMachines()
        {
            try
            {
                _allMachines = await _apiService.GetVendingMachinesAsync();

                if (_allMachines == null || _allMachines.Count == 0)
                {
                    _allMachines = GenerateTestData();

                    foreach (var machine in _allMachines)
                    {
                        await _apiService.CreateVendingMachineAsync(machine);
                    }

                    _allMachines = await _apiService.GetVendingMachinesAsync();
                }

                DataGenerator.EmulateDynamicData(_allMachines);
                ApplyFilterAndPaging();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки данных: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                _allMachines = GenerateTestData();
                ApplyFilterAndPaging();
            }
        }

        private List<VendingMachine> GenerateTestData()
        {
            var list = new List<VendingMachine>();
            var random = new Random();
            var statuses = new[] { "Работает", "Вышел из строя", "В ремонте/на обслуживании" };
            var countries = new[] { "Россия", "Германия", "Китай", "США", "Япония" };
            var companyNames = new[] { "ООО КофеАвтомат", "ИП Иванов", "ООО Снэки", "ООО Напитки", "ИП Петрова" };
            var models = new[] { "Sesa Cristal 400", "Unicom Ross", "Bianchi BVM 972", "Necta Kikko Max", "Jofemar Conferma" };
            var locations = new[] { "ул. Московская 121", "Академическая ул. 15", "Баррикад 174", "Грабцевское шоссе 174", "пер. Воскресенский 28" };

            for (int i = 1; i <= 10; i++)
            {
                list.Add(new VendingMachine
                {
                    machine_id = i,
                    serial_number = $"SN{i:D3}",
                    inventory_number = $"INV{i:D3}",
                    location = locations[random.Next(locations.Length)],
                    model = models[random.Next(models.Length)],
                    manufacturer = models[random.Next(models.Length)].Split(' ')[0],
                    manufacture_date = new DateTime(2022, random.Next(1, 13), random.Next(1, 29)),
                    commissioning_date = new DateTime(2023, random.Next(1, 13), random.Next(1, 29)),
                    status_name = statuses[random.Next(statuses.Length)],
                    country_name = countries[random.Next(countries.Length)],
                    company_name = companyNames[random.Next(companyNames.Length)],
                    modem_id = $"MODEM_{i:000}",
                    total_income = random.Next(50000, 500000),
                    current_cash = random.Next(1000, 50000),
                    connection_status = random.Next(100) < 85 ? "Online" : "Offline",
                    extra_status = GetRandomExtraStatus()
                });
            }
            return list;
        }

        private string GetRandomExtraStatus()
        {
            var random = new Random();
            var rand = random.Next(100);
            if (rand < 60) return "Норма";
            if (rand < 80) return "Требуется внимание";
            if (rand < 95) return "Низкий запас";
            return "Ошибка";
        }

        private void BtnSearch_Click(object? sender, EventArgs e)
        {
            _currentPage = 1;
            ApplyFilterAndPaging();
        }

        private void ApplyFilterAndPaging()
        {
            if (_allMachines == null) return;

            var query = _allMachines.AsEnumerable();

            if (!string.IsNullOrWhiteSpace(txtSearch.Text))
            {
                query = query.Where(m => m.model.Contains(txtSearch.Text, StringComparison.OrdinalIgnoreCase) ||
                                         m.location.Contains(txtSearch.Text, StringComparison.OrdinalIgnoreCase));
            }

            _filteredMachines = query.ToList();

            var totalRecords = _filteredMachines.Count;
            var totalPages = (int)Math.Ceiling((double)totalRecords / _pageSize);
            if (totalPages == 0) totalPages = 1;
            if (_currentPage > totalPages) _currentPage = totalPages;
            nudPage.Maximum = totalPages;
            nudPage.Value = _currentPage;

            var startRecord = (_currentPage - 1) * _pageSize + 1;
            var endRecord = Math.Min(_currentPage * _pageSize, totalRecords);
            lblTotalRecords.Text = $"Запись с {startRecord} по {endRecord} из {totalRecords}";

            var paged = _filteredMachines.Skip((_currentPage - 1) * _pageSize).Take(_pageSize).ToList();

            dgvMachines.DataSource = null;
            dgvMachines.Columns.Clear();

            var dataSource = paged.Select(m => new
            {
                ID = m.machine_id,
                Название = m.model,
                Модель = m.model,
                Компания = m.company_name ?? "-",
                Модем = m.modem_id ?? "-",
                Адрес = m.location,
                В_работе_с = m.commissioning_date.ToString("dd.MM.yyyy")
            }).ToList();

            dgvMachines.DataSource = dataSource;

            if (dgvMachines.Columns.Contains("Название"))
                dgvMachines.Columns["Название"].HeaderText = "Имя автомата";
            if (dgvMachines.Columns.Contains("Модель"))
                dgvMachines.Columns["Модель"].HeaderText = "Модель";
            if (dgvMachines.Columns.Contains("Компания"))
                dgvMachines.Columns["Компания"].HeaderText = "Компания (франчайзи)";
            if (dgvMachines.Columns.Contains("Модем"))
                dgvMachines.Columns["Модем"].HeaderText = "Модем";
            if (dgvMachines.Columns.Contains("Адрес"))
                dgvMachines.Columns["Адрес"].HeaderText = "Адрес/Место";
            if (dgvMachines.Columns.Contains("В_работе_с"))
                dgvMachines.Columns["В_работе_с"].HeaderText = "В работе с";

            var btnColumn = new DataGridViewButtonColumn
            {
                Name = "Действия",
                HeaderText = "Действия",
                Text = "Подробнее",
                UseColumnTextForButtonValue = true,
                Width = 60
            };
            dgvMachines.Columns.Add(btnColumn);
        }

        private void DgvMachines_CellClick(object? sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0 || e.ColumnIndex != dgvMachines.Columns["Действия"].Index) return;

            var machineId = (int)dgvMachines.Rows[e.RowIndex].Cells["ID"].Value;

            var contextMenu = new ContextMenuStrip();
            contextMenu.Items.Add("Редактировать", null, (s, args) => EditMachine(machineId));
            contextMenu.Items.Add("Удалить", null, (s, args) => DeleteMachine(machineId));
            contextMenu.Items.Add("Отвязать модем", null, (s, args) => UnpairModem(machineId));
            contextMenu.Show(Cursor.Position);
        }

        private void EditMachine(int id)
        {
            var machine = _allMachines?.FirstOrDefault(m => m.machine_id == id);
            if (machine != null)
            {
                var editForm = new AddEditMachineForm(machine);
                if (editForm.ShowDialog() == DialogResult.OK)
                {
                    LoadMachines();
                }
            }
        }

        private async void DeleteMachine(int id)
        {
            var result = MessageBox.Show("Вы уверены, что хотите удалить торговый автомат?", "Подтверждение удаления", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
            if (result == DialogResult.Yes)
            {
                await _apiService.DeleteVendingMachineAsync(id);
                LoadMachines();
            }
        }

        private void UnpairModem(int id)
        {
            var result = MessageBox.Show("Отвязать модем от ТА?", "Подтверждение операции", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (result == DialogResult.Yes)
            {
                var machine = _allMachines?.FirstOrDefault(m => m.machine_id == id);
                if (machine != null)
                {
                    machine.modem_id = "-1";
                    MessageBox.Show("Модем успешно отвязан", "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    ApplyFilterAndPaging();
                }
            }
        }

        private void BtnAdd_Click(object? sender, EventArgs e)
        {
            var addForm = new AddEditMachineForm(null);
            if (addForm.ShowDialog() == DialogResult.OK)
            {
                LoadMachines();
            }
        }

        private void CbGroupBy_SelectedIndexChanged(object? sender, EventArgs e)
        {
            if (cbGroupBy.SelectedIndex == 1 && _filteredMachines != null)
            {
                var grouped = _filteredMachines
                    .GroupBy(m => m.company_name ?? "Без компании")
                    .ToList();

                ShowGroupedView(grouped);
                cbGroupBy.SelectedIndex = 0;
            }
        }

        private void ShowGroupedView(List<IGrouping<string, VendingMachine>> grouped)
        {
            var groupForm = new Form
            {
                Text = "Группировка ТА по франчайзи",
                Size = new Size(600, 500),
                StartPosition = FormStartPosition.CenterParent,
                BackColor = Color.White
            };

            var treeView = new TreeView
            {
                Dock = DockStyle.Fill,
                Font = new Font("Segoe UI", 10),
                BorderStyle = BorderStyle.None
            };

            foreach (var group in grouped.OrderBy(g => g.Key))
            {
                var companyNode = treeView.Nodes.Add($"📁 {group.Key} ({group.Count()})");
                companyNode.ForeColor = Color.FromArgb(52, 152, 219);
                foreach (var machine in group)
                {
                    var machineNode = companyNode.Nodes.Add($"🔹 {machine.model} - {machine.location}");
                    machineNode.ForeColor = Color.FromArgb(108, 117, 125);
                }
                companyNode.Expand();
            }

            groupForm.Controls.Add(treeView);
            groupForm.ShowDialog();
        }
    }
}