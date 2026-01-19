// BindingExtensions.cs
//  Andrew Baylis
//  Created: 25/08/2025

#region using

using System.Reflection;
using Avalonia;
using Avalonia.Data;

#endregion

namespace AJBAvalonia;

public static class BindingExtensions
{
    #region Static Methods

    public static Type? GetSourceTypeForBinding(this AvaloniaObject element, AvaloniaProperty property)
    {
        var b = BindingOperations.GetBindingExpressionBase(element, property);
        if (b != null)
        {
            var sourcetypeProp = b.GetType().GetProperty("SourceType", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (sourcetypeProp != null)
            {
                return sourcetypeProp.GetValue(b) as Type;
            }
        }

        return null;
    }

    #endregion
}