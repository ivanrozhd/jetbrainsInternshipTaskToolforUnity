# ToolApp

## Dear Reviewers
Thank you for taking the time to review this project. This repository contains a C# tool developed for processing Unity gaming scenes. The tool is designed to analyze Unity scene files, extract information about GameObject hierarchies and identify unused scripts in a Unity project.

### How to Use

1. **Building the Executable**:
   - The executable `tool.exe` will be generated.

2. **Running the Tool**:
   - Open a command line interface.
   - Navigate to the directory containing `tool.exe`.
   - Run the tool using the following command:
     ```
     ./tool.exe <path_to_unity_project> <path_to_output_folder>
     ```
   - Replace `<path_to_unity_project>` with the full path to your Unity project.
   - Replace `<path_to_output_folder>` with the path where you want the output files to be saved.

### Output

- The tool generates two types of output:
  - A dump file for each Unity scene, showing the hierarchy of GameObjects.
  - A text file listing all unused scripts found in the project, along with their paths and GUIDs.

