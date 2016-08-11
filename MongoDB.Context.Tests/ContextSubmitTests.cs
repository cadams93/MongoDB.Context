using System;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;

namespace MongoDB.Context.Tests
{
	[TestFixture]
	public class ContextSubmitTests : ContextTestBase
	{
		[Test]
		public void Should_Fail_WhenSubmitCalledAndAlreadySubmitting()
		{
			var testEntities = GetTestEntities();
			using (var ctx = new MockMongoContext(testEntities))
			{
				var continueSubmitTask = Task.Delay(100);
				var submitOneTask = Task.Run(() => ctx.SubmitChanges());
				var submitTwoTask = Task.Run(() =>
				{
					Thread.Sleep(50);
					var exception = Assert.Throws(typeof(Exception), () => ctx.SubmitChanges());
					Assert.That(exception.Message, Is.EqualTo("Already submitting changes"));
				});

				Task.WaitAny(continueSubmitTask, submitOneTask, submitTwoTask);

				// Force the submits to finish, and thus the test
				ctx.WithinSubmitEvent.Set();
			}
		}
	}
}
