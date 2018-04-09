using Xunit;

namespace Ksuid.Test
{
	public class TestBase62
	{
		[Fact]
		public void CharacterSetIsCorrect()
		{
			var characterSet = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz".ToCharArray();

			Assert.Equal(characterSet, Base62.CharacterSet);
		}
	}
}
