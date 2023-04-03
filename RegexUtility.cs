using System;
using System.Text.RegularExpressions;

namespace GDL
{
    internal class RegexUtility
    {
        public static bool Match(string target, string pattern, Action<Regex, bool>? callback = null)
        {
            var regex = new Regex(pattern);
            bool isMatch = regex.IsMatch(target);
            callback?.Invoke(regex, isMatch);
            return isMatch;
        }
    }
}
