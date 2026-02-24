#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using FellowOakDicom;
using FellowOakDicom.Imaging;
using UnityEditor;

public static class ImagingVolumetricGenerator
{
    private static List<DicomDataset> _dicomDatasets;
    private static Dictionary<string, List<DicomDataset>> _dicomSeries;
    private static DicomDataset _dicomDataset;
    private static DicomDataset _functionalGroupValues;
    private static Color[] _colors;
    private static int _width;
    private static int _height;
    private static int _depth;
    private static Texture3D _studyTexture;
    private static float _rescaleSlope;
    private static float _rescaleIntercept;
    private static float _spacingBetweenSlices;
    private static float _pixelSpacingRow;
    private static float _pixelSpacingColumn;
    private static string _seriesUID;
    private static string _modality;
    private static string _instanceCreationDate;
    private static string _instanceCreationTime;
    private static Vector3 _orientationVectorX;
    private static Vector3 _orientationVectorY;
    private static Vector3 _orientationVectorZ;
    private static bool _isMultiFrame;
    private static bool _isDateTimeMissing;
    private static bool _hasFunctionalGroup;
    
    [MenuItem("PhantomAR/Load DICOM folder")]
    private static void LoadDicomFiles()
    {
        _dicomSeries = new Dictionary<string, List<DicomDataset>>();
        var fullPath = EditorUtility.OpenFolderPanel("Open DICOM folder", "", "");
        if (string.IsNullOrEmpty(fullPath)) return;
        var files = Directory.GetFiles(fullPath, "*.dcm", SearchOption.AllDirectories).ToList();

        foreach (var file in files)
        {
            _dicomDataset = DicomFile.Open(file).Dataset;
            if (!_dicomDataset.Contains(DicomTag.PixelData)) continue;
            
            _seriesUID = _dicomDataset.GetSingleValue<DicomUID>(DicomTag.SeriesInstanceUID).UID;
            if (!_dicomSeries.ContainsKey(_seriesUID)) _dicomSeries.Add(_seriesUID, new List<DicomDataset> { _dicomDataset });
            else _dicomSeries[_seriesUID].Add(_dicomDataset);
        }
        
        Debug.Log($"{_dicomSeries.Values.Sum(list => list.Count)} files loaded; {_dicomSeries.Count} series detected");

        foreach (var series in _dicomSeries.Values)
        {
            series.Sort((dataset, nextDataset) => dataset.GetSingleValue<int>(DicomTag.InstanceNumber) < nextDataset.GetSingleValue<int>(DicomTag.InstanceNumber) ? -1 : 1);
            _dicomDatasets = series;
            _dicomDataset = _dicomDatasets[0];
            _hasFunctionalGroup = _dicomDataset.Contains(DicomTag.SharedFunctionalGroupsSequence);
            if (_hasFunctionalGroup) _functionalGroupValues = _dicomDataset.FunctionalGroupValues(0);
            CreateVolumetricTexture();
        }
    }
    
    private static void CreateVolumetricTexture()
    {
        SetDimensions();
        GetTags();
        SortSlices();
        SetVolumeData();
        CreateAssets();
    }

    private static void SetDimensions()
    {
        var numberOfFrames = 0;
        if (_hasFunctionalGroup) _functionalGroupValues.TryGetSingleValue(DicomTag.NumberOfFrames, out numberOfFrames);
        if (numberOfFrames == 0) _dicomDataset.TryGetSingleValue(DicomTag.NumberOfFrames, out numberOfFrames);
        _isMultiFrame = numberOfFrames > 1;
        var pixelData = DicomPixelData.Create(_dicomDataset);
        _width = pixelData.Width;
        _height = pixelData.Height;
        _depth = _isMultiFrame ? numberOfFrames : _dicomDatasets.Count;
        _colors = new Color[_width * _height * _depth];
    }

    private static void GetTags()
    {
        double[] pixelSpacing;
        var orientationMatrix = new float[6];
        
        if (_hasFunctionalGroup)
        {
            _rescaleSlope = _functionalGroupValues.GetSingleValueOrDefault(DicomTag.RescaleSlope, 1);
            _rescaleIntercept = _functionalGroupValues.GetSingleValueOrDefault(DicomTag.RescaleIntercept, -1024);
            _spacingBetweenSlices = _functionalGroupValues.GetSingleValueOrDefault(DicomTag.SpacingBetweenSlices, 1f);
            pixelSpacing = _functionalGroupValues.GetValues<double>(DicomTag.PixelSpacing);
            _functionalGroupValues.TryGetValues(DicomTag.ImageOrientationPatient, out orientationMatrix);
        }
        else
        {
            _rescaleSlope = _dicomDataset.GetSingleValueOrDefault(DicomTag.RescaleSlope, 1);
            _rescaleIntercept = _dicomDataset.GetSingleValueOrDefault(DicomTag.RescaleIntercept, -1024);
            _spacingBetweenSlices = _dicomDataset.GetSingleValueOrDefault(DicomTag.SpacingBetweenSlices, 1f);
            pixelSpacing = _dicomDataset.GetValues<double>(DicomTag.PixelSpacing);
            _dicomDataset.TryGetValues(DicomTag.ImageOrientationPatient, out orientationMatrix);
        }
        
        _seriesUID = _dicomDataset.GetSingleValue<DicomUID>(DicomTag.SeriesInstanceUID).UID;
        _modality = _dicomDataset.GetSingleValue<string>(DicomTag.Modality);

        _isDateTimeMissing = false;
        if (!_dicomDataset.TryGetSingleValue(DicomTag.InstanceCreationDate, out _instanceCreationDate))
        {
            _instanceCreationDate = "DateMissing";
            _isDateTimeMissing = true;
        }
        if (!_dicomDataset.TryGetSingleValue(DicomTag.InstanceCreationTime, out _instanceCreationTime))
        {
            _instanceCreationTime = "TimeMissing";
            _isDateTimeMissing = true;
        }
        
        _pixelSpacingRow = (float)pixelSpacing[0];
        _pixelSpacingColumn = (float)pixelSpacing[1];
        _orientationVectorX = new Vector3(-orientationMatrix[0],  -orientationMatrix[1], orientationMatrix[2]);
        _orientationVectorY = new Vector3(-orientationMatrix[3],  -orientationMatrix[4], orientationMatrix[5]);
    }
    
    private static void SortSlices()
    {
        if (_isMultiFrame) return;
        
        _dicomDatasets.Sort((dataset, nextDataset) => dataset.GetValues<float>(DicomTag.ImagePositionPatient)[2] < nextDataset.GetValues<float>(DicomTag.ImagePositionPatient)[2] ? -1 : 1);
    }

    private static void SetVolumeData()
    {
        for (var z = 0; z < _depth; z++)
        {
            var zOffset = z * _height * _width;
            
            var image = _isMultiFrame ? new DicomImage(_dicomDatasets[0]).RenderImage(z) : new DicomImage(_dicomDatasets[z]).RenderImage();

            for (var y = 0; y < _height; y++)
            {
                var yOffset = y * _width;
                
                for (var x = 0; x < _width; x++)
                {
                    _colors[x + yOffset + zOffset] = new Color(0, 0, 0, image.GetPixel(x, y).R / 255f);
                }
            }
        }
    }
    
    private static void CreateAssets()
    {
        _studyTexture = new Texture3D(_width, _height, _depth, TextureFormat.Alpha8, false)
        {
            wrapMode = TextureWrapMode.Clamp,
            filterMode = FilterMode.Trilinear
        };
        _studyTexture.SetPixels(_colors);
        _studyTexture.Apply();
        var imagingSO = ScriptableObject.CreateInstance<ImagingSO>();
        imagingSO.Initialize(_seriesUID, _studyTexture, _rescaleSlope, _rescaleIntercept, _spacingBetweenSlices, _pixelSpacingRow, _pixelSpacingColumn, _modality, _orientationVectorX, _orientationVectorY);
        
        var filename = $"Assets/Imaging/{_modality}/{_modality}_{_instanceCreationDate}_{_instanceCreationTime}{(_isDateTimeMissing ? $"_{_seriesUID}" : "")}";
        if (!AssetDatabase.IsValidFolder("Assets/Imaging")) AssetDatabase.CreateFolder("Assets", "Imaging");
        if (!AssetDatabase.IsValidFolder($"Assets/Imaging/{_modality}")) AssetDatabase.CreateFolder("Assets/Imaging", _modality);
        AssetDatabase.CreateAsset(_studyTexture, $"{filename}.asset");
        AssetDatabase.CreateAsset(imagingSO, $"{filename}_SO.asset");
    }
}
#endif