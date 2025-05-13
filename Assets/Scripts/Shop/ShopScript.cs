using Abilities;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShopScript : MonoBehaviour
{
    public GameObject ShopUi;
    public GameObject EquipUI;
    Ability[] Abilities;
    GameObject player;
    PlayerAttackScript playerAttackScript;
    bool b;
    // Start is called before the first frame update
    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player");
        playerAttackScript = player.GetComponent<PlayerAttackScript>();
        Abilities = playerAttackScript.GetComponentsInChildren<Ability>();
        foreach (Ability a in Abilities)
        {
                a.gameObject.SetActive(!a.gameObject.activeSelf);
        }
        b = true;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tab)) { 
            b = !b;
            if (b)
            {
                Cursor.lockState = CursorLockMode.None;
            }
            else {
                Cursor.lockState = CursorLockMode.Locked;
                EquipUI.SetActive(b);
            }
            ShopUi.SetActive(b);
        };
    }

   public void EnableAbility(string AbilityName) {

        foreach (Ability a in Abilities)
        {
            if (a.name == AbilityName)
            {
                //a.gameObject.SetActive(!a.gameObject.activeSelf);
                a.Bought = true;
            }
        }
        playerAttackScript.refreshInvetory();
    }
    public void DisableShop() { 
        ShopUi.SetActive(!ShopUi.activeSelf);
        EquipUI.SetActive(!EquipUI.activeSelf);
    }
    public void LoadAbilities()
    {

    }



}
