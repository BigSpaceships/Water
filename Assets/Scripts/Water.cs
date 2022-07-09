using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using UnityEngine;
using Color = UnityEngine.Color;

public class Water : MonoBehaviour {
    public Material waterMat;
    public Texture3D waterTexture;

    public GameObject plane;

    public Vector3Int size;
    [Range(0, 2)] public float scale;

    public bool newSeed = true;
    
    public void SetWater() {
        waterTexture = new Texture3D(size.x, size.y, size.z, TextureFormat.RGBA32, false);
        
        System.Random prng = new System.Random();
        Noise noise = new Noise();

        if (newSeed) {
            noise = new Noise(prng.Next());
        }
        

        for (int x = 0; x < waterTexture.width; x++) {
            for (int y = 0; y < waterTexture.height; y++) {
                for (int z = 0; z < waterTexture.depth; z++) {
                    // float brightness = noise.Evaluate(new Vector3(x, y, z) * scale);
                     var pos = new Vector3(x, y, z);
                     pos = size / 2 - pos;
                     float brightness = pos.magnitude * scale;
                    
                    waterTexture.SetPixel(x, y, z, new Color(brightness, brightness, brightness));
                }
            }
        }
        
        waterTexture.SetPixel(0, 0, 0, Color.red);
        
        waterTexture.Apply();
        
        waterMat.SetTexture("_Water", waterTexture);
        waterMat.SetVector("_Size", transform.localScale);
    }

    public void SetMesh(Mesh mesh) {
        GetComponent<MeshFilter>().mesh = mesh;
    }
}
