using UnityEngine;
using System;
using System.Collections.Generic;
using DialogueGraph.Shared;
using UnityEditor;

[Serializable]
public class SpeakerData
{
    [Delayed]
    public string Name;

    [Delayed]
    public string Key;

    public Texture2D Sprite;

    public AudioClip AudioClip;

    // ---- HUMEURS ----
    // TO EDIT
    [Serializable]
    public class HumeurClass
    {
        [HideInInspector]public string Label; // TO EDIT automatic update 
        public HUMEUR Humeur;
        public Texture2D Text2D;
    }

    public List<HumeurClass> Humeurs  = new  List<HumeurClass>(); 

    // TO EDIT check null return (T model)
    public Texture2D GetTextByHumeur(HUMEUR humeur)
    {
        foreach (HumeurClass h in Humeurs)
        {
            if(h.Humeur == humeur)
            {
                if(!h.Text2D) 
                    Debug.LogWarning(humeur + " Asset is null for " + this.Name + ".");
                
                return h.Text2D;
            }
        }
        // didn't found humeur
        Debug.LogWarning(humeur + " Doesn't exist for " + this.Name + ".");
        return null;
    }

    // ---- EDITOR ----
    public void UpdateDebugInfo()
    {
        // key
        Key = "SPK_" + Name[0];
        for (int i = 0; i < Name.Length; i++)
        {
            if (Name[i] == ' ')
                Key += Name[i+1];
        }

        // Humeur debug
        foreach (HumeurClass h in Humeurs)
        {
            h.Label = h.Humeur.ToString();
        }
    }
}


