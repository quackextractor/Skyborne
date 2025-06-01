using Abilities;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;


public class ShopScript : MonoBehaviour
{
    /**
     TO DO: MARK BUTTONS WHEN ARENT BOUGHT 
            CASES FOR WHEN A ABILTY ISNT BOUGHT 
            Debug.Log("ability is not selected"); replace with something that shows on the screen maybe make unresponsive
              un hard code button make the instatiate 
              
     */

    private CurrencyManager _currencyManager;



    void OnEnable()
    {
        GameMaster.OnGameOver += HandleGameOver;
        GameMaster.OnGameCompleted += HandleGameOver;
    }

    void OnDisable()
    {
        GameMaster.OnGameOver -= HandleGameOver;
        GameMaster.OnGameCompleted -= HandleGameOver;
    }

    private void HandleGameOver()
    {
        gameOver = true;
        ShopUi.SetActive(false);
        EquipUI.SetActive(false);
    }

    public GameObject ShopUi;
    public GameObject EquipUI;

    public UIDocument shopUI;
    public UIDocument equipUI;

    Ability[] Abilities;
    GameObject player;
    PlayerAttackScript playerAttackScript;
    bool b;
    private bool gameOver = false;
    // Start is called before the first frame update
    void Start()
    {
        _currencyManager = FindObjectOfType<CurrencyManager>();
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
                UnityEngine.Cursor.lockState = CursorLockMode.None;
                Time.timeScale = 0f;
                player.GetComponent<PlayerController>().enabled = false;
            }
            else {
                UnityEngine.Cursor.lockState = CursorLockMode.Locked;
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
            if (a.name == AbilityName && !a.Bought && _currencyManager.SpendMoney(a.Cost) )
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

    private void SetupShop()
    {
        var root = shopUI.rootVisualElement;
        Debug.Log("Setting up Main Menu...");

        SetupButton(root, "Fireball", () => {
            EnableAbility("Fireball");
        });

        SetupButton(root, "Gust", () => {
            EnableAbility("Gust");

        });

        SetupButton(root, "Burst", () => {
            EnableAbility("Burst");

        });

        SetupButton(root, "Inventory", () => {
            DisableShop();
        });
    }
    private void SetupEquip()
    {
        var root = shopUI.rootVisualElement;
        Debug.Log("Setting up Main Menu...");

        SetupButton(root, "Fireball", () => {
            SelectAbility("Fireball");
        });

        SetupButton(root, "Gust", () => {
            SelectAbility("Gust");

        });

        SetupButton(root, "Burst", () => {
            SelectAbility("Burst");

        });
        SetupButton(root, "Q", () => {
            Equip("Q");

        });
        SetupButton(root, "E", () => {
            Equip("E");

        });

        SetupButton(root, "Shop", () => {
            DisableShop();
        });
    }
    private void SetupButton(VisualElement root, string buttonName, System.Action callback)
    {
        var button = root.Q<Button>(buttonName);
        if (button != null)
        {
            button.clicked += callback;
            // Debug.Log($"Successfully set up button: {buttonName}");
        }
        else
        {
            Debug.LogError($"Button '{buttonName}' not found in UI!");
        }
    }
}
