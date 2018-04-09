using Moq;
using Ksuid;
using System;
using Xunit;
using System.Net.NetworkInformation;

namespace Ksuid.Test
{
	public class TestNode
	{
		[Fact]
		public void SettingValidSingletonEnvironment()
		{
			var newEnv = "dev";
			Node.Singleton.Environment = newEnv;

			Assert.Equal(newEnv, Node.Singleton.Environment);
		}

		[Fact]
		public void SettingValidNodeEnvironment()
		{
			var newEnv = "dev";
			var node = new Node();
			node.Environment = newEnv;

			Assert.Equal(newEnv, node.Environment);
		}

		[Theory]
		[InlineData("xo_xo")]
		[InlineData("!prod")]
		[InlineData("PrODucTioN")]
		public void SettingInvalidEnvironment(string environment)
		{
			Assert.Throws<ArgumentException>(() => Node.Singleton.Environment = environment);
		}

		[Theory]
		[InlineData("res_ource")]
		[InlineData("!resource")]
		[InlineData("ResOuRCe")]
		public void GenerateWithInvalidResource(string resource)
		{
			Assert.Throws<ArgumentException>(() => Node.Singleton.Generate(resource));
		}

		[Fact]
		public void GenerateNull()
		{
			Assert.Throws<ArgumentNullException>(() => Node.Singleton.Generate(null));
		}
	}
}
