#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Build.Profile;
using UnityEditor.XR.Management;
using UnityEditor.XR.Management.Metadata;
using UnityEngine;
using Vuforia;

[InitializeOnLoad]
public static class PlatformConfigurator
{
    static PlatformConfigurator()
    {
        ConfigurePlatformSettings();
    }
    
    private static void ConfigurePlatformSettings()
    {
        var profile = BuildProfile.GetActiveBuildProfile();
        if (profile is null) return;
        
        switch (profile.name)
        {
            case "Android":
                ApplyAndroidSettings();
                break;
            case "Meta Quest":
                ApplyMetaQuestSettings();
                break;
        }
    }
    
    private static void ApplyAndroidSettings()
    {
        Resources.Load<VuforiaConfiguration>("VuforiaConfiguration").Vuforia.DelayedInitialization = false;
        var xrSettings = XRGeneralSettingsPerBuildTarget.XRGeneralSettingsForBuildTarget(BuildTargetGroup.Android);
        xrSettings.InitManagerOnStart = false;
        XRPackageMetadataStore.RemoveLoader(xrSettings.AssignedSettings, "UnityEngine.XR.OpenXR.OpenXRLoader", BuildTargetGroup.Android);
    }
    
    private static void ApplyMetaQuestSettings()
    {
        Resources.Load<VuforiaConfiguration>("VuforiaConfiguration").Vuforia.DelayedInitialization = true;
        var xrSettings = XRGeneralSettingsPerBuildTarget.XRGeneralSettingsForBuildTarget(BuildTargetGroup.Android);
        xrSettings.InitManagerOnStart = true;
        XRPackageMetadataStore.AssignLoader(xrSettings.AssignedSettings, "UnityEngine.XR.OpenXR.OpenXRLoader", BuildTargetGroup.Android);
    }
}
#endif