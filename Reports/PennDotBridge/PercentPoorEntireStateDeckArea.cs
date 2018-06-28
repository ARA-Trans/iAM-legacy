using System;
using Microsoft.Office.Interop.Excel;

namespace Reports.PennDotBridge
{
    internal class PercentPoorEntireStateDeckArea :PennDotBaseReport, IReportActions
    {
        public PercentPoorEntireStateDeckArea(string networkId, string simulationId, _Worksheet oSheet)
            : base(networkId, simulationId, oSheet)
        {

        }
        public void CreateReport()
        {
            Report.SheetPageSetup(Sheet, "% Poor Entire State Deck Area", 50d, 20d, 10d, "", DateTime.Now.ToLongDateString(), "Page &P", 4);
        }
    }
}
