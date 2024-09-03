using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Normal134 : MonoBehaviour
{
    public float[] coord2;
    // Start is called before the first frame update
    void Start()
    {
        coord2 = GameObject.Find("Receiver").GetComponent<Receiver>().coord;
    }

    // Update is called once per frame
    void Update()
    {
        transform.position = new Vector3(coord2[402]/50 -coord2[405]/50, coord2[403]/50-coord2[406]/50 -20 , coord2[404]/50-coord2[407]/50);
    }
}

