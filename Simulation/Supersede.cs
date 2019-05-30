using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using RoadCareGlobalOperations;
using Simulation.Interface;

namespace Simulation
{
    class Supersede:ISupersede
    {
        public int SupersedeId { get; }
        public int TreatmentId { get; }
        public int SupersedeTreatmentId { get; }
        public Criterias Criteria { get; }


        public Supersede(int supersedeId, int treatmentId, int supersedeTreatmentId, string criteria)
        {
            SupersedeId = supersedeId;
            TreatmentId = treatmentId;
            SupersedeTreatmentId = supersedeTreatmentId;

            Criteria = new Criterias("SUPERSEDE", "BINARY_CRITERIA",SupersedeId.ToString());
            byte[] assemblyCriteria = null;
            string currentCriteria = "";
            if (!string.IsNullOrWhiteSpace(criteria)) currentCriteria = Simulation.ConvertOMSAttribute(criteria);
            assemblyCriteria = SimulationMessaging.GetSerializedCalculateEvaluate("SUPERSEDE", "BINARY_CRITERIA", SupersedeId.ToString(), assemblyCriteria);
            if (assemblyCriteria != null && assemblyCriteria.Length > 0)
            {
                Criteria.Evaluate = (CalculateEvaluate.CalculateEvaluate)AssemblySerialize.DeSerializeObjectFromByteArray(assemblyCriteria);
                if (Criteria.Evaluate.OriginalInput != currentCriteria)
                {
                    Criteria.Evaluate = null;
                }
            }

            if (Criteria.Evaluate == null || Criteria.Evaluate.m_cr == null) return;
            if (!File.Exists(Criteria.Evaluate.m_cr.PathToAssembly))
            {
                Criteria.Evaluate = null;
            }

            foreach (String str in Criteria.Errors)
            {
                SimulationMessaging.AddMessage(new SimulationMessage("Error: Supersede criteria = " + Criteria));
            }

        }





    }
}
