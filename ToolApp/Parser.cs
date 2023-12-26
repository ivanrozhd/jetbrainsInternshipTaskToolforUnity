using System.Text.RegularExpressions;
using ToolApp.Data;
using ToolApp.Modifier;
using ToolApp;
using ToolApp.ObjectManagment;

public class Parser
{
    // Main functionality
    public static void Main(string[] args)
    {

        // Check if the correct number of arguments are provided
        if (args.Length != 2)
        {
            Console.WriteLine("Usage: tool.exe <unity_project_path> <output_folder_path>");
            return;
        }


        // Managing inputData
        string projectFolderPath = args[0]; // First argument: Unity project path

        if (!Directory.Exists(projectFolderPath))
        {
            Console.WriteLine($"Error: The specified Unity project path does not exist: {projectFolderPath}");
            return;
        }
        string outputFolderPath = args[1];  // Second argument: Output folder path

        // creating new output folder and file
        if (!Directory.Exists(outputFolderPath))
        {
            try
            {
                Directory.CreateDirectory(outputFolderPath);
                Console.WriteLine($"Output folder created: {outputFolderPath}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error creating output folder: {ex.Message}");
                return;
            }
        }

        // Construct the output file path
        string outputFilePathScripts = Path.Combine(outputFolderPath, "UnusedScripts.txt");

        // Get all .unity files in the project
        string[] scenePaths = Directory.GetFiles(projectFolderPath, "*.unity", SearchOption.AllDirectories);

   
        // extracting not usable Scripts from the project
        var allUsedGuids = ScriptUtility.GetAllUsedScriptGuids(scenePaths);
        var allUnusedScripts = ScriptUtility.GetAllUnusedScripts(projectFolderPath, allUsedGuids);
        ScriptUtility.WriteUnusedScriptsToFile(outputFilePathScripts, allUnusedScripts);

        // extracting objects from the scene
        foreach (string scenePath in scenePaths)
        {
            string sceneName = Path.GetFileNameWithoutExtension(scenePath);
            string sceneContents = File.ReadAllText(scenePath);

            string modifiedYaml = YamlModifier.ModifyYaml(sceneContents);


            var gameObjects = ParseGameObjects(modifiedYaml);
            var transforms = ParseTransforms(modifiedYaml);

            ParseTransformsAndMapToGameObjects(transforms, gameObjects);
            var hierarchy = HierarchyBuilder.BuildHierarchy(gameObjects);

            string outputFilePath = Path.Combine(outputFolderPath, sceneName + ".unity.dump");

            using (StreamWriter fileWriter = new StreamWriter(outputFilePath))
            {
                HierarchyBuilder.WriteHierarchyDump(hierarchy, fileWriter);
            }
         
        }
    }

    // Parse essential Data to create the more convenient structure for GameObject
    private static Dictionary<string, GameObject> ParseGameObjects(string yamlContent)
    {
        Dictionary<string, GameObject> gameObjects = new Dictionary<string, GameObject>();
        Regex gameObjectRegex = new Regex(@"- GameObject:\s+null\s+m_fileID: (\d+)\s+m_Name: (.*?)(?=m_Component:|\Z)", RegexOptions.Singleline);
        Regex componentRegex = new Regex(@"component: \{fileID: (\d+)\}", RegexOptions.Singleline);

        foreach (Match gameObjectMatch in gameObjectRegex.Matches(yamlContent))
        {
            string fileID = gameObjectMatch.Groups[1].Value;
            string name = gameObjectMatch.Groups[2].Value.Trim(new char[] { ' ', '\"' });

            GameObject gameObject = new GameObject { FileID = fileID, Name = name, FatherID = "0", FileIDTransform = "0" };

            int startIndex = gameObjectMatch.Index + gameObjectMatch.Length;
            int endIndex = yamlContent.IndexOf("- GameObject:", startIndex);
            endIndex = endIndex == -1 ? yamlContent.Length : endIndex;
            string componentPart = yamlContent[startIndex..endIndex];

            foreach (Match componentMatch in componentRegex.Matches(componentPart))
            {
                string componentFileID = componentMatch.Groups[1].Value;
                gameObject.ComponentFileIDs.Add(componentFileID);
            }

            gameObjects[fileID] = gameObject;

        }

        return gameObjects;
    }

    // Parse essential Data to create the more convenient structure for Transforms/RectTransforms
    private static Dictionary<string, Transform> ParseTransforms(string yamlContent)
    {
        Dictionary<string, Transform> transforms = new Dictionary<string, Transform>();
        Regex transformRegex = new Regex(@"- Transform: null\s+m_fileID: (\d+)\s+m_GameObject: \{fileID: (\d+)\}\s+m_Father: \{fileID: (\d+)\}", RegexOptions.Singleline);

        foreach (Match match in transformRegex.Matches(yamlContent))
        {
            string fileID = match.Groups[1].Value;
            string gameObjectFileID = match.Groups[2].Value;
            string fatherFileID = match.Groups[3].Value;

            transforms[fileID] = new Transform
            {
                FileID = fileID,
                GameObjectFileID = gameObjectFileID,
                FatherFileID = fatherFileID
            };
        }

        return transforms;
    }

    // Extract the corresponding transform/rectTransform components for their GameObjects and fulfilled the FatherID and FileIDTransform of GameObject
    // All Transforms/RectTransforms have their GameObject and with this component all parent-child relationships are built
    // FileIDTransform is the ID of the component which GameObject has. The FatherID is the id of another Transform/RectTransform which is basically the parent of the current Transform component
    private static void ParseTransformsAndMapToGameObjects(Dictionary<string, Transform> transforms, Dictionary<string, GameObject> gameObjects)
    {
        foreach (KeyValuePair<string,Transform> kvp in transforms)
        {
            var transform = kvp.Value;

            foreach (GameObject gameObject in gameObjects.Values)
            {
                if (gameObject.ComponentFileIDs.Contains(transform.FileID))
                {
                    gameObject.FatherID = transform.FatherFileID;
                    gameObject.FileIDTransform = transform.FileID;
                    break;
                }
            }
        }
    }





}

