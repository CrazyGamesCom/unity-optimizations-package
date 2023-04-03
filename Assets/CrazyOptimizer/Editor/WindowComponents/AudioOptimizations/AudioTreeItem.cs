using CrazyGames.TreeLib;
using System;
using System.IO;
using UnityEditor;

namespace CrazyGames.WindowComponents.AudioOptimizations
{
    public class AudioTreeItem : TreeElement
    {
        public string AudioPath { get; }
        public string AudioName { get; }

        // public int TextureMaxSize => _platformSettings.maxTextureSize;
        // public int CrunchCompressionQuality => _platformSettings.compressionQuality;
        // public bool HasCrunchCompression => _audioImporter.crunchedCompression;
        // public TextureImporterFormat TextureFormat => _platformSettings.format;
        // public TextureImporterType TextureType => _audioImporter.textureType;
        // public TextureImporterCompression TextureCompression => _audioImporter.textureCompression;
        //
        // public string TextureCompressionName
        // {
        //     get
        //     {
        //         switch (TextureCompression)
        //         {
        //             case TextureImporterCompression.Uncompressed:
        //                 return "Uncompressed";
        //             case TextureImporterCompression.Compressed:
        //                 return "Normal";
        //             case TextureImporterCompression.CompressedHQ:
        //                 return "High";
        //             case TextureImporterCompression.CompressedLQ:
        //                 return "Low";
        //             default:
        //                 throw new ArgumentOutOfRangeException();
        //         }
        //     }
        // }

        private readonly AudioImporter _audioImporter;
        private readonly AudioImporterSampleSettings _platformSettings;

        public AudioTreeItem(string name, int depth, int id, string audioPath, AudioImporter audioImporter) : base(name, depth, id)
        {
            if (depth == -1)
                return;

            AudioPath = audioPath;
            AudioName = Path.GetFileName(audioPath);

            _audioImporter = audioImporter;
            _platformSettings = _audioImporter.GetOverrideSampleSettings("WebGL");
        }
    }
}