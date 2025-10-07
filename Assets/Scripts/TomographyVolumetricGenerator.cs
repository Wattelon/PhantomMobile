#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;
using FellowOakDicom;
using FellowOakDicom.Imaging;
using UnityEditor;

public static class TomographyVolumetricGenerator
{
    private static List<DicomFile> _dicomFiles;
    private static DicomDataset _dicomDataset;
    private static Color[] _colors;
    private static int _width;
    private static int _height;
    private static int _depth;
    private static float _rescaleSlope;
    private static float _rescaleIntercept;
    private static float _sliceThickness;
    private static float _pixelSpacingRow;
    private static float _pixelSpacingColumn;
    private static string _modality;
    private static string _instanceCreationDate;
    private static string _instanceCreationTime;
    

    [MenuItem("PhantomAR/Load DICOM files")]
    private static void CreateVolumetricTexture()
    {
        LoadDicomFiles();
        SetDimensions();
        GetTags();
        SetVolumeData();
        CreateTexture();
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
        _rescaleSlope = _dicomDataset.GetSingleValue<float>(DicomTag.RescaleSlope);
        _rescaleIntercept = _dicomDataset.GetSingleValue<float>(DicomTag.RescaleIntercept);
        _sliceThickness = _dicomDataset.GetSingleValueOrDefault(DicomTag.SliceThickness, 1);
        var pixelSpacing = _dicomDataset.GetValues<float>(DicomTag.PixelSpacing);
        _pixelSpacingRow = pixelSpacing[0];
        _pixelSpacingColumn = pixelSpacing[1];
        _modality = _dicomDataset.GetSingleValue<string>(DicomTag.Modality);
        _instanceCreationDate = _dicomDataset.GetSingleValue<string>(DicomTag.InstanceCreationDate);
        _instanceCreationTime = _dicomDataset.GetSingleValue<string>(DicomTag.InstanceCreationTime);
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
        var texture = new Texture3D(_width, _height, _depth, TextureFormat.Alpha8, false);
        texture.SetPixels(_colors);
        texture.Apply();
        if (!AssetDatabase.IsValidFolder($"Assets/Resources/Studies/{_modality}")) AssetDatabase.CreateFolder("Assets/Resources/Studies", _modality);
        AssetDatabase.CreateAsset(texture, $"Assets/Resources/Studies/{_modality}/{_modality} {_instanceCreationDate} {_instanceCreationTime}.asset");
    }
}
#endif