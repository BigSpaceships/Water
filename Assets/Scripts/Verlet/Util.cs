using System;
using UnityEngine;

namespace Verlet {
    public static class Util {
        public static Vector3Int MaxSections;
        public static Vector3Int MinSections;
        public static Vector3Int TotalSections => MaxSections - MinSections;
        public static Vector3Int TotalSectionsAndOne => TotalSections + Vector3Int.one;
        public static int NumberOfSections => TotalSectionsAndOne.x * TotalSectionsAndOne.y * TotalSectionsAndOne.z;
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
            var x = Mathf.FloorToInt(section / TotalSectionsAndOne.x) % TotalSectionsAndOne.x;
            var y = Mathf.FloorToInt(section / (TotalSectionsAndOne.x * TotalSectionsAndOne.z));
            var z = section % TotalSectionsAndOne.z;
            // Debug.Log(section);
            
            return new Vector3Int(x, y, z) + MinSections;
        }

        public static int GetIntSection(Vector3Int section) {
            section -= MinSections;
            return section.x * TotalSectionsAndOne.z + section.y * TotalSectionsAndOne.z * TotalSectionsAndOne.x + section.z;
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

        public static ParticleStruct GetParticleStruct(Particle particle) {
            return new ParticleStruct() {
                InBounds = particle.InBounds,
                Index = particle.Index,
                Position = particle.Position,
                LastPosition = particle.LastPosition,
                Section = particle.SectionInt,
                Radius = particle.Radius,
            };
        }

        public static Particle GetParticle(ParticleStruct particleStruct) {
            return new Particle() {
                InBounds = particleStruct.InBounds,
                Index = particleStruct.Index,
                LastPosition = particleStruct.LastPosition,
                Position = particleStruct.Position,
                Radius = particleStruct.Radius,
                SectionInt = particleStruct.Index,
            };
        }
    }
}