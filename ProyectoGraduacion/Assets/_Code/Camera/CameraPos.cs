using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraPos : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        if (players.Length == 0)
            return;

        Vector3 averagePosition = Vector3.zero;

        foreach (GameObject player in players)
        {
            averagePosition += player.transform.position;
        }

        averagePosition /= players.Length;

        transform.position = averagePosition;
    }
}
