using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;

namespace Gomoku_Client.View
{
  public partial class Setting : Page
  {
    private MainGameUI _mainWindow;
    public Setting(MainGameUI mainGameUI)
    {
      InitializeComponent();


            GeneralVolumeSlider.Value = mainGameUI.MasterVolValue * 100;
            BackgroundVolumeSlider.Value = mainGameUI.BGMVolValue * 100;
            SfxVolumeSlider.Value = mainGameUI.SFXVolValue * 100;

            AddSliderHoverEffects(GeneralSliderContainer, GeneralDecBtn, GeneralIncBtn);
      AddSliderHoverEffects(BackgroundVolumeSliderContainer, BackgroundVolumeDecBtn, BackgroundVolumeIncBtn);
      AddSliderHoverEffects(SfxSliderContainer, SfxDecBtn, SfxIncBtn);

      AddSliderClickToPosition(GeneralVolumeSlider);
      AddSliderClickToPosition(BackgroundVolumeSlider);
      AddSliderClickToPosition(SfxVolumeSlider);

      _mainWindow = mainGameUI;
    }

    private void AddSliderHoverEffects(Grid container, Button decBtn, Button incBtn)
    {
      container.MouseEnter += (s, e) =>
      {
        decBtn.Opacity = 1;
        incBtn.Opacity = 1;
      };

      container.MouseLeave += (s, e) =>
      {
        decBtn.Opacity = 0;
        incBtn.Opacity = 0;
      };
    }

    private void AddSliderClickToPosition(Slider slider)
    {
      bool isDragging = false;

      // click don
      slider.PreviewMouseLeftButtonDown += (s, e) =>
      {
        if (s is Slider clickedSlider)
        {
          var track = GetTrackFromSlider(clickedSlider);
          if (track != null)
          {
            var thumb = track.Thumb;

            Point mousePosition = e.GetPosition(clickedSlider);
            double sliderWidth = clickedSlider.ActualWidth;

            double percentage = mousePosition.X / sliderWidth;
            percentage = Math.Max(0, Math.Min(1, percentage));

            double range = clickedSlider.Maximum - clickedSlider.Minimum;
            double newValue = clickedSlider.Minimum + (range * percentage);

            clickedSlider.Value = newValue;

            // Bắt đầu kéo tùy chỉnh
            isDragging = true;
            clickedSlider.CaptureMouse();
            e.Handled = true;
          }
        }
      };

      // keo
      slider.PreviewMouseMove += (s, e) =>
      {
        if (isDragging && s is Slider draggedSlider)
        {
          Point mousePosition = e.GetPosition(draggedSlider);
          double sliderWidth = draggedSlider.ActualWidth;

          double percentage = mousePosition.X / sliderWidth;
          percentage = Math.Max(0, Math.Min(1, percentage));

          double range = draggedSlider.Maximum - draggedSlider.Minimum;
          double newValue = draggedSlider.Minimum + (range * percentage);

          draggedSlider.Value = newValue;
          e.Handled = true;
        }
      };

      //tha
      slider.PreviewMouseLeftButtonUp += (s, e) =>
      {
        if (isDragging && s is Slider releasedSlider)
        {
          isDragging = false;
          releasedSlider.ReleaseMouseCapture();
          e.Handled = true;
        }
      };
    }

    private Track? GetTrackFromSlider(Slider slider)
    {
      var template = slider.Template;
      if (template != null)
      {
        return template.FindName("PART_Track", slider) as Track;
      }
      return null;
    }

    private void DecreaseVolume_Click(object sender, RoutedEventArgs e)
    {
            _mainWindow.ButtonClick.Stop();
            _mainWindow.ButtonClick.Play();
            if (sender is Button button && button.Tag is string sliderName)
      {
        var slider = FindName(sliderName) as Slider;
        if (slider != null)
        {
          slider.Value = Math.Max(slider.Minimum, slider.Value - 5);
        }
      }
    }

    private void IncreaseVolume_Click(object sender, RoutedEventArgs e)
    {
            _mainWindow.ButtonClick.Stop();
            _mainWindow.ButtonClick.Play();
            if (sender is Button button && button.Tag is string sliderName)
      {
        var slider = FindName(sliderName) as Slider;
        if (slider != null)
        {
          slider.Value = Math.Min(slider.Maximum, slider.Value + 5);
        }
      }
    }

    private void BackButton_Checked(object sender, RoutedEventArgs e)
    {
      if (_mainWindow == null)
      {
        MessageBox.Show("Không tìm thấy cửa sổ chính.", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
        return;
      }
            _mainWindow.ButtonClick.Stop();
            _mainWindow.ButtonClick.Play();
      _mainWindow.ShowMenuWithAnimation();
    }

        private void BackgroundVolumeSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (_mainWindow != null && BackgroundVolumeSlider != null)
            {
                _mainWindow.BGMVolValue = e.NewValue / 100.0;

                _mainWindow.UpdateActualBGM();
            }
        }

        private void GeneralVolumeSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (_mainWindow != null && GeneralVolumeSlider != null)
            {
                _mainWindow.MasterVolValue = e.NewValue / 100.0;

                _mainWindow.UpdateActualBGM();
            }
        }

        private void SfxVolumeSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (_mainWindow != null && SfxVolumeSlider != null)
            {
                _mainWindow.SFXVolValue = e.NewValue / 100.0;

                _mainWindow.UpdateActualBGM();
            }
        }
    }
}