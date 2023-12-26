using ToolApp.Data;

namespace ToolApp.ObjectManagment
{
    public class HierarchyBuilder
    {
        // Maps GameObjects with their chidlren 
        public static Dictionary<GameObject, List<GameObject>> BuildHierarchy(Dictionary<string, GameObject> gameObjects)
        {
            var hierarchy = new Dictionary<GameObject, List<GameObject>>();

            foreach (GameObject gameObject in gameObjects.Values)
            {
                hierarchy[gameObject] = new List<GameObject>();
            }

            // Map children to their respective parents
            foreach (GameObject potentialChild in gameObjects.Values)
            {
                if (string.IsNullOrEmpty(potentialChild.FatherID))
                {
                    continue; // no parent - no reason to search for it
                }

                foreach (GameObject potentialParent in gameObjects.Values)
                {

                    if (potentialChild.FatherID.Equals(potentialParent.FileIDTransform))
                    {
                        hierarchy[potentialParent].Add(potentialChild);
                        break; // Break the loop once the parent is found
                    }
                }
            }

            return hierarchy;
        }


        // Writes the GameObjects in hierarchical order in the file
        public static void WriteHierarchyDump(Dictionary<GameObject, List<GameObject>> hierarchy, StreamWriter fileWriter, int level = 0, GameObject parent = null)
        {
            if (parent == null)
            {
                foreach (GameObject rootGameObject in hierarchy.Keys.Where(k => k != null && k.FatherID.Equals("0")))
                {
                    fileWriter.WriteLine(FormatGameObjectForDump(rootGameObject, level));
                    WriteHierarchyDump(hierarchy, fileWriter, level + 1, rootGameObject);
                }
            }
            else
            {
                foreach (GameObject child in hierarchy[parent])
                {
                    fileWriter.WriteLine(FormatGameObjectForDump(child, level));
                    WriteHierarchyDump(hierarchy, fileWriter, level + 1, child);
                }
            }
        }


        // Format the GameObject information for the dump file.
        private static string FormatGameObjectForDump(GameObject gameObject, int level)
        {
            return $"{new string('-', level + 1)}{gameObject.Name}";
        }

    }


}
