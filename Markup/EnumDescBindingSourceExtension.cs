// EnumDescBindingSourceExtension.cs
// Andrew Baylis
//  Created: 20/01/2024

#region using

using System.ComponentModel;
using Avalonia.Markup.Xaml;

#endregion

namespace AJBAvalonia.Markup;

public readonly struct EnumDesc : IEquatable<EnumDesc>
{
    public EnumDesc(object value, string description)
    {
        Description = description;
        Value = value;
    }

    #region Properties

    public string Description { get; }

    public object Value { get; }

    #endregion

    #region Static Methods

    public static bool operator ==(EnumDesc left, EnumDesc right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(EnumDesc left, EnumDesc right)
    {
        return !(left == right);
    }

    #endregion

    #region Override Methods

    public override bool Equals(object? obj)
    {
        if (obj is EnumDesc d)
        {
            return Equals(d);
        }

        return false;
    }

    public override int GetHashCode()
    {
        unchecked
        {
            var hash = 17;

            if (Value != null)
            {
                hash = hash * 31 + Value.GetHashCode();
            }

            if (!string.IsNullOrEmpty(Description))
            {
                hash = hash * 31 + Description.GetHashCode();
            }

            return hash;
        }
    }

    #endregion

    #region IEquatable<EnumDesc> Members

    public bool Equals(EnumDesc other)
    {
        return other.Value.Equals(Value) && other.Description == Description;
    }

    #endregion
}

/// <summary>
///     Sample use: DisplayMemberPath="Description" SelectedValuePath="Value" ItemsSource="{Binding
///     Source={ajbWpf:EnumDescBindingSource {x:Type model:SacTimeSlot}}}"
/// </summary>
public class EnumDescBindingSourceExtension : MarkupExtension
{
    #region Fields

    private Type? _enumType;

    #endregion

    public EnumDescBindingSourceExtension()
    {
    }

    public EnumDescBindingSourceExtension(Type enumType)
    {
        EnumType = enumType;
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

    #endregion

    #region Override Methods

    public override object ProvideValue(IServiceProvider serviceProvider)
    {
        if (null == _enumType)
        {
            throw new InvalidOperationException("The EnumType must be specified.");
        }

        return MakeEnumDescArray();
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
        if (_enumType != null)
        {
            var enumValues = Enum.GetValues(_enumType);

            for (var i = 0; i < enumValues.Length; i++)
            {
                if (enumValues.GetValue(i) is Enum e)
                {
                    var desc = GetEnumDescription(e);
                    enumDescArray.Add(new EnumDesc(e, desc));
                }
            }
        }

        return enumDescArray.ToArray();
    }

    #endregion
}