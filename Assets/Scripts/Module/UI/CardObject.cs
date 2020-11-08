using System;
using System.Collections.Generic;
using UnityEngine;

namespace KtaneVideoPoker
{
    namespace UI
    {
        public class CardObject
        {
            private static String[] RankValues = new[]
            {
                "0", "A", "2", "3", "4", "5", "6", "7", "8", "9", "I0", "J", "Q", "K"
            };
            private static Dictionary<Core.Suit, String> SuitValues = new Dictionary<Core.Suit, String>()
            {
                { Core.Suit.Clubs, "\u2663" },
                { Core.Suit.Diamonds, "\u2666" },
                { Core.Suit.Hearts, "\u2665" },
                { Core.Suit.Spades, "\u2660" }
            };

            public KMSelectable Selectable;
            private GameObject Back;
            public TextMesh RankText;
            public TextMesh SuitText;
            public TextMesh BigSuitText;
            public GameObject FaceQuad;
            public TextMesh HeldText;
            public TextMesh DeucesWildText;

            public CardObject(KMSelectable selectable, GameObject back, TextMesh rank, TextMesh suit, TextMesh bigSuit, TextMesh held, TextMesh wild, GameObject face)
            {
                Selectable = selectable;
                Back = back;
                RankText = rank;
                SuitText = suit;
                BigSuitText = bigSuit;
                HeldText = held;
                DeucesWildText = wild;
                FaceQuad = face;
            }

            public bool Held
            {
                get
                {
                    return HeldText.gameObject.activeSelf;
                }
                set
                {
                    HeldText.gameObject.SetActive(value);
                }
            }

            public VideoPokerScript Owner
            {
                get
                {
                    return Selectable.Parent.GetComponent<VideoPokerScript>();
                }
            }

            private Core.Card _Card;
            private bool IsShowing = false;

            public Core.Card? Card
            {
                get
                {
                    return IsShowing ? new Core.Card?(_Card) : null;
                }
                set
                {
                    if (value == null)
                    {
                        IsShowing = false;
                        RankText.text = SuitText.text = BigSuitText.text = "";
                        FaceQuad.SetActive(false);
                        DeucesWildText.text = "";
                        Back.SetActive(true);
                    }
                    else
                    {
                        IsShowing = true;
                        Back.SetActive(false);
                        _Card = value.Value;
                        if (_Card.IsJoker)
                        {
                            RankText.text = "?";
                            SuitText.text = BigSuitText.text = "";
                            RankText.color = SuitText.color = BigSuitText.color = Color.black;
                            FaceQuad.SetActive(false);
                        }
                        else
                        {
                            RankText.text = RankValues[_Card.Rank];
                            SuitText.text = BigSuitText.text = SuitValues[_Card.Suit];

                            bool isRed = _Card.Suit == Core.Suit.Diamonds || _Card.Suit == Core.Suit.Hearts;

                            RankText.color = SuitText.color = BigSuitText.color = isRed ? new Color(14/15f, 0, 0) : Color.black;

                            // A bit of a hackish way to determine if we're in Deuces Wild mode, but it works
                            if (_Card.Rank == 2 && Owner.VariationText.Text.Equals("DEUCES WILD"))
                            {
                                var hexColor = isRed ? "ee0000" : "000000";
                                // Values are chosen to fade and shrink by about 25% each line
                                DeucesWildText.text = string.Format("<size=120><color=\"#{0}ff\">WILD</color></size>\n<size=90><color=\"#{0}bf\">WILD</color></size>\n<size=67.5><color=\"#{0}8f\">WILD</color></size>\n<size=50.625><color=\"#{0}6c\">WILD</color></size>", hexColor);
                            }
                            else
                            {
                                DeucesWildText.text = "";
                            }

                            Vector3 scale = Vector3.one;
                            switch (_Card.Rank)
                            {
                                case 11:
                                    FaceQuad.SetActive(true);
                                    FaceQuad.GetComponent<Renderer>().material = (isRed ? Owner.MaterialInfo.RedFaces : Owner.MaterialInfo.BlackFaces)[0];
                                    break;
                                case 12:
                                    FaceQuad.SetActive(true);
                                    FaceQuad.GetComponent<Renderer>().material = (isRed ? Owner.MaterialInfo.RedFaces : Owner.MaterialInfo.BlackFaces)[1];
                                    break;
                                case 13:
                                    FaceQuad.SetActive(true);
                                    FaceQuad.GetComponent<Renderer>().material = (isRed ? Owner.MaterialInfo.RedFaces : Owner.MaterialInfo.BlackFaces)[2];
                                    break;
                                case 1:
                                    FaceQuad.SetActive(true);
                                    FaceQuad.GetComponent<Renderer>().material = (isRed ? Owner.MaterialInfo.RedFaces : Owner.MaterialInfo.BlackFaces)[3];
                                    break;
                                default:
                                    FaceQuad.SetActive(false);
                                    scale.x = _Card.Rank == 10 ? 0.85f : 1;
                                    break;
                            }
                            RankText.transform.localScale = scale;
                        }
                    }
                }
            }
        }
    }
}
