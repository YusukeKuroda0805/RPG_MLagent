using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FeedbackSystem : MonoBehaviour
{
    
    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.G))
        {
            GetComponent<BattleController>().PushGoodKey();
        }
    }
}
