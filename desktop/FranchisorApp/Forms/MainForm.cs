using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;
using FranchisorApp.Services;
using FranchisorApp.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FranchisorApp.Forms
{
    public partial class MainForm : Form
    {
        private Panel sidebar;
        private Panel contentPanel;
        private Button btnHome, btnMonitor, btnAdmin;
        private Label lblUserInfo;
        private PictureBox pbUserAvatar;
        private Panel userDropdownPanel;  
        private User? currentUser;
        private ApiService _apiService;
        private NotificationService _notifyService;
        private System.Windows.Forms.Timer dropdownTimer;

        public MainForm(User user)
        {
            _apiService = new ApiService();
            _notifyService = NotificationService.Instance;
            currentUser = user;

            InitializeComponent();

            _notifyService.Initialize(this);
            LoadUserData();
            ShowHomePage();
        }

        private void InitializeComponent()
        {
            this.Text = "VendoMatic - Личный кабинет франчайзера";
            this.WindowState = FormWindowState.Maximized;
            this.BackColor = Color.FromArgb(240, 242, 245);

            sidebar = new Panel
            {
                Width = 260,
                Dock = DockStyle.Left,
                BackColor = Color.FromArgb(44, 62, 80)
            };

            var lblTitle = new Label
            {
                Text = "VendoMatic",
                Font = new Font("Segoe UI", 18, FontStyle.Bold),
                ForeColor = Color.White,
                Location = new Point(25, 25),
                Size = new Size(200, 40)
            };
            sidebar.Controls.Add(lblTitle);

            var separator = new Panel
            {
                Location = new Point(15, 75),
                Size = new Size(230, 1),
                BackColor = Color.FromArgb(68, 86, 104)
            };
            sidebar.Controls.Add(separator);

            btnHome = CreateMenuButton("Главная", 95);
            btnMonitor = CreateMenuButton("Монитор ТА", 155);
            btnAdmin = CreateMenuButton("Администрирование ТА", 215);

            sidebar.Controls.Add(btnHome);
            sidebar.Controls.Add(btnMonitor);
            sidebar.Controls.Add(btnAdmin);

            var userPanel = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 80,
                BackColor = Color.FromArgb(35, 50, 65)
            };

            pbUserAvatar = new PictureBox
            {
                Size = new Size(45, 45),
                Location = new Point(15, 18),
                BackColor = Color.FromArgb(52, 152, 219),
                SizeMode = PictureBoxSizeMode.Zoom
            };

            lblUserInfo = new Label
            {
                Text = "Загрузка...",
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                ForeColor = Color.White,
                Location = new Point(70, 20),
                Size = new Size(170, 45),
                Cursor = Cursors.Hand
            };
            lblUserInfo.Click += LblUserInfo_Click;

            userPanel.Controls.Add(pbUserAvatar);
            userPanel.Controls.Add(lblUserInfo);
            sidebar.Controls.Add(userPanel);

            contentPanel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(240, 242, 245),
                Padding = new Padding(20),
                AutoScroll = true
            };

            this.Controls.Add(contentPanel);
            this.Controls.Add(sidebar);
            userDropdownPanel = new Panel
            {
                BackColor = Color.White,
                Visible = false,
                Size = new Size(180, 110),
                BorderStyle = BorderStyle.FixedSingle
            };

            var btnProfile = new Button
            {
                Text = "Мой профиль",
                Dock = DockStyle.Top,
                Height = 36,
                FlatStyle = FlatStyle.Flat,
                TextAlign = ContentAlignment.MiddleLeft,
                Padding = new Padding(10, 0, 0, 0),
                BackColor = Color.White
            };
            btnProfile.FlatAppearance.BorderSize = 0;
            btnProfile.Click += (s, e) => { userDropdownPanel.Visible = false; ShowProfile(); };

            var btnSessions = new Button
            {
                Text = "Мои сессии",
                Dock = DockStyle.Top,
                Height = 36,
                FlatStyle = FlatStyle.Flat,
                TextAlign = ContentAlignment.MiddleLeft,
                Padding = new Padding(10, 0, 0, 0),
                BackColor = Color.White
            };
            btnSessions.FlatAppearance.BorderSize = 0;
            btnSessions.Click += (s, e) => { userDropdownPanel.Visible = false; ShowSessions(); };

            var btnLogout = new Button
            {
                Text = "Выход",
                Dock = DockStyle.Top,
                Height = 36,
                FlatStyle = FlatStyle.Flat,
                TextAlign = ContentAlignment.MiddleLeft,
                Padding = new Padding(10, 0, 0, 0),
                BackColor = Color.White
            };
            btnLogout.FlatAppearance.BorderSize = 0;
            btnLogout.Click += (s, e) => { Application.Exit(); };

            var separatorLine = new Panel
            {
                Dock = DockStyle.Top,
                Height = 1,
                BackColor = Color.FromArgb(220, 220, 220)
            };

            userDropdownPanel.Controls.Add(btnLogout);
            userDropdownPanel.Controls.Add(separatorLine);
            userDropdownPanel.Controls.Add(btnSessions);
            userDropdownPanel.Controls.Add(btnProfile);

            this.Controls.Add(userDropdownPanel);
            userDropdownPanel.BringToFront();

            dropdownTimer = new System.Windows.Forms.Timer { Interval = 3000 };
            dropdownTimer.Tick += (s, e) => { userDropdownPanel.Visible = false; dropdownTimer.Stop(); };

            this.Click += (s, e) => userDropdownPanel.Visible = false;
            contentPanel.Click += (s, e) => userDropdownPanel.Visible = false;

            btnHome.Click += (s, e) => ShowHomePage();
            btnMonitor.Click += (s, e) => ShowMonitorPage();
            btnAdmin.Click += (s, e) => ShowAdminPage();
        }

        private Button CreateMenuButton(string text, int y)
        {
            var btn = new Button
            {
                Text = text,
                Location = new Point(0, y),
                Size = new Size(260, 50),
                FlatStyle = FlatStyle.Flat,
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 11, FontStyle.Regular),
                TextAlign = ContentAlignment.MiddleLeft,
                BackColor = Color.Transparent,
                Padding = new Padding(25, 0, 0, 0)
            };
            btn.FlatAppearance.BorderSize = 0;
            btn.FlatAppearance.MouseOverBackColor = Color.FromArgb(68, 86, 104);
            btn.FlatAppearance.MouseDownBackColor = Color.FromArgb(35, 50, 65);
            return btn;
        }

        private void LblUserInfo_Click(object? sender, EventArgs e)
        {
            var labelPos = lblUserInfo.PointToScreen(Point.Empty);
            userDropdownPanel.Location = new Point(labelPos.X + 120, labelPos.Y + 45);
            userDropdownPanel.Visible = true;
            userDropdownPanel.BringToFront();

            dropdownTimer.Stop();
            dropdownTimer.Start();
        }

        private void LoadUserData()
        {
            if (currentUser != null)
            {
                lblUserInfo.Text = currentUser.ShortName + "\n" + (currentUser.role_name ?? "Администратор");

                var bmp = new Bitmap(45, 45);
                using (var g = Graphics.FromImage(bmp))
                {
                    g.Clear(Color.FromArgb(52, 152, 219));
                    using (var font = new Font("Segoe UI", 14, FontStyle.Bold))
                    {
                        var initials = currentUser.ShortName.Length > 1 ? currentUser.ShortName.Substring(0, 2) : "U";
                        var sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
                        g.DrawString(initials, font, Brushes.White, new Rectangle(0, 0, 45, 45), sf);
                    }
                }
                pbUserAvatar.Image = bmp;
            }
        }

        private void ShowProfile()
        {
            if (currentUser != null)
            {
                MessageBox.Show(
                    "ПРОФИЛЬ ПОЛЬЗОВАТЕЛЯ\n\n" +
                    "ФИО: " + currentUser.full_name + "\n" +
                    "Email: " + currentUser.email + "\n" +
                    "Телефон: " + (currentUser.phone ?? "не указан") + "\n" +
                    "Роль: " + (currentUser.role_name ?? "Администратор"),
                    "Мой профиль", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void ShowSessions()
        {
            MessageBox.Show(
                "АКТИВНЫЕ СЕССИИ\n\n" +
                "Текущая сессия\n" +
                "  Вход: " + DateTime.Now.ToString("dd.MM.yyyy HH:mm") + "\n" +
                "  IP: 127.0.0.1\n" +
                "  Устройство: Windows PC",
                "Мои сессии", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private async void ShowHomePage()
        {
            contentPanel.Controls.Clear();
            contentPanel.SuspendLayout();

            try
            {
                var loadingLabel = new Label
                {
                    Text = "Загрузка данных из базы...",
                    Font = new Font("Segoe UI", 14),
                    ForeColor = Color.Gray,
                    TextAlign = ContentAlignment.MiddleCenter,
                    Dock = DockStyle.Fill
                };
                contentPanel.Controls.Add(loadingLabel);
                contentPanel.ResumeLayout();
                await Task.Delay(100);

                var machines = await _apiService.GetVendingMachinesAsync();

                if (machines == null || machines.Count == 0)
                {
                    contentPanel.Controls.Clear();
                    var emptyLabel = new Label
                    {
                        Text = "В базе данных нет торговых автоматов.\nДобавьте их через раздел 'Администрирование ТА'",
                        Font = new Font("Segoe UI", 14),
                        ForeColor = Color.Gray,
                        TextAlign = ContentAlignment.MiddleCenter,
                        Dock = DockStyle.Fill
                    };
                    contentPanel.Controls.Add(emptyLabel);
                    return;
                }

                DataGenerator.EmulateDynamicData(machines);
                contentPanel.Controls.Clear();

                int workingCount = machines.Count(m => m.status_name == "Работает");
                int brokenCount = machines.Count(m => m.status_name == "Вышел из строя");
                int maintenanceCount = machines.Count(m => m.status_name == "В ремонте/на обслуживании");
                int efficiency = machines.Count > 0 ? (workingCount * 100 / machines.Count) : 0;
                var totalIncome = machines.Sum(m => m.total_income);
                var totalCash = machines.Sum(m => m.current_cash);

                var lblCompany = new Label
                {
                    Text = "ООО Торговые Автоматы",
                    Font = new Font("Segoe UI", 20, FontStyle.Bold),
                    ForeColor = Color.FromArgb(44, 62, 80),
                    Location = new Point(0, 0),
                    Size = new Size(900, 40)
                };
                contentPanel.Controls.Add(lblCompany);

                var lblSubtitle = new Label
                {
                    Text = "Личный кабинет. Главная",
                    Font = new Font("Segoe UI", 12),
                    ForeColor = Color.FromArgb(108, 117, 125),
                    Location = new Point(0, 45),
                    Size = new Size(900, 25)
                };
                contentPanel.Controls.Add(lblSubtitle);

                var pnlEfficiency = CreateCard("Эффективность сети", 0, 85, 280, 150);
                var lblEfficiencyValue = new Label
                {
                    Text = $"{efficiency}%",
                    Font = new Font("Segoe UI", 32, FontStyle.Bold),
                    Location = new Point(20, 55),
                    Size = new Size(240, 70),
                    TextAlign = ContentAlignment.MiddleCenter,
                    ForeColor = efficiency > 70 ? Color.FromArgb(46, 204, 113) : Color.FromArgb(241, 196, 15)
                };
                var lblEfficiencyDesc = new Label
                {
                    Text = "работающих автоматов",
                    Font = new Font("Segoe UI", 9),
                    ForeColor = Color.Gray,
                    Location = new Point(20, 115),
                    Size = new Size(240, 25),
                    TextAlign = ContentAlignment.MiddleCenter
                };
                pnlEfficiency.Controls.Add(lblEfficiencyValue);
                pnlEfficiency.Controls.Add(lblEfficiencyDesc);
                contentPanel.Controls.Add(pnlEfficiency);

                var pnlState = CreateCard("Состояние сети", 300, 85, 320, 150);
                pnlState.Controls.Add(new Label { Text = $"Работает: {workingCount}", Location = new Point(15, 55), Size = new Size(290, 30), Font = new Font("Segoe UI", 11), ForeColor = Color.FromArgb(46, 204, 113) });
                pnlState.Controls.Add(new Label { Text = $"Не работает: {brokenCount}", Location = new Point(15, 90), Size = new Size(290, 30), Font = new Font("Segoe UI", 11), ForeColor = Color.FromArgb(231, 76, 60) });
                pnlState.Controls.Add(new Label { Text = $"На обслуживании: {maintenanceCount}", Location = new Point(15, 125), Size = new Size(290, 30), Font = new Font("Segoe UI", 11), ForeColor = Color.FromArgb(52, 152, 219) });
                contentPanel.Controls.Add(pnlState);

                var pnlSummary = CreateCard("Сводка", 640, 85, 300, 150);
                pnlSummary.Controls.Add(new Label { Text = $"Доход: {totalIncome:N0} руб.", Location = new Point(15, 55), Size = new Size(270, 30), Font = new Font("Segoe UI", 11) });
                pnlSummary.Controls.Add(new Label { Text = $"В автоматах: {totalCash:N0} руб.", Location = new Point(15, 90), Size = new Size(270, 30), Font = new Font("Segoe UI", 11) });
                pnlSummary.Controls.Add(new Label { Text = $"Всего ТА: {machines.Count}", Location = new Point(15, 125), Size = new Size(270, 30), Font = new Font("Segoe UI", 11, FontStyle.Bold) });
                contentPanel.Controls.Add(pnlSummary);

                var pnlDynamics = CreateCard("Динамика продаж за последние 10 дней", 0, 255, 620, 300);

                var chartFilterPanel = new Panel
                {
                    Location = new Point(15, 50),
                    Size = new Size(590, 35)
                };

                var rbAmount = new RadioButton
                {
                    Text = "По сумме продаж",
                    Location = new Point(10, 8),
                    Size = new Size(130, 25),
                    Checked = true,
                    Font = new Font("Segoe UI", 9)
                };

                var rbCount = new RadioButton
                {
                    Text = "По количеству продаж",
                    Location = new Point(150, 8),
                    Size = new Size(140, 25),
                    Font = new Font("Segoe UI", 9)
                };

                chartFilterPanel.Controls.Add(rbAmount);
                chartFilterPanel.Controls.Add(rbCount);

                var salesChart = new Chart
                {
                    Location = new Point(15, 90),
                    Size = new Size(590, 180),
                    BackColor = Color.White
                };
                var salesChartArea = new ChartArea { Name = "SalesArea" };
                salesChart.ChartAreas.Add(salesChartArea);
                var salesSeries = new Series
                {
                    Name = "Sales",
                    ChartType = SeriesChartType.Column,
                    Color = Color.FromArgb(52, 152, 219)
                };
                salesChart.Series.Add(salesSeries);

                var random = new Random();
                var dates = new[] { "01.03", "02.03", "03.03", "04.03", "05.03", "06.03", "07.03", "08.03", "09.03", "10.03" };

                void UpdateChart(bool byAmount)
                {
                    salesSeries.Points.Clear();
                    for (int i = 0; i < 10; i++)
                    {
                        double value = byAmount ? random.Next(5000, 50000) : random.Next(20, 300);
                        var point = salesSeries.Points.Add(value);
                        point.AxisLabel = dates[i];
                    }
                    salesChart.ChartAreas["SalesArea"].AxisY.Title = byAmount ? "Сумма (руб.)" : "Количество (шт)";
                }

                rbAmount.CheckedChanged += (s, e) => { if (rbAmount.Checked) UpdateChart(true); };
                rbCount.CheckedChanged += (s, e) => { if (rbCount.Checked) UpdateChart(false); };
                UpdateChart(true);

                pnlDynamics.Controls.Add(chartFilterPanel);
                pnlDynamics.Controls.Add(salesChart);
                contentPanel.Controls.Add(pnlDynamics);

                var pnlNews = CreateCard("Новости", 640, 255, 300, 300);
                var newsList = new ListBox
                {
                    Location = new Point(10, 50),
                    Size = new Size(280, 225),
                    Font = new Font("Segoe UI", 9),
                    BorderStyle = BorderStyle.None,
                    BackColor = Color.White
                };
                newsList.Items.Add("28.01.25 - Установка КПУ на полную заводскую отрасль");
                newsList.Items.Add("31.12.24 - Подготовка подразделения к КТV Testing");
                newsList.Items.Add("28.12.24 - Ставка НДС 5% в 2% для КПВ");
                newsList.Items.Add("04.12.24 - Резервный срок СВМ системы КТV Stop");
                newsList.Items.Add("27.11.24 - Наши модели основных автомобилей от КТV Testing");
                newsList.Items.Add("20.11.24 - Получение сертификата РСО 053.4.03");
                pnlNews.Controls.Add(newsList);
                contentPanel.Controls.Add(pnlNews);
            }
            catch (Exception ex)
            {
                contentPanel.Controls.Clear();
                var errorLabel = new Label
                {
                    Text = $"Ошибка загрузки данных: {ex.Message}",
                    Font = new Font("Segoe UI", 12),
                    ForeColor = Color.Red,
                    TextAlign = ContentAlignment.MiddleCenter,
                    Dock = DockStyle.Fill
                };
                contentPanel.Controls.Add(errorLabel);
            }
            finally
            {
                contentPanel.ResumeLayout();
            }
        }

        private Panel CreateCard(string title, int x, int y, int width, int height)
        {
            var panel = new Panel
            {
                Location = new Point(x, y),
                Size = new Size(width, height),
                BackColor = Color.White,
                BorderStyle = BorderStyle.None
            };

            panel.Paint += (s, e) =>
            {
                ControlPaint.DrawBorder(e.Graphics, panel.ClientRectangle,
                    Color.FromArgb(220, 220, 220), 1, ButtonBorderStyle.Solid,
                    Color.FromArgb(220, 220, 220), 1, ButtonBorderStyle.Solid,
                    Color.FromArgb(220, 220, 220), 1, ButtonBorderStyle.Solid,
                    Color.FromArgb(220, 220, 220), 1, ButtonBorderStyle.Solid);
            };

            var lblTitle = new Label
            {
                Text = title,
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                Location = new Point(15, 12),
                Size = new Size(width - 30, 30),
                ForeColor = Color.FromArgb(44, 62, 80)
            };
            panel.Controls.Add(lblTitle);

            var line = new Panel
            {
                Location = new Point(15, 42),
                Size = new Size(width - 30, 1),
                BackColor = Color.FromArgb(230, 232, 235)
            };
            panel.Controls.Add(line);

            return panel;
        }

        private void ShowMonitorPage()
        {
            var monitorForm = new MachineMonitorForm();
            monitorForm.ShowDialog();
        }

        private void ShowAdminPage()
        {
            var adminForm = new AdminMachinesForm();
            adminForm.ShowDialog();
        }
    }
}