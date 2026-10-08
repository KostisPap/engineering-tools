// See https://aka.ms/new-console-template for more information
using System.Dynamic;

namespace PileDrivingFatigue
{
    class Program
    {
        public static void Main()
        {
            int blows1 = 116;
            int blows2 = 282;
            int blows3 = 546;
            int blows4 = 1655;

            /* NOTE: define the path to the folder that contains the stress-history spreadsheets listed below.
               Set the PILE_DRIVING_DATA_DIR environment variable, or leave it unset to use the current working directory.
               The summary file (Summary.dat) is written to the same folder. */
            string dataFolder = Environment.GetEnvironmentVariable("PILE_DRIVING_DATA_DIR") ?? Directory.GetCurrentDirectory();

            string[] fileNames =
            {
                "3.01_Flange Bolt Hole Stresses - Top Surface.xlsx",
                "3.02_Flange Bolt Hole Stresses - Middle Surface.xlsx",
                "3.03_Flange Bolt Hole Stresses - Bottom Surface.xlsx",
                "3.04_Flange Elliptical Fillet Stresses - Position A.xlsx",
                "3.05_Flange Elliptical Fillet Stresses - Position B.xlsx",
                "3.06_Flange Circ Weld - External Weld.xlsx",
                "3.07_Flange Circ Weld - Internal Weld.xlsx",
                "4.01_Internal Pltform Ring-MP Wall Stresses (top of ring).xlsx",
                "4.02_Internal Pltform Ring-MP Wall Stresses (bottom of ring).xlsx",
                "4.03_Internal Pltform Ring-MP Wall Stresses (at rat hole).xlsx",
                "4.04_ Internal Platform Ring - Stiffener Rat Hole Stresses.xlsx",
                "4.05_Internal Platform Ring - MP Wall at stiffener weld.xlsx",
                "4.06_Internal Platform Ring - Ring stresses at top surface.xlsx",
                "4.07_Internal Platform Ring - Ring stresses at bottom surface.xlsx",
                "4.08_Internal Platform Ring - Stiffener to MP Stiffener side.xlsx",
                "4.09_Internal Platform Ring - Stiffener to Ring Platform-Stiffener Side.xlsx",
                "4.10_Internal Platform Ring - Stiffener to Ring Platform-Platform Side.xlsx",
                "5.01_Upper Trunnion-MP External.xlsx",
                "5.02_Upper Trunnion-Trunnion External.xlsx",
                "5.03_Upper Trunnion-MP Internal.xlsx",
                "5.04_Upper Trunnion-Trunnion Internal.xlsx",
                "6.01_Lower Trunnion-MP External.xlsx",
                "6.02_Lower Trunnion-Trunnion External.xlsx",
                "6.03_Lower Trunnion-MP Internal.xlsx",
                "6.04_Lower Trunnion-Trunnion Internal.xlsx",
                "7.01_MP Door Stresses-External Surface.xlsx",
                "7.02_MP Door Stresses-Mid Surface.xlsx",
                "7.03_MP Door Stresses-Internal Surface.xlsx",
                "8.01_Replenishment Hole Stresses-External Surface.xlsx",
                "8.02_Replenishment Hole Stresses-Mid Surface.xlsx",
                "8.03_Replenishment Hole Stresses-Internal Surface.xlsx",
                "9.01_Cable Entry Hole Stresses-External Surface.xlsx",
                "9.02_Cable Entry Hole Stresses-Mid Surface.xlsx",
                "9.03_Cable Entry Hole Stresses-Internal Surface.xlsx"
            };

            string[] filePaths = fileNames.Select(x => Path.Combine(dataFolder, x)).ToArray();

            string[] separators = { "\\", "_" };
            string[] locationNames = fileNames.Select(x => x.Split(separators, StringSplitOptions.None)[^2]).ToArray();

            string sheetName = "Sheet1";

            /* Specify number of hot spots per location*/
            int[] locationNodes = 
            {
                4,
                4,
                4,
                5,
                5,
                3,
                3, 
                3,
                3,
                1,
                3,
                9,
                5,
                4,
                12,
                12,
                9,
                8,
                8,
                8,
                8,
                8,
                8,
                8,
                8,
                12,
                12,
                12,
                8,
                8,
                8,
                8,
                8,
                8
            };


            /* Specify plate thicknesses */
            double[] locationTk =
            {
                220 ,
                220 ,
                220 ,
                120 ,
                120 ,
                120 ,
                120 ,
                104 ,
                104 ,
                104 ,
                20  ,
                104 ,
                25  ,
                25  ,
                20  ,
                20  ,
                25  ,
                108 ,
                25  ,
                108 ,
                25  ,
                76  ,
                25  ,
                76  ,
                25  ,
                139 ,
                139 ,
                139 ,
                108 ,
                108 ,
                108 ,
                92  ,
                92  ,
                92
            };

            double DFF = 3;
            /* Specify list of SN curves for each location hotspot */
            List<SnCurve> locationSnCurves = new List<SnCurve>();
            /* Flange */
            SnCurve tmpSnCurve = new SnCurve(SNCurve.B2_AIR);   locationSnCurves.Add(tmpSnCurve);
            tmpSnCurve = new SnCurve(SNCurve.B2_AIR); locationSnCurves.Add(tmpSnCurve);
            tmpSnCurve = new SnCurve(SNCurve.B2_AIR); locationSnCurves.Add(tmpSnCurve);
            tmpSnCurve = new SnCurve(SNCurve.B2_AIR); locationSnCurves.Add(tmpSnCurve);
            tmpSnCurve = new SnCurve(SNCurve.B2_AIR); locationSnCurves.Add(tmpSnCurve);
            tmpSnCurve = new SnCurve(SNCurve.D_AIR); locationSnCurves.Add(tmpSnCurve);
            tmpSnCurve = new SnCurve(SNCurve.D_AIR); locationSnCurves.Add(tmpSnCurve);
            /* IPlatform */
            tmpSnCurve = new SnCurve(SNCurve.D_AIR); locationSnCurves.Add(tmpSnCurve);
            tmpSnCurve = new SnCurve(SNCurve.D_AIR); locationSnCurves.Add(tmpSnCurve);
            tmpSnCurve = new SnCurve(SNCurve.D_AIR); locationSnCurves.Add(tmpSnCurve);
            tmpSnCurve = new SnCurve(SNCurve.C_AIR); locationSnCurves.Add(tmpSnCurve);
            tmpSnCurve = new SnCurve(SNCurve.D_AIR); locationSnCurves.Add(tmpSnCurve);
            tmpSnCurve = new SnCurve(SNCurve.D_AIR); locationSnCurves.Add(tmpSnCurve);
            tmpSnCurve = new SnCurve(SNCurve.D_AIR); locationSnCurves.Add(tmpSnCurve);
            tmpSnCurve = new SnCurve(SNCurve.D_AIR); locationSnCurves.Add(tmpSnCurve);
            tmpSnCurve = new SnCurve(SNCurve.D_AIR); locationSnCurves.Add(tmpSnCurve);
            tmpSnCurve = new SnCurve(SNCurve.D_AIR); locationSnCurves.Add(tmpSnCurve);
            /* Upper Trunnion */
            tmpSnCurve = new SnCurve(SNCurve.D_AIR); locationSnCurves.Add(tmpSnCurve);
            tmpSnCurve = new SnCurve(SNCurve.D_AIR); locationSnCurves.Add(tmpSnCurve);
            tmpSnCurve = new SnCurve(SNCurve.D_AIR); locationSnCurves.Add(tmpSnCurve);
            tmpSnCurve = new SnCurve(SNCurve.D_AIR); locationSnCurves.Add(tmpSnCurve);
            /* Lower Trunnion */
            tmpSnCurve = new SnCurve(SNCurve.D_AIR); locationSnCurves.Add(tmpSnCurve);
            tmpSnCurve = new SnCurve(SNCurve.D_AIR); locationSnCurves.Add(tmpSnCurve);
            tmpSnCurve = new SnCurve(SNCurve.D_AIR); locationSnCurves.Add(tmpSnCurve);
            tmpSnCurve = new SnCurve(SNCurve.D_AIR); locationSnCurves.Add(tmpSnCurve);
            /* MP Door */
            tmpSnCurve = new SnCurve(SNCurve.C_AIR); locationSnCurves.Add(tmpSnCurve);
            tmpSnCurve = new SnCurve(SNCurve.C_AIR); locationSnCurves.Add(tmpSnCurve);
            tmpSnCurve = new SnCurve(SNCurve.C_AIR); locationSnCurves.Add(tmpSnCurve);
            /* Replenishment Hole */
            tmpSnCurve = new SnCurve(SNCurve.C_AIR); locationSnCurves.Add(tmpSnCurve);
            tmpSnCurve = new SnCurve(SNCurve.C_AIR); locationSnCurves.Add(tmpSnCurve);
            tmpSnCurve = new SnCurve(SNCurve.C_AIR); locationSnCurves.Add(tmpSnCurve);
            /* Cable Entry */
            tmpSnCurve = new SnCurve(SNCurve.C_AIR); locationSnCurves.Add(tmpSnCurve);
            tmpSnCurve = new SnCurve(SNCurve.C_AIR); locationSnCurves.Add(tmpSnCurve);
            tmpSnCurve = new SnCurve(SNCurve.C_AIR); locationSnCurves.Add(tmpSnCurve);

            /* Lists to store location time histories */
            List<List<PileDrivingData>> locationStressHistories = new List<List<PileDrivingData>>();

            for (int i = 0; i < filePaths.Length; i++)
            {
                ExcelReader excelReader = new ExcelReader();

                List<PileDrivingData> positionStressHistories = excelReader.GetLocationTimeHistories(filePaths[i], sheetName, locationNodes[i]);
                locationStressHistories.Add(positionStressHistories);
            }

            /* Lists to store location damages */
            List<List<List<double>>> allDamages = new List<List<List<double>>>();
            /* Lists to store location damages */
            List<List<List<NodeRainflow>>> allRainflows = new List<List<List<NodeRainflow>>>();



            Rainflow rainflow = new Rainflow();

            /// Iterate for the different spreadsheets
            for (int i = 0; i < filePaths.Length; i++)
            {
                /* Lists to store location damages */
                List<List<double>> locationDamage = new List<List<double>>();
                /* Lists to store location rainflows */
                List<List<NodeRainflow>> locationRainflows = new List<List<NodeRainflow>>();


                /// Iterate the number of nodes within each sheet
                for (int j = 0; j < locationNodes[i]; j++)
                {
                    List<NodeRainflow> positionRainflows = new List<NodeRainflow>();
                    List<double> nodeDamage = new List<double>();


                    NodeRainflow nodeRainflow = new NodeRainflow();

                    /// Rainflow count the four bands of stress histograms
                    nodeRainflow.RainflowBand01 = rainflow.GetResults(locationStressHistories[i][j].StressBand01);
                    nodeRainflow.RainflowBand02 = rainflow.GetResults(locationStressHistories[i][j].StressBand02);
                    nodeRainflow.RainflowBand03 = rainflow.GetResults(locationStressHistories[i][j].StressBand03);
                    nodeRainflow.RainflowBand04 = rainflow.GetResults(locationStressHistories[i][j].StressBand04);


                    /// Initiate band damage
                    double damage = 0;
                    /// Accumulate damage for 1st band
                    foreach (var tmpDP in nodeRainflow.RainflowBand01)
                    { damage += rainflow.RetDamage(tmpDP.Magnitude, tmpDP.Count, locationTk[i], locationSnCurves[i]); }
                    /// Add Band 1 damage to list
                    nodeDamage.Add(damage);

                    /// Initiate band damage
                    damage = 0;
                    /// Accumulate damage for 2nd band
                    foreach (var tmpDP in nodeRainflow.RainflowBand02)
                    { damage += rainflow.RetDamage(tmpDP.Magnitude, tmpDP.Count, locationTk[i], locationSnCurves[i]); }
                    nodeDamage.Add(damage);

                    /// Initiate band damage
                    damage = 0;
                    /// Accumulate damage for 3rd band
                    foreach (var tmpDP in nodeRainflow.RainflowBand03)
                    { damage += rainflow.RetDamage(tmpDP.Magnitude, tmpDP.Count, locationTk[i], locationSnCurves[i]); }
                    nodeDamage.Add(damage);

                    /// Initiate band damage
                    damage = 0;
                    /// Accumulate damage for 4th band
                    foreach (var tmpDP in nodeRainflow.RainflowBand04)
                    { damage += rainflow.RetDamage(tmpDP.Magnitude, tmpDP.Count, locationTk[i], locationSnCurves[i]); }
                    nodeDamage.Add(damage);

                    /// Store node rainflows in rainflow list
                    positionRainflows.Add(nodeRainflow);

                    /// Store position damages in damage list
                    locationDamage.Add(nodeDamage);
                    /// Store position rainflows in rainflow list
                    locationRainflows.Add(positionRainflows);

                }
                /// Store all damages in damage list
                allDamages.Add(locationDamage);
                /// Store position rainflows in rainflow list
                allRainflows.Add(locationRainflows);

            }


            /// Print results to console
            string fileHeader = "  Location    NodeNo       Thk  SN-Curve      Dmg1      Dmg2      Dmg3      Dmg4       DFF  TotalDMG\n";


            string fileBody = "";

            for (int i = 0; i < filePaths.Length; i++)
            {
                for (int j = 0; j < locationNodes[i]; j++)
                {
                    fileBody += $"{locationNames[i].PadLeft(10)}{j + 1,10:N0}{locationTk[i],10:N0}{locationSnCurves[i].Curve.ToString().PadLeft(10)}{allDamages[i][j][0],10:E2}{allDamages[i][j][1],10:E2}{allDamages[i][j][2],10:E2}{allDamages[i][j][3],10:E2}{DFF,10:N0}{(allDamages[i][j][0] * blows1 + allDamages[i][j][1] * blows2 + allDamages[i][j][2] * blows3 + allDamages[i][j][3] * blows4) * DFF,10:E2}\n";
                }
            }

            string fileContent = fileHeader + fileBody;

            File.WriteAllText(Path.Combine(dataFolder, "Summary.dat"), fileContent);



            Console.WriteLine("Wait here before exit");
            Console.WriteLine("Wait here before exit");
            Console.WriteLine("Wait here before exit");
            Console.WriteLine("Wait here before exit");
            Console.WriteLine("Wait here before exit");
            Console.WriteLine("Wait here before exit");


        }
    }
}