using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.XR;
using Random = UnityEngine.Random;

namespace Verlet {
    [ExecuteAlways]
    public class DrawCubes : MonoBehaviour {
        public int numberOfCubes;
        public Mesh mesh;
        public Material material;

        public bool drawInEditMode;

        [Range(0, 10)] public float gravity;
        [Range(0, 1)] public float drag;

        public float dropletScale = 10;
        public float sphereRadius = 7;
        public int sectionsInSphere = 8;

        private List<List<Matrix4x4>> batches = new List<List<Matrix4x4>>();

        private List<Particle> waterDroplets = new List<Particle>();

        private IDictionary<Vector3Int, List<int>> particlesInSection = new Dictionary<Vector3Int, List<int>>();

        public int subSteps = 2;

        private void RenderBatches() {
            SetupBatches();

            foreach (var batch in batches) {
                for (int i = 0; i < mesh.subMeshCount; i++) {
                    Graphics.DrawMeshInstanced(mesh, i, material, batch);
                }
            }
        }

        public void SetupParticles() {
            waterDroplets.Clear();

            for (int i = 0; i < numberOfCubes; i++) {
                var particle = new Particle(this, Random.onUnitSphere * Random.Range(0, sphereRadius)) {
                    Index = waterDroplets.Count
                };

                waterDroplets.Add(particle);

                SetSection(particle);
            }
        }

        private void SetSection(Particle particle) {
            if (!particlesInSection.ContainsKey(particle.Section)) {
                particlesInSection.Add(particle.Section, new List<int>());
            }

            if (!particlesInSection[particle.Section].Contains(particle.Index)) {
                particlesInSection[particle.Section].Add(particle.Index);
            }

            if (particlesInSection.ContainsKey(particle.OldSection)) {
                if (particlesInSection[particle.OldSection].Contains(particle.Index))
                    particlesInSection[particle.OldSection].Remove(particle.Index);
            }

            particle.OldSection = particle.OldOldSection;
        }

        public void SetupBatches() {
            int addedMatrices = 0;

            batches.Clear();

            batches.Add(new List<Matrix4x4>());

            foreach (var particle in waterDroplets) {
                if (addedMatrices >= 1000) {
                    batches.Add(new List<Matrix4x4>());
                    addedMatrices = 0;
                }

                batches[batches.Count - 1]
                    .Add(Matrix4x4.TRS(particle.Position, Quaternion.identity,
                        Vector3.one * dropletScale * 2)); // GOD I hate that 2. With a scale of 1 the radius is a half

                addedMatrices++;
            }
        }

        private void UpdateWater() {
            for (int i = 0; i < subSteps; i++) {
                float subTime = 1 / (100 * (float) subSteps);

                foreach (var droplet in waterDroplets) {

                    var oldPos = droplet.Position;

                    droplet.Position += droplet.Position - droplet.LastPosition;
                    droplet.Position += Vector3.down * gravity * subTime * subTime;

                    Vector3 direction = (droplet.Position - oldPos);
                    var ray = new Ray(oldPos, direction.normalized);
                    if (Physics.SphereCast(ray, dropletScale, out RaycastHit hit, direction.magnitude)) {

                        var posWhereDropletHitsObject = oldPos + direction.normalized * hit.distance;
                        var vectorLeftToTravel = droplet.Position - posWhereDropletHitsObject;

                        var reflectedVector = Vector3.Reflect(vectorLeftToTravel, hit.normal);

                        droplet.Position = posWhereDropletHitsObject + reflectedVector;
                        // oldPos = posWhereDropletHitsObject;

                    }

                    List<int> dropletsToCheck = new List<int>();

                    var sectionsToCheck = droplet.GetAdjacentSections();

                    foreach (var section in sectionsToCheck) {
                        if (particlesInSection.ContainsKey(section))
                            dropletsToCheck.AddRange(particlesInSection[section]);
                    }

                    // Debug.Log(droplet.Section);
                    foreach (var otherDropletIndex in dropletsToCheck) {
                        var otherDroplet = waterDroplets[otherDropletIndex];
                        if (otherDroplet == droplet) {
                            continue;
                        }

                        if ((otherDroplet.Position - droplet.Position).magnitude < 2 * dropletScale) {
                            var distBetween = 2 * dropletScale - (droplet.Position - otherDroplet.Position).magnitude;
                            // Debug.Log(distBetween);

                            var relativeVectorNormalized = (droplet.Position - otherDroplet.Position).normalized;

                            droplet.Position += relativeVectorNormalized * (distBetween / 2);
                            otherDroplet.Position += relativeVectorNormalized * -(distBetween / 2);
                        }
                    }

                    if (droplet.UpdateSection())
                        SetSection(droplet);
                    // Debug.Log(dropletsToCheck.Aggregate("", (s, i) => s + ", " + i));

                    droplet.LastPosition = oldPos;
                }
            }
        }

        public void Update() {
            if (Application.isPlaying) {
                UpdateWater();
            }

            if (drawInEditMode) {
                RenderBatches();
            }
        }

        private void Start() {
            SetupParticles();
        }
    }
}