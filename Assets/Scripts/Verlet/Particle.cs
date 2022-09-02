using System.Collections.Generic;
using System.Collections.Specialized;
using UnityEngine;

namespace Verlet {
    public class Particle {
        public Vector3 Position;
        public Vector3 LastPosition;
        
        public int SectionInt;
        public int Index;

        public bool InBounds = true;

        public bool Active => InBounds;

        public float Radius;

        public Particle(DrawCubes drawer, Vector3 pos) : this(pos) {
            SectionInt = Util.GetIntSection(Util.GetSection(Position));
        }

        public Particle(Vector3 pos){
            Position = pos;
            LastPosition = pos;
        }

        public Particle() : this(Vector3.zero) {
        }
    }

    public struct ParticleStruct {
        public Vector3 Position;
        public Vector3 LastPosition;

        public int Section;
        public int Index;

        public float Radius;

        public bool InBounds;
        public bool Active => InBounds;
    }
}