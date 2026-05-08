using UnityEngine;
using System.Collections.Generic;
using System;

public class Beach : MonoBehaviour
{

    /* Attributes */

    [SerializeField] private int beachId = 1;

    private Dictionary<string, List<(string description, bool status)>> beachProperties = new Dictionary<string, List<(string description, bool status)>>(); // Attribute for beaches parameters list (based on date)
    private Dictionary<string, List<bool>> propertiesModified = new Dictionary<string, List<bool>>(); // Attribute for checking if a property was modified (based on date)
    private Dictionary<string, float> rank = new Dictionary<string, float>(); // Attribute for beach rank (based on date)

    /* Custom methods */

    public void AddProperty(string date, string description, bool status) { // Adding a beach property *needs database implementation*
        if (!beachProperties.ContainsKey(date)){
            beachProperties[date] = new List<(string description, bool status)>();
            propertiesModified[date] = new List<bool>();
        }
        beachProperties[date].Add((description, status));
        propertiesModified[date].Add(true); // Needs to be false after database implementation

        DatabaseManager.Instance.AddPropertyForBeachDay(beachId, date, description, status);
    }

    public void ModifyProperty(string date, string newDescription, bool newStatus, int index) { // Modifying a beach property *needs database implementation*
        string oldDescription = beachProperties[date][index].description;

        beachProperties[date][index] = (newDescription, newStatus);
        propertiesModified[date][index] = true;

        DatabaseManager.Instance.ModifyPropertyForBeachDay(beachId, date, oldDescription, newDescription, newStatus);
    }

    public void CopyBeachProperties(Dictionary<string, List<(string description, bool status)>> source, Dictionary<string, List<bool>> sourceModified) { // Copying a set of properties from another beach *needs database implementation*
        beachProperties.Clear();
        propertiesModified.Clear();
        foreach (var property in source)
        {
            beachProperties[property.Key] = new List<(string description, bool status)>(property.Value);
        }
        foreach (var property in sourceModified)
        {
            propertiesModified[property.Key] = new List<bool>(property.Value);
        }

        foreach (var property in beachProperties)
        {
            foreach (var item in property.Value)
            {
                DatabaseManager.Instance.AddPropertyForBeachDay(beachId, property.Key, item.description, item.status);
            }
        }
    }

    public void DeleteProperty(string date, int index) { // Deleting a beach property *needs database implementation*
        string description = beachProperties[date][index].description;

        beachProperties[date].RemoveAt(index);
        propertiesModified[date].RemoveAt(index);

        DatabaseManager.Instance.DeletePropertyFromAllBeaches(description);
    }

    public void DeleteAllProperties() { // Deleting all beach properties *needs database implementation*
        beachProperties.Clear();
        propertiesModified.Clear();

        DatabaseManager.Instance.DeleteAllPropertiesForBeach(beachId);
    }

    public void AddRank(string date, float newRank) { // Adding beach rank *needs database implementation*
        rank[date] = Mathf.Clamp(newRank, 0f, 4f);

        DatabaseManager.Instance.AddRankForBeachDay(beachId, date, newRank);
    }
    // Loads a property from the database without marking it as modified.
    public void LoadPropertyFromDatabase(string date, string description, bool status) {

        if (!beachProperties.ContainsKey(date)) {
            beachProperties[date] = new List<(string description, bool status)>();
            propertiesModified[date] = new List<bool>();
        }

        beachProperties[date].Add((description, status));
        propertiesModified[date].Add(false);
    }
    // Loads a rank from the database.
    public void LoadRankFromDatabase(string date, float beachRank) {
        rank[date] = beachRank;
    }
    // Sets the beach id.
    public void SetBeachId(int id) { beachId = id; }

    /* Getters */
    // Getter for beach properties
    public Dictionary<string, List<(string description, bool status)>> GetBeachProperties() {return beachProperties;}
    // Getter for properties modified
    public Dictionary<string, List<bool>> GetPropertiesModified() {return propertiesModified;}
    // Getter for beach rank
    public Dictionary<string, float> GetRank() {return rank;}
}