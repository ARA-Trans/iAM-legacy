using System;
using Microsoft.Office.Interop.Excel;

namespace Reports.PennDotBridge
{
    internal class PoorBridgesByDeckArea:PennDotBaseReport, IReportActions
    {
        public PoorBridgesByDeckArea(string networkId, string simulationId, _Worksheet oSheet)
            : base(networkId, simulationId, oSheet)
        {

        }

        public void CreateReport()
        {
            Report.SheetPageSetup(Sheet, "Poor Bridges by Deck Area", 50d, 20d, 10d, "", DateTime.Now.ToLongDateString(), "Page &P", 6);
        }
    }
}
