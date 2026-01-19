// TransparentButton.cs
// Andrew Baylis
//  Created: 18/10/2024

using Avalonia.Controls;
using Avalonia.Media;

namespace AJBAvalonia;

internal class TransparentButton : Button
{
    public TransparentButton()
    {
        Background = new SolidColorBrush(Colors.Transparent);
        BorderBrush = new SolidColorBrush(Colors.Transparent);
    }
}