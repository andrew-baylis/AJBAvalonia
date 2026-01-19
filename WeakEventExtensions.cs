// WeakEventExtensions.cs
// Andrew Baylis
//  Created: 22/09/2024

#region using

using System.ComponentModel;
using Avalonia.Utilities;

#endregion

namespace AJBAvalonia;

public static class WeakEventExtensions
{
    #region Static Methods

    public static void PropertyChangedSubscribe<THostClass>(INotifyPropertyChanged target, EventHandler<PropertyChangedEventArgs> handlerInHost) where THostClass : class
    {
        WeakEventHandlerManager.Subscribe<INotifyPropertyChanged, PropertyChangedEventArgs, THostClass>(target, nameof(INotifyPropertyChanged.PropertyChanged), handlerInHost);
    }

    public static void PropertyChangedUnsubscribe<THostClass>(INotifyPropertyChanged target, EventHandler<PropertyChangedEventArgs> handlerInHost) where THostClass : class
    {
        WeakEventHandlerManager.Unsubscribe<PropertyChangedEventArgs, THostClass>(target, nameof(INotifyPropertyChanged.PropertyChanged), handlerInHost);
    }

    #endregion
}