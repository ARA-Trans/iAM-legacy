using System;
using Microsoft.Office.Interop.Excel;

namespace Reports.PennDotBridge
{
    internal class NhsGoodFairPoorNumberBridges:PennDotBaseReport,IReportActions
    {
        public NhsGoodFairPoorNumberBridges(string networkId, string simulationId, _Worksheet oSheet)
            : base(networkId, simulationId, oSheet)
        {

        }
        public void CreateReport()
        {
            Report.SheetPageSetup(Sheet, "NHS Good Fair Poor # Bridges", 50d, 20d, 10d, "", DateTime.Now.ToLongDateString(), "Page &P", 13);
        }

    }
}
