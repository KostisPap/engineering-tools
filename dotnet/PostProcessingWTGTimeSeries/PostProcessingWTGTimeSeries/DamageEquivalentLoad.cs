using System;
using System.Collections.Generic;

namespace PostProcessingWTGTimeSeries
{

    class DamageEquivalentLoad
    {
        public double DEL_3;
        public double DEL_4;
        public double DEL_5;

        public double DEL(List<DataPoint> List)
        {

            double m5 = 5;
            double m4 = 4;
            double m3 = 3;

            double Neq = 1 * Math.Pow(10, 7);
            double M = 25;

            /* Analysis duration of 10 minutes (600s) needs to be converted to life of M years */
            double total_life_scalar = M * 365.25 * 24 * 60 * 60 / 600;

            double sumNM_m5 = 0;
            double sumNM_m4 = 0;
            double sumNM_m3 = 0;
            foreach (DataPoint dp in List)
            {
                sumNM_m5 += (dp.Count * total_life_scalar) * Math.Pow(dp.Magnitude, m5);
                sumNM_m4 += (dp.Count * total_life_scalar) * Math.Pow(dp.Magnitude, m4);
                sumNM_m3 += (dp.Count * total_life_scalar) * Math.Pow(dp.Magnitude, m3);
            }

            DEL_3 = Math.Pow(M * sumNM_m5 / Neq, 1 / m5);
            DEL_4 = Math.Pow(M * sumNM_m4 / Neq, 1 / m4);
            DEL_5 = Math.Pow(M * sumNM_m3 / Neq, 1 / m3);

            return 0;

        }
    }
}
