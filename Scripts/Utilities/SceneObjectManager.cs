using System.Collections.Generic;
using UnityEngine;

namespace Nakshatra.Plugins
{
    public class SceneObjectManager : MonoBehaviour
    {
        [System.Serializable]
        public class ChildObjectState
        {
            public string namePath;
            public Vector3 position;
            public Quaternion rotation;
            public bool isActive;
        }

        public List<ChildObjectState> CaptureState()
        {
            var result = new List<ChildObjectState>();
            foreach (Transform child in GetComponentsInChildren<Transform>(true))
            {
                if (child == transform) continue;

                result.Add(new ChildObjectState
                {
                    namePath = GetHierarchyPath(child),
                    position = child.position,
                    rotation = child.rotation,
                    isActive = child.gameObject.activeSelf
                });
            }
            return result;
        }

        public void ApplyState(List<ChildObjectState> savedStates)
        {
            foreach (var state in savedStates)
            {
                var obj = transform.Find(state.namePath);
                if (obj != null)
                {
                    obj.position = state.position;
                    obj.rotation = state.rotation;
                    obj.gameObject.SetActive(state.isActive);
                }
            }

            // Destroy objects not present in saved state
            foreach (Transform child in GetComponentsInChildren<Transform>(true))
            {
                if (child == transform) continue;

                string path = GetHierarchyPath(child);
                bool existsInSave = savedStates.Exists(s => s.namePath == path);
                if (!existsInSave)
                {
                    Destroy(child.gameObject);
                }
            }
        }

        private string GetHierarchyPath(Transform t)
        {
            string path = t.name;
            while (t.parent != null && t.parent != transform)
            {
                t = t.parent;
                path = t.name + "/" + path;
            }
            return path;
        }
    }
}
