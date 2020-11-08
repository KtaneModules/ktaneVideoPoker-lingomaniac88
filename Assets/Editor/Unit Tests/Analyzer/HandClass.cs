using System;
using System.Collections.Generic;
using System.Linq;

namespace KtaneVideoPoker
{
    using Core;

    namespace Analyzer
    {
        /// <summary>A collection of cards with generic suits..</summary>
        public class HandClass
        {
            private static IEnumerable<SuitClass>[] AllSuitClassesByJokerCount = new[]
            {
                new SuitClass[]
                {
                    new SuitClassABCDE(4, 0, 0, 0, 0, 0),
                    new SuitClassABCDE(12, 1, 0, 0, 0, 0),
                    new SuitClassABCDE(12, 0, 1, 0, 0, 0),
                    new SuitClassABCDE(12, 0, 0, 1, 0, 0),
                    new SuitClassABCDE(12, 0, 0, 0, 1, 0),
                    new SuitClassABCDE(12, 0, 0, 0, 0, 1),
                    new SuitClassABCDE(12, 1, 1, 0, 0, 0),
                    new SuitClassABCDE(12, 1, 0, 1, 0, 0),
                    new SuitClassABCDE(12, 1, 0, 0, 1, 0),
                    new SuitClassABCDE(12, 1, 0, 0, 0, 1),
                    new SuitClassABCDE(12, 0, 1, 1, 0, 0),
                    new SuitClassABCDE(12, 0, 1, 0, 1, 0),
                    new SuitClassABCDE(12, 0, 1, 0, 0, 1),
                    new SuitClassABCDE(12, 0, 0, 1, 1, 0),
                    new SuitClassABCDE(12, 0, 0, 1, 0, 1),
                    new SuitClassABCDE(12, 0, 0, 0, 1, 1),
                    new SuitClassABCDE(24, 1, 2, 0, 0, 0),
                    new SuitClassABCDE(24, 1, 0, 2, 0, 0),
                    new SuitClassABCDE(24, 1, 0, 0, 2, 0),
                    new SuitClassABCDE(24, 1, 0, 0, 0, 2),
                    new SuitClassABCDE(24, 0, 1, 2, 0, 0),
                    new SuitClassABCDE(24, 0, 1, 0, 2, 0),
                    new SuitClassABCDE(24, 0, 1, 0, 0, 2),
                    new SuitClassABCDE(24, 0, 0, 1, 2, 0),
                    new SuitClassABCDE(24, 0, 0, 1, 0, 2),
                    new SuitClassABCDE(24, 0, 0, 0, 1, 2),
                    new SuitClassABCDE(24, 0, 0, 1, 1, 2),
                    new SuitClassABCDE(24, 0, 1, 0, 1, 2),
                    new SuitClassABCDE(24, 1, 0, 0, 1, 2),
                    new SuitClassABCDE(24, 0, 0, 1, 2, 1),
                    new SuitClassABCDE(24, 0, 1, 0, 2, 1),
                    new SuitClassABCDE(24, 0, 1, 1, 2, 0),
                    new SuitClassABCDE(24, 0, 0, 2, 1, 1),
                    new SuitClassABCDE(24, 0, 1, 2, 0, 1),
                    new SuitClassABCDE(24, 0, 1, 2, 1, 0),
                    new SuitClassABCDE(24, 0, 2, 0, 1, 1),
                    new SuitClassABCDE(24, 0, 2, 1, 0, 1),
                    new SuitClassABCDE(24, 0, 2, 1, 1, 0),
                    new SuitClassABCDE(24, 2, 0, 0, 1, 1),
                    new SuitClassABCDE(24, 2, 0, 1, 0, 1),
                    new SuitClassABCDE(24, 2, 0, 1, 1, 0),
                    new SuitClassABCDE(24, 3, 3, 0, 1, 2),
                    new SuitClassABCDE(24, 3, 0, 3, 1, 2),
                    new SuitClassABCDE(24, 3, 0, 1, 3, 2),
                    new SuitClassABCDE(24, 3, 0, 1, 2, 3),
                    new SuitClassABCDE(24, 0, 3, 3, 1, 2),
                    new SuitClassABCDE(24, 0, 3, 1, 3, 2),
                    new SuitClassABCDE(24, 0, 3, 1, 2, 3),
                    new SuitClassABCDE(24, 0, 1, 3, 3, 2),
                    new SuitClassABCDE(24, 0, 1, 3, 2, 3),
                    new SuitClassABCDE(24, 0, 1, 2, 3, 3),
                    new SuitClassAABCD(12, 0, 1, 0, 0, 0),
                    new SuitClassAABCD(12, 0, 1, 0, 0, 1),
                    new SuitClassAABCD(12, 0, 1, 0, 1, 0),
                    new SuitClassAABCD(12, 0, 1, 1, 0, 0),
                    new SuitClassAABCD(24, 0, 1, 0, 0, 2),
                    new SuitClassAABCD(24, 0, 1, 0, 2, 0),
                    new SuitClassAABCD(24, 0, 1, 2, 0, 0),
                    new SuitClassAABCD(24, 0, 1, 0, 2, 2),
                    new SuitClassAABCD(24, 0, 1, 2, 0, 2),
                    new SuitClassAABCD(24, 0, 1, 2, 2, 0),
                    new SuitClassAABCD(12, 0, 1, 2, 2, 2),
                    new SuitClassAABCD(24, 0, 1, 0, 1, 2),
                    new SuitClassAABCD(24, 0, 1, 0, 2, 1),
                    new SuitClassAABCD(24, 0, 1, 2, 0, 1),
                    new SuitClassAABCD(12, 0, 1, 2, 3, 3),
                    new SuitClassAABCD(12, 0, 1, 3, 2, 3),
                    new SuitClassAABCD(12, 0, 1, 3, 3, 2),
                    new SuitClassAABCD(24, 0, 1, 0, 2, 3),
                    new SuitClassAABCD(24, 0, 1, 2, 0, 3),
                    new SuitClassAABCD(24, 0, 1, 2, 3, 0),
                    new SuitClassAABBC(12, 0, 1, 2, 3, 0),
                    new SuitClassAABBC(12, 0, 1, 2, 3, 2),
                    new SuitClassAABBC(24, 0, 1, 0, 2, 0),
                    new SuitClassAABBC(24, 0, 1, 0, 2, 1),
                    new SuitClassAABBC(24, 0, 1, 0, 2, 2),
                    new SuitClassAABBC(24, 0, 1, 0, 2, 3),
                    new SuitClassAABBC(12, 0, 1, 0, 1, 0),
                    new SuitClassAABBC(12, 0, 1, 0, 1, 2),
                    new SuitClassAAABC(24, 0, 1, 2, 0, 1),
                    new SuitClassAAABC(12, 0, 1, 2, 0, 3),
                    new SuitClassAAABC(12, 0, 1, 2, 3, 0),
                    new SuitClassAAABC(12, 0, 1, 2, 0, 0),
                    new SuitClassAAABC(4, 0, 1, 2, 3, 3),
                    new SuitClassAAABB(12, 0, 1, 2, 0, 1),
                    new SuitClassAAABB(12, 0, 1, 2, 0, 3),
                    new SuitClassAAAAB(4, 0, 1, 2, 3, 0),
                }
            };

            public static IEnumerable<HandClass> AllHandClassesWithMaximumJokerCount(int maxJokerCount)
            {
                return AllSuitClassesByJokerCount.Take(maxJokerCount + 1).SelectMany(x => x).SelectMany(suitClass => suitClass.GetHandClasses());
            }

            public readonly SuitClass SuitClass;
            public readonly IEnumerable<int> Ranks;

            public int JokersInHand
            {
                get
                {
                    return 5 - SuitClass.GetNumberOfNonWildCards();
                }
            }

            public HandClass(SuitClass suitClass, IEnumerable<int> ranks)
            {
                if (suitClass.GetNumberOfNonWildCards() != ranks.Count())
                {
                    throw new ArgumentException(string.Format("suitClass and ranks must have the same length (given lengths: {0}, {1})", suitClass.GetNumberOfNonWildCards(), ranks.Count()));
                }
                SuitClass = suitClass;
                Ranks = ranks;
            }

            public IEnumerable<Hand> GetAllHands(int deckSize) {
                var jokerOptions = Enumerable.Range(0, deckSize - Util.StandardDeckSize).Select(n => new Card(n)).Combinations(JokersInHand);
                var suitChoices = Card.AllSuits.Permutations(SuitClass.GetNumberOfDistinctSuits());
                var nonJokerOptions = suitChoices.Select(suits => {
                    var array = suits.ToArray();
                    // var indices = Util.CombinedId(deckSize, Ranks.Select((rank, i) => new Card(rank, array[SuitClass[i]]).Id).OrderBy(n => n));
                    return Ranks.Select((rank, i) => new Card(rank, array[SuitClass[i]]));
                });

                // Filter out rearrangements of cards
                var combinedIdsToCardLists = new Dictionary<long, IEnumerable<Card>>();
                foreach (var cardList in nonJokerOptions)
                {
                    var combinedId = Util.CombinedId(deckSize, cardList.Select(card => card.Id).OrderBy(n => n));
                    if (!combinedIdsToCardLists.ContainsKey(combinedId))
                    {
                        combinedIdsToCardLists[combinedId] = cardList;
                    }
                }

                nonJokerOptions = combinedIdsToCardLists.Values;

                return jokerOptions.SelectMany(jokers => nonJokerOptions.Select(nonJokers => new Hand(nonJokers.Concat(jokers))));
            }

            public Hand GetSampleHand(int deckSize)
            {
                return GetAllHands(deckSize).First();
            }

            public int GetWeightWithJokerCount(int numJokersInDeck)
            {
                return SuitClass.Weight * (int) Util.Ncr(numJokersInDeck, JokersInHand);
            }
        }
    }
}
