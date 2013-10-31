using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace OrbitalConstruction
{
    public enum BuildMode
    {
        Init,
        Scan,
        Adjust,
        Build,
        Release,
        Confirm,
        Pause,
        Error
    }

    public class UIStatus
    {
        public bool MeasuredMass = false;
        public double RocketPartsNeeded = 0;
        public bool GUIactive = false;
        public bool linklfosliders = true;
        public bool stagebeforebuild = false;
        public Vector2 resscroll;
        public bool canbuildcraft = false;
        public Dictionary<string, float> resourcesliders = new Dictionary<string, float>();
    }

    public class Styles
    {
        public GUIStyle normal;
        public GUIStyle red;
        public GUIStyle yellow;
        public GUIStyle green;
        public GUIStyle white;
        public GUIStyle label;
        public GUIStyle slider;
        public GUIStyle sliderText;
        public GUIStyle mySty;
        public GUIStyle cancelButton;
        public GUIStyle buildButton;

        private bool initialized;

        public void Init()
        {
            if (initialized)
                return;
            initialized = true;

            normal = new GUIStyle(GUI.skin.button);
            normal.normal.textColor = normal.focused.textColor = Color.white;
            normal.hover.textColor = normal.active.textColor = Color.yellow;
            normal.onNormal.textColor = normal.onFocused.textColor = normal.onHover.textColor = normal.onActive.textColor = Color.green;
            normal.padding = new RectOffset(8, 8, 8, 8);

            buildButton = new GUIStyle(GUI.skin.button);
            buildButton.normal.textColor = normal.focused.textColor = Color.white;
            buildButton.hover.textColor = normal.active.textColor = Color.yellow;
            buildButton.onNormal.textColor = normal.onFocused.textColor = normal.onHover.textColor = normal.onActive.textColor = Color.green;
            buildButton.padding = new RectOffset(8, 8, 8, 8);
            buildButton.fontSize = 24;

            red = new GUIStyle(GUI.skin.box);
            red.padding = new RectOffset(8, 8, 8, 8);
            red.normal.textColor = red.focused.textColor = Color.red;

            yellow = new GUIStyle(GUI.skin.box);
            yellow.padding = new RectOffset(8, 8, 8, 8);
            yellow.normal.textColor = yellow.focused.textColor = Color.yellow;

            green = new GUIStyle(GUI.skin.box);
            green.padding = new RectOffset(8, 8, 8, 8);
            green.normal.textColor = green.focused.textColor = Color.green;

            white = new GUIStyle(GUI.skin.box);
            white.padding = new RectOffset(8, 8, 8, 8);
            white.normal.textColor = white.focused.textColor = Color.white;

            label = new GUIStyle(GUI.skin.label);
            label.normal.textColor = label.focused.textColor = Color.white;
            label.alignment = TextAnchor.MiddleCenter;

            slider = new GUIStyle(GUI.skin.horizontalSlider);
            slider.margin = new RectOffset(0, 0, 0, 0);

            sliderText = new GUIStyle(GUI.skin.label);
            sliderText.alignment = TextAnchor.MiddleCenter;
            sliderText.margin = new RectOffset(0, 0, 0, 0);

            mySty = new GUIStyle(GUI.skin.button);
            mySty.normal.textColor = mySty.focused.textColor = Color.white;
            mySty.hover.textColor = mySty.active.textColor = Color.yellow;
            mySty.onNormal.textColor = mySty.onFocused.textColor = mySty.onHover.textColor = mySty.onActive.textColor = Color.green;
            mySty.padding = new RectOffset(8, 8, 8, 8);

            cancelButton = new GUIStyle(GUI.skin.button);
            cancelButton.normal.textColor = cancelButton.focused.textColor = cancelButton.hover.textColor = cancelButton.active.textColor = Color.red;
            cancelButton.onNormal.textColor = cancelButton.onFocused.textColor = cancelButton.onHover.textColor = cancelButton.onActive.textColor = Color.green;
            cancelButton.padding = new RectOffset(8, 8, 8, 8);
        }
    }

    /// <summary>
    /// This is an amalgamation of Extraplanetary Launchpads v3.3 by skykooler and Orbital Construction Redux 4.2 by Interfect (original Orbital Construction by Zorkinian)
    /// The parts, module names, and back end vehicle teleportation mechanics are adapted from OC. The GUI and resource management are adapted from EL. Since it's a mish-mash,
    /// I'm sure you'll see where the code needs to be cleaned up and organized. It works, however, so I think I'm done with it. --Attosecond, 10/31/13
    /// </summary>
    public partial class SpaceBuilt18 : Jumper18
    {
        //class variables
        private Rect windowPos = new Rect(Screen.width / 2, Screen.height / 2, 10f, 10f);
        private BuildMode mode = BuildMode.Init;
        private UIStatus uis = new UIStatus();
        private Styles style = new Styles();
        public double RocketPartsNeeded = 0;
        public List<Vessel> SpaceDocks = new List<Vessel>();
        public List<double> SpaceDockRocketPartInventory = new List<double>();
        public Dictionary<string, double> VesselCost = new Dictionary<string, double>();
        private bool DocksAvailable = false;
        private int selectedDockIndex = -1;
        private int previousDockIndex = -1;
        private string[] DockButtons;
        private string DockName;
        PartResourceDefinition rp_def = PartResourceLibrary.Instance.GetDefinition("RocketParts");
        private VesselResources DockResources;
        private ISpaceDestination chosenLaunchFacility;

        private void WindowGUI(int windowID)
        {
            style.Init();
            GUILayout.BeginVertical();
            
            //Now for the big switch statement
            switch (mode)
            {
                case BuildMode.Init:
                    //Scan the current vessel to see what we need to build it
                    VesselCost = getBuildCost(vessel.parts);

                    if (!DocksAvailable)
                    {
                        //scan for spacedocks, and collect total RocketPart info for them
                        SpaceDocks = GetAllSpaceDocks(vessel, ref SpaceDockRocketPartInventory);
                        if (SpaceDocks == null)
                        {
                            //PROBLEM-- NO SpaceDocks found
                            PopupDialog.SpawnPopupDialog("Sorry", "No SpaceDocks found.", "OK", false, HighLogic.Skin);
                            mode = BuildMode.Error;
                            break;
                        }

                        List<int> removeindices = new List<int>();
                        for (int cnt = 0; cnt < SpaceDockRocketPartInventory.Count; cnt++)
                        {
                            if (SpaceDockRocketPartInventory[cnt] >= RocketPartsNeeded)
                            {
                                DocksAvailable = true;
                            }
                            else
                            {
                                removeindices.Add(cnt);
                            }
                        }
                        //Remove any docks that don't have enough RocketParts
                        if (removeindices.Count > 0)
                        {
                            for (int cnt = 0; cnt < removeindices.Count; cnt++)
                            {
                                SpaceDocks.RemoveAt(removeindices[cnt]);
                                SpaceDockRocketPartInventory.RemoveAt(removeindices[cnt]);
                            }
                        }

                        if (!DocksAvailable)
                        {
                            //No docks were found with enough RocketParts. Abort
                            mode = BuildMode.Error;
                            PopupDialog.SpawnPopupDialog("Sorry", "Existing SpaceDocks do not have enough RocketParts. This vessel needs at least " + Math.Round(RocketPartsNeeded,2) + " RocketParts to build.", "OK", false, HighLogic.Skin);
                            break;
                        }

                        //We made it here, so we must have found one or more docks. Prepare the button labels
                        DockButtons = new string[SpaceDocks.Count];
                        for (int cnt = 0; cnt < SpaceDocks.Count; cnt++)
                        {
                            DockButtons[cnt] = SpaceDocks[cnt].vesselName + " (" + Math.Round(SpaceDockRocketPartInventory[cnt] * (double)rp_def.density, 2) + " tons of RocketParts available)";
                        }
                    }
                    mode = BuildMode.Scan;
                    GUILayout.Box("Please select a SpaceDock to build at:", style.white);
                    selectedDockIndex = GUILayout.SelectionGrid(selectedDockIndex, DockButtons.ToArray(), 1, style.mySty, GUILayout.ExpandWidth(true));
                    break;

                case BuildMode.Scan:
                    //Now wait until a dock is selected
                    if (selectedDockIndex != previousDockIndex)
                    {
                        previousDockIndex = selectedDockIndex;
                        Vessel v = SpaceDocks[selectedDockIndex];
                        v.Load();
                        DockName = v.vesselName;
                        DockResources = new VesselResources(SpaceDocks[selectedDockIndex]);
                        mode = BuildMode.Adjust;
                    }
                    GUILayout.Box("Please select a SpaceDock to build at:", style.white);
                    selectedDockIndex = GUILayout.SelectionGrid(selectedDockIndex, DockButtons.ToArray(), 1, style.mySty, GUILayout.ExpandWidth(true));
                    break;
                case BuildMode.Adjust:
                    //Double check to see if the selected dock has changed
                    if (selectedDockIndex != previousDockIndex)
                    {
                        previousDockIndex = selectedDockIndex;
                        Vessel v = SpaceDocks[selectedDockIndex];
                        v.Load();
                        DockName = v.vesselName;
                        DockResources = new VesselResources(SpaceDocks[selectedDockIndex]);
                    }
                    //display resource sliders
                    GUILayout.Box("Selected Craft:	" + DockName, style.white);
                    // Resource requirements
                    GUILayout.Label("Resources required to build:", style.label, GUILayout.Width(600), GUILayout.ExpandWidth(true));

                    // Link LFO toggle
                    uis.linklfosliders = GUILayout.Toggle(uis.linklfosliders, "Link RocketFuel sliders for LiquidFuel and Oxidizer");
                    GUILayout.TextField("Place launch clamps in a separate stage preceeding all other stages.", style.label);
                    uis.stagebeforebuild = GUILayout.Toggle(uis.stagebeforebuild, "Stage before building");
                    uis.resscroll = GUILayout.BeginScrollView(uis.resscroll, GUILayout.Width(600), GUILayout.Height(300), GUILayout.ExpandWidth(true));
                    GUILayout.BeginHorizontal();

                    // Headings
                    GUILayout.Label("Resource", style.label, GUILayout.Width(120));
                    GUILayout.Label("Fill Percentage", style.label, GUILayout.Width(300));
                    GUILayout.Label("Required", style.label, GUILayout.Width(75));
                    GUILayout.Label("Available", style.label, GUILayout.Width(75));
                    GUILayout.EndHorizontal();

                    uis.canbuildcraft = true;	   // default to can build - if something is stopping us from building, we will set to false later

                    if (!VesselCost.ContainsKey("RocketParts")) {
				        // if the craft to be built has no rocket parts storage, then the amount to use is not adjustable
				        string resname = "RocketParts";
                        double available = DockResources.ResourceAmount(resname);
                        ResourceLine(resname, resname, 1.0F, RocketPartsNeeded, RocketPartsNeeded, available);
			        }

                    // Cycle through required resources
                    foreach (KeyValuePair<string, double> pair in VesselCost)
                    {
                        string resname = pair.Key;	// Holds REAL resource name. May need to translate from "JetFuel" back to "LiquidFuel"
                        string reslabel = resname;	 // Resource name for DISPLAY purposes only. Internally the app uses pair.Key
                        if (reslabel == "JetFuel")
                        {
                            if (pair.Value == 0f)
                            {
                                // Do not show JetFuel line if not being used
                                continue;
                            }
                            //resname = "JetFuel";
                            resname = "LiquidFuel";
                        }
                        if (!uis.resourcesliders.ContainsKey(pair.Key))
                        {
                            uis.resourcesliders.Add(pair.Key, 1);
                        }

                        // If in link LFO sliders mode, rename Oxidizer to LFO (Oxidizer) and LiquidFuel to LFO (LiquidFuel)
                        if (reslabel == "Oxidizer")
                        {
                            reslabel = "RocketFuel (Ox)";
                        }
                        if (reslabel == "LiquidFuel")
                        {
                            reslabel = "RocketFuel (LF)";
                        }

                        double minAmount = 0.0;
                        double maxAmount = VesselCost[resname];
                        if (resname == "RocketParts")
                        {
                            minAmount += RocketPartsNeeded;
                            maxAmount += RocketPartsNeeded;
                        }

                        double available = DockResources.ResourceAmount(resname);
                        // If LFO LiquidFuel exists and we are on LiquidFuel (Non-LFO), then subtract the amount used by LFO(LiquidFuel) from the available amount
                        if (pair.Key == "JetFuel")
                        {
                            available -= VesselCost["LiquidFuel"] * uis.resourcesliders["LiquidFuel"];
                            if (available < 0.0)
                                available = 0.0;
                        }

                        uis.resourcesliders[pair.Key] = ResourceLine(reslabel, pair.Key, uis.resourcesliders[pair.Key], minAmount, maxAmount, available);
                        if (uis.linklfosliders)
                        {
                            float tmp = uis.resourcesliders[pair.Key];
                            if (pair.Key == "Oxidizer")
                            {
                                uis.resourcesliders["LiquidFuel"] = tmp;
                            }
                            else if (pair.Key == "LiquidFuel")
                            {
                                uis.resourcesliders["Oxidizer"] = tmp;
                            }
                        }
                    }

                    GUILayout.EndScrollView();

                    // Build button
                    if (uis.canbuildcraft)
                    {
                        if (GUILayout.Button("Build", style.buildButton, GUILayout.ExpandWidth(true)))
                        {
                            mode = BuildMode.Build;
                        }
                    }
                    else
                    {
                        GUILayout.Box("Please adjust resources.", style.red);
                    }
                    if (GUILayout.Button("Go back to dock selection", style.normal))
                    {
                        mode = BuildMode.Scan;
                        RenderingManager.RemoveFromPostDrawQueue(3, new Callback(drawGUI));
                        windowPos = new Rect(Screen.width / 2, Screen.height / 2, 10f, 10f);
                        windowPos = GUILayout.Window(1, windowPos, WindowGUI, "Orbital Construction", GUILayout.MinWidth(140), GUILayout.MinHeight(80));
                        RenderingManager.AddToPostDrawQueue(3, new Callback(drawGUI));
                        selectedDockIndex = previousDockIndex = -1;
                    }
                    break;
                case BuildMode.Build:
                    //First thing we need to do is remove all Kerbals
                    vessel.rootPart.DespawnAllCrew();
                    List<ProtoCrewMember> crew = vessel.GetVesselCrew();
                    foreach (ProtoCrewMember p in crew)
                    {
                        vessel.rootPart.RemoveCrewmember(p);
                    }

                    //Use necessary resources from the dock
                    UseResources(vessel, SpaceDocks[selectedDockIndex]);

                    if (!SpaceDocks[selectedDockIndex].loaded)
                    {
                        SpaceDocks[selectedDockIndex].Load();
                    }
                    chosenLaunchFacility = new RemoteSpaceDock18(SpaceDocks[selectedDockIndex]);
                    SetChosenTarget(chosenLaunchFacility);

                    //If using launch clamps, release them
                    if (uis.stagebeforebuild)
                    {
                        Staging.ActivateNextStage();
                    }

                    SetJumpState(JumpState18.MatchingOrbits);
                    OnUpdate();         //wasn't getting called on its own for some reason
                            
                    if(chosenLaunchFacility.IsDestinationLanded()) 
                    {
                        // We need to land
                        mode = BuildMode.Confirm;
                        windowPos = GUILayout.Window(1, windowPos, WindowGUI, "Orbital Construction", GUILayout.MinWidth(140), GUILayout.MinHeight(80));
                    }
                    else
                    {
                        // We're in the right spot
                        mode = BuildMode.Release;
                    }
                    break;
                case BuildMode.Confirm:
                    if (GUILayout.Button("Land at dock", style.buildButton))
                    {
                        // Land at the dock (right now)
                        LandAtDock();
                        mode = BuildMode.Release;
                    }
                    break;
                case BuildMode.Release:
                    SetJumpState(JumpState18.Idle);
                    RenderingManager.RemoveFromPostDrawQueue(3, new Callback(drawGUI)); //close the GUI
                    part.explode();                    // Get rid of the part (single-use only)
                    break;
                case BuildMode.Error:
                    break;
            }

            GUILayout.FlexibleSpace();
            if (GUILayout.Button("Cancel", style.cancelButton))
            {
                KillGUI();
            }
            GUILayout.EndVertical();
            GUI.DragWindow();
        }

        #region UI stuff
        public override void OnStart(PartModule.StartState state)
        {
            RenderingManager.AddToPostDrawQueue(3, new Callback(drawGUI)); //start the GUI
            uis.GUIactive = true;
        }
        private void drawGUI()
        {
            GUI.skin = HighLogic.Skin;
            windowPos = GUILayout.Window(1, windowPos, WindowGUI, "Orbital Construction", GUILayout.MinWidth(140), GUILayout.MinHeight(80));
        }
        public void KillGUI()
        {
            RenderingManager.RemoveFromPostDrawQueue(3, new Callback(drawGUI)); //stop the GUI
            part.explode();
        }
        #endregion

        #region Helper functions
        private float ResourceLine(string label, string resourceName, float fraction, double minAmount, double maxAmount, double available)
        {
            GUILayout.BeginHorizontal();

            // Resource name
            GUILayout.Box(label, style.white, GUILayout.Width(120), GUILayout.Height(40));

            // Fill amount
            GUILayout.BeginVertical();
            GUILayout.FlexibleSpace();
            // limit slider to 0.5% increments
            if (minAmount == maxAmount)
            {
                GUILayout.Box("Must be 100%", GUILayout.Width(300), GUILayout.Height(20));
                fraction = 1.0F;
            }
            else
            {
                fraction = (float)Math.Round(GUILayout.HorizontalSlider(fraction, 0.0F, 1.0F, style.slider, GUI.skin.horizontalSliderThumb, GUILayout.Width(300), GUILayout.Height(20)), 3);
                fraction = (Mathf.Floor(fraction * 200)) / 200;
                GUILayout.Box((fraction * 100).ToString() + "%", style.sliderText, GUILayout.Width(300), GUILayout.Height(20));
            }
            GUILayout.FlexibleSpace();
            GUILayout.EndVertical();

            double required = minAmount * (1 - fraction) + maxAmount * fraction;

            // Calculate if we have enough resources to build
            GUIStyle requiredStyle = style.green;
            if (available < required)
            {
                requiredStyle = style.red;
                // prevent building unless debug mode is on, or kethane is not
                // installed (kethane is required for resource production)
                uis.canbuildcraft = false;
            }
            // Required and Available
            GUILayout.Box((Math.Round(required, 2)).ToString(), requiredStyle, GUILayout.Width(75), GUILayout.Height(40));
            GUILayout.Box((Math.Round(available, 2)).ToString(), style.white, GUILayout.Width(75), GUILayout.Height(40));

            // Flexi space to make sure any unused space is at the right-hand edge
            GUILayout.FlexibleSpace();

            GUILayout.EndHorizontal();

            return fraction;
        }

        private void UseResources(Vessel craft, Vessel Dock)
        {
            VesselResources craftResources = new VesselResources(craft);
            if (!Dock.loaded)
            {
                Dock.Load();
            }
            VesselResources padResources = new VesselResources(Dock);

            // Remove all resources that we might later fill (hull resources will not be touched)
            HashSet<string> resources_to_remove = new HashSet<string>(VesselCost.Keys);
            craftResources.RemoveAllResources(resources_to_remove);

            // remove rocket parts required for the hull and solid fuel
            padResources.TransferResource("RocketParts", -RocketPartsNeeded);

            // use resources
            foreach (KeyValuePair<string, double> pair in VesselCost)
            {
                // If resource is "JetFuel", rename to "LiquidFuel"
                string res = pair.Key;
                if (pair.Key == "JetFuel")
                {
                    res = "LiquidFuel";
                    if (pair.Value == 0)
                        continue;
                }
                if (!uis.resourcesliders.ContainsKey(pair.Key))
                {
                    Debug.Log(String.Format("[EL] missing slider {0}", pair.Key));
                    continue;
                }
                // Calculate resource cost based on slider position - note use pair.Key NOT res! we need to use the position of the dedicated LF slider not the LF component of LFO slider
                double tot = pair.Value * uis.resourcesliders[pair.Key];
                // Transfer the resource from the vessel doing the building to the vessel being built
                padResources.TransferResource(res, -tot);
                craftResources.TransferResource(res, tot);
            }
        }
        #endregion
    }
}
