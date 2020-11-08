namespace KtaneVideoPoker
{
	namespace Test
	{
		public class NotSoUglyDucksTest: VariantTest
		{
			public override Core.Strategy GetStrategy()
			{
				return Variants.DeucesWild.StrategyNotSoUglyDucks;
			}

			public override string GetStrategyName()
			{
				return "nsud";
			}

			public override Variants.IVariant GetVariant()
			{
				return Variants.DeucesWild.NotSoUglyDucks();
			}
		}
	}
}
