using System;
using Microsoft.Office.Interop.Excel;

namespace Reports.PennDotBridge
{
    internal class Bridge5To4NonNhs : PennDotBaseReport,IReportActions
    {
        public Bridge5To4NonNhs(string networkId, string simulationId, _Worksheet oSheet)
            : base(networkId, simulationId, oSheet)
        {

        }

        public void CreateReport()
        {
            Report.SheetPageSetup(Sheet, "Bridge 5 to 4 Non-NHS", 50d, 20d, 10d, "", DateTime.Now.ToLongDateString(), "Page &P", 12);
        }
    }
}
