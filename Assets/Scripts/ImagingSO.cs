using System;
using UnityEngine;

[Serializable]
public class ImagingSO : ScriptableObject
{
    [SerializeField] private Texture3D studyTexture;
    [SerializeField] private byte[] pixels;
    [SerializeField] private int width;
    [SerializeField] private int height;
    [SerializeField] private int depth;
    [SerializeField] private string seriesUID;
    [SerializeField] private string modality;
    [SerializeField] private float rescaleSlope;
    [SerializeField] private float rescaleIntercept;
    [SerializeField] private float spacingBetweenSlices;
    [SerializeField] private float pixelSpacingRow;
    [SerializeField] private float pixelSpacingColumn;
    [SerializeField] private Vector3 orientationVectorX;
    [SerializeField] private Vector3 orientationVectorY;
    [SerializeField] private Vector3 scalingVector;
    
    public Texture3D StudyTexture => studyTexture;
    public byte[] Pixels => pixels;
    public int Width => width;
    public int Height => height;
    public int Depth => depth;
    public float RescaleSlope => rescaleSlope;
    public float RescaleIntercept => rescaleIntercept;
    public float SpacingBetweenSlices => spacingBetweenSlices;
    public float PixelSpacingRow => pixelSpacingRow;
    public float PixelSpacingColumn => pixelSpacingColumn;
    public string Modality => modality;
    public Vector3 OrientationVectorX => orientationVectorX;
    public Vector3 OrientationVectorY => orientationVectorY;
    public Vector3 ScalingVector => scalingVector;

    public void Initialize(string seriesUID, Texture3D studyTexture, byte[] pixels, int width, int height, int depth, float rescaleSlope, float rescaleIntercept,  float spacingBetweenSlices, float pixelSpacingRow, float pixelSpacingColumn, string modality, Vector3 orientationVectorX, Vector3 orientationVectorY, Vector3 scalingVector)
    {
        this.seriesUID = seriesUID;
        this.studyTexture = studyTexture;
        this.pixels = pixels;
        this.width = width;
        this.height = height;
        this.depth = depth;
        this.rescaleSlope = rescaleSlope;
        this.rescaleIntercept = rescaleIntercept;
        this.spacingBetweenSlices = spacingBetweenSlices;
        this.pixelSpacingRow = pixelSpacingRow;
        this.pixelSpacingColumn = pixelSpacingColumn;
        this.modality = modality;
        this.orientationVectorX = orientationVectorX;
        this.orientationVectorY = orientationVectorY;
        this.scalingVector = scalingVector;
    }
}