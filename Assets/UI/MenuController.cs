using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace UI
{
    public class MenuController : MonoBehaviour
    {
        [Header("UI Documents - Auto-assigned if left empty")]
        public UIDocument mainMenuDocument;
        public UIDocument tutorialDocument;
        public UIDocument optionsDocument;
        public UIDocument creditsDocument;
    
        [Header("Tutorial Data")]
        public List<TutorialSlide> tutorialSlides = new List<TutorialSlide>();
    
        private VisualElement currentMenu;
        private int currentSlideIndex = 0;

        // Track initialization status for each menu
        private bool isMainMenuInitialized = false;
        private bool isTutorialMenuInitialized = false;
        private bool isOptionsMenuInitialized = false;
        private bool isCreditsMenuInitialized = false;
    
        // Tutorial slide data structure
        [System.Serializable]
        public class TutorialSlide
        {
            public string title = "Tutorial Slide";
            [TextArea(3, 5)]
            public string description = "This is a tutorial slide description.";
            public Texture2D slideImage;
        }
    
        private void Awake()
        {
            // Auto-find UI Documents if not assigned
            if (mainMenuDocument == null || tutorialDocument == null || 
                optionsDocument == null || creditsDocument == null)
            {
                AutoAssignUIDocuments();
            }
            
            // Create default tutorial slides if none exist
            if (tutorialSlides.Count == 0)
            {
                CreateDefaultTutorialSlides();
            }
        }
    
        private void Start()
        {
            // Initialize all menus as inactive first
            HideAllMenus();
            ShowMainMenu();
        }
    
        private void AutoAssignUIDocuments()
        {
            UIDocument[] allUIDocuments = FindObjectsOfType<UIDocument>();
            
            foreach (UIDocument doc in allUIDocuments)
            {
                string docName = doc.name.ToLower();
                if (docName.Contains("main") && mainMenuDocument == null)
                    mainMenuDocument = doc;
                else if (docName.Contains("tutorial") && tutorialDocument == null)
                    tutorialDocument = doc;
                else if (docName.Contains("options") && optionsDocument == null)
                    optionsDocument = doc;
                else if (docName.Contains("credits") && creditsDocument == null)
                    creditsDocument = doc;
            }
            
            Debug.Log($"Auto-assigned UI Documents: Main={mainMenuDocument?.name}, Tutorial={tutorialDocument?.name}, Options={optionsDocument?.name}, Credits={creditsDocument?.name}");
        }
    
        private void CreateDefaultTutorialSlides()
        {
            tutorialSlides.Add(new TutorialSlide 
            { 
                title = "Welcome to Skyborne", 
                description = "Embark on an epic aerial adventure in the floating islands of Aetheria. Master the art of flight and discover ancient secrets hidden in the clouds." 
            });
            tutorialSlides.Add(new TutorialSlide 
            { 
                title = "Flight Controls", 
                description = "Use WASD to move your aircraft. Hold Shift to boost and Space to brake. Master these controls to navigate through challenging aerial courses." 
            });
            tutorialSlides.Add(new TutorialSlide 
            { 
                title = "Combat System", 
                description = "Left click to fire your primary weapon. Right click for secondary weapons. Use the mouse to aim and target enemies in thrilling dogfights." 
            });
            tutorialSlides.Add(new TutorialSlide 
            { 
                title = "Exploration", 
                description = "Discover hidden islands, ancient ruins, and mysterious artifacts. Each location holds secrets that will help you on your journey." 
            });
            tutorialSlides.Add(new TutorialSlide 
            { 
                title = "Ready to Fly", 
                description = "You're now ready to begin your adventure in Skyborne. Take to the skies and become the ultimate sky pilot!" 
            });
        }
    
        private void SetupMainMenu()
        {
            var root = mainMenuDocument.rootVisualElement;
            Debug.Log("Setting up Main Menu...");
    
            SetupButton(root, "PlayButton", () => {
                Debug.Log("Play button clicked - Starting Game...");
                // Add your game start logic here
                // SceneManager.LoadScene("GameScene");
            });
    
            SetupButton(root, "TutorialButton", () => {
                Debug.Log("Tutorial button clicked");
                ShowTutorialMenu();
            });
    
            SetupButton(root, "OptionsButton", () => {
                Debug.Log("Options button clicked");
                ShowOptionsMenu();
            });
    
            SetupButton(root, "CreditsButton", () => {
                Debug.Log("Credits button clicked");
                ShowCreditsMenu();
            });
    
            SetupButton(root, "QuitButton", () => {
                Debug.Log("Quit button clicked");
                QuitGame();
            });
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
    
            // Setup volume sliders
            var masterVolumeSlider = root.Q<Slider>("MasterVolumeSlider");
            var sfxVolumeSlider = root.Q<Slider>("SFXVolumeSlider");
            var masterVolumeValue = root.Q<Label>("MasterVolumeValue");
            var sfxVolumeValue = root.Q<Label>("SFXVolumeValue");

            if (masterVolumeSlider != null)
            {
                if (masterVolumeValue != null)
                    masterVolumeValue.text = $"{masterVolumeSlider.value:F0}%";
                
                masterVolumeSlider.RegisterValueChangedCallback(evt => {
                    if (masterVolumeValue != null)
                        masterVolumeValue.text = $"{evt.newValue:F0}%";
                    AudioListener.volume = evt.newValue / 100f;
                });
            }
    
            if (sfxVolumeSlider != null)
            {
                if (sfxVolumeValue != null)
                    sfxVolumeValue.text = $"{sfxVolumeSlider.value:F0}%";
                
                sfxVolumeSlider.RegisterValueChangedCallback(evt => {
                    if (sfxVolumeValue != null)
                        sfxVolumeValue.text = $"{evt.newValue:F0}%";
                });
            }
    
            // Setup toggles
            var fullscreenToggle = root.Q<Toggle>("FullscreenToggle");
            var vsyncToggle = root.Q<Toggle>("VSyncToggle");
    
            if (fullscreenToggle != null)
            {
                fullscreenToggle.value = Screen.fullScreen;
                fullscreenToggle.RegisterValueChangedCallback(evt => {
                    Screen.fullScreen = evt.newValue;
                });
            }
    
            if (vsyncToggle != null)
            {
                vsyncToggle.value = QualitySettings.vSyncCount > 0;
                vsyncToggle.RegisterValueChangedCallback(evt => {
                    QualitySettings.vSyncCount = evt.newValue ? 1 : 0;
                });
            }
    
            SetupButton(root, "BackToMainButton", ShowMainMenu);
        }
    
        private void SetupCreditsMenu()
        {
            var root = creditsDocument.rootVisualElement;
            Debug.Log("Setting up Credits Menu...");
            
            SetupButton(root, "BackToMainButton", ShowMainMenu);
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
                Debug.LogError($"Button '{buttonName}' not found in {root.name} UI!");
            }
        }   
    
        public void ShowMainMenu()
        {
            Debug.Log("Showing Main Menu");
            HideAllMenus();
            
            if (mainMenuDocument != null)
            {
                mainMenuDocument.gameObject.SetActive(true);
                currentMenu = mainMenuDocument.rootVisualElement;
                
                if (!isMainMenuInitialized)
                {
                    SetupMainMenu();
                    isMainMenuInitialized = true;
                }
            }
        }
    
        public void ShowTutorialMenu()
        {
            Debug.Log("Showing Tutorial Menu");
            HideAllMenus();
            
            if (tutorialDocument != null)
            {
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
        }
    
        public void ShowOptionsMenu()
        {
            Debug.Log("Showing Options Menu");
            HideAllMenus();
            
            if (optionsDocument != null)
            {
                optionsDocument.gameObject.SetActive(true);
                currentMenu = optionsDocument.rootVisualElement;
                
                if (!isOptionsMenuInitialized)
                {
                    SetupOptionsMenu();
                    isOptionsMenuInitialized = true;
                }
            }
        }
    
        public void ShowCreditsMenu()
        {
            Debug.Log("Showing Credits Menu");
            HideAllMenus();
            
            if (creditsDocument != null)
            {
                creditsDocument.gameObject.SetActive(true);
                currentMenu = creditsDocument.rootVisualElement;
                
                if (!isCreditsMenuInitialized)
                {
                    SetupCreditsMenu();
                    isCreditsMenuInitialized = true;
                }
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
            if (currentSlideIndex < tutorialSlides.Count - 1)
            {
                currentSlideIndex++;
                UpdateTutorialSlide();
            }
        }
    
        private void UpdateTutorialSlide()
        {
            if (tutorialDocument == null || !tutorialDocument.gameObject.activeInHierarchy || tutorialSlides.Count == 0) 
                return;
            
            var root = tutorialDocument.rootVisualElement;
            
            if (currentSlideIndex < tutorialSlides.Count)
            {
                var slide = tutorialSlides[currentSlideIndex];
            
                var slideTitle = root.Q<Label>("SlideTitle");
                var slideDescription = root.Q<Label>("SlideDescription");
                var slideCounter = root.Q<Label>("SlideCounter");
                
                if (slideTitle != null) slideTitle.text = slide.title;
                if (slideDescription != null) slideDescription.text = slide.description;
                if (slideCounter != null) slideCounter.text = $"Slide {currentSlideIndex + 1} of {tutorialSlides.Count}";
            
                // Update slide image
                var slideImage = root.Q<VisualElement>("SlideImage");
                if (slideImage != null)
                {
                    slideImage.style.backgroundImage = slide.slideImage != null 
                        ? new StyleBackground(slide.slideImage) 
                        : StyleKeyword.Null;
                }
            
                // Update navigation buttons
                var prevButton = root.Q<Button>("PreviousButton");
                var nextButton = root.Q<Button>("NextButton");
                
                if (prevButton != null) prevButton.SetEnabled(currentSlideIndex > 0);
                if (nextButton != null) nextButton.SetEnabled(currentSlideIndex < tutorialSlides.Count - 1);
            
                // Update slide indicators
                for (int i = 0; i < 5; i++)
                {
                    var indicator = root.Q<VisualElement>($"Indicator{i}");
                    if (indicator != null)
                    {
                        if (i == currentSlideIndex)
                            indicator.AddToClassList("active");
                        else
                            indicator.RemoveFromClassList("active");
                    }
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
