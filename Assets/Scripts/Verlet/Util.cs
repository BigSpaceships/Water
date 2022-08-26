using System;
using UnityEngine;

namespace Verlet {
    public static class Util {
        public static Vector3Int MaxSections;
        public static Vector3Int MinSections;
        public static Vector3Int TotalSections => MaxSections - MinSections;
        public static int NumberOfSections => (TotalSections.x + 1) * (TotalSections.y + 1) * (TotalSections.z + 1);
        public static float SectionSize;
        
        public static void DrawSection(Vector3Int section, Color color) {
            var sectionPos = (Vector3) section * SectionSize;

            Debug.DrawLine(new Vector3(-SectionSize / 2, -SectionSize / 2, -SectionSize / 2) + sectionPos,
                new Vector3(-SectionSize / 2, -SectionSize / 2, SectionSize / 2) + sectionPos, color);
            Debug.DrawLine(new Vector3(-SectionSize / 2, -SectionSize / 2, SectionSize / 2) + sectionPos,
                new Vector3(SectionSize / 2, -SectionSize / 2, SectionSize / 2) + sectionPos, color);
            Debug.DrawLine(new Vector3(SectionSize / 2, -SectionSize / 2, SectionSize / 2) + sectionPos,
                new Vector3(SectionSize / 2, -SectionSize / 2, -SectionSize / 2) + sectionPos, color);
            Debug.DrawLine(new Vector3(SectionSize / 2, -SectionSize / 2, -SectionSize / 2) + sectionPos,
                new Vector3(-SectionSize / 2, -SectionSize / 2, -SectionSize / 2) + sectionPos, color);

            Debug.DrawLine(new Vector3(-SectionSize / 2, -SectionSize / 2, -SectionSize / 2) + sectionPos,
                new Vector3(-SectionSize / 2, SectionSize / 2, -SectionSize / 2) + sectionPos, color);
            Debug.DrawLine(new Vector3(-SectionSize / 2, -SectionSize / 2, SectionSize / 2) + sectionPos,
                new Vector3(-SectionSize / 2, SectionSize / 2, SectionSize / 2) + sectionPos, color);
            Debug.DrawLine(new Vector3(SectionSize / 2, -SectionSize / 2, SectionSize / 2) + sectionPos,
                new Vector3(SectionSize / 2, SectionSize / 2, SectionSize / 2) + sectionPos, color);
            Debug.DrawLine(new Vector3(SectionSize / 2, -SectionSize / 2, -SectionSize / 2) + sectionPos,
                new Vector3(SectionSize / 2, SectionSize / 2, -SectionSize / 2) + sectionPos, color);

            Debug.DrawLine(new Vector3(-SectionSize / 2, SectionSize / 2, -SectionSize / 2) + sectionPos,
                new Vector3(-SectionSize / 2, SectionSize / 2, SectionSize / 2) + sectionPos, color);
            Debug.DrawLine(new Vector3(-SectionSize / 2, SectionSize / 2, SectionSize / 2) + sectionPos,
                new Vector3(SectionSize / 2, SectionSize / 2, SectionSize / 2) + sectionPos, color);
            Debug.DrawLine(new Vector3(SectionSize / 2, SectionSize / 2, SectionSize / 2) + sectionPos,
                new Vector3(SectionSize / 2, SectionSize / 2, -SectionSize / 2) + sectionPos, color);
            Debug.DrawLine(new Vector3(SectionSize / 2, SectionSize / 2, -SectionSize / 2) + sectionPos,
                new Vector3(-SectionSize / 2, SectionSize / 2, -SectionSize / 2) + sectionPos, color);
        }

        public static Vector3Int GetVector3IntSection(int section) {
            return new Vector3Int(Mathf.FloorToInt(section / (float) (TotalSections.y * TotalSections.z)),
                Mathf.FloorToInt(section / (float) TotalSections.z) % TotalSections.x , section % TotalSections.x % TotalSections.y) + MinSections;
        }

        public static int GetIntSection(Vector3Int section) {
            section -= MinSections;
            return section.x * TotalSections.y * TotalSections.z + section.y * TotalSections.z + section.z;
        }

        public static Vector3Int GetSection(Vector3 pos) {
            var x = Mathf.FloorToInt((pos.x + SectionSize / 2) / SectionSize);
            var y = Mathf.FloorToInt((pos.y + SectionSize / 2) / SectionSize);
            var z = Mathf.FloorToInt((pos.z + SectionSize / 2) / SectionSize);

            return new Vector3Int(x, y, z);
        }

        public static bool SectionExists(Vector3Int section) {
            return SectionExists(GetIntSection(section));
        }

        public static bool SectionExists(int section) {
            return section >= 0 && section <= NumberOfSections;
        }
    }
}