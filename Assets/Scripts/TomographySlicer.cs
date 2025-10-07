using System;
using System.Collections;
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Rendering;

public class TomographySlicer : MonoBehaviour
{
    [SerializeField] private Axis currentAxis;
    [SerializeField][Range(0, 511)] private int currentIndex;
    [SerializeField] private Vector3 dimensions;
    [SerializeField] private List<Transform> pivotPoints;
    [SerializeField] private bool filterBrightness;
    [SerializeField][Range(0, 1)] private float minBrightness;
    [SerializeField][Range(0, 1)] private float maxBrightness = 1;
    [SerializeField] private Texture3D tomographyTexture;

    private MeshRenderer _meshRenderer;
    private Material _material;
    private LocalKeyword[] _axisKeywords = new LocalKeyword[3];
    private bool _isUpdateSliceDelayed;
    
    private static readonly int TomographyTexture = Shader.PropertyToID("_TomographyTexture");
    private static readonly int Slice = Shader.PropertyToID("_Slice");
    private static readonly int MinBrightness = Shader.PropertyToID("_MinBrightness");
    private static readonly int MaxBrightness = Shader.PropertyToID("_MaxBrightness");

    public int TextureWidth => tomographyTexture.width;
    public int TextureHeight => tomographyTexture.height;
    public int TextureDepth => tomographyTexture.depth;
    public Axis CurrentAxis => currentAxis;

    private void Awake()
    {
        var meshRenderer = GetComponent<MeshRenderer>();
        _material = new Material(meshRenderer.material);
        _material.SetTexture(TomographyTexture, tomographyTexture);
        _axisKeywords[0] = _material.shader.keywordSpace.FindKeyword("_AXIS_AXIAL");
        _axisKeywords[1] = _material.shader.keywordSpace.FindKeyword("_AXIS_SAGITTAL");
        _axisKeywords[2] = _material.shader.keywordSpace.FindKeyword("_AXIS_CORONAL");
        meshRenderer.material = _material;
    }

    private void Start()
    {
        UpdateSlice();
    }

    private void UpdateSlice()
    {
        /*var texture = tomographyData.Slices[currentAxis][currentIndex];
        if (filterBrightness && (minBrightness != 0 || maxBrightness != 1)) texture = FilterBrightness(texture);
        _meshRenderer.material.mainTexture = texture;*/

        transform.SetParent(pivotPoints[(int)currentAxis], false);
        transform.localPosition = Vector3.zero;
        transform.localRotation = Quaternion.identity;

        var dimension = ((int)currentAxis + 2) % 3;
        float step;
        switch (currentAxis)
        {
            case Axis.Axial:
                step = dimensions[dimension] / TextureDepth;
                _material.SetFloat(Slice, (float)currentIndex / TextureDepth);
                break;
            case Axis.Sagittal:
                step = dimensions[dimension] / TextureWidth;
                _material.SetFloat(Slice, (float)currentIndex / TextureWidth);
                break;
            case Axis.Coronal:
                step = dimensions[dimension] / TextureHeight;
                _material.SetFloat(Slice, (float)currentIndex / TextureHeight);
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
        transform.localPosition = Vector3.forward * (currentIndex * step);
    }

    private Texture2D FilterBrightness(Texture2D texture)
    {
        var filteredTexture = new Texture2D(texture.width, texture.height, TextureFormat.RGB565, false);
        var colors = texture.GetPixels();

        for (var i = 0; i < colors.Length; i++)
        {
            if (colors[i].grayscale > maxBrightness || colors[i].grayscale < minBrightness) colors[i] = Color.black;
        }
        
        filteredTexture.SetPixels(colors);
        filteredTexture.Apply();
        return filteredTexture;
    }

    public void SetAxis(int axis)
    {
        currentAxis = (Axis)axis;
        for (int i = 0; i < 3; i++)
        {
            _material.SetKeyword(_axisKeywords[i], i == axis);
        }
        UpdateSlice();
    }

    public void SetIndex(float index)
    {
        currentIndex = (int)index;
        UpdateSlice();
        if (!_isUpdateSliceDelayed) StartCoroutine(DelayedUpdateSlice(0.1f));
    }

    public void SetMinBrightness(float value)
    {
        minBrightness = value;
        _material.SetFloat(MinBrightness, value);
        UpdateSlice();
    }

    public void SetMaxBrightness(float value)
    {
        maxBrightness = value;
        _material.SetFloat(MaxBrightness, value);
        UpdateSlice();
    }

    public void SetBrightness(float min, float max)
    {
        minBrightness = min;
        maxBrightness = max;
        _material.SetFloat(MinBrightness, min);
        _material.SetFloat(MaxBrightness, 1.75f - max);
    }

    public void SetBrightnessFilter(bool value)
    {
        filterBrightness = value;
        UpdateSlice();
    }

    private IEnumerator DelayedUpdateSlice(float delay)
    {
        _isUpdateSliceDelayed = true;
        yield return new WaitForSeconds(delay);
        _isUpdateSliceDelayed = false;
        UpdateSlice();
    }
}

public enum Axis
{
    Axial,
    Sagittal,
    Coronal
}