using System.Collections.Generic;
using UnityEngine;

namespace Verlet {
    public class Particle {
        public Vector3 Position;
        public Vector3 LastPosition;

        public Vector3 Velocity => LastPosition - Position;
        public Vector3Int Section;
        public Vector3Int OldSection;
        public Vector3Int OldOldSection;
        public int SectionInt;
        public int Index;

        public bool InBounds = true;

        public bool Active => InBounds;

        public float radius;

        private DrawCubes drawer;

        public Particle(DrawCubes drawer, Vector3 pos) : this(pos) {
            this.drawer = drawer;
        }

        public Particle(Vector3 pos){
            Position = pos;
            LastPosition = pos;
            
            OldSection = Vector3Int.zero;

            if (drawer != null) {
                UpdateSection();
            }
        }

        public Particle() : this(Vector3.zero) {
        }

        public bool UpdateSection() {
            var sizeOfSection = drawer.SectionSize;

            var x = Mathf.FloorToInt((Position.x + sizeOfSection / 2) / sizeOfSection);
            var y = Mathf.FloorToInt((Position.y + sizeOfSection / 2) / sizeOfSection);
            var z = Mathf.FloorToInt((Position.z + sizeOfSection / 2) / sizeOfSection);
            
            Section = new Vector3Int(x, y, z);

            var changed = OldSection != Section;

            OldOldSection = Section;

            if (changed) {
                SectionInt = Util.GetIntSection(Section);
            }

            return changed;
        }

        public List<Vector3Int> GetAdjacentSections() {

            return new List<Vector3Int>() {
                new(Section.x + 1, Section.y + 1, Section.z + 1),
                new(Section.x + 1, Section.y + 1, Section.z),
                new(Section.x + 1, Section.y + 1, Section.z - 1),
                new(Section.x + 1, Section.y, Section.z + 1),
                new(Section.x + 1, Section.y, Section.z),
                new(Section.x + 1, Section.y, Section.z - 1),
                new(Section.x + 1, Section.y - 1, Section.z + 1),
                new(Section.x + 1, Section.y - 1, Section.z),
                new(Section.x + 1, Section.y - 1, Section.z - 1),
                new(Section.x, Section.y + 1, Section.z + 1),
                new(Section.x, Section.y + 1, Section.z),
                new(Section.x, Section.y + 1, Section.z - 1),
                new(Section.x, Section.y, Section.z + 1),
                new(Section.x, Section.y, Section.z),
                new(Section.x, Section.y, Section.z - 1),
                new(Section.x, Section.y - 1, Section.z + 1),
                new(Section.x, Section.y - 1, Section.z),
                new(Section.x, Section.y - 1, Section.z - 1),
                new(Section.x - 1, Section.y + 1, Section.z + 1),
                new(Section.x - 1, Section.y + 1, Section.z),
                new(Section.x - 1, Section.y + 1, Section.z - 1),
                new(Section.x - 1, Section.y, Section.z + 1),
                new(Section.x - 1, Section.y, Section.z),
                new(Section.x - 1, Section.y, Section.z - 1),
                new(Section.x - 1, Section.y - 1, Section.z + 1),
                new(Section.x - 1, Section.y - 1, Section.z),
                new(Section.x - 1, Section.y - 1, Section.z - 1),
            };
        }
    }
}