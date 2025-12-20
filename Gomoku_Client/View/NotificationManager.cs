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

        public void ShowNotification(
            string title,
            string message,
            Notification.NotificationType type = Notification.NotificationType.Info,
            int autoCloseDuration = 3000,
            EventHandler? onAccept = null,
            EventHandler? onDecline = null)
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
                if (onAccept != null)
                {
                    notification.AcceptClicked += onAccept;
                }
                if (onDecline != null)
                {
                    notification.DeclineClicked += onDecline;
                }
                Notifications.Insert(0, notification);

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

/*   HOW TO USE
    there are 2 types of notification: 
    first is info, which will only show title and message
    second is YesNo, which will add the actions row, allowing user to accept or decline

this is a preview:
1. info
NotificationManager.Instance.ShowNotification(
    "THÔNG BÁO",
    "bla bla bla"
    // default type is info, auto close after 3s
);

2. info without auto close
NotificationManager.Instance.ShowNotification(
    "THÔNG BÁO",
    "no auto close this time",
    Notification.NotificationType.Info,
    0 // 0 disable auto close
);

3. YesNo with callback
NotificationManager.Instance.ShowNotification(
    "THÔNG BÁO",
    "calback test",
    Notification.NotificationType.YesNo,
    0,
    onAccept: (s, e) =>
    {
        //do something when accepted
    },
    onDecline: (s, e) =>
    {
        //do something when declined
    }
);

4. YesNo with function ref
// Using method references for cleaner code
NotificationManager.Instance.ShowNotification(
    "THÔNG BÁO",
    "test idk",
    Notification.NotificationType.YesNo,
    0,
    onAccept: DoSomething,
    onDecline: DontDoSomething
);

private void DoSomething(object? sender, EventArgs e)
{
    // accepted
}

private void DontDoSomething(object? sender, EventArgs e)
{
    // declined
}
*/