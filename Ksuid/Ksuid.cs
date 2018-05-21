using System;
using System.Threading;

namespace Ksuid
{
	public class Ksuid
	{
		private static Node _singleton = new Node();

		/// <summary>
		/// Gets or sets the Environment of the Singleton Node.
		/// </summary>
		public static string Environment
		{
			get { return _singleton.Environment; }
			set { _singleton.Environment = value; }
		}

		/// <summary>
		/// Gets or sets the Instance Identifier of the Singleton Node.
		/// </summary>
		public static InstanceIdentifier InstanceIdentifier
		{
			get { return _singleton.InstanceIdentifier; }
			set { _singleton.InstanceIdentifier = value; }
		}

		/// <summary>
		/// Generates a KSUID using the Singleton Node.
		/// </summary>
		/// <param name="resource">The resource of the KSUID.</param>
		public static Id Generate(string resource)
		{
			return _singleton.Generate(resource);
		}

		/// <summary>
		/// Parses a string id represention of a KSUID.
		/// </summary>
		/// <param name="id">The string representation of the ksuid.</param>
		public static Id Parse(string input)
		{
			var parts = splitPrefix(input);
			var decoded = Base62.Decode(parts.payload);

			var timestamp = new byte[8];
			var instance = new byte[9];
			var sequence = new byte[4];

			Array.Copy(decoded, timestamp, 8);
			Array.Copy(decoded, 8, instance, 0, 9);
			Array.Copy(decoded, 17, sequence, 0, 4);

			if (BitConverter.IsLittleEndian)
			{
				Array.Reverse(timestamp);
				Array.Reverse(sequence);
			}

			return new Id(
				parts.environment,
				parts.resource,
				BitConverter.ToUInt64(timestamp, 0),
				new InstanceIdentifier(instance),
				BitConverter.ToUInt32(sequence, 0)
			);
		}

		/// <summary>
		/// Split a KSUID string into it's three parts.
		/// </summary>
		/// <param name="environment">The environment of the KSUID.</param>
		/// <param name="resource">The resource of the KSUID.</param>
		/// <param name="payload">The encoded payload of the KSUID.</param>
		private static (string environment, string resource, string payload) splitPrefix(string input)
		{
			var match = Constants.KsuidRegex.Match(input);
			if (!match.Success || match.Groups.Count != 4)
				throw new FormatException("Ksuid format is invalid");

			var env = match.Groups[1].Value;

			if (match.Groups[1].Value == "prod")
				throw new FormatException("Production environment is implied. Remove \"prod_\".");

			return (
				String.IsNullOrEmpty(env) ? "prod" : env,
				match.Groups[2].Value,
				match.Groups[3].Value
			);
		}
	}
}
