using System;
using System.Diagnostics;
using System.Linq;
using System.Net.NetworkInformation;
using System.Security.Cryptography;
using System.Text.RegularExpressions;

namespace Ksuid
{
	public class Node
	{
		/// <summary>
		/// Creates a new Node for generating Ksuid's.
		/// </summary>
		/// <param name="environment">The environment of the Ksuid.</param>
		/// <param name="instanceIdentifier">The instance identifier of the Ksuid.</param>
		public Node(string environment = "prod", InstanceIdentifier instanceIdentifier = null)
		{
			if (instanceIdentifier == null)
				instanceIdentifier = GetInstanceIdentifier();

			Environment = environment;
			InstanceIdentifier = instanceIdentifier;
		}

		/// <summary>
		/// Gets or sets the environment of the ksuid.
		/// </summary>
		public string Environment
		{
			get { return environment; }
			set {
				Validation.ValidatePrefix(nameof(value), value);

				this.environment = value;
			}
		}
		private string environment;

		/// <summary>
		/// Gets or sets the instance identifier of the ksuid.
		/// </summary>
		public InstanceIdentifier InstanceIdentifier
		{
			get { return instanceIdentifier; }
			set {
				if (value == null)
					throw new ArgumentNullException(nameof(value), $"{nameof(value)} cannot be null");

				instanceIdentifier = value;
			}
		}
		private InstanceIdentifier instanceIdentifier;

		/// <summary>
		/// The current sequence.
		/// </summary>
		private UInt32 currentSequence = 0;

		/// <summary>
		/// The last time a ksuid was generated.
		/// </summary>
		private UInt64 lastTimestamp = 0;

		/// <summary>
		/// Generates a new Ksuid.
		/// </summary>
		/// <param name="resource">The resource type of the ksuid.</param>
		public Id Generate(string resource)
		{
			if (resource == null)
				throw new ArgumentNullException(nameof(resource), $"{nameof(resource)} cannot be null.");

			if (resource == "")
				throw new ArgumentException(nameof(resource), $"{nameof(resource)} cannot be empty.");

			var now = DateTime.UtcNow.ToUnixTime();

			if (lastTimestamp == now)
				currentSequence += 1;
			else
			{
				lastTimestamp = now;
				currentSequence = 0;
			}

			return new Id(
				Environment,
				resource,
				lastTimestamp,
				InstanceIdentifier,
				currentSequence
			);
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
	}
}
