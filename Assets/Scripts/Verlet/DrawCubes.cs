using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
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
        public float sectionSize;
        public float SectionSize => sectionSize < dropletScale ? dropletScale : sectionSize;

        private List<List<Matrix4x4>> batches = new List<List<Matrix4x4>>();

        private List<Particle> waterDroplets = new List<Particle>();

        private List<Vector3Int> availableSections = new List<Vector3Int>();

        private IDictionary<Vector3Int, List<Vector3Int>> adjacentSections =
            new Dictionary<Vector3Int, List<Vector3Int>>();

        private IDictionary<Vector3Int, List<int>> particlesInSection = new Dictionary<Vector3Int, List<int>>();

        public SphereCollider sphereCollider;

        public int subSteps = 2;

        public float threshold;
        
        private bool _doSim;

        public GameObject display;

        private void RenderBatches() {
            SetupBatches();

            foreach (var batch in batches) {
                for (int i = 0; i < mesh.subMeshCount; i++) {
                    Graphics.DrawMeshInstanced(mesh, i, material, batch);
                }
            }
        }

        public void SetupSections() {
            var collidersWhereWaterCanGo = new List<Collider>();

            foreach (var objectTransform in FindObjectsOfType(typeof(Transform))) {
                if (((Transform) objectTransform).gameObject.layer == LayerMask.NameToLayer("WaterAllowed")) {
                    var colliders = ((Transform) objectTransform).GetComponents<Collider>();
                    collidersWhereWaterCanGo.AddRange(colliders);
                }
            }

            availableSections = new List<Vector3Int>();

            foreach (var collider in collidersWhereWaterCanGo) {
                var bounds = collider.bounds;
                var min = new Vector3Int((int) Math.Truncate(bounds.min.x / SectionSize),
                    (int) Math.Truncate(bounds.min.y / SectionSize),
                    (int) Math.Truncate(bounds.min.z / SectionSize)) - Vector3Int.one;

                var max = new Vector3Int((int) Math.Truncate(bounds.max.x / SectionSize),
                    (int) Math.Truncate(bounds.max.y / SectionSize),
                    (int) Math.Truncate(bounds.max.z / SectionSize)) + Vector3Int.one;

                for (int x = min.x; x <= max.x; x++) {
                    for (int y = min.y; y <= max.y; y++) {
                        for (int z = min.z; z <= max.z; z++) {
                            var pos = new Vector3Int(x, y, z);
                            // Debug.Log(pos);
                            if (Physics.CheckBox((Vector3) pos * SectionSize,
                                    new Vector3(SectionSize / 2, SectionSize / 2, SectionSize / 2),
                                    Quaternion.identity, LayerMask.GetMask("WaterAllowed")))
                                availableSections.Add(pos);
                        }
                    }
                }
            }


            availableSections = availableSections.Distinct().ToList();

            foreach (var section in availableSections) {
                particlesInSection[section] = new List<int>();
                adjacentSections[section] = GetAdjacentSections(section);
            }
        }

        public void SetupParticles() {
            waterDroplets.Clear();

            var boxScale = transform.localScale;

            for (int i = 0; i < numberOfCubes; i++) {
                var particle = new Particle(this,
                    new Vector3(Random.value * boxScale.x - boxScale.x / 2,
                        Random.value * boxScale.y - boxScale.y / 2,
                        Random.value * boxScale.z - boxScale.z / 2)) {
                    Index = waterDroplets.Count - 1,
                    radius = dropletScale
                };

                waterDroplets.Add(particle);

                SetSection(particle);
            }
        }

        private void SetSection(Particle particle) {
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
            // TODO: Redo sections
            for (int subStep = 0; subStep < subSteps; subStep++) {
                float subTime = Time.fixedDeltaTime / (1 * (float) subSteps);

                foreach (var droplet in waterDroplets) {
                    var oldPos = droplet.Position;

                    droplet.Position += droplet.Position - droplet.LastPosition;
                    droplet.Position += Vector3.down * gravity * subTime * subTime;

                    CollideWithWorldSphereCast(droplet, oldPos);

                    droplet.inBounds = Physics.CheckSphere(droplet.Position, Single.Epsilon,
                        LayerMask.GetMask("WaterAllowed"));

                    if (!droplet.inBounds)
                        continue;

                    // List<int> dropletsToCheck = new List<int>();

                    // var sectionsToCheck = adjacentSections[droplet.Section];
                    //
                    // foreach (var section in sectionsToCheck) {
                    //     // this doesn't work 
                    //     if (!particlesInSection.ContainsKey(section)) {
                    //         Debug.Log(section);
                    //     }
                    //     else {
                    //         dropletsToCheck.AddRange(particlesInSection[section]);
                    //     }
                    // }

                    // Debug.Log(dropletsToCheck.Aggregate("", (s, i1) => s + ", " + i1));

                    // CollideWithOtherDroplets(droplet, dropletsToCheck); // TODO: Only thing that might not work rn (Yep doesn't work)

                    if (droplet.UpdateSection())
                        SetSection(droplet);

                    droplet.LastPosition = oldPos;
                }

                var i = 0;
                while (i < waterDroplets.Count) {
                    if (waterDroplets[i].inBounds) {
                        if (waterDroplets[i].Index != i) {
                            particlesInSection[waterDroplets[i].Section].Remove(waterDroplets[i].Index);
                            particlesInSection[waterDroplets[i].Section].Add(i);
                            waterDroplets[i].Index = i;
                        }

                        i++;
                        continue;
                    }

                    particlesInSection[waterDroplets[i].Section].Remove(waterDroplets[i].Index);
                    waterDroplets.RemoveAt(i);
                }
            }
        }

        public void Update() {
            if (drawInEditMode) {
                RenderBatches();
            }
        }

        private void FixedUpdate() {
            if (Application.isPlaying && _doSim) {
                UpdateWater();
            }
        }

        private void Start() {
            _doSim = false;

            if (sphereCollider == null) {
                if (!TryGetComponent(out sphereCollider)) {
                    sphereCollider = gameObject.AddComponent<SphereCollider>();
                }
            }

            // sphereCollider.isTrigger = true;
            
            SetupSections();
            SetupParticles();
            StartCoroutine(StartSim());
        }

        IEnumerator StartSim() {
            yield return new WaitForSeconds(1);
            _doSim = true;
        }

        public void CollideWithOtherDroplets(Particle droplet, List<int> dropletsToCheck) {
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
        }

        public void CollideWithWorldSphereCast(Particle droplet, Vector3 oldPos) {
            if (!Physics.CheckSphere(droplet.Position, dropletScale, LayerMask.GetMask("Default"))) return;

            CollideParticle(droplet, sphereCollider);
        }

        public static void CollideParticle(Particle droplet, SphereCollider collider) {
            // collider.center = droplet.Position;
            collider.radius = droplet.radius;

            var overlaps = Physics.OverlapSphere(droplet.Position, droplet.radius, LayerMask.GetMask("Default"));

            if (overlaps.Length == 0) {
                return;
            }

            var overlap = overlaps[0];

            var penetration = Physics.ComputePenetration(collider, droplet.Position, Quaternion.identity, overlap,
                overlap.transform.position, overlap.transform.rotation, out Vector3 direction, out float distance);
            
            
            if (penetration) {
                // Debug.Log(distance);

                droplet.Position = droplet.Position + direction * distance;
            }

            // Debug.DrawRay(droplet.Position, direction * distance, Color.green);
        }

        public static void CollideParticleContinuously(Particle droplet, Vector3 targetPos, float threshold) {
            
            var ray = new Ray(droplet.Position, targetPos - droplet.Position);
        
            if (Physics.SphereCast(ray, droplet.radius, out var hit)) {
            
                var targetPosThatMoves = targetPos;

                targetPosThatMoves -= ray.GetPoint(hit.distance);
            
                var worldToNormal = Quaternion.FromToRotation(hit.normal, Vector3.up);
                var normalToWorld = Quaternion.Inverse(worldToNormal);

                targetPosThatMoves = worldToNormal * targetPosThatMoves;

                targetPosThatMoves.y = 0;

                targetPosThatMoves = normalToWorld * targetPosThatMoves;
            

                targetPosThatMoves += ray.GetPoint(hit.distance);

                var rayDirection = targetPosThatMoves - targetPos;
                var rayToGoBackAlong = new Ray(targetPos, rayDirection);

                var distance = rayDirection.magnitude;

                targetPosThatMoves = rayToGoBackAlong.GetPoint(distance + threshold);
                droplet.Position = targetPosThatMoves;
            }
        }

        private void CheckSpherePath(Particle droplet, Vector3 currentTargetPos) { // dammit too slow
            var directionDistance = currentTargetPos - droplet.Position;

            var ray = new Ray(droplet.Position, directionDistance.normalized);

            // Debug.DrawRay(droplet.Position, directionDistance);
            if (Physics.SphereCast(ray, dropletScale, out RaycastHit hit, directionDistance.magnitude,
                    LayerMask.GetMask("Default"))) {

                var targetPosThatMoves = currentTargetPos;

                targetPosThatMoves -= ray.GetPoint(hit.distance);

                var worldToNormal = Quaternion.FromToRotation(hit.normal, Vector3.up);
                var normalToWorld = Quaternion.Inverse(worldToNormal);

                targetPosThatMoves = worldToNormal * targetPosThatMoves;

                targetPosThatMoves.y = 0;

                targetPosThatMoves = normalToWorld * targetPosThatMoves;

                targetPosThatMoves += ray.GetPoint(hit.distance);
                
                var rayDirection = targetPosThatMoves - currentTargetPos;
                var rayToGoBackAlong = new Ray(currentTargetPos, rayDirection);

                var distance = rayDirection.magnitude;

                targetPosThatMoves = rayToGoBackAlong.GetPoint(distance + threshold);
                
                // var newRay = new Ray(hit.point, (targetPosThatMoves - hit.point).normalized);
                // if (Physics.SphereCast(newRay, dropletScale, (targetPosThatMoves - hit.point).magnitude)) {
                    // droplet.Position = hit.point;
                    // CheckSpherePath(droplet, targetPosThatMoves);
                // }
                // else {
                    droplet.Position = targetPosThatMoves;

                    // Debug.Log(Physics.CheckSphere(droplet.Position, dropletScale));
                    
                    return;
                // }

                // var oldPos = droplet.Position;
                // CheckSpherePath(droplet, currentTargetPos);

                // return;
            }

            droplet.Position = currentTargetPos;
        }

        public List<Vector3Int> GetAdjacentSections(Vector3Int section) {
            List<Vector3Int> adjSectionsToCheck = new List<Vector3Int> {
                new Vector3Int(section.x - 1, section.y - 1, section.z - 1),
                new Vector3Int(section.x - 1, section.y - 1, section.z - 0),
                new Vector3Int(section.x - 1, section.y - 1, section.z + 1),

                new Vector3Int(section.x - 1, section.y - 0, section.z - 1),
                new Vector3Int(section.x - 1, section.y - 0, section.z - 0),
                new Vector3Int(section.x - 1, section.y - 0, section.z + 1),

                new Vector3Int(section.x - 1, section.y + 1, section.z - 1),
                new Vector3Int(section.x - 1, section.y + 1, section.z + 0),
                new Vector3Int(section.x - 1, section.y + 1, section.z + 1),

                new Vector3Int(section.x - 0, section.y - 1, section.z - 1),
                new Vector3Int(section.x - 0, section.y - 1, section.z - 0),
                new Vector3Int(section.x - 0, section.y - 1, section.z + 1),

                new Vector3Int(section.x - 0, section.y - 0, section.z - 1),
                new Vector3Int(section.x - 0, section.y - 0, section.z - 0),
                new Vector3Int(section.x - 0, section.y - 0, section.z + 1),

                new Vector3Int(section.x - 0, section.y + 1, section.z - 1),
                new Vector3Int(section.x - 0, section.y + 1, section.z + 0),
                new Vector3Int(section.x - 0, section.y + 1, section.z + 1),

                new Vector3Int(section.x + 1, section.y - 1, section.z - 1),
                new Vector3Int(section.x + 1, section.y - 1, section.z - 0),
                new Vector3Int(section.x + 1, section.y - 1, section.z + 1),

                new Vector3Int(section.x + 1, section.y - 0, section.z - 1),
                new Vector3Int(section.x + 1, section.y - 0, section.z - 0),
                new Vector3Int(section.x + 1, section.y - 0, section.z + 1),

                new Vector3Int(section.x + 1, section.y + 1, section.z - 1),
                new Vector3Int(section.x + 1, section.y + 1, section.z + 0),
                new Vector3Int(section.x + 1, section.y + 1, section.z + 1),
            };

            var adjSections = new List<Vector3Int>();

            foreach (var sectionToCheck in adjSectionsToCheck) {
                if (Physics.CheckBox((Vector3) sectionToCheck * SectionSize,
                        new Vector3(SectionSize / 2, SectionSize / 2, SectionSize / 2),
                        Quaternion.identity, LayerMask.GetMask("WaterAllowed"))) adjSections.Add(sectionToCheck);
            }

            return adjSections;
        }

        private void OnDrawGizmos() {
            foreach (var section in availableSections) {
                // DrawSection(section);
            }
            
            var x = Mathf.FloorToInt((display.transform.position.x + sectionSize / 2) / sectionSize);
            var y = Mathf.FloorToInt((display.transform.position.y + sectionSize / 2) / sectionSize);
            var z = Mathf.FloorToInt((display.transform.position.z + sectionSize / 2) / sectionSize);

            if (!adjacentSections.ContainsKey(new Vector3Int(x, y, z))) return;
            
            foreach (var section in adjacentSections[new Vector3Int(x, y, z)]) {
                // DrawSection(section);
            }
        }

        private void DrawSection(Vector3Int section) {
            var sectionPos = (Vector3) section * SectionSize;

            Debug.DrawLine(new Vector3(-SectionSize / 2, -SectionSize / 2, -SectionSize / 2) + sectionPos,
                new Vector3(-SectionSize / 2, -SectionSize / 2, SectionSize / 2) + sectionPos);
            Debug.DrawLine(new Vector3(-SectionSize / 2, -SectionSize / 2, SectionSize / 2) + sectionPos,
                new Vector3(SectionSize / 2, -SectionSize / 2, SectionSize / 2) + sectionPos);
            Debug.DrawLine(new Vector3(SectionSize / 2, -SectionSize / 2, SectionSize / 2) + sectionPos,
                new Vector3(SectionSize / 2, -SectionSize / 2, -SectionSize / 2) + sectionPos);
            Debug.DrawLine(new Vector3(SectionSize / 2, -SectionSize / 2, -SectionSize / 2) + sectionPos,
                new Vector3(-SectionSize / 2, -SectionSize / 2, -SectionSize / 2) + sectionPos);

            Debug.DrawLine(new Vector3(-SectionSize / 2, -SectionSize / 2, -SectionSize / 2) + sectionPos,
                new Vector3(-SectionSize / 2, SectionSize / 2, -SectionSize / 2) + sectionPos);
            Debug.DrawLine(new Vector3(-SectionSize / 2, -SectionSize / 2, SectionSize / 2) + sectionPos,
                new Vector3(-SectionSize / 2, SectionSize / 2, SectionSize / 2) + sectionPos);
            Debug.DrawLine(new Vector3(SectionSize / 2, -SectionSize / 2, SectionSize / 2) + sectionPos,
                new Vector3(SectionSize / 2, SectionSize / 2, SectionSize / 2) + sectionPos);
            Debug.DrawLine(new Vector3(SectionSize / 2, -SectionSize / 2, -SectionSize / 2) + sectionPos,
                new Vector3(SectionSize / 2, SectionSize / 2, -SectionSize / 2) + sectionPos);

            Debug.DrawLine(new Vector3(-SectionSize / 2, SectionSize / 2, -SectionSize / 2) + sectionPos,
                new Vector3(-SectionSize / 2, SectionSize / 2, SectionSize / 2) + sectionPos);
            Debug.DrawLine(new Vector3(-SectionSize / 2, SectionSize / 2, SectionSize / 2) + sectionPos,
                new Vector3(SectionSize / 2, SectionSize / 2, SectionSize / 2) + sectionPos);
            Debug.DrawLine(new Vector3(SectionSize / 2, SectionSize / 2, SectionSize / 2) + sectionPos,
                new Vector3(SectionSize / 2, SectionSize / 2, -SectionSize / 2) + sectionPos);
            Debug.DrawLine(new Vector3(SectionSize / 2, SectionSize / 2, -SectionSize / 2) + sectionPos,
                new Vector3(-SectionSize / 2, SectionSize / 2, -SectionSize / 2) + sectionPos);
        }
    }
}