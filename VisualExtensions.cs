// VisualExtensions.cs
//  Andrew Baylis
//  Created: 06/02/2026

#region using

using Avalonia;
using Avalonia.Controls;
using Avalonia.LogicalTree;
using Avalonia.Media.Imaging;
using Avalonia.VisualTree;

#endregion

namespace AJBAvalonia;

public static class VisualExtensions
{
    #region Static Methods

    public static List<T> GetAllChildrenOfType<T>(this Visual control)
    {
        var children = new List<T>();
        foreach (var item in control.GetVisualChildren())
        {
            if (item is T item1)
            {
                children.Add(item1);
            }

            children.AddRange(item.GetAllChildrenOfType<T>());
        }

        return children;
    }

    public static T? GetChildOfType<T>(this Visual control, string? name = null)
    {
        foreach (var item in control.GetVisualChildren())
        {
            if (item is T item1)
            {
                if (!string.IsNullOrEmpty(name))
                {
                    if (item.Name == name)
                    {
                        return item1;
                    }
                }
                else
                {
                    return item1;
                }
            }

            var res = item.GetChildOfType<T>(name);
            if (res != null)
            {
                return res;
            }
        }

        return default;
    }

    public static T? GetFirstLogicalChildOfType<T>(this ILogical control)
    {
        foreach (var item in control.GetLogicalChildren())
        {
            if (item is T item1)
            {
                return item1;
            }

            var res = item.GetFirstLogicalChildOfType<T>();
            if (res != null)
            {
                return res;
            }
        }

        return default;
    }

    public static T? GetVisualParent<T>(this Visual visual) where T : Visual
    {
        var p = visual.GetVisualParent();
        while (p != null && p is not T)
        {
            p = p.GetVisualParent();
        }

        return p as T;
    }

    public static Bitmap? ConvertToBitmap(this WindowIcon icon)
    {
        using var stream = new MemoryStream();
        icon.Save(stream);
        stream.Position = 0;
        return new Bitmap(stream);
    }
    
    #endregion
}