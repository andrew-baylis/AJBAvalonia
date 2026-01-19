// IconSourceExtension.cs
// Andrew Baylis
//  Created: 19/05/2024

#region using

using System.Reflection;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Platform;

#endregion

namespace AJBAvalonia.Markup;

public class IconSourceExtension : MarkupExtension
{
    #region Properties

    public Uri? Source { get; set; }

    #endregion

    #region Override Methods

    public override WindowIcon ProvideValue(IServiceProvider serviceProvider)
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

            return new WindowIcon(AssetLoader.Open(Source));
        }

        throw new InvalidOperationException("Source must be set");
    }

    #endregion
}