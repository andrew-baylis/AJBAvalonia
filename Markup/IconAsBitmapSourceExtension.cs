// IconAsBitmapSourceExtension.cs
//  Andrew Baylis
//  Created: 08/02/2026

#region using

using System.Reflection;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Media.Imaging;
using Avalonia.Platform;

#endregion

namespace AJBAvalonia.Markup;

public class IconAsBitmapSourceExtension : MarkupExtension
{
    #region Properties

    public Uri? Source { get; set; }

    #endregion

    #region Public Methods

    public override Bitmap ProvideValue(IServiceProvider serviceProvider)
    {
        if (Source != null)
        {
            if (!Source.IsAbsoluteUri)
            {
                var s = Source.OriginalString;
                if (s.StartsWith('/'))
                {
                    var assemblyName = Assembly.GetExecutingAssembly().GetName();
                    Source = new Uri($"avares://{assemblyName.Name}{Source.OriginalString}");
                }
                else
                {
                    var callerUri = ((IUriContext?) serviceProvider.GetService(typeof(IUriContext)))?.BaseUri;
                    if (callerUri != null)
                    {
                        Source = new Uri(callerUri, Source.OriginalString);
                    }
                }
            }

            if (Source.OriginalString.EndsWith(".ico"))
            {
                var windowIcon = new WindowIcon(AssetLoader.Open(Source));
                using var stream = new MemoryStream();
                windowIcon.Save(stream);
                stream.Position = 0;
                return new Bitmap(stream);
            }

            return new Bitmap(AssetLoader.Open(Source));
        }

        throw new InvalidOperationException("Source must be set");
    }

    #endregion
}