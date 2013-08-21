using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace KerbalTestOne
{
    class RemoteSpaceDock18 : ILaunchFacility
    {
        Vessel remoteDock;
        
        // How much mass is a single rocket part?
        public static float ROCKETPARTS_DENSITY = 0.0025F;

        public RemoteSpaceDock18(Vessel remoteDock)
        {
            this.remoteDock = remoteDock;
        }

        public bool CanFacilityBuildThisVessel(Vessel v)
        {
            //1) figure out the total mass of the vessel
            double totalMass = SpaceDockUtilities18.DetermineMassOfVessel(v);
            //2) multiply by penalty (%25?)
            double penalizedmass = totalMass * 1.25;
            float partsNeeded = (float)penalizedmass * ROCKETPARTS_DENSITY;
            //3) see if there are enough parts available by removing it, then adding it back in
            remoteDock.Load();
            Part first = remoteDock.rootPart;
            MonoBehaviour.print("Requesting " + partsNeeded + " RocketParts");
            float amount = first.RequestResource("RocketParts", partsNeeded);
            MonoBehaviour.print("Received " + amount + " RocketParts");
            if (amount < partsNeeded)
            {
                MonoBehaviour.print("Amount was less than needed");
                //not enough. Restore parts and move on.
                return false;
            }
            
            float returnedAmount = first.RequestResource("RocketParts", -amount);
            MonoBehaviour.print("Returned" + returnedAmount + " RocketParts");
            return true;
        }

        public bool BuildThisVessel(Vessel v)
        {
            double totalMass = SpaceDockUtilities18.DetermineMassOfVessel(v);
            //2) multiply by penalty (%25?)
            double penalizedmass = totalMass * 1.25;
            float partsNeeded = (float)penalizedmass * ROCKETPARTS_DENSITY;
            //3) see if there are enough parts available by removing it, then adding it back in
            remoteDock.Load();
            Part first = remoteDock.rootPart;
            MonoBehaviour.print("Requesting " + partsNeeded + " RocketParts");
            float amount = first.RequestResource("RocketParts", partsNeeded);
            MonoBehaviour.print("Received " + amount + " RocketParts");
            return true;
        }

        public void DeliverParts(Dictionary<string, int> parts)
        {
            throw new NotImplementedException();
        }

        public UnityEngine.Vector3 GetPreciseDistanceToDestination(Vessel currentVessel)
        {
            return remoteDock.transform.position - currentVessel.transform.position;
        }

        public UnityEngine.Vector3 GetSafeDistanceToDestination(Vessel currentVessel, float bufferDistance)
        {
            if (this.IsDestinationLanded())
            {
                //TODO: we need to look at which way is up, and use that to get a safe distance.
                return new Vector3();
            }
            else
            {
                Vector3 preciseDistance = remoteDock.transform.position - currentVessel.transform.position;
                Vector3 safeDistance = preciseDistance;
                safeDistance.y += bufferDistance;
                return safeDistance;
            }
        }

        public UnityEngine.Vector3 GetVelocityOfDestination(Vessel currentVessel)
        {
            return remoteDock.obt_velocity;
        }

        public UnityEngine.Quaternion GetHeadingOfDestination()
        {
            throw new NotImplementedException();
        }

        public bool IsDestinationLanded()
        {
        	// We may not have launched yet, actually. Apply heuristics!
    		return (remoteDock.missionTime < 5 || remoteDock.altitude < 1000 || remoteDock.LandedOrSplashed);
        }

        public Orbit GetOrbitOfDestination()
        {
            return remoteDock.orbit;
        }

        public Orbit GetSafeOrbitOfDestination()
        {
        	if(IsDestinationLanded())
        	{
        		// Give an orbit over the body we're landed on
        		var body = remoteDock.orbit.referenceBody;
        		return CreateOrbit(0, 0, 200000 + body.Radius, 0, 0, 0, 0, body);
        	}
        	else
        	{
        		// Give an orbit a bit behind ours.
        		Orbit safe = GetOrbitOfDestination().Clone();
        		UnityEngine.MonoBehaviour.print("OrbitalConstruction: Epoch " + safe.epoch.ToString() + " for dock");
        		
        		var epochOffset = (((new System.Random()).NextDouble() * 500) + 750.0) / GetOrbitOfDestination().orbitalSpeed;
        		UnityEngine.MonoBehaviour.print("OrbitalConstruction: Epoch offset: " + epochOffset.ToString());
        		
        		safe.epoch -=  epochOffset;
        		UnityEngine.MonoBehaviour.print("OrbitalConstruction: Epoch " + safe.epoch.ToString() + " for ship");
        		return safe;
        	}
            
        }

        public static Orbit CreateOrbit(double inc, double e, double sma, double lan, double w, double mEp, double epoch, CelestialBody body)
        {
            if (double.IsNaN(inc))
                inc = 0;
            if (double.IsNaN(e))
                e = 0;
            if (double.IsNaN(sma))
                sma = body.Radius + body.maxAtmosphereAltitude + 10000;
            if (double.IsNaN(lan))
                lan = 0;
            if (double.IsNaN(w))
                w = 0;
            if (double.IsNaN(mEp))
                mEp = 0;
            if (double.IsNaN(epoch))
                mEp = Planetarium.GetUniversalTime();

            if (Math.Sign(e - 1) == Math.Sign(sma))
                sma = -sma;

            if (Math.Sign(sma) >= 0)
            {
                while (mEp < 0)
                    mEp += Math.PI * 2;
                while (mEp > Math.PI * 2)
                    mEp -= Math.PI * 2;
            }

            return new Orbit(inc, e, sma, lan, w, mEp, epoch, body);
        }
        
        // Get the ground location latitude of this SpaceDock
        public double GetLatitude()
        {
        	return remoteDock.latitude;
        }
        
        // Get the ground location longitude of this SpaceDock
        public double GetLongitude()
        {
        	return remoteDock.longitude;
        }
        
        
        // Get the ground altitude of this SpaceDock
        public double GetAltitude()
        {
        	return remoteDock.altitude;
        }
    }
}
