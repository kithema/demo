using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using FranchisorApp.Forms;
using FranchisorApp.Models;
using System.IO;

namespace FranchisorApp.Services
{
    public class NotificationService
    {
        private static NotificationService? _instance;
        private static readonly object _lock = new object();
        private Form? _parentForm;
        private Queue<Notification> _notificationQueue;
        private bool _isShowing;
        private string _logFilePath;

        public static NotificationService Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (_lock)
                    {
                        _instance ??= new NotificationService();
                    }
                }
                return _instance;
            }
        }

        private NotificationService()
        {
            _notificationQueue = new Queue<Notification>();
            _isShowing = false;
            _logFilePath = Path.Combine(Application.StartupPath, "notifications.log");
        }

        public void Initialize(Form parentForm)
        {
            _parentForm = parentForm;
        }

        public void ShowNotification(Notification notification)
        {
            LogNotification(notification);
            AddWithPriority(notification);
            ProcessQueue();
        }

        private void AddWithPriority(Notification notification)
        {
            var tempList = _notificationQueue.ToList();
            tempList.Add(notification);

            var sorted = tempList.OrderByDescending(n =>
                n.Type == NotificationType.Critical ? 4 :
                n.Type == NotificationType.Warning ? 3 :
                n.Type == NotificationType.Info ? 2 : 1).ToList();

            _notificationQueue.Clear();
            foreach (var item in sorted)
            {
                _notificationQueue.Enqueue(item);
            }
        }

        private void LogNotification(Notification notification)
        {
            try
            {
                var logEntry = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] [{notification.Type}] {notification.Message}";
                if (notification.MachineId.HasValue)
                    logEntry += $" (Аппарат ID: {notification.MachineId})";

                File.AppendAllText(_logFilePath, logEntry + Environment.NewLine);
            }
            catch { }
        }

        private async void ProcessQueue()
        {
            if (_isShowing || _notificationQueue.Count == 0) return;

            _isShowing = true;

            while (_notificationQueue.Count > 0)
            {
                var notification = _notificationQueue.Dequeue();

                if (_parentForm != null && !_parentForm.IsDisposed)
                {
                    _parentForm.BeginInvoke(new Action(() =>
                    {
                        var toast = new ToastNotificationForm(notification.Message, notification.Type);
                        toast.Show();
                    }));
                }

                int delay = notification.Type == NotificationType.Critical ? 10000 :
                           notification.Type == NotificationType.Warning ? 7000 : 5000;
                await Task.Delay(delay + 500);
            }

            _isShowing = false;
        }
    }
}