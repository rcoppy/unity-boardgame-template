using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Alex.BoardGame.Editor
{
    [CustomEditor(typeof(Alex.BoardGame.Board))]
    public class BoardEditor : UnityEditor.Editor
    {
        public SerializedProperty _shouldShowBoard;

        private void OnEnable()
        {
            _shouldShowBoard = serializedObject.FindProperty("_showBoardWhenNotActive"); 
        }

        void OnSceneGUI()
        {
            /*var board = target as Board;

            var color = new Color(1, 0.8f, 0.4f, 1);
            Handles.color = color;
         
            // display object "value" in scene
            GUI.color = color;
            Handles.Label(board.Origin, board.gameObject.name);*/
        }

        public override void OnInspectorGUI()
        {
            // Get value before change
            bool prevBoardStatus = _shouldShowBoard.boolValue;

            // Make all the public and serialized fields visible in Inspector
            base.OnInspectorGUI();

            // Load changed values
            serializedObject.Update();

            Board b = (Board)target;
            
            // Check if value has changed
            if (prevBoardStatus != _shouldShowBoard.boolValue)
            {
                // Do something...
                b.CheckBoardReload();
            }

            serializedObject.ApplyModifiedProperties();

            if (GUILayout.Button("Hard despawn"))
            {
                b.HardDespawn();    
            }

            if (GUILayout.Button("Hard spawn"))
            {
                b.SpawnBoard();
            }

            EditorGUILayout.HelpBox("Make sure that when you build this scene no game tiles are currently visible", MessageType.Warning);
        }
    }
}
