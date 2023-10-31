using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Tracing;
using UnityEngine;
using UnityEngine.UI;

public class TestComputeShaderCS : MonoBehaviour
{
    // Start is called before the first frame update

    public const int RESOLUTION = 256;
    public const int CUBE_COUNT = 5;
    public Mesh mesh;
    public Material material;
    public ComputeShader computeShader;
    public RenderTexture renderTexture;

    private float counter = 0;

    public struct Cube {
        public Vector3 position;
        public Color color;
    }

    private List<GameObject> cubeObjs = null;
    private Cube[] cubeData;

    void Start()
    {
        initializeRT();

        
        computeShader.SetTexture(0, "Result", renderTexture);
        computeShader.SetFloat("resolution", RESOLUTION);
        
        //CreateCubes();
    }

    private void initializeRT()
    {
        renderTexture = new RenderTexture(RESOLUTION, RESOLUTION, 24);
        renderTexture.enableRandomWrite = true;
        renderTexture.Create();
    }

    public void CreateCubes() {
        /*
        if (cubeObjs != null)
            return;
        */
        cubeObjs = new List<GameObject>();
        cubeData = new Cube[CUBE_COUNT * CUBE_COUNT];

        for (int i = 0; i < CUBE_COUNT; i++) {
            for (int j = 0; j < CUBE_COUNT; j++) {
                CreateCube(i, j);
            }
        }
    }

    private void CreateCube(int x, int y)
    {
        GameObject newCube = new GameObject("Cube " + (x * CUBE_COUNT + y), typeof(MeshFilter), typeof(MeshRenderer));
        newCube.GetComponent<MeshFilter>().mesh = mesh;
        newCube.GetComponent<MeshRenderer>().material = new Material(material);
        newCube.transform.position = new Vector3(x, y, UnityEngine.Random.Range(-0.1f, 0.1f));

        Color c = UnityEngine.Random.ColorHSV();
        newCube.GetComponent<MeshRenderer>().material.SetColor("_Color", c);

        cubeObjs.Add(newCube);

        // Into Buffer
        Cube newBufferCube = new Cube();
        newBufferCube.position = newCube.transform.position;
        newBufferCube.color = c;
        cubeData[x * CUBE_COUNT + y] = newBufferCube;
    }

    public void OnRandomizeGPU() {
        int dataBlockSize = sizeof(float) * (3 + 4);

        ComputeBuffer computeBuffer = new ComputeBuffer(CUBE_COUNT * CUBE_COUNT, dataBlockSize);
        computeBuffer.SetData(cubeData);

        computeShader.SetBuffer(0, "cubes", computeBuffer);
        computeShader.SetFloat("resolution", cubeData.Length);

        computeShader.Dispatch(0, cubeData.Length / 10, 1, 1);

        computeBuffer.GetData(cubeData);

        for (int i = 0; i < cubeObjs.Count; i++) {
            GameObject cubeObj = cubeObjs[i];
            Cube cubeDatum = cubeData[i];
            cubeObj.transform.position = cubeDatum.position;
            cubeObj.GetComponent<MeshRenderer>().material.SetColor("_Color" ,cubeDatum.color);
        }

        computeBuffer.Dispose();
    }

    // Update is called once per frame
    void Update()
    {
        /*
        if (UnityEngine.Random.Range(0, 1) < 0.1f)
            OnRandomizeGPU();
        */

        computeShader.SetFloat("t", counter);
        computeShader.Dispatch(0, renderTexture.width / 8, renderTexture.height / 8, 1);
        counter++;
        material.SetTexture("_MainTex", renderTexture);
    }
    /*
    void OnRenderImage(RenderTexture src, RenderTexture dest) {
        if (renderTexture == null) {
            initializeRT();
        }

        computeShader.SetTexture(0, "Result", renderTexture);
        computeShader.Dispatch(0, renderTexture.width / 8, renderTexture.height / 8, 1);

        Graphics.Blit(renderTexture, dest);
    }
    */
}
