# unity-boardgame-template
Work-in-progress boilerplate for checkerboard-type / tile-based boardgames in Unity.

Uses a couple of free placeholder assets which I need to get around to properly crediting; and the numeral textures were authoredby Kimberly over at CU GameDev.

<img src="https://user-images.githubusercontent.com/9970080/143268371-27db163c-9c50-4029-a839-2349520469c1.png" width="25%" ><br>

---

# Known issues: 
---


# Solved issues:

[1382261](https://fogbugz.unity3d.com/default.asp?1382261_gj5dnp0g3nv5ijmf)
> Player build hangs on blank screen after reporting UnloadTime; memory leak proceeds to eat up all system memory til crash.

This has something to do with the way Unity is deserializing data when the player runtime initializes; logger isn't very useful, so still waiting to hear back from Unity support. 
