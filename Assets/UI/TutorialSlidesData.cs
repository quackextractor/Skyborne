using System;
using System.Collections.Generic;
using UnityEngine;

namespace UI
{
    [CreateAssetMenu(menuName = "UI/Tutorial Slides Data", fileName = "NewTutorialSlidesData")]
    public class TutorialSlidesData : ScriptableObject
    {
        [Serializable]
        public class Slide
        {
            public string title = "Tutorial Slide";
            [TextArea(3,5)] 
            public string description = "This is a tutorial slide description.";
            public Texture2D slideImage;
        }

        [Tooltip("The ordered list of tutorial slides.")]
        public List<Slide> slides = new List<Slide>();
    }
}