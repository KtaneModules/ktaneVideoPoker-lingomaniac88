using System;
using System.Collections;
using UnityEngine;

namespace KtaneVideoPoker
{
    namespace UI
    {
        public class Button
        {
            public const float HOLD_WAIT_TIME = 3f;

            public KMSelectable Selectable;
            private Renderer Block;
            public TextMesh Text;

            public Action<KMSelectable> OnShortPress;
            public Action<KMSelectable> OnLongPress;

            protected VideoPokerScript Owner;

            private Coroutine ActiveCoroutine;

            public Button(KMSelectable selectable, Renderer block, TextMesh text)
            {
                Selectable = selectable;
                Block = block;
                Text = text;

                Owner = Selectable.Parent.GetComponent<VideoPokerScript>();

                Selectable.OnInteract += delegate
                {
                    Selectable.AddInteractionPunch(0.5f);
                    Owner.Audio.PlaySoundAtTransform(Enabled ? "touch" : "badtouch", Owner.transform);
                    if (Enabled)
                    {
                        if (ActiveCoroutine != null)
                        {
                            Owner.StopCoroutine(ActiveCoroutine);
                        }
                        ActiveCoroutine = Owner.StartCoroutine(CheckForLongPress());
                    }
                    return false;
                };
                Selectable.OnInteractEnded += delegate()
                {
                    if (Enabled)
                    {
                        if (ActiveCoroutine != null)
                        {
                            Owner.StopCoroutine(ActiveCoroutine);
                            ActiveCoroutine = null;
                            if (OnShortPress != null)
                            {
                                OnShortPress(Selectable);
                            }
                        }
                    }
                };
            }

            public virtual void Disable()
            {
                Text.gameObject.SetActive(false);
                Block.material = Owner.MaterialInfo.ButtonDisabled;
            }

            public virtual void Enable()
            {
                Text.gameObject.SetActive(true);
                Block.material = Owner.MaterialInfo.ButtonEnabled;
            }

            public bool Enabled
            {
                get
                {
                    return Text.gameObject.activeSelf;
                }
            }

            private IEnumerator CheckForLongPress()
            {
                yield return new WaitForSeconds(HOLD_WAIT_TIME);
                bool stillActive = (ActiveCoroutine != null);
                ActiveCoroutine = null;
                if (stillActive && OnLongPress != null)
                {
                    OnLongPress(Selectable);
                }
            }
        }
    }
}
