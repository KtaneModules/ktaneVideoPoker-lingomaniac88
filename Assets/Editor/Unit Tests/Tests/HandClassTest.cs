using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;

namespace KtaneVideoPoker
{
	namespace Test
	{
		public class HandClassTest
		{
			public IEnumerable<Analyzer.HandClass> AllHandClassesWithMaximumJokerCount(int n)
			{
				return Analyzer.HandClass.AllHandClassesWithMaximumJokerCount(n);
			}

			[Test]
			public void HandClassCount()
			{
				// The Wizard feeds us this number.
				Assert.IsTrue(AllHandClassesWithMaximumJokerCount(0).Count() == 134459);
				Assert.IsTrue(AllHandClassesWithMaximumJokerCount(0).Select(handClass => handClass.GetWeightWithJokerCount(0)).Sum() == Core.Util.Ncr(52, 5));
			}

			[Test]
			public void HandClassWeights()
			{
				Assert.IsTrue(AllHandClassesWithMaximumJokerCount(0).All(handClass => handClass.GetWeightWithJokerCount(0) == handClass.GetAllHands(Core.Util.StandardDeckSize).Count()));
			}
		}
	}
}
