// DragAllowArgs.cs
//  Andrew Baylis
//  Created: 13/03/2026

#region using

using Avalonia.Input;

#endregion

namespace AJBAvalonia.DragDropInternal;

internal class DragAllowArgs : EventArgs
{
    public DragAllowArgs(PointerPressedEventArgs pointerArgs)
    {
        PointerArgs = pointerArgs;
        Allow = true;
    }

    #region Properties

    public bool Allow { get; set; }

    public PointerPressedEventArgs PointerArgs { get; }

    #endregion
}