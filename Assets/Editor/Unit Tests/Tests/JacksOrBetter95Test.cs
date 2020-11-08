namespace KtaneVideoPoker
{
	namespace Test
	{
		public class JacksOrBetter95Test: VariantTest
		{
			public override Core.Strategy GetStrategy()
			{
				return Variants.JacksOrBetter.Strategy95;
			}

			public override string GetStrategyName()
			{
				return "job95";
			}

			public override Variants.IVariant GetVariant()
			{
				return new Variants.JacksOrBetter(9, 5);
			}
		}
	}
}
