using System.Text;
using System.Text.RegularExpressions;
namespace ToolApp.Modifier
{

    // The class which contains one method for simplification of reading unity scene file, extracting only necessery components vital for the analysing data
    public class YamlModifier
    {
        // Modification of the unity file for the convenience, extracting Transform and  GameObject properties
        public static string ModifyYaml(string inputYaml)
        {

            // Remove lines starting with %YAML and %TAG
            inputYaml = Regex.Replace(inputYaml, @"^%.*$", string.Empty, RegexOptions.Multiline);

            // Split the input YAML into GameObject, RectTransform and Transform entries
            string[] entries = Regex.Split(inputYaml, "--- !u!\\d+ &(\\d+)");

            // Create a StringBuilder to build the modified YAML with necessery data
            StringBuilder modifiedYaml = new StringBuilder();
            modifiedYaml.AppendLine("---");
            int fileID = 0; // for GameObject, Transform and RectTransform their unique IDs

            string currentComponent = null;
            bool isTransformEntry = false;
            string currentTransformFileID = null;

            foreach (string entry in entries)
            {
                string trimmedEntry = entry.Trim();
                if (string.IsNullOrEmpty(trimmedEntry))
                    continue;

                // Check if the entry starts with "GameObject:" or "Transform:" or "RectTransform:"
                if (trimmedEntry.StartsWith("GameObject:"))
                {
                    // Extract Name
                    string[] lines = trimmedEntry.Split('\n');
                    string nameLine = Array.Find(lines, line => line.Trim().StartsWith("m_Name:"));
                    string name = nameLine?.Trim().Substring("m_Name:".Length).Trim();

                    // Extract m_component and its components
                    List<string> components = new List<string>();
                    bool isComponentEntry = false;

                    foreach (var line in lines)
                    {
                        if (line.Trim() == "m_Component:")
                        {
                            isComponentEntry = true;
                        }
                        else if (isComponentEntry && line.Trim().StartsWith("- component:"))
                        {
                            components.Add(line.Trim());
                        }
                        else
                        {
                            isComponentEntry = false;
                        }
                    }

                    // Build the modified YAML for the GameObject entry
                    modifiedYaml.AppendLine("- GameObject: null");

                    if (!string.IsNullOrEmpty(name))
                    {
                        modifiedYaml.AppendLine($"  m_fileID: {fileID}");
                    }
                    else
                    {
                        modifiedYaml.AppendLine($"  m_fileID: 0");
                    }

                    modifiedYaml.AppendLine($"  m_Name: {name}");

                    if (components.Count > 0)
                    {
                        modifiedYaml.AppendLine("  m_Component:");
                        foreach (var component in components)
                        {
                            modifiedYaml.AppendLine($"    {component}");
                        }
                    }

                    modifiedYaml.AppendLine();
                }
                else if (trimmedEntry.StartsWith("Transform:") || trimmedEntry.StartsWith("RectTransform:"))
                {


                    string[] lines = trimmedEntry.Split('\n');

                    //m_GameObject
                    string nameLine = Array.Find(lines, line => line.Trim().StartsWith("m_GameObject:"));
                    string m_GameObject = nameLine?.Trim().Substring("m_GameObject:".Length).Trim();

                    //m_Father
                    string nameLine1 = Array.Find(lines, line => line.Trim().StartsWith("m_Father:"));
                    string m_Father = nameLine1?.Trim().Substring("m_Father:".Length).Trim();


                    // Build the modified YAML for the GameObject entry
                    modifiedYaml.AppendLine("- Transform: null");
                    modifiedYaml.AppendLine($"  m_fileID: {fileID}");
                    modifiedYaml.AppendLine($"  m_GameObject: {m_GameObject}");
                    modifiedYaml.AppendLine($"  m_Father: {m_Father}");
                    modifiedYaml.AppendLine();
                }
                else if (int.TryParse(trimmedEntry, out int currentFileID))
                {
                    // Update the current fileID if a valid number is found
                    fileID = currentFileID;
                }

            }

            return modifiedYaml.ToString();
        }

        public static string ModifyYamlScripts(string inputYaml)
        {
            // Remove lines starting with %YAML and %TAG
            inputYaml = Regex.Replace(inputYaml, @"^%.*$", string.Empty, RegexOptions.Multiline);

            // Split the input YAML into GameObject, RectTransform and Transform entries
            string[] entries = Regex.Split(inputYaml, "--- !u!\\d+ &(\\d+)");

            // Create a StringBuilder to build the modified YAML with necessery data
            StringBuilder modifiedYaml = new StringBuilder();
            modifiedYaml.AppendLine("---");


            foreach (string entry in entries)
            {
                string trimmedEntry = entry.Trim();
                if (string.IsNullOrEmpty(trimmedEntry))
                    continue;


                if (trimmedEntry.StartsWith("MonoBehaviour:"))
                {
                    string[] lines = trimmedEntry.Split('\n');

                    // Extract m_GameObject
                    string gameObjectLine = Array.Find(lines, line => line.Trim().StartsWith("m_GameObject:"));
                    string m_GameObject = gameObjectLine?.Trim().Substring("m_GameObject:".Length).Trim();

                    // Extract m_Script
                    string scriptLine = Array.Find(lines, line => line.Trim().StartsWith("m_Script:"));
                    string m_Script = scriptLine?.Trim().Substring("m_Script:".Length).Trim();

                    // Extract GUID from m_Script
                    string guid = "";
                    if (!string.IsNullOrEmpty(m_Script))
                    {
                        Match guidMatch = Regex.Match(m_Script, @"guid: ([a-f0-9]+),");
                        if (guidMatch.Success)
                        {
                            guid = guidMatch.Groups[1].Value;
                        }
                    }

                    // Build the modified YAML for the MonoBehaviour entry
                    modifiedYaml.AppendLine("- MonoBehaviour: null");
                    modifiedYaml.AppendLine($"  m_GameObject: {m_GameObject}");
                    modifiedYaml.AppendLine($"  m_Script: {m_Script}"); // GUID is stored here
                    modifiedYaml.AppendLine($"  guid: {guid}");
                    modifiedYaml.AppendLine();
                }


            }

            return modifiedYaml.ToString();
        }

    }

     
    }
