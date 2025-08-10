using System;
using UnityEngine;
using System.Collections.Generic;

public class TomographySlicer : MonoBehaviour
{
    [SerializeField] private TomographySO tomographyData;
    [SerializeField] private Tomography currentTomography;
    [SerializeField] private Axis currentAxis;
    [SerializeField][Range(0, 511)] private int currentIndex;
    [SerializeField] private Vector3 dimensions;
    [SerializeField] private List<Transform> pivotPoints;
    [SerializeField] private bool filterBrightness;
    [SerializeField][Range(0, 1)] private float minBrightness;
    [SerializeField][Range(0, 1)] private float maxBrightness = 1;

    [SerializeField] private List<Texture2D> slicesAxial;
    [SerializeField] private List<Texture2D> slicesSagittal;
    [SerializeField] private List<Texture2D> slicesCoronal;

    private MeshRenderer _meshRenderer;

    private void Awake()
    {
        _meshRenderer = GetComponent<MeshRenderer>();
    }

    private void OnEnable()
    {
        UpdateSlice();
    }

    private void Start()
    {
        tomographyData.Slices.Add(Axis.Axial, slicesAxial);
        tomographyData.Slices.Add(Axis.Sagittal, slicesSagittal);
        tomographyData.Slices.Add(Axis.Coronal, slicesCoronal);
    }

    private void UpdateSlice()
    {
        var texture = tomographyData.Slices[currentAxis][currentIndex];
        if (filterBrightness) texture = FilterBrightness(texture);
        _meshRenderer.material.mainTexture = texture;

        transform.SetParent(pivotPoints[(int)currentAxis], false);
        transform.localPosition = Vector3.zero;
        transform.localRotation = Quaternion.identity;

        var dimension = ((int)currentAxis + 2) % 3;
        var step = dimensions[dimension] / tomographyData.Slices[currentAxis].Count;
        transform.localPosition = Vector3.forward * (currentIndex * step);
    }

    private Texture2D FilterBrightness(Texture2D texture)
    {
        var filteredTexture = new Texture2D(texture.width, texture.height, TextureFormat.RGBA64, false);
        filteredTexture.CopyPixels(texture);
        
        for (var i = 0; i < filteredTexture.width; i++)
        {
            for (var j = 0; j < filteredTexture.height; j++)
            {
                if (filteredTexture.GetPixel(i, j).r < maxBrightness && filteredTexture.GetPixel(i, j).r > minBrightness) continue;
                filteredTexture.SetPixel(i, j, Color.black);
            }
        }
        
        filteredTexture.Apply();
        return filteredTexture;
    }

    public void SetAxis(int axis)
    {
        currentAxis = (Axis)axis;
        UpdateSlice();
    }

    public void SetIndex(float index)
    {
        currentIndex = (int)index;
        UpdateSlice();
    }

    public void SetMinBrightness(float value)
    {
        minBrightness = value;
        UpdateSlice();
    }

    public void SetMaxBrightness(float value)
    {
        maxBrightness = value;
        UpdateSlice();
    }
}

public enum Tomography
{
    Computer,
    MagneticResonance
}

public enum Axis
{
    Axial,
    Sagittal,
    Coronal
}