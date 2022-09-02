using UnityEngine;

namespace Verlet {
    public class Testing : MonoBehaviour {
        public GameObject target;
        public GameObject display;
        public GameObject origin;

        public float threshold;

        private Particle droplet;

        private void Start() {
            droplet = new Particle {Radius = display.transform.localScale.x / 2};
        }

        private void Update() {
            if (Input.GetMouseButtonDown(0)) {
                Cursor.lockState = CursorLockMode.Locked;
            }

            if (Input.GetMouseButtonUp(0)) {
                Cursor.lockState = CursorLockMode.None;
            }
        
            if (Input.GetMouseButton(0)) {
                transform.Rotate(new Vector3(-Input.GetAxis("Mouse Y"), 0));
                transform.Rotate(Vector3.up, Input.GetAxis("Mouse X"), Space.World);
            }

            transform.Translate(new Vector3(Input.GetAxis("Horizontal"), Input.GetAxis("UpDown"), Input.GetAxis("Vertical")) * .7f, Space.Self);

            droplet.Position = origin.transform.position;
            
            // DrawCubes.CollideParticleContinuously(droplet, target.transform.position, threshold);
            DrawCubes.CollideParticle(droplet, GetComponent<SphereCollider>());
            
            display.transform.position = droplet.Position;
        }
    }
}
