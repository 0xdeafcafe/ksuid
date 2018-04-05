using System;
using Ksuid;
using Xunit;
using Xunit.Abstractions;

namespace Ksuid.Tests
{
	public class TestyBoii
	{
		private readonly ITestOutputHelper output;

		public TestyBoii(ITestOutputHelper output)
		{
			this.output = output;
		}

		[Fact]
		public void ValidTest()
		{
			var node = new Node();
			var id = node.Generate("user");
			output.WriteLine(id.ToString());
			Id.Parse(id.ToString());
		}
	}
}
