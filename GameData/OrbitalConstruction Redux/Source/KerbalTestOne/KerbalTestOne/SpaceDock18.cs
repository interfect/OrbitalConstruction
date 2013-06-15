using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KerbalTestOne
{
    /// <summary>
    /// This class is only used to set persistence values.
    /// </summary>
    class SpaceDock18 : PartModule
    {
        public override void OnSave(ConfigNode node)
        {
            print("Saving custom data!");
            node.AddValue("SpaceDock", 1);
            base.OnSave(node);
        }

        public override void OnLoad(ConfigNode node)
        {
            print("Loading custom data!");
            base.OnLoad(node);
        }
    }
}
