/*
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraSkyBoxSettings : MonoBehaviour
{
    Skybox customSkybox;
    ReflectionProbe probeComponent;

    // Start is called before the first frame update
    void Start()
    {



        // Reference to load material for skybox from Assets/Resources folder: https://answers.unity.com/questions/881890/load-material-from-assets.html
        // Reference: https://forum.unity.com/threads/changing-skybox-material-through-script.125672/
        RenderSettings.skybox = Resources.Load("SceneBackground/BlurBrownBoxBackground", typeof(Material)) as Material;
        DynamicGI.UpdateEnvironment();



        // Reference: https://docs.unity3d.com/ScriptReference/ReflectionProbe.html
        probeComponent = GameObject.Find("SnowGlobe").AddComponent<ReflectionProbe>();

        // The probe will contribute to reflections inside a box of size 10x10x10 centered on the position of the probe
        probeComponent.size = new Vector3(10, 10, 10);
       
        // Set the type to realtime and refresh the probe every frame
        probeComponent.mode = UnityEngine.Rendering.ReflectionProbeMode.Realtime;
        probeComponent.refreshMode = UnityEngine.Rendering.ReflectionProbeRefreshMode.EveryFrame;
    }

    private void Update()
    {
        probeComponent.transform.position = GameObject.Find("SnowGlobe").transform.position;
    }
}

*/