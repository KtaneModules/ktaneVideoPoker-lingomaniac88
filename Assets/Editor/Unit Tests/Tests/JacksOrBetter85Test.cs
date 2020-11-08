namespace KtaneVideoPoker
{
	namespace Test
	{
		public class JacksOrBetter85Test: VariantTest
		{
			public override Core.Strategy GetStrategy()
			{
				return Variants.JacksOrBetter.Strategy85;
			}

			public override string GetStrategyName()
			{
				return "job85";
			}

			public override Variants.IVariant GetVariant()
			{
				return new Variants.JacksOrBetter(8, 5);
			}
		}
	}
}
