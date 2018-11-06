using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Simulation
{
    internal class CalculateBenefit
    {
        private int Year { get; }
        private Treatments Treatment { get; }
        private List<Deteriorate> Deteriorates { get; }
        private List<CalculatedAttribute> CalculatedAttributes { get; }
        private List<Consequences> NoTreatmentConsequences { get; }
        private List<Committed> CommittedProjects { get; }
        private Dictionary<string, List<AttributeChange>> CommittedConsequences { get; }
        private Dictionary<string, CommittedEquation> CommittedEquations { get; }

        public CalculateBenefit(int year, 
            Treatments treatmentToEvaluate,
            List<Deteriorate> deteriorates,
            List<CalculatedAttribute> calculatedAttributes,
            List<Consequences> noTreatmentConsequences,
            List<Committed> committedProjects,
            Dictionary<string, List<AttributeChange>> committedConsequences, 
            Dictionary<string,CommittedEquation> committedEquation)
        {
            Year = year;
            Treatment = treatmentToEvaluate;
            Deteriorates = deteriorates;
            CalculatedAttributes = calculatedAttributes;
            NoTreatmentConsequences = noTreatmentConsequences;
            CommittedProjects = committedProjects;
            CommittedConsequences = committedConsequences;
            CommittedEquations = committedEquation;
        }

        public double Solve(Hashtable hashAttributeValue)
        {
            return SimulationMessaging.Method.IsBenefitCost ? SolveBenefitCost(hashAttributeValue) : SolveRemainingLife(hashAttributeValue);
        }

        private double SolveRemainingLife(Hashtable hashAttributeValue)
        {
            throw new NotImplementedException();
        }

        private double SolveBenefitCost(Hashtable attributeValue)
        {
            //Apply consequence of treatment
            attributeValue = ApplyConsequences(Treatment.ConsequenceList, attributeValue);

            var attributeValueNext = new Hashtable();
            double sumBenefit = 0;
            //Calculate the 100 year benefit

            for (var i = 0; i < 100; i++)
            {
                // Set all values for which there are no default deterioration. May get overridden.
                foreach (var key in attributeValue.Keys)
                {
                    //If no default deterioration exists, then add to next value
                    if (!Deteriorates.Exists(d => d.Attribute == key.ToString() && d.Default))
                    {
                        attributeValueNext.Add(key, attributeValue[key]);
                    }
                }

                // Deteriorate each non default (may override previous step).
                foreach (var deteriorate in Deteriorates)
                {
                    if (deteriorate.Default) continue;

                    if (deteriorate.IsCriteriaMet(attributeValue))
                    {
                        attributeValueNext[deteriorate.Attribute] =
                            deteriorate.IterateOneYear(attributeValue, out bool outOfRange);
                    }
                }
                
                //Deteriorate each value that did not have a non-default set (will not override).
                foreach (var deteriorate in Deteriorates)
                {
                    if (!deteriorate.Default) continue;

                    if (!attributeValueNext.ContainsKey(deteriorate.Attribute))
                    {
                        attributeValueNext[deteriorate.Attribute] =
                            deteriorate.IterateOneYear(attributeValue, out bool outOfRange);
                    }
                }

                // Apply No Treatment or Committed Consequences
                var committed = CommittedProjects.Find(c => c.Year == Year);
                attributeValueNext = committed == null ? ApplyConsequences(NoTreatmentConsequences, attributeValueNext) : ApplyCommittedConsequences(attributeValueNext,committed);
                
                
                
                // Solve calculated fields

                attributeValueNext = SolveCalculatedFields(attributeValueNext);



                //Look up Method.Benefit variable
                if (SimulationMessaging.GetAttributeAscending(SimulationMessaging.Method.BenefitAttribute))
                {
                    //For attributes that get smaller with deterioration.
                    if ((double) attributeValueNext[SimulationMessaging.Method.BenefitAttribute] >
                        SimulationMessaging.Method.BenefitLimit)
                    {
                        sumBenefit += (double) attributeValueNext[SimulationMessaging.Method.BenefitAttribute] -
                                      SimulationMessaging.Method.BenefitLimit;
                    }
                }
                else
                {
                    //For attributes that get larger with deterioration.
                    if ((double)attributeValueNext[SimulationMessaging.Method.BenefitAttribute] <
                        SimulationMessaging.Method.BenefitLimit)
                    {
                        sumBenefit += SimulationMessaging.Method.BenefitLimit -
                                      (double) attributeValueNext[SimulationMessaging.Method.BenefitAttribute];

                    }
                }

                //Prepare for next iteration.
                attributeValue = new Hashtable();
                foreach (var key in attributeValueNext.Keys)
                {
                    attributeValue.Add(key, attributeValueNext[key]);
                }

            }


            return sumBenefit;
        }

        /// <summary>
        /// Apply consequences from treatment (or no treatment).
        /// </summary>
        /// <param name="consequences">List of consequences from treatment</param>
        /// <param name="hashInput">Input values for determining criteria and equations</param>
        /// <returns></returns>
        private Hashtable ApplyConsequences(List<Consequences> consequences, Hashtable hashInput)
        {
            object sValue;
            var hashOutput = new Hashtable();

            var hashConsequences = new Hashtable();
            foreach (var consequence in consequences)
            {
                if (consequence.Default)
                {
                    if (!hashConsequences.Contains(consequence.Attributes[0]))
                    {
                        hashConsequences.Add(consequence.Attributes[0], consequence);
                    }
                }
                else
                {
                    if (consequence.Criteria.IsCriteriaMet(hashInput))
                    {
                        if (hashConsequences.Contains(consequence.Attributes[0]))
                        {
                            var consequenceIsDefault = (Consequences)hashConsequences[consequence.Attributes[0]];
                            if (consequenceIsDefault.Default != true) continue;
                            hashConsequences.Remove(consequence.Attributes[0]);
                            hashConsequences.Add(consequence.Attributes[0], consequence);
                        }
                        else
                        {
                            hashConsequences.Add(consequence.Attributes[0], consequence);
                        }
                    }
                }
            }

            // Get all of this years deteriorated values.
            foreach (var key in hashInput.Keys)
            {
                sValue = hashInput[key];
                if (hashConsequences.Contains(key))
                {
                    var consequence = (Consequences)hashConsequences[key];

                    if (consequence.IsEquation)
                    {
                        sValue = consequence.GetConsequence(hashInput);
                    }
                    else
                    {
                        var change = consequence.AttributeChange[0];
                        if (change != null)
                        {
                            sValue = change.ApplyChange(sValue);
                        }
                    }
                }
                hashOutput.Add(key, sValue);
            }


            return hashOutput;
        }

        private Hashtable ApplyCommittedConsequences(Hashtable hashInput, Committed commit)
        {
            var hashOutput = new Hashtable();


            var hashConsequences = new Hashtable();
            var committedConsequences = CommittedConsequences[commit.ConsequenceID];

            foreach (AttributeChange attributeChange in committedConsequences)
            {
                if (SimulationMessaging.AttributeMinimum.Contains(attributeChange.Attribute))
                    attributeChange.Minimum =
                        SimulationMessaging.AttributeMinimum[attributeChange.Attribute].ToString();
                if (SimulationMessaging.AttributeMaximum.Contains(attributeChange.Attribute))
                    attributeChange.Maximum =
                        SimulationMessaging.AttributeMaximum[attributeChange.Attribute].ToString();
                hashConsequences.Add(attributeChange.Attribute, attributeChange);
            }

            // Get all of this years deteriorated values.
            foreach (String key in hashInput.Keys)
            {
                object sValue = hashInput[key];

                if (hashConsequences.Contains(key))
                {
                    AttributeChange change = (AttributeChange) hashConsequences[key];
                    if (change != null && CommittedEquations.ContainsKey(change.Change))
                    {
                        CommittedEquation ce = CommittedEquations[change.Change];
                        if (!ce.HasErrors) sValue = ce.GetConsequence(hashInput);
                    }
                    else
                    {
                        if (change != null)
                        {
                            sValue = change.ApplyChange(sValue);
                        }
                    }
                }

                hashOutput.Add(key, sValue);

            }

            return hashOutput;
        }



        public Hashtable SolveCalculatedFields(Hashtable hashInput)
        {
            var hashOutput = new Hashtable();
            foreach (var calculatedattribute in CalculatedAttributes)
            {
                if (!calculatedattribute.Default) continue;
                var calculated = calculatedattribute.Calculate(hashInput);
                if (hashOutput.Contains(calculatedattribute.Attribute))
                {
                    hashOutput[calculatedattribute.Attribute] = calculated;
                }
                else
                {
                    hashOutput.Add(calculatedattribute.Attribute, calculated);
                }
            }

            foreach (var calculatedattribute in CalculatedAttributes)
            {
                if (calculatedattribute.Default) continue;
                if (!calculatedattribute.IsCriteriaMet(hashInput)) continue;
                var calculated = calculatedattribute.Calculate(hashInput);
                if (hashOutput.Contains(calculatedattribute.Attribute))
                {
                    hashOutput[calculatedattribute.Attribute] = calculated;
                }
                else
                {
                    hashOutput.Add(calculatedattribute.Attribute, calculated);
                }
            }

            foreach (string key in hashInput.Keys)
            {
                if (hashOutput.ContainsKey(key)) continue;
                hashOutput.Add(key,hashInput[key]);
            }


            return hashOutput;
        }
    }
}
