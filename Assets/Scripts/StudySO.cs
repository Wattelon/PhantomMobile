using UnityEngine;

public class StudySO : ScriptableObject
{
    [SerializeField] private Texture3D studyTexture;
    [SerializeField] private float rescaleSlope;
    [SerializeField] private float rescaleIntercept;
    [SerializeField] private float sliceThickness;
    [SerializeField] private float pixelSpacingRow;
    [SerializeField] private float pixelSpacingColumn;
    [SerializeField] private string modality;
    
    public Texture3D StudyTexture => studyTexture;
    public float RescaleSlope => rescaleSlope;
    public float RescaleIntercept => rescaleIntercept;
    public float SliceThickness => sliceThickness;
    public float PixelSpacingRow => pixelSpacingRow;
    public float PixelSpacingColumn => pixelSpacingColumn;
    public string Modality => modality;

    public void Initialize(Texture3D studyTexture, float rescaleSlope, float rescaleIntercept,  float sliceThickness, float pixelSpacingRow, float pixelSpacingColumn, string modality)
    {
        this.studyTexture = studyTexture;
        this.rescaleSlope = rescaleSlope;
        this.rescaleIntercept = rescaleIntercept;
        this.sliceThickness = sliceThickness;
        this.pixelSpacingRow = pixelSpacingRow;
        this.pixelSpacingColumn = pixelSpacingColumn;
        this.modality = modality;
    }
}