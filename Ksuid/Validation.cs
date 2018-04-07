using System;
using System.Text.RegularExpressions;

namespace Ksuid
{
	internal static class Validation
	{
		internal static void ValidatePrefix(string field, string value)
		{
			if (!Constants.PrefixRegex.IsMatch(value))
				throw new ArgumentException(field, $"{field} is not valid. It must match [a-z\\d]+");
		}
	}
}
