using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace KerbalTestOne
{
	// Ripped from HyperEdit to land the ship, seriously, you guys.
	public class LanderAttachment : MonoBehaviour
    {
        private bool _alreadyTeleported;
        public double Latitude;
        public double Longitude;
        public double Altitude;

        public void FixedUpdate()
        {
            var vessel = GetComponent<Vessel>();
            if (vessel != FlightGlobals.ActiveVessel)
            {
                Destroy(this);
                return;
            }
            if (_alreadyTeleported)
            {
                if (vessel.LandedOrSplashed)
                {
                    Destroy(this);
                }
                else
                {
                    var accel = (vessel.srf_velocity + vessel.upAxis) * -0.5;
                    vessel.ChangeWorldVelocity(accel);
                }
            }
            else
            {
                var alt = vessel.mainBody.pqsController.GetSurfaceHeight(
                    QuaternionD.AngleAxis(Longitude, Vector3d.down) *
                    QuaternionD.AngleAxis(Latitude, Vector3d.forward) * Vector3d.right) -
                          vessel.mainBody.pqsController.radius;
                alt = Math.Max(alt, 0); // Underwater!
                var diff = vessel.mainBody.GetWorldSurfacePosition(Latitude, Longitude, alt + Altitude) - vessel.GetWorldPos3D();
                if (vessel.Landed)
                    vessel.Landed = false;
                else if (vessel.Splashed)
                    vessel.Splashed = false;
                foreach (var part in vessel.parts.Where(part => part.Modules.OfType<LaunchClamp>().Any()).ToList())
                    part.Die();
                HyperEditBehaviour.Krakensbane.Teleport(diff);
                vessel.ChangeWorldVelocity(-vessel.obt_velocity);
                _alreadyTeleported = true;
            }
        }
    }
	
	// Ripped from HyperEdit to be a Krackensbane Getter
	public class HyperEditBehaviour : MonoBehaviour
    {
        private static Krakensbane _krakensbane;

        public static Krakensbane Krakensbane
        {
            get { return _krakensbane ?? (_krakensbane = (Krakensbane)FindObjectOfType(typeof(Krakensbane))); }
        }
    }
	
	public static class Extentions
    {
        public static bool IsVessel(this Orbit orbit)
        {
            return FlightGlobals.fetch != null && FlightGlobals.Vessels.FirstOrDefault(v => v.orbitDriver != null && v.orbit == orbit);
        }

        public static bool IsPlanet(this Orbit orbit)
        {
            return FlightGlobals.fetch != null && FlightGlobals.Bodies.FirstOrDefault(v => v.orbitDriver != null && v.orbit == orbit);
        }

        public static void Set(this Orbit orbit, Orbit newOrbit)
        {
        	UnityEngine.MonoBehaviour.print("OrbitalConstruction: Setting orbit");  
        	
            var vessel = FlightGlobals.fetch == null ? null : FlightGlobals.Vessels.FirstOrDefault(v => v.orbitDriver != null && v.orbit == orbit);
            var body = FlightGlobals.fetch == null ? null : FlightGlobals.Bodies.FirstOrDefault(v => v.orbitDriver != null && v.orbit == orbit);
            if (vessel != null)
                WarpShip(vessel, newOrbit);
            else if (body != null)
                WarpPlanet(body, newOrbit);
            else
                HardsetOrbit(orbit, newOrbit);
        }

        private static void WarpShip(Vessel vessel, Orbit newOrbit)
        {
            if (newOrbit.getRelativePositionAtUT(Planetarium.GetUniversalTime()).magnitude > newOrbit.referenceBody.sphereOfInfluence)
            {
                return;
            }

         	UnityEngine.MonoBehaviour.print("OrbitalConstruction: Warping ship");   
            
            vessel.Landed = false;
            vessel.Splashed = false;
            vessel.landedAt = string.Empty;
            var parts = vessel.parts;
            if (parts != null)
            {
                var clamps = parts.Where(p => p.Modules != null && p.Modules.OfType<LaunchClamp>().Any()).ToList();
                foreach (var clamp in clamps)
                    clamp.Die();
            }

            try
            {
                OrbitPhysicsManager.HoldVesselUnpack(60);
            }
            catch (NullReferenceException)
            {
            }

            foreach (var v in (FlightGlobals.fetch == null ? (IEnumerable<Vessel>)new[] { vessel } : FlightGlobals.Vessels).Where(v => v.packed == false))
                v.GoOnRails();

            HardsetOrbit(vessel.orbit, newOrbit);

            vessel.orbitDriver.pos = vessel.orbit.pos.xzy;
            vessel.orbitDriver.vel = vessel.orbit.vel;
        }

        private static void WarpPlanet(CelestialBody body, Orbit newOrbit)
        {
            
        	UnityEngine.MonoBehaviour.print("OrbitalConstruction: warping planet");
        	
        	var oldBody = body.referenceBody;
            HardsetOrbit(body.orbit, newOrbit);
            if (oldBody != newOrbit.referenceBody)
            {
                oldBody.orbitingBodies.Remove(body);
                newOrbit.referenceBody.orbitingBodies.Add(body);
            }
            body.CBUpdate();
        }

        private static void HardsetOrbit(Orbit orbit, Orbit newOrbit)
        {
        	UnityEngine.MonoBehaviour.print("OrbitalConstruction: Hard setting orbit");
            orbit.inclination = newOrbit.inclination;
            orbit.eccentricity = newOrbit.eccentricity;
            orbit.semiMajorAxis = newOrbit.semiMajorAxis;
            orbit.LAN = newOrbit.LAN;
            orbit.argumentOfPeriapsis = newOrbit.argumentOfPeriapsis;
            orbit.meanAnomalyAtEpoch = newOrbit.meanAnomalyAtEpoch;
            orbit.epoch = newOrbit.epoch;
            orbit.referenceBody = newOrbit.referenceBody;
            orbit.Init();
            orbit.UpdateFromUT(Planetarium.GetUniversalTime());
        }

        public static void Teleport(this Krakensbane krakensbane, Vector3d offset)
        {
            foreach (var vessel in FlightGlobals.Vessels.Where(v => v.packed == false && v != FlightGlobals.ActiveVessel))
                vessel.GoOnRails();
            krakensbane.setOffset(offset);
        }

        public static Rect Set(this Rect rect, int width, int height)
        {
            return new Rect(rect.xMin, rect.yMin, width, height);
        }

        public static Orbit Clone(this Orbit o)
        {
            return new Orbit(o.inclination, o.eccentricity, o.semiMajorAxis, o.LAN,
                             o.argumentOfPeriapsis, o.meanAnomalyAtEpoch, o.epoch, o.referenceBody);
        }

        public static float Soi(this CelestialBody body)
        {
            var radius = (float)(body.sphereOfInfluence * 0.95);
            if (Planetarium.fetch != null && body == Planetarium.fetch.Sun || float.IsNaN(radius) || float.IsInfinity(radius) || radius < 0 || radius > 200000000000f)
                radius = 200000000000f; // jool apo = 72,212,238,387
            return radius;
        }
    }
	
    public enum JumpState18
    {
        Idle,
        MatchingOrbits,
        MicroJumpToSafeTarget,
        FinalApproach,
        ForceMatchVelocity
    }

    public class Jumper18 : PartModule
    {
        private JumpState18 jumpState;
        private ISpaceDestination chosenTarget;

        public void SetJumpState(JumpState18 jumpState)
        {
            this.jumpState = jumpState;
        }

        public void SetChosenTarget(ISpaceDestination destination)
        {
            chosenTarget = destination;
        }
        
        public override void OnUpdate()
        {
            //depending on our mode, we do different things
            //print(jumpState);

            if ((jumpState != JumpState18.Idle) && (jumpState != JumpState18.ForceMatchVelocity))
            {
                print("JumpState:" + jumpState);
            }

            switch (jumpState)
            {
                case JumpState18.Idle:
                    //do nothing
                    break;
                case JumpState18.MatchingOrbits:
                    
                    print("OrbitalConstruction: Matching orbits");
                    
                    // Go to the safe orbit for the destination (in space or over its planet)
                    Orbit orb = chosenTarget.GetSafeOrbitOfDestination();
            		vessel.orbitDriver.orbit.Set(orb);
                    
                    jumpState = JumpState18.Idle;
                    break;
                case JumpState18.MicroJumpToSafeTarget:
                    
                    break;
                case JumpState18.FinalApproach:
                    
                	// It's a landed dock, and we're in orbit over the body.
                	// Land at the dock.
                	LandAtDock();

                    jumpState = JumpState18.Idle;
                    break;
                case JumpState18.ForceMatchVelocity:
                    break;
                default:
                    break;
            }


            base.OnUpdate();
        }

        
        public void LandAtDock()
        {
        	if (chosenTarget != null)
            {
            	Orbit orb = chosenTarget.GetOrbitOfDestination();
        	
	    		print("OrbitalConstruction: Landing at landed dock.");
	    		// First get the radius of what we're going to.
	    		var radius = orb.referenceBody.Radius;
	    		
	    		// We're already in orbit
	            
	            // Work out where the dock is
	            var dockLatitude = chosenTarget.GetLatitude();
	            var dockLongitude = chosenTarget.GetLongitude();
	            
	            print("OrbitalConstruction: Dock is at:" + dockLatitude.ToString() + ", " + dockLongitude.ToString());
	            
	            // Make a Random so we can randomly place our ship near the dock
	            var random = new System.Random();
	            
	            // Figure out an offset in meters, convert to a subtended angle in radians, and then to an angle in degrees.
	            var latitudeOffset = ((random.NextDouble() * 500 + 500) / radius) / (Math.PI / 180);
	            // Same thing, but this time we can be + or -
	            var longitudeOffset = ((random.NextDouble() * 500 - 250) / radius) / (Math.PI / 180);
	            
	            // Calculate target position, ignoring any wraping over poles
	            var targetLatitude = dockLatitude + latitudeOffset;
	            if(targetLatitude > 90) 
	            {
	            	targetLatitude = 90;
	            }
	            if(targetLatitude < -90) 
	            {
	            	targetLatitude = -90;
	            }
	            
	            var targetLongitude = dockLongitude + longitudeOffset;
	            if(targetLongitude > 180) 
	            {
	            	targetLongitude -= 360;
	            }
	            if(targetLongitude < -180)
	            {
	            	targetLongitude += 360;
	            }
	            
	            var targetAltitude = 50;
	            
	            print("OrbitalConstruction: Target is at:" + targetLatitude.ToString() + ", " + targetLongitude.ToString() + " Alt " + targetAltitude.ToString());
	            
	         	// "Land" 10m above the surface.
	            LandAtTarget(targetLatitude, targetLongitude, targetAltitude);
        	}
        }

        
        private bool Nullcheck()
        {
            if (FlightGlobals.fetch == null || FlightGlobals.ActiveVessel == null)
            {
                print("OrbitalConstruction: Could not find the active vessel (are you in the flight scene?)");
                return true;
            }
            return false;
        }
        
        // This uses HyperEdit's LanderAttachment to get us down to the surface, but we need to be orbiting the right thing.
        private void LandAtTarget(double latitude, double longitude, double altitude)
        {
            if (Nullcheck())
                return;
            
            if (FlightGlobals.fetch == null || FlightGlobals.ActiveVessel == null)
            {
                print("OrbitalConstruction: Could not find active vessel");
                return;
            }
            var lander = FlightGlobals.ActiveVessel.GetComponent<LanderAttachment>();
            if (lander == null)
            {
            	print("OrbitalConstruction: Adding landerAttachment");
                lander = FlightGlobals.ActiveVessel.gameObject.AddComponent<LanderAttachment>();
                lander.Latitude = latitude;
                lander.Longitude = longitude;
                lander.Altitude = altitude;
            }
            else
                UnityEngine.Object.Destroy(lander);
        }
        
        

    }
}
