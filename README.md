# Engineering Tools

A selection of small automation tools I wrote for structural and fatigue analysis of offshore wind turbine support structures: post-processing of aeroelastic and finite-element time series, fatigue damage calculations, and a few helpers for design checks.

The tools are independent of each other. Each lives in its own folder and can be used on its own.

> **Status:** personal engineering utilities, shared as code samples. They were written for specific analysis workflows and file formats, so they are not general-purpose libraries. **No input data, results or third-party software are included.**

## Contents

| Tool | Language | What it does |
|---|---|---|
| [`dotnet/PostProcessingWTGTimeSeries`](dotnet/PostProcessingWTGTimeSeries) | C# (.NET 5) | Reads WTG load time series (one file per load case) and rainflow-counts the bending-moment histories at a set of hot-spot angles. Uses a load case probability file (zero-probability cases are skipped), calculates bolt fatigue damage for a bolted flange, and includes damage-equivalent-load (DEL) routines. |
| [`dotnet/WTGFEMTimeseriesComparison`](dotnet/WTGFEMTimeseriesComparison) | C# (.NET 6, Windows) | Compares WTG load time series against the corresponding FEM-model interface loads, load case by load case. Produces time-series and scatter plots (1:1 line with ±5 % bands) using OxyPlot. |
| [`dotnet/PileDrivingFatigue`](dotnet/PileDrivingFatigue) | C# (.NET 6) | Reads stress histories from a set of spreadsheets (one per structural detail), rainflow-counts them in four stress bands, and calculates S-N curve fatigue damage per hot spot for a given number of pile-driving blows. Writes a summary table. |
| [`dotnet/PPD_Check`](dotnet/PPD_Check) | C# (.NET 6) | Pile penetration depth (PPD) check for a monopile. Reads P-y curve files, derives soil-layer ultimate capacities, and checks shear and moment capacity against the design loads for a given pile length. |
| [`python/AeroelasticAnalysis_Batch_Post_Processing_*.py`](python) | Python 3 | Walk through the output of an aeroelastic-analysis batch run, collect the min/max/mean/standard-deviation statistics of each load case, and write selected channels to a `.csv` (wind speed, power coefficient and thrust coefficient; or wind speed and 1P frequency). |
| [`python/Parametric_Evaluation_of_Bolted_Flange_Spreadsheet_v1.py`](python) | Python 3 | Drives a bolted-flange design spreadsheet through a parametric sweep (diameter, flange width, bolt count, thicknesses) by writing the parameters into a template workbook and saving one workbook per combination. |

## Before you run anything

### 1. You must define the path to your data

None of the programs ship with data, and all machine-specific paths have been removed. Each program needs to be pointed at your own data folder:

| Tool | How to set the data path |
|---|---|
| `PileDrivingFatigue` | Set the `PILE_DRIVING_DATA_DIR` environment variable (default: current working directory). The spreadsheet file names expected in that folder are listed in `Program.cs`. `Summary.dat` is written to the same folder. |
| `PPD_Check` | Set the `PPD_SOIL_CURVE_DIR` environment variable (default: current working directory). Files matching `*Cyclic.pycf` are read recursively. |
| `PostProcessingWTGTimeSeries` | Run from the folder holding the result files, or set `workingDirectory` in `Properties/launchSettings.json` (currently the placeholder `REPLACE_WITH_PATH_TO_DATA_FOLDER`). Edit `filePattern` in `Program.cs` to match your result file names. |
| `WTGFEMTimeseriesComparison` | Run from a folder with `WTG` and `FEM` sub-folders (and a load case ID map file), or set `workingDirectory` in `Properties/launchSettings.json` (placeholder as above). |
| `AeroelasticAnalysis_Batch_Post_Processing_*.py` | Copy the script into the folder of the batch results. The path must contain a folder called `Batch runs`, which is used to name the output `.csv`. |
| `Parametric_Evaluation_of_Bolted_Flange_Spreadsheet_v1.py` | Set the `BOLTED_FLANGE_TEMPLATE_DIR` environment variable (default: current working directory). You need to supply your own template workbook. |

### 2. Input formats are tool-specific

The readers parse specific column layouts, row offsets and file-naming conventions (for example the probability file in `PostProcessingWTGTimeSeries` is parsed by position). If your files differ, the parsing code will need adapting.

### 3. Folder layout expected by the comparison tool

`WTGFEMTimeseriesComparison` looks for WTG results in a `WTG` sub-folder (files matching `*WindD*`) and FEM results in a `FEM` sub-folder (files matching `*Interface*`), plus a load case map file (`*Map*`) in the working folder.

## Not included

### `Rainflow.cs` (required by two tools)

`PostProcessingWTGTimeSeries` and `PileDrivingFatigue` depend on a rainflow-counting and S-N curve class, `Rainflow.cs`. It was not written by me and is therefore not part of this repository, so **these two projects do not compile as published**. To build them you need to supply your own implementation with the following API, declared in the project's own namespace (`PostProcessingWTGTimeSeries` or `PileDrivingFatigue`):

```csharp
public enum SNCurve { B2_AIR, C_AIR, D_AIR /* ... any further S-N curve classes you need */ }

public class SnCurve
{
    public SNCurve Curve;
    public SnCurve(SNCurve curve);
}

public class DataPoint            // one rainflow result (a cycle class)
{
    public double A;              // see DamageEquivalentLoad / BoltFatigue for how A, B,
    public double B;              // Magnitude and Count are used
    public double Magnitude;
    public double Mean;
    public double Count;          // number of cycles
    public DataPoint(double a, double b, double count);
}

public class Rainflow
{
    public Rainflow();
    public List<DataPoint> GetResults(double[] stress);
    public double RetDamage(double stress, double detcycles, double plateThickness, SnCurve snCurve);
}
```

Notes:

- `GetResults` rainflow-counts a time history (the tools pass moment or stress histories) and returns the counted cycles.
- `RetDamage` returns the Palmgren-Miner damage for `detcycles` cycles of range `stress`, for the given S-N curve and plate thickness (thickness correction).
- `PostProcessingWTGTimeSeries` uses `GetResults` and the `DataPoint` fields (`A`, `B`, `Magnitude`, `Count`); `PileDrivingFatigue` additionally uses `RetDamage`, `SnCurve` and `SNCurve`.
- Check the call sites in `Program.cs`, `BoltFatigue.cs`, `DamageEquivalentLoad.cs` and `PileDrivingData.cs` for the exact usage.
- The `simplexcel` NuGet package referenced by `PostProcessingWTGTimeSeries` is not used by any file in this repository; it is only needed if your own rainflow class exports Excel files.

Any standard rainflow-counting implementation, together with the S-N curves of your chosen design standard, can be used.

### Other items not included

- Input data and results of any analysis (load time series, FEM stress spreadsheets, P-y curve files, probability and load case map files).
- The spreadsheet template used by the bolted-flange script.
- The commercial aeroelastic and FEM software that produces the input files.
- A small file-renaming helper script, which added nothing to the collection.

## Requirements

- **.NET:** the SDK for the target framework of each project (`net5.0`, `net6.0`, `net6.0-windows`). Newer SDKs can build them after retargeting. NuGet packages are restored automatically: `ExcelDataReader` (`PileDrivingFatigue`), `simplexcel` (`PostProcessingWTGTimeSeries`), `OxyPlot.Core` and `OxyPlot.Wpf` (`WTGFEMTimeseriesComparison`).
- **Python:** Python 3 with `openpyxl` and `numpy` for the bolted-flange script. The batch post-processing scripts only use the standard library.

Build and run a C# tool, for example:

```bash
cd dotnet/PileDrivingFatigue
export PILE_DRIVING_DATA_DIR="/path/to/your/data"   # PowerShell: $env:PILE_DRIVING_DATA_DIR = "C:\path\to\data"
dotnet run --project PileDrivingFatigue
```

## Disclaimer

These tools are provided as-is, without warranty, and have not been validated for use in design. Calculation results must always be independently checked.
