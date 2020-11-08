using System;
using System.Linq;
using UnityEngine;

namespace KtaneVideoPoker
{
    namespace UI
    {
        public class SpeedButton : Button
        {
            public TextMesh ArrowText;

            private int Speed = 2;

            public SpeedButton(KMSelectable selectable, Renderer block, TextMesh text, TextMesh arrowText) : base(selectable, block, text)
            {
                ArrowText = arrowText;
            }

            public void ChangeSpeed()
            {
                Speed = (Speed + 1) % 4;
                ArrowText.text = String.Format("<color=red>{0}</color>{1}", Enumerable.Repeat(">", Speed).Join(""), Enumerable.Repeat(">", 3 - Speed).Join(""));
            }

            public override void Disable()
            {
                base.Disable();
                ArrowText.gameObject.SetActive(false);
            }

            public override void Enable()
            {
                base.Enable();
                ArrowText.gameObject.SetActive(true);
            }

            public float GetDelay()
            {
                return new[] {0.2f, 0.15f, 0.1f, 0.05f}[Speed];
            }

            public int GetSpeedIndex()
            {
                return Speed;
            }
        }
    }
}
