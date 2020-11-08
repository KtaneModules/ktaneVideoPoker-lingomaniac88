namespace KtaneVideoPoker
{
	namespace Test
	{
		public class JacksOrBetter96Test: VariantTest
		{
			public override Core.Strategy GetStrategy()
			{
				return Variants.JacksOrBetter.Strategy96;
			}

			public override string GetStrategyName()
			{
				return "job96";
			}

			public override Variants.IVariant GetVariant()
			{
				return new Variants.JacksOrBetter(9, 6);
			}
		}
	}
}
