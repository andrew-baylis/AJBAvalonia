// AJBAvaloniaFluent.xaml.cs
// Andrew Baylis
//  Created: 20/01/2024

#region using

using Avalonia.Markup.Xaml;
using Avalonia.Styling;

#endregion

namespace AJBAvalonia;

public class AJBControlsFluent : Styles
{
    public AJBControlsFluent(IServiceProvider? sp = null)
    {
        AvaloniaXamlLoader.Load(sp, this);
    }
}