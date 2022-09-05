using AssetStoreTools.Utility.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine.UIElements;

namespace AssetStoreTools.Uploader
{
    public abstract class UploadWorkflowView : VisualElement
    {
        protected TextField PathSelectionField;

        // Upload data
        protected string MainExportPath;
        protected List<string> ExtraExportPaths = new List<string>();
        protected string LocalPackageGuid;
        protected string LocalPackagePath;
        protected string LocalProjectPath;

        protected Action _serializeSelection;

        public abstract string Name { get; }
        public abstract string DisplayName { get; }

        protected UploadWorkflowView(Action serializeSelection)
        {
            _serializeSelection = serializeSelection;
            style.display = DisplayStyle.None;
        }

        public string[] GetAllExportPaths()
        {
            var allPaths = new List<string>(ExtraExportPaths);
            if (!string.IsNullOrEmpty(MainExportPath))
                allPaths.Insert(0, MainExportPath);
            return allPaths.ToArray();
        }

        public string GetLocalPackageGuid()
        {
            return LocalPackageGuid;
        }

        public string GetLocalPackagePath()
        {
            return LocalPackagePath;
        }

        public string GetLocalProjectPath()
        {
            return LocalProjectPath;
        }

        protected abstract void SetupWorkflow();

        public virtual JsonValue SerializeWorkflow()
        {
            var workflowDict = JsonValue.NewDict();

            var mainExportPathDict = JsonValue.NewDict();
            mainExportPathDict["path"] = MainExportPath;

            if (MainExportPath != null && !MainExportPath.StartsWith("Assets/") && !MainExportPath.StartsWith("Packages/"))
                mainExportPathDict["guid"] = new JsonValue("");
            else
                mainExportPathDict["guid"] = AssetDatabase.AssetPathToGUID(MainExportPath);

            workflowDict["mainPath"] = mainExportPathDict;

            var extraExportPathsList = new List<JsonValue>();
            foreach (var path in ExtraExportPaths)
            {
                var pathDict = JsonValue.NewDict();
                pathDict["path"] = path;
                pathDict["guid"] = AssetDatabase.AssetPathToGUID(path);
                extraExportPathsList.Add(pathDict);
            }
            workflowDict["extraPaths"] = extraExportPathsList;

            return workflowDict;
        }

        protected bool DeserializeMainExportPath(JsonValue json, out string mainExportPath)
        {
            mainExportPath = string.Empty;
            try
            {
                var mainPathDict = json["mainPath"];

                if (!mainPathDict.ContainsKey("path") || !mainPathDict["path"].IsString())
                    return false;

                mainExportPath = DeserializePath(mainPathDict);
                return true;
            }
            catch
            {
                return false;
            }
        }

        protected void DeserializeExtraExportPaths(JsonValue json, out List<string> extraExportPaths)
        {
            extraExportPaths = new List<string>();
            try
            {
                var extraPathsList = json["extraPaths"].AsList();
                extraExportPaths.AddRange(extraPathsList.Select(DeserializePath));
            }
            catch
            {
                ASDebug.LogWarning($"Deserializing extra export paths for {Name} failed");
                extraExportPaths.Clear();
            }
        }

        private string DeserializePath(JsonValue pathDict)
        {
            // First pass - retrieve from GUID
            var exportPath = AssetDatabase.GUIDToAssetPath(pathDict["guid"].AsString());
            // Second pass - retrieve directly
            if (string.IsNullOrEmpty(exportPath))
                exportPath = pathDict["path"].AsString();
            return exportPath;
        }

        public abstract void LoadSerializedWorkflow(JsonValue json, string lastUploadedPath, string lastUploadedGuid);

        public abstract void LoadSerializedWorkflowFallback(string lastUploadedPath, string lastUploadedGuid);

        protected abstract void BrowsePath();

        public abstract Task<PackageExporter.ExportResult> ExportPackage(bool isCompleteProject);
    }
}