// EnumComboBox.cs
//  Andrew Baylis
//  Created: 24/07/2025

#region using

using System.ComponentModel;
using AJBAvalonia.Markup;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Interactivity;

#endregion

namespace AJBAvalonia;

public class EnumComboBox : ComboBox
{
    #region Avalonia Properties

    public static readonly StyledProperty<IEnumerable<Enum>?> LimitedToSelectProperty = AvaloniaProperty.Register<EnumComboBox, IEnumerable<Enum>?>(nameof(LimitedToSelect));

    #endregion

    #region Fields

    private Type? _enumType;
    private IPointer? _pointer;

    #endregion

    public EnumComboBox()
    {
        DisplayMemberBinding = new Binding(nameof(EnumDesc.Description));
        SelectedValueBinding = new Binding(nameof(EnumDesc.Value));
    }

    #region Properties

    public Type? EnumType
    {
        get => _enumType;
        set
        {
            if (value != _enumType)
            {
                if (null != value)
                {
                    var enumType = Nullable.GetUnderlyingType(value) ?? value;

                    if (!enumType.IsEnum)
                    {
                        throw new ArgumentException("Type must be for an Enum.");
                    }
                }

                _enumType = value;
            }
        }
    }

    public IEnumerable<Enum>? LimitedToSelect
    {
        get => GetValue(LimitedToSelectProperty);
        set => SetValue(LimitedToSelectProperty, value);
    }

    protected override Type StyleKeyOverride => typeof(ComboBox);

    #endregion

    #region Protected Methods

    protected override void OnLoaded(RoutedEventArgs e)
    {
        base.OnLoaded(e);
        RefreshItemSourceList();
    }

    protected override void OnPointerEntered(PointerEventArgs e)
    {
        base.OnPointerEntered(e);
        _pointer = e.Pointer;
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        if (change.Property == LimitedToSelectProperty)
        {
            RefreshItemSourceList();
        }
        else if (change.Property == SelectedIndexProperty)
        {
            _pointer?.Capture(null);
        }

        base.OnPropertyChanged(change);
    }

    #endregion

    #region Private Methods

    private string GetEnumDescription(object enumerationValue)
    {
        var type = enumerationValue.GetType();

        if (!type.IsEnum)
        {
            throw new ArgumentException(@"EnumerationValue must be of Enum type", nameof(enumerationValue));
        }

        var enumString = enumerationValue.ToString();
        if (!string.IsNullOrEmpty(enumString))
        {
            var memberInfo = type.GetMember(enumString);

            if (memberInfo.Length > 0)
            {
                var attrs = memberInfo[0].GetCustomAttributes(typeof(DescriptionAttribute), false);

                if (attrs.Length > 0)
                {
                    return ((DescriptionAttribute) attrs[0]).Description;
                }
            }
        }

        //If we have no description attribute, just return the ToString of the enum
        return enumString ?? string.Empty;
    }

    private EnumDesc[] MakeEnumDescArray()
    {
        var enumDescArray = new List<EnumDesc>();
        if (LimitedToSelect != null)
        {
            enumDescArray.AddRange(from item in LimitedToSelect let desc = GetEnumDescription(item) select new EnumDesc(item, desc));
        }
        else if (_enumType != null)
        {
            var enumValues = Enum.GetValues(_enumType);
            enumDescArray.AddRange(from Enum e in enumValues let desc = GetEnumDescription(e) select new EnumDesc(e, desc));
        }

        return enumDescArray.ToArray();
    }

    private void RefreshItemSourceList()
    {
        var oldSelection = SelectedValue;
        try
        {
            ItemsSource = MakeEnumDescArray();
        }
        finally
        {
            SelectedValue = oldSelection;
        }

        if (SelectedIndex < 0)
        {
            SelectedIndex = 0;
        }
    }

    #endregion
}