using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LoadUI : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        JSONReader reader = new();
        List<Move> moves = reader.ReadMovesJSON();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
