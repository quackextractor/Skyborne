using Abilities;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

public class ShopScript : MonoBehaviour
{
    public UIDocument shopUI;
    public UIDocument equipUI;

    private VisualElement _shopRoot;
    private VisualElement _equipRoot;
    private VisualElement _buttonRoot;

    private CurrencyManager _currencyManager;
    private Ability[] Abilities;
    private GameObject player;
    private PlayerAttackScript playerAttackScript;
    private bool b = false;
    private bool gameOver = false;
    private Ability ability;

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
        SetUIDisplay(_shopRoot, false);
        SetUIDisplay(_equipRoot, false);
    }

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

        _shopRoot = shopUI.rootVisualElement;
        _equipRoot = equipUI.rootVisualElement;

        SetupButton(_shopRoot, "ReturnToMenu", () => { SceneManager.LoadScene(0);  });
        SetupButton(_shopRoot, "QuitGame", QuitGame);

        SetupShop();
        SetupEquip();

        SetUIDisplay(_shopRoot, false);
        SetUIDisplay(_equipRoot, false);

        b = false;
    }

    void Update()
    {
        if (gameOver)
            return;

        if (Input.GetKeyDown(KeyCode.Tab))
        {
            b = !b;
            if (b)
            {
                UnityEngine.Cursor.lockState = CursorLockMode.None;
                Time.timeScale = 0f;
                player.GetComponent<PlayerController>().enabled = false;
            }
            else
            {
                UnityEngine.Cursor.lockState = CursorLockMode.Locked;
                Time.timeScale = 1f;
                player.GetComponent<PlayerController>().enabled = true;
            }

            SetUIDisplay(_shopRoot, b);
            SetUIDisplay(_equipRoot, false); // Equip UI only shown when triggered from button
        }
    }

    public void EnableAbility(string AbilityName)
    {
        var root = shopUI.rootVisualElement;

        foreach (Ability a in Abilities)
        {
            if (a.name == AbilityName && !a.Bought && _currencyManager.SpendMoney(a.Cost))
            {
                a.gameObject.SetActive(true);
                a.Bought = true;
                a.gameObject.SetActive(false);

                var shopButton = root.Q<Button>(AbilityName);
                if (shopButton != null)
                {
                    shopButton.AddToClassList("bought");
                    shopButton.SetEnabled(false);
                }

                // ✅ Update equip UI button color
                var equipButton = _equipRoot.Q<Button>(AbilityName);
                if (equipButton != null)
                {
                    equipButton.RemoveFromClassList("ability-locked");
                    equipButton.AddToClassList("ability-unlocked");
                }
            }
        }
    }



    public void SelectAbility(string abilityName)
    {
        foreach (Ability a in Abilities)
        {
            if (a.name == abilityName && a.Bought)
            {
                ability = a;
            }
        }
    }

    public void Equip(string input)
    {
        if (ability == null)
        {
            Debug.Log("Ability is not selected");
            return;
        }

        switch (input)
        {
            case "Q": playerAttackScript.SelectQAbility(ability); break;
            case "E": playerAttackScript.SelectEAbility(ability); break;
        }
    }

    public void DisableShop()
    {
        SetUIDisplay(_shopRoot, false);
        SetUIDisplay(_equipRoot, true);
    }

    private void SetupShop()
    {
        var root = shopUI.rootVisualElement;

        foreach (Ability a in Abilities)
        {
            string abilityName = a.name;
            Button button = root.Q<Button>(abilityName);

            if (button == null)
            {
                Debug.LogError($"Button for ability '{abilityName}' not found!");
                continue;
            }

            button.text = $"{abilityName} (${a.Cost})";
            button.clicked += () => EnableAbility(abilityName);

            if (a.Bought)
            {
                Debug.Log($"Graying out button for: {abilityName}");
                button.AddToClassList("bought");
                button.SetEnabled(false); // Optional: Disable click
            }
        }

        // Setup Inventory button
        SetupButton(root, "Inventory", () => {
            DisableShop();
        });
    }

    private void QuitGame() {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
    }

    private void SetupEquip()
    {
        foreach (var ability in Abilities)
        {
            var button = _equipRoot.Q<Button>(ability.name);
            if (button != null)
            {
                button.text = ability.name;

                if (ability.Bought)
                    button.AddToClassList("ability-unlocked");
                else
                    button.AddToClassList("ability-locked");

                button.clicked += () => SelectAbility(ability.name);
            }
        }

        SetupButton(_equipRoot, "Q", () => Equip("Q"));
        SetupButton(_equipRoot, "E", () => Equip("E"));
        SetupButton(_equipRoot, "Shop", () => {
            SetUIDisplay(_shopRoot, true);
            SetUIDisplay(_equipRoot, false);
        });
    }

    private void SetupAbilityButton(VisualElement root, string abilityName, int cost)
    {
        var button = root.Q<Button>(abilityName);
        var ability = GetAbilityByName(abilityName);

        if (button != null && ability != null)
        {
            button.text = ability.Bought ? "Bought" : "Buy";
            button.SetEnabled(!ability.Bought);

            button.clicked += () =>
            {
                if (!ability.Bought && _currencyManager.SpendMoney(ability.Cost))
                {
                    ability.Bought = true;
                    button.text = "Bought";
                    button.SetEnabled(false);
                }
            };
        }
    }

    private Ability GetAbilityByName(string name)
    {
        foreach (var a in Abilities)
        {
            if (a.name == name)
                return a;
        }
        return null;
    }
    private void SetupButton(VisualElement root, string buttonName, System.Action callback)
    {
        var button = root.Q<Button>(buttonName);
        if (button != null)
        {
            button.clicked += callback;
        }
        else
        {
            Debug.LogError($"Button '{buttonName}' not found in UI!");
        }
    }

    private void SetUIDisplay(VisualElement root, bool visible)
    {
        root.style.display = visible ? DisplayStyle.Flex : DisplayStyle.None;
    }
}
