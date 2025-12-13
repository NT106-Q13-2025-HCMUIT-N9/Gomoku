using System.Globalization;
using System.Windows.Data;

namespace Gomoku_Client.Converters
{
  public class SliderValueToWidthConverter : IMultiValueConverter
  {
    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
      if (values.Length == 4 &&
          values[0] is double currentValue &&
          values[1] is double minimum &&
          values[2] is double maximum &&
          values[3] is double actualWidth)
      {
        if (maximum - minimum == 0)
          return 0.0;

        double percentage = (currentValue - minimum) / (maximum - minimum);

        return actualWidth * percentage;
      }

      return 0.0;
    }

    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
    {
      throw new NotImplementedException();
    }
  }
}