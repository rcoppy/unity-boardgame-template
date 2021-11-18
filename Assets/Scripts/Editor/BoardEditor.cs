using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Alex.BoardGame.Editor
{
    [CustomEditor(typeof(Alex.BoardGame.Board))]
    public class BoardEditor : UnityEditor.Editor
    {
        void OnSceneGUI()
        {
            /*var board = target as Board;

            var color = new Color(1, 0.8f, 0.4f, 1);
            Handles.color = color;
         
            // display object "value" in scene
            GUI.color = color;
            Handles.Label(board.Origin, board.gameObject.name);*/
        }
    }
}
