using System.Collections.Generic;
using UnityEngine;

namespace OccaSoftware.Rotoscope.Runtime
{
    [CreateAssetMenu(fileName = "RotoscopeData", menuName = "ScriptableObjects/RotoscopeData")]
    public class RotoscopeData : ScriptableObject
    {
        public List<Color> Colors = new List<Color>();

        [HideInInspector]
        public Texture2D Gradient;

        [Min(0)]
        public int StartDistance;

        [Min(0)]
        public int EndDistance = 10;

        private const int minColors = 2;
        private const int maxColors = 10;

        private void Awake()
        {
            CreateGradient();
        }

        private void OnValidate()
        {
            StartDistance = Mathf.Min(StartDistance, EndDistance);
            ValidateGradient();
            CreateGradient();
        }

        public void ValidateAndCreateGradient()
        {
            ValidateGradient();
            CreateGradient();
        }

        private void ValidateGradient()
        {
            while (Colors.Count < minColors)
            {
                Colors.Add(Color.black);
            }

            if (Colors.Count > maxColors)
            {
                Colors = Colors.GetRange(0, 10);
            }
        }

        private void CreateGradient()
        {
            Color[] _colors = new Color[maxColors];
            for (int a = 0; a < Colors.Count; a++)
            {
                _colors[a] = Colors[a];
            }

            Gradient = new Texture2D(maxColors, 1);
            Gradient.SetPixels(_colors);
            Gradient.Apply();
        }
    }
}
