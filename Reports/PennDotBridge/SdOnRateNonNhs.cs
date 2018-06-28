using System;
using Microsoft.Office.Interop.Excel;

namespace Reports.PennDotBridge
{
    internal class SdOnRateNonNhs:PennDotBaseReport, IReportActions
    {
        public SdOnRateNonNhs(string networkId, string simulationId, _Worksheet oSheet)
            : base(networkId, simulationId, oSheet)
        {

        }
        public void CreateReport()
        {
            Report.SheetPageSetup(Sheet, "SD on Rate Non-NHS", 50d, 20d, 10d, "", DateTime.Now.ToLongDateString(), "Page &P", 8);
        }
    }
}
