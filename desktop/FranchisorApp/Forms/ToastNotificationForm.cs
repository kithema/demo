using System;
using System.Drawing;
using System.Windows.Forms;
using FranchisorApp.Models;

namespace FranchisorApp.Forms
{
    public partial class ToastNotificationForm : Form
    {
        private System.Windows.Forms.Timer autoCloseTimer;

        public ToastNotificationForm(string message, NotificationType type, int durationMs = 5000)
        {
            InitializeComponent();
            SetupForm(message, type);

            int actualDuration = type switch
            {
                NotificationType.Critical => 10000,
                NotificationType.Warning => 7000,
                _ => 5000
            };

            autoCloseTimer = new System.Windows.Forms.Timer { Interval = actualDuration };
            autoCloseTimer.Tick += (s, e) => { autoCloseTimer.Stop(); this.Close(); };
            autoCloseTimer.Start();
        }

        private void InitializeComponent()
        {
            this.FormBorderStyle = FormBorderStyle.None;
            this.StartPosition = FormStartPosition.Manual;
            this.ShowInTaskbar = false;
            this.TopMost = true;
            this.Size = new Size(400, 90);
            this.BackColor = Color.White;
            this.Opacity = 0.97;
        }

        private void SetupForm(string message, NotificationType type)
        {
            Color backColor = type switch
            {
                NotificationType.Critical => Color.FromArgb(231, 76, 60),
                NotificationType.Warning => Color.FromArgb(241, 196, 15),
                NotificationType.Info => Color.FromArgb(52, 152, 219),
                NotificationType.Success => Color.FromArgb(46, 204, 113),
                _ => Color.FromArgb(46, 204, 113)
            };
            this.BackColor = backColor;

            string icon = type switch
            {
                NotificationType.Critical => "❌",
                NotificationType.Warning => "⚠️",
                NotificationType.Info => "ℹ️",
                NotificationType.Success => "✓",
                _ => "✓"
            };

            string title = type switch
            {
                NotificationType.Critical => "КРИТИЧЕСКАЯ ОШИБКА",
                NotificationType.Warning => "ПРЕДУПРЕЖДЕНИЕ",
                NotificationType.Info => "ИНФОРМАЦИЯ",
                NotificationType.Success => "УСПЕХ",
                _ => "УВЕДОМЛЕНИЕ"
            };

            var iconLabel = new Label
            {
                Text = icon,
                Font = new Font("Segoe UI", 24),
                Location = new Point(12, 28),
                Size = new Size(45, 45),
                ForeColor = Color.White,
                BackColor = Color.Transparent,
                TextAlign = ContentAlignment.MiddleCenter
            };

            var titleLabel = new Label
            {
                Text = title,
                Font = new Font("Segoe UI", 8, FontStyle.Bold),
                Location = new Point(65, 12),
                Size = new Size(290, 18),
                ForeColor = Color.FromArgb(255, 255, 255, 220),
                BackColor = Color.Transparent
            };

            var messageLabel = new Label
            {
                Text = message,
                Font = new Font("Segoe UI", 10, FontStyle.Regular),
                Location = new Point(65, 32),
                Size = new Size(290, 40),
                ForeColor = Color.White,
                BackColor = Color.Transparent
            };

            var closeButton = new Button
            {
                Text = "×",
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.Transparent,
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 14, FontStyle.Bold),
                Size = new Size(30, 30),
                Location = new Point(360, 5),
                Cursor = Cursors.Hand
            };
            closeButton.FlatAppearance.BorderSize = 0;
            closeButton.Click += (s, e) => this.Close();

            this.Controls.Add(iconLabel);
            this.Controls.Add(titleLabel);
            this.Controls.Add(messageLabel);
            this.Controls.Add(closeButton);

            if (type == NotificationType.Critical)
            {
                var okButton = new Button
                {
                    Text = "Понятно",
                    FlatStyle = FlatStyle.Flat,
                    BackColor = Color.White,
                    ForeColor = backColor,
                    Font = new Font("Segoe UI", 9, FontStyle.Bold),
                    Size = new Size(80, 28),
                    Location = new Point(300, 55),
                    Cursor = Cursors.Hand
                };
                okButton.FlatAppearance.BorderSize = 0;
                okButton.Click += (s, e) => this.Close();
                this.Controls.Add(okButton);
                closeButton.Location = new Point(360, 5);
                messageLabel.Size = new Size(225, 35);
                this.Size = new Size(400, 100);
            }

            this.Opacity = 0;
            var fadeTimer = new System.Windows.Forms.Timer { Interval = 30 };
            fadeTimer.Tick += (s, e) =>
            {
                if (this.Opacity < 0.98) this.Opacity += 0.1;
                else fadeTimer.Stop();
            };
            fadeTimer.Start();

            var screen = Screen.PrimaryScreen.WorkingArea;
            this.Location = new Point(screen.Right - this.Width - 10, screen.Bottom - this.Height - 10);
        }
    }
}