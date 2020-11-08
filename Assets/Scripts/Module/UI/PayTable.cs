using System.Linq;
using UnityEngine;

namespace KtaneVideoPoker
{
    namespace UI
    {
        public class PayTable
        {
            private Renderer Background;
            private TextMesh[] Texts;

            public bool Visible
            {
                get
                {
                    return Background.gameObject.activeSelf;
                }
                set
                {
                    Background.gameObject.SetActive(value);
                    Texts.ToList().ForEach(text => text.gameObject.SetActive(value));
                }
            }

            public PayTable(Renderer background, TextMesh[] texts)
            {
                Background = background;
                Texts = texts;
            }

            public void LoadVariant(VariantInfo variant)
            {
                var handTypes = variant.VariantLowBet.HandTypes().Select(ht => ht.ToFriendlyString().Replace("s", "<size=90>S</size>").Replace("w", "<size=90>W</size>"));
                Texts[0].text = handTypes.Join("\n");
                for (int i = 1; i <= 5; i++)
                {
                    var subvariant = variant.VariantIfUsingMaxBet(i == 5);
                    Texts[i].text = subvariant.HandTypes().Select(ht => subvariant.PayoutForResult(ht) * i).Join("\n");
                }
            }
        }
    }
}
