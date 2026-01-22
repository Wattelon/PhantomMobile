using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

public class Menu : MonoBehaviour
{
    [SerializeField] private List<ModeComponents> modes;
    
    private UIDocument _uiDocument;
    private DropdownField _dropdownFieldMode;
    private DropdownField _dropdownFieldCTAxis;
    private DropdownField _dropdownFieldMRIAxis;
    private DropdownField _dropdownFieldUSAxis;
    private SliderInt _sliderCTSlicer;
    private SliderInt _sliderMRISlicer;
    private SliderInt _sliderUSSlicer;
    private Toggle _brightnessToggle;
    private MinMaxSlider _brightnessSlider;
    private Slider _minBrightnessSlider;
    private Slider _maxBrightnessSlider;
    private Toggle[] _atlasToggles;
    private Atlas _atlas;
    private StudySlicer _slicerCT;
    private StudySlicer _slicerMRI;
    private StudySlicer _slicerUS;
    private Button _modeButtonAtlas;
    private Button _modeButtonCT;
    private Button _modeButtonMRI;
    private Button _modeButtonUS;
    private Button _buttonResults;
    private Button _buttonTest;

    private void Awake()
    {
        _uiDocument = GetComponent<UIDocument>();
        _dropdownFieldMode = _uiDocument.rootVisualElement.Q<DropdownField>();
        _dropdownFieldCTAxis = _uiDocument.rootVisualElement.Q<DropdownField>("DropdownField-CTAxis");
        _dropdownFieldMRIAxis = _uiDocument.rootVisualElement.Q<DropdownField>("DropdownField-MRIAxis");
        _dropdownFieldUSAxis = _uiDocument.rootVisualElement.Q<DropdownField>("DropdownField-USAxis");
        _sliderCTSlicer = _uiDocument.rootVisualElement.Q<SliderInt>("Slider-CTSlicer");
        _sliderMRISlicer = _uiDocument.rootVisualElement.Q<SliderInt>("Slider-MRISlicer");
        _sliderUSSlicer = _uiDocument.rootVisualElement.Q<SliderInt>("Slider-USSlicer");
        _brightnessToggle = _uiDocument.rootVisualElement.Q<Toggle>("BrightnessToggle");
        _brightnessSlider = _uiDocument.rootVisualElement.Q<MinMaxSlider>("BrightnessSlider");
        _minBrightnessSlider = _uiDocument.rootVisualElement.Q<Slider>("Slider-MinBrightness");
        _maxBrightnessSlider = _uiDocument.rootVisualElement.Q<Slider>("Slider-MaxBrightness");
        _modeButtonAtlas = _uiDocument.rootVisualElement.Q<Button>("Button-ModeAtlas");
        _modeButtonCT = _uiDocument.rootVisualElement.Q<Button>("Button-ModeCT");
        _modeButtonMRI = _uiDocument.rootVisualElement.Q<Button>("Button-ModeMRI");
        _modeButtonUS = _uiDocument.rootVisualElement.Q<Button>("Button-ModeUS");
        _buttonTest = _uiDocument.rootVisualElement.Q<Button>("Button-Test");
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
        var ultrasoundMode = modes.FirstOrDefault(mode => mode.applicationMode == ApplicationMode.Ultrasound);
        _slicerUS = ultrasoundMode.item.GetComponentInChildren<StudySlicer>();
    }

    private void OnEnable()
    {
        _modeButtonAtlas.RegisterCallback<ClickEvent, ApplicationMode>(OnModeButtonClick, ApplicationMode.Atlas);
        _modeButtonCT.RegisterCallback<ClickEvent, ApplicationMode>(OnModeButtonClick, ApplicationMode.ComputedTomography);
        _modeButtonMRI.RegisterCallback<ClickEvent, ApplicationMode>(OnModeButtonClick, ApplicationMode.MagneticResonanceImaging);
        _modeButtonUS.RegisterCallback<ClickEvent, ApplicationMode>(OnModeButtonClick, ApplicationMode.Ultrasound);
        _buttonTest.RegisterCallback<ClickEvent>(OnTestButtonClick);
        _dropdownFieldMode.RegisterValueChangedCallback(OnDropdownFieldModeChange);
        _dropdownFieldCTAxis.RegisterValueChangedCallback(OnDropdownFieldAxisChange);
        _dropdownFieldMRIAxis.RegisterValueChangedCallback(OnDropdownFieldAxisChange);
        _dropdownFieldUSAxis.RegisterValueChangedCallback(OnDropdownFieldAxisChange);
        _sliderCTSlicer.RegisterValueChangedCallback(OnSliderChange);
        _sliderMRISlicer.RegisterValueChangedCallback(OnSliderChange);
        _sliderUSSlicer.RegisterValueChangedCallback(OnSliderChange);
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
        _modeButtonAtlas.UnregisterCallback<ClickEvent, ApplicationMode>(OnModeButtonClick);
        _modeButtonCT.UnregisterCallback<ClickEvent, ApplicationMode>(OnModeButtonClick);
        _modeButtonMRI.UnregisterCallback<ClickEvent, ApplicationMode>(OnModeButtonClick);
        _modeButtonUS.UnregisterCallback<ClickEvent, ApplicationMode>(OnModeButtonClick);
        _dropdownFieldMode.UnregisterValueChangedCallback(OnDropdownFieldModeChange);
        _dropdownFieldCTAxis.UnregisterValueChangedCallback(OnDropdownFieldAxisChange);
        _dropdownFieldMRIAxis.UnregisterValueChangedCallback(OnDropdownFieldAxisChange);
        _dropdownFieldUSAxis.UnregisterValueChangedCallback(OnDropdownFieldAxisChange);
        _sliderCTSlicer.UnregisterValueChangedCallback(OnSliderChange);
        _sliderMRISlicer.UnregisterValueChangedCallback(OnSliderChange);
        _sliderUSSlicer.UnregisterValueChangedCallback(OnSliderChange);
        _brightnessSlider.UnregisterValueChangedCallback(OnBrightnessSliderChange);
        for (var i = 0; i < _atlasToggles.Length; i++)
        {
            var index = i;
            _atlasToggles[i].RegisterValueChangedCallback(evt => _atlas.SetAtlasVisibility(index, evt.newValue));
        }
    }

    private void OnTestButtonClick(ClickEvent evt)
    {
        SceneManager.LoadScene(1);
    }
    
    private void OnModeButtonClick(ClickEvent evt, ApplicationMode applicationMode)
    {
        foreach (var mode in modes)
        {
            mode.menu.style.display = mode.applicationMode == applicationMode ? DisplayStyle.Flex : DisplayStyle.None;
            mode.item.SetActive(mode.applicationMode == applicationMode);
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
        else if (evt.target == _dropdownFieldUSAxis)
        {
            _slicerUS.SetAxis(_dropdownFieldUSAxis.index);
            SetSliderHighValue(_sliderUSSlicer, _slicerUS);
        }
    }
    
    private void OnSliderChange(ChangeEvent<int> evt)
    {
        if (evt.target == _sliderCTSlicer) _slicerCT.SetIndex(evt.newValue);
        else if (evt.target == _sliderMRISlicer) _slicerMRI.SetIndex(evt.newValue);
        else if (evt.target == _sliderUSSlicer) _slicerUS.SetIndex(evt.newValue);
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
    MagneticResonanceImaging,
    Ultrasound
}

[Serializable]
public struct ModeComponents
{
    public ApplicationMode applicationMode;
    public GameObject item;
    public VisualElement menu;
}