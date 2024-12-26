using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace FirebaseIntegration
{
    public class FirebaseModel
    {
        private const string LocalProjectsFileName = "firebase_projects.json";

        public List<string> ProjectIds { get; private set; }

        public FirebaseModel()
        {
            ProjectIds = new List<string>();
        }

        public void LoadFromFile()
        {
            string path = Path.Combine(Application.persistentDataPath, LocalProjectsFileName);

            if (File.Exists(path))
            {
                string json = File.ReadAllText(path);
                ProjectIds = JsonUtility.FromJson<ProjectIdList>(json)?.projects ?? new List<string>();
            }
        }

        public void SaveToFile()
        {
            string path = Path.Combine(Application.persistentDataPath, LocalProjectsFileName);
            string json = JsonUtility.ToJson(new ProjectIdList { projects = ProjectIds }, true);
            File.WriteAllText(path, json);
        }

        public void AddProjectId(string projectId)
        {
            if (!ProjectIds.Contains(projectId))
            {
                ProjectIds.Add(projectId);
            }
        }

        public void RemoveProjectId(string projectId)
        {
            if (ProjectIds.Contains(projectId))
            {
                ProjectIds.Remove(projectId);
            }
        }

        [System.Serializable]
        private class ProjectIdList
        {
            public List<string> projects;
        }
    }   
}
