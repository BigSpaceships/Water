using System.Collections.Generic;
using System.Collections.Specialized;
using UnityEngine;

namespace Verlet {
    public class Particle {
        public Vector3 Position;
        public Vector3 LastPosition;

        public Vector3Int Section;
        public Vector3Int OldSection;
        public Vector3Int OldOldSection;
        public int SectionInt;
        public int Index;

        public bool InBounds = true;

        public bool SectionChanged;

        public bool Active => InBounds;

        public float radius;

        public Particle(DrawCubes drawer, Vector3 pos) : this(pos) {
            Section = Util.GetSection(Position);
            SectionInt = Util.GetIntSection(Section);
        }

        public Particle(Vector3 pos){
            Position = pos;
            LastPosition = pos;
            
            OldSection = Vector3Int.zero;

        }

        public Particle() : this(Vector3.zero) {
        }
    }
}