# F.E.E.L. (FrontEnd - Emulator Launcher)

---------------------------------------------------------------------------------------
Official web site: http://feelfrontend.altervista.org

Graphic frontend for retrogaming PCs (mame-cabinets, bartop, etc.) with animations, snap/videosnap management, customizable skins and configurable for infinite emulators and game lists.

Technical info
---------------------------------------------------------------------------------------
Totally written in c# using
- Microsoft .NET framework 3.5 (VS 2008 EE+)
- Microsoft XNA Game Studio 3.1
- SharpDx https://github.com/sharpdx/SharpDX
- various smaller libs

This sw stack allows execution on almost *any* Windows PC, from oldest "usable" ones (say WinXP) to newest.
HW requirements are minimal (Intel P4 or similar, with even very old video cards, are adequate).

#### Building FEEL

Steps for building:

- install Microsoft .NET Framework 3.5 (it's usually on board on all recent Win OSes: please refer to specific doc)
- install Microsoft Visual C# Express 2008 http://go.microsoft.com/?linkid=7729278
- install Microsoft XNA Game Studio 3.1 https://www.microsoft.com/en-us/download/details.aspx?id=39
- start VS2008 and create a new empty project
- clone FEEL repository inside project's root directory
- add FEEL project using "Add.. Existing project" and selecting Feel.csproj
- build project either in Debug or Release mode
- copy all items in __init_files__ (changelog, config, data, layouts...) into bin\x86\[Debug|Release] directory 

Now you're ready to run the frontend.

Other info/wiki/others
---------------------------------------------------------------------------------------
*Write me*

Licensing
---------------------------------------------------------------------------------------
Copyright (C) 2017 Maurizio Montel

This program is free software: you can redistribute it and/or modify it under the terms of the GNU Lesser General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.

This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU Lesser General Public License for more details.

You should have received a copy of the GNU Lesser General Public License along with this program. If not, see http://www.gnu.org/licenses/.