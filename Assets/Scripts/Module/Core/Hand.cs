using System;
using System.Collections.Generic;
using System.Linq;

namespace KtaneVideoPoker
{
    namespace Core
    {
        /// <summary>A five-card poker hand.</summary>
        public class Hand
        {
            public readonly Card[] Cards;

            /// <summary>Constructs a new hand from an `IEnumerable` of `Card`s, ensuring it has length 5.</summary>
            public Hand(IEnumerable<Card> cards)
            {
                if (cards.Count() != 5)
                {
                    throw new ArgumentException("expected enumerable with count 5, instead got count " + cards.Count(), "cards");
                }
                Cards = cards.ToArray();
            }

            /// <summary>Constructs a new hand from five cards.</summary>
            public Hand(Card c1, Card c2, Card c3, Card c4, Card c5)
            {
                Cards = new Card[] {c1, c2, c3, c4, c5};
            }

            /// <summary></summary>
            public int[] GetCardIdSortIndices()
            {
                var indicesOfSortedCards = Enumerable.Range(0, 5).OrderBy(i => Cards[i].Id).ToArray();

                var result = new[] {0, 0, 0, 0, 0};
                for (int i = 0; i < 5; i++)
                {
                    result[indicesOfSortedCards[i]] = i;
                }

                return result;
            }

            public DetailedHandInfo GetDetailedHandInfo()
            {
                return new DetailedHandInfo(this);
            }

            /// <summary>If this hand is straight-like, returns the rank of the highest card in that straight. Otherwise, returns 0.</summary>
            public int GetHighestStraightCard()
            {
                if (GetPairwiseMatchCount() > 0)
                {
                    return 0;
                }

                /*var ranks = Cards.Where(card => !card.IsJoker).Select(card => 1 << card.Rank).Sum();

                if ((ranks & (1 << 13 | 1 << 12 | 1 << 11 | 1 << 10 | 1 << 1)) == ranks)
                {
                    return 1;
                }
                else
                {
                    for (int maxRank = 13; maxRank >= 5; maxRank--)
                    {
                        if ((ranks & (31 << (maxRank - 4))) == ranks)
                        {
                            return maxRank;
                        }
                    }
                }*/

                var ranksPresent = Enumerable.Repeat(0, 15).ToList();
                foreach (var card in Cards.Where(card => !card.IsJoker))
                {
                    ranksPresent[card.Rank] = 1;
                    if (card.Rank == 1)
                    {
                        ranksPresent[14] = 1;
                    }
                }

                int total = GetJokerCount() + ranksPresent.GetRange(10, 5).Sum();

                if (total == 5)
                {
                    return 1;
                }

                for (int maxRank = 13; maxRank >= 5; maxRank--)
                {
                    total += ranksPresent[maxRank - 4] - ranksPresent[maxRank + 1];
                    if (total == 5)
                    {
                        return maxRank;
                    }
                }

                return 0;
            }

            /// <summary>Returns the number of jokers in this hand.</summary>
            public int GetJokerCount()
            {
                return Cards.Count(card => card.IsJoker);
            }

            /// <summary>Returns the number of pairs of cards with the same rank, excluding jokers.</summary>
            public int GetPairwiseMatchCount()
            {
                var nonJokers = Cards.Where(card => !card.IsJoker).ToArray();
                int result = 0;
                for (int i = 0; i < nonJokers.Length; i++)
                {
                    for (int j = i + 1; j < nonJokers.Length; j++)
                    {
                        if (nonJokers[i].Rank == nonJokers[j].Rank)
                        {
                            result++;
                        }
                    }
                }
                return result;
            }

            /// <summary>Returns whether or not this hand is "flush-like," i.e., all non-joker cards are of the same suit.</summary>
            public bool IsFlushLike()
            {
                return Cards.Where(card => !card.IsJoker).Select(card => card.Suit).Distinct().Count() <= 1;
            }

            public override string ToString()
            {
                return string.Format("Hand[{0}]", Cards.Join(","));
            }
        }
    }
}
