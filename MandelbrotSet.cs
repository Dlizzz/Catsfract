using CatsControls;

namespace Catsfract
{
    /// <summary>
    /// Implement the points set worker for a Mandelbrot set
    /// </summary>
    class MandelbrotSet : PointsSet, IPointsSet 
    {
        /// <summary>
        /// Create the Mandelbrot set
        /// </summary>
        /// <param name="minThreshold">Minimum value of Threshold</param>
        /// <param name="maxThreshold">Maximum value of Threshold</param>
        public MandelbrotSet(int minThreshold, int maxThreshold) : base(minThreshold, maxThreshold) { }

        /// <summary>
        /// Worker function to calculte the points value
        /// </summary>
        /// <param name="ca">Real part of the point</param>
        /// <param name="cb">Imaginary part of the point</param>
        /// <returns>Value for the point</returns>
        public int PointSetWorker(double ca, double cb)
        {
            int n = 0;
            double za = 0;
            double zb = 0;
            double zasq, zbsq, magnsq;

            while (n < _threshold)
            {
                zasq = za * za;
                zbsq = zb * zb;

                // Mandelbrot set : For each C in the complex plan, Zn+1 = Zn² + C, with Z0 = 0
                // new za must be calculated after new zb, as new zb is calculated from za
                zb = 2 * za * zb + cb;
                za = zasq - zbsq + ca;

                // new z magnitude squared
                magnsq = za * za + zb * zb;

                // if new z magitude squared is over 4, then the serie is considered as divergent
                // we get out of the loop and return the number of iteration to be used a value for the point
                if (magnsq > 4) break;

                n++;
            }
            // n from 0 to _threshold (included)
            return n;
        }
    }
}
