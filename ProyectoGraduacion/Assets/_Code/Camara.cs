using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Camara : MonoBehaviour
{
    public Transform bg0;
    public float factor0 = 1f;
    
    public Transform bg1;
    public float factor1 = 1 / 2f;

    public Transform bg2;
    public float factor2 = 1 / 4f;

    private float displacement;
    private float iniCamPosFrame;
    private float nextCamPosFrame;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        iniCamPosFrame = transform.position.x;
    }
}
