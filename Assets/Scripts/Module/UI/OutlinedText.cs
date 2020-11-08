using UnityEngine;

namespace KtaneVideoPoker
{
    namespace UI
    {
        public class OutlinedText
        {
            public TextMesh Base;
            public TextMesh Outline;

            public bool Visible
            {
                get
                {
                    return Base.gameObject.activeSelf;
                }
                set
                {
                    Base.gameObject.SetActive(value);
                    Outline.gameObject.SetActive(value);
                }
            }

            public string Text
            {
                get
                {
                    return Base.text;
                }
                set
                {
                    Base.text = value;
                    Outline.text = value;
                }
            }

            public OutlinedText(TextMesh baseText, TextMesh outline)
            {
                Base = baseText;
                Outline = outline;
                Outline.text = Base.text;
                Base.GetComponent<Renderer>().sortingOrder = -1;
                Outline.GetComponent<Renderer>().sortingOrder = 0;
            }
        }
    }
}
