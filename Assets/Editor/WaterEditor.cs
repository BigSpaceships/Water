using MarchingCubes;
using UnityEditor;
using UnityEngine;

namespace Editor {
    [CustomEditor(typeof(Water))]
    public class WaterEditor : UnityEditor.Editor {
        public override void OnInspectorGUI() {
            Water waterTarget = (Water) target;

            DrawDefaultInspector();

            if (GUILayout.Button("Set Water")) {
                waterTarget.SetWater();
            }

            if (GUILayout.Button("Generate Mesh")) {
                waterTarget.SetMesh(waterTarget.GetComponent<MeshGenerator>().GenerateMesh(waterTarget.waterTexture));
            }
        }
    }
}