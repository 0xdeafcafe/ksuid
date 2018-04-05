using System;
using System.Text.RegularExpressions;

namespace Ksuid
{
	public class InstanceIdentifier
	{
		/// <summary>
		/// Creates a new instance identifier, including the scheme in the instance data.
		/// </summary>
		/// <param name="bytes">The instance data, with scheme prepended.</param>
		public InstanceIdentifier(byte[] bytes)
		{
			if (bytes?.Length != 9)
				throw new ArgumentException($"{nameof(bytes)} has to be 9 bytes long.", nameof(bytes));

			Enum.IsDefined(typeof(InstanceScheme), bytes[0]);

			Scheme = (InstanceScheme) bytes[0];
			Bytes = new ArraySegment<byte>(bytes, 1, bytes.Length - 2).Array;
		}

		/// <summary>
		/// Creates a new instance identifier, specifying the scheme.
		/// </summary>
		/// <param name="scheme">The scheme of the instance data.</param>
		/// <param name="bytes">The instance data.</param>
		public InstanceIdentifier(InstanceScheme scheme, byte[] bytes)
		{
			if (bytes?.Length != 8)
				throw new ArgumentException($"{nameof(bytes)} has to be 8 bytes long.", nameof(bytes));

			Scheme = scheme;
			Bytes = bytes;
		}

		/// <summary>
		/// The scheme of the instance data.
		/// </summary>
		public InstanceScheme Scheme { get; private set; }

		/// <summary>
		/// The instance data.
		/// </summary>
		public byte[] Bytes
		{
			get { return this.bytes; }
			private set { this.bytes = value; }
		}
		private byte[] bytes;

		/// <summary>
		/// The merged scheme and bytes fields. This can only happen once.
		/// </summary>
		private byte[] mergedByteArray;

		/// <summary>
		/// Gets the merged scheme and data as a byte array.
		/// </summary>
		public byte[] ToByteArray()
		{
			if (mergedByteArray == null)
			{
				// Only do this operation once
				mergedByteArray = new byte[9] { (byte)Scheme, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };
				Array.Copy(Bytes, 0, mergedByteArray, 1, 8);
			}

			return mergedByteArray;
		}
	}

	/// <summary>
	/// The different schemas the instance data can be.
	/// </summary>
	public enum InstanceScheme
	{
		Random = 82, // R
		MacAndPID = 72, // H
		DockerCont = 82, // D
	}
}
