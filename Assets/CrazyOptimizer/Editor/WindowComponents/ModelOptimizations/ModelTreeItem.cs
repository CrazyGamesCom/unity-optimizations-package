using CrazyGames.TreeLib;
using System;
using System.IO;
using UnityEditor;

namespace CrazyGames.WindowComponents.ModelOptimizations
{
    public class ModelTreeItem : TreeElement
    {
        public string ModelPath { get; }
        public string ModelName { get; }

        public bool IsReadWriteEnabled => _modelImporter.isReadable;
        public bool ArePolygonsOptimized => _modelImporter.optimizeMeshPolygons;
        public bool AreVerticesOptimized => _modelImporter.optimizeMeshVertices;
        public ModelImporterMeshCompression MeshCompression => _modelImporter.meshCompression;
        public ModelImporterAnimationCompression AnimationCompression => _modelImporter.animationCompression;

        public string MeshCompressionName => MeshCompression switch
        {
            ModelImporterMeshCompression.Off => "Off",
            ModelImporterMeshCompression.Low => "Low",
            ModelImporterMeshCompression.Medium => "Medium",
            ModelImporterMeshCompression.High => "High",
            _ => throw new ArgumentOutOfRangeException(),
        };

        public string AnimationCompressionName => AnimationCompression switch
        {
            ModelImporterAnimationCompression.Off => "Off",
            ModelImporterAnimationCompression.KeyframeReduction => "KeyframeReduction",
            ModelImporterAnimationCompression.KeyframeReductionAndCompression => "KeyframeReductionAndCompression",
            ModelImporterAnimationCompression.Optimal => "Optimal",
            _ => throw new ArgumentOutOfRangeException(),
        };

        private readonly ModelImporter _modelImporter;

        public ModelTreeItem(string name, int depth, int id, string modelPath, ModelImporter modelImporter) : base(name, depth, id)
        {
            if (depth == -1)
                return;

            ModelPath = modelPath;
            ModelName = Path.GetFileName(modelPath);

            _modelImporter = modelImporter;
        }
    }
}