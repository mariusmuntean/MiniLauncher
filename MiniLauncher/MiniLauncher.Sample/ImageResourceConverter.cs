using System;
using System.Globalization;
using System.Reflection;
using Xamarin.Forms;

namespace MiniLauncher.Sample
{
    public class ImageResourceConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var imageSource = ImageSource.FromResource(value as string, typeof(ImageResourceConverter).GetTypeInfo().Assembly);
            return imageSource;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}