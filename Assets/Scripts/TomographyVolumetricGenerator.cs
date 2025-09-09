#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using FellowOakDicom;
using FellowOakDicom.Imaging;
using UnityEditor;

public class TomographyVolumetricGenerator
{
    private static Tomography _tomography;
    private static List<DicomFile> _dicomFiles;
    private static Color[] _colors;
    private static int _width;
    private static int _height;
    private static int _depth;
    private static float _rescaleSlope;
    private static float _rescaleIntercept;

    [MenuItem("Tomography/Generate Computed Tomography")]
    private static void GenerateComputerTomography()
    {
        _tomography = Tomography.Computed;
        CreateVolumetricTomography();
    }
    
    [MenuItem("Tomography/Generate Magnetic Resonance Imaging")]
    private static void GenerateMagneticResonanceImaging()
    {
        _tomography = Tomography.MagneticResonance;
        CreateVolumetricTomography();
    }
    
    private static void CreateVolumetricTomography()
    {
        LoadDicomFiles();
        SetDimensions();
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
        
        _dicomFiles.Sort((file, nextFile) => file.Dataset.GetSingleValueOrDefault(DicomTag.InstanceNumber, 0) < nextFile.Dataset.GetSingleValueOrDefault(DicomTag.InstanceNumber, 0) ? -1 : 1);
    }
    
    private static void SetDimensions()
    {
        var pixelData = DicomPixelData.Create(_dicomFiles[0].Dataset);
        _width = pixelData.Width;
        _height = pixelData.Height;
        _depth = _dicomFiles.Count;
        _colors = new Color[_width * _height * _depth];
        _rescaleSlope = _dicomFiles[0].Dataset.GetSingleValueOrDefault(DicomTag.RescaleSlope, 1);
        _rescaleIntercept = _dicomFiles[0].Dataset.GetSingleValueOrDefault(DicomTag.RescaleIntercept, -1024);
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
        AssetDatabase.CreateAsset(texture, $"Assets/Resources/Tomography/{_tomography.ToString()}.asset");
    }
}
#endif