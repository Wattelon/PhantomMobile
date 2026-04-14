using System.Diagnostics;
using System.IO;
using System.Linq;
using UnityEngine;
using Debug = UnityEngine.Debug;

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
        _imagingSO = ScriptableObject.CreateInstance<ImagingSO>();
    }

    private void OnEnable()
    {
        Menu.VolumeAlphaChanged += OnVolumeAlphaChanged;
        Menu.VolumeAlphaThresholdChanged += OnVolumeAlphaThresholdChanged;
        Menu.VolumeStepSizeChanged += OnVolumeStepSizeChanged;
        Menu.VolumeChanged += OnVolumeChanged;
    }

    private void OnDisable()
    {
        Menu.VolumeAlphaChanged -= OnVolumeAlphaChanged;
        Menu.VolumeAlphaThresholdChanged -= OnVolumeAlphaThresholdChanged;
        Menu.VolumeStepSizeChanged -= OnVolumeStepSizeChanged;
        Menu.VolumeChanged -= OnVolumeChanged;
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

    private void OnVolumeChanged(string volumeName)
    {
        var json = File.ReadAllText($"{Application.persistentDataPath}/{volumeName}.json");
        json = JsonCompressor.Decompress(json);
        JsonUtility.FromJsonOverwrite(json, _imagingSO);
        var texture = new Texture3D(_imagingSO.Width, _imagingSO.Height, _imagingSO.Depth, TextureFormat.Alpha8, false)
        {
            wrapMode = TextureWrapMode.Clamp,
            filterMode = FilterMode.Trilinear
        };
        texture.SetPixels32(_imagingSO.Pixels.Select(pixel => new Color32(0, 0, 0, pixel)).ToArray());
        texture.Apply();
        
        _material.mainTexture = texture;
        transform.localScale = _imagingSO.ScalingVector;
    }
}