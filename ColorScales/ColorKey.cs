using System;
using Windows.UI;
using Windows.UI.Xaml;


namespace Catsfract
{
    public struct ColorKey : IEquatable<ColorKey>
    {
        private double _position;

        public Color ARGBValue { get; set; }
        
        public double Position 
        { 
            get => _position; 
            set
            {
                if (value < 0 || value > 1)
                {
                    throw new ArgumentOutOfRangeException(nameof(Position), ((App)Application.Current).AppResourceLoader.GetString("ValueNotPercentage"));
                }
                _position = value;
            }
        }

        public static int Compare(ColorKey x, ColorKey y)
        {
            if (x.Position == y.Position) return 0;
            return (x.Position < y.Position) ? -1 : 1;
        }

        public override int GetHashCode() => Convert.ToInt32(_position) ^ ARGBValue.GetHashCode();
        public static bool operator ==(ColorKey left, ColorKey right) => left.Equals(right);
        public static bool operator !=(ColorKey left, ColorKey right) => !(left == right);
        public bool Equals(ColorKey other) => other.Position == _position && other.ARGBValue == ARGBValue;
        public override bool Equals(object obj) => obj is ColorKey && Equals((ColorKey)obj);
    }  
}