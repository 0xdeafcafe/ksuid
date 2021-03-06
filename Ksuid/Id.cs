using System;
using System.Text;
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
		/// Since the id is immutable, we only actually convert it to a friendly string
		/// once. This stores that.
		/// </summary>
		private string friendlyStr;

		/// <summary>
		/// Checks of the environment of the ksuid.
		/// </summary>
		/// <param name="env">The environment to compare with.</param>
		public bool IsEnvironment(string env)
		{
			return String.Equals(Environment, env);
		}

		/// <summary>
		/// Checks of the resource of the ksuid.
		/// </summary>
		/// <param name="resource">The resource to compare with.</param>
		public bool IsResource(string resource)
		{
			return String.Equals(Resource, resource);
		}

		/// <summary>
		/// Gets the string representation of the ksuid.
		/// </summary>
		public override string ToString()
		{
			if (str != null)
				return str;

			var sb = new StringBuilder();

			sb.Append(Environment == "prod" ? "" : $"{Environment}_");
			sb.Append(Resource);
			sb.Append("_");

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
			Array.Copy(instance, 0, decoded, 8, 9);
			Array.Copy(sequenceId, 0, decoded, 17, 4);

			sb.Append(Base62.Encode(decoded).PadLeft(Constants.EncodedLength, '0'));

			return str = sb.ToString();
		}

		/// <summary>
		/// Compares the environment, resource, and the content of the payload of a kusid.
		/// </summary>
		/// <param name="obj"></param>
		/// <returns></returns>
		public override bool Equals(object obj)
		{
			if (!(obj is Id))
				return false;

			var id = (Id)obj;

			return ToString() == this.ToString();
		}

		/// <summary>
		/// Gets the hash code of the `ToString` result.
		/// </summary>
		public override int GetHashCode()
		{
			return ToString().GetHashCode();
		}

		/// <summary>
		///
		/// </summary>
		/// <returns></returns>
		public string ToFriendlyString()
		{
			if (friendlyStr != null)
				return friendlyStr;

			var sb = new StringBuilder();
			var time = Extensions.UnixEpoch.AddSeconds(Timestamp);

			sb.AppendLine($"Environment: {Environment}");
			sb.AppendLine($"Resource: {Resource}");
			sb.AppendLine($"Timestamp: {time.ToLongDateString()} {time.ToLongTimeString()}");
			sb.AppendLine(InstanceIdentifier.ToFriendlyString());
			sb.AppendLine($"Sequence Id: {SequenceId}");

			return friendlyStr = sb.ToString();
		}
	}
}
