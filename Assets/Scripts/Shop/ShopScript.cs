using Abilities;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShopScript : MonoBehaviour
{
   public GameObject ShopUi;
    public GameObject[] Abilities;
    public GameObject player;
    PlayerAttackScript playerAttackScript;
    bool b;
    // Start is called before the first frame update
    void Start()
    {
        Abilities = GameObject.FindGameObjectsWithTag("Abilities");
        player = GameObject.FindGameObjectWithTag("Player");
        playerAttackScript = player.GetComponent<PlayerAttackScript>();
        b = true;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tab)) { 
            b = !b;
            ShopUi.SetActive(b);
        };
    }

   public void EnableAbility(string AbilityName) {

        foreach (GameObject a in Abilities)
        {
            if (a.name == AbilityName)
            {
                a.SetActive(!a.active);
            }

        }
        playerAttackScript.refreshInvetory();
    }




}
