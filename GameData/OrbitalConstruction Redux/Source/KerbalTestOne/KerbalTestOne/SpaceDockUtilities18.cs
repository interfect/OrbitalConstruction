using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace OrbitalConstruction
{
    public static class SpaceDockUtilities18
    {
        public static bool DetermineIfVesselIsSpaceDock(Vessel v)
        {
            //attosecond 10/23/13, there's a simpler way to do this, and we don't need to load a bunch of vessels into memory!
            foreach (ProtoPartSnapshot p in v.protoVessel.protoPartSnapshots)
            {
                if (p.partName.Equals("SpaceDock"))
                {
                    return true;
                }
            }
            return false;
        }

        public static List<Vessel> GetAllSpaceDocks(Vessel currentVessel)
        {
            //attosecond 10/23/13, now the scan doesn't load a single vessel, avoiding messy conflicts (I imagine I saw some issues with KAS-enabled vessesls being loaded and unloaded.
            List<Vessel> docks = new List<Vessel>();
            foreach (Vessel v in FlightGlobals.Vessels)
            {
                if (v == currentVessel)
                {
                    continue;
                }
                //v.Load();
                if (DetermineIfVesselIsSpaceDock(v))
                {
                    docks.Add(v);
                    MonoBehaviour.print("Vessel " + v.name + " is a spacedock!");
                }
            }
            MonoBehaviour.print(FlightGlobals.Vessels.Count + " ships found. " + docks.Count + " are spacedocks.");
            return docks;
        }


        public static double DetermineMassOfVessel(Vessel v)
        {
            return (double)v.GetTotalMass();
            //entire function replaced by Vessel.GetTotalMass(), attosecond 10/22/13
        }
    }
}
