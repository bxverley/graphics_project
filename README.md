# Graphics and Visualisation Project 
### Snow Globe Simulation on Unity 


How to run:

1. Load project into unity
2. Navigate to Loading_screen_Scene.unity in Assests/Scenes
3. Click on the file 
4. Press the play button to run the snow globe simulation

Instructions for the simulation:
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


To render bounding volumes in the snow globes
- In SnowGlobe.cs, uncomment the function: kMeansFunctions.ShowSpheres() at line 128
- To disable the bounding volume, comment the line again 
