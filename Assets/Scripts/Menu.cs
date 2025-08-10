using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

public class Menu : MonoBehaviour
{
    [SerializeField] private List<ModeComponents> modes;
    
    private UIDocument _uiDocument;
    private DropdownField _dropdownField;
    private VisualElement[] _atlasToggles;
    private Atlas _atlas;

    private void Awake()
    {
        _uiDocument = GetComponent<UIDocument>();
        _dropdownField = _uiDocument.rootVisualElement.Q<DropdownField>();
        for (var i = 0; i < modes.Count; i++)
        {
            var mode = modes[i];
            mode.menu = _uiDocument.rootVisualElement.Q<VisualElement>(mode.applicationMode.ToString());
            modes[i] = mode;
        }

        var atlasMode = modes.FirstOrDefault(mode => mode.applicationMode == ApplicationMode.Atlas);
        _atlas = atlasMode.item.GetComponent<Atlas>();
        _atlasToggles = atlasMode.menu.Children().ToArray();
    }

    private void OnEnable()
    {
        _dropdownField.RegisterValueChangedCallback(OnDropdownFieldChange);
        for (var i = 0; i < _atlasToggles.Length; i++)
        {
            var i1 = i;
            _atlasToggles[i].RegisterCallback<ChangeEvent<bool>>(evt => _atlas.SetAtlasVisibility(i1, evt.newValue));
        }
    }

    private void OnDisable()
    {
        _dropdownField.UnregisterValueChangedCallback(OnDropdownFieldChange);
        for (var i = 0; i < _atlasToggles.Length; i++)
        {
            var i1 = i;
            _atlasToggles[i].UnregisterCallback<ChangeEvent<bool>>(evt => _atlas.SetAtlasVisibility(i1, evt.newValue));
        }
    }

    private void OnDropdownFieldChange(ChangeEvent<string> evt)
    {
        var applicationMode = (ApplicationMode)_dropdownField.index;
        foreach (var mode in modes)
        {
            mode.menu.style.display = mode.applicationMode == applicationMode ? DisplayStyle.Flex : DisplayStyle.None;
            mode.item.SetActive(mode.applicationMode == applicationMode);
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