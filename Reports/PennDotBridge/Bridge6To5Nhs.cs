using System;
using Microsoft.Office.Interop.Excel;

namespace Reports.PennDotBridge
{
    internal class Bridge6To5Nhs:PennDotBaseReport,IReportActions
    {
        public Bridge6To5Nhs(string networkId, string simulationId, _Worksheet oSheet)
            : base(networkId, simulationId, oSheet)
        {

        }
        public void CreateReport()
        {
            Report.SheetPageSetup(Sheet, "Bridge 6 to 5 NHS", 50d, 20d, 10d, "", DateTime.Now.ToLongDateString(), "Page &P", 9);
        }
    }
}
