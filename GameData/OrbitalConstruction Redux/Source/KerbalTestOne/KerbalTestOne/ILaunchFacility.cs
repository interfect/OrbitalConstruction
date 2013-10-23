using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OrbitalConstruction
{
    public interface ILaunchFacility: ISpaceDestination
    {
        /// <summary>
        /// Returns whether or not this launch facility has the parts necessary to build the given vessel
        /// </summary>
        /// <param name="v"></param>
        /// <returns></returns>
        bool CanFacilityBuildThisVessel(Vessel v);

        /// <summary>
        /// Removes the parts from the inventory needed to build the given vessel.
        /// </summary>
        /// <param name="v"></param>
        bool BuildThisVessel(Vessel v);

        /// <summary>
        /// This is how the spaceDock accepts new cargo.
        /// </summary>
        /// <param name="parts"></param>
        void DeliverParts(Dictionary<string, int> parts);
    }
}
