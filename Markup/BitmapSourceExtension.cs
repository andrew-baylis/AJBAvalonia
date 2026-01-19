// BitmapSourceExtension.cs
// Andrew Baylis
//  Created: 20/01/2024

#region using

using System.Reflection;
using Avalonia.Markup.Xaml;
using Avalonia.Media.Imaging;
using Avalonia.Platform;

#endregion

namespace AJBAvalonia.Markup;

public class BitmapSourceExtension : MarkupExtension
{
    #region Properties

    public Uri? Source { get; set; }

    #endregion

    #region Override Methods

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

            return new Bitmap(AssetLoader.Open(Source));
        }

        throw new InvalidOperationException("Source must be set");
    }

    #endregion
}