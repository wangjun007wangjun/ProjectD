using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BornCube : MonoBehaviour
{
    public GameObject cube;
    // Start is called before the first frame update
    void Start()
    {
        float temp = 5.625f;
        for (int i = 0; i < 64; i++)
        {
            GameObject cubeT = Instantiate(cube) as GameObject;
            cubeT.SetActive(true);
            cubeT.transform.localPosition = Vector3.zero;
            cubeT.transform.localEulerAngles = new Vector3(0, temp * i, 0);
        }
    }
}
