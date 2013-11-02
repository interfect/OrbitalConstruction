using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace OrbitalConstruction
{
    public partial class SpaceBuilt18 : Jumper18
    {
        //scan all vessels to see if they're docks
        //for each dock found, get a resource map
        public List<Vessel> GetAllSpaceDocks(Vessel currentVessel, ref List<double> inventory)
        {
            List<Vessel> docks = new List<Vessel>();
            foreach (Vessel v in FlightGlobals.Vessels)
            {
                if (v == currentVessel)
                {
                    continue;
                }
                
                if (DetermineIfVesselIsSpaceDock(v))
                {
                    docks.Add(v);
                    v.Load();
                    double amt = 0;
                    foreach (Part p in v.parts)
                    {
                        foreach (PartResource pr in p.Resources)
                        {
                            if (pr.resourceName.Equals("RocketParts"))
                            {
                                amt += pr.amount;
                            }
                        }
                    }
                    inventory.Add(amt);
                    //MonoBehaviour.print("Vessel " + v.name + " is a spacedock,");
                    //MonoBehaviour.print(v.name + " has " + amt + " RocketParts.");
                }
            }
            //MonoBehaviour.print(FlightGlobals.Vessels.Count + " ships found. " + docks.Count + " are spacedocks.");
            if (docks.Count > 0)
            {
                return docks;
            }
            else
            {
                return null;
            }
        }

        //Determine if a given vessel is a SpaceDock
        public bool DetermineIfVesselIsSpaceDock(Vessel v)
        {
            bool retval = false;
            foreach (ProtoPartSnapshot p in v.protoVessel.protoPartSnapshots)
            {
                if (p.partName.Equals("SpaceDock"))
                {
                    retval = true;
                }
            }
            return retval;
        }

        //Returns the build cost for this vessel
        public Dictionary<string, double> getBuildCost(List<Part> partlist)
        {
            float mass = 0.0f;
            Dictionary<string, double> resources = new Dictionary<string, double>();
            Dictionary<string, double> hull_resources = new Dictionary<string, double>();

            foreach (Part pt in partlist)
            {
                string part_name = pt.name;
                mass += pt.mass;
                foreach (PartResource r in pt.Resources)
                {
                    if (r.resourceName == "IntakeAir" || r.resourceName == "KIntakeAir")
                    {
                        // Ignore intake Air
                        continue;
                    }

                    Dictionary<string, double> res_dict = resources;

                    PartResourceDefinition res_def;
                    res_def = PartResourceLibrary.Instance.GetDefinition(r.resourceName);
                    if (res_def.resourceTransferMode == ResourceTransferMode.NONE
                        || res_def.resourceFlowMode == ResourceFlowMode.NO_FLOW)
                    {
                        res_dict = hull_resources;
                    }

                    if (!res_dict.ContainsKey(r.resourceName))
                    {
                        res_dict[r.resourceName] = 0.0;
                    }
                    res_dict[r.resourceName] += r.amount;
                }
            }
           
            // RocketParts for the hull is a separate entity to RocketParts in
            // storage containers
            PartResourceDefinition rp_def;
            rp_def = PartResourceLibrary.Instance.GetDefinition("RocketParts");
            RocketPartsNeeded = mass / rp_def.density;

            // If non pumpable resources are used, convert to RocketParts
            foreach (KeyValuePair<string, double> pair in hull_resources)
            {
                PartResourceDefinition res_def;
                res_def = PartResourceLibrary.Instance.GetDefinition(pair.Key);
                double hull_mass = pair.Value * res_def.density;
                double hull_parts = hull_mass / rp_def.density;
                RocketPartsNeeded += hull_parts;
            }

            // If there is JetFuel (ie LF only tanks as well as LFO tanks - eg a SpacePlane) then split the Surplus LF off as "JetFuel"
            if (resources.ContainsKey("Oxidizer") && resources.ContainsKey("LiquidFuel"))
            {
                double jetFuel = 0.0;
                // The LiquidFuel:Oxidizer ratio is 9:11. Try to minimize rounding effects.
                jetFuel = (11 * resources["LiquidFuel"] - 9 * resources["Oxidizer"]) / 11;
                if (jetFuel < 0.01)
                {
                    // Forget it. not getting far on that. Any discrepency this
                    // small will be due to precision losses.
                    jetFuel = 0.0;
                }
                resources["LiquidFuel"] -= jetFuel;
                resources["JetFuel"] = jetFuel;
            }

            return resources;
        }
    }
}
