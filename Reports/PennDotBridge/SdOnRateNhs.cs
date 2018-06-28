using System;
using Microsoft.Office.Interop.Excel;

namespace Reports.PennDotBridge
{
    internal class SdOnRateNhs:PennDotBaseReport,IReportActions
    {
        public SdOnRateNhs(string networkId, string simulationId, _Worksheet oSheet)
            : base(networkId, simulationId, oSheet)
        {

        }

        public void CreateReport()
        {
            Report.SheetPageSetup(Sheet, "SD On Rate NHS", 50d, 20d, 10d, "", DateTime.Now.ToLongDateString(), "Page &P", 7);
        }
    }
}
