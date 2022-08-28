using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Verlet {
    [ExecuteAlways]
    public class DrawCubes : MonoBehaviour {
        
        [Header("Setup Settings")]
        [SerializeField] private int numberOfCubes;
        [SerializeField] private Mesh mesh;
        [SerializeField] private Material material;
        
        [Space] 
        [SerializeField] private SphereCollider sphereCollider;
        
        [Space]
        public float dropletScale = 10;
        public float sectionSize;
        public float SectionSize => sectionSize < dropletScale ? dropletScale : sectionSize;
        
        [SerializeField] private bool drawInEditMode;

        [Header("Simulation Settings")]
        [Range(0, 10)] public float gravity;
        [Range(0, 1)] public float drag;
        
        [Space]
        // [SerializeField] private float threshold; // Used for continuous collisions 
        [SerializeField] private int subSteps = 2;
        
        private enum RenderTypes {
            All,
            AllWithDisplayHighlighted,
            AroundDisplay,
            None,
        }
        
        [Header("Section Display")]
        [SerializeField] private RenderTypes renderType;
        [SerializeField] private GameObject sectionDisplay;
        [SerializeField] private Color sectionColor = Color.white;
        
        private List<List<Matrix4x4>> batches = new List<List<Matrix4x4>>();

        private List<Particle> waterDroplets = new List<Particle>();

        private List<Vector3Int> availableSections = new List<Vector3Int>();

        private int[,] _adjacentSections;

        private List<int>[] _particlesInSection;
        
        private List<Vector3Int>[] _sections;
        
        private bool _doSim;

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
                            
                            if (Physics.CheckBox((Vector3) pos * SectionSize,
                                    new Vector3(SectionSize / 2, SectionSize / 2, SectionSize / 2),
                                    Quaternion.identity, LayerMask.GetMask("WaterAllowed")))
                                availableSections.Add(pos);
                        }
                    }
                }
            }

            availableSections = availableSections.Distinct().ToList();

            Util.MinSections = availableSections.Aggregate(new Vector3Int(int.MaxValue, int.MaxValue, int.MaxValue), Vector3Int.Min);
            Util.MaxSections = availableSections.Aggregate(new Vector3Int(int.MinValue, int.MinValue, int.MinValue), Vector3Int.Max);;

            var adjacentSections = new Dictionary<Vector3Int, List<Vector3Int>>();
            
            foreach (var section in availableSections) {
                adjacentSections[section] = GetAdjacentSections(section);
            }

            _adjacentSections = new int[Util.NumberOfSections, 27];
            
            foreach (var (key, value) in adjacentSections) {
                int sectionIndex = Util.GetIntSection(key);

                for (int i = 0; i < 27; i++) {
                    try {
                        int sectionToAssign = i < value.Count ? Util.GetIntSection(value[i]) : -1;
                        _adjacentSections[sectionIndex, i] = sectionToAssign;
                    }
                    catch (Exception e) {
                        Debug.LogException(e);
                    }
                }
            }

            _particlesInSection = new List<int>[Util.NumberOfSections];
            for (int i = 0; i < (_particlesInSection.Length); i++) {
                _particlesInSection[i] = new List<int>();
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
                    Index = waterDroplets.Count,
                    radius = dropletScale,
                    Drawer = this,
                };

                waterDroplets.Add(particle);
                particle.SectionInt = Util.GetIntSection(particle.Section);

                SetSection(particle);
            }
        }

        private void SetSection(Particle particle) {
            if (particle.SectionChanged) {
                _particlesInSection[Util.GetIntSection(particle.OldSection)].Remove(particle.Index);
                particle.SectionChanged = false;
            }
            
            if (!_particlesInSection[particle.SectionInt].Contains(particle.Index)) {
                _particlesInSection[particle.SectionInt].Add(particle.Index);
            }

            particle.OldSection = particle.OldOldSection;
        }

        private void SetupBatches() {
            int addedMatrices = 0;

            batches.Clear();

            batches.Add(new List<Matrix4x4>());

            foreach (var particle in waterDroplets) {
                if (!particle.Active) continue;

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
            for (int subStep = 0; subStep < subSteps; subStep++) {
                float subTime = Time.fixedDeltaTime / (1 * (float) subSteps);

                foreach (var droplet in waterDroplets) {
                    if (!droplet.Active) continue;
                    
                    var oldPos = droplet.Position;

                    droplet.Position += droplet.Position - droplet.LastPosition;
                    droplet.Position += Vector3.down * gravity * subTime * subTime;

                    CollideWithWorldSphereCast(droplet, oldPos);

                    droplet.InBounds = Physics.CheckSphere(droplet.Position, Single.Epsilon,
                        LayerMask.GetMask("WaterAllowed"));

                    if (!droplet.Active)
                        continue;

                    var dropletsToCheck = new List<int>();

                    for (int i = 0; i < 27; i++) {
                        var section = _adjacentSections[droplet.SectionInt, i];
                        if (section == -1) continue;
                        
                        dropletsToCheck.AddRange(_particlesInSection[section]);
                    }
                    

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

                    CollideWithOtherDroplets(droplet, dropletsToCheck);

                    if (droplet.UpdateSection())
                        SetSection(droplet);

                    droplet.LastPosition = oldPos;
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

        public void Start() {
            _doSim = false;

            Util.SectionSize = SectionSize;

            SetupSections();
            SetupParticles();
            StartCoroutine(StartSim());
        }

        IEnumerator StartSim() {
            yield return new WaitForSeconds(1);
            _doSim = true;
            // Debug.Break();
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
            collider.radius = droplet.radius;

            var overlaps = new Collider[16];
            Physics.OverlapSphereNonAlloc(droplet.Position, droplet.radius, overlaps, LayerMask.GetMask("Default"));
            
            var distances = new List<Vector3>();
            
            foreach (var overlap in overlaps) {
                if (overlap == null) {
                    break;
                }
                
                var penetration = Physics.ComputePenetration(collider, droplet.Position, Quaternion.identity, overlap,
                    overlap.transform.position, overlap.transform.rotation, out Vector3 direction, out float distance);

                if (penetration) {
                    distances.Add(direction * distance);
                }
            }

            foreach (var vector3 in distances) {
                // Debug.DrawRay(droplet.Position, vector3, Color.yellow);
                droplet.Position += vector3;
            }

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

        private List<Vector3Int> GetAdjacentSections(Vector3Int section) {
            List<Vector3Int> adjSectionsToCheck = new List<Vector3Int> {
                new(section.x - 1, section.y - 1, section.z - 1),
                new(section.x - 1, section.y - 1, section.z - 0),
                new(section.x - 1, section.y - 1, section.z + 1),

                new(section.x - 1, section.y - 0, section.z - 1),
                new(section.x - 1, section.y - 0, section.z - 0),
                new(section.x - 1, section.y - 0, section.z + 1),

                new(section.x - 1, section.y + 1, section.z - 1),
                new(section.x - 1, section.y + 1, section.z + 0),
                new(section.x - 1, section.y + 1, section.z + 1),

                new(section.x - 0, section.y - 1, section.z - 1),
                new(section.x - 0, section.y - 1, section.z - 0),
                new(section.x - 0, section.y - 1, section.z + 1),

                new(section.x - 0, section.y - 0, section.z - 1),
                new(section.x - 0, section.y - 0, section.z - 0),
                new(section.x - 0, section.y - 0, section.z + 1),

                new(section.x - 0, section.y + 1, section.z - 1),
                new(section.x - 0, section.y + 1, section.z + 0),
                new(section.x - 0, section.y + 1, section.z + 1),

                new(section.x + 1, section.y - 1, section.z - 1),
                new(section.x + 1, section.y - 1, section.z - 0),
                new(section.x + 1, section.y - 1, section.z + 1),

                new(section.x + 1, section.y - 0, section.z - 1),
                new(section.x + 1, section.y - 0, section.z - 0),
                new(section.x + 1, section.y - 0, section.z + 1),

                new(section.x + 1, section.y + 1, section.z - 1),
                new(section.x + 1, section.y + 1, section.z + 0),
                new(section.x + 1, section.y + 1, section.z + 1),
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
            switch (renderType) {
                case RenderTypes.AllWithDisplayHighlighted:
                    foreach (var section in availableSections) {
                        Util.DrawSection(section, sectionColor);
                    }
                    
                    if (sectionDisplay != null) {
                        var section = Util.GetSection(sectionDisplay.transform.position);
                        var sectionIndex = Util.GetIntSection(section);

                        Util.DrawSection(Util.GetVector3IntSection(sectionIndex), Color.red);
                    }
                    
                    break;
                case RenderTypes.All:
                    foreach (var section in availableSections) {
                        Util.DrawSection(section, sectionColor);
                    }

                    break;
                case RenderTypes.AroundDisplay:
                    if (sectionDisplay != null) {
                        var section = Util.GetSection(sectionDisplay.transform.position);
                        var sectionIndex = Util.GetIntSection(section);

                        if (!Util.SectionExists(sectionIndex)) return;

                        for (int i = 0; i < 27; i++) {
                            var indexOfSectionToDraw = _adjacentSections[sectionIndex, i];

                            if (indexOfSectionToDraw == -1) {
                                continue;
                            }
                            Util.DrawSection(Util.GetVector3IntSection(indexOfSectionToDraw), sectionColor);
                        }

                        Util.DrawSection(Util.GetVector3IntSection(sectionIndex), Color.red);
                    }
                    
                    break;
            }
        }
    }
}