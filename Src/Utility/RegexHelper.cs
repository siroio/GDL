using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace GDL
{
    public static class RegexHelper
    {
        public static bool Match(string target, string pattern, Action<Regex, bool>? callback = null)
        {
            var regex = new Regex(pattern);
            bool isMatch = regex.IsMatch(target);
            callback?.Invoke(regex, isMatch);
            return isMatch;
        }

        public static bool MatchArray(IEnumerable<string> targets, string pattern, Action<Regex, bool>? callback = null)
        {
            bool result = false;
            foreach (var target in targets)
            {
                result = Match(target, pattern, callback);
            }
            return result;
        }
    }
}
