using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EdgePosition : MonoBehaviour
{
    public float side;
    // Use this for initialization
    void Start()
    {

        transform.position = new Vector3(side * (Camera.main.orthographicSize * Camera.main.aspect), 0.0F, transform.position.z);
    }

    // Update is called once per frame
    void Update()
    {

    }
}
