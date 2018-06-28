using Microsoft.Office.Interop.Excel;
namespace Reports.PennDotBridge
{
    internal class PennDotBaseReport
    {
        public  _Worksheet Sheet { get; }
        public string NetworkId { get; }

        public string SimulationId { get; }


    public PennDotBaseReport(string networkId, string simulationId, _Worksheet oSheet)
        {
            Sheet = oSheet;
            NetworkId = networkId;
            SimulationId = simulationId;
        }
    }
}
