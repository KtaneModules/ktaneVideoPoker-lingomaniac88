namespace KtaneVideoPoker
{
    namespace Variants
    {
        public interface IVariant
        {
            /// <summary>The number of jokers used in this variant.</summary>
            int JokerCount { get; }

            /// <summary>Evaluates and returns the hand type for a particular hand.</summary>
            Core.HandResult Evaluate(Core.Hand hand);

            /// <summary>An array of paying HandResults that this game can return.</summary>
            Core.HandResult[] HandTypes();

            /// <summary>Returns the payout for a particular hand type.</summary>
            int PayoutForResult(Core.HandResult result);
        }
    }
}