using CrazyGames.TreeLib;
using System;
using System.IO;
using UnityEditor;

namespace CrazyGames.WindowComponents.TextureOptimizations
{
    public class TextureTreeItem : TreeElement
    {
        public string TexturePath { get; }
        public string TextureName { get; }

        public int TextureMaxSize => _platformSettings.maxTextureSize;
        public int CrunchCompressionQuality => _platformSettings.compressionQuality;
        public bool HasCrunchCompression => _textureImporter.crunchedCompression;
        public TextureImporterFormat TextureFormat => _platformSettings.format;
        public TextureImporterType TextureType => _textureImporter.textureType;
        public TextureImporterCompression TextureCompression => _textureImporter.textureCompression;

        public string TextureCompressionName
        {
            get
            {
                switch (TextureCompression)
                {
                    case TextureImporterCompression.Uncompressed:
                        return "Uncompressed";
                    case TextureImporterCompression.Compressed:
                        return "Normal";
                    case TextureImporterCompression.CompressedHQ:
                        return "High";
                    case TextureImporterCompression.CompressedLQ:
                        return "Low";
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }

        private readonly TextureImporter _textureImporter;
        private readonly TextureImporterPlatformSettings _platformSettings;

        public TextureTreeItem(string name, int depth, int id, string texturePath, TextureImporter textureImporter) : base(name, depth, id)
        {
            if (depth == -1)
                return;

            TexturePath = texturePath;
            TextureName = Path.GetFileName(texturePath);

            _textureImporter = textureImporter;
            _platformSettings = _textureImporter.GetPlatformTextureSettings("WebGL");
        }
    }
}