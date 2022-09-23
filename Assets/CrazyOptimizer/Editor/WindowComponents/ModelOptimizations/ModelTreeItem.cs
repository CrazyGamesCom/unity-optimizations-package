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

        public string MeshCompressionName
        {
            get
            {
                switch (MeshCompression)
                {
                    case ModelImporterMeshCompression.Off:
                        return "Off";
                    case ModelImporterMeshCompression.Low:
                        return "Low";
                    case ModelImporterMeshCompression.Medium:
                        return "Medium";
                    case ModelImporterMeshCompression.High:
                        return "High";
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }

        public string AnimationCompressionName
        {
            get
            {
                switch (AnimationCompression)
                {
                    case ModelImporterAnimationCompression.Off:
                        return "Off";
                    case ModelImporterAnimationCompression.KeyframeReduction:
                        return "KeyframeReduction";
                    case ModelImporterAnimationCompression.KeyframeReductionAndCompression:
                        return "KeyframeReductionAndCompression";
                    case ModelImporterAnimationCompression.Optimal:
                        return "Optimal";
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }

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