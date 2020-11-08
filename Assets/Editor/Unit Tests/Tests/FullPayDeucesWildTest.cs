namespace KtaneVideoPoker
{
	namespace Test
	{
		public class FullPayDeucesWildTest: VariantTest
		{
			public override Core.Strategy GetStrategy()
			{
				return Variants.DeucesWild.StrategyFullPayDeucesWild;
			}

			public override string GetStrategyName()
			{
				return "fpdw";
			}

			public override Variants.IVariant GetVariant()
			{
				return Variants.DeucesWild.FullPayDeucesWild();
			}
		}
	}
}
