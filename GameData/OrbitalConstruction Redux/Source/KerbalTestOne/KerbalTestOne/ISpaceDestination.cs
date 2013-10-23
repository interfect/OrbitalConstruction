using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace OrbitalConstruction
{
   
    public interface ISpaceDestination
    {
        /// <summary>
        /// Returns a vector that points exactly at the destination
        /// </summary>
        /// <param name="currentVessel"></param>
        /// <returns></returns>
        Vector3 GetPreciseDistanceToDestination(Vessel currentVessel);

        /// <summary>
        /// Returns a vector that points nearby the destination, but in a safe area (above ground)
        /// </summary>
        /// <param name="currentVessel"></param>
        /// <returns></returns>
        Vector3 GetSafeDistanceToDestination(Vessel currentVessel, float bufferDistance);

        Vector3 GetVelocityOfDestination(Vessel currentVessel);

        Quaternion GetHeadingOfDestination();

        /// <summary>
        /// Returns whether or not this particular destination is landed on some stellar object.
        /// </summary>
        /// <returns></returns>
        bool IsDestinationLanded();
        
        
        // If it is landed, these produce the coordinates it's at.
     	double GetLatitude();
     	double GetLongitude();
     	double GetAltitude();
     	
        

        Orbit GetOrbitOfDestination();

        Orbit GetSafeOrbitOfDestination();
    }
}
