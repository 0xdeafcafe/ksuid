using System;
using Xunit;

namespace Ksuid.Test
{
	public class TestKsuid
	{
		[Theory]
		[InlineData(
			"testing_000000BPL4RZaImj5irv0RM56z6Ce",
			"prod",
			"testing",
			1523119808,
			InstanceScheme.MacAndPID,
			new byte[] { 0x8c, 0x85, 0x90, 0x1b, 0x18, 0x9c, 0x40, 0xad },
			0
		)]
		[InlineData(
			"client_000000BPG6Lks9tQoAiJYrBRSXPX6",
			"prod",
			"client",
			1522947222,
			InstanceScheme.MacAndPID,
			new byte[] { 0x8c, 0x85, 0x90, 0x5f, 0x44, 0xca, 0x80, 0xd9 },
			0
		)]
		public void ParseWithoutEnvironment(string id, string env, string res, ulong ts, InstanceScheme sc, byte[] ib, uint seq)
		{
			var parsedId = Ksuid.Parse(id);

			Assert.Equal(env, parsedId.Environment);
			Assert.Equal(res, parsedId.Resource);
			Assert.Equal<ulong>(ts, parsedId.Timestamp);
			Assert.Equal(sc, parsedId.InstanceIdentifier.Scheme);
			Assert.Equal(ib, parsedId.InstanceIdentifier.Bytes);
			Assert.Equal<uint>(seq, parsedId.SequenceId);
		}

		[Theory]
		[InlineData(
			"dev_testing_000000BPL4RZaImj5irv0RM56z6Ce",
			"dev",
			"testing",
			1523119808,
			InstanceScheme.MacAndPID,
			new byte[] { 0x8c, 0x85, 0x90, 0x1b, 0x18, 0x9c, 0x40, 0xad },
			0
		)]
		[InlineData(
			"dev_client_000000BPG6Lks9tQoAiJYrBRSXPX6",
			"dev",
			"client",
			1522947222,
			InstanceScheme.MacAndPID,
			new byte[] { 0x8c, 0x85, 0x90, 0x5f, 0x44, 0xca, 0x80, 0xd9 },
			0
		)]
		public void ParseWithEnvironment(string id, string env, string res, ulong ts, InstanceScheme sc, byte[] ib, uint seq)
		{
			var parsedId = Ksuid.Parse(id);

			Assert.Equal(env, parsedId.Environment);
			Assert.Equal(res, parsedId.Resource);
			Assert.Equal<ulong>(ts, parsedId.Timestamp);
			Assert.Equal(sc, parsedId.InstanceIdentifier.Scheme);
			Assert.Equal(ib, parsedId.InstanceIdentifier.Bytes);
			Assert.Equal<uint>(seq, parsedId.SequenceId);
		}

		[Fact]
		public void ParseWithProdEnvironment()
		{
			Assert.Throws<FormatException>(() => Ksuid.Parse("prod_testing_000000BPL4RZaImj5irv0RM56z6Ce"));
		}

		[Theory]
		[InlineData("")]
		[InlineData("_")]
		[InlineData("!resource_")]
		[InlineData("ReSouRCe)")]
		public void ParseWithInvalidResource(string resource)
		{
			Assert.Throws<FormatException>(() => Ksuid.Parse($"{resource}000000BPL4RZaImj5irv0RM56z6Ce"));
		}

		[Fact]
		public void ParseNull()
		{
			Assert.Throws<ArgumentNullException>(() => Ksuid.Parse(null));
		}

		[Fact]
		public void ParseInvalidPayload()
		{
			Assert.Throws<FormatException>(() => Ksuid.Parse("test_000000BPG296UCnyv841TMQvmOhq!"));
		}

		[Theory]
		[InlineData("test_000000BPG296UCnyv841TMQvmOhqSP")]
		[InlineData("test_000000BPG296UCnyv841TMQvmOhq")]
		public void ParseInvalidLength(string id)
		{
			Assert.Throws<FormatException>(() => Ksuid.Parse(id));
		}
	}
}
