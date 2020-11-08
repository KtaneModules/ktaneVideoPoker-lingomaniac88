using System;
using UnityEngine;

namespace KtaneVideoPoker
{
    namespace UI
    {
        public class ProgressLightManager
        {
            private Renderer[] Cylinders;
            private Light[] Lights;

            private int Value;

            private Material RedMaterial;
            private Material GreenMaterial;
            private Material OffMaterial;

            public ProgressLightManager(Renderer[] cylinders, Light[] lights, Material red, Material green, Material off)
            {
                Cylinders = cylinders;
                Lights = lights;
                RedMaterial = red;
                GreenMaterial = green;
                OffMaterial = off;
            }

            public void SetValue(int n)
            {
                Value = n;
                for (int i = 0; i < Cylinders.Length; i++)
                {
                    Cylinders[i].material = (i < n) ? GreenMaterial : OffMaterial;
                }
                for (int i = 0; i < Lights.Length; i++)
                {
                    Lights[i].gameObject.SetActive(i < n);
                }
            }

            public void SetAllRed(bool allRed)
            {
                if (allRed)
                {
                    for (int i = 0; i < Cylinders.Length; i++)
                    {
                        Cylinders[i].material = RedMaterial;
                    }
                    for (int i = 0; i < Lights.Length; i++)
                    {
                        Lights[i].gameObject.SetActive(true);
                    }
                }
                else
                {
                    SetValue(Value);
                }
            }
        }
    }
}
