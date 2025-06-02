using Menu;
using UnityEngine;
using UnityEngine.UIElements;

namespace UI
{
    public class MenuController : MonoBehaviour
    {
        private LevelLoad _levelLoad;

        [Header("UI Documents - Auto-assigned if left empty")]
        public UIDocument mainMenuDocument;
        public UIDocument tutorialDocument;
        public UIDocument optionsDocument;
        public UIDocument creditsDocument;

        [Header("Tutorial Data (SO)")]
        [Tooltip("Drag in your TutorialSlidesData ScriptableObject asset here.")]
        public TutorialSlidesData tutorialData;

        [Header("Slideshow Image Constraints")]
        [Tooltip("Maximum display width in pixels")]
        public int maxW = 500;
        [Tooltip("Maximum display height in pixels")]
        public int maxH = 300;

        // PlayerPrefs keys
        private const string ShowFPSKey = "ShowFPS";
        private const string VSyncEnabledKey = "VSyncEnabled"; // New key for VSync

        // Track initialization status for each menu
        private bool isMainMenuInitialized = false;
        private bool isTutorialMenuInitialized = false;
        private bool isOptionsMenuInitialized = false;
        private bool isCreditsMenuInitialized = false;

        // Current slide index
        private int currentSlideIndex = 0;

        private VisualElement currentMenu;

        private void Awake()
        {
            // Auto-find UI Documents if not assigned
            if (mainMenuDocument == null || tutorialDocument == null ||
                optionsDocument == null || creditsDocument == null)
            {
                AutoAssignUIDocuments();
            }

            // Ensure TutorialSlidesData is assigned
            if (tutorialDocument != null && tutorialData == null)
            {
                Debug.LogError("[MenuController] TutorialSlidesData asset not assigned! Please assign in the Inspector.");
            }

            _levelLoad = FindObjectOfType<LevelLoad>();
        }

        private void Start()
        {
            // Load VSync setting at game start
            ApplyVSyncSetting(PlayerPrefs.GetInt(VSyncEnabledKey, 1) == 1);

            // Initialize all menus as inactive first
            HideAllMenus();
            ShowMainMenu();
        }

        private void AutoAssignUIDocuments()
        {
            var allUIDocs = FindObjectsOfType<UIDocument>();
            foreach (var doc in allUIDocs)
            {
                var nameLower = doc.name.ToLower();
                if (nameLower.Contains("main") && mainMenuDocument == null)
                    mainMenuDocument = doc;
                else if (nameLower.Contains("tutorial") && tutorialDocument == null)
                    tutorialDocument = doc;
                else if (nameLower.Contains("options") && optionsDocument == null)
                    optionsDocument = doc;
                else if (nameLower.Contains("credits") && creditsDocument == null)
                    creditsDocument = doc;
            }

            Debug.Log($"Auto-assigned UI Documents: Main={mainMenuDocument?.name}, Tutorial={tutorialDocument?.name}, Options={optionsDocument?.name}, Credits={creditsDocument?.name}");
        }

        private void SetupMainMenu()
        {
            var root = mainMenuDocument.rootVisualElement;
            Debug.Log("Setting up Main Menu...");

            SetupButton(root, "PlayButton", () =>
            {
                Debug.Log("Play button clicked - Starting Game...");
                HideAllMenus();
                _levelLoad.LoadLevel();
            });

            SetupButton(root, "TutorialButton", ShowTutorialMenu);
            SetupButton(root, "OptionsButton", ShowOptionsMenu);
            SetupButton(root, "CreditsButton", ShowCreditsMenu);
            SetupButton(root, "QuitButton", QuitGame);
        }

        private void SetupTutorialMenu()
        {
            var root = tutorialDocument.rootVisualElement;
            Debug.Log("Setting up Tutorial Menu...");

            SetupButton(root, "PreviousButton", PreviousSlide);
            SetupButton(root, "NextButton", NextSlide);
            SetupButton(root, "BackToMainButton", ShowMainMenu);
        }

        private void SetupOptionsMenu()
        {
            var root = optionsDocument.rootVisualElement;
            Debug.Log("Setting up Options Menu...");

            // Volume sliders
            var masterSlider = root.Q<Slider>("MasterVolumeSlider");
            var sfxSlider    = root.Q<Slider>("SFXVolumeSlider");
            var masterLabel  = root.Q<Label>("MasterVolumeValue");
            var sfxLabel     = root.Q<Label>("SFXVolumeValue");

            if (masterSlider != null && masterLabel != null)
            {
                masterLabel.text = $"{masterSlider.value:F0}%";
                masterSlider.RegisterValueChangedCallback(evt =>
                {
                    masterLabel.text = $"{evt.newValue:F0}%";
                    AudioListener.volume = evt.newValue / 100f;
                });
            }

            if (sfxSlider != null && sfxLabel != null)
            {
                sfxLabel.text = $"{sfxSlider.value:F0}%";
                sfxSlider.RegisterValueChangedCallback(evt =>
                {
                    sfxLabel.text = $"{evt.newValue:F0}%";
                });
            }

            // Fullscreen & VSync toggles
            var fsToggle    = root.Q<Toggle>("FullscreenToggle");
            var vsyncToggle = root.Q<Toggle>("VSyncToggle");

            if (fsToggle != null)
            {
                fsToggle.value = Screen.fullScreen;
                fsToggle.RegisterValueChangedCallback(evt => Screen.fullScreen = evt.newValue);
            }

            if (vsyncToggle != null)
            {
                // Load saved VSync setting (default to true)
                bool vsyncEnabled = PlayerPrefs.GetInt(VSyncEnabledKey, 1) == 1;
                vsyncToggle.value = vsyncEnabled;
                
                vsyncToggle.RegisterValueChangedCallback(evt => 
                {
                    ApplyVSyncSetting(evt.newValue);
                    PlayerPrefs.SetInt(VSyncEnabledKey, evt.newValue ? 1 : 0);
                    PlayerPrefs.Save();
                });
            }

            // FPS Counter Toggle
            var fpsToggle = root.Q<Toggle>("FPSCounterToggle");
            if (fpsToggle != null)
            {
                // Load saved setting (default to false)
                fpsToggle.value = PlayerPrefs.GetInt(ShowFPSKey, 0) == 1;
                fpsToggle.RegisterValueChangedCallback(evt =>
                {
                    PlayerPrefs.SetInt(ShowFPSKey, evt.newValue ? 1 : 0);
                    PlayerPrefs.Save();
                });
            }

            SetupButton(root, "BackToMainButton", ShowMainMenu);
        }

        // Applies VSync setting and ensures it persists
        private void ApplyVSyncSetting(bool enabled)
        {
            QualitySettings.vSyncCount = enabled ? 1 : 0;
            Debug.Log($"VSync set to: {(enabled ? "Enabled" : "Disabled")}");
        }

        private void SetupCreditsMenu()
        {
            var root = creditsDocument.rootVisualElement;
            Debug.Log("Setting up Credits Menu...");
            SetupButton(root, "BackToMainButton", ShowMainMenu);
        }

        private void SetupButton(VisualElement root, string name, System.Action callback)
        {
            var btn = root.Q<Button>(name);
            if (btn != null) btn.clicked += callback;
            else Debug.LogError($"[MenuController] Button '{name}' not found on {root.name}");
        }

        public void ShowMainMenu()
        {
            Debug.Log("Showing Main Menu");
            HideAllMenus();

            mainMenuDocument.gameObject.SetActive(true);
            currentMenu = mainMenuDocument.rootVisualElement;

            if (!isMainMenuInitialized)
            {
                SetupMainMenu();
                isMainMenuInitialized = true;
            }
        }

        public void ShowTutorialMenu()
        {
            Debug.Log("Showing Tutorial Menu");
            HideAllMenus();

            tutorialDocument.gameObject.SetActive(true);
            currentMenu = tutorialDocument.rootVisualElement;

            if (!isTutorialMenuInitialized)
            {
                SetupTutorialMenu();
                isTutorialMenuInitialized = true;
            }

            currentSlideIndex = 0;
            UpdateTutorialSlide();
        }

        public void ShowOptionsMenu()
        {
            Debug.Log("Showing Options Menu");
            HideAllMenus();

            optionsDocument.gameObject.SetActive(true);
            currentMenu = optionsDocument.rootVisualElement;

            if (!isOptionsMenuInitialized)
            {
                SetupOptionsMenu();
                isOptionsMenuInitialized = true;
            }
        }

        public void ShowCreditsMenu()
        {
            Debug.Log("Showing Credits Menu");
            HideAllMenus();

            creditsDocument.gameObject.SetActive(true);
            currentMenu = creditsDocument.rootVisualElement;

            if (!isCreditsMenuInitialized)
            {
                SetupCreditsMenu();
                isCreditsMenuInitialized = true;
            }
        }

        private void HideAllMenus()
        {
            if (mainMenuDocument != null)
            {
                mainMenuDocument.gameObject.SetActive(false);
                isMainMenuInitialized = false;
            }

            if (tutorialDocument != null)
            {
                tutorialDocument.gameObject.SetActive(false);
                isTutorialMenuInitialized = false;
            }

            if (optionsDocument != null)
            {
                optionsDocument.gameObject.SetActive(false);
                isOptionsMenuInitialized = false;
            }

            if (creditsDocument != null)
            {
                creditsDocument.gameObject.SetActive(false);
                isCreditsMenuInitialized = false;
            }
        }

        private void PreviousSlide()
        {
            if (currentSlideIndex > 0)
            {
                currentSlideIndex--;
                UpdateTutorialSlide();
            }
        }

        private void NextSlide()
        {
            var slides = tutorialData?.slides;
            if (slides == null) return;

            if (currentSlideIndex < slides.Count - 1)
            {
                currentSlideIndex++;
                UpdateTutorialSlide();
            }
        }

        private void UpdateTutorialSlide()
        {
            if (tutorialDocument == null || !tutorialDocument.gameObject.activeInHierarchy)
                return;

            var slides = tutorialData?.slides;
            if (slides == null || slides.Count == 0)
                return;

            // Clamp index
            currentSlideIndex = Mathf.Clamp(currentSlideIndex, 0, slides.Count - 1);
            var slide = slides[currentSlideIndex];

            var root = tutorialDocument.rootVisualElement;

            // Assign title/description/counter via .text
            var titleLabel = root.Q<Label>("SlideTitle");
            if (titleLabel != null)
                titleLabel.text = slide.title;

            var descLabel = root.Q<Label>("SlideDescription");
            if (descLabel != null)
                descLabel.text = slide.description;

            var counterLabel = root.Q<Label>("SlideCounter");
            if (counterLabel != null)
                counterLabel.text = $"Slide {currentSlideIndex + 1} of {slides.Count}";

            // Slide image
            var imgVE = root.Q<VisualElement>("SlideImage");
            if (imgVE != null)
            {
                if (slide.slideImage != null)
                {
                    int texW = slide.slideImage.width;
                    int texH = slide.slideImage.height;
                    float scale = Mathf.Min(1f, (float)maxW / texW, (float)maxH / texH);
                    imgVE.style.width  = new StyleLength(new Length(Mathf.RoundToInt(texW * scale), LengthUnit.Pixel));
                    imgVE.style.height = new StyleLength(new Length(Mathf.RoundToInt(texH * scale), LengthUnit.Pixel));
                    imgVE.style.backgroundImage = new StyleBackground(slide.slideImage);
                }
                else
                {
                    imgVE.style.backgroundImage = StyleKeyword.Null;
                    imgVE.style.width  = StyleKeyword.Null;
                    imgVE.style.height = StyleKeyword.Null;
                    imgVE.ClearClassList();
                    imgVE.AddToClassList("slide-image");
                }
            }

            // Nav buttons
            root.Q<Button>("PreviousButton")?.SetEnabled(currentSlideIndex > 0);
            root.Q<Button>("NextButton")?.SetEnabled(currentSlideIndex < slides.Count - 1);

            // Indicators (if you have fixed count)
            for (int i = 0; i < 5; i++)
            {
                var indicator = root.Q<VisualElement>($"Indicator{i}");
                if (indicator != null)
                {
                    if (i == currentSlideIndex) indicator.AddToClassList("active");
                    else                         indicator.RemoveFromClassList("active");
                }
            }
        }

        private void QuitGame()
        {
    #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
    #else
            Application.Quit();
    #endif
        }
    }
}