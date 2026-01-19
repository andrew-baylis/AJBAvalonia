// CustomTypeBinding.cs
//  Andrew Baylis
//  Created: 14/09/2024

using System.ComponentModel;
using System.Reactive.Linq;

using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Data.Core;
using Avalonia.Markup.Xaml;
using Avalonia.Utilities;

namespace AJBAvalonia.CustomTypeObjects;

public class CustomTypeBinding : Binding
{
    public CustomTypeBinding()
    {
    }

    public CustomTypeBinding(string path) : base(path)
    {
    }
    [Obsolete]
    public override InstancedBinding? Initiate(AvaloniaObject target, AvaloniaProperty? targetProperty, object? anchor = null, bool enableDataValidation = false)
    {
        return base.Initiate(target, targetProperty, anchor, enableDataValidation);
    }
}