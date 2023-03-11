using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

public class SnowGlobe : MonoBehaviour
{
    // Output to Unity Logs Reference: https://docs.unity3d.com/ScriptReference/Debug.Log.html
    // Debug.Log("Message");

    // Reference for Mesh Generation: https://www.youtube.com/watch?v=eJEpeUH1EMg
    private Mesh snowGlobe;
    private List<Vector3> vertices;
    private List<Vector3> vertexNormals;
    private List<int> triangles;

    // For metallic and glossy texture
    private float materialMetallicVal = 0.7f; 
    private float materialGlossyVal = 1;

    private string objectName;
    private string[] objectsNames;
    private int objectNameIndex;

    MeshRenderer gameObjectMeshRenderer;
    MeshFilter gameObjectMeshFilter;

    ParticleSystem snowParticleSystem;

    // Start is called before the first frame update
    void Start()
    {
        objectsNames = new string[] { "garg", "mickey", "sphere" };
        objectNameIndex = 0;
        

        // These 2 MUST HAVE, else Unity does not draw mesh in render.
        // Reference:
        // https://stackoverflow.com/questions/31586186/loading-a-obj-into-unity-at-runtime
        // https://docs.unity3d.com/560/Documentation/Manual/class-MeshFilter.html           
        gameObjectMeshRenderer = gameObject.AddComponent<MeshRenderer>();
        gameObjectMeshFilter = gameObject.AddComponent<MeshFilter>();

        snowGlobe = new Mesh();

        GenerateGlobeProperties();
        UpdateGlobeProperties();

        gameObjectMeshFilter.mesh = snowGlobe;

        SetMeshRendererProperties();




        // NOT WORKING YET //

        // ------ Snow Particle System ------ //
        snowParticleSystem = gameObject.AddComponent<ParticleSystem>();

        int particleCount = 10;

        ParticleSystem.Particle[] particleList = new ParticleSystem.Particle[particleCount];

        for(int i = 0; i < particleCount; i++)
        {
            particleList[i] = new ParticleSystem.Particle();
            particleList[i].startColor = Color.white;
            particleList[i].position = Vector3.zero;
            particleList[i].remainingLifetime = float.PositiveInfinity;
        }

        snowParticleSystem.SetParticles(particleList);
        
        snowParticleSystem.Stop();

        // snowParticleSystem.Emit(5);

        // Reference on setting particle colors: https://answers.unity.com/questions/1363190/how-do-i-change-particle-system-color-via-script-i.html
        // Reference on having to have a ParticleSystem.MainModule class variable and not directly using snowParticleSystem.main.startColor (which gives error) : https://forum.unity.com/threads/error-cs1612-cannot-modify-a-value-type-return-value-of-unityengine-particlesystem-main.793899/
        // ParticleSystem.MainModule mainProperties = snowParticleSystem.main;
        // mainProperties.startColor = Color.white; 

        // ------ Snow Particle System ------ //

    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.C))
        {
            snowGlobe.Clear();

            // Need to use objectsNames.Length - 1, or else objectNameIndex++ would go out of range.
            if (objectNameIndex < objectsNames.Length - 1)
            {
                objectNameIndex++; 
            }
            else
            {
                objectNameIndex = 0;
            }

            GenerateGlobeProperties();
            UpdateGlobeProperties();
        }
    }

    void GenerateGlobeProperties()
    {
        objectName = objectsNames[objectNameIndex];

        vertices = new List<Vector3>();
        vertexNormals = new List<Vector3>();
        triangles = new List<int>();

        List<(int vertexIndex, int vertexNormIndex)> listOfTuples = new List<(int vertexIndex, int vertexNormIndex)>();    // To arrange vertexNormal list later on.
        // Tuple Reference: https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/builtin-types/value-tuples


        // Reference for reading file: https://stackoverflow.com/questions/31586186/loading-a-obj-into-unity-at-runtime
        // Another (maybe) useful link: https://support.unity.com/hc/en-us/articles/115000341143-How-do-I-read-and-write-data-from-a-text-file-
        string path = "Assets/Resources/" + objectName + ".obj";
        string eachLine;

        using (StreamReader streamReader = new StreamReader(path))
        {
            while (streamReader.Peek() >= 0) // Reference for checking if current line has data: https://learn.microsoft.com/en-us/dotnet/api/system.io.streamreader.readline?view=net-7.0
            {
                eachLine = streamReader.ReadLine();
                

                string[] splitValues = eachLine.Split(" "); // Reference for splitting line: https://learn.microsoft.com/en-us/dotnet/csharp/how-to/parse-strings-using-split
                string typeOfData = splitValues[0];


                if (typeOfData == "vn")
                {
                    vertexNormals.Add(new Vector3(float.Parse(splitValues[1]), float.Parse(splitValues[2]), float.Parse(splitValues[3])));
                }


                else if (typeOfData == "v")
                {
                    vertices.Add(new Vector3(float.Parse(splitValues[1]), float.Parse(splitValues[2]), float.Parse(splitValues[3])));
                }

                else if (typeOfData == "f")
                {
                    string[] splitFaceFirstVtx = splitValues[1].Split("/");
                    string[] splitFaceSecondVtx = splitValues[2].Split("/");
                    string[] splitFaceThirdVtx = splitValues[3].Split("/");

                    // Vertex Index 1 in original obj file triangle face data values has to be changed to index 0,
                    // since all lists (eg. the vertices list and vertexNormal list) starts from index 0, not index 1.
                    // So need to -1 from original indices in triangle face data (the rows for face in the data file).
                    int faceVtxIndex1 = int.Parse(splitFaceFirstVtx[0]) - 1;
                    int faceVtxNormIndex1 = int.Parse(splitFaceFirstVtx[2]) - 1;
                    int faceVtxIndex2 = int.Parse(splitFaceSecondVtx[0]) - 1;
                    int faceVtxNormIndex2 = int.Parse(splitFaceSecondVtx[2]) - 1;
                    int faceVtxIndex3 = int.Parse(splitFaceThirdVtx[0]) - 1;
                    int faceVtxNormIndex3 = int.Parse(splitFaceThirdVtx[2]) - 1;

                    // Adding Vertex Indices for Triangle Faces
                    triangles.Add(faceVtxIndex1);
                    triangles.Add(faceVtxIndex2);
                    triangles.Add(faceVtxIndex3);

                    // Adding tuples of vertex index and corresponding vertex normal index
                    listOfTuples.Add( (faceVtxIndex1, faceVtxNormIndex1) );
                    listOfTuples.Add( (faceVtxIndex2, faceVtxNormIndex2) );
                    listOfTuples.Add( (faceVtxIndex3, faceVtxNormIndex3) );

                }
            }
        }

        

        // Sort to put duplicates together.
        listOfTuples.Sort((tuple1, tuple2) => tuple1.vertexIndex.CompareTo(tuple2.vertexIndex));  // Reference to sort list of tuples: https://stackoverflow.com/questions/4668525/sort-listtupleint-int-in-place

        // After Extracting File Data, to rearrange vertex normals.
        int prevVtxIdx = -1, prevVtxNormIdx = -1;
        for (int i = 0; i < listOfTuples.Count; i++)
        {
 

            int currentVtxIdx = listOfTuples[i].vertexIndex;
            int currentVtxNormIdx = listOfTuples[i].vertexNormIndex;


            if (currentVtxIdx != prevVtxIdx) {
                // To match the vertex normals to the correct vertex
                // where vertexNormal list index 0 would be the correct vertex normal for vertices list index 0 vertex.

                // Useful Reference if need validation: https://stackoverflow.com/questions/2094239/swap-two-items-in-listt
                Vector3 tempForSwap = vertexNormals[currentVtxIdx];
                vertexNormals[currentVtxIdx] = vertexNormals[currentVtxNormIdx];
                vertexNormals[currentVtxNormIdx] = tempForSwap;

                prevVtxIdx = currentVtxIdx;
                prevVtxNormIdx = currentVtxNormIdx;
            }

        }

    }

    void UpdateGlobeProperties()
    {
        snowGlobe.Clear();

        // Reference to convert list to object array: https://stackoverflow.com/questions/782096/convert-listt-to-object
        Vector3[] convertedVerticesArray = vertices.Cast<Vector3>().ToArray();
        Vector3[] convertedVertexNormalArray = vertexNormals.Cast<Vector3>().ToArray();
        int[] convertedTrianglesArray = triangles.Cast<int>().ToArray();
        Color32[] globeTransparencyColors = new Color32[vertices.Count];


        for(int i = 0; i < vertices.Count; i++)
        {
            globeTransparencyColors[i] = new Color32(1,1,1,0);
        }


        snowGlobe.vertices = convertedVerticesArray;
        snowGlobe.triangles = convertedTrianglesArray;
        snowGlobe.colors32 = globeTransparencyColors;
        // snowGlobe.normals = convertedVertexNormalArray;
        snowGlobe.RecalculateNormals();
    }

    void SetMeshRendererProperties()
    {
        // To set material properties of mesh, eg. metallic and colors, MUST MODIFY RENDERER COMPONENT ADDED ABOVE,
        // NOT THE MESH OBJECT (snowGlobe) ITSELF!
        // So do rendererName.material.SetFloat("_NameOfSectionToChangeBasedOnLink_", value).
        // See third post of link for all sections that could be change for material properties.
        // Reference: https://forum.unity.com/threads/set-smoothness-of-material-in-script.381247/
        gameObjectMeshRenderer.material.SetFloat("_Metallic", materialMetallicVal);
        gameObjectMeshRenderer.material.SetFloat("_Glossiness", materialGlossyVal);
        gameObjectMeshRenderer.material.SetColor("_Color", new Color32(255, 255, 255, 0));      // Must change color too, else no transparency if alpha channel not 0.
        gameObjectMeshRenderer.material.SetFloat("_Mode", 3);                                   // Setting "Rendering Mode" to transparent.

        // Reference from SetupMaterialWithBlendMode script: https://forum.unity.com/threads/standard-material-shader-ignoring-setfloat-property-_mode.344557/
        // Then can directly set materials without using Unity Editor to set transparency material.
        gameObjectMeshRenderer.material.SetOverrideTag("RenderType", "Transparent");            
        gameObjectMeshRenderer.material.renderQueue = 3000;
        gameObjectMeshRenderer.material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
        gameObjectMeshRenderer.material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        gameObjectMeshRenderer.material.SetInt("_ZWrite", 0);
        gameObjectMeshRenderer.material.DisableKeyword("_ALPHATEST_ON");
        gameObjectMeshRenderer.material.DisableKeyword("_ALPHABLEND_ON");
        gameObjectMeshRenderer.material.EnableKeyword("_ALPHAPREMULTIPLY_ON");
    }
}
