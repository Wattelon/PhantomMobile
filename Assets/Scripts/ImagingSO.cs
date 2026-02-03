using UnityEngine;

public class ImagingSO : ScriptableObject
{
    [SerializeField] private Texture3D studyTexture;
    [SerializeField] private float rescaleSlope;
    [SerializeField] private float rescaleIntercept;
    [SerializeField] private float sliceThickness;
    [SerializeField] private float pixelSpacingRow;
    [SerializeField] private float pixelSpacingColumn;
    [SerializeField] private string modality;
    [SerializeField] private Vector3 orientationVectorX;
    [SerializeField] private Vector3 orientationVectorY;
    
    public Texture3D StudyTexture => studyTexture;
    public float RescaleSlope => rescaleSlope;
    public float RescaleIntercept => rescaleIntercept;
    public float SliceThickness => sliceThickness;
    public float PixelSpacingRow => pixelSpacingRow;
    public float PixelSpacingColumn => pixelSpacingColumn;
    public string Modality => modality;
    public Vector3 OrientationVectorX => orientationVectorX;
    public Vector3 OrientationVectorY => orientationVectorY;

    public void Initialize(Texture3D studyTexture, float rescaleSlope, float rescaleIntercept,  float sliceThickness, float pixelSpacingRow, float pixelSpacingColumn, string modality, Vector3 orientationVectorX, Vector3 orientationVectorY)
    {
        this.studyTexture = studyTexture;
        this.rescaleSlope = rescaleSlope;
        this.rescaleIntercept = rescaleIntercept;
        this.sliceThickness = sliceThickness;
        this.pixelSpacingRow = pixelSpacingRow;
        this.pixelSpacingColumn = pixelSpacingColumn;
        this.modality = modality;
        this.orientationVectorX = orientationVectorX;
        this.orientationVectorY = orientationVectorY;
    }
}