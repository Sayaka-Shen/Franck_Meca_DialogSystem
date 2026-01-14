using UnityEngine;
using System;
using System.Collections.Generic;
using DialogueGraph.Shared;

[Serializable]
public class SpeakerData
{
    public string Name;

    public string Key;

    [Serializable]
    public class HumeurClass
    {
        public string Label; // TO EDIT automatic update 
        public HUMEUR Humeur;
        public Texture2D Text2D;
    }

    public List<HumeurClass> HumeurText  = new  List<HumeurClass>(); 

    // TO EDIT check null return (T model)
    public Texture2D GetTextByHumeur(HUMEUR humeur)
    {
        foreach (HumeurClass h in HumeurText)
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


    
}


