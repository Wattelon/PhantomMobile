using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using UnityEngine.UIElements;

public class Menu : MonoBehaviour
{
    [SerializeField] private List<ModeComponents> modes;
    [SerializeField] private List<Locale> locales;
    
    private UIDocument _uiDocument;
    private DropdownField _dropdownFieldCTAxis;
    private DropdownField _dropdownFieldMRIAxis;
    private DropdownField _dropdownFieldUSAxis;
    private DropdownField _dropdownFieldLanguage;
    private SliderInt _sliderCTSlicer;
    private SliderInt _sliderMRISlicer;
    private SliderInt _sliderUSSlicer;
    private Slider _sliderVolumeAlphaThreshold;
    private Slider _sliderVolumeAlpha;
    private Slider _sliderVolumeStepSize;
    private MinMaxSlider _brightnessSlider;
    private Toggle[] _atlasToggles;
    private Atlas _atlas;
    private ImagingSlicer _slicerCT;
    private ImagingSlicer _slicerMRI;
    private ImagingSlicer _slicerUS;
    private Button _modeButtonAtlas;
    private Button _modeButtonCT;
    private Button _modeButtonMRI;
    private Button _modeButtonUS;
    private Button _modeButtonVolume;
    private Button _buttonVolumeLibrary;
    private Button _buttonVolumeLoad;
    private Button _buttonLanguage;

    public static Action<float> VolumeAlphaChanged;
    public static Action<float> VolumeAlphaThresholdChanged;
    public static Action<float> VolumeStepSizeChanged;

    private void Awake()
    {
        _uiDocument = GetComponent<UIDocument>();
        _dropdownFieldCTAxis = _uiDocument.rootVisualElement.Q<DropdownField>("DropdownField-CTAxis");
        _dropdownFieldMRIAxis = _uiDocument.rootVisualElement.Q<DropdownField>("DropdownField-MRIAxis");
        _dropdownFieldUSAxis = _uiDocument.rootVisualElement.Q<DropdownField>("DropdownField-USAxis");
        _dropdownFieldLanguage = _uiDocument.rootVisualElement.Q<DropdownField>("DropdownField-Language");
        _sliderCTSlicer = _uiDocument.rootVisualElement.Q<SliderInt>("Slider-CTSlicer");
        _sliderMRISlicer = _uiDocument.rootVisualElement.Q<SliderInt>("Slider-MRISlicer");
        _sliderUSSlicer = _uiDocument.rootVisualElement.Q<SliderInt>("Slider-USSlicer");
        _sliderVolumeAlpha = _uiDocument.rootVisualElement.Q<Slider>("Slider-VolumeAlpha");
        _sliderVolumeAlphaThreshold = _uiDocument.rootVisualElement.Q<Slider>("Slider-VolumeAlphaThreshold");
        _sliderVolumeStepSize = _uiDocument.rootVisualElement.Q<Slider>("Slider-VolumeStepSize");
        _brightnessSlider = _uiDocument.rootVisualElement.Q<MinMaxSlider>("BrightnessSlider");
        _modeButtonAtlas = _uiDocument.rootVisualElement.Q<Button>("Button-ModeAtlas");
        _modeButtonCT = _uiDocument.rootVisualElement.Q<Button>("Button-ModeCT");
        _modeButtonMRI = _uiDocument.rootVisualElement.Q<Button>("Button-ModeMRI");
        _modeButtonUS = _uiDocument.rootVisualElement.Q<Button>("Button-ModeUS");
        _modeButtonVolume = _uiDocument.rootVisualElement.Q<Button>("Button-ModeVolume");
        _buttonLanguage = _uiDocument.rootVisualElement.Q<Button>("Button-Language");
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
        _slicerCT = computedTomographyMode.item.GetComponentInChildren<ImagingSlicer>();
        var magneticResonanceImagingMode = modes.FirstOrDefault(mode => mode.applicationMode == ApplicationMode.MagneticResonanceImaging);
        _slicerMRI = magneticResonanceImagingMode.item.GetComponentInChildren<ImagingSlicer>();
        var ultrasoundMode = modes.FirstOrDefault(mode => mode.applicationMode == ApplicationMode.Ultrasound);
        _slicerUS = ultrasoundMode.item.GetComponentInChildren<ImagingSlicer>();
    }

    private void OnEnable()
    {
        _modeButtonAtlas.RegisterCallback<ClickEvent, ApplicationMode>(OnModeButtonClick, ApplicationMode.Atlas);
        _modeButtonCT.RegisterCallback<ClickEvent, ApplicationMode>(OnModeButtonClick, ApplicationMode.ComputedTomography);
        _modeButtonMRI.RegisterCallback<ClickEvent, ApplicationMode>(OnModeButtonClick, ApplicationMode.MagneticResonanceImaging);
        _modeButtonUS.RegisterCallback<ClickEvent, ApplicationMode>(OnModeButtonClick, ApplicationMode.Ultrasound);
        _modeButtonVolume.RegisterCallback<ClickEvent, ApplicationMode>(OnModeButtonClick, ApplicationMode.VolumeRendering);
        _buttonLanguage.RegisterCallback<ClickEvent>(OnButtonLanguageClick);
        _dropdownFieldCTAxis.RegisterValueChangedCallback(OnDropdownFieldAxisChange);
        _dropdownFieldMRIAxis.RegisterValueChangedCallback(OnDropdownFieldAxisChange);
        _dropdownFieldUSAxis.RegisterValueChangedCallback(OnDropdownFieldAxisChange);
        _dropdownFieldLanguage.RegisterValueChangedCallback(OnDropdownFieldLanguageChange);
        _dropdownFieldLanguage.index = locales.IndexOf(LocalizationSettings.SelectedLocale);
        _sliderCTSlicer.RegisterValueChangedCallback(OnSlicerSliderChange);
        _sliderMRISlicer.RegisterValueChangedCallback(OnSlicerSliderChange);
        _sliderUSSlicer.RegisterValueChangedCallback(OnSlicerSliderChange);
        _sliderVolumeAlpha.RegisterValueChangedCallback(OnVolumeSliderChange);
        _sliderVolumeAlphaThreshold.RegisterValueChangedCallback(OnVolumeSliderChange);
        _sliderVolumeStepSize.RegisterValueChangedCallback(OnVolumeSliderChange);
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
        _buttonLanguage.UnregisterCallback<ClickEvent>(OnButtonLanguageClick);
        _dropdownFieldCTAxis.UnregisterValueChangedCallback(OnDropdownFieldAxisChange);
        _dropdownFieldMRIAxis.UnregisterValueChangedCallback(OnDropdownFieldAxisChange);
        _dropdownFieldUSAxis.UnregisterValueChangedCallback(OnDropdownFieldAxisChange);
        _dropdownFieldLanguage.UnregisterValueChangedCallback(OnDropdownFieldLanguageChange);
        _sliderCTSlicer.UnregisterValueChangedCallback(OnSlicerSliderChange);
        _sliderMRISlicer.UnregisterValueChangedCallback(OnSlicerSliderChange);
        _sliderUSSlicer.UnregisterValueChangedCallback(OnSlicerSliderChange);
        _sliderVolumeAlpha.UnregisterValueChangedCallback(OnVolumeSliderChange);
        _sliderVolumeAlphaThreshold.UnregisterValueChangedCallback(OnVolumeSliderChange);
        _sliderVolumeStepSize.UnregisterValueChangedCallback(OnVolumeSliderChange);
        _brightnessSlider.UnregisterValueChangedCallback(OnBrightnessSliderChange);
        for (var i = 0; i < _atlasToggles.Length; i++)
        {
            var index = i;
            _atlasToggles[i].RegisterValueChangedCallback(evt => _atlas.SetAtlasVisibility(index, evt.newValue));
        }
    }
    
    private void OnModeButtonClick(ClickEvent evt, ApplicationMode applicationMode)
    {
        foreach (var mode in modes)
        {
            mode.menu.style.display = mode.applicationMode == applicationMode ? DisplayStyle.Flex : DisplayStyle.None;
            mode.item.SetActive(mode.applicationMode == applicationMode);
        }
    }
    
    private void OnButtonLanguageClick(ClickEvent evt)
    {
        _dropdownFieldLanguage.style.visibility = _dropdownFieldLanguage.style.visibility ==  Visibility.Visible ? Visibility.Hidden : Visibility.Visible;
        _dropdownFieldLanguage.SetEnabled(_dropdownFieldLanguage.style.visibility == Visibility.Visible);
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
    
    private void OnDropdownFieldLanguageChange(ChangeEvent<string> evt)
    {
        LocalizationSettings.SelectedLocale = locales[_dropdownFieldLanguage.index];
    }
    
    private void OnSlicerSliderChange(ChangeEvent<int> evt)
    {
        if (evt.target == _sliderCTSlicer) _slicerCT.SetIndex(evt.newValue);
        else if (evt.target == _sliderMRISlicer) _slicerMRI.SetIndex(evt.newValue);
        else if (evt.target == _sliderUSSlicer) _slicerUS.SetIndex(evt.newValue);
    }

    private void SetSliderHighValue(SliderInt slider, ImagingSlicer slicer)
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
    
    private void OnVolumeSliderChange(ChangeEvent<float> evt)
    {
        if (evt.target == _sliderVolumeAlpha) VolumeAlphaChanged.Invoke(evt.newValue);
        else if (evt.target == _sliderVolumeAlphaThreshold) VolumeAlphaThresholdChanged.Invoke(evt.newValue);
        else if (evt.target == _sliderVolumeStepSize) VolumeStepSizeChanged.Invoke(evt.newValue);
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
    Ultrasound,
    VolumeRendering
}

[Serializable]
public struct ModeComponents
{
    public ApplicationMode applicationMode;
    public GameObject item;
    public VisualElement menu;
}