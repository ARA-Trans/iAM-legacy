using System;
using Microsoft.Office.Interop.Excel;

namespace Reports.PennDotBridge
{
    internal class PoorBridgesByCount:PennDotBaseReport, IReportActions
    {
        public PoorBridgesByCount(string networkId, string simulationId, _Worksheet oSheet)
            : base(networkId, simulationId, oSheet)
        {

        }

        public void CreateReport()
        {
            Report.SheetPageSetup(Sheet, "Poor Bridges by Count", 50d, 20d, 10d, "", DateTime.Now.ToLongDateString(), "Page &P", 5);
        }
    }
}
