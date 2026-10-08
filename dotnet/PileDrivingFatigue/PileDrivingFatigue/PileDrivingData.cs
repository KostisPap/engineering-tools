using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PileDrivingFatigue
{
    public class PileDrivingData
    {
        public double[] Time { get; set; }
        public double[] StressBand01 { get; set; }
        public double[] StressBand02 { get; set; }
        public double[] StressBand03 { get; set; }
        public double[] StressBand04 { get; set; }

    }

    public class NodeRainflow
    {
        public List<DataPoint> RainflowBand01 { get; set; }
        public List<DataPoint> RainflowBand02 { get; set; }
        public List<DataPoint> RainflowBand03 { get; set; }
        public List<DataPoint> RainflowBand04 { get; set; }

    }
}
