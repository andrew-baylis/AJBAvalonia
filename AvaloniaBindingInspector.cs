// AvaloniaBindingInspector.cs
// Andrew Baylis
// Created: 29/06/2026

using Avalonia;

namespace AJBAvalonia;
/// <summary>
/// type must descend form AvaloniaObject for these helpers to work.
/// </summary>
public static class AvaloniaBindingInspector
{
    #region Public Methods

    /// <summary>
    ///     Returns all direct Avalonia properties (CLR-backed properties).
    /// </summary>
    public static IReadOnlyList<AvaloniaProperty> GetBindableDirectProperties(Type type)
    {
        return AvaloniaPropertyRegistry.Instance
            .GetRegistered(type)
            .Where(p => p.IsDirect)
            .ToList();
    }

    /// <summary>
    ///     Returns all AvaloniaProperty instances registered for the given type.
    ///     Includes styled, direct, attached, and inherited properties.
    /// </summary>
    public static IReadOnlyList<AvaloniaProperty> GetBindableProperties(Type type)
    {
        return AvaloniaPropertyRegistry.Instance
            .GetRegistered(type)
            .ToList();
    }

    /// <summary>
    ///     Returns all non-direct Avalonia properties (i.e. styled properties).
    ///     This includes attached styled properties as well.
    /// </summary>
    public static IReadOnlyList<AvaloniaProperty> GetBindableStyledProperties(Type type)
    {
        return AvaloniaPropertyRegistry.Instance
            .GetRegistered(type)
            .Where(p => !p.IsDirect)
            .ToList();
    }

    #endregion
}