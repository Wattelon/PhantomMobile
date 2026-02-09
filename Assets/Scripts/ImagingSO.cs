using UnityEngine;

public class ImagingSO : ScriptableObject
{
    [SerializeField] private Texture3D studyTexture;
    [SerializeField] private string seriesUID;
    [SerializeField] private string modality;
    [SerializeField] private float rescaleSlope;
    [SerializeField] private float rescaleIntercept;
    [SerializeField] private float spacingBetweenSlices;
    [SerializeField] private float pixelSpacingRow;
    [SerializeField] private float pixelSpacingColumn;
    [SerializeField] private Vector3 orientationVectorX;
    [SerializeField] private Vector3 orientationVectorY;
    
    public Texture3D StudyTexture => studyTexture;
    public float RescaleSlope => rescaleSlope;
    public float RescaleIntercept => rescaleIntercept;
    public float SpacingBetweenSlices => spacingBetweenSlices;
    public float PixelSpacingRow => pixelSpacingRow;
    public float PixelSpacingColumn => pixelSpacingColumn;
    public string Modality => modality;
    public Vector3 OrientationVectorX => orientationVectorX;
    public Vector3 OrientationVectorY => orientationVectorY;

    public void Initialize(string seriesUID, Texture3D studyTexture, float rescaleSlope, float rescaleIntercept,  float spacingBetweenSlices, float pixelSpacingRow, float pixelSpacingColumn, string modality, Vector3 orientationVectorX, Vector3 orientationVectorY)
    {
        this.seriesUID = seriesUID;
        this.studyTexture = studyTexture;
        this.rescaleSlope = rescaleSlope;
        this.rescaleIntercept = rescaleIntercept;
        this.spacingBetweenSlices = spacingBetweenSlices;
        this.pixelSpacingRow = pixelSpacingRow;
        this.pixelSpacingColumn = pixelSpacingColumn;
        this.modality = modality;
        this.orientationVectorX = orientationVectorX;
        this.orientationVectorY = orientationVectorY;
    }
}