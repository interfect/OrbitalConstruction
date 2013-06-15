// modified by evilC - evilc@evilc.com
// With 2.0 changes by Interfect
// v 3.0

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
//using System.Text.RegularExpressions;


namespace KerbalTestOne
{
    public enum BuildMode
    {
        Scan,
        Build,
        Confirm,
        Release
    }

    public class UIStatus
    {
        static public bool Checked = false;
        static public double PartsNeeded = 0;
        static public bool StageFirst = false;
        static public bool DockSelected = false;
        static public bool Scanned = false;
        //static public bool Delivered = false;
    }

    class SpaceBuilt18 : Jumper18
    {
        //Basic ui options:
        //1) show how much is needed to build this
        //2) scan for docks
        //3) build at dock
        //4) Confirm build successful
        //5) release docking clamps

        protected Rect windowPos;
        Vessel chosenVessel;
        ILaunchFacility chosenLaunchFacility;
        private int targetIndex = -1;
        private int previousIndex = -1;
        private Vessel targetVessel = null;
        private List<Vessel> docks;
        BuildMode mode;

        private void WindowGUI(int windowID)
        {
            GUIStyle mySty = new GUIStyle(GUI.skin.button);
            mySty.normal.textColor = mySty.focused.textColor = Color.white;
            mySty.hover.textColor = mySty.active.textColor = Color.yellow;
            mySty.onNormal.textColor = mySty.onFocused.textColor = mySty.onHover.textColor = mySty.onActive.textColor = Color.green;
            mySty.padding = new RectOffset(8, 8, 8, 8);


            GUIStyle redSty = new GUIStyle(GUI.skin.box);
            redSty.padding = new RectOffset(8, 8, 8, 8); 
            redSty.normal.textColor = redSty.focused.textColor = Color.red;

            GUIStyle yelSty = new GUIStyle(GUI.skin.box);
            yelSty.padding = new RectOffset(8, 8, 8, 8);
            yelSty.normal.textColor = yelSty.focused.textColor = Color.yellow;

            GUIStyle grnSty = new GUIStyle(GUI.skin.box);
            grnSty.padding = new RectOffset(8, 8, 8, 8);
            grnSty.normal.textColor = grnSty.focused.textColor = Color.green;

            GUIStyle whiSty = new GUIStyle(GUI.skin.box);
            whiSty.padding = new RectOffset(8, 8, 8, 8); 
            whiSty.normal.textColor = whiSty.focused.textColor = Color.white;

            GUILayout.BeginVertical();
            //if (GUILayout.Button("DESTROY", mySty, GUILayout.ExpandWidth(true)))//GUILayout.Button is "true" when clicked

            GUIStyle labelStyle = new GUIStyle(GUI.skin.label);
            //GUILayout.Label("Needed Parts:", labelStyle);
            if (!UIStatus.Checked)
            {
                UIStatus.PartsNeeded = SpaceDockUtilities18.DetermineMassOfVessel(vessel) * 1.25;
                UIStatus.Checked = true;
            }

            GUILayout.Box("Tons of spare parts needed for this Craft: " + UIStatus.PartsNeeded, whiSty);

            if (chosenLaunchFacility != null)
            {
                UIStatus.DockSelected = true;
            }
            else
            {
                UIStatus.DockSelected = false;
            }
            switch (mode)
            {
                case BuildMode.Scan:
                    if (GUILayout.Button("Scan for Docks"))
                    {
                        //scan for spaceDocks
                        docks = SpaceDockUtilities18.GetAllSpaceDocks(vessel);
                        mode = BuildMode.Build;
                        UIStatus.Scanned = true;
                    }
                    break;
                case BuildMode.Build:
                    GUILayout.Box("If using Launch Stabilizers, put them alone in the first stage and check the box below!", yelSty);
                    if (GUILayout.Toggle(UIStatus.StageFirst, "Stage before building"))
                    {
                        UIStatus.StageFirst = true;
                    }
                    else
                    {
                        UIStatus.StageFirst = false;
                    }
                    if (GUILayout.Button("Build at selected dock"))
                    {
                        if (chosenLaunchFacility.CanFacilityBuildThisVessel(vessel))
                        {
                            if (UIStatus.StageFirst)
                            {
                                Staging.ActivateNextStage();
                            }
                            SetJumpState(JumpState18.MatchingOrbits);
                            chosenLaunchFacility.BuildThisVessel(vessel);
                            
                            if(chosenLaunchFacility.IsDestinationLanded()) 
                            {
                            	// We need to land
                            	mode = BuildMode.Confirm;
                            }
                            else
                            {
                        		// We're in the right spot
                            	mode = BuildMode.Release;
                            }
                        }
                    }
                    break;
                case BuildMode.Confirm:
                    {
                        if (GUILayout.Button("Land at dock"))
                        {
                        	// Land at the dock (right now)
                        	LandAtDock();
                            mode = BuildMode.Release;
                        }
                    }
                    break;
                case BuildMode.Release:
                    {
                        SetJumpState(JumpState18.Idle);
                        RenderingManager.RemoveFromPostDrawQueue(3, new Callback(drawGUI)); //close the GUI
                            
                        // Get rid of the part (single-use only)
                        part.explode();
                        UIStatus.Scanned = false;   // reset UI in case user restarts
                        //UIStatus.Delivered = true;
                    }
                    break;
            }


            if (UIStatus.Scanned)
            {
                if (UIStatus.DockSelected)
                {
                    if (chosenLaunchFacility.CanFacilityBuildThisVessel(vessel))
                    {
                        GUILayout.Box("The chosen Dock has enough Parts to build this Craft", grnSty);
                    }
                    else
                    {
                        GUILayout.Box("The chosen Dock does not have enough Parts to build this Craft", redSty);
                    }
                }
                else
                {
                    GUILayout.Box("Please choose a Dock to build this Craft at", yelSty);
                }
                if (docks == null)
                {
                    docks = new List<Vessel>();
                }

                GUIStyle headingStyle = new GUIStyle(GUI.skin.label);
                headingStyle.alignment = TextAnchor.MiddleCenter;
                headingStyle.fontStyle = FontStyle.Bold;
                GUILayout.Label("Build At:", headingStyle);
                List<Vessel> dockList = new List<Vessel>();
                dockList = docks;

                targetIndex = GUILayout.SelectionGrid(targetIndex, dockList.ConvertAll(v => v.vesselName).ToArray(), 1, mySty, GUILayout.ExpandWidth(true));
                if (targetIndex != previousIndex)
                {
                    try
                    {
                        Vessel v = dockList[targetIndex];
                        print("Choosing: " + v.name);
                        chosenVessel = v;
                        chosenLaunchFacility = new RemoteSpaceDock18(v);
                        SetChosenTarget(chosenLaunchFacility);
                        previousIndex = targetIndex;
                        targetVessel = v;
                    }
                    catch
                    {
                    }
                }
            }
            else
            {
                GUILayout.Box("Please use the Scan button to select a Dock to build at.", redSty);
            }


            GUILayout.EndVertical();

            //DragWindow makes the window draggable. The Rect specifies which part of the window it can by dragged by, and is 
            //clipped to the actual boundary of the window. You can also pass no argument at all and then the window can by
            //dragged by any part of it. Make sure the DragWindow command is AFTER all your other GUI input stuff, or else
            //it may "cover up" your controls and make them stop responding to the mouse.
            GUI.DragWindow(new Rect(0, 0, 10000, 20));

        }

        #region UI boilerplate

        private void drawGUI()
        {
            if (vessel.isActiveVessel)
            {
                GUI.skin = HighLogic.Skin;
                windowPos = GUILayout.Window(31, windowPos, WindowGUI, "Orbital Construction", GUILayout.MinWidth(200), GUILayout.MinHeight(200));
            }
        }

        //protected override void onFlightStart()  //Called when vessel is placed on the launchpad
        public override void OnAwake()
        {
            base.OnAwake();
            RenderingManager.AddToPostDrawQueue(3, new Callback(drawGUI));//start the GUI
            mode = BuildMode.Scan;
        }
        //protected override void onPartStart()
        public override void OnStart(PartModule.StartState state)
        {
            if ((windowPos.x == 0) && (windowPos.y == 0))//windowPos is used to position the GUI window, lets set it in the center of the screen
            {
                windowPos = new Rect(Screen.width / 2, Screen.height / 2, 10, 10);
            }
            base.OnStart(state);
        }

        #endregion
    }
}
