using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
                if (value < 0 || value > 100)
                {
                    throw new ArgumentOutOfRangeException(nameof(Position), ((App)Application.Current).AppResourceLoader.GetString("ValueNotPercentage"));
                }
                _position = value;
            }
        }

        public override int GetHashCode()
        {
            return Convert.ToInt32(_position) ^ ARGBValue.GetHashCode(); 
        }

        public static bool operator ==(ColorKey left, ColorKey right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(ColorKey left, ColorKey right)
        {
            return !(left == right);
        }

        public bool Equals(ColorKey other)
        {
            return (other.Position == _position && other.ARGBValue == ARGBValue);
        }

        public override bool Equals(object obj)
        {
            return obj is ColorKey && Equals((ColorKey)obj);
        }
    }

    public class ColorCollection : IList<ColorKey>
    {
        public int ColorsCount { get; set; }
        public List<Color> Colors { get; }



    }
}