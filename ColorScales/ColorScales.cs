using System.Collections.Generic;
using Windows.UI;

namespace Catsfract
{
    public static class ColorScales
    {
        private static readonly Dictionary<string, ColorsCollection> _colorScale = new Dictionary<string, ColorsCollection>
        {
            { 
                "Viridis", new ColorsCollection(100)
                {
                    new ColorKey {Position = 0.0, ARGBValue = Color.FromArgb(0, 68, 2, 85)},
                    new ColorKey {Position = 0.25, ARGBValue = Color.FromArgb(0, 58, 83, 139)},
                    new ColorKey {Position = 0.5, ARGBValue = Color.FromArgb(0, 32, 144, 140)},
                    new ColorKey {Position = 0.75, ARGBValue = Color.FromArgb(0, 91, 200, 98)},
                    new ColorKey {Position = 1.0, ARGBValue = Color.FromArgb(0, 250, 230, 34)}
                }
            }
        };

        public static IReadOnlyDictionary<string, ColorsCollection> ColorScale { get => _colorScale; }
    }
}
