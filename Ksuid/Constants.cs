using System;
using System.Text.RegularExpressions;

namespace Ksuid
{
	public static class Constants
	{
		/// <summary>
		/// The length of the payload when it has been decoded from Base62.
		/// </summary>
		public const int DecodedLength = 21;

		/// <summary>
		/// The length of the payload when it has been encoded to Base62.
		/// </summary>
		public const int EncodedLength = 29;

		/// <summary>
		/// The compiled regex for validating a Ksuid. With capture groups for the
		/// environment, resource, and payload.
		/// </summary>
		public static readonly Regex KsuidRegex = new Regex(
			@"(?:(?<environment>[a-z\d]+)_)?(?:(?<resource>[a-z\d]+)_{1})(?<payload>[a-zA-Z\d]{29})",
			RegexOptions.Compiled
		);

		/// <summary>
		/// The compiled regex for validating the prefix of a ksuid.
		/// </summary>
		public static readonly Regex PrefixRegex = new Regex(
			@"[a-z\d]+",
			RegexOptions.Compiled
		);
	}
}
