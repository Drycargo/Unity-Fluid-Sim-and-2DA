using System.Collections.Generic;
using System.Numerics;
using UnityEngine;
using Vector2 = UnityEngine.Vector2;
using Vector3 = UnityEngine.Vector3;

public class FluidScript : MonoBehaviour
{
    // Start is called before the first frame update

    // Resolution & threads
    public int resolution = 2048;

    public int threadCountX {
        get {return (resolution + 7) / 8;}
    }

    public int threadCountY {
        get {return (resolution + 7) / 8;}
    }

    public int resX {
        get {return threadCountX * 8;}
    }

    public int resY {
        get {return threadCountY * 8;}
    }

    // Public fields
    public Texture2D initialTex;
    public ComputeShader currentShader;
    public Material currentMaterial;
    public Material targetMaterial;

    public List<GameObject> colliders;

    // Simulation Consts
    public float viscosity = 1e-6f;
    public int linSolveIteration = 20;
    public float vorticity = 15f;

    private Vector2 previousPos = Vector2.zero;

    // RT
    private RenderTexture velRT0;

    private RenderTexture velRT1;
    private RenderTexture velRT2;

    private RenderTexture pressureRT0;
    private RenderTexture pressureRT1;
    private RenderTexture velDivRT0;
    private RenderTexture velCurlRT0;

    private RenderTexture colorRT0;
    private RenderTexture colorRT1;
    private RenderTexture colorRT2;

    // Shader Kernels
    private int advectKernel;
    private int forceKernel;
    private int diffuse2DKernel;
    private int diffuse1DKernel;
    private int projectKernel;
    private int projectSetupKernel;
    private int curlKernel;
    private int vorticityKernel;

    void Start()
    {
        InitTextures();
        InitShaders();

        //colliders = new List<GameObject>();
        Graphics.Blit(initialTex, colorRT1);
    }

    // Update is called once per frame
    void Update()
    {
        float dt = Time.deltaTime;
        float dx = 1.0f / resY;

        currentShader.SetFloat("DELTA_T", dt);
        currentShader.SetFloat("worldTime", Time.time);

        // Start Operation
        // Advect Vel: Vel0 --Advect--> Vel1
        currentShader.Dispatch(advectKernel, threadCountX, threadCountY, 1);

        // Diffuse Vel:
        // -- Vel1 + Vel0 --Diffuse--> Vel2
        // -- Vel2 + Vel0 --Diffuse--> Vel1
        float diffuseA = dx * dx / (viscosity * dt);
        currentShader.SetFloat("DIFF_A", diffuseA);
        currentShader.SetFloat("DIFF_B", 4 + diffuseA);
        Graphics.CopyTexture(velRT1, velRT0);
        // --linear solve
        linsolve(diffuse2DKernel, "Diff2DSrc", "Diff2DDest", velRT1, velRT2);

        // TEMP FORCE
        // Vel1 --Force--> Vel2
        Vector3 colliderPos = colliders[0].transform.position;
        Vector2 input = new Vector2(colliderPos.x/16f, colliderPos.z/10f);
        currentShader.SetVector("forcePos", input);
        currentShader.SetVector("forceVector", input - previousPos);
        currentShader.Dispatch(forceKernel, threadCountX, threadCountY, 1);

        // Curl
        // Vel2 --Curl--> vCurl0
        currentShader.Dispatch(curlKernel, threadCountX, threadCountY, 1);

        // Vorticity
        // Vel2 + vCurl0 --Vorticity--> Vel1
        currentShader.Dispatch(vorticityKernel, threadCountX, threadCountY, 1);

        // Setup Project
        // Vel1 --ProjectSetup--> VelDiv0 (velocity divergence) + Pressure0
        currentShader.Dispatch(projectSetupKernel, threadCountX, threadCountY, 1);
    
        // Diffuse Pressure
        // -- Pressure0 + VelDivergence0 --Diffuse--> Pressure1
        // -- Pressure1 + VelDivergence0 --Diffuse--> Pressure0
        currentShader.SetFloat("DIFF_A", - dx * dx);
        currentShader.SetFloat("DIFF_B", 4);
    
        linsolve(diffuse1DKernel, "Diff1DSrc", "Diff1DDest", pressureRT0, pressureRT1);
    
        // Complete Project
        // -- Vel1 + vDiv0 --Project--> Vel0
        currentShader.Dispatch(projectKernel, threadCountX, threadCountY, 1);

        // Draw on material
        Graphics.CopyTexture(colorRT0, colorRT2);

        // Diffuse Color
        /*
        currentMaterial.SetFloat("DIFF_A", diffuseA);
        currentMaterial.SetFloat("DIFF_B", 4 + diffuseA);
        currentMaterial.SetTexture("_OrigTex", colorRT2);

        for (int i = 0; i < linSolveIteration; i++) {
            Graphics.Blit(colorRT0, colorRT1, currentMaterial, 0);
            Graphics.Blit(colorRT1, colorRT0, currentMaterial, 0);
        }
        */

        // Advect Color
        Graphics.Blit(colorRT0, colorRT1, currentMaterial, 1);

        RenderTexture temp = colorRT1;
        colorRT1 = colorRT0;
        colorRT0 = temp;

        targetMaterial.SetTexture("_MainTex", colorRT0);
        previousPos = input;
    }

    void OnRenderImage(RenderTexture src, RenderTexture dest) {
        Graphics.Blit(colorRT1, dest, currentMaterial, 1);
    }

    void OnDestroy() {
        Destroy(velRT0);
        Destroy(velRT1);
        Destroy(velRT2);
        Destroy(pressureRT0);
        Destroy(pressureRT1);
        Destroy(velDivRT0);
        Destroy(velCurlRT0);
        Destroy(colorRT0);
        Destroy(colorRT1);
        Destroy(colorRT2);
    }

    // ===== Helpers =====
    private void linsolve(int diffuseKernel, string sourceName, string destName, RenderTexture rt1, RenderTexture rt2) {
        for (int i = 0; i < linSolveIteration; i++) {
            currentShader.SetTexture(diffuseKernel, sourceName, rt1);
            currentShader.SetTexture(diffuseKernel, destName, rt2);
            currentShader.Dispatch(diffuseKernel, threadCountX, threadCountY, 1);

            currentShader.SetTexture(diffuseKernel, sourceName, rt2);
            currentShader.SetTexture(diffuseKernel, destName, rt1);
            currentShader.Dispatch(diffuseKernel, threadCountX, threadCountY, 1);
        }
    }
    private void InitTextures() {
        velRT0 = CreateRT(2, resX, resY);
        velRT1 = CreateRT(2, resX, resY);
        velRT2 = CreateRT(2, resX, resY);

        pressureRT0 = CreateRT(1, resX, resY);
        pressureRT1 = CreateRT(1, resX, resY);

        velDivRT0 = CreateRT(1, resX, resY);
        velCurlRT0 = CreateRT(1, resX, resY);

        colorRT0 = CreateRT(4, resX, resY);
        colorRT1 = CreateRT(4, resX, resY);
        colorRT2 = CreateRT(4, resX, resY);
    }

    private void InitShaders() {
        advectKernel = currentShader.FindKernel("Advect");
        forceKernel = currentShader.FindKernel("Force");
        diffuse2DKernel = currentShader.FindKernel("Diffuse2D");
        diffuse1DKernel = currentShader.FindKernel("Diffuse1D");
        projectKernel = currentShader.FindKernel("Project");
        projectSetupKernel = currentShader.FindKernel("ProjectSetup");
        curlKernel = currentShader.FindKernel("Curl");
        vorticityKernel = currentShader.FindKernel("Vorticity");

        // Set inputs and outputs

        // Vel0 --Advect--> Vel1
        currentShader.SetTexture(advectKernel, "VelSrc", velRT0);
        currentShader.SetTexture(advectKernel, "AdvectOutDest", velRT1);

        // _ + Vel0 --Diffuse--> _
        // -- 1 + 0 -> 2; 2 + 0 -> 1
        currentShader.SetTexture(diffuse2DKernel, "Diff2DOrigSrc", velRT0);

        // Vel1 --Force--> Vel2
        currentShader.SetTexture(forceKernel, "AdvectOutSrc", velRT1);
        currentShader.SetTexture(forceKernel, "AdvectOutDest", velRT2);

        // Vel2 --Curl--> vCurl0
        currentShader.SetTexture(curlKernel, "AdvectOutSrc", velRT2);
        currentShader.SetTexture(curlKernel, "VelCurl", velCurlRT0);

        // Vel2 + vCurl0 --Vorticity--> Vel1
        currentShader.SetTexture(vorticityKernel, "AdvectOutSrc", velRT2);
        currentShader.SetTexture(vorticityKernel, "VelCurl", velCurlRT0);
        currentShader.SetTexture(vorticityKernel, "AdvectOutDest", velRT1);

        // Vel1 --ProjectSetup--> vDiv1 (velocity divergence) + Pressure0
        currentShader.SetTexture(projectSetupKernel, "AdvectOutSrc", velRT1); //
        currentShader.SetTexture(projectSetupKernel, "VelDivergence", velDivRT0);
        currentShader.SetTexture(projectSetupKernel, "PressureSrc", pressureRT0);

        // _ + VelDivergence --Diffuse--> _
        // Diffuse pressure with divergence
        // -- p0 + vDiv0 -> p1; p1 + vDiv0 -> p0
        currentShader.SetTexture(diffuse1DKernel, "Diff1DOrigSrc", velDivRT0);

        // Vel1 + vDiv0 --Project--> Vel0
        currentShader.SetTexture(projectKernel, "AdvectOutSrc", velRT1); //
        currentShader.SetTexture(projectKernel, "PressureSrc", pressureRT0);
        currentShader.SetTexture(projectKernel, "VelDest", velRT0);

        currentShader.SetFloat("CURL", vorticity);
        currentShader.SetFloat("FORCE_DECAY", 75);
        currentShader.SetFloat("FORCE_AMPLITUDE", 5);
        currentMaterial.SetTexture("_VelocityField", velRT0);
    }

    private RenderTexture CreateRT(int pixelDimension, int width = -1, int height = -1) {
        width = (width == -1) ? resX : width;
        height = (height == -1) ? resY : height;

        RenderTextureFormat RTFormat;
        switch(pixelDimension) {
            case 1: {
                RTFormat = RenderTextureFormat.RHalf;
                break;
            }
            case 2: {
                RTFormat = RenderTextureFormat.RGHalf;
                break;
            }
            case 4: {
                RTFormat = RenderTextureFormat.ARGBHalf;
                break;
            }
            default: {
                RTFormat = RenderTextureFormat.ARGBHalf;
                break;
            }
        }

        RenderTexture renderTexture = new RenderTexture(width, height, 0, RTFormat);
        renderTexture.enableRandomWrite = true;
        renderTexture.Create();

        return renderTexture;
    }
}
