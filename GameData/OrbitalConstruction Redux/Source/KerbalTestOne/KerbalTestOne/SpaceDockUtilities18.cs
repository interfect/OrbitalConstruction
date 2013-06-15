using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace KerbalTestOne
{
    public static class SpaceDockUtilities18
    {
        public static bool DetermineIfVesselIsSpaceDock(Vessel v)
        {
            foreach (Part p in v.parts)
            {
                //see if it has our module
                foreach (PartModule mod in p.Modules)
                {
                    //MonoBehaviour.print(mod.moduleName);
                    if (mod.moduleName.Equals("SpaceDock18"))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        public static List<Vessel> GetAllSpaceDocks(Vessel currentVessel)
        {
            List<Vessel> docks = new List<Vessel>();
            foreach (Vessel v in FlightGlobals.Vessels)
            {
                if (v == currentVessel)
                {
                    continue;
                }
                v.Load();
                if (DetermineIfVesselIsSpaceDock(v))
                {
                    docks.Add(v);
                }
            }
            return docks;
        }


        public static double DetermineMassOfVessel(Vessel v)
        {
            double totalmass = 0;
            foreach (Part p in v.parts)
            {
                MonoBehaviour.print("Part:" + p.name);
                MonoBehaviour.print("Mass:" + p.mass);
                totalmass += p.mass;
                //we also need to coutn the resouces in each part, and multiply that by density... but that can wait.
                double resMass = (double)p.GetResourceMass();
                MonoBehaviour.print("ResMass:" + p.GetResourceMass());
                totalmass += resMass;
            }
            MonoBehaviour.print("Mass of vessel: " + totalmass);
            return totalmass;
        }
    }
}
