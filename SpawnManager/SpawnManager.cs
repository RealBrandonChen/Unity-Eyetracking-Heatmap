using System.Collections;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using UnityEngine;
using UnityEngine.SceneManagement;
using PupilLabs;
using ObjBehaviour;
using CsvHelper;


public class SpawnManager : MonoBehaviour
{

    public GameObject[] cubes;
    public GameObject parentObj;
    //public GameObject[] initCubes;
    
    public string FilePath;

    public static int redCubes;
    //PlayerRotate playerRotate;
    // Start is called before the first frame update
    void Awake()
    {
        //playerRotate = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerRotate>();

        redCubes = 8;
        int num_cubes = cubes.Length;
        //Vector3[] CubePosList = new Vector3[10];
        //var posCSV = new StringBuilder();
        TextWriter tw = new StreamWriter(FilePath, false);       //write the cube position into .CSV file
        tw.WriteLine("CubeIndex" + "," + "CubePosX" + "," + "CubePosY" + "," + "CubePosZ");
        tw.Close();
        tw = new StreamWriter(FilePath, true);

        for (int i = 0; i < num_cubes;)
        {
            int randomX = UnityEngine.Random.Range(-6, 6);
            //int randomZ = UnityEngine.Random.Range(10, 20);
            int planeZ = 15;
            int randomY = UnityEngine.Random.Range(-6, 6);
            Vector3 Cube_Pos = new Vector3(randomX, randomY, planeZ);

            Collider[] hitColliders = Physics.OverlapSphere(Cube_Pos, 1.0f); //detect the surrounding cubes

            if (hitColliders.Length <= 1)                                   //if there is no overlapping cube, then generate cube
            {
                Instantiate(cubes[i], Cube_Pos, Quaternion.identity, parentObj.transform);

                tw.WriteLine(i + "," + randomX + "," + randomY + "," + planeZ);
    

                i++;
            }

            
        }
        tw.Close();
    }

    // Update is called once per frame
    void Update()
    {
        if (redCubes == 17)
        {
            StartCoroutine(SceneReload());
        }
    }

    IEnumerator SceneReload()
    {
        yield return new WaitForSeconds(8.0f);
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
