// TextEntryBehaviour.cs
//  Andrew Baylis
//  Created: 20/01/2024

#region using

using Avalonia.Controls;
using Avalonia.Xaml.Interactivity;

#endregion

namespace AJBAvalonia;

public enum TextEntryEnum
{
    PositiveInteger,
    AnyInteger,
    PositiveDouble,
    AnyDouble,
    PositiveCurrency,
    AnyCurrency,
    Time12Hr,
    Time24Hr,
    Email,
    Url,
    AnyText
}

public class TextEntryBehaviour : Behavior<TextBox>
{
    #region Properties

    /// <summary>
    ///     Gets or sets the type of text entry for the associated TextBox.
    /// </summary>
    /// <value>
    ///     The type of text entry, which determines the validation rules for the TextBox.
    ///     The default is <see cref="TextEntryEnum.AnyDouble" />.
    /// </value>
    /// <remarks>
    ///     This property is used in conjunction with the <see cref="TextEntryBehaviour" /> to
    ///     apply specific validation rules to a TextBox. For example, if this property is set
    ///     to <see cref="TextEntryEnum.PositiveInteger" />, the TextBox will only accept positive integers.
    /// </remarks>

    public TextEntryEnum TextEntry { get; set; } = TextEntryEnum.AnyDouble;

    #endregion

    #region Protected Methods

    protected override void OnAttached()
    {
        base.OnAttached();
        if (AssociatedObject != null)
        {
            AssociatedObject.TextChanging += AssociatedObjectOnTextChanging;
        }
    }

    protected override void OnDetaching()
    {
        if (AssociatedObject != null)
        {
            AssociatedObject.TextChanging -= AssociatedObjectOnTextChanging;
        }

        base.OnDetaching();
    }

    #endregion

    #region Private Methods

    private void AssociatedObjectOnTextChanging(object? sender, TextChangingEventArgs e)
    {
        if (AssociatedObject != null)
        {
            var s = AssociatedObject.Text;
            if (!string.IsNullOrEmpty(s))
            {
                var accept = TextEntry switch
                {
                    TextEntryEnum.AnyInteger => RegExExtensions.anyInteger.IsMatch(s),
                    TextEntryEnum.AnyDouble => RegExExtensions.anyDouble.IsMatch(s),
                    TextEntryEnum.AnyCurrency => RegExExtensions.anyCurrency.IsMatch(s),
                    TextEntryEnum.PositiveCurrency => RegExExtensions.positiveCurrency.IsMatch(s),
                    TextEntryEnum.PositiveDouble => RegExExtensions.positiveDouble.IsMatch(s),
                    TextEntryEnum.PositiveInteger => RegExExtensions.positiveInteger.IsMatch(s),
                    TextEntryEnum.Time12Hr => RegExExtensions.time12hrClock.IsMatch(s),
                    TextEntryEnum.Time24Hr => RegExExtensions.time24hrClock.IsMatch(s),
                    TextEntryEnum.Email => RegExExtensions.emailRegex.IsMatch(s),
                    TextEntryEnum.Url => RegExExtensions.urlRegex.IsMatch(s),
                    _ => true
                };
                if (!accept)
                {
                    AssociatedObject.Undo();
                }
            }
        }
    }

    #endregion

    //private readonly Regex anyCurrency = new(@"^[+-]?\p{Sc}?(\d*(\.\d{0,2})?)$", RegexOptions.Compiled);
    //private readonly Regex anyDouble = new(@"^[+-]?(\d*(\.\d*)?)$", RegexOptions.Compiled);
    //private readonly Regex anyInteger = new(@"^[+-]?[0-9]?$", RegexOptions.Compiled);
    //private readonly Regex positiveCurrency = new(@"^\p{Sc}?(\d*(\.\d{0,2})?)$", RegexOptions.Compiled);
    //private readonly Regex positiveDouble = new(@"^(\d*(\.\d*)?)$", RegexOptions.Compiled);
    //private readonly Regex positiveInteger = new(@"^[0-9]?$", RegexOptions.Compiled);
    //private readonly Regex time12hrClock = new(@"^(?<hour>1[0-2]|0?[0-9])(?::(?<minute>[0-5]?[0-9])?(?::(?<seconds>[0-5]?[0-9])?)?)?\s*(?<ampm>[AaPp][Mm]?)?$$", RegexOptions.Compiled);
    //private readonly Regex time24hrClock = new(@"^(?<hour>2[0-3]|1[0-2]|0?[0-9])(?::(?<minute>[0-5]?[0-9])?(?::(?<seconds>[0-5]?[0-9])?)?)?$", RegexOptions.Compiled);
}