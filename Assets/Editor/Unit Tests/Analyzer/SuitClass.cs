using System;
using System.Collections.Generic;
using System.Linq;

namespace KtaneVideoPoker
{
    using Core;

    namespace Analyzer
    {
        /// <summary>A list of interchangeable suits that can be used to create a `HandClass`.</summary>
        public abstract class SuitClass
        {
            protected static int[] AllRanks = new int[] {1, 13, 12, 11, 10, 9, 8, 7, 6, 5, 4, 3, 2};

            protected int[] SuitValues;

            public int Weight { get; protected set; }

            public int this[int i]
            {
                get
                {
                    return SuitValues[i];
                }
            }

            protected abstract HandClass CreateHandClassFromRankChoices(IEnumerable<int> ranks);

            /// <summary>Returns all possible hand classes for this suit class.</summary>
            public IEnumerable<HandClass> GetHandClasses()
            {
                return GetRankChoices().Select(CreateHandClassFromRankChoices);
            }

            /// <summary>The number of distinct suits in any hand represented by this suit class.</summary>
            public int GetNumberOfDistinctSuits()
            {
                return SuitValues.Distinct().Count();
            }

            /// <summary>The number of non-wild cards represented by this suit class.</summary>
            public int GetNumberOfNonWildCards()
            {
                return SuitValues.Length;
            }

            /// <summary>Generates all possible lists of ranks used to generate hands.</summary>
            protected abstract IEnumerable<IEnumerable<int>> GetRankChoices();
        }

        /// <summary>Five non-wild cards, all of different ranks.</summary>
        public class SuitClassABCDE: SuitClass
        {
            public SuitClassABCDE(int weight, params int[] suitValues)
            {
                Weight = weight;
                SuitValues = suitValues;
            }

            protected override HandClass CreateHandClassFromRankChoices(IEnumerable<int> ranks)
            {
                return new HandClass(this, ranks);
            }

            protected override IEnumerable<IEnumerable<int>> GetRankChoices()
            {
                return AllRanks.Combinations(5);
            }
        }

        /// <summary>Five non-wild cards, one pair.</summary>
        public class SuitClassAABCD: SuitClass
        {
            public SuitClassAABCD(int weight, params int[] suitValues)
            {
                Weight = weight;
                SuitValues = suitValues;
            }

            protected override HandClass CreateHandClassFromRankChoices(IEnumerable<int> ranks)
            {
                return new HandClass(this, ranks.Take(1).Concat(ranks));
            }

            protected override IEnumerable<IEnumerable<int>> GetRankChoices()
            {
                return AllRanks.Combinations(1).SelectMany(pairedRank => AllRanks.Except(pairedRank).Combinations(3).Select(pairedRank.Concat));
            }
        }

        /// <summary>Five non-wild cards, two pairs.</summary>
        public class SuitClassAABBC: SuitClass
        {
            public SuitClassAABBC(int weight, params int[] suitValues)
            {
                Weight = weight;
                SuitValues = suitValues;
            }

            protected override HandClass CreateHandClassFromRankChoices(IEnumerable<int> ranks)
            {
                return new HandClass(this, ranks.Take(1).Concat(ranks.Take(2)).Concat(ranks.Skip(1)));
            }

            protected override IEnumerable<IEnumerable<int>> GetRankChoices()
            {
                return AllRanks.Combinations(2).SelectMany(pairedRanks => AllRanks.Except(pairedRanks).Select(kicker => pairedRanks.Concat(new[] {kicker})));
            }
        }

        /// <summary>Five non-wild cards, three of a kind.</summary>
        public class SuitClassAAABC: SuitClass
        {
            public SuitClassAAABC(int weight, params int[] suitValues)
            {
                Weight = weight;
                SuitValues = suitValues;
            }

            protected override HandClass CreateHandClassFromRankChoices(IEnumerable<int> ranks)
            {
                return new HandClass(this, Enumerable.Repeat(ranks.First(), 2).Concat(ranks));
            }

            protected override IEnumerable<IEnumerable<int>> GetRankChoices()
            {
                return AllRanks.Combinations(1).SelectMany(tripsRank => AllRanks.Except(tripsRank).Combinations(2).Select(tripsRank.Concat));
            }
        }

        /// <summary>Five non-wild cards, full house.</summary>
        public class SuitClassAAABB: SuitClass
        {
            public SuitClassAAABB(int weight, params int[] suitValues)
            {
                Weight = weight;
                SuitValues = suitValues;
            }

            protected override HandClass CreateHandClassFromRankChoices(IEnumerable<int> ranks)
            {
                var array = ranks.ToArray();
                return new HandClass(this, new[] {array[0], array[0], array[0], array[1], array[1]});
            }

            protected override IEnumerable<IEnumerable<int>> GetRankChoices()
            {
                return AllRanks.Combinations(1).SelectMany(tripsRank => AllRanks.Except(tripsRank).Combinations(1).Select(tripsRank.Concat));
            }
        }

        /// <summary>Five non-wild cards, four of a kind.</summary>
        public class SuitClassAAAAB: SuitClass
        {
            public SuitClassAAAAB(int weight, params int[] suitValues)
            {
                Weight = weight;
                SuitValues = suitValues;
            }

            protected override HandClass CreateHandClassFromRankChoices(IEnumerable<int> ranks)
            {
                return new HandClass(this, Enumerable.Repeat(ranks.First(), 3).Concat(ranks));
            }

            protected override IEnumerable<IEnumerable<int>> GetRankChoices()
            {
                return AllRanks.Combinations(1).SelectMany(quadsRank => AllRanks.Except(quadsRank).Combinations(1).Select(quadsRank.Concat));
            }
        }
    }
}