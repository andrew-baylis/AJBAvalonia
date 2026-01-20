// RegExExtensions.cs
//  Andrew Baylis
//  Created: 19/01/2026

#region using

using System.Text.RegularExpressions;

#endregion

namespace AJBAvalonia;

public static class RegExExtensions
{
    #region Fields

    public static readonly Regex anyCurrency = new(@"^[+-]?\p{Sc}?(\d*(\.\d{0,2})?)$", RegexOptions.Compiled);
    public static readonly Regex anyDouble = new(@"^[+-]?(\d*(\.\d*)?)$", RegexOptions.Compiled);
    public static readonly Regex anyInteger = new(@"^[+-]?[0-9]?$", RegexOptions.Compiled);
    public static readonly Regex emailRegex = new(@"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$", RegexOptions.Compiled);
    public static readonly Regex positiveCurrency = new(@"^\p{Sc}?(\d*(\.\d{0,2})?)$", RegexOptions.Compiled);
    public static readonly Regex positiveDouble = new(@"^(\d*(\.\d*)?)$", RegexOptions.Compiled);
    public static readonly Regex positiveInteger = new(@"^[0-9]?$", RegexOptions.Compiled);
    public static readonly Regex time12hrClock = new(@"^(?<hour>1[0-2]|0?[0-9])(?::(?<minute>[0-5]?[0-9])?(?::(?<seconds>[0-5]?[0-9])?)?)?\s*(?<ampm>[AaPp][Mm]?)?$$",
                                                     RegexOptions.Compiled);
    public static readonly Regex time24hrClock = new(@"^(?<hour>2[0-3]|1[0-2]|0?[0-9])(?::(?<minute>[0-5]?[0-9])?(?::(?<seconds>[0-5]?[0-9])?)?)?$", RegexOptions.Compiled);
    public static readonly Regex urlRegex = new(@"^(https?:\/\/)?([\w\d\.-]+)\.([\w\.]{2,6})([\/].*)?$", RegexOptions.Compiled);

    #endregion

    #region Static Methods

    public static bool MatchAnyCurrency(string input)
    {
        return anyCurrency.IsMatch(input);
    }

    public static bool MatchAnyDouble(string input)
    {
        return anyDouble.IsMatch(input);
    }

    public static bool MatchAnyInteger(string input)
    {
        return anyInteger.IsMatch(input);
    }

    public static bool MatchEmail(string email)
    {
        return emailRegex.IsMatch(email);
    }

    public static bool MatchPositiveCurrency(string input)
    {
        return positiveCurrency.IsMatch(input);
    }

    public static bool MatchPositiveDouble(string input)
    {
        return positiveDouble.IsMatch(input);
    }

    public static bool MatchPositiveInteger(string input)
    {
        return positiveInteger.IsMatch(input);
    }

    public static bool MatchTime12HrClock(string time)
    {
        return time12hrClock.IsMatch(time);
    }

    public static bool MatchTime24HrClock(string time)
    {
        return time24hrClock.IsMatch(time);
    }

    public static bool MatchUrl(string url)
    {
        return urlRegex.IsMatch(url);
    }

    #endregion

    #region Nested type: $extension

    extension(string text)
    {
        #region Public Methods

        public bool IsValidEmail()
        {
            return MatchEmail(text);
        }

        public bool IsValidTime12Hr()
        {
            return MatchTime12HrClock(text);
        }

        public bool IsValidTime24Hr()
        {
            return MatchTime24HrClock(text);
        }

        public bool IsValidUrl()
        {
            return MatchUrl(text);
        }

        #endregion
    }

    #endregion
}