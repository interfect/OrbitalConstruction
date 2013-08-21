OrbitalConstruction
===================

Interfect's version of Zorkinian's Orbital Construction Mod

This version incorporates both evilC's changes and Interfect's new surface base functionality.

Known Issues
------------

* After building something at a space dock and switching to a nearby craft, the newly built craft will appear to be orbiting backwards or otherwise absurdly. Going to the space center and back will fix this issue.

Changelog
---------

4.2:
* Updated 3D Printer to work with new converter system. Works as of Kethane 0.7.7.
* Changed RocketParts density to 1/400 tons per unit, like in Extraplanetary Launchpads. Tanks still hold the same mass, which may make them unbalanced relative to EL tanks. To use both mods together, delete this mod's resource config file.

4.1:
* Stopped referencing the Kethane models since the Kethane dev asked nicely. 3D printer now has an ugly placeholder model, and needs a new one.

4.0:

* Updated for KSP 0.21.
* Fixed a bug where, if you didn't successfully get your craft to orbit, the mass would be stuck at the mass of the first craft you tried to launch.
* Fixed low performance while a SpaceDock is selected.
* Changed from "SpareParts" to "RocketParts". If you want to use this mod along with Extraplanetary Launchpads, remove this mod's resource config file.
* Added a Kethane-powered 3D printer to make RocketParts from Kethane. This shows up in-game as a copy of the big Kethane converter. Since it doesn't actually include any assets from the Kethane pack, it's in compliance with any provisions of the Kethane license that can actually be enforced.

interfect's 3.0:

* Interfect's 2.0 changes
* evilC's changes

interfect's 2.0:

* Updated for 0.20
* Added support for landed bases.

evicC's version:

* Fixed exessive lag at launch time
* Added status to UI; Prompts on usage etc
* Added mechanism to deal with Launch Stabilizers
