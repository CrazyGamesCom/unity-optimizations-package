using System;
using System.IO;
using CrazyGames.TreeLib;
using UnityEditor;

namespace CrazyGames.WindowComponents.TextureOptimizations
{
    public class TextureTreeItem : TreeElement
    {
        public readonly string texturePath;
        public readonly string textureName;
        public readonly int textureMaxSize;
        public TextureImporterFormat textureFormat;
        public readonly TextureImporterType textureType;
        public readonly int crunchCompressionQuality;
        public readonly bool hasCrunchCompression;
        public readonly string compression; // none/low quality, medium quality, high quality (values in TextureImporterCompression)

        //  private string texturePath;
        // private TextureImporter textureImporter;

        public TextureTreeItem(string name, int depth, int id, string texturePath, TextureImporter textureImporter) : base(name, depth, id)
        {
            if (depth == -1)
                return;
            this.texturePath = texturePath;
            textureName = Path.GetFileName(texturePath);
            textureImporter.GetPlatformTextureSettings("WebGL", out textureMaxSize, out textureFormat, out crunchCompressionQuality);
            hasCrunchCompression = textureImporter.crunchedCompression;
            textureType = textureImporter.textureType;
            switch (textureImporter.textureCompression)
            {
                case TextureImporterCompression.Uncompressed:
                    compression = "Uncompressed";
                    break;
                case TextureImporterCompression.Compressed:
                    compression = "Normal";
                    break;
                case TextureImporterCompression.CompressedHQ:
                    compression = "High";
                    break;
                case TextureImporterCompression.CompressedLQ:
                    compression = "Low";
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}