using System;
using System.Text.RegularExpressions;

namespace Ksuid
{
	internal static class Extensions
	{
		/// <summary>
		/// The DateTime representation of the Unix epoch.
		/// </summary>
		private static readonly DateTime UnixEpoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

		/// <summary>
		/// Gets the unix timestamp representation of the specified time.
		/// </summary>
		internal static UInt64 ToUnixTime(this DateTime dateTime)
		{
			var timeSpan = (dateTime - UnixEpoch);

			return Convert.ToUInt64(timeSpan.TotalSeconds);
		}
	}
}
