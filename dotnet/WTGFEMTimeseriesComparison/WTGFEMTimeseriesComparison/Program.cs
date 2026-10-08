using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.IO;
using OxyPlot;
using LineSeries = OxyPlot.Series.LineSeries;
using OxyPlot.Wpf;
using OxyPlot.Axes;
using OxyPlot.Legends;



using System;
using System.Linq;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Diagnostics;
using System.Net.NetworkInformation;
using System.Security.Cryptography;
using OxyPlot.Series;

//using System;
using System.Numerics;
using System.Threading.Tasks;
using System.Windows.Media.Animation;
//using System.Diagnostics;



namespace WTGFEMTimeseriesComparison
{
    class Program
    {

        [STAThread]
        static void Main(string[] args)
        {
            /* Define the directory the result files are stored */
            /* NOTE: the data folder is not part of this repository. Define it by running the executable from the
               folder that contains the WTG and FEM sub-folders, or by setting "workingDirectory" in
               Properties/launchSettings.json. */
            /* If want to read the directory from current directory */
            /* This is if we want to copy the executable in the correct folder */
            /* or by setting the working directory from debug properties (or the launchSettings.json file within the project)*/
            string current_dir = Directory.GetCurrentDirectory();
            DirectoryInfo current_dir_info = new DirectoryInfo(current_dir);


            /* Directories where timeseries are stored*/
            string dirWTG = String.Concat(current_dir, @"\WTG");
            string dirFEM = String.Concat(current_dir, @"\FEM");

            /* Explore and find all WTG files */
            string[] filesWTG = Directory.GetFiles(dirWTG, "*WindD*", SearchOption.AllDirectories);
            /* Explore and find all FEM files */
            string[] filesFEM = Directory.GetFiles(dirFEM, "*Interface*", SearchOption.AllDirectories);


            /* Analysis ID as the name of the current working directory */
            string analysis_id = current_dir_info.Name;


            // Set the Loadcase ID map file directory as the parent directory of the batch runs
            string[] loadcaseIDFile = Directory.GetFiles(current_dir, "*Map*", SearchOption.TopDirectoryOnly);

            /* Markov Matrix file name */
            string MMName;
            int MMCounter = 0;
            do
            {
                MMName = $"{analysis_id}_MarkovMatrix_{999:000}_{MMCounter:00}.xlsx";
                MMCounter++;
            } while (File.Exists(string.Concat(current_dir_info.Parent.FullName, @"\", MMName)));


            string[] loadcaseIDContent = File.ReadAllLines(loadcaseIDFile[0]);

            // Create dictionary of Loadcase ID names
            Dictionary<string, string> dictLCIDMap = new Dictionary<string, string>();
            // Create character list with delimiters of the loadcase ID map file lines
            string[] delimitersLCID = new string[] { "," };


            // Read probability txt file line by line below line 1 
            for (int i = 1; i < loadcaseIDContent.Length; i++)
            {
                // From each line read the data as Line properties
                dictLCIDMap.Add(
                    loadcaseIDContent[i].Split(delimitersLCID, StringSplitOptions.None)[0],
                    loadcaseIDContent[i].Split(delimitersLCID, StringSplitOptions.None)[1]
                    );
            }


            /*
            // For each result file append the results list with the individual results
            for (int i = 0; i < files.Length; i++)
            // Iterate counter of progress
            {
                console_counter++;
                if (console_counter % 10 == 0)
                {
                    Console.WriteLine($"Processed {console_counter} of {files.Length}");
                }
                // Store the probability of the current analysis in
                probability_i = prob_dict[files[i].Split(delimiters1, StringSplitOptions.None)[files[i].Split(delimiters1, StringSplitOptions.None).Length - 4]];
                // Only consider the loadcase if the probability is nonzero
                // and create a new result item with the loadcase rainflow results
                if (probability_i > 0) { results[i] = new Result(files[i], probability_i, theta); }
            }
            */



            /* Create dictionaries to contain the WTG and FEM loadcase results*/
            Dictionary<string, Result> wtgLoadsDict = new Dictionary<string, Result>();
            Dictionary<string, Result> femLoadsDict = new Dictionary<string, Result>();

            /* Delimiter array to be used in ... */
            string[] delimiters1 = new string[] { @"\" };

            /* Create counter to keep track of progress of the */
            int console_counter = 0;
            string wtgLCName = null;
            string femLCName = null;
            int plotCounter = 0;



            /* Variable to define the type of plots we want to create*/
            /* "Timehistories" | "Comparison" */
            string plotType = null;
            //plotType = "Timehistories";
            plotType = "Comparison";


            /* Set start and end of the reading loop */
            int start = 0000;   int end = start + 100;
            //start = 500;    end = start + 500;
            //start = 1000;   end = start + 500;
            //start = 1500;   end = start + 500;
            //start = 2000;   end = start + 500;
            //start = 2500;   end = start + 500;
            //start = 3000;   end = start + 500;
            //start = 3500;   end = start + 500;
            //start = 4000;   end = start + 500;
            //start = 4500;   end = start + 500;
            //start = 5000;   end = start + 500;
            //start = 5500;   end = start + 500;
            //start = 6000;   end = start + 500;
            //start = 6500;   end = start + 500;
            //start = 7000;   end = start + 500;
            //start = 7500;   end = start + 500;
            //start = 8000;   end = filesFEM.Length;
            //start = 100; end = start+1;
            //start = 0;   end = filesFEM.Length;

            /* Read the results in parallel mode for potential computation efficiency */
            // Parallel.For(0, filesFEM.Length, i => {


            for (int j = 0; j < filesFEM.Length; j += 500)
            {
            
                start = j;
                end = j + 500;
                if (end > filesFEM.Length) { end = filesFEM.Length; }
            
                wtgLoadsDict = new Dictionary<string, Result>();
                femLoadsDict = new Dictionary<string, Result>();

            //for (int i = j; (i < j+500) && (i < filesFEM.Length); i++)

                for (int i = start; i < end; i++)
                {
                    /* Iterate progress counter */
                    console_counter = i+1;
                    if (console_counter % 10 == 0)
                    {
                        Console.WriteLine($"Processed {console_counter} of {filesWTG.Length} loadcases -> {100 * console_counter / filesWTG.Length:00}%");
                    }
                    /* Store the name of the current loadcases in variables */
                    femLCName = filesFEM[i].Split(delimiters1, StringSplitOptions.None)[filesFEM[i].Split(delimiters1, StringSplitOptions.None).Length - 1];
                    wtgLCName = dictLCIDMap[femLCName];

                    string femFilePath = filesFEM[i];
                    string wtgFilePath = String.Concat(dirWTG, "\\", wtgLCName);

                    /* Store the loadcase loads in the dictionary for WTG and FEM results */
                    femLoadsDict[femLCName] = new Result(femFilePath);
                    wtgLoadsDict[wtgLCName] = new Result(wtgFilePath);
                };

                List<double> femTime = new List<double>();

                List<double> femPX = new List<double>();
                List<double> wtgPX = new List<double>();

                List<double> femPY = new List<double>();
                List<double> wtgPY = new List<double>();

                List<double> femPZ = new List<double>();
                List<double> wtgPZ = new List<double>();

                List<double> femMX = new List<double>();
                List<double> wtgMX = new List<double>();

                List<double> femMY = new List<double>();
                List<double> wtgMY = new List<double>();

                List<double> femMZ = new List<double>();
                List<double> wtgMZ = new List<double>();

                int loadExponent = 5;
                int pxExponent = 5;
                int pyExponent = 5;
                int pzExponent = 7;
                int mxExponent = 7;
                int myExponent = 7;
                int mzExponent = 6;
                


                foreach (KeyValuePair<string, Result> entry in femLoadsDict)
                {

                    if (plotType == "Timehistories")
                    {
                        femTime = new List<double>();

                        femPX = new List<double>();
                        wtgPX = new List<double>();

                        femPY = new List<double>();
                        wtgPY = new List<double>();

                        femPZ = new List<double>();
                        wtgPZ = new List<double>();

                        femMX = new List<double>();
                        wtgMX = new List<double>();

                        femMY = new List<double>();
                        wtgMY = new List<double>();

                        femMZ = new List<double>();
                        wtgMZ = new List<double>();
                    }

                //femTime = entry.Value.FEMResults.Select(x => x.Time).ToList();
                //femPX = entry.Value.FEMResults.Select(x => Math.Pow(-x.PY / Math.Pow(10, pxExponent), loadExponent)).ToList();

                foreach (FEMLine sline in entry.Value.FEMResults)
                    {
                    
                    //femTime.Add(sline.Time);
                    //femPX.Add(-sline.PY);
                    //femPY.Add(sline.PZ);
                    //femPZ.Add(sline.PX);
                    //femMX.Add(-sline.MY);
                    //femMY.Add(sline.MZ);
                    //femMZ.Add(sline.MX);


                    femTime.Add(sline.Time);
                        femPX.Add(Math.Pow(-sline.PY / Math.Pow(10, pxExponent), loadExponent));
                        femPY.Add(Math.Pow( sline.PZ / Math.Pow(10, pyExponent), loadExponent));
                        femPZ.Add(Math.Pow( sline.PX / Math.Pow(10, pzExponent), loadExponent));
                        femMX.Add(Math.Pow(-sline.MY / Math.Pow(10, mxExponent), loadExponent));
                        femMY.Add(Math.Pow( sline.MZ / Math.Pow(10, myExponent), loadExponent));
                        femMZ.Add(Math.Pow( sline.MX / Math.Pow(10, mzExponent), loadExponent));
                    }
                    foreach (ConvertedWTGLine vline in wtgLoadsDict[dictLCIDMap[entry.Key]].ConvertedWTGResults)
                    {
                        //wtgPX.Add(vline.PX);
                        //wtgPY.Add(vline.PY);
                        //wtgPZ.Add(vline.PZ);
                        //wtgMX.Add(vline.MX);
                        //wtgMY.Add(vline.MY);
                        //wtgMZ.Add(vline.MZ);

                        wtgPX.Add(Math.Pow(vline.PX / Math.Pow(10, pxExponent), loadExponent));
                        wtgPY.Add(Math.Pow(vline.PY / Math.Pow(10, pyExponent), loadExponent));
                        wtgPZ.Add(Math.Pow(vline.PZ / Math.Pow(10, pzExponent), loadExponent));
                        wtgMX.Add(Math.Pow(vline.MX / Math.Pow(10, mxExponent), loadExponent));
                        wtgMY.Add(Math.Pow(vline.MY / Math.Pow(10, myExponent), loadExponent));
                        wtgMZ.Add(Math.Pow(vline.MZ / Math.Pow(10, mzExponent), loadExponent));
                    }

                    if (plotType == "Timehistories")
                    {
                        doubleLinePlotter($"PX", femTime.ToArray(), femPX.ToArray(), entry.Key, femTime.ToArray(), wtgPX.ToArray(), dictLCIDMap[entry.Key], String.Concat(current_dir, @"\Timehistories02\", $"{analysis_id}_{plotCounter:00000}_11_PXTimeHist.png"));
                        doubleLinePlotter($"PY", femTime.ToArray(), femPY.ToArray(), entry.Key, femTime.ToArray(), wtgPY.ToArray(), dictLCIDMap[entry.Key], String.Concat(current_dir, @"\Timehistories02\", $"{analysis_id}_{plotCounter:00000}_12_PYTimeHist.png"));
                        doubleLinePlotter($"PZ", femTime.ToArray(), femPZ.ToArray(), entry.Key, femTime.ToArray(), wtgPZ.ToArray(), dictLCIDMap[entry.Key], String.Concat(current_dir, @"\Timehistories02\", $"{analysis_id}_{plotCounter:00000}_13_PZTimeHist.png"));
                        doubleLinePlotter($"MX", femTime.ToArray(), femMX.ToArray(), entry.Key, femTime.ToArray(), wtgMX.ToArray(), dictLCIDMap[entry.Key], String.Concat(current_dir, @"\Timehistories02\", $"{analysis_id}_{plotCounter:00000}_14_MXTimeHist.png"));
                        doubleLinePlotter($"MY", femTime.ToArray(), femMY.ToArray(), entry.Key, femTime.ToArray(), wtgMY.ToArray(), dictLCIDMap[entry.Key], String.Concat(current_dir, @"\Timehistories02\", $"{analysis_id}_{plotCounter:00000}_15_MYTimeHist.png"));
                        doubleLinePlotter($"MZ", femTime.ToArray(), femMZ.ToArray(), entry.Key, femTime.ToArray(), wtgMZ.ToArray(), dictLCIDMap[entry.Key], String.Concat(current_dir, @"\Timehistories02\", $"{analysis_id}_{plotCounter:00000}_16_MZTimeHist.png"));
                    }
                }

                if (plotType == "Comparison")
                {
                    string startLCName = filesFEM[start].Split(delimiters1, StringSplitOptions.None)[filesFEM[start].Split(delimiters1, StringSplitOptions.None).Length - 1];
                    string endLCName = filesFEM[end - 1].Split(delimiters1, StringSplitOptions.None)[filesFEM[end - 1].Split(delimiters1, StringSplitOptions.None).Length - 1];
                    

                    scatterPlotter($"PX Plot ({startLCName} to {endLCName})", femPX.ToArray(), wtgPX.ToArray(), String.Concat(current_dir, @"\Comparison04\", $"{analysis_id}_{start / 100:00}_01_PXPlot.png"));
                    scatterPlotter($"PY Plot ({startLCName} to {endLCName})", femPY.ToArray(), wtgPY.ToArray(), String.Concat(current_dir, @"\Comparison04\", $"{analysis_id}_{start / 100:00}_02_PYPlot.png"));
                    //scatterPlotter($"PZ Plot ({startLCName} to {endLCName})", femPZ.ToArray(), wtgPZ.ToArray(), String.Concat(current_dir, @"\Comparison04\", $"{analysis_id}_{start / 100:00}_03_PZPlot.png"));

                    scatterPlotter($"MX Plot ({startLCName} to {endLCName})", femMX.ToArray(), wtgMX.ToArray(), String.Concat(current_dir, @"\Comparison04\", $"{analysis_id}_{start / 100:00}_04_MXPlot.png"));
                    scatterPlotter($"MY Plot ({startLCName} to {endLCName})", femMY.ToArray(), wtgMY.ToArray(), String.Concat(current_dir, @"\Comparison04\", $"{analysis_id}_{start / 100:00}_05_MYPlot.png"));
                    scatterPlotter($"MZ Plot ({startLCName} to {endLCName})", femMZ.ToArray(), wtgMZ.ToArray(), String.Concat(current_dir, @"\Comparison04\", $"{analysis_id}_{start / 100:00}_06_MZPlot.png"));

                    /* Loop to keep track of the plotted graphs*/
                    plotCounter++;
                    if (plotCounter % 10 == 0)
                    {
                        Console.WriteLine($"Plotted {plotCounter} of {filesWTG.Length} loadcases -> {100 * plotCounter / filesWTG.Length:00}%");
                    }

                }

                /* Loop to keep track of the plotted graphs*/
                plotCounter++;
                if (plotCounter % 10 == 0)
                {
                    Console.WriteLine($"Plotted {plotCounter} of {filesWTG.Length} loadcases -> {100 * plotCounter / filesWTG.Length:00}%");
                }
            }
        }
        public static void scatterPlotter(string _title, double[] _x, double[] _y, string _fp)
        {
            var plot = new PlotModel { Title = _title };
            var Series = new ScatterSeries { MarkerType = MarkerType.Circle, MarkerFill = OxyColors.Black };
            for (int i = 0; i < _x.Length; i+=5)
            {
                if ((_x[i]>10 || _x[i] < -10) && (_y[i]< -5 || _y[i] > 5)) { Series.Points.Add(new ScatterPoint(_x[i], _y[i], 2.0)); }
            }
            plot.Series.Add(Series);

            /* Axis definition: Min, Max and labels */
            double max = 1000;
            double min = -1000;
            plot.Axes.Add(new LinearAxis { Position = AxisPosition.Bottom, Minimum = min, Maximum = max, Title = "FEM" });
            plot.Axes.Add(new LinearAxis { Position = AxisPosition.Left, Minimum = min, Maximum = max, Title = "WTG" });

            var line1to1 = new LineSeries();
            line1to1.Points.Add(new DataPoint(min, min));
            line1to1.Points.Add(new DataPoint(max, max));
            plot.Series.Add(line1to1);

            var line1to105 = new LineSeries();
            line1to105.Points.Add(new DataPoint(min, min*1.05));
            line1to105.Points.Add(new DataPoint(max, max*1.05));
            plot.Series.Add(line1to105);

            var line1to095 = new LineSeries();
            line1to095.Points.Add(new DataPoint(min, min * 0.95));
            line1to095.Points.Add(new DataPoint(max, max * 0.95));
            plot.Series.Add(line1to095);

            PngExporter png = new PngExporter();
            png.Width = 1500;
            png.Height = 1500;
            png.ExportToFile(plot, _fp);
        }
        public static void scatterPow5Plotter(string _title, double[] _x, double[] _y, string _fp)
        {
            var plot = new PlotModel { Title = _title };
            var Series = new ScatterSeries { MarkerType = MarkerType.Circle, MarkerFill = OxyColors.Black };
            for (int i = 0; i < _x.Length; i += 5)
            {
                double x5 = Math.Pow(_x[i], 5);
                double y5 = Math.Pow(_y[i], 5);
                Series.Points.Add(new ScatterPoint(x5, y5, 2.0));
            }
            plot.Series.Add(Series);

            /* Axis definition: Min, Max and labels */
            double max = Math.Max(Math.Pow(_x.Max(),5), Math.Pow(_x.Min(), 5));
            plot.Axes.Add(new LinearAxis { Position = AxisPosition.Bottom, Minimum = 0, Maximum = max, Title = "FEM" });
            plot.Axes.Add(new LinearAxis { Position = AxisPosition.Left, Minimum = 0, Maximum = max, Title = "WTG" });

            /* Guide line with slope of 1 */
            var line1to1 = new LineSeries();
            line1to1.Points.Add(new DataPoint(0, 0));
            line1to1.Points.Add(new DataPoint(max, max));
            plot.Series.Add(line1to1);

            /* Guide line with slope of 1.05 (5% difference) */
            var line1to105 = new LineSeries();
            line1to105.Points.Add(new DataPoint(0, 0));
            line1to105.Points.Add(new DataPoint(max, max * 1.05));
            plot.Series.Add(line1to105);

            /* Guide line with slope of 0.95 (5% difference) */
            var line1to095 = new LineSeries();
            line1to095.Points.Add(new DataPoint(0, 0));
            line1to095.Points.Add(new DataPoint(max, max * 0.95));
            plot.Series.Add(line1to095);

            PngExporter png = new PngExporter();
            png.Width = 1500;
            png.Height = 1500;
            png.ExportToFile(plot, _fp);
        }

        public static void linePlotter(string _title, double[] _x, double[] _y, string _fp)
        {
            var plot = new PlotModel { Title = _title };
            var Series = new LineSeries();
            for (int i = 0; i < _x.Length; i++)
            {
                Series.Points.Add(new DataPoint(_x[i], _y[i]));
            }
            plot.Series.Add(Series);

            PngExporter png = new PngExporter();
            png.Width = 1500;
            png.Height = 1500;
            png.ExportToFile(plot, _fp);
        }
        public static void doubleLinePlotter(string _title, double[] _x1, double[] _y1, string _id1, double[] _x2, double[] _y2, string _id2, string _fp)
        {
            var plot = new PlotModel { Title = _title };
            var Series = new LineSeries { Title = _id1 };
            for (int i = 0; i<_x1.Length; i++)
            {
                if ((_x1[i] > 650) && (_x1[i] < 700)) { Series.Points.Add(new DataPoint(_x1[i], _y1[i])); }
            }
            plot.Series.Add(Series);

            Series = new LineSeries { Title = _id2 };
            for (int i = 0; i<_x2.Length; i++)
            {
                if ((_x2[i] > 650) && (_x2[i] < 700)) { Series.Points.Add(new DataPoint(_x2[i], _y2[i])); }
            }
            plot.Series.Add(Series);

            /* Legend definition*/
            plot.Legends.Add(new Legend() { LegendPosition = LegendPosition.RightTop, });

            PngExporter png = new PngExporter();
            png.Width = 1000;
            png.Height = 500;
            png.ExportToFile(plot, _fp);
        }



    }


    class Result
    {
        public List<ConvertedWTGLine> ConvertedWTGResults;
        public List<OriginalWTGLine> OriginalWTGResults;
        public List<FEMLine> FEMResults;
        public Result(string filePath)
        {

            string[] content = File.ReadAllLines(filePath);

            /* Identify whether the class is used to return WTG or FEM loads */
            if (filePath.Contains("WindD"))
            {
                /* Create list of Line objects */
                List<ConvertedWTGLine> lines = new List<ConvertedWTGLine>();
                /* Read file line by line below line 250 to truncate the first 10 seconds of the analysis  */
                /* Read below line 16250 to truncate 650seconds of the analysis */
                for (int i = 250; i < content.Length; i++)
                {
                    /* From each line read the data as Line properties */
                    ConvertedWTGLine l = new ConvertedWTGLine(content[i]);
                    /* Append lines list with the current line */
                    /* only if the timestep is compatible with FEM */
                    if ( (l.Time % 0.1 < 0.0001) || (l.Time % 0.1 > 0.0999) ) { lines.Add(l); }
                }
                /* Store lines results to the class property to be returned to main program */
                ConvertedWTGResults = lines;
            }
            else if (filePath.Contains("Interface"))
            {
                /* Create list of Line objects */
                List<FEMLine> lines = new List<FEMLine>();
                /* Read file line by line below line 177 to truncate the headers and the first 10s of the analysis  */
                /* Read below line 6577 to truncate 650seconds of the analysis */
                for (int i = 177; i < content.Length; i++)
                {
                    /* From each line read the data as Line properties */
                    FEMLine l = new FEMLine(content[i]);
                    /* Append lines list with the current line */
                    /* only if the timestep is compatible with WTG */
                    if ( (l.Time % 0.04 < 0.0001) || (l.Time % 0.04 > 0.0399) ) { lines.Add(l); }
                }
                /* Store lines results to the class property to be returned to main program */
                FEMResults = lines;
            }
        }
    }

    public class OriginalWTGLine
    {
        // Create properties of Line object
        public readonly string Description;
        public readonly double Time;
        public readonly double AX;
        public readonly double AY;
        public readonly double AZ;
        public readonly double FX;
        public readonly double FY;
        public readonly double FZ;
        public readonly double MX;
        public readonly double MY;
        public readonly double MZ;
        public readonly double UX;
        public readonly double UY;
        public readonly double UZ;
        public readonly double PhiX;
        public readonly double PhiY;
        public readonly double PhiZ;
        public readonly double Eta;

        public OriginalWTGLine(string input)
        {

            // Create character list with delimiters of the probability file lines
            string[] wtgDelimiters = new string[] { ",", "#" };

            // Use queue here instead of string array
            // !!! Potential improvement by using both TrimEntries and RemoveEmptyEntries
            Queue<string> queue = new Queue<string>(input.Split(wtgDelimiters, StringSplitOptions.TrimEntries));

            // Dequeuing the first empty entry (This will be redundant if RemoveEmptyEntries option is used)
            queue.Dequeue();

            Description = queue.Dequeue();

            Time = Convert.ToDouble(queue.Dequeue());
            AX = Convert.ToDouble(queue.Dequeue());
            AY = Convert.ToDouble(queue.Dequeue());
            AZ = Convert.ToDouble(queue.Dequeue());
            FX = Convert.ToDouble(queue.Dequeue());
            FY = Convert.ToDouble(queue.Dequeue());
            FZ = Convert.ToDouble(queue.Dequeue());
            MX = Convert.ToDouble(queue.Dequeue());
            MY = Convert.ToDouble(queue.Dequeue());
            MZ = Convert.ToDouble(queue.Dequeue());
            UX = Convert.ToDouble(queue.Dequeue());
            UY = Convert.ToDouble(queue.Dequeue());
            UZ = Convert.ToDouble(queue.Dequeue());
            PhiX = Convert.ToDouble(queue.Dequeue());
            PhiY = Convert.ToDouble(queue.Dequeue());
            PhiZ = Convert.ToDouble(queue.Dequeue());
            Eta = Convert.ToDouble(queue.Dequeue());

        }
    }

    public class ConvertedWTGLine
    {
        // Create properties of Line object
        public readonly double Time;
        public readonly double PX;
        public readonly double PY;
        public readonly double PZ;
        public readonly double MX;
        public readonly double MY;
        public readonly double MZ;

        public ConvertedWTGLine(string input)
        {

            // Create character list with delimiters of the probability file lines
            string[] femDelimiters = new string[] { "\t" };

            // Use queue here instead of string array
            // !!! Potential improvement by using both TrimEntries and RemoveEmptyEntries
            Queue<string> queue = new Queue<string>(input.Split(femDelimiters, StringSplitOptions.TrimEntries));

            // Dequeuing each entry to get the class properties
            Time = Convert.ToDouble(queue.Dequeue());
            PX = Convert.ToDouble(queue.Dequeue());
            PY = Convert.ToDouble(queue.Dequeue());
            PZ = Convert.ToDouble(queue.Dequeue());
            MX = Convert.ToDouble(queue.Dequeue());
            MY = Convert.ToDouble(queue.Dequeue());
            MZ = Convert.ToDouble(queue.Dequeue());

        }
    }

    public class FEMLine
    {
        // Create properties of Line object
        public readonly string Joint    ;
        public readonly double Time     ;
        public readonly string Member   ;

        //public readonly double D;
        public readonly double PX       ;
        public readonly double PY       ;
        public readonly double PZ       ;
        public readonly double MX       ;
        public readonly double MY       ;
        public readonly double MZ       ;

        public FEMLine(string input)
        {

            // Create character list with delimiters of the probability file lines
            string[] femDelimiters = new string[] { " " };

            // Use queue here instead of string array
            // !!! Potential improvement by using both TrimEntries and RemoveEmptyEntries
            Queue<string> queue = new Queue<string>(input.Split(femDelimiters, StringSplitOptions.RemoveEmptyEntries));

            // Dequeuing each entry to get the class properties

            Joint = queue.Dequeue();
            /* Time step of the analysis must be imported here */
            Time = (Convert.ToDouble(queue.Dequeue()) - 1) * 0.1;
            Member = queue.Dequeue();
            //D = Convert.ToDouble(queue.Dequeue());
            PX = Convert.ToDouble(queue.Dequeue());
            PY = Convert.ToDouble(queue.Dequeue());
            PZ = Convert.ToDouble(queue.Dequeue());
            MX = Convert.ToDouble(queue.Dequeue());
            MY = Convert.ToDouble(queue.Dequeue());
            MZ = Convert.ToDouble(queue.Dequeue());

        }
    }


}



