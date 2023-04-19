# Graphics and Visualisation Project 
### Snow Globe Simulation on Unity 

![](https://github.com/bxverley/graphics_project/blob/main/snowglobe%20shake.gif)

#### How to run:
1. Load project into unity
  1.1 In the Unity Hub application window "Projects" tab, click "Open", and click on "Snow_Globe" folder, then the "Open" button
2 Once the project is opened in Unity, click on the "Scenes" folder at the bottom and double-click "Loading_screen_Scene" 
 (If you can't find it, try navigating to Assests/Scenes and look for a file that is named similarly to "Loading_screen_Scene.unity" or "Loading_screen_Scene".)
3. Press the play button to run the snow globe simulation

#### Instructions for the simulation:
- To load the snow globe, press the Load SnowGlobe button in the middle of the screen 
- Left Click: Translate (Globe)
- Right Click: Rotate (Globe)
- A: Rotate left (Camera)
- D: Rotate Right (Camera)
- W: Zoom in (Camera)
- S: Zoom out (Camera)
- 2: Move Up (Camera)
- X: Move Down (Camera)
- C: Next shape (GlobeShape)
- Shift+C: Previous shape (GlobeShape)

#### To render bounding volumes in the snow globes:
- In SnowGlobe.cs, uncomment the function: kMeansFunctions.ShowSpheres() at line 128
- To disable the bounding volume, comment the line again 

**NOTE:**

There will be a pause between loading each model when C is pressed to go to the next shape/ Shift+C is pressed to go to the previous object.
Please wait for a short while as the next model is being generated
