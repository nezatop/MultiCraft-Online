using UnityEngine;
using System;
using Lean.Localization;

public class LanguageManager : MonoBehaviour
{
    public void Russainlanguage() 
    {
         LeanLocalization.SetCurrentLanguageAll("Russian");
    }
    
    public void EnglishLanguage() 
    {
         LeanLocalization.SetCurrentLanguageAll("English");
    }
}
