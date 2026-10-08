using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PPD_Check
{
    /// <summary>
    /// Reader class for P-y curve files. After constructing, call <see cref="GetPyDataPoints"/> to retrieve the P-y curves. This class is threadsafe.
    /// </summary>
    public class PyReader
    {
        /// <summary>
        /// The number of output locations/pile configurations. Presently unused as class is configured to read single locations/pile configurations.
        /// </summary>
        private int nData;
        /// <summary>
        /// The number of columns in the file.
        /// </summary>
        private int nCols;
        /// <summary>
        /// The number of rows associated with the location/pile configuration.
        /// </summary>
        private int nRows;
        /// <summary>
        /// The content of the P-y file as a string array.
        /// </summary>
        private string[] content;
        /// <summary>
        /// Construct a PyReader.
        /// </summary>
        /// <param name="filePath">The fully qualified file path to the P-y curve.</param>
        public PyReader(string filePath)
        {
            content = System.IO.File.ReadAllLines(filePath);
            int[] control = content[0].Split(" ", StringSplitOptions.RemoveEmptyEntries).Select(x => int.Parse(x)).ToArray();
            nData = control[0];
            nCols = control[1];
            nRows = control[2];
        }

        /// <summary>
        /// Returns the P-y curves as an array.
        /// </summary>
        /// <returns>Array of <see cref="PyDataPoint"/> consisting of depth, P and y.</returns>
        public PyDataPoint[] GetPyDataPoints()
        {
            PyDataPoint[] PyCurves = new PyDataPoint[nRows];

            string data = content[3];
            double[] y = data.Split(" ", StringSplitOptions.RemoveEmptyEntries).Select(x => double.Parse(x)).ToArray();
            double thickness = new double();

            for (int i = 0; i < nRows; i++)
            {
                double[] row = content[i + 5].Split(" ", StringSplitOptions.RemoveEmptyEntries).Select(x => double.Parse(x)).ToArray();
                double depth = row[0];
                double[] P = row[1..];

                if (i == 0)
                {
                    thickness = Convert.ToDouble(content[6].Split(" ", StringSplitOptions.RemoveEmptyEntries)[0]);
                }

                for (int j = 0; j < P.Length; j++)
                    P[j] *= thickness;
                PyCurves[i] = new PyDataPoint(P, y, depth);
            }

            return PyCurves;
        }
    }

    /// <summary>
    /// Class encapsulating a P-y curve.
    /// </summary>
    public class PyDataPoint
    {
        /// <summary>
        /// The depth, from mudline, at which this datapoint lies.
        /// </summary>
        public double Depth;
        /// <summary>
        /// The displacement component of the P-y curve.
        /// </summary>
        public double[] Y;
        /// <summary>
        /// The force component of the P-y curve.
        /// </summary>
        public double[] P;

        public double UltimateCapacity => P[Array.IndexOf(Y, Y.Max())];

        /// <summary>
        /// Construct a new <see cref="PyDataPoint"/> object.
        /// </summary>
        /// <param name="P">The force component of the P-y curve.</param>
        /// <param name="y">The displacement component of the P-y curve.</param>
        /// <param name="depth">The depth, from mudline, at which this datapoint lies.</param>
        public PyDataPoint(double[] P, double[] y, double depth)
        {
            this.P = P;
            this.Y = y;
            this.Depth = depth;
        }
    }

    /// <summary>
    /// Class performing the PPD Check.
    /// </summary>
    public class PPDChecker
    {
        public bool PassFail => Utilisation < 1;
        /// <summary>
        /// The result of the PPD check for given Moment, Shear and Pile.
        /// </summary>
        public readonly double Utilisation;
        /// <summary>
        /// Shear and moment capacities at the point near the design shear force.
        /// </summary>
        public readonly double MaxMomentMoment;
        public readonly double MaxMomentShear;
        /// <summary>
        /// Shear and moment capacities at the point near the design bending moment.
        /// </summary>
        public readonly double MaxShearShear;
        public readonly double MaxShearMoment;
        /// <summary>
        /// Array of mean ultimate force for each soil layer.
        /// </summary>
        public readonly double[] MeanPointLoads;
        /// <summary>
        /// Array of soil layer centre elevation.
        /// </summary>
        public readonly double[] SoilLayerCentreElevation;
        /// <summary>
        /// Array of shear force capacities for different CoR elevations.
        /// </summary>
        public readonly double[] CapacitiesArrayShear;
        /// <summary>
        /// Array of bending moment capacities about mudline for different CoR elevations.
        /// </summary>
        public readonly double[] CapacitiesArrayMoment;


        /// <summary>
        /// Method that performs the PPD check.
        /// </summary>
        /// <param name="pyData">The P-y curve at the monopile location.</param>
        /// <param name="pileBot">The coordinate, from mudline, at which the bottom of the monopile is.</param>
        /// <param name="moment">The bending moment at the top of the monopile.</param>
        /// <param name="shear">The shear force at the top of the monopile.</param>
        public PPDChecker(PyDataPoint[] pyData, double pileBot, double moment, double shear)
        //public bool GetPPDResult(PyDataPoint[] pyData, double pileBot, double moment, double shear)
        {


            // Length of arrays is the number of soil layers
            MeanPointLoads = new double[pyData.Length - 1];
            SoilLayerCentreElevation = new double[pyData.Length - 1];

            // Evaluation of the mean soil layer capacity and layer centre elevation.
            for (int i = 0; i < pyData.Length - 1; i++)
            {
                MeanPointLoads[i] = (pyData[i].UltimateCapacity + pyData[i + 1].UltimateCapacity) / 2;
                SoilLayerCentreElevation[i] = (pyData[i].Depth + pyData[i + 1].Depth) / 2;
            }

            // Array with shear force capacities for the Centre of Rotation at the middle of the corresponding soil layer.
            CapacitiesArrayShear = new double[pyData.Length];
            // Array with bending moment about mudline capacities for the Centre of Rotation at the middle of the corresponding soil layer.
            CapacitiesArrayMoment = new double[pyData.Length];

            // Variable to store the index of the shear capacity at the point where moment is just below the design moment
            int maxShearIndex =0;
            // Variable to store the index of the moment capacity at the point where shear is just below the design shear
            int maxMomentIndex=0;

            // iterate to evaluate resulting shear and moment arrays for different CoR.
            // CoRs are assumed at the edges of the soil layers.
            for (int j = 0; (j < pyData.Length); j++)
            {
                // Confine calculations within the length of the pile
                if (pyData[j].Depth < pileBot)
                {
                    // iterate for each soil layer
                    for (int i = 0; (i < SoilLayerCentreElevation.Length); i++)
                    {
                        // Confine calculations within the length of the pile
                        if (SoilLayerCentreElevation[i] < pileBot)
                        {
                            // if CoR is below the soil layer
                            if (pyData[j].Depth > SoilLayerCentreElevation[i])
                            {
                                // Soil layer contribution to shear force capacity
                                CapacitiesArrayShear[j] += MeanPointLoads[i];
                                // Soil layer contribution to bending moment capacity
                                CapacitiesArrayMoment[j] -= MeanPointLoads[i] * SoilLayerCentreElevation[i];
                            }
                            else
                            {
                                // Soil layer contribution o=to shear force capacity
                                CapacitiesArrayShear[j] -= MeanPointLoads[i];
                                // Soil layer contribution to bending moment capacity
                                CapacitiesArrayMoment[j] += MeanPointLoads[i] * SoilLayerCentreElevation[i];
                            }
                        }
                    }
                    // Finding the shear capacity for the first failure point with moment capacity just below the design bending moment.
                    if (CapacitiesArrayMoment[j] > moment)
                    {
                        // The index of the shear capacity in the CapacitiesArrayShear.
                        maxShearIndex = j;
                    }
                    // Finding the moment capacity for the first failure point with shear capacity just below the design shear force.
                    if (CapacitiesArrayShear[j] < shear)
                    {
                        // The index of the moment capacity in the CapacitiesArrayMoment.
                        maxMomentIndex = j;
                    }
                }
            }

            if (maxMomentIndex - maxShearIndex == 1)
                maxShearIndex -= 1;

            // Shear capacity at failure point just below the design bending moment.
            MaxShearShear = CapacitiesArrayShear[maxShearIndex + 1];
            // Bending moment at failure point just below the design bending moment.
            MaxShearMoment = CapacitiesArrayMoment[maxShearIndex + 1];

            // Bending moment at failure point just below the design shear force.
            MaxMomentMoment = CapacitiesArrayMoment[maxMomentIndex];
            // Bending moment at failure point just below the design bending moment.
            MaxMomentShear = CapacitiesArrayShear[maxMomentIndex];

            
            // Variables defining the trend line of the failure line.
            double capacityLineConstant = (MaxMomentMoment * MaxShearShear/MaxMomentShear - MaxShearMoment) / (MaxShearShear/MaxMomentShear - 1);
            double capacityLineSlope = (MaxMomentMoment - capacityLineConstant) / MaxMomentShear;

            // Variable defining the trend line of the design point.
            double designPointSlope = moment / shear;

            // Variables to store the coordinates of the intercept between the capacity/failure curve and the projection of the design point.
            double interceptX = (-capacityLineConstant) / (capacityLineSlope - designPointSlope);
            double interceptY = designPointSlope * interceptX;

            // Distance from origin (0,0) to the design point.
            double designPointDistance = Math.Sqrt(shear * shear + moment * moment);
            // Distance from origin (0,0) to the intercept point.
            double interceptDistance = Math.Sqrt(interceptX * interceptX + interceptY * interceptY);
            // Utilisation as the ratio of the design point distance over the intercept point distance from (0,0).
            double utilisation = designPointDistance / interceptDistance;


            this.Utilisation = utilisation;

            // Initiate variable to store the result of the PPD check.
            bool result = new bool();

            // The method returns true if the design is valid/ utilisation below unity.
            if (utilisation < 1)
            {
                result = true;
            }
            // The method returns false if the design is overutilised.
            else
            {
                result = false;
            }

            //return result;

        }


    }
}
