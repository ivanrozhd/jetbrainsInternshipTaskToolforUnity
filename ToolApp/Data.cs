namespace ToolApp.Data
{
 
    // All standard objects in the scene are GameObjects
    public class GameObject
    {
        public string FileID { get; set; }
        public string Name { get; set; }
        public List<string> ComponentFileIDs { get; set; } = new List<string>();
        public string FatherID { get; set; }
        public string FileIDTransform { get; set; }

        public override string ToString()
        {
            string components = string.Join(", ", ComponentFileIDs);
            return $"GameObject: {Name} (FileID: {FileID}), Components: {components}, FatherID: {FatherID}, FileIDTransform: {FileIDTransform}";
        }
    }

    // RectTransform and Transform contain the same essential data to define the hierarchy(parents and children)
    public class Transform
    {
        public string FileID { get; set; }
        public string GameObjectFileID { get; set; }
        public string FatherFileID { get; set; }

        public override string ToString()
        {
            return $"Transform: FileID = {FileID}, GameObjectFileID = {GameObjectFileID}, FatherFileID = {FatherFileID}";
        }
    }


}
