using System;
using System.Threading;

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
				instanceIdentifier = InstanceIdentifier.GetInstanceIdentifier();

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

		private Mutex mutex = new Mutex();

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

			mutex.WaitOne();

			var now = DateTime.UtcNow.ToUnixTime();

			if (lastTimestamp == now)
				currentSequence += 1;
			else
			{
				lastTimestamp = now;
				currentSequence = 0;
			}

			mutex.ReleaseMutex();

			return new Id(
				Environment,
				resource,
				lastTimestamp,
				InstanceIdentifier,
				currentSequence
			);
		}
	}
}
