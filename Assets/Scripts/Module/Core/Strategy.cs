using System;
using System.Collections.Generic;
using System.Linq;

namespace KtaneVideoPoker
{
    namespace Core
    {
        using Rule = Func<DetailedHandInfo, RuleResult>;

        public struct RuleResult
        {
            public static RuleResult Pass(params int[] strategies)
            {
                return new RuleResult(strategies, null);
            }

            public static RuleResult Pass(string extraRuleIndex, params int[] strategies)
            {
                return new RuleResult(strategies, extraRuleIndex);
            }

            public static RuleResult Fail(string extraRuleIndex = null)
            {
                return new RuleResult(new int[0], extraRuleIndex);
            }

            public readonly int[] Strategies;
            public readonly string ExtraRuleIndex;

            private RuleResult(int[] strategies, string extraRuleIndex)
            {
                Strategies = strategies;
                ExtraRuleIndex = extraRuleIndex;
            }
        }

        public struct Result
        {
            public readonly int RuleIndex;

            public readonly string[] ExtraRules;

            public readonly int[] Strategies;

            public Result(int ruleIndex, int[] strategies) : this(ruleIndex, null, strategies)
            {
            }

            public Result(int ruleIndex, string[] extraRules, int[] strategies)
            {
                RuleIndex = ruleIndex;
                ExtraRules = extraRules ?? new string[0];
                Strategies = strategies;
            }
        }

        public class Strategy
        {
            public readonly Rule[] Rules;

            public Strategy(params Rule[] rules)
            {
                Rules = rules;
            }

            public virtual Result Evaluate(Hand hand)
            {
                var detailedHandInfo = new DetailedHandInfo(hand);

                var extraRules = new List<string>();

                for (int i = 0; i < Rules.Length; i++)
                {
                    var result = Rules[i](detailedHandInfo);

                    if (result.ExtraRuleIndex != null)
                    {
                        extraRules.Add(result.ExtraRuleIndex);
                    }

                    if (result.Strategies.Any())
                    {
                        return new Result(i, extraRules.ToArray(), result.Strategies);
                    }
                }

                // No rules apply, discard everything
                return new Result(-1, extraRules.ToArray(), new[] {0});
            }
        }

        public class DeucesWildStrategy : Strategy
        {
            private int JokerOffset;

            public DeucesWildStrategy(int jokerOffset, params Rule[] rules) : base(rules)
            {
                JokerOffset = jokerOffset;
            }

            public override Result Evaluate(Hand hand)
            {
                // Slight post-processing: convert each deuce into a joker
                var newHand = new Hand(hand.Cards.Select(card => card.Rank == 2 ? new Card(JokerOffset + (int)card.Suit) : card));
                return base.Evaluate(newHand);
            }
        }
    }
}