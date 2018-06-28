using System;
using Microsoft.Office.Interop.Excel;

namespace Reports.PennDotBridge
{
    internal class NonNhsGoodFairPoorNumberBridges:PennDotBaseReport, IReportActions
    {
        public NonNhsGoodFairPoorNumberBridges(string networkId, string simulationId, _Worksheet oSheet)
            : base(networkId, simulationId, oSheet)
        {

        }

        public void CreateReport()
        {
            Report.SheetPageSetup(Sheet, "NonNHS Good Fair Poor # Bridges", 50d, 20d, 10d, "", DateTime.Now.ToLongDateString(), "Page &P", 15);
        }
    }
}
