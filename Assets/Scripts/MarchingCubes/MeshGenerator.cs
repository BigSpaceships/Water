using System;
using System.Collections.Generic;
using UnityEngine;

namespace MarchingCubes {
    public class MeshGenerator : MonoBehaviour {
        [Range(0, 1)]
        public float threshold = .5f;
        public Mesh GenerateMesh(Texture3D texture) {
            var size = new Vector3Int(texture.width - 1, texture.height - 1, texture.depth - 1);
            var numberOfCubes = size.x * size.y * size.z;

            var vertices = new List<Vector3>();
            var triangles = new List<int>();

            for (int i = 0; i < numberOfCubes; i++) {
                Vector3Int pos = GetPosFromIndex(size, i);

                var sides = GetTriangulation(texture, pos);
                var points = GetPoints(sides, pos);

                if (points.Length % 3 != 0) {
                    LogArray(sides);
                    LogArray(points);
                }
                
                foreach (var point in points) {
                    if (!vertices.Contains(point)) {
                        vertices.Add(point);
                    }

                    var index = vertices.IndexOf(point);
                    
                    triangles.Add(index);
                }
            }

            var mesh = new Mesh();
            mesh.SetVertices(vertices);
            mesh.SetTriangles(triangles, 0, false);
            mesh.RecalculateNormals();

            return mesh;
        }

        public Vector3Int GetPosFromIndex(Vector3Int size, int index) {
            var x = index % size.x;
            var y = Mathf.FloorToInt((float) index / (size.x * size.z));
            var z = Mathf.FloorToInt((float) index / size.x) % size.z;

            return new Vector3Int(x, y, z);
        }

        public Vector3[] GetPoints(int[] sides, Vector3Int pos) {
            var points = new List<Vector3>();
            foreach (var side in sides) {
                switch (side) {
                    case -1:
                        continue;
                    case 0:
                        points.Add(new Vector3(.5f, 0, 0));
                        break;
                    case 1:
                        points.Add(new Vector3(0, 0, .5f));
                        break;
                    case 2:
                        points.Add(new Vector3(.5f, 0, 1));
                        break;
                    case 3:
                        points.Add(new Vector3(1, 0, .5f));
                        break;
                    case 4:
                        points.Add(new Vector3(.5f, 1, 0));
                        break;
                    case 5:
                        points.Add(new Vector3(0, 1, .5f));
                        break;
                    case 6:
                        points.Add(new Vector3(.5f, 1, 1));
                        break;
                    case 7:
                        points.Add(new Vector3(1, 1, .5f));
                        break;
                    case 8:
                        points.Add(new Vector3(1, .5f, 0));
                        break;
                    case 9:
                        points.Add(new Vector3(0, .5f, 0));
                        break;
                    case 10:
                        points.Add(new Vector3(0, .5f, 1));
                        break;
                    case 11:
                        points.Add(new Vector3(1, .5f, 1));
                        break;
                }
            }

            var pointsArray = points.ToArray();
            for (int i = 0; i < pointsArray.Length; i++) {
                pointsArray[i] += pos;
            }

            return pointsArray;
        }

        public int[] GetTriangulation(Texture3D texture, Vector3Int pos) {
            int index = 0;

            if (texture.GetPixel(pos.x + 1, pos.y, pos.z).r > threshold) index |= 1;
            if (texture.GetPixel(pos.x, pos.y, pos.z).r > threshold) index |= 2;
            if (texture.GetPixel(pos.x, pos.y, pos.z + 1).r > threshold) index |= 4;
            if (texture.GetPixel(pos.x + 1, pos.y, pos.z + 1).r > threshold) index |= 8;
            if (texture.GetPixel(pos.x + 1, pos.y + 1, pos.z).r > threshold) index |= 16;
            if (texture.GetPixel(pos.x, pos.y + 1, pos.z).r > threshold) index |= 32;
            if (texture.GetPixel(pos.x, pos.y + 1, pos.z + 1).r > threshold) index |= 64;
            if (texture.GetPixel(pos.x + 1, pos.y + 1, pos.z + 1).r > threshold) index |= 128;
            
            Debug.Log(index);

            int[] sides = new int[15];
            for (int i = 0; i < 15; i++) {
                sides[i] = Triangulation.Tris[index, i];
                Debug.Log(Triangulation.Tris[index, i]);
                Debug.Log(i);
            }

            return sides;
        }

        public void LogArray<T>(T[] array) {
            var output = "";

            foreach (var obj in array) {
                output += obj.ToString();
                output += ",";
            }
            
            Debug.Log(output);
        }
    }
}