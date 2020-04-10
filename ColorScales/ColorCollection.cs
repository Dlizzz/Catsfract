using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Windows.UI;
using Windows.UI.Xaml;


namespace Catsfract
{
    public class ColorsCollection : IList<ColorKey>
    {
        private readonly List<ColorKey> colorKeys = new List<ColorKey>();
        private readonly List<Color> _colorElements = new List<Color>();

        public ColorsCollection(int colorElementsCount)
        {
            ColorElementsCount = colorElementsCount;
        }

        #region ColorKey
        public ColorKey this[int index]
        {
            get => colorKeys[index];
            set
            {
                colorKeys[index] = value;
                UpdateColorElements();
            }
        }

        public void Add(ColorKey item)
        {
            colorKeys.Add(item);
            UpdateColorElements();
        }

        public void Clear()
        {
            colorKeys.Clear();
            UpdateColorElements();
        }

        public void Insert(int index, ColorKey item)
        {
            colorKeys.Insert(index, item);
            UpdateColorElements();
        }

        public bool Remove(ColorKey item)
        {
            bool removed = colorKeys.Remove(item);
            if (removed) UpdateColorElements();
            return removed;
        }

        public void RemoveAt(int index)
        {
            colorKeys.RemoveAt(index);
            UpdateColorElements();
        }

        public int Count => colorKeys.Count;
        public bool IsReadOnly => false;
        public bool Contains(ColorKey item) => colorKeys.Contains(item);
        public void CopyTo(ColorKey[] array, int arrayIndex) => colorKeys.CopyTo(array, arrayIndex);
        public IEnumerator<ColorKey> GetEnumerator() => colorKeys.GetEnumerator();
        public int IndexOf(ColorKey item) => colorKeys.IndexOf(item);
        IEnumerator IEnumerable.GetEnumerator() => colorKeys.GetEnumerator();
        #endregion

        #region ColorElements
        public int ColorElementsCount 
        { 
            get => _colorElements.Count;
            set
            {
                if (value <= 0) throw new ArgumentOutOfRangeException(nameof(ColorElementsCount), ((App)App.Current).AppResourceLoader.GetString("ValueNotStrictlyPositive"));
                
                _colorElements.Clear();
                Color[] colors = new Color[value];
                _colorElements.AddRange(colors);

                UpdateColorElements();
            }        
        }

        public ReadOnlyCollection<Color> ColorElements { get => _colorElements.AsReadOnly(); }

        public Color ScaleToColor(double scale)
        {
            if (scale < 0 || scale > 1) throw new ArgumentOutOfRangeException(nameof(scale), ((App)Application.Current).AppResourceLoader.GetString("ValueNotPercentage"));

            int index = Convert.ToInt32(scale * (_colorElements.Count - 1));
            return _colorElements[index];
        }

        public Color ScaleToColorInverse(double scale)
        {
            if (scale < 0 || scale > 1) throw new ArgumentOutOfRangeException(nameof(scale), ((App)Application.Current).AppResourceLoader.GetString("ValueNotPercentage"));

            int index = Convert.ToInt32(scale * (_colorElements.Count - 1));
            return _colorElements[_colorElements.Count - 1 - index];
        }

        private void UpdateColorElements()
        {
            if (_colorElements.Count == 0) return;
            if (colorKeys.Count == 0) 
            {
                _colorElements.ForEach((Color color) => color = Colors.Black);
                return;
            }

            for (int index = 0; index < _colorElements.Count; index++)
            {
                _colorElements[index] = ComputeColor((double)index / _colorElements.Count); 
            }
        }

        private Color ComputeColor(double position)
        {
            ColorKey segmentStart = new ColorKey { Position = 0.0, ARGBValue = Colors.Black };
            ColorKey segmentEnd = new ColorKey { Position = 1.0, ARGBValue = Colors.Black };

            foreach (ColorKey colorKey in colorKeys)
            {
                if (position == colorKey.Position) return colorKey.ARGBValue;
                if (position > colorKey.Position & colorKey.Position >= segmentStart.Position) segmentStart = colorKey;
                else if (position < colorKey.Position & colorKey.Position <= segmentEnd.Position) segmentEnd = colorKey;
            }

            double relativePosition = (position - segmentStart.Position) / (segmentEnd.Position - segmentStart.Position);
            Color color;
            color.R = ColorComponent(relativePosition, segmentStart.ARGBValue.R, segmentEnd.ARGBValue.R);
            color.G = ColorComponent(relativePosition, segmentStart.ARGBValue.G, segmentEnd.ARGBValue.G);
            color.B = ColorComponent(relativePosition, segmentStart.ARGBValue.B, segmentEnd.ARGBValue.B);

            return color;
        }

        private static byte ColorComponent(double position, byte start, byte end) => Convert.ToByte(start + (end - start) * position);
        #endregion
    }
}