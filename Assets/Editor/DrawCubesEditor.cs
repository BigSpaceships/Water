using UnityEditor;
using UnityEngine;

namespace Editor {
    [CustomEditor(typeof(Verlet.DrawCubes))]
    public class DrawCubesEditor : UnityEditor.Editor {
        public override void OnInspectorGUI() {
            DrawDefaultInspector();

            if (GUILayout.Button("ResetBuffers")) {
                ((Verlet.DrawCubes)target).SetupParticles();
            }
        }
    }
}