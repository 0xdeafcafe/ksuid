using Xunit;

namespace Ksuid.Test
{
	public class TestClass
	{
		[Fact]
		public void ValidTest()
		{
			var node = new Node("prison");
			var leydon = node.Generate("leydon");
			var id = Id.Parse(leydon.ToString());
		}
	}
}
