
using System;
using Microsoft.Office.Interop.Excel;

namespace Reports.PennDotBridge
{
    internal class NhsGoodFairPoorDeckArea:PennDotBaseReport,IReportActions
    {
        public NhsGoodFairPoorDeckArea(string networkId, string simulationId, _Worksheet oSheet)
            : base(networkId, simulationId, oSheet)
        {

        }
        public void CreateReport()
        {
            Report.SheetPageSetup(Sheet, "NHS Good Fair Poor Deck Area", 50d, 20d, 10d, "", DateTime.Now.ToLongDateString(), "Page &P", 14);
        }
    }
}
