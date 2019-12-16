using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChooseType : MonoBehaviour
{
    public GameObject parentCanvas;

    // Start is called before the first frame update
    void Start()
    {
        Main.PauseGame(true);
        //parentCanvas = this.transform.root.gameObject;
    }

    public void TypeOfAI(int index )
    {
        Main.PauseGame(false);
        //Debug.Log("クリックしました");
        GameObject.Find("Battlelogic").GetComponent<BattleController>().defaultAI(index);
        Destroy(this.gameObject);
    }
}
