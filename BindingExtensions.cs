// BindingExtensions.cs
//  Andrew Baylis
//  Created: 25/08/2025

#region using

using Avalonia;
using Avalonia.Data;
using System.Reflection;

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

    public static object? GetTargetObjectForBinding(this AvaloniaObject element, AvaloniaProperty property)
    {
        var b = BindingOperations.GetBindingExpressionBase(element, property);
        if (b != null)
        {
            var targetObjectMethodInfo = b.GetType().GetMethod("TryGetTarget", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic,
                null,
                [typeof(object).MakeByRefType()], null);
            if (targetObjectMethodInfo != null)
            {
                var p = new object?[] { null };
                var result = targetObjectMethodInfo.Invoke(b, p);
                if (result is bool bResult && bResult)
                {
                    return p[0];
                }
            }
        }
        return null;
    }

    #endregion
}