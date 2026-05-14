using System;
using System.Drawing;
using System.Windows.Forms;
using FranchisorApp.Services;
using FranchisorApp.Models;

namespace FranchisorApp.Forms
{
    public partial class LoginForm : Form
    {
        private TextBox txtUsername;
        private TextBox txtPassword;
        private Button btnLogin;
        private Label lblError;

        public LoginForm()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.Text = "Авторизация - VendoMatic";
            this.Size = new Size(450, 520);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.BackColor = Color.White;
            this.Padding = new Padding(0);

            var mainPanel = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(40, 30, 40, 30),
                BackColor = Color.White
            };

            btnLogin = new Button
            {
                Text = "Войти в систему",
                Size = new Size(370, 50),
                Location = new Point(0, 0),
                BackColor = Color.FromArgb(52, 152, 219),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                Cursor = Cursors.Hand,
                FlatAppearance = { BorderSize = 0 }
            };
            btnLogin.Click += BtnLogin_Click;

            var buttonPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 70,
                Padding = new Padding(0, 10, 0, 10)
            };
            buttonPanel.Controls.Add(btnLogin);
            mainPanel.Controls.Add(buttonPanel);

            var inputPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 200,
                Padding = new Padding(0, 10, 0, 10)
            };

            var lblUsername = new Label
            {
                Text = "Электронная почта",
                Location = new Point(0, 10),
                Size = new Size(370, 25),
                Font = new Font("Segoe UI", 10),
                ForeColor = Color.FromArgb(80, 85, 90)
            };

            txtUsername = new TextBox
            {
                Location = new Point(0, 38),
                Size = new Size(370, 35),
                Font = new Font("Segoe UI", 12),
                BorderStyle = BorderStyle.FixedSingle,
                Text = "admin@example.com"
            };

            var lblPassword = new Label
            {
                Text = "Пароль",
                Location = new Point(0, 90),
                Size = new Size(370, 25),
                Font = new Font("Segoe UI", 10),
                ForeColor = Color.FromArgb(80, 85, 90)
            };

            txtPassword = new TextBox
            {
                Location = new Point(0, 118),
                Size = new Size(370, 35),
                UseSystemPasswordChar = true,
                Font = new Font("Segoe UI", 12),
                BorderStyle = BorderStyle.FixedSingle,
                Text = "123456"
            };

            inputPanel.Controls.Add(lblUsername);
            inputPanel.Controls.Add(txtUsername);
            inputPanel.Controls.Add(lblPassword);
            inputPanel.Controls.Add(txtPassword);
            mainPanel.Controls.Add(inputPanel);

            lblError = new Label
            {
                Text = "",
                Dock = DockStyle.Top,
                Height = 40,
                ForeColor = Color.FromArgb(231, 76, 60),
                TextAlign = ContentAlignment.MiddleCenter,
                Font = new Font("Segoe UI", 9),
                Visible = true
            };
            mainPanel.Controls.Add(lblError);

            var logoPanel = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(0, 20, 0, 0)
            };

            var lblLogo = new Label
            {
                Text = "VendoMatic",
                Font = new Font("Segoe UI", 28, FontStyle.Bold),
                ForeColor = Color.FromArgb(44, 62, 80),
                TextAlign = ContentAlignment.MiddleCenter,
                Dock = DockStyle.Top,
                Height = 60
            };

            var lblSubLogo = new Label
            {
                Text = "Управление торговыми автоматами",
                Font = new Font("Segoe UI", 10),
                ForeColor = Color.FromArgb(108, 117, 125),
                TextAlign = ContentAlignment.MiddleCenter,
                Dock = DockStyle.Top,
                Height = 25
            };

            logoPanel.Controls.Add(lblSubLogo);
            logoPanel.Controls.Add(lblLogo);
            mainPanel.Controls.Add(logoPanel);

            this.Controls.Add(mainPanel);
        }

        private void BtnLogin_Click(object? sender, EventArgs e)
        {
            if (txtUsername.Text == "admin@example.com" && txtPassword.Text == "123456")
            {
                var user = new User
                {
                    user_id = 1,
                    full_name = "Автоматов А.А.",
                    email = "admin@example.com",
                    phone = "+7 (999) 123-45-67",
                    role_id = 1,
                    role_name = "Администратор"
                };

                this.Hide();
                var mainForm = new MainForm(user);
                mainForm.Show();
                mainForm.FormClosed += (s, args) => this.Close();
            }
            else
            {
                lblError.Text = "Неверный email или пароль";
                txtPassword.Clear();
                txtPassword.Focus();
            }
        }
    }
}