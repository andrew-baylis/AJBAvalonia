// CustomTypeLinkObject.cs
//  Andrew Baylis
//  Created: 14/09/2024

#region using

using System.ComponentModel;
using Avalonia;
using Avalonia.Data;
using Avalonia.Utilities;

#endregion

namespace AJBAvalonia.CustomTypeObjects;

/// <summary>
///     Used for linking a ICustomTypeDescriptor object to a binding
/// </summary>
public class CustomTypeLinkObject : AvaloniaObject
{
    #region Avalonia Properties

    public static readonly DirectProperty<CustomTypeLinkObject, object?> SourceProperty =
        AvaloniaProperty.RegisterDirect<CustomTypeLinkObject, object?>(nameof(Source), o => o.Source, (o, v) => o.Source = v);
    public static readonly DirectProperty<CustomTypeLinkObject, object?> TargetPropertyValueProperty =
        AvaloniaProperty.RegisterDirect<CustomTypeLinkObject, object?>(nameof(TargetPropertyValue), o => o.TargetPropertyValue, (o, v) => o.TargetPropertyValue = v);
    public static readonly StyledProperty<object?> SourceTempProperty = AvaloniaProperty.Register<CustomTypeLinkObject, object?>(nameof(SourceTemp));

    public object? SourceTemp
    {
        get => GetValue(SourceTempProperty);
        set => SetValue(SourceTempProperty, value);
    }
    #endregion

    #region Fields

    private object? _source;

    private object? _targetPropertyValue;

    private IDisposable? _bindingToTarget;

    #endregion

    #region Properties

    public BindingMode Mode { get; set; } = BindingMode.Default;

    public string Path { get; set; } = string.Empty;

    public object? Source
    {
        get => _source;
        set
        {
            if (_source is INotifyPropertyChanged inot)
            {
                WeakEventHandlerManager.Unsubscribe<PropertyChangedEventArgs, CustomTypeLinkObject>(_source,
                                                                                                    nameof(INotifyPropertyChanged.PropertyChanged),
                                                                                                    OnSourcePropertyChanged);
            }

            SetAndRaise(SourceProperty, ref _source, value);
            if (_source is INotifyPropertyChanged p)
            {
                WeakEventHandlerManager.Subscribe<INotifyPropertyChanged, PropertyChangedEventArgs, CustomTypeLinkObject>(
                    p,
                    nameof(INotifyPropertyChanged.PropertyChanged),
                    OnSourcePropertyChanged);
            }

            GetSourcePropertyValue();
        }
    }

    public object? TargetPropertyValue
    {
        get => _targetPropertyValue;
        set
        {
            SetAndRaise(TargetPropertyValueProperty, ref _targetPropertyValue, value);
            SetSourcePropertyValue();
        }
    }

    #endregion

    #region Private Methods

    private void GetSourcePropertyValue()
    {
        if (_source is ICustomTypeDescriptor custom && !string.IsNullOrEmpty(Path))
        {
            var prop = custom.GetProperties().Find(Path, true);
            var oldtargetValue = _targetPropertyValue;
            _targetPropertyValue = prop?.GetValue(_source);
            RaisePropertyChanged(TargetPropertyValueProperty, oldtargetValue, _targetPropertyValue);
        }
    }

    private void OnSourcePropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        GetSourcePropertyValue();
    }

    private void SetSourcePropertyValue()
    {
        if (Mode == BindingMode.Default || Mode == BindingMode.TwoWay || Mode == BindingMode.OneWayToSource)
        {
            if (_source is ICustomTypeDescriptor custom && !string.IsNullOrEmpty(Path))
            {
                var prop = custom.GetProperties().Find(Path, true);
                if (prop?.IsReadOnly == false)
                {
                    prop?.SetValue(_source, TargetPropertyValue);
                }
            }
        }
    }

    #endregion
}