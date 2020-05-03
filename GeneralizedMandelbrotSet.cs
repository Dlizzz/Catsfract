using System.Numerics;
using CatsControls.PointsSet;

namespace Catsfract
{
    /// <summary>
    /// Implement the points set worker for a Mandelbrot set with all paremeters
    /// </summary>
    class GeneralizedMandelbrotSet: PointsSetWorker, IPointsSetWorker 
    {
        /// <summary>
        /// Create the Mandelbrot set
        /// </summary>
        /// <param name="minThreshold">Minimum value of Threshold</param>
        /// <param name="maxThreshold">Maximum value of Threshold</param>
        public GeneralizedMandelbrotSet(int minThreshold, int maxThreshold) : base(minThreshold, maxThreshold) 
        {
            // Add parameters
            var parameterRoot = new PointsSetComplexParameter(new Complex(-2, -2), new Complex(2, 2));
            var parameterPower = new PointsSetDoubleParameter(1, 3);
            _parameters.Add("Root", parameterRoot);
            _parameters.Add("Power", parameterPower);
        }

        /// <summary>
        /// Worker function to calculte the points value
        /// </summary>
        /// <param name="ca">Real part of the point</param>
        /// <param name="cb">Imaginary part of the point</param>
        /// <returns>Value for the point</returns>
        public int PointsSetWorker(double ca, double cb)
        {
            int n = 0;
            double power = ((PointsSetDoubleParameter)_parameters["Power"]).Value;
            Complex c = new Complex(ca, cb);
            Complex zn = new Complex(
                ((PointsSetComplexParameter)_parameters["Root"]).RealValue,
                ((PointsSetComplexParameter)_parameters["Root"]).ImaginaryValue);
            Complex zn1;

            while (n < _threshold)
            {
                zn1 = Complex.Pow(zn, power) + c;

                if (zn1.Magnitude > 2) break;

                n++;
            }
            // n from 0 to _threshold (included)
            return n;
        }
    }
}
