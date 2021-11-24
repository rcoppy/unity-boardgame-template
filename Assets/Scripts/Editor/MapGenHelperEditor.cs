using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Alex.BoardGame.Editor
{
    [CustomEditor(typeof(Alex.BoardGame.MapGenHelper))]
    public class MapGenHelperEditor : UnityEditor.Editor
    {
        public SerializedProperty _mapFile;

        private void OnEnable()
        {
            _mapFile = serializedObject.FindProperty("_mapFile");
        }

        public override void OnInspectorGUI()
        {
            // Get value before change
            var lastFile = _mapFile.objectReferenceValue; // as TextAsset;

            // Make all the public and serialized fields visible in Inspector
            base.OnInspectorGUI();

            // Load changed values
            serializedObject.Update();

            // Check if value has changed
            if (lastFile != _mapFile.objectReferenceValue) // as TextAsset)
            {
                Debug.Log("Map file changed");

                // Do something...
                var mgh = (MapGenHelper)target;
                mgh.FireFileChanged();
            }

            serializedObject.ApplyModifiedProperties();
        }
    }
}
