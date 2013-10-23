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
            //attosecond 10/23/13, there's a simpler way to do this (see below)
            foreach (Part p in v.parts)
            {
                if (p.name.Equals("SpaceDock"))
                {
                    MonoBehaviour.print("Found a spacedock!");
                        return true;
                }
            }
            return false;
        }

        public static List<Vessel> GetAllSpaceDocks(Vessel currentVessel)
        {
            int cnt = 0;
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
                else
                {
                    v.Unload();                 //added by attosecond 10/22/13 to reduce memory usage. The vessel gets loaded again if/when we need to check its RocketParts reserves.
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
