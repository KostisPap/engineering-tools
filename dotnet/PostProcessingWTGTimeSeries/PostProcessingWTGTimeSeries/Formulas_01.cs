using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AeroelasticAnalysisPostprocessing_v1
{
    class TubularSection : Section
    {
        public double OuterDiameter { get; set; }
        public double Thickness { get; set; }


        public TubularSection(double OD, double Th)
        {
            OuterDiameter = OD;
            Thickness = Th;
            Area = Math.PI * (Math.Pow(OD, 2) - Math.Pow(OD - 2 * Th, 2)) / 4;
            MomentOfInertia = (OD - Th) / 2 / Math.PI * (Math.Pow(OD, 4) - Math.Pow(OD - 2 * Th, 4)) / 64;
        }
    }

    public abstract class Section
    {
        public double Area { get; set; }
        public double MomentOfInertia { get; set; }
    }
}


/*
int counter = 0;

foreach (var item in files)
{
    Console.WriteLine(item.ToString());
    Console.WriteLine(counter.ToString());

    counter++;
}
*/
//files.ToList().ForEach(i => Console.WriteLine(i.ToString().Split(@"\", StringSplitOptions.None)[i.ToString().Split(@"\", StringSplitOptions.None).Length - 3]));
