using Google.Protobuf.WellKnownTypes;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Effects;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Gomoku_Client.View
{
    public static class UIUtils
    {
        public static Border CreateFriendRequestCard(
            string name,
            Action<object, RoutedEventArgs> AcceptButton_Click,
            Action<object, RoutedEventArgs> RefuseButton_Click,
            ResourceDictionary resource
        )
        {
            Border newFriendRequestCard = new Border();
            newFriendRequestCard.Name = name;
            newFriendRequestCard.Style = resource["FriendCardStyle"] as Style;

            Grid grid = new Grid();
            grid.ColumnDefinitions.Add(new ColumnDefinition() { Width = GridLength.Auto });
            grid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(1, GridUnitType.Star) });
            grid.ColumnDefinitions.Add(new ColumnDefinition() { Width = GridLength.Auto });

            Brush goldBrush = (Brush)new BrushConverter().ConvertFrom("#FFD700");

            Border avatar = new Border()
            {
                Width = 45,
                Height = 45,
                CornerRadius = new CornerRadius(22.5),
                BorderBrush = goldBrush,
                BorderThickness = new Thickness(2),
                Margin = new Thickness(0, 0, 12, 0)
            };

            ImageBrush avatarBrush = new ImageBrush()
            {
                ImageSource = new BitmapImage(new Uri("pack://application:,,,/Assets/Gomoku.ico")),
                Stretch = Stretch.UniformToFill
            };
            avatar.Background = avatarBrush;

            Grid.SetColumn(avatar, 0);
            grid.Children.Add(avatar);

            StackPanel namePanel = new StackPanel()
            {
                VerticalAlignment = VerticalAlignment.Center
            };

            TextBlock nameTextBlock = new TextBlock()
            {
                Text = name,
                FontSize = 14,
                FontWeight = FontWeights.SemiBold,
                Foreground = (Brush)new BrushConverter().ConvertFrom("#ECECEC")
            };

            namePanel.Children.Add(nameTextBlock);

            Grid.SetColumn(namePanel, 1);
            grid.Children.Add(namePanel);

            StackPanel actionButtonsPanel = new StackPanel()
            {
                Orientation = Orientation.Horizontal,
                VerticalAlignment = VerticalAlignment.Center
            };

            Button acceptButton = new Button()
            {
                Width = 35,
                Height = 35,
                Background = (Brush)new BrushConverter().ConvertFrom("#00D946"),
                BorderThickness = new Thickness(0),
                Cursor = Cursors.Hand,
                Margin = new Thickness(0, 0, 5, 0),
                ToolTip = "Chấp nhận",
                Name = name
            };
            acceptButton.Click += (sender, e) => AcceptButton_Click(sender, e);

            Path acceptIcon = new Path()
            {
                Data = Geometry.Parse("M5,12 L10,17 L20,7"),
                Stroke = Brushes.White,
                StrokeThickness = 2.5,
                Width = 16,
                Height = 16,
                Stretch = Stretch.Uniform
            };
            acceptButton.Content = acceptIcon;

            Button refuseButton = new Button()
            {
                Width = 35,
                Height = 35,
                Background = (Brush)new BrushConverter().ConvertFrom("#FF4655"),
                BorderThickness = new Thickness(0),
                Cursor = Cursors.Hand,
                ToolTip = "Từ chối",
                Name = name
            };
            refuseButton.Click += (sender, e) => RefuseButton_Click(sender, e);

            Path refuseIcon = new Path()
            {
                Data = Geometry.Parse("M6,6 L18,18 M18,6 L6,18"),
                Stroke = Brushes.White,
                StrokeThickness = 2.5,
                Width = 16,
                Height = 16,
                Stretch = Stretch.Uniform,
                Name = name
            };
            refuseButton.Content = refuseIcon;

            actionButtonsPanel.Children.Add(acceptButton);
            actionButtonsPanel.Children.Add(refuseButton);

            Grid.SetColumn(actionButtonsPanel, 2);
            grid.Children.Add(actionButtonsPanel);

            newFriendRequestCard.Child = grid;

            return newFriendRequestCard;
        }

        public static Border CreateFriendCard(
            string name,
            Action<object, RoutedEventArgs> ChallengeButton_Click,
            Action<object, RoutedEventArgs> UnfriendButton_Click,
            ResourceDictionary resource
        )
        {
            Border newFriendCard = new Border();
            newFriendCard.Name = name;
            newFriendCard.Style = resource["FriendCardStyle"] as Style;

            Grid grid = new Grid();
            grid.ColumnDefinitions.Add(new ColumnDefinition() { Width = GridLength.Auto });
            grid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(1, GridUnitType.Star) });
            grid.ColumnDefinitions.Add(new ColumnDefinition() { Width = GridLength.Auto });

            Brush goldBrush = (Brush)new BrushConverter().ConvertFrom("#FFD700");

            Border avatar = new Border()
            {
                Width = 45,
                Height = 45,
                CornerRadius = new CornerRadius(22.5),
                BorderBrush = goldBrush,
                BorderThickness = new Thickness(2),
                Margin = new Thickness(0, 0, 12, 0)
            };

            ImageBrush avatarBrush = new ImageBrush()
            {
                ImageSource = new BitmapImage(new Uri("pack://application:,,,/Assets/Gomoku.ico")),
                Stretch = Stretch.UniformToFill
            };
            avatar.Background = avatarBrush;

            Grid.SetColumn(avatar, 0);
            grid.Children.Add(avatar);

            StackPanel namePanel = new StackPanel() { VerticalAlignment = VerticalAlignment.Center };
            TextBlock nameTextBlock = new TextBlock()
            {
                Text = name,
                FontSize = 14,
                FontWeight = FontWeights.SemiBold,
                Foreground = (Brush)new BrushConverter().ConvertFrom("#ECECEC")
            };
            namePanel.Children.Add(nameTextBlock);

            Grid.SetColumn(namePanel, 1);
            grid.Children.Add(namePanel);

            StackPanel actionButtonsPanel = new StackPanel()
            {
                Orientation = Orientation.Horizontal,
                VerticalAlignment = VerticalAlignment.Center
            };

            ControlTemplate GetCircleTemplate(string iconPath)
            {
                ControlTemplate template = new ControlTemplate(typeof(Button));
                FrameworkElementFactory gridRoot = new FrameworkElementFactory(typeof(Grid));
                gridRoot.Name = "Root";

                FrameworkElementFactory ellipse = new FrameworkElementFactory(typeof(Ellipse));
                ellipse.SetBinding(Shape.FillProperty, new Binding("Background") { RelativeSource = RelativeSource.TemplatedParent });
                ellipse.SetBinding(Shape.StrokeProperty, new Binding("BorderBrush") { RelativeSource = RelativeSource.TemplatedParent });
                ellipse.SetBinding(Shape.StrokeThicknessProperty, new Binding("BorderThickness") { RelativeSource = RelativeSource.TemplatedParent });

                FrameworkElementFactory iconImage = new FrameworkElementFactory(typeof(Image));
                iconImage.SetValue(Image.SourceProperty, new BitmapImage(new Uri(iconPath, UriKind.RelativeOrAbsolute)));
                iconImage.SetValue(FrameworkElement.WidthProperty, 50.0);
                iconImage.SetValue(FrameworkElement.HeightProperty, 50.0);
                iconImage.SetValue(Image.StretchProperty, Stretch.Fill);

                gridRoot.AppendChild(ellipse);
                gridRoot.AppendChild(iconImage);
                template.VisualTree = gridRoot;

                Trigger hoverTrigger = new Trigger { Property = UIElement.IsMouseOverProperty, Value = true };
                hoverTrigger.Setters.Add(new Setter
                {
                    TargetName = "Root",
                    Property = UIElement.EffectProperty,
                    Value = new DropShadowEffect { BlurRadius = 20, ShadowDepth = 0, Opacity = 1, Color = Color.FromRgb(255, 70, 85) }
                });

                Trigger disabledTrigger = new Trigger { Property = UIElement.IsEnabledProperty, Value = false };
                disabledTrigger.Setters.Add(new Setter(UIElement.OpacityProperty, 0.5, "Root"));

                template.Triggers.Add(hoverTrigger);
                template.Triggers.Add(disabledTrigger);
                return template;
            }

            Button challengeBtn = new Button
            {
                Width = 50,
                Height = 50,
                Background = (Brush)new BrushConverter().ConvertFrom("#FF4655"),
                BorderBrush = (Brush)new BrushConverter().ConvertFrom("#FF4655"),
                BorderThickness = new Thickness(2),
                Cursor = Cursors.Hand,
                Margin = new Thickness(0, 0, 20, 0),
                ToolTip = "Thách đấu",
                Template = GetCircleTemplate("pack://application:,,,/Assets/Riot_Games_Logo.png")
            };
            challengeBtn.Click += (s, e) => ChallengeButton_Click(s, e);

            Button unfriendBtn = new Button
            {
                Width = 50,
                Height = 50,
                Background = (Brush)new BrushConverter().ConvertFrom("#FF4655"),
                BorderBrush = (Brush)new BrushConverter().ConvertFrom("#FF4655"),
                BorderThickness = new Thickness(2),
                Cursor = Cursors.Hand,
                ToolTip = "Xóa bạn",
                Template = GetCircleTemplate("pack://application:,,,/Assets/delete-friend.png"),
                Name = name
            };
            unfriendBtn.Click += (s, e) => UnfriendButton_Click(s, e);

            actionButtonsPanel.Children.Add(challengeBtn);
            actionButtonsPanel.Children.Add(unfriendBtn);

            Grid.SetColumn(actionButtonsPanel, 2);
            grid.Children.Add(actionButtonsPanel);

            newFriendCard.Child = grid;

            return newFriendCard;
        }

        public static Border CreateDrawMatchItem(string opponentName, string duration)
        {
            Border mainBorder = new Border
            {
                Padding = new Thickness(20, 15, 20, 15),
                Margin = new Thickness(0),
                BorderBrush = (SolidColorBrush)new BrushConverter().ConvertFrom("#FFD700"),
                BorderThickness = new Thickness(0, 0, 6, 0),
                Name = opponentName
            };

            LinearGradientBrush backgroundBrush = new LinearGradientBrush
            {
                StartPoint = new Point(0, 0.5),
                EndPoint = new Point(1, 0.5)
            };
            backgroundBrush.GradientStops.Add(new GradientStop((Color)ColorConverter.ConvertFromString("#FFD700"), 0));
            backgroundBrush.GradientStops.Add(new GradientStop((Color)ColorConverter.ConvertFromString("#0F0F1E"), 0.13));
            mainBorder.Background = backgroundBrush;

            Grid contentGrid = new Grid();
            contentGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(140) });
            contentGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            contentGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(180) });
            contentGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(150) });

            TextBlock txtResult = new TextBlock
            {
                Text = "Hòa",
                FontSize = 14,
                FontWeight = FontWeights.Bold,
                Foreground = Brushes.White,
                FontFamily = new FontFamily("Segoe UI Black"),
                VerticalAlignment = VerticalAlignment.Center
            };
            Grid.SetColumn(txtResult, 0);
            contentGrid.Children.Add(txtResult);

            StackPanel opponentPanel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                VerticalAlignment = VerticalAlignment.Center
            };
            Grid.SetColumn(opponentPanel, 1);

            Border avatarBorder = new Border
            {
                Width = 35,
                Height = 35,
                CornerRadius = new CornerRadius(17.5),
                BorderBrush = (SolidColorBrush)new BrushConverter().ConvertFrom("#FFD700"),
                BorderThickness = new Thickness(2),
                Margin = new Thickness(0, 0, 12, 0)
            };

            avatarBorder.Background = new ImageBrush
            {
                ImageSource = new BitmapImage(new Uri("pack://application:,,,/Assets/Gomoku.ico")),
                Stretch = Stretch.UniformToFill
            };
            opponentPanel.Children.Add(avatarBorder);

            TextBlock txtOpponent = new TextBlock
            {
                Text = opponentName,
                FontSize = 14,
                FontWeight = FontWeights.SemiBold,
                Foreground = (SolidColorBrush)new BrushConverter().ConvertFrom("#ECECEC"),
                VerticalAlignment = VerticalAlignment.Center
            };
            opponentPanel.Children.Add(txtOpponent);

            contentGrid.Children.Add(opponentPanel);

            TextBlock txtDuration = new TextBlock
            {
                Text = duration,
                FontSize = 13,
                Foreground = (SolidColorBrush)new BrushConverter().ConvertFrom("#ECECEC"),
                VerticalAlignment = VerticalAlignment.Center
            };
            Grid.SetColumn(txtDuration, 3);
            contentGrid.Children.Add(txtDuration);

            mainBorder.Child = contentGrid;
            return mainBorder;
        }

        public static Border CreateWinMatchItem(string opponentName, string duration)
        {
            Border mainBorder = new Border
            {
                Padding = new Thickness(20, 15, 20, 15),
                Margin = new Thickness(0),
                BorderBrush = (SolidColorBrush)new BrushConverter().ConvertFrom("#00D946"),
                BorderThickness = new Thickness(0, 0, 6, 0),
                Name = opponentName
            };

            LinearGradientBrush backgroundBrush = new LinearGradientBrush
            {
                StartPoint = new Point(0, 0.5),
                EndPoint = new Point(1, 0.5)
            };
            backgroundBrush.GradientStops.Add(new GradientStop((Color)ColorConverter.ConvertFromString("#00D946"), 0));
            backgroundBrush.GradientStops.Add(new GradientStop((Color)ColorConverter.ConvertFromString("#0F0F1E"), 0.13));
            mainBorder.Background = backgroundBrush;

            Grid contentGrid = new Grid();
            contentGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(140) });
            contentGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            contentGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(180) });
            contentGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(150) });

            TextBlock txtResult = new TextBlock
            {
                Text = "Thắng",
                FontSize = 14,
                FontWeight = FontWeights.Bold,
                Foreground = Brushes.White,
                FontFamily = new FontFamily("Segoe UI Black"),
                VerticalAlignment = VerticalAlignment.Center
            };
            Grid.SetColumn(txtResult, 0);
            contentGrid.Children.Add(txtResult);

            StackPanel opponentPanel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                VerticalAlignment = VerticalAlignment.Center
            };
            Grid.SetColumn(opponentPanel, 1);

            Border avatarBorder = new Border
            {
                Width = 35,
                Height = 35,
                CornerRadius = new CornerRadius(17.5),
                BorderBrush = (SolidColorBrush)new BrushConverter().ConvertFrom("#00D946"),
                BorderThickness = new Thickness(2),
                Margin = new Thickness(0, 0, 12, 0)
            };

            avatarBorder.Background = new ImageBrush
            {
                ImageSource = new BitmapImage(new Uri("pack://application:,,,/Assets/Gomoku.ico")),
                Stretch = Stretch.UniformToFill
            };
            opponentPanel.Children.Add(avatarBorder);

            TextBlock txtOpponent = new TextBlock
            {
                Text = opponentName,
                FontSize = 14,
                FontWeight = FontWeights.SemiBold,
                Foreground = (SolidColorBrush)new BrushConverter().ConvertFrom("#ECECEC"),
                VerticalAlignment = VerticalAlignment.Center
            };
            opponentPanel.Children.Add(txtOpponent);

            contentGrid.Children.Add(opponentPanel);

            TextBlock txtDuration = new TextBlock
            {
                Text = duration,
                FontSize = 13,
                Foreground = (SolidColorBrush)new BrushConverter().ConvertFrom("#ECECEC"),
                VerticalAlignment = VerticalAlignment.Center
            };
            Grid.SetColumn(txtDuration, 3);
            contentGrid.Children.Add(txtDuration);

            mainBorder.Child = contentGrid;
            return mainBorder;
        }

        public static Border CreateLoseMatchItem(string opponentName, string duration)
        {
            Border mainBorder = new Border
            {
                Padding = new Thickness(20, 15, 20, 15),
                Margin = new Thickness(0),
                BorderBrush = (SolidColorBrush)new BrushConverter().ConvertFrom("#FF4655"),
                BorderThickness = new Thickness(0, 0, 6, 0),
                Name = opponentName
            };

            LinearGradientBrush backgroundBrush = new LinearGradientBrush
            {
                StartPoint = new Point(0, 0.5),
                EndPoint = new Point(1, 0.5)
            };
            backgroundBrush.GradientStops.Add(new GradientStop((Color)ColorConverter.ConvertFromString("#FF4655"), 0));
            backgroundBrush.GradientStops.Add(new GradientStop((Color)ColorConverter.ConvertFromString("#0F0F1E"), 0.13));
            mainBorder.Background = backgroundBrush;

            Grid contentGrid = new Grid();
            contentGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(140) });
            contentGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            contentGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(180) });
            contentGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(150) });

            TextBlock txtResult = new TextBlock
            {
                Text = "Thua",
                FontSize = 14,
                FontWeight = FontWeights.Bold,
                Foreground = Brushes.White,
                FontFamily = new FontFamily("Segoe UI Black"),
                VerticalAlignment = VerticalAlignment.Center
            };
            Grid.SetColumn(txtResult, 0);
            contentGrid.Children.Add(txtResult);

            StackPanel opponentPanel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                VerticalAlignment = VerticalAlignment.Center
            };
            Grid.SetColumn(opponentPanel, 1);

            Border avatarBorder = new Border
            {
                Width = 35,
                Height = 35,
                CornerRadius = new CornerRadius(17.5),
                BorderBrush = (SolidColorBrush)new BrushConverter().ConvertFrom("#FF4655"),
                BorderThickness = new Thickness(2),
                Margin = new Thickness(0, 0, 12, 0)
            };

            avatarBorder.Background = new ImageBrush
            {
                ImageSource = new BitmapImage(new Uri("pack://application:,,,/Assets/Gomoku.ico")),
                Stretch = Stretch.UniformToFill
            };
            opponentPanel.Children.Add(avatarBorder);

            TextBlock txtOpponent = new TextBlock
            {
                Text = opponentName,
                FontSize = 14,
                FontWeight = FontWeights.SemiBold,
                Foreground = (SolidColorBrush)new BrushConverter().ConvertFrom("#ECECEC"),
                VerticalAlignment = VerticalAlignment.Center
            };
            opponentPanel.Children.Add(txtOpponent);

            contentGrid.Children.Add(opponentPanel);

            TextBlock txtDuration = new TextBlock
            {
                Text = duration,
                FontSize = 13,
                Foreground = (SolidColorBrush)new BrushConverter().ConvertFrom("#ECECEC"),
                VerticalAlignment = VerticalAlignment.Center
            };
            Grid.SetColumn(txtDuration, 3);
            contentGrid.Children.Add(txtDuration);

            mainBorder.Child = contentGrid;
            return mainBorder;
        }
    }
}
