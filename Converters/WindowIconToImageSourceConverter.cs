// WindowIconToImageSourceConverter.cs
//  Andrew Baylis
//  Created: 06/02/2026

using Avalonia.Controls;
using Avalonia.Data.Converters;
using System.Globalization;
using Avalonia.Media;

namespace AJBAvalonia.Converters;

public class WindowIconToImageSourceConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is WindowIcon windowIcon && targetType.IsAssignableFrom(typeof(Avalonia.Media.IImage)))
        {
            using var stream = new MemoryStream();
            windowIcon.Save(stream);
            stream.Position = 0;
            return new Avalonia.Media.Imaging.Bitmap(stream);
        }
        
        if (value is IImage image)
        {
            return image;
        }

        return null;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
