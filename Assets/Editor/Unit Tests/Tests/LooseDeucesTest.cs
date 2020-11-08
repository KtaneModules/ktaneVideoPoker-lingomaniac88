namespace KtaneVideoPoker
{
	namespace Test
	{
		public class LooseDeucesTest: VariantTest
		{
			public override Core.Strategy GetStrategy()
			{
				return Variants.DeucesWild.StrategyLooseDeuces;
			}

			public override string GetStrategyName()
			{
				return "ld";
			}

			public override Variants.IVariant GetVariant()
			{
				return Variants.DeucesWild.LooseDeuces();
			}
		}
	}
}
