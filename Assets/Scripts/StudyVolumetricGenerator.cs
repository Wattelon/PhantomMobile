#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using FellowOakDicom;
using FellowOakDicom.Imaging;
using UnityEditor;

public static class StudyVolumetricGenerator
{
    private static List<DicomFile> _dicomFiles;
    private static DicomDataset _dicomDataset;
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

    [MenuItem("PhantomAR/Load DICOM folder")]
    private static void CreateVolumetricTexture()
    {
        LoadDicomFiles();
        SetDimensions();
        GetTags();
        SetVolumeData();
        CreateTexture();
        CreateScriptableObject();
    }
    
    private static void LoadDicomFiles()
    {
        _dicomFiles = new List<DicomFile>();
        var fullPath = EditorUtility.OpenFolderPanel("Open DICOM folder", "", "");
        var files = Directory.GetFiles(fullPath, "*.dcm").ToList();

        foreach (var file in files)
        {
            _dicomFiles.Add(DicomFile.Open(file));
        }
        
        _dicomFiles.Sort((file, nextFile) => file.Dataset.GetSingleValue<int>(DicomTag.InstanceNumber) < nextFile.Dataset.GetSingleValue<int>(DicomTag.InstanceNumber) ? -1 : 1);
    }
    
    private static void SetDimensions()
    {
        _dicomDataset = _dicomFiles[0].Dataset;
        var pixelData = DicomPixelData.Create(_dicomDataset);
        _width = pixelData.Width;
        _height = pixelData.Height;
        _depth = _dicomFiles.Count;
        _colors = new Color[_width * _height * _depth];
    }

    private static void GetTags()
    {
        _rescaleSlope = _dicomDataset.GetSingleValueOrDefault(DicomTag.RescaleSlope, 1);
        _rescaleIntercept = _dicomDataset.GetSingleValueOrDefault(DicomTag.RescaleIntercept, -1024);
        _sliceThickness = _dicomDataset.GetSingleValue<float>(DicomTag.SliceThickness);
        var pixelSpacing = _dicomDataset.GetValues<float>(DicomTag.PixelSpacing);
        _pixelSpacingRow = pixelSpacing[0];
        _pixelSpacingColumn = pixelSpacing[1];
        _modality = _dicomDataset.GetSingleValue<string>(DicomTag.Modality);
        _instanceCreationDate = _dicomDataset.GetSingleValue<string>(DicomTag.InstanceCreationDate);
        _instanceCreationTime = _dicomDataset.GetSingleValue<string>(DicomTag.InstanceCreationTime);
        var orientationMatrix = _dicomDataset.GetValues<float>(DicomTag.ImageOrientationPatient);
        _orientationVectorX = new Vector3(orientationMatrix[0],  orientationMatrix[1], orientationMatrix[2]);
        _orientationVectorY = new Vector3(orientationMatrix[3],  orientationMatrix[4], orientationMatrix[5]);
    }

    private static void SetVolumeData()
    {
        for (var z = 0; z < _depth; z++)
        {
            var zOffset = z * _height * _width;
            
            var file = _dicomFiles[z];
            var image = new DicomImage(file.Dataset).RenderImage();

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
        if (!AssetDatabase.IsValidFolder($"Assets/Studies/{_modality}")) AssetDatabase.CreateFolder("Assets/Studies", _modality);
        AssetDatabase.CreateAsset(_studyTexture, $"Assets/Studies/{_modality}/{_modality}_{_instanceCreationDate}_{_instanceCreationTime}.asset");
    }

    private static void CreateScriptableObject()
    {
        var studySO = ScriptableObject.CreateInstance<StudySO>();
        studySO.Initialize(_studyTexture, _rescaleSlope, _rescaleIntercept, _sliceThickness, _pixelSpacingRow, _pixelSpacingColumn, _modality, _orientationVectorX, _orientationVectorY);
        AssetDatabase.CreateAsset(studySO, $"Assets/Studies/{_modality}/{_modality}_{_instanceCreationDate}_{_instanceCreationTime}_SO.asset");
    }
}
#endif