using System;
using UnityEngine;

namespace KtaneVideoPoker
{
    namespace UI
    {
        public class MaterialInfo
        {
            public Material ButtonDisabled;
            public Material ButtonEnabled;

            public Material[] BlackFaces;
            public Material[] RedFaces;

            public MaterialInfo(Material disabled, Material enabled, Material[] blackFaces, Material[] redFaces)
            {
                ButtonDisabled = disabled;
                ButtonEnabled = enabled;
                BlackFaces = blackFaces;
                RedFaces = redFaces;
            }
        }
    }
}
