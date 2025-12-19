using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;

namespace Gomoku_Client.View
{
    public class NotificationManager
    {
        private static NotificationManager? _instance;
        public static NotificationManager Instance => _instance ??= new NotificationManager();

        public ObservableCollection<NotificationItem> Notifications { get; } = new ObservableCollection<NotificationItem>();

        private NotificationManager() { }

        public void ShowNotification(string title, string message, Notification.NotificationType type = Notification.NotificationType.Info, int autoCloseDuration = 3000)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                var notification = new NotificationItem
                {
                    Title = title,
                    Message = message,
                    Type = type,
                    AutoCloseDuration = autoCloseDuration
                };

                Notifications.Insert(0, notification);

                if (autoCloseDuration > 0)
                {
                    var timer = new System.Windows.Threading.DispatcherTimer
                    {
                        Interval = TimeSpan.FromMilliseconds(autoCloseDuration)
                    };
                    timer.Tick += (s, e) =>
                    {
                        timer.Stop();
                        RemoveNotification(notification);
                    };
                    timer.Start();
                }
            });
        }

        public void RemoveNotification(NotificationItem notification)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                if (Notifications.Contains(notification))
                {
                    Notifications.Remove(notification);
                }
            });
        }
    }

    public class NotificationItem
    {
        public string Title { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public Notification.NotificationType Type { get; set; }
        public int AutoCloseDuration { get; set; }
        public event EventHandler? AcceptClicked;
        public event EventHandler? DeclineClicked;

        public void OnAcceptClicked()
        {
            AcceptClicked?.Invoke(this, EventArgs.Empty);
        }

        public void OnDeclineClicked()
        {
            DeclineClicked?.Invoke(this, EventArgs.Empty);
        }
    }
}