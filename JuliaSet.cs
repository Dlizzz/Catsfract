using System.Numerics;
using System.ComponentModel;
using CatsControls.PointsSet;


namespace Catsfract
{
    /// <summary>
    /// Implement the points set worker for a Julia set using the specific seed (real, imaginary)
    /// </summary>
    class JuliaSet : PointsSetWorker, IPointsSetWorker
    {
        /// <summary>
        /// Create the Julia set, with the seed and with min and max threshold for calculation
        /// </summary>
        /// <param name="minThreshold">Minimum value of Threshold</param>
        /// <param name="maxThreshold">Maximum value of Threshold</param>
        public JuliaSet(int minThreshold, int maxThreshold) : base(minThreshold, maxThreshold) 
        {
            var parameter = new PointsSetComplexParameter(new Complex(-2, -2), new Complex(2, 2));
            // Add parameter
            _parameters.Add("Seed", parameter);
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
            double za = ca;
            double zb = cb;
            double zasq, zbsq, magnsq;
            double sa = ((PointsSetComplexParameter)_parameters["Seed"]).RealValue;
            double sb = ((PointsSetComplexParameter)_parameters["Seed"]).ImaginaryValue;

            while (n < _threshold)
            {
                zasq = za * za;
                zbsq = zb * zb;

                // Julia set : For each Z in the complex plan, Zn+1 = Zn² + Seed 
                // new za must be calculated after new zb, as new zb is calculated from za
                zb = 2 * za * zb + sb;
                za = zasq - zbsq + sa;

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
