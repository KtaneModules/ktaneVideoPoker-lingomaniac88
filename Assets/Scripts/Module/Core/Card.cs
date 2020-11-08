using System;

namespace KtaneVideoPoker
{
    namespace Core
    {
        public enum Suit
        {
            Clubs,
            Diamonds,
            Hearts,
            Spades,
            Joker
        }

        /// <summary>A playing card, which can either be a standard card or a joker.</summary>
        public struct Card
        {
            public static Suit[] AllSuits = new[] {Suit.Clubs, Suit.Diamonds, Suit.Hearts, Suit.Spades};

            public static Card CreateWithId(int cardId)
            {
                if (cardId < 0)
                {
                    throw new ArgumentException("Invalid card ID: " + cardId);
                }
                else if (cardId > Core.Util.StandardDeckSize)
                {
                    return new Card(cardId - Core.Util.StandardDeckSize);
                }
                else
                {
                    return new Card((cardId + 1) % 13 + 1, (Suit) (cardId / 13));
                }
            }

            public readonly int Rank;
            public readonly Suit Suit;

            /// <summary>A unique identifier for this card.
            /// IDs between 0 and 51 correspond to 2 through A of the suits in alphabetical order. IDs of 52 and higher correspond to jokers, if present.</summary>
            public int Id
            {
                get
                {
                    if (IsJoker)
                    {
                        return Core.Util.StandardDeckSize + Rank;
                    }
                    else
                    {
                        return 13 * (int) Suit + (Rank + 11) % 13;
                    }
                }
            }

            /// <summary>Returns whether or not this card is a joker.</summary>
            public bool IsJoker
            {
                get
                {
                    return Suit == Suit.Joker;
                }
            }

            /// <summary>Constructs a new standard card.</summary>
            public Card(int rank, Suit suit)
            {
                Rank = rank;
                Suit = suit;
            }

            /// <summary>Constructs a joker.</summary>
            public Card(int jokerId)
            {
                Rank = jokerId;
                Suit = Suit.Joker;
            }

            public override string ToString()
            {
                if (Suit == Suit.Joker)
                {
                    return "?" + Rank;
                }
                else
                {
                    return string.Format("{0}{1}", "A23456789TJQK"[Rank - 1], char.ToLowerInvariant(Suit.ToString()[0]));
                }
            }
        }
    }
}