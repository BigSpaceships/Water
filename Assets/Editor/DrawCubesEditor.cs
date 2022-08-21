using UnityEditor;
using UnityEngine;

namespace Editor {
    [CustomEditor(typeof(Verlet.DrawCubes))]
    public class DrawCubesEditor : UnityEditor.Editor {
        public override void OnInspectorGUI() {
            DrawDefaultInspector();

            GUILayout.Space(6);
            
            if (GUILayout.Button("ResetBuffers")) {
                ((Verlet.DrawCubes)target).SetupParticles();
            }
        }
    }
}