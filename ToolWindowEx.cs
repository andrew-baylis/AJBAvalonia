// ToolWindowEx.cs
//  Andrew Baylis
//  Created: 06/02/2026

#region using

#endregion

namespace AJBAvalonia;

/// <summary>
///     ToolWindow has narrower caption bar, no icon and no system menu.
///     It is designed to be used as a child window of a main application window,
///     such as a dockable panel or a floating tool palette. It provides a more compact and
///     streamlined user interface for tools
///     and utilities that do not require the full functionality of a standard window.
/// </summary>
public class ToolWindowEx : DialogWindowEx
{
    #region Properties

    protected override Type StyleKeyOverride => typeof(ToolWindowEx);

    #endregion
}