using Abilities;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShopScript : MonoBehaviour
{
    /**
     TO DO: MARK BUTTONS WHEN ARENT BOUGHT 
            CASES FOR WHEN A ABILTY ISNT BOUGHT 
            MAKE IT SO YOU CANT HAVE DUPLICATE ABILITY IN YOUR SLOTS 
            MAKE UI PRETTIER
            Debug.Log("ability is not selected"); replace with something that shows on the screen maybe make unresponsive
            Pause the game when turning on the shop
            
     */




    void OnEnable()
    {
        GameMaster.OnGameOver += HandleGameOver;
    }

    void OnDisable()
    {
        GameMaster.OnGameOver -= HandleGameOver;
    }

    private void HandleGameOver()
    {
        gameOver = true;
        ShopUi.SetActive(false);
        EquipUI.SetActive(false);
    }

    public GameObject ShopUi;
    public GameObject EquipUI;
    Ability[] Abilities;
    GameObject player;
    PlayerAttackScript playerAttackScript;
    bool b;
    private bool gameOver = false;
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
        b = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (gameOver)
            return;
        if (Input.GetKeyDown(KeyCode.Tab)) { 
            b = !b;
            if (b)
            {
                Cursor.lockState = CursorLockMode.None;
                Time.timeScale = 0f;
                player.GetComponent<PlayerController>().enabled = false;
            }
            else {
                Cursor.lockState = CursorLockMode.Locked;
                Time.timeScale = 1f;
                player.GetComponent<PlayerController>().enabled = true;
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
                a.gameObject.SetActive(true);
                a.Bought = true;
                a.gameObject.SetActive(false);
            }
        }
    }
    public void DisableShop() { 
        ShopUi.SetActive(!ShopUi.activeSelf);
        EquipUI.SetActive(!EquipUI.activeSelf);
    }


    Ability ability;
    public void SelectAbility(string AbilityName)
    {
        foreach (Ability a in Abilities)
        {
            if (a.name == AbilityName && a.Bought)
            {
                   ability = a;
            }
        }
    }

    public void Equip(string input) {
       
        if (ability == null)
        {
            Debug.Log("ability is not selected");
            return;
        }
        switch (input) {
            case "Q": playerAttackScript.SelectQAbility(ability);   
                break;
            case "E": playerAttackScript.SelectEAbility(ability);
                break;
            default: break;
        }
    }



}
