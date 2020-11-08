using System;
using System.Collections.Generic;
using System.Linq;

namespace KtaneVideoPoker
{
    namespace Core
    {
        public class DetailedHandInfo
        {
            private static bool DefaultWildRule(Card card)
            {
                return card.IsJoker;
            }

            /// <summary>Our own local copy of the cards.</summary>
            public readonly Card[] Cards;

            /// <summary>A reference pointing cards back to their positions in the original hand.</summary>
            public readonly Dictionary<Suit, Dictionary<int, int>> CardPositionMatrix;

            /// <summary>The quantity of each rank.</summary>
            public readonly List<int> RankCounts;

            /// <summary>An array of ranks for each suit, guaranteed to be sorted (Aces count as low).</summary>
            public readonly Dictionary<Suit, int[]> RanksPerSuit;

            /// <summary>An array of positions of all wild cards in the hand.</summary>
            public readonly int[] WildPositions;

            /// <summary>The number of jokers in this hand.</summary>
            public int WildCount
            {
                get
                {
                    return WildPositions.Length;
                }
            }

            public DetailedHandInfo(Hand hand, Func<Card, bool> wildRule = null)
            {
                if (wildRule == null)
                {
                    wildRule = DefaultWildRule;
                }

                CardPositionMatrix = new[] {Suit.Clubs, Suit.Diamonds, Suit.Hearts, Suit.Spades}.ToDictionary(suit => suit, suit => new Dictionary<int, int>());
                RankCounts = Enumerable.Repeat(0, 14).ToList();
                
                var rawRanksPerSuit = new[] {Suit.Clubs, Suit.Diamonds, Suit.Hearts, Suit.Spades}.ToDictionary(suit => suit, suit => new List<int>());

                var wildPositions = new List<int>();

                for (int i = 0; i < hand.Cards.Length; i++)
                {
                    var card = hand.Cards[i];
                    if (wildRule(card))
                    {
                        wildPositions.Add(i);
                    }
                    else
                    {
                        CardPositionMatrix[card.Suit][card.Rank] = i;
                        RankCounts[card.Rank]++;
                        rawRanksPerSuit[card.Suit].Add(card.Rank);
                    }
                }

                RanksPerSuit = rawRanksPerSuit.ToDictionary(kvp => kvp.Key, kvp => kvp.Value.OrderBy(n => n).ToArray());

                WildPositions = wildPositions.ToArray();

                // Make new copies of each card
                Cards = hand.Cards.Select(card => Card.CreateWithId(card.Id)).ToArray();
            }
        }
    }
}