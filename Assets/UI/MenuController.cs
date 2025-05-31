using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace UI
{
    public class MenuController : MonoBehaviour
    {
        [Header("UI Documents")]
        public UIDocument mainMenuDocument;
        public UIDocument tutorialDocument;
        public UIDocument optionsDocument;
        public UIDocument creditsDocument;
    
        [Header("Tutorial Data")]
        public List<TutorialSlide> tutorialSlides;
    
        private VisualElement currentMenu;
        private int currentSlideIndex = 0;
    
        // Tutorial slide data structure
        [System.Serializable]
        public class TutorialSlide
        {
            public string title;
            [TextArea(3, 5)]
            public string description;
            public Texture2D slideImage;
        }
    
        private void Start()
        {
            InitializeMenus();
            ShowMainMenu();
        }
    
        private void InitializeMenus()
        {
            // Initialize Main Menu
            SetupMainMenu();
        
            // Initialize Tutorial Menu
            SetupTutorialMenu();
        
            // Initialize Options Menu
            SetupOptionsMenu();
        
            // Initialize Credits Menu
            SetupCreditsMenu();
        
            // Hide all menus initially
            HideAllMenus();
        }
    
        private void SetupMainMenu()
        {
            var root = mainMenuDocument.rootVisualElement;
    
            // Add null checks and error handling
            var playButton = root.Q<Button>("PlayButton");
            var tutorialButton = root.Q<Button>("TutorialButton");
            var optionsButton = root.Q<Button>("OptionsButton");
            var creditsButton = root.Q<Button>("CreditsButton");
            var quitButton = root.Q<Button>("QuitButton");
    
            if (playButton != null)
            {
                playButton.clicked += () => {
                    Debug.Log("Play button clicked - Starting Game...");
                    // Add game start logic here
                };
            }
            else
            {
                Debug.LogError("PlayButton not found in MainMenu UXML");
            }
    
            if (tutorialButton != null)
            {
                tutorialButton.clicked += () => {
                    Debug.Log("Tutorial button clicked");
                    ShowTutorialMenu();
                };
            }
            else
            {
                Debug.LogError("TutorialButton not found in MainMenu UXML");
            }
    
            if (optionsButton != null)
            {
                optionsButton.clicked += () => {
                    Debug.Log("Options button clicked");
                    ShowOptionsMenu();
                };
            }
            else
            {
                Debug.LogError("OptionsButton not found in MainMenu UXML");
            }
    
            if (creditsButton != null)
            {
                creditsButton.clicked += () => {
                    Debug.Log("Credits button clicked");
                    ShowCreditsMenu();
                };
            }
            else
            {
                Debug.LogError("CreditsButton not found in MainMenu UXML");
            }
    
            if (quitButton != null)
            {
                quitButton.clicked += () => {
                    Debug.Log("Quit button clicked");
                    QuitGame();
                };
            }
            else
            {
                Debug.LogError("QuitButton not found in MainMenu UXML");
            }
        }
    
        private void SetupTutorialMenu()
        {
            var root = tutorialDocument.rootVisualElement;
    
            var previousButton = root.Q<Button>("PreviousButton");
            var nextButton = root.Q<Button>("NextButton");
            var backButton = root.Q<Button>("BackToMainButton");
    
            if (previousButton != null)
            {
                previousButton.clicked += () => {
                    Debug.Log("Previous slide button clicked");
                    PreviousSlide();
                };
            }
            else
            {
                Debug.LogError("PreviousButton not found in TutorialMenu UXML");
            }
    
            if (nextButton != null)
            {
                nextButton.clicked += () => {
                    Debug.Log("Next slide button clicked");
                    NextSlide();
                };
            }
            else
            {
                Debug.LogError("NextButton not found in TutorialMenu UXML");
            }
    
            if (backButton != null)
            {
                backButton.clicked += () => {
                    Debug.Log("Back to main menu from tutorial clicked");
                    ShowMainMenu();
                };
            }
            else
            {
                Debug.LogError("BackToMainButton not found in TutorialMenu UXML");
            }
    
            // Initialize tutorial content
            UpdateTutorialSlide();
        }
    
        private void SetupOptionsMenu()
        {
            var root = optionsDocument.rootVisualElement;
    
            // Setup volume sliders with null checks
            var masterVolumeSlider = root.Q<Slider>("MasterVolumeSlider");
            var sfxVolumeSlider = root.Q<Slider>("SFXVolumeSlider");
    
            if (masterVolumeSlider != null)
            {
                masterVolumeSlider.RegisterValueChangedCallback(evt => {
                    var valueLabel = root.Q<Label>("MasterVolumeValue");
                    if (valueLabel != null)
                    {
                        valueLabel.text = $"{evt.newValue:F0}%";
                    }
                    AudioListener.volume = evt.newValue / 100f;
                    Debug.Log($"Master volume changed to: {evt.newValue}%");
                });
            }
            else
            {
                Debug.LogError("MasterVolumeSlider not found in OptionsMenu UXML");
            }
    
            if (sfxVolumeSlider != null)
            {
                sfxVolumeSlider.RegisterValueChangedCallback(evt => {
                    var valueLabel = root.Q<Label>("SFXVolumeValue");
                    if (valueLabel != null)
                    {
                        valueLabel.text = $"{evt.newValue:F0}%";
                    }
                    Debug.Log($"SFX volume changed to: {evt.newValue}%");
                    // Set SFX volume here
                });
            }
            else
            {
                Debug.LogError("SFXVolumeSlider not found in OptionsMenu UXML");
            }
    
            // Setup toggles with null checks
            var fullscreenToggle = root.Q<Toggle>("FullscreenToggle");
            var vsyncToggle = root.Q<Toggle>("VSyncToggle");
    
            if (fullscreenToggle != null)
            {
                fullscreenToggle.RegisterValueChangedCallback(evt => {
                    Screen.fullScreen = evt.newValue;
                    Debug.Log($"Fullscreen toggled: {evt.newValue}");
                });
            }
            else
            {
                Debug.LogError("FullscreenToggle not found in OptionsMenu UXML");
            }
    
            if (vsyncToggle != null)
            {
                vsyncToggle.RegisterValueChangedCallback(evt => {
                    QualitySettings.vSyncCount = evt.newValue ? 1 : 0;
                    Debug.Log($"VSync toggled: {evt.newValue}");
                });
            }
            else
            {
                Debug.LogError("VSyncToggle not found in OptionsMenu UXML");
            }
    
            var backButton = root.Q<Button>("BackToMainButton");
            if (backButton != null)
            {
                backButton.clicked += () => {
                    Debug.Log("Back to main menu from options clicked");
                    ShowMainMenu();
                };
            }
            else
            {
                Debug.LogError("BackToMainButton not found in OptionsMenu UXML");
            }
        }
    
        private void SetupCreditsMenu()
        {
            var root = creditsDocument.rootVisualElement;
            var backButton = root.Q<Button>("BackToMainButton");
    
            if (backButton != null)
            {
                backButton.clicked += () => {
                    Debug.Log("Back to main menu from credits clicked");
                    ShowMainMenu();
                };
            }
            else
            {
                Debug.LogError("BackToMainButton not found in CreditsMenu UXML");
            }
        }
    
        private void ShowMainMenu()
        {
            HideAllMenus();
            mainMenuDocument.gameObject.SetActive(true);
            currentMenu = mainMenuDocument.rootVisualElement;
        }
    
        private void ShowTutorialMenu()
        {
            HideAllMenus();
            tutorialDocument.gameObject.SetActive(true);
            currentMenu = tutorialDocument.rootVisualElement;
            currentSlideIndex = 0;
            UpdateTutorialSlide();
        }
    
        private void ShowOptionsMenu()
        {
            HideAllMenus();
            optionsDocument.gameObject.SetActive(true);
            currentMenu = optionsDocument.rootVisualElement;
        }
    
        private void ShowCreditsMenu()
        {
            HideAllMenus();
            creditsDocument.gameObject.SetActive(true);
            currentMenu = creditsDocument.rootVisualElement;
        }
    
        private void HideAllMenus()
        {
            mainMenuDocument.gameObject.SetActive(false);
            tutorialDocument.gameObject.SetActive(false);
            optionsDocument.gameObject.SetActive(false);
            creditsDocument.gameObject.SetActive(false);
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
            var root = tutorialDocument.rootVisualElement;
        
            if (tutorialSlides.Count > 0 && currentSlideIndex < tutorialSlides.Count)
            {
                var slide = tutorialSlides[currentSlideIndex];
            
                root.Q<Label>("SlideTitle").text = slide.title;
                root.Q<Label>("SlideDescription").text = slide.description;
                root.Q<Label>("SlideCounter").text = $"Slide {currentSlideIndex + 1} of {tutorialSlides.Count}";
            
                // Update slide image
                var slideImage = root.Q<VisualElement>("SlideImage");
                if (slide.slideImage != null)
                {
                    slideImage.style.backgroundImage = new StyleBackground(slide.slideImage);
                }
            
                // Update navigation buttons
                root.Q<Button>("PreviousButton").SetEnabled(currentSlideIndex > 0);
                root.Q<Button>("NextButton").SetEnabled(currentSlideIndex < tutorialSlides.Count - 1);
            
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
