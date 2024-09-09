using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Normal134 : MonoBehaviour
{
    public Vector3[] coord2;
    // Start is called before the first frame update
    void Start()
    {
        coord2 = GameObject.Find("Receiver").GetComponent<IReceiver>().GetCoord();
    }

    // Update is called once per frame
    void Update()
    {
        transform.position = (coord2[402 / 3] - coord2[405 / 3]) / 50 + Helpers.PointB;
    }
}

