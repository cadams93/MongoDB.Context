using MongoDB.Context.Lcs;
using NUnit.Framework;

namespace MongoDB.Context.Tests.Lcs
{
	[TestFixture]
	public class StringLcsTests
	{
		[Test]
		public void Should_NoOverlap_WhenDifferentStrings()
		{
			var alg = new LcsAlgorithm<char>();

			var result = alg.GetLcs("ABCD".ToCharArray(), "1234".ToCharArray());
			Assert.AreEqual(0, result.Sequence.Count);
		}

		[Test]
		public void Should_OverlapFour_WhenStringPrefixEqual()
		{
			var alg = new LcsAlgorithm<char>();

			var result = alg.GetLcs("ABCD".ToCharArray(), "ABCD1234".ToCharArray());
			Assert.AreEqual(4, result.Sequence.Count);

			var result2 = alg.GetLcs("ABCD5678".ToCharArray(), "ABCD1234".ToCharArray());
			Assert.AreEqual(4, result2.Sequence.Count);

			var result3 = alg.GetLcs("ABCD5678".ToCharArray(), "ABCD".ToCharArray());
			Assert.AreEqual(4, result3.Sequence.Count);
		}

		[Test]
		public void Should_OverlapFour_WhenStringSuffixEqual()
		{
			var alg = new LcsAlgorithm<char>();

			var result = alg.GetLcs("ABCD".ToCharArray(), "1234ABCD".ToCharArray());
			Assert.AreEqual(4, result.Sequence.Count);

			var result2 = alg.GetLcs("5678ABCD".ToCharArray(), "1234ABCD".ToCharArray());
			Assert.AreEqual(4, result2.Sequence.Count);

			var result3 = alg.GetLcs("5678ABCD".ToCharArray(), "ABCD".ToCharArray());
			Assert.AreEqual(4, result3.Sequence.Count);
		}

		[Test]
		public void Should_OverlapFour_WhenHelloAndHolloCompared()
		{
			var alg = new LcsAlgorithm<char>();

			var result = alg.GetLcs("HELLO".ToCharArray(), "HOLLO".ToCharArray());
			Assert.AreEqual(4, result.Sequence.Count);
		}

		[Test]
		public void Should_NoOverlap_WhenStringCaseIncorrect()
		{
			var alg = new LcsAlgorithm<char>();

			var result = alg.GetLcs("HELLO".ToCharArray(), "hello".ToCharArray());
			Assert.AreEqual(0, result.Sequence.Count);
		}
	}
}
