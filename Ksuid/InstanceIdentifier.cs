using System;
using System.Diagnostics;
using System.Linq;
using System.Net.NetworkInformation;
using System.Security.Cryptography;
using System.Text;
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

			if (!Enum.IsDefined(typeof(InstanceScheme), (int) bytes[0]))
				throw new ArgumentException($"{nameof(bytes)} has to start with a valid scheme.", nameof(bytes));

			Scheme = (InstanceScheme) bytes[0];
			Bytes = new byte[bytes.Length - 1];
			Array.Copy(bytes, 1, Bytes, 0, bytes.Length - 1);
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
		/// Since the instance identifier is immutable, we only actually convert it to a
		/// friendly string once. This stores that.
		/// </summary>
		private string friendlyStr;

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

		/// <summary>
		///
		/// </summary>
		public string ToFriendlyString()
		{
			if (friendlyStr != null)
				return friendlyStr;

			var sb = new StringBuilder();

			sb.AppendLine($"Instance scheme: {Scheme.ToString()} ({(int) Scheme})");
			sb.AppendLine($"Instance data:");

			switch(Scheme)
			{
				case InstanceScheme.MacAndPID:
					var macAddr = new byte[6];
					var pid = new byte[2];

					Array.Copy(Bytes, 0, macAddr, 0, 6);
					Array.Copy(Bytes, 5, pid, 0, 2);

					if (BitConverter.IsLittleEndian)
						Array.Reverse(pid);

					sb.AppendLine($"  Machine addr: {BitConverter.ToString(macAddr).ToLower()}");
					sb.AppendLine($"  Process id: {BitConverter.ToInt16(pid, 0)}");
					break;

				case InstanceScheme.Random:
				case InstanceScheme.DockerCont:
				default:
					sb.AppendLine($"  Data: {BitConverter.ToString(Bytes).ToLower()}");
					break;
			}

			return friendlyStr = sb.ToString();
		}

		/// <summary>
		/// Generates an instance identifier. Tries to call `GetMacPidInstance`, and
		/// fallsback to `GetRandomInstance`.
		/// </summary>
		public static InstanceIdentifier GetInstanceIdentifier()
		{
			var macPidInstance = GetMacPidInstance();
			if (macPidInstance != null)
				return macPidInstance;

			return GetRandomInstance();
		}

		/// <summary>
		/// Gets an instance identifier of the machines address and process id.
		/// </summary>
		public static InstanceIdentifier GetMacPidInstance()
		{
			var machineAddress = NetworkInterface
				.GetAllNetworkInterfaces()
				.Where(nic => nic.OperationalStatus == OperationalStatus.Up && nic.NetworkInterfaceType != NetworkInterfaceType.Loopback)
				.Select(nic => nic.GetPhysicalAddress().GetAddressBytes())
				.FirstOrDefault();

			if (machineAddress == null)
				return null;

			var pid = Process.GetCurrentProcess().Id % Math.Pow(2, 16);
			var pidBytes = BitConverter.GetBytes(pid);
			var bytes = new byte[8];

			if (BitConverter.IsLittleEndian)
				Array.Reverse(pidBytes);

			Array.Copy(machineAddress, bytes, 6);
			Array.Copy(pidBytes, 0, bytes, 6, 2);

			return new InstanceIdentifier(InstanceScheme.MacAndPID, bytes);
		}

		/// <summary>
		/// Gets an instance identifier of 8 random bytes.
		/// </summary>
		internal static InstanceIdentifier GetRandomInstance()
		{
			using (var rng = new RNGCryptoServiceProvider())
			{
				var randomData = new byte[0x08];
				rng.GetBytes(randomData);
				return new InstanceIdentifier(InstanceScheme.Random, randomData);
			}
		}

		/// <summary>
		/// Gets an instance identifier of the docker container the application is running
		/// inside.
		/// </summary>
		internal static InstanceIdentifier GetDockerInstance()
		{
			throw new NotImplementedException();
		}
	}

	/// <summary>
	/// The different schemas the instance data can be.
	/// </summary>
	public enum InstanceScheme
	{
		Random = 82, // char('R')
		MacAndPID = 72, // char('H')
		DockerCont = 68, // char('D')
	}
}
