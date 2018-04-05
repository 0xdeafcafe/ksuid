using System;
using System.Text.RegularExpressions;

namespace Ksuid
{
	public class Id
	{
		public Id(string environment, string resource, UInt64 timestamp,
			InstanceIdentifier instanceIdentifier, UInt32 sequenceId)
		{
			Validation.ValidatePrefix(nameof(environment), environment);
			Validation.ValidatePrefix(nameof(resource), resource);

			if (instanceIdentifier == null)
				throw new ArgumentNullException(nameof(instanceIdentifier), $"{nameof(instanceIdentifier)} cannot be null.");

			Environment = environment;
			Resource = resource;
			Timestamp = timestamp;
			InstanceIdentifier = instanceIdentifier;
			SequenceId = sequenceId;
		}

		/// <summary>
		/// Gets the environment of the ksuid.
		/// </summary>
		public string Environment { get; private set; }

		/// <summary>
		/// Gets the resource of the ksuid.
		/// </summary>
		public string Resource { get; private set; }

		/// <summary>
		/// Gets the timestamp of the ksuid.
		/// </summary>
		public UInt64 Timestamp { get; private set; }

		/// <summary>
		/// Gets the instance identifier of the ksuid.
		/// </summary>
		public InstanceIdentifier InstanceIdentifier { get; private set; }

		/// <summary>
		/// Gets the sequence id of the ksuid.
		/// </summary>
		public UInt32 SequenceId { get; private set; }

		/// <summary>
		/// Since the id is immutable, we only actually convert it to a string once. This
		/// stores that.
		/// </summary>
		private string str;

		/// <summary>
		/// Gets the string representation of the ksuid.
		/// </summary>
		public override string ToString()
		{
			if (str != null)
				return str;

			var env = Environment == "prod" ? "" : $"{Environment}_";
			var prefix = $"{env}{Resource}_";
			var decoded = new byte[Constants.DecodedLength];

			var timestamp = BitConverter.GetBytes(Timestamp);
			var instance = InstanceIdentifier.ToByteArray();
			var sequenceId = BitConverter.GetBytes(SequenceId);

			if (BitConverter.IsLittleEndian)
			{
				Array.Reverse(timestamp);
				Array.Reverse(sequenceId);
			}

			Array.Copy(timestamp, decoded, 8);
			Array.Copy(instance, 0, decoded, 7, 9);
			Array.Copy(sequenceId, 0, decoded, 16, 4);

			str = prefix + Base62.Encode(decoded).PadLeft(Constants.EncodedLength);

			return str;
		}

		/// <summary>
		/// Parses a ksuid.
		/// </summary>
		/// <param name="id">The string representation of the ksuid.</param>
		public static Id Parse(string input)
		{
			var parts = splitPrefix(input);
			var decoded = Base62.Decode(parts.payload);
			var decodedTrimmed = new byte[Constants.DecodedLength];
			Array.Copy(decoded, decoded.Length - Constants.DecodedLength, decodedTrimmed, 0, Constants.DecodedLength);

			var timestamp = new byte[8];
			var instance = new byte[9];
			var sequence = new byte[4];

			Array.Copy(decodedTrimmed, timestamp, 8);
			Array.Copy(decodedTrimmed, 7, instance, 0, 9);
			Array.Copy(decodedTrimmed, 16, sequence, 0, 4);

			if (BitConverter.IsLittleEndian)
			{
				Array.Reverse(timestamp);
				Array.Reverse(sequence);
			}

			return new Id(
				parts.environment,
				parts.resource,
				BitConverter.ToUInt64(timestamp, 0),
				new InstanceIdentifier(null),
				BitConverter.ToUInt32(sequence, 0)
			);
		}

		private static (string environment, string resource, string payload) splitPrefix(string input)
		{
			var match = Constants.KsuidRegex.Match(input);
			if (!match.Success || match.Groups.Count != 4)
				throw new FormatException("Ksuid format is invalid");

			return (
				match.Groups[1].Value ?? "prod",
				match.Groups[2].Value,
				match.Groups[3].Value
			);
		}
	}
}
