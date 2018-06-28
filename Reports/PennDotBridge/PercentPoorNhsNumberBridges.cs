using System;
using Microsoft.Office.Interop.Excel;

namespace Reports.PennDotBridge
{
    internal class PercentPoorNhsNumberBridges :PennDotBaseReport, IReportActions
    {
        public PercentPoorNhsNumberBridges(string networkId, string simulationId, _Worksheet oSheet)
            : base(networkId, simulationId, oSheet)
        {

        }

        public void CreateReport()
        {
            Report.SheetPageSetup(Sheet, "% Poor NHS # Bridges", 50d, 20d, 10d, "", DateTime.Now.ToLongDateString(), "Page &P", 2);
        }
    }
}
