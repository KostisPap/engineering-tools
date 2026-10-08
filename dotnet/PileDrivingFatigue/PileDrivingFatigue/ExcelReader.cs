using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ExcelDataReader;

namespace PileDrivingFatigue
{
    public class ExcelReader
    {
        public List<PileDrivingData> GetLocationTimeHistories(string FilePath, string SheetName, int positions)
        {
            System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);

            List<PileDrivingData> locationStressHistories = new List<PileDrivingData>();

            /* Specify the excel directory */
            string filePath = FilePath;

            using (var stream = File.Open(filePath, FileMode.Open, FileAccess.Read))
            {
                // Auto-detect format, supports:
                //  - Binary Excel files (2.0-2003 format; *.xls)
                //  - OpenXml Excel files (2007 format; *.xlsx, *.xlsb)
                using (var reader = ExcelReaderFactory.CreateReader(stream))
                {
                    // 2. Use the AsDataSet extension method
                    var result = reader.AsDataSet(new ExcelDataSetConfiguration()
                    {

                        ConfigureDataTable = tableReader => new ExcelDataTableConfiguration()
                        {
                            UseHeaderRow = true,
                            /* Skip the first 11 rows */
                            ReadHeaderRow = rowReader => { for (int i = 0; i < 10; i++) { rowReader.Read(); } }
                        }
                    });

                    // The result of each spreadsheet is in result.Tables
                    var timeHistoryTables = result.Tables[SheetName];

                    for (int i = 0;i < positions; i++)
                    {
                        locationStressHistories.Add(SingleNodeTimeHistories(i, timeHistoryTables));
                    }
                }
            }
            return locationStressHistories;
        }

        private static PileDrivingData SingleNodeTimeHistories(int positions, System.Data.DataTable? timeHistoryTables)
        {
            PileDrivingData pileDrivingData = new PileDrivingData();


            double[] time = new double[timeHistoryTables.Rows.Count];
            double[] stressBand01 = new double[timeHistoryTables.Rows.Count];
            double[] stressBand02 = new double[timeHistoryTables.Rows.Count];
            double[] stressBand03 = new double[timeHistoryTables.Rows.Count];
            double[] stressBand04 = new double[timeHistoryTables.Rows.Count];

            int row = 0;
            var items = timeHistoryTables.Rows[row].ItemArray;

            do
            {
                items = timeHistoryTables.Rows[row].ItemArray;

                time[row] = double.Parse(items[19 + 17 * positions].ToString());
                stressBand01[row] = double.Parse(items[29 + 17 * positions].ToString());
                stressBand02[row] = double.Parse(items[30 + 17 * positions].ToString());
                stressBand03[row] = double.Parse(items[31 + 17 * positions].ToString());
                stressBand04[row] = double.Parse(items[32 + 17 * positions].ToString());

                row++;

                if (row % 1000 == 0)
                { Console.Write($"{row} rows processed\n"); }

            } while (row < timeHistoryTables.Rows.Count);

            pileDrivingData.Time = time;
            pileDrivingData.StressBand01 = stressBand01;
            pileDrivingData.StressBand02 = stressBand02;
            pileDrivingData.StressBand03 = stressBand03;
            pileDrivingData.StressBand04 = stressBand04;


            return pileDrivingData;
        }
    }
}
