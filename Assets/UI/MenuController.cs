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
        
            root.Q<Button>("PlayButton").clicked += () => {
                Debug.Log("Starting Game...");
                // Add game start logic here
            };
        
            root.Q<Button>("TutorialButton").clicked += ShowTutorialMenu;
            root.Q<Button>("OptionsButton").clicked += ShowOptionsMenu;
            root.Q<Button>("CreditsButton").clicked += ShowCreditsMenu;
            root.Q<Button>("QuitButton").clicked += QuitGame;
        }
    
        private void SetupTutorialMenu()
        {
            var root = tutorialDocument.rootVisualElement;
        
            root.Q<Button>("PreviousButton").clicked += PreviousSlide;
            root.Q<Button>("NextButton").clicked += NextSlide;
            root.Q<Button>("BackToMainButton").clicked += ShowMainMenu;
        
            // Initialize tutorial content
            UpdateTutorialSlide();
        }
    
        private void SetupOptionsMenu()
        {
            var root = optionsDocument.rootVisualElement;
        
            // Setup volume sliders
            var masterVolumeSlider = root.Q<Slider>("MasterVolumeSlider");
            var sfxVolumeSlider = root.Q<Slider>("SFXVolumeSlider");
        
            masterVolumeSlider.RegisterValueChangedCallback(evt => {
                root.Q<Label>("MasterVolumeValue").text = $"{evt.newValue:F0}%";
                AudioListener.volume = evt.newValue / 100f;
            });
        
            sfxVolumeSlider.RegisterValueChangedCallback(evt => {
                root.Q<Label>("SFXVolumeValue").text = $"{evt.newValue:F0}%";
                // Set SFX volume here
            });
        
            // Setup toggles
            var fullscreenToggle = root.Q<Toggle>("FullscreenToggle");
            var vsyncToggle = root.Q<Toggle>("VSyncToggle");
        
            fullscreenToggle.RegisterValueChangedCallback(evt => {
                Screen.fullScreen = evt.newValue;
            });
        
            vsyncToggle.RegisterValueChangedCallback(evt => {
                QualitySettings.vSyncCount = evt.newValue ? 1 : 0;
            });
        
            root.Q<Button>("BackToMainButton").clicked += ShowMainMenu;
        }
    
        private void SetupCreditsMenu()
        {
            var root = creditsDocument.rootVisualElement;
            root.Q<Button>("BackToMainButton").clicked += ShowMainMenu;
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