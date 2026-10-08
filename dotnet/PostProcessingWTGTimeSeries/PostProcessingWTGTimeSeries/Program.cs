using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.IO;

namespace PostProcessingWTGTimeSeries
{
    class Program
    {
        static void Main()
        {
            /* Define the directory the result files are stored */
            /* NOTE: the data folder is not part of this repository. Define it by running the executable from the
               folder that holds the result files, or by setting "workingDirectory" in Properties/launchSettings.json. */
            /* If want to read the directory from current directory */
            /* This is if we want to copy the executable in the correct folder */
            /* or by setting the working directory from debug properties (or the launchSettings.json file within the project)*/
            string current_dir = System.IO.Directory.GetCurrentDirectory();
            string directory = String.Concat(current_dir, @"");

            System.IO.DirectoryInfo di = new System.IO.DirectoryInfo(directory);
            /* NOTE: adjust the file-name pattern to match your result files */
            string filePattern = "*";
            string[] files = System.IO.Directory.GetFiles(directory, filePattern, System.IO.SearchOption.AllDirectories);
            /* In case the read order is not the same as the one in File Explorer */
            // Array.Sort(files, new StringComparer());

            /* Analysis ID */
            string analysis_id = directory.Split(@"\", StringSplitOptions.None)[^1];


            /* Test to check rainflow code against online source */
            /* https://www.mathworks.com/help/signal/ref/rainflow.html */
            //double[] timehistory = new double[] { -2, 1, -3, 5, -1, 3, -4, 4, -2 };
            //Rainflow rainflowtest = new Rainflow(); //1 sec
            //List<DataPoint> RainflowResults = rainflowtest.GetResults(timehistory);



            // Set the probability file directory as the parent directory of the batch runs
            string[] probability_file = System.IO.Directory.GetFiles(di.Parent.FullName, "*Probabilities*", System.IO.SearchOption.AllDirectories);

            double[] hotSpotThetas = new double[] {0, 45, 90, 135, 180, 225, 270, 315}; // the angle of the hot spot relative to the x axis
            Dictionary<int, double> damageDict = new Dictionary<int, double>(); // Dictionary to store the damage at the different hot spots

            /* Markov Matrix file name */
            //string MMName;
            //int MMCounter = 0;
            //do
            //{
            //    MMName = $"{analysis_id}_MarkovMatrix_{theta:000}_{MMCounter:00}.xlsx";
            //    MMCounter++;
            //} while (System.IO.File.Exists(string.Concat(di.Parent.FullName, @"\", MMName)));


            string[] prob_content = System.IO.File.ReadAllLines(probability_file[0]);

            // Create dictionary of probabilities
            Dictionary<string, double> prob_dict = new Dictionary<string, double>();
            // Create character list with delimiters of the probability file lines
            string[] delimiters1 = new string[] {"\t", "_", @"\"};

            // Read probability txt file line by line below line 1 
            for (int i = 1; i < prob_content.Length; i++)
            {
                // From each line read the data as Line properties
                prob_dict.Add(
                    prob_content[i].Split(delimiters1, StringSplitOptions.None)[10],
                    Convert.ToDouble(prob_content[i].Split(delimiters1, StringSplitOptions.None)[14])
                    );
            }


            double probability_i = new double();


            foreach (double theta in hotSpotThetas)
            {
                int console_counter = 0;
                Result[] results = new Result[files.Length];

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
                ///*
                // Read the results in parallel mode for potential computation efficiency
                Parallel.For(0, files.Length, i =>
                {
                    // Iterate counter of progress
                    console_counter++;
                    if (console_counter % 10 == 0)
                    {
                        Console.WriteLine($"Processed {console_counter} of {files.Length}");
                    }
                    // Store the probability of the current analysis in
                    probability_i = prob_dict[files[i].Split(delimiters1, StringSplitOptions.None)[^4]];
                    // Only consider the loadcase if the probability is nonzero
                    // and create a new result item with the loadcase rainflow results
                    if (probability_i > 0) { results[i] = new Result(files[i], probability_i, theta); }
                });
                //*/

                //object name = new object();
                //combined Markov matrix for all files

                /* Create list of <DataPoint> objects, named resultsAllLoadcases and assign it a new object of class DataPoint */
                List<DataPoint> resultsAllLoadcases = new List<DataPoint>();

                /* TODO get the total number of cycles */
                //int totalCount = 0;

                // Iterate through the result data
                foreach (var result in results)
                {
                    // Run RainflowResults on each result file and append resultsAllLoadcases with that
                    if (result.RainflowResults != null)
                    {
                        resultsAllLoadcases.AddRange(result.RainflowResults);
                    }
                    /* TODO get the total number of cycles */
                    //totalCount += result.RainflowResults.Count;
                }

                double damage = new double();

                foreach (DataPoint dp in resultsAllLoadcases)
                {
                    damage += BoltFatigue.BoltDamage(dp.A, dp.B, dp.Count);
                }
                // Convert from 10min (600sec) to the design life duration
                damage *= 25 * 365.25 * 24 * 60 / 10;

                damageDict[(int)theta] = damage;
            }
        }
    }

    class Result
    {
        public List<DataPoint> RainflowResults;
        public Result(string filePath, double probability, double angle)
        {
            string[] content = System.IO.File.ReadAllLines(filePath);

            /* Create list of Line objects */
            List<Line> lines = new List<Line>();

            /* Read the whole analysis results and truncate based on time instead of defining the row explicitly */
            //for (int i = 0; i < content.Length - 1; i++)
            /* Read file line by line below line 5006 to truncate the transient 200s in the beginning of the analysis  */
            for (int i = 5005; i < content.Length - 1; i++)
            {
                /* Read line by line until the time value is greater than 200 */
                /* used if the index for reading the files is set to start from the beginning of the document */
                //if (double.TryParse(content[i].Split(",")[1], out double readTime) && readTime >= 200) 
                //{

                /* From each line read the data as Line properties */
                Line l = new Line(content[i]);
                /* Append lines list with the current line */
                lines.Add(l);

                //}
            }

            //{
            //}

            if (lines.Count != 0)
            {
                // using the lines List extract moment history
                // in this case bending moment at hot spot is given depending on angle of the hot spot [Nm]
                double[] momentHistory = lines.Select(x => (
                Math.Cos(angle * Math.PI / 180) * x.MY - Math.Sin(angle * Math.PI / 180) * x.MX)).ToArray();
                //rainflow count
                Rainflow rainflow = new Rainflow(); //1 sec
                RainflowResults = rainflow.GetResults(momentHistory);
                foreach (var rainflowResult in RainflowResults)
                {
                    rainflowResult.Count *= probability;
                }
            }
        }
    }

   public class Line
    {
        // Create properties of Line object
        public readonly string Description  ;
        public readonly double Time         ;
        public readonly double AX           ;
        public readonly double AY           ;
        public readonly double AZ           ;
        public readonly double FX           ;
        public readonly double FY           ;
        public readonly double FZ           ;
        public readonly double MX           ;
        public readonly double MY           ;
        public readonly double MZ           ;
        public readonly double UX           ;
        public readonly double UY           ;
        public readonly double UZ           ;
        public readonly double PhiX         ;
        public readonly double PhiY         ;
        public readonly double PhiZ         ;
        public readonly double Eta          ;

        public Line(string input)
        {

            // Create character list with delimiters of the probability file lines
            string[] delimiters2 = new string[] {",", "#"};

            // Use queue here instead of string array
            // !!! Potential improvement by using both TrimEntries and RemoveEmptyEntries
            Queue<string> queue = new Queue<string>(input.Split(delimiters2, StringSplitOptions.TrimEntries));

            // Dequeuing the first empty entry (This will be redundant if RemoveEmptyEntries option is used)
            queue.Dequeue();

            Description = queue.Dequeue();
           
            Time        = Convert.ToDouble(queue.Dequeue());
            AX          = Convert.ToDouble(queue.Dequeue());
            AY          = Convert.ToDouble(queue.Dequeue());
            AZ          = Convert.ToDouble(queue.Dequeue());
            FX          = Convert.ToDouble(queue.Dequeue());
            FY          = Convert.ToDouble(queue.Dequeue());
            FZ          = Convert.ToDouble(queue.Dequeue());
            MX          = Convert.ToDouble(queue.Dequeue());
            MY          = Convert.ToDouble(queue.Dequeue());
            MZ          = Convert.ToDouble(queue.Dequeue());
            UX          = Convert.ToDouble(queue.Dequeue());
            UY          = Convert.ToDouble(queue.Dequeue());
            UZ          = Convert.ToDouble(queue.Dequeue());
            PhiX        = Convert.ToDouble(queue.Dequeue());
            PhiY        = Convert.ToDouble(queue.Dequeue());
            PhiZ        = Convert.ToDouble(queue.Dequeue());
            Eta         = Convert.ToDouble(queue.Dequeue());


            // Ideas to change solution to using Parse
            /*
            if (double.TryParse(queue.Dequeue(), out double test4))
            {

            }
            if (double.TryParse(queue.Dequeue(), out double _))
            {

            }
            */
        }
    }
}
