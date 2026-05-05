using System;
using System.Collections;
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Rendering;
using UnityEngine.Splines;

public class ImagingSlicer : MonoBehaviour
{
    [SerializeField] private Axis currentAxis;
    [SerializeField][Range(0, 511)] private int currentIndex;
    [SerializeField] private Vector3 dimensions;
    [SerializeField] private List<Transform> pivotPoints;
    [SerializeField] private bool moveAlongSpline;
    [SerializeField] private bool filterBrightness;
    [SerializeField][Range(0, 1)] private float minBrightness;
    [SerializeField][Range(0, 1)] private float maxBrightness = 1;
    [SerializeField] private ImagingSO imagingSO;

    private MeshRenderer _meshRenderer;
    private Material _material;
    private SplineAnimate _splineAnimate;
    private readonly LocalKeyword[] _axisKeywords = new LocalKeyword[3];
    private bool _isUpdateSliceDelayed;
    
    private static readonly int StudyTexturePropertyID = Shader.PropertyToID("_StudyTexture");
    private static readonly int SlicePropertyID = Shader.PropertyToID("_Slice");
    private static readonly int MinBrightnessPropertyID = Shader.PropertyToID("_MinBrightness");
    private static readonly int MaxBrightnessPropertyID = Shader.PropertyToID("_MaxBrightness");

    public int TextureWidth => imagingSO.StudyTexture.width;
    public int TextureHeight => imagingSO.StudyTexture.height;
    public int TextureDepth => imagingSO.StudyTexture.depth;
    public Axis CurrentAxis => currentAxis;

    private void Awake()
    {
        var meshRenderer = GetComponent<MeshRenderer>();
        _material = new Material(meshRenderer.material);
        _material.SetTexture(StudyTexturePropertyID, imagingSO.StudyTexture);
        _axisKeywords[0] = _material.shader.keywordSpace.FindKeyword("_AXIS_AXIAL");
        _axisKeywords[1] = _material.shader.keywordSpace.FindKeyword("_AXIS_SAGITTAL");
        _axisKeywords[2] = _material.shader.keywordSpace.FindKeyword("_AXIS_CORONAL");
        meshRenderer.material = _material;
        if (moveAlongSpline) _splineAnimate = GetComponent<SplineAnimate>();
    }

    private void Start()
    {
        UpdateSlice();
    }

    private void UpdateSlice()
    {
        /*transform.SetParent(pivotPoints[(int)currentAxis], false);
        transform.localPosition = Vector3.zero;
        transform.localRotation = Quaternion.identity;

        var dimension = ((int)currentAxis + 2) % 3;
        float step;
        switch (currentAxis)
        {
            case Axis.Axial:
                step = dimensions[dimension] / TextureDepth;
                _material.SetFloat(SlicePropertyID, (float)currentIndex / TextureDepth);
                break;
            case Axis.Sagittal:
                step = dimensions[dimension] / TextureWidth;
                _material.SetFloat(SlicePropertyID, (float)currentIndex / TextureWidth);
                break;
            case Axis.Coronal:
                step = dimensions[dimension] / TextureHeight;
                _material.SetFloat(SlicePropertyID, (float)currentIndex / TextureHeight);
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }*/
        if (moveAlongSpline)
        {
            _splineAnimate.NormalizedTime = (float)currentIndex / TextureDepth;
            _material.SetFloat(SlicePropertyID, (float)currentIndex / TextureDepth);
        }
        else switch (currentAxis)
        {
            case Axis.Axial:
                transform.localRotation = Quaternion.LookRotation(Vector3.Cross(imagingSO.OrientationVectorX, imagingSO.OrientationVectorY), imagingSO.OrientationVectorY);
                transform.localPosition = transform.localRotation * Vector3.forward * ((-0.5f * TextureDepth + currentIndex) * imagingSO.SpacingBetweenSlices / 1000);
                transform.localScale = new Vector3(imagingSO.PixelSpacingColumn * TextureWidth / 1000, imagingSO.PixelSpacingRow * TextureHeight / 1000, 1);
                _material.SetFloat(SlicePropertyID, (float)currentIndex / TextureDepth);
                break;
            case Axis.Sagittal:
                transform.localRotation = Quaternion.LookRotation(imagingSO.OrientationVectorX, Vector3.Cross(imagingSO.OrientationVectorX, imagingSO.OrientationVectorY));
                transform.localPosition = transform.localRotation * Vector3.forward * ((-0.5f * TextureWidth + currentIndex) * imagingSO.PixelSpacingColumn / 1000);
                transform.localScale = new Vector3(imagingSO.SpacingBetweenSlices * TextureDepth / 1000, imagingSO.PixelSpacingRow * TextureHeight / 1000, 1);
                _material.SetFloat(SlicePropertyID, (float)currentIndex / TextureWidth);
                break;
            case Axis.Coronal:
                transform.localRotation = Quaternion.LookRotation(imagingSO.OrientationVectorY, Vector3.Cross(imagingSO.OrientationVectorX, imagingSO.OrientationVectorY));
                transform.localPosition = transform.localRotation * Vector3.forward * ((-0.5f * TextureHeight + currentIndex) * imagingSO.PixelSpacingRow / 1000);
                transform.localScale = new Vector3(imagingSO.PixelSpacingColumn * TextureWidth / 1000, imagingSO.SpacingBetweenSlices * TextureDepth / 1000, 1);
                _material.SetFloat(SlicePropertyID, (float)currentIndex / TextureHeight);
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    public void SetAxis(int axis)
    {
        currentAxis = (Axis)axis;
        for (var i = 0; i < 3; i++)
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
        _material.SetFloat(MinBrightnessPropertyID, value);
        UpdateSlice();
    }

    public void SetMaxBrightness(float value)
    {
        maxBrightness = value;
        _material.SetFloat(MaxBrightnessPropertyID, value);
        UpdateSlice();
    }

    public void SetBrightness(float min, float max)
    {
        minBrightness = min;
        maxBrightness = max;
        _material.SetFloat(MinBrightnessPropertyID, min);
        _material.SetFloat(MaxBrightnessPropertyID, 1.75f - max);
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