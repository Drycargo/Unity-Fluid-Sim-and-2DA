using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CaptureRTScript : MonoBehaviour
{
    public RenderTexture capturedTexture;
    public RenderTexture colorOutput;
    public Material colorOutputMat;
    public ComputeShader computeShader;

    public int velWidth = 2048;

    private RenderTexture oldColor;
    private RenderTexture velField;

    void Start()
    {
        oldColor = new RenderTexture(2048, 2048, 0);
        oldColor.enableRandomWrite = true;
        oldColor.Create();

        velField = new RenderTexture(velWidth, velWidth, 0, RenderTextureFormat.RGHalf);
        velField.enableRandomWrite = true;
        velField.Create();


        computeShader.SetTexture(0, "OutputVel", velField);
        computeShader.SetTexture(0, "Input", capturedTexture);
        computeShader.SetFloat("FORCE_AMP", 10);

        //colorOutputMat.SetTexture("_NewTex", capturedTexture);
        colorOutputMat.SetTexture("_VelocityField", velField);
    }

    // Update is called once per frame
    void Update()
    {
        //Graphics.CopyTexture(colorOutput, oldColor);
        computeShader.Dispatch(0, velWidth / 8, velWidth / 8, 1);
        Graphics.Blit(capturedTexture, colorOutput ,colorOutputMat);
    }

    void OnDestroy() {
        Destroy(oldColor);
        Destroy(velField);
    }
}
