using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;

// Make internals visible to the testing lib.
[assembly:InternalsVisibleTo("Ksuid.Test")]

namespace Ksuid
{
	internal static class Base62
	{
		static Base62()
		{
			CharacterSet = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz".ToCharArray();
		}

		public static char[] CharacterSet { get; private set; }

		const int offsetUppercase = 10;

		const int offsetLowercase = 36;

		/// <summary>
		/// Convert a byte array
		/// </summary>
		/// <param name="original">Byte array</param>
		/// <returns>Base62 string</returns>
		public static string Encode(byte[] src)
		{
			const ushort dstBase = 62;
			var digits = new List<int> { 0 };

			foreach (var s in src)
			{
				var carry = (int)s;

				for (var j = 0; j < digits.Count; j++)
				{
					carry += digits[j] << 8;
					digits[j] = carry % dstBase;
					carry = (carry / dstBase) | 0;
				}

				while(carry > 0)
				{
					digits.Add(carry % dstBase);
					carry = (carry / dstBase) | 0;
				}
			}

			var dst = new char[digits.Count];

			for (var i = 0; i < digits.Count; i++)
				dst[i] = CharacterSet[digits[i]];

			Array.Reverse(dst);

			return new string(dst);
		}

		/// <summary>
		/// Convert a Base62 string to byte array
		/// </summary>
		/// <param name="src">A Base62 string encoded string.</param>
		/// <returns>Byte array</returns>
		public static byte[] Decode(string src)
		{
			const ushort srcBase = 62;
			const ulong dstBase = 4294967296;

			var bytes = Encoding.ASCII.GetBytes(src);
			var parts = new byte[Constants.EncodedLength]
			{
				base62Value(bytes[0]),
				base62Value(bytes[1]),
				base62Value(bytes[2]),
				base62Value(bytes[3]),
				base62Value(bytes[4]),
				base62Value(bytes[5]),
				base62Value(bytes[6]),
				base62Value(bytes[7]),
				base62Value(bytes[8]),
				base62Value(bytes[9]),

				base62Value(bytes[10]),
				base62Value(bytes[11]),
				base62Value(bytes[12]),
				base62Value(bytes[13]),
				base62Value(bytes[14]),
				base62Value(bytes[15]),
				base62Value(bytes[16]),
				base62Value(bytes[17]),
				base62Value(bytes[18]),
				base62Value(bytes[19]),

				base62Value(bytes[20]),
				base62Value(bytes[21]),
				base62Value(bytes[22]),
				base62Value(bytes[23]),
				base62Value(bytes[24]),
				base62Value(bytes[25]),
				base62Value(bytes[26]),
				base62Value(bytes[27]),
				base62Value(bytes[28]),
			};

			var n = Constants.DecodedLength;
			var dst = new byte[n];
			var bp = new byte[parts.Length];
			Array.Copy(parts, bp, parts.Length);

			while (bp.Length > 0)
			{
				var quotient = new List<byte>();
				var remainder = (ulong)0;

				foreach (var c in bp)
				{
					var value = (ulong)c + (ulong)(remainder) * srcBase;
					var digit = value / dstBase;
					remainder = value % dstBase;

					if (quotient.Count != 0 || digit != 0)
						quotient.Add((byte)digit);
				}

				if (n < 4)
					throw new InvalidOperationException("Output buffer is too short.");

				dst[n - 4] = (byte)(remainder >> 24);
				dst[n - 3] = (byte)(remainder >> 16);
				dst[n - 2] = (byte)(remainder >> 8);
				dst[n - 1] = (byte)(remainder);
				n -= 4;
				bp = quotient.ToArray();
			}

			return dst;
		}

		/// <summary>
		/// Gets the base62 value of a given byte.
		/// </summary>
		/// <param name="digit">The digit as a byte.</param>
		private static byte base62Value(byte digit)
		{
			var casted = (int)digit;

			if (casted >= '0' && casted <= '9')
				return (byte)(casted - '0');

			if (casted >= 'A' && casted <= 'Z')
				return (byte)(offsetUppercase + (casted - 'A'));

			return (byte)(offsetLowercase + (casted - 'a'));
		}
	}
}
