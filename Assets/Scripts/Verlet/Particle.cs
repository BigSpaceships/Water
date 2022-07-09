using System.Collections.Generic;
using UnityEngine;

namespace Verlet {
    public class Particle {
        public Vector3 Position;
        public Vector3 LastPosition;
        public Vector3Int Section;
        public Vector3Int OldSection;
        public Vector3Int OldOldSection;
        public int Index;

        private DrawCubes drawer;

        public Particle(DrawCubes drawer, Vector3 pos) {
            this.drawer = drawer;
            
            Position = pos;
            LastPosition = pos;
            
            OldSection = Vector3Int.zero;

            UpdateSection();
        }

        public bool UpdateSection() {
            var sizeOfSection = drawer.sphereRadius * 2 / drawer.sectionsInSphere;

            var x = Mathf.FloorToInt((Position.x + drawer.sphereRadius) / sizeOfSection);
            var y = Mathf.FloorToInt((Position.y + drawer.sphereRadius) / sizeOfSection);
            var z = Mathf.FloorToInt((Position.z + drawer.sphereRadius) / sizeOfSection);
            
            Section = new Vector3Int(x, y, z);

            var changed = OldSection != Section;

            OldOldSection = Section;

            return changed;
        }

        public List<Vector3Int> GetAdjacentSections() {
            List<Vector3Int> adjSections = new List<Vector3Int>();

            bool negX = Section.x > 0;
            bool posX = Section.x < drawer.sectionsInSphere - 1;
            bool negY = Section.y > 0;
            bool posY = Section.y < drawer.sectionsInSphere - 1;
            bool negZ = Section.z > 0;
            bool posZ = Section.z < drawer.sectionsInSphere - 1;

            // negative X
            if (negX && negY && negZ) adjSections.Add(Section + Vector3Int.left + Vector3Int.down + Vector3Int.back);
            if (negX && negY && true) adjSections.Add(Section + Vector3Int.left + Vector3Int.down);
            if (negX && negY && posZ) adjSections.Add(Section + Vector3Int.left + Vector3Int.down + Vector3Int.forward);

            if (negX && true && negZ) adjSections.Add(Section + Vector3Int.left + Vector3Int.back);
            if (negX && true && true) adjSections.Add(Section + Vector3Int.left);
            if (negX && true && posZ) adjSections.Add(Section + Vector3Int.left + Vector3Int.forward);
            
            if (negX && posY && negZ) adjSections.Add(Section + Vector3Int.left + Vector3Int.up + Vector3Int.back);
            if (negX && posY && true) adjSections.Add(Section + Vector3Int.left + Vector3Int.up);
            if (negX && posY && posZ) adjSections.Add(Section + Vector3Int.left + Vector3Int.up + Vector3Int.forward);
            
            // same X
            if (true && negY && negZ) adjSections.Add(Section + Vector3Int.down + Vector3Int.back);
            if (true && negY && true) adjSections.Add(Section + Vector3Int.down);
            if (true && negY && posZ) adjSections.Add(Section + Vector3Int.down + Vector3Int.forward);

            if (true && true && negZ) adjSections.Add(Section + Vector3Int.back);
            if (true && true && true) adjSections.Add(Section);
            if (true && true && posZ) adjSections.Add(Section + Vector3Int.forward);
            
            if (true && posY && negZ) adjSections.Add(Section + Vector3Int.up + Vector3Int.back);
            if (true && posY && true) adjSections.Add(Section + Vector3Int.up);
            if (true && posY && posZ) adjSections.Add(Section + Vector3Int.up + Vector3Int.forward);
            
            // positive X
            if (posX && negY && negZ) adjSections.Add(Section + Vector3Int.right + Vector3Int.down + Vector3Int.back);
            if (posX && negY && true) adjSections.Add(Section + Vector3Int.right + Vector3Int.down);
            if (posX && negY && posZ) adjSections.Add(Section + Vector3Int.right + Vector3Int.down + Vector3Int.forward);

            if (posX && true && negZ) adjSections.Add(Section + Vector3Int.right + Vector3Int.back);
            if (posX && true && true) adjSections.Add(Section + Vector3Int.right);
            if (posX && true && posZ) adjSections.Add(Section + Vector3Int.right + Vector3Int.forward);
            
            if (posX && posY && negZ) adjSections.Add(Section + Vector3Int.right + Vector3Int.up + Vector3Int.back);
            if (posX && posY && true) adjSections.Add(Section + Vector3Int.right + Vector3Int.up);
            if (posX && posY && posZ) adjSections.Add(Section + Vector3Int.right + Vector3Int.up + Vector3Int.forward);

            return adjSections;
        }
    }
}