using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.EventSystems;
using PupilLabs;

namespace ObjBehaviour
{
    public class TrackingTarget : MonoBehaviour
    {
        public Color targetLockedColor = new Color(37, 214, 243);
        public Color targetOnColor = Color.red;
        public float targetOnTime = 0f;
        
        public AudioClip colorChangeClip;
        AudioSource cubeAudio;
        MeshRenderer rend;
        int cubeLayer;
        Color originalColor;
        
        // Start is called before the first frame update
        void Awake()
        {
            rend = GetComponent<MeshRenderer>();
            cubeAudio = GetComponent<AudioSource>();
            originalColor = rend.material.color;

            Char[] clone = { '(', 'C', 'l', 'o', 'n', 'e', ')' };
            cubeLayer = LayerMask.NameToLayer(gameObject.name.TrimEnd(clone));
            enabled = false;
        }
        // Update is called once per frame
        void Update()
        {
            if (SpawnManager.redCubes == cubeLayer)
            {
                if (enabled) //check if the hitpoint is on the cube
                {
                    targetOnTime += Time.deltaTime;
                }
                if (targetOnTime >= 3.0f)   //check if the floating time of the hitpoint is greater than 3 seconds,
                                            //if so, color -> ColorChange 
                                            //if not, the color should change back to its original color when the hitpoint leaves
                {
                    ColorChange();
                    SoundEffectPlay();
                }
            }
        }

        private void OnEnable()
        {
            if (rend.material.color != targetOnColor) //make sure the color won't change back to blue if the cube is red
            {
                Material[] objMaterials = rend.materials;
                foreach (Material material in objMaterials)
                {
                    material.color = targetLockedColor;
                }
            }
        }

        private void OnDisable()
        {
            targetOnTime = 0f;
            if (rend.material.color == targetLockedColor)
            {
                Material[] objMaterials = rend.materials;
                foreach (Material material in objMaterials)
                {
                    material.color = originalColor;
                }
            }
        }

        void ColorChange()
        {
            Material[] objMaterials = rend.materials;
            foreach (Material material in objMaterials)
            {
                material.color = targetOnColor;
            }
            SpawnManager.redCubes += 1;
        }
        void SoundEffectPlay()
        {
            cubeAudio.clip = colorChangeClip;
            cubeAudio.Play();
        }
    }
}

        
        
    


        
    
