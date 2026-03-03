using UnityEngine;

public class VolumeRenderer : MonoBehaviour
{
    private Renderer _renderer;
    private Material _material;
    private ImagingSO _imagingSO;
    
    private static readonly int Alpha = Shader.PropertyToID("_Alpha");
    private static readonly int AlphaThreshold = Shader.PropertyToID("_AlphaThreshold");
    private static readonly int StepSize = Shader.PropertyToID("_StepSize");

    private void Awake()
    {
        _renderer = GetComponent<Renderer>();
        _material = _renderer.material;
    }

    private void OnEnable()
    {
        Menu.VolumeAlphaChanged += OnVolumeAlphaChanged;
        Menu.VolumeAlphaThresholdChanged += OnVolumeAlphaThresholdChanged;
        Menu.VolumeStepSizeChanged += OnVolumeStepSizeChanged;
    }

    private void OnDisable()
    {
        Menu.VolumeAlphaChanged -= OnVolumeAlphaChanged;
        Menu.VolumeAlphaThresholdChanged -= OnVolumeAlphaThresholdChanged;
        Menu.VolumeStepSizeChanged -= OnVolumeStepSizeChanged;
    }

    private void OnVolumeAlphaChanged(float alpha)
    {
        _material.SetFloat(Alpha, alpha);
    }

    private void OnVolumeAlphaThresholdChanged(float alphaThreshold)
    {
        _material.SetFloat(AlphaThreshold, alphaThreshold);
    }

    private void OnVolumeStepSizeChanged(float stepSize)
    {
        _material.SetFloat(StepSize, stepSize);
    }

    private void OnVolumeChanged(ImagingSO imagingSO)
    {
        _imagingSO = imagingSO;
        _material.mainTexture = imagingSO.StudyTexture;
        transform.localScale = imagingSO.ScalingVector;
    }
}