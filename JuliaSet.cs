using CatsControls;

namespace Catsfract
{
    /// <summary>
    /// Implement the points set worker for a Julia set using the specific seed (real, imaginary)
    /// </summary>
    class JuliaSet : PointsSet, IPointsSet
    {
        // Backing store for the seed as independant double to optimize calculation
        private double _sa, _sb;

        /// <summary>
        /// Create the Julia set, with the seed and with min and max threshold for calculation
        /// </summary>
        /// <param name="minThreshold">Minimum value of Threshold</param>
        /// <param name="maxThreshold">Maximum value of Threshold</param>
        /// <param name="seed">The complex seed used for the Julia set</param>
        public JuliaSet(int minThreshold, int maxThreshold, (double, double) seed) : base(minThreshold, maxThreshold)
        {
            //Store the seed as independant double to optimize calculation
            (_sa, _sb) = seed;
        }

        /// <summary>
        /// The complex seed used for the Julia set. Must be provided as a tuple of double to optimize calculation.  
        /// </summary>
        public (double, double) Seed
        {
            get => (_sa, _sb);
            set
            {
                (_sa, _sb) = value;
            }
        }

        /// <summary>
        /// Worker function to calculte the points value
        /// </summary>
        /// <param name="ca">Real part of the point</param>
        /// <param name="cb">Imaginary part of the point</param>
        /// <returns>Value for the point</returns>
        public int PointSetWorker(double ca, double cb)
        {
            int n = 0;
            double za = ca;
            double zb = cb;
            double zasq, zbsq, magnsq;

            while (n < _threshold)
            {
                zasq = za * za;
                zbsq = zb * zb;

                // Julia set : For each Z in the complex plan, Zn+1 = Zn² + Seed 
                // new za must be calculated after new zb, as new zb is calculated from za
                zb = 2 * za * zb + _sb;
                za = zasq - zbsq + _sa;

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
