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
        private List<Deteriorate> CurrentDeteriorate { get; }
        
        private List<Treatments> SimulationTreatments { get; }


        private bool IsBenefitAttributeAscending { get; }
        private bool IsBenefitCalculated { get; }



        public CalculateBenefit(int year, 
            Treatments treatmentToEvaluate,
            List<Deteriorate> deteriorates,
            List<CalculatedAttribute> calculatedAttributes,
            List<Consequences> noTreatmentConsequences,
            List<Committed> committedProjects,
            Dictionary<string, List<AttributeChange>> committedConsequences, 
            Dictionary<string,CommittedEquation> committedEquation,
            List<Treatments> simulationTreatments)
        {
            Year = year;
            Treatment = treatmentToEvaluate;
            Deteriorates = deteriorates;
            CalculatedAttributes = calculatedAttributes;
            NoTreatmentConsequences = noTreatmentConsequences;
            CommittedProjects = committedProjects;
            CommittedConsequences = committedConsequences;
            CommittedEquations = committedEquation;
            IsBenefitAttributeAscending =  SimulationMessaging.GetAttributeAscending(SimulationMessaging.Method.BenefitAttribute);
            IsBenefitCalculated = calculatedAttributes.Any(a => a.Attribute == SimulationMessaging.Method.BenefitAttribute);
            CurrentDeteriorate = new List<Deteriorate>();
            SimulationTreatments = simulationTreatments;

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

            
            double sumBenefit = 0;
            UpdateCurrentDeteriorate(attributeValue);

            var apparentAgeHints = new Dictionary<string, int>();
            foreach (var deteriorate in CurrentDeteriorate)
            {
                apparentAgeHints.Add(deteriorate.Attribute, 0);
            }




            //Calculate the 100 year benefit
            for (var i = 0; i < 100; i++)
            {
                //Make sure the current deterioration equations are current (meet all criteria).
                UpdateCurrentDeteriorate(attributeValue);

                //// Deteriorate current deterioration model.
                foreach (var deteriorate in CurrentDeteriorate)
                {
                    attributeValue[deteriorate.Attribute] =
                        deteriorate.IterateOneYear(attributeValue, apparentAgeHints[deteriorate.Attribute], out double apparentAge);

                    apparentAgeHints[deteriorate.Attribute] = (int) apparentAge;
                }


                // Apply No Treatment or Committed Consequences (or Scheduled)
                var committed = CommittedProjects.Find(c => c.Year == Year);
                var scheduled = Treatment.Scheduleds.Find(s => s.ScheduledYear == i);

                //If both a committed and scheduled.  No benefit.  Can't do both!
                if (scheduled != null && committed != null)
                {
                    return 0;
                }

                //Only get here if scheduled and committed do notcollide.
                if (committed != null)
                {
                    if (!string.IsNullOrWhiteSpace(committed.ScheduledTreatmentId))
                    {
                        var scheduledTreatment =
                            SimulationTreatments.Find(f => f.TreatmentID == committed.ScheduledTreatmentId);
                        attributeValue = ApplyConsequences(scheduledTreatment.ConsequenceList,attributeValue);
                    }
                    else
                    {
                       
                        attributeValue = ApplyCommittedConsequences(attributeValue, committed);
                    }
                }
                else if (scheduled != null)
                {
                    attributeValue = ApplyConsequences(scheduled.Treatment.ConsequenceList, attributeValue);
                }
                else
                {
                    attributeValue = ApplyConsequences(NoTreatmentConsequences, attributeValue);
                }



                //// Solve calculated fields
                attributeValue = SolveCalculatedFields(attributeValue);



                //Look up Method.Benefit variable
                if (IsBenefitAttributeAscending)
                {
                    //For attributes that get smaller with deterioration.
                    if ((double)attributeValue[SimulationMessaging.Method.BenefitAttribute] >
                        SimulationMessaging.Method.BenefitLimit)
                    {
                        sumBenefit += (double)attributeValue[SimulationMessaging.Method.BenefitAttribute] -
                                      SimulationMessaging.Method.BenefitLimit;
                    }
                }
                else
                {
                    //For attributes that get larger with deterioration.
                    if ((double)attributeValue[SimulationMessaging.Method.BenefitAttribute] <
                        SimulationMessaging.Method.BenefitLimit)
                    {
                        sumBenefit += SimulationMessaging.Method.BenefitLimit -
                                      (double)attributeValue[SimulationMessaging.Method.BenefitAttribute.ToUpper()];

                    }
                }



            }


            return sumBenefit;
        }

        private void UpdateCurrentDeteriorate(Hashtable hashInput)
        {
            var needsUpdate = false;

            if (CurrentDeteriorate.Count > 0)
            {
                foreach (var deteriorate in CurrentDeteriorate)
                {
                    if (!deteriorate.IsCriteriaMet(hashInput))
                    {
                        needsUpdate = true;
                    }
                }

            }
            else
            {
                needsUpdate = true;
            }

            if (!needsUpdate) return;

            CurrentDeteriorate.Clear();
            // Deteriorate each non default (may override previous step).
            foreach (var deteriorate in Deteriorates)
            {
                if (deteriorate.Default) continue;

                if (deteriorate.IsCriteriaMet(hashInput))
                {
                    CurrentDeteriorate.Add(deteriorate);
                }
            }

            //Deteriorate each value that did not have a non-default set (will not override).
            foreach (var deteriorate in Deteriorates)
            {
                if (!deteriorate.Default) continue;

                if(CurrentDeteriorate.All(d => d.Attribute != deteriorate.Attribute))
                { 
                    CurrentDeteriorate.Add(deteriorate);
                }
            }
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

            //No Treatment usually only increments age.  If this is the case (and is the only effect of No Treatment solve it quickly.
            if (consequences.Count == 1 && consequences[0].AttributeChange.Count == 1)
            {
                if (consequences[0].AttributeChange[0].Attribute == "AGE")
                {
                    sValue = hashInput["AGE"];
                    hashInput["AGE"] = consequences[0].AttributeChange[0].ApplyChange(sValue);
                    return hashInput;
                }
            }



            //Otherwise do the whole thing.
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
