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
    private static List<DicomFile> _dicomFiles;
    private static Dictionary<DicomUID, List<DicomFile>> _dicomSeries;
    private static DicomDataset _dicomDataset;
    private static DicomDataset _functionalGroupValues;
    private static Color[] _colors;
    private static int _width;
    private static int _height;
    private static int _depth;
    private static Texture3D _studyTexture;
    private static float _rescaleSlope;
    private static float _rescaleIntercept;
    private static float _sliceThickness;
    private static float _pixelSpacingRow;
    private static float _pixelSpacingColumn;
    private static string _modality;
    private static string _instanceCreationDate;
    private static string _instanceCreationTime;
    private static Vector3 _orientationVectorX;
    private static Vector3 _orientationVectorY;
    private static Vector3 _orientationVectorZ;
    private static bool _isMultiFrame;
    private static bool _hasFunctionalGroup;

    private static void CreateVolumetricTexture()
    {
        SetDimensions();
        GetTags();
        SetVolumeData();
        CreateTexture();
        CreateScriptableObject();
    }
    
    [MenuItem("PhantomAR/Load DICOM folder")]
    private static void LoadDicomFiles()
    {
        _dicomSeries = new Dictionary<DicomUID, List<DicomFile>>();
        var fullPath = EditorUtility.OpenFolderPanel("Open DICOM folder", "", "");
        var files = Directory.GetFiles(fullPath, "*.dcm", SearchOption.AllDirectories).ToList();

        foreach (var file in files)
        {
            var dicomFile = DicomFile.Open(file);
            _dicomDataset = dicomFile.Dataset;
            if (!_dicomDataset.Contains(DicomTag.PixelData)) continue;
            
            var seriesUid = _dicomDataset.GetSingleValue<DicomUID>(DicomTag.SeriesInstanceUID);
            if (!_dicomSeries.ContainsKey(seriesUid)) _dicomSeries.Add(seriesUid, new List<DicomFile> { dicomFile });
            else _dicomSeries[seriesUid].Add(dicomFile);
        }
        
        Debug.Log($"{_dicomSeries.Count} series and {_dicomSeries.Values.Sum(list => list.Count)} files loaded");

        foreach (var series in _dicomSeries.Values)
        {
            series.Sort((file, nextFile) => file.Dataset.GetSingleValue<int>(DicomTag.InstanceNumber) < nextFile.Dataset.GetSingleValue<int>(DicomTag.InstanceNumber) ? -1 : 1);
            _dicomFiles = series;
            _dicomDataset = _dicomFiles[0].Dataset;
            _hasFunctionalGroup = _dicomDataset.Contains(DicomTag.SharedFunctionalGroupsSequence);
            if (_hasFunctionalGroup) _functionalGroupValues = _dicomDataset.FunctionalGroupValues(0);
            CreateVolumetricTexture();
        }
    }
    
    private static void SetDimensions()
    {
        int numberOfFrames;
        if (_hasFunctionalGroup) _functionalGroupValues.TryGetSingleValue(DicomTag.NumberOfFrames, out numberOfFrames);
        else _dicomDataset.TryGetSingleValue(DicomTag.NumberOfFrames, out numberOfFrames);
        _isMultiFrame = numberOfFrames > 1;
        var pixelData = DicomPixelData.Create(_dicomDataset);
        _width = pixelData.Width;
        _height = pixelData.Height;
        _depth = _isMultiFrame ? numberOfFrames : _dicomFiles.Count;
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
            _sliceThickness = _functionalGroupValues.GetSingleValueOrDefault(DicomTag.SliceThickness, 1f);
            pixelSpacing = _functionalGroupValues.GetValues<double>(DicomTag.PixelSpacing);
            _functionalGroupValues.TryGetValues(DicomTag.ImageOrientationPatient, out orientationMatrix);
        }
        else
        {
            _rescaleSlope = _dicomDataset.GetSingleValueOrDefault(DicomTag.RescaleSlope, 1);
            _rescaleIntercept = _dicomDataset.GetSingleValueOrDefault(DicomTag.RescaleIntercept, -1024);
            _sliceThickness = _dicomDataset.GetSingleValueOrDefault(DicomTag.SliceThickness, 1f);
            pixelSpacing = _dicomDataset.GetValues<double>(DicomTag.PixelSpacing);
            _dicomDataset.TryGetValues(DicomTag.ImageOrientationPatient, out orientationMatrix);
        }
        
        _modality = _dicomDataset.GetSingleValue<string>(DicomTag.Modality);
        _instanceCreationDate = _dicomDataset.GetSingleValueOrDefault(DicomTag.InstanceCreationDate, $"DateMissing_{System.DateTime.Now:yyyy.MM.dd}");
        _instanceCreationTime = _dicomDataset.GetSingleValueOrDefault(DicomTag.InstanceCreationTime, $"TimeMissing_{System.DateTime.Now:HH.mm.ss}");
        
        _pixelSpacingRow = (float)pixelSpacing[0];
        _pixelSpacingColumn = (float)pixelSpacing[1];
        _orientationVectorX = new Vector3(orientationMatrix[0],  orientationMatrix[1], orientationMatrix[2]);
        _orientationVectorY = new Vector3(orientationMatrix[3],  orientationMatrix[4], orientationMatrix[5]);
    }

    private static void SetVolumeData()
    {
        for (var z = 0; z < _depth; z++)
        {
            var zOffset = z * _height * _width;
            
            var image = _isMultiFrame ? new DicomImage(_dicomFiles[0].Dataset).RenderImage(z) : new DicomImage(_dicomFiles[z].Dataset).RenderImage();

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
    
    private static void CreateTexture()
    {
        _studyTexture = new Texture3D(_width, _height, _depth, TextureFormat.Alpha8, false);
        _studyTexture.SetPixels(_colors);
        _studyTexture.Apply();
        if (!AssetDatabase.IsValidFolder("Assets/Imaging")) AssetDatabase.CreateFolder("Assets", "Imaging");
        if (!AssetDatabase.IsValidFolder($"Assets/Imaging/{_modality}")) AssetDatabase.CreateFolder("Assets/Imaging", _modality);
        AssetDatabase.CreateAsset(_studyTexture, $"Assets/Imaging/{_modality}/{_modality}_{_instanceCreationDate}_{_instanceCreationTime}.asset");
    }

    private static void CreateScriptableObject()
    {
        var imagingSO = ScriptableObject.CreateInstance<ImagingSO>();
        imagingSO.Initialize(_studyTexture, _rescaleSlope, _rescaleIntercept, _sliceThickness, _pixelSpacingRow, _pixelSpacingColumn, _modality, _orientationVectorX, _orientationVectorY);
        AssetDatabase.CreateAsset(imagingSO, $"Assets/Imaging/{_modality}/{_modality}_{_instanceCreationDate}_{_instanceCreationTime}_SO.asset");
    }
}
#endif