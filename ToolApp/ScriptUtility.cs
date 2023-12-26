using System.Text.RegularExpressions;
using ToolApp.Modifier;

namespace ToolApp
{
    public class ScriptUtility
    {
        // Map guids with corresponding script names for simplicity 
        public static Dictionary<string, string> MapGuidsToScripts(string projectFolderPath)
        {
            Dictionary<string, string> guidToScriptMap = new Dictionary<string, string>();
            string[] scriptMetaFiles = Directory.GetFiles(projectFolderPath, "*.cs.meta", SearchOption.AllDirectories);

            foreach (var metaFilePath in scriptMetaFiles)
            {
                string metaFileContent = File.ReadAllText(metaFilePath);
                Match guidMatch = Regex.Match(metaFileContent, @"guid: ([a-f0-9]+)");

                if (guidMatch.Success)
                {
                    string guid = guidMatch.Groups[1].Value;
                    string relativePath = metaFilePath.Substring(projectFolderPath.Length).Replace(".meta", "");
                    guidToScriptMap[guid] = relativePath;
                }
            }

            return guidToScriptMap;
        }

        // exctracting data from unity scenes from monobehaviour component based on the GUID of the script
        public static HashSet<string> GetAllUsedScriptGuids(string[] scenePaths)
        {
            HashSet<string> allUsedGuids = new HashSet<string>();

            foreach (string scenePath in scenePaths)
            {
                string sceneContents = File.ReadAllText(scenePath);
                string modifiedYaml = YamlModifier.ModifyYamlScripts(sceneContents);
                var monoBehaviourGuids = ExtractMonoBehaviourGuids(modifiedYaml);

                foreach (string guid in monoBehaviourGuids)
                {
                    allUsedGuids.Add(guid);
                }
            }

            return allUsedGuids;
        }

        private static HashSet<string> ExtractMonoBehaviourGuids(string yamlContent)
        {
            HashSet<string> scriptGuids = new HashSet<string>();
            Regex monoBehaviourRegex = new Regex(@"m_Script: \{fileID: \d+, guid: ([a-f0-9]+), type: \d+\}");

            foreach (Match match in monoBehaviourRegex.Matches(yamlContent))
            {
                scriptGuids.Add(match.Groups[1].Value);
            }

            return scriptGuids;
        }

        // based on the used guids we find out which guids have not been used at all
        public static Dictionary<string, string> GetAllUnusedScripts(string projectFolderPath, HashSet<string> usedGuids)
        {
            Dictionary<string, string> guidToScriptMap = MapGuidsToScripts(projectFolderPath);
            Dictionary<string, string> unusedScripts = new Dictionary<string, string>();

            foreach (KeyValuePair<string,string> kvp in guidToScriptMap)
            {
                if (!usedGuids.Contains(kvp.Key))
                {
                    unusedScripts.Add(kvp.Key, kvp.Value);
                }
            }

            return unusedScripts;
        }

        // the format for the outputfile "Relative file path" - GUID 
        public static void WriteUnusedScriptsToFile(string filePath, Dictionary<string, string> unusedScripts)
        {
            using (StreamWriter writer = new StreamWriter(filePath))
            {
                foreach (KeyValuePair<string, string> script in unusedScripts)
                {
                    writer.WriteLine($"{script.Value} - {script.Key}");
                }
            }
        }

    }

}
