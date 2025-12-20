using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;
using System.Windows.Threading;

namespace Gomoku_Client.View
{
    public partial class Notification : UserControl
    {
        private DispatcherTimer? _autoCloseTimer;
        public event EventHandler? AcceptClicked;
        public event EventHandler? DeclineClicked;

        public enum NotificationType
        {
            Info,
            YesNo
        }
        //databinding
        public static readonly DependencyProperty TitleProperty =
            DependencyProperty.Register("Title", typeof(string), typeof(Notification),
                new PropertyMetadata(string.Empty, OnPropertyChanged));

        public static readonly DependencyProperty MessageProperty =
            DependencyProperty.Register("Message", typeof(string), typeof(Notification),
                new PropertyMetadata(string.Empty, OnPropertyChanged));

        public static readonly DependencyProperty TypeProperty =
            DependencyProperty.Register("Type", typeof(NotificationType), typeof(Notification),
                new PropertyMetadata(NotificationType.Info, OnPropertyChanged));

        public static readonly DependencyProperty AutoCloseDurationProperty =
            DependencyProperty.Register("AutoCloseDuration", typeof(int), typeof(Notification),
                new PropertyMetadata(0, OnPropertyChanged));

        public string Title
        {
            get => (string)GetValue(TitleProperty);
            set => SetValue(TitleProperty, value);
        }

        public string Message
        {
            get => (string)GetValue(MessageProperty);
            set => SetValue(MessageProperty, value);
        }

        public NotificationType Type
        {
            get => (NotificationType)GetValue(TypeProperty);
            set => SetValue(TypeProperty, value);
        }

        public int AutoCloseDuration
        {
            get => (int)GetValue(AutoCloseDurationProperty);
            set => SetValue(AutoCloseDurationProperty, value);
        }

        private static void OnPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is Notification notification)
            {
                notification.UpdateUI();
            }
        }

        public Notification()
        {
            InitializeComponent();
            Loaded += Notification_Loaded;
        }

        private void Notification_Loaded(object sender, RoutedEventArgs e)
        {
            UpdateUI();
            var slideIn = (Storyboard)this.Resources["SlideIn"];
            slideIn.Begin();

            if (AutoCloseDuration > 0)
            {
                _autoCloseTimer = new DispatcherTimer
                {
                    Interval = TimeSpan.FromMilliseconds(AutoCloseDuration)
                };
                _autoCloseTimer.Tick += AutoCloseTimer_Tick;
                _autoCloseTimer.Start();
            }
        }

        private void UpdateUI()
        {
            TitleText.Text = Title;
            MessageText.Text = Message;

            if (Type == NotificationType.YesNo)
            {
                ActionButtonsContainer.Visibility = Visibility.Visible;
                AcceptButton.Visibility = Visibility.Visible;
                DeclineButton.Visibility = Visibility.Visible;
            }
            else
            {
                ActionButtonsContainer.Visibility = Visibility.Collapsed;
            }
        }

        public void Show(string title, string message, NotificationType type = NotificationType.Info, int autoCloseDuration = 0)
        {
            Title = title;
            Message = message;
            Type = type;
            AutoCloseDuration = autoCloseDuration;

            UpdateUI();

            var slideIn = (Storyboard)this.Resources["SlideIn"];
            slideIn.Begin();

            if (autoCloseDuration > 0)
            {
                _autoCloseTimer = new DispatcherTimer
                {
                    Interval = TimeSpan.FromMilliseconds(autoCloseDuration)
                };
                _autoCloseTimer.Tick += AutoCloseTimer_Tick;
                _autoCloseTimer.Start();
            }
        }

        private void AutoCloseTimer_Tick(object? sender, EventArgs e)
        {
            _autoCloseTimer?.Stop();
            Hide();
        }

        public void Hide()
        {
            var slideOut = (Storyboard)this.Resources["SlideOut"];
            slideOut.Completed += (s, e) =>
            {
                Dispatcher.BeginInvoke(new Action(() =>
                {
                    var item = this.DataContext as NotificationItem;
                    if (item != null)
                    {
                        NotificationManager.Instance.RemoveNotification(item);
                    }
                }), System.Windows.Threading.DispatcherPriority.ContextIdle);
            };
            slideOut.Begin();
        }

        private void CloseBtn_Click(object sender, RoutedEventArgs e)
        {
            _autoCloseTimer?.Stop();
            Hide();
        }

        private void AcceptButton_Click(object sender, RoutedEventArgs e)
        {
            _autoCloseTimer?.Stop();
            AcceptClicked?.Invoke(this, EventArgs.Empty);

            var item = this.DataContext as NotificationItem;
            item?.OnAcceptClicked();

            Hide();
        }

        private void DeclineButton_Click(object sender, RoutedEventArgs e)
        {
            _autoCloseTimer?.Stop();
            DeclineClicked?.Invoke(this, EventArgs.Empty);

            var item = this.DataContext as NotificationItem;
            item?.OnDeclineClicked();

            Hide();
        }
    }
}

/*   how to use: 

1: change the following design so it look like this
<Page x:Class="Gomoku_Client.View.test123"
      xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
      xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
      xmlns:ctrls="clr-namespace:Gomoku_Client.View">  <------ add this pls

    <Grid>
        <!-- existing content -->
        
        <!-- here is notification -->
        <ItemsControl ItemsSource="{Binding Source={x:Static ctrls:NotificationManager.Instance}, Path=Notifications}"
              HorizontalAlignment="Right"
              VerticalAlignment="Bottom"
              Margin="0,0,20,20"
              Panel.ZIndex="9999">
            <ItemsControl.ItemsPanel>
                <ItemsPanelTemplate>
                    <StackPanel VerticalAlignment="Bottom"/>
                </ItemsPanelTemplate>
            </ItemsControl.ItemsPanel>
            <ItemsControl.ItemTemplate>
                <DataTemplate>
                    <ctrls:Notification x:Name="NotificationControl"
                              Title="{Binding Title}"
                              Message="{Binding Message}"
                              Type="{Binding Type}"
                              AutoCloseDuration="{Binding AutoCloseDuration}"
                              Margin="0,0,0,10"/>
                </DataTemplate>
            </ItemsControl.ItemTemplate>
        </ItemsControl>
    </Grid>
</Page>

- to show plain notification, do the following
    NotificationManager.Instance.ShowNotification(
        "title",
        "message",
        Notification.NotificationType.Info,
        3000 // auto close timer
    );

- to show notification with accept/decline buttons, do the following

var notification = new NotificationItem
{
    Title = "title",
    Message = "message skibidi",
    Type = Notification.NotificationType.YesNo,
    AutoCloseDuration = 5000
};

notification.AcceptClicked += (s, e) =>
{
    MessageBox.Show("tralalelo tralala");
};

notification.DeclineClicked += (s, e) =>
{
    MessageBox.Show("WE are charlie kirk!");
};
// DO NOT FORGET THIS
Application.Current.Dispatcher.Invoke(() =>
{
    NotificationManager.Instance.Notifications.Insert(0, notification);
});

con j ko hieu ib t hoi
*/
