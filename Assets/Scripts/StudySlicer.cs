using System;
using System.Collections;
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Rendering;

public class StudySlicer : MonoBehaviour
{
    [SerializeField] private Axis currentAxis;
    [SerializeField][Range(0, 511)] private int currentIndex;
    [SerializeField] private Vector3 dimensions;
    [SerializeField] private List<Transform> pivotPoints;
    [SerializeField] private bool filterBrightness;
    [SerializeField][Range(0, 1)] private float minBrightness;
    [SerializeField][Range(0, 1)] private float maxBrightness = 1;
    [SerializeField] private StudySO studySO;

    private MeshRenderer _meshRenderer;
    private Material _material;
    private LocalKeyword[] _axisKeywords = new LocalKeyword[3];
    private bool _isUpdateSliceDelayed;
    
    private static readonly int StudyTexturePropertyID = Shader.PropertyToID("_StudyTexture");
    private static readonly int SlicePropertyID = Shader.PropertyToID("_Slice");
    private static readonly int MinBrightnessPropertyID = Shader.PropertyToID("_MinBrightness");
    private static readonly int MaxBrightnessPropertyID = Shader.PropertyToID("_MaxBrightness");

    public int TextureWidth => studySO.StudyTexture.width;
    public int TextureHeight => studySO.StudyTexture.height;
    public int TextureDepth => studySO.StudyTexture.depth;
    public Axis CurrentAxis => currentAxis;

    private void Awake()
    {
        var meshRenderer = GetComponent<MeshRenderer>();
        _material = new Material(meshRenderer.material);
        _material.SetTexture(StudyTexturePropertyID, studySO.StudyTexture);
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
        }
        transform.localPosition = Vector3.forward * (currentIndex * step);*/
        
        switch (currentAxis)
        {
            case Axis.Axial:
                transform.localRotation = Quaternion.LookRotation(Vector3.Cross(studySO.OrientationVectorX, studySO.OrientationVectorY), -studySO.OrientationVectorY);
                transform.localPosition = transform.forward * ((-0.5f * TextureDepth + currentIndex) * studySO.SliceThickness / 1000);
                transform.localScale = new Vector3(studySO.PixelSpacingColumn * TextureWidth / 1000, studySO.PixelSpacingRow * TextureHeight / 1000, 1);
                _material.SetFloat(SlicePropertyID, (float)currentIndex / TextureDepth);
                break;
            case Axis.Sagittal:
                transform.localRotation = Quaternion.LookRotation(Vector3.right, Vector3.forward);
                transform.localPosition = transform.forward * ((-0.5f * TextureWidth + currentIndex) * studySO.PixelSpacingColumn / 1000);
                transform.localScale = new Vector3(studySO.SliceThickness * TextureDepth / 1000, studySO.PixelSpacingRow * TextureHeight / 1000, 1);
                _material.SetFloat(SlicePropertyID, (float)currentIndex / TextureWidth);
                break;
            case Axis.Coronal:
                transform.localRotation = Quaternion.LookRotation(Vector3.up, Vector3.forward);
                transform.localPosition = transform.forward * ((-0.5f * TextureHeight + currentIndex) * studySO.PixelSpacingRow / 1000);
                transform.localScale = new Vector3(studySO.PixelSpacingColumn * TextureWidth / 1000, studySO.SliceThickness * TextureDepth / 1000, 1);
                _material.SetFloat(SlicePropertyID, (float)currentIndex / TextureHeight);
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
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