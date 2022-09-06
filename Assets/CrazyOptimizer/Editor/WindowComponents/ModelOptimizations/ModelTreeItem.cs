using CrazyGames.TreeLib;
using System;
using System.IO;
using UnityEditor;

namespace CrazyGames.WindowComponents.ModelOptimizations
{
    public class ModelTreeItem : TreeElement
    {
        public readonly string modelPath;
        public readonly string modelName;
        public readonly bool isReadWriteEnabled;
        public readonly bool arePolygonsOptimized;
        public readonly bool areVerticesOptimized;
        public readonly string meshCompression; // Off, Low, Medium, High (values in ModelImporterMeshCompression)
        public readonly string animationCompression; // Off, KeyframeReduction, KeyframeReductionAndCompression, Optimal (values in ModelImporterAnimationCompression)

        public ModelTreeItem(string name, int depth, int id, string modelPath, ModelImporter modelImporter) : base(name, depth, id)
        {
            if (depth == -1)
                return;

            this.modelPath = modelPath;
            modelName = Path.GetFileName(modelPath);

            isReadWriteEnabled = modelImporter.isReadable;
            arePolygonsOptimized = modelImporter.optimizeMeshPolygons;
            areVerticesOptimized = modelImporter.optimizeMeshVertices;

            switch (modelImporter.meshCompression)
            {
                case ModelImporterMeshCompression.Off:
                    meshCompression = "Off";
                    break;
                case ModelImporterMeshCompression.Low:
                    meshCompression = "Low";
                    break;
                case ModelImporterMeshCompression.Medium:
                    meshCompression = "Medium";
                    break;
                case ModelImporterMeshCompression.High:
                    meshCompression = "High";
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            switch (modelImporter.animationCompression)
            {
                case ModelImporterAnimationCompression.Off:
                    animationCompression = "Off";
                    break;
                case ModelImporterAnimationCompression.KeyframeReduction:
                    animationCompression = "KeyframeReduction";
                    break;
                case ModelImporterAnimationCompression.KeyframeReductionAndCompression:
                    animationCompression = "KeyframeReductionAndCompression";
                    break;
                case ModelImporterAnimationCompression.Optimal:
                    animationCompression = "Optimal";
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}