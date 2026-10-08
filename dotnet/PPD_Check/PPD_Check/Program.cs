using PPD_Check;
using System;

// NOTE: define the path to the folder containing the P-y curve files (*Cyclic.pycf).
// Set the PPD_SOIL_CURVE_DIR environment variable, or edit the line below.
string soilCurveDirectory = Environment.GetEnvironmentVariable("PPD_SOIL_CURVE_DIR") ?? System.IO.Directory.GetCurrentDirectory();

string[] paths = System.IO.Directory.GetFiles(soilCurveDirectory, "*Cyclic.pycf", System.IO.SearchOption.AllDirectories); ;


double[] lengths = new double[paths.Length];
lengths[0] = 27;
lengths[1] = 26.5;
lengths[2] = 29.5;
lengths[3] = 26.25;
lengths[4] = 26.75;
lengths[5] = 26.5;
lengths[6] = 26.5;

List<PyReader> pyReaderList = new List<PyReader>();
List<PyDataPoint[]> pyDataPointList = new List<PyDataPoint[]>();
List<PPDChecker> pPDCheckerList = new List<PPDChecker>();

for (int i = 0; i < paths.Length; i++)
{
    pyReaderList.Add(new PyReader(paths[i]));

    pyDataPointList.Add(new PyReader(paths[i]).GetPyDataPoints());

    pPDCheckerList.Add(new PPDChecker(new PyReader(paths[i]).GetPyDataPoints(), lengths[i], 936460000, 23380000));

    bool ppdCheckResult = pPDCheckerList[i].PassFail;
}
