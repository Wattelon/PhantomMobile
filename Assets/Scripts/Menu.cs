using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

public class Menu : MonoBehaviour
{
    [SerializeField] private List<ModeComponents> modes;
    
    private UIDocument _uiDocument;
    private DropdownField _dropdownFieldMode;
    private DropdownField _dropdownFieldCTAxis;
    private DropdownField _dropdownFieldMRIAxis;
    private SliderInt _sliderCTSlicer;
    private SliderInt _sliderMRISlicer;
    private Toggle _brightnessToggle;
    private MinMaxSlider _brightnessSlider;
    private Slider _minBrightnessSlider;
    private Slider _maxBrightnessSlider;
    private Toggle[] _atlasToggles;
    private Atlas _atlas;
    private StudySlicer _slicerCT;
    private StudySlicer _slicerMRI;

    private void Awake()
    {
        _uiDocument = GetComponent<UIDocument>();
        _dropdownFieldMode = _uiDocument.rootVisualElement.Q<DropdownField>();
        _dropdownFieldCTAxis = _uiDocument.rootVisualElement.Q<DropdownField>("DropdownField-CTAxis");
        _dropdownFieldMRIAxis = _uiDocument.rootVisualElement.Q<DropdownField>("DropdownField-MRIAxis");
        _sliderCTSlicer = _uiDocument.rootVisualElement.Q<SliderInt>("Slider-CTSlicer");
        _sliderMRISlicer = _uiDocument.rootVisualElement.Q<SliderInt>("Slider-MRISlicer");
        _brightnessToggle = _uiDocument.rootVisualElement.Q<Toggle>("BrightnessToggle");
        _brightnessSlider = _uiDocument.rootVisualElement.Q<MinMaxSlider>("BrightnessSlider");
        _minBrightnessSlider = _uiDocument.rootVisualElement.Q<Slider>("Slider-MinBrightness");
        _maxBrightnessSlider = _uiDocument.rootVisualElement.Q<Slider>("Slider-MaxBrightness");
        for (var i = 0; i < modes.Count; i++)
        {
            var mode = modes[i];
            mode.menu = _uiDocument.rootVisualElement.Q<VisualElement>(mode.applicationMode.ToString());
            modes[i] = mode;
        }

        var atlasMode = modes.FirstOrDefault(mode => mode.applicationMode == ApplicationMode.Atlas);
        _atlas = atlasMode.item.GetComponent<Atlas>();
        _atlasToggles = atlasMode.menu.Children().Select(atlasToggle => atlasToggle as Toggle).ToArray();
        var computedTomographyMode = modes.FirstOrDefault(mode => mode.applicationMode == ApplicationMode.ComputedTomography);
        _slicerCT = computedTomographyMode.item.GetComponentInChildren<StudySlicer>();
        var magneticResonanceImagingMode = modes.FirstOrDefault(mode => mode.applicationMode == ApplicationMode.MagneticResonanceImaging);
        _slicerMRI = magneticResonanceImagingMode.item.GetComponentInChildren<StudySlicer>();
    }

    private void OnEnable()
    {
        _dropdownFieldMode.RegisterValueChangedCallback(OnDropdownFieldModeChange);
        _dropdownFieldCTAxis.RegisterValueChangedCallback(OnDropdownFieldAxisChange);
        _dropdownFieldMRIAxis.RegisterValueChangedCallback(OnDropdownFieldAxisChange);
        _sliderCTSlicer.RegisterValueChangedCallback(OnSliderChange);
        _sliderMRISlicer.RegisterValueChangedCallback(OnSliderChange);
        //_brightnessToggle.RegisterValueChangedCallback(OnBrightnessToggle);
        _brightnessSlider.RegisterValueChangedCallback(OnBrightnessSliderChange);
        for (var i = 0; i < _atlasToggles.Length; i++)
        {
            var index = i;
            _atlasToggles[i].RegisterValueChangedCallback(evt => _atlas.SetAtlasVisibility(index, evt.newValue));
        }
    }

    private void OnDisable()
    {
        _dropdownFieldMode.UnregisterValueChangedCallback(OnDropdownFieldModeChange);
        _dropdownFieldCTAxis.UnregisterValueChangedCallback(OnDropdownFieldAxisChange);
        _dropdownFieldMRIAxis.UnregisterValueChangedCallback(OnDropdownFieldAxisChange);
        _sliderCTSlicer.UnregisterValueChangedCallback(OnSliderChange);
        _sliderMRISlicer.UnregisterValueChangedCallback(OnSliderChange);
        _brightnessSlider.UnregisterValueChangedCallback(OnBrightnessSliderChange);
        for (var i = 0; i < _atlasToggles.Length; i++)
        {
            var index = i;
            _atlasToggles[i].RegisterValueChangedCallback(evt => _atlas.SetAtlasVisibility(index, evt.newValue));
        }
    }

    private void OnDropdownFieldModeChange(ChangeEvent<string> evt)
    {
        var applicationMode = (ApplicationMode)_dropdownFieldMode.index;
        foreach (var mode in modes)
        {
            mode.menu.style.display = mode.applicationMode == applicationMode ? DisplayStyle.Flex : DisplayStyle.None;
            mode.item.SetActive(mode.applicationMode == applicationMode);
        }
    }
    
    private void OnDropdownFieldAxisChange(ChangeEvent<string> evt)
    {
        if (evt.target == _dropdownFieldCTAxis)
        {
            _slicerCT.SetAxis(_dropdownFieldCTAxis.index);
            SetSliderHighValue(_sliderCTSlicer, _slicerCT);
        }
        else if (evt.target == _dropdownFieldMRIAxis)
        {
            _slicerMRI.SetAxis(_dropdownFieldMRIAxis.index);
            SetSliderHighValue(_sliderMRISlicer, _slicerMRI);
        }
    }
    
    private void OnSliderChange(ChangeEvent<int> evt)
    {
        if (evt.target == _sliderCTSlicer)
        {
            _slicerCT.SetIndex(evt.newValue);
        }
        else if (evt.target == _sliderMRISlicer)
        {
            _slicerMRI.SetIndex(evt.newValue);
        }
    }

    private void SetSliderHighValue(SliderInt slider, StudySlicer slicer)
    {
        int maxIndex;
        switch (slicer.CurrentAxis)
        {
            case Axis.Axial:
                maxIndex = slicer.TextureDepth;
                break;
            case Axis.Sagittal:
                maxIndex = slicer.TextureWidth;
                break;
            case Axis.Coronal:
                maxIndex = slicer.TextureHeight;
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
        slider.value = Mathf.Clamp(slider.value, 0, maxIndex);
        slider.highValue = maxIndex;
    }
    
    private void OnBrightnessToggle(ChangeEvent<bool> evt)
    {
        _slicerMRI.SetBrightnessFilter(evt.newValue);
    }
    
    private void OnBrightnessSliderChange(ChangeEvent<Vector2> evt)
    {
        _slicerMRI.SetBrightness(evt.newValue.x, evt.newValue.y);
    }

    public void RenewAtlasVisibility()
    {
        for (var i = 0; i < _atlasToggles.Length; i++)
        {
            _atlas.SetAtlasVisibility(i, _atlasToggles[i].value);
        }
    }
}

public enum ApplicationMode
{
    View,
    Atlas,
    ComputedTomography,
    MagneticResonanceImaging
}

[Serializable]
public struct ModeComponents
{
    public ApplicationMode applicationMode;
    public GameObject item;
    public VisualElement menu;
}