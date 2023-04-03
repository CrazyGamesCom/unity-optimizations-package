using System.Collections.Generic;
using UnityEditor;

namespace CrazyGames
{
    public class OptimizerUtils
    {
        /**
         * Find the paths of the scenes that will end up in the final build of the game.
         */
        public static List<string> GetScenesInBuildPath()
        {
            var scenesInBuild = new List<string>();
            foreach (var scene in EditorBuildSettings.scenes)
            {
                if (scene.enabled)
                {
                    scenesInBuild.Add(scene.path);
                }
            }

            return scenesInBuild;
        }
    }
}