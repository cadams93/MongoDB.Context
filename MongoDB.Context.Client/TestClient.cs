using System.Linq;
using System.Threading.Tasks;
using MongoDB.Driver;

namespace MongoDB.Context.Client
{
	public class TestClient
	{
		static void Main(string[] args)
		{
			using (var ctx = new MongoContext(new MongoClient()))
			using (var ctx2 = new MongoContext(new MongoClient()))
			{
				var entity = ctx.TestEntities.Find().First();
				var entityAgain = ctx2.TestEntities.Find().First();

				entity.Enum = EnumTest.Value3;
				entityAgain.Enum = EnumTest.Value2;

				Parallel.Invoke(() => ctx.SubmitChanges(), () => ctx2.SubmitChanges());
			}
		}
	}
}
