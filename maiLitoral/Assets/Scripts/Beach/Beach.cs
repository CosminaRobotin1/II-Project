using UnityEngine;
using System.Collections.Generic;
using System;
using System.Linq;

public class Beach : MonoBehaviour {

    /* Attributes */

    [SerializeField] private int beachId;
    private Dictionary<string, List<(List<string> descriptions, string type, bool? boolValue, int? intValue, string stringValue)>> beachProperties = new Dictionary<string, List<(List<string> descriptions, string type, bool? boolValue, int? intValue, string stringValue)>>(); // Attribute for beaches parameters list (based on date)
    private Dictionary<string, List<bool>> propertiesModified = new Dictionary<string, List<bool>>(); // Attribute for checking if a property was modified (based on date)
    private Dictionary<string, float> rank = new Dictionary<string, float>(); // Attribute for beach rank (based on date)

    /* Custom methods */
    public void AddOrUpdateProperty(string date, List<string> descriptions, string type, string value) { // Adding a beach property
        if (!beachProperties.ContainsKey(date)) {
            beachProperties[date] = new List<(List<string> descriptions, string type, bool? boolValue, int? intValue, string stringValue)>();
            propertiesModified[date] = new List<bool>();
        }
        bool? boolValue = null; int? intValue = null; string stringValue = null;
        switch (type.ToLower()) {
            case "bool":
            if (bool.TryParse(value, out bool parsedBool)) {
                boolValue = parsedBool;
            } else {
                Debug.LogError("Check property type!");
                return;
            }
            break;
            case "int":
            if (int.TryParse(value, out int parsedInt)) {
                intValue = parsedInt;
            } else {
                Debug.LogError("Check property type!");
                return;
            }
            break;
            case "string":
                stringValue = value;
            break;
            default:
                Debug.LogError("Invalid property type!");
            return;
        }
        PropertyData property = DatabaseManager.Instance.GetPropertyDataByName(descriptions[0]);
        if (property == null) {
            property = DatabaseManager.Instance.CreateProperty(descriptions, type);
        }
        if(property == null) {
            Debug.LogError("Invalid property!");
            return;
        }
        DatabaseManager.Instance.AddOrUpdateProperty(property, date, beachId, value);
        DatabaseManager.Instance.CalculateRank(date, beachId);
        float updatedRank = DatabaseManager.Instance.GetRankForBeachDay(beachId, date);
        LoadRankFromDatabase(date, updatedRank);
        int existingIndex = beachProperties[date].FindIndex(p => p.descriptions[0] == descriptions[0]);
        if (existingIndex >= 0) {
            beachProperties[date][existingIndex] = (descriptions, type, boolValue, intValue, stringValue);
            propertiesModified[date][existingIndex] = true;
        } else {
            beachProperties[date].Add((descriptions, type, boolValue, intValue, stringValue));
            propertiesModified[date].Add(false); 
        }

    }
    public void ModifyProperty(string date, List<string> newDescriptions, string newType, string newValue, int index) { // Modifying a beach property
        if (beachProperties.TryGetValue(date, out var properties)) {
            if (index < 0 || index >= properties.Count) {
                Debug.LogError("Beach properties invalid index!");
                return;
            }
        } else {
            Debug.LogError("Beach properties date is invalid!");
            return;
        }
        bool? boolValue = null; int? intValue = null; string stringValue = null;
        switch (newType) {
            case "bool":
            if (bool.TryParse(newValue, out bool parsedBool)) {
                boolValue = parsedBool;
            } else {
                Debug.LogError("Check property type!");
                return;
            }
            break;
            case "int":
            if (int.TryParse(newValue, out int parsedInt)) {
                intValue = parsedInt;
            } else {
                Debug.LogError("Check property type!");
                return;
            }
            break;
            case "string":
            stringValue = newValue;
            break;
        default:
            Debug.LogError("Invalid property type!");
            return;
        }
        PropertyData property = DatabaseManager.Instance.GetPropertyDataByIndex(index + 1);
        DatabaseManager.Instance.ModifyProperty(property, newDescriptions, newType, newValue);
        if(property == null) {
            Debug.LogError("Invalid property!");
            return;
        }
        beachProperties[date][index] = (newDescriptions, newType, boolValue, intValue, stringValue);
        propertiesModified[date][index] = true;
    }
    // public void CopyBeachProperties(Dictionary<string, List<(string description, bool status)>> source, Dictionary<string, List<bool>> sourceModified) { // Copying a set of properties from another beach
    //     if(source == null) {
    //         Debug.LogError("Copied beach properties source is null!");
    //         return;
    //     }
    //     beachProperties.Clear();
    //     propertiesModified.Clear();
    //     foreach (var property in source) {
    //         beachProperties[property.Key] = new List<(string description, bool status)>(property.Value);
    //     }
    //     foreach (var property in sourceModified) {
    //         propertiesModified[property.Key] = new List<bool>(property.Value);
    //     }
    //     foreach (var property in beachProperties) {
    //         foreach (var item in property.Value) {
    //             //DatabaseManager.Instance.AddPropertyForBeachDay(beachId, property.Key, item.description, item.status);
    //         }
    //     }
    // }
    public void DeletePropertyForBeach(string date, int beachIndex, int propertyIndex) { // Deleting a beach property
        PropertyData property = DatabaseManager.Instance.GetPropertyDataByIndex(propertyIndex + 1);
        if(property == null) {
            Debug.LogError("Invalid property!");
            return;
        }
        DatabaseManager.Instance.RemovePropertyForBeach(property, beachIndex);
        beachProperties[date].RemoveAt(propertyIndex);
        propertiesModified[date].RemoveAt(propertyIndex);
    }
    public void AddRank(string date, float rank) { // Adding beach rank
        this.rank[date] = Mathf.Clamp(rank, 0f, 4f);
        DatabaseManager.Instance.AddRank(beachId, date, rank);
    }
    public void LoadPropertiesFromDatabase(string date, List<string> descriptions, string type, string value) { // Loads a property from the database without marking it as modified
        if (!beachProperties.ContainsKey(date)) {
            beachProperties[date] = new List<(List<string> descriptions, string type, bool? boolValue, int? intValue, string stringValue)>();
            propertiesModified[date] = new List<bool>();
        }
        bool? boolValue = null; int? intValue = null; string stringValue = null;
        switch (type) {
            case "bool":
            if (bool.TryParse(value, out bool parsedBool)) {
                boolValue = parsedBool;
            } else {
                if(value != "") {
                    Debug.LogError("Check property type!");
                } 
            }
            break;
            case "int":
            if (int.TryParse(value, out int parsedInt)) {
                intValue = parsedInt;
            } else {
                Debug.LogError("Check property type!");
                return;
            }
            break;
            case "string":
            stringValue = value;
            break;
            default:
                Debug.LogError("Invalid property type!");
            return;
        }
        beachProperties[date].Add((descriptions, type, boolValue, intValue, stringValue));
        propertiesModified[date].Add(boolValue != null ? true : false);
    }
    public void LoadRankFromDatabase(string date, float rank) { // Loads a rank from the database
        this.rank[date] = rank;
    }
    public void SetBeachId(int id) { // Sets the beach id
        beachId = id;
    }

    /* Getters */

    public Dictionary<string, List<(List<string> descriptions, string type, bool? boolValue, int? intValue, string stringValue)>> GetBeachProperties() { // Getter for beach properties
        return beachProperties.ToDictionary(
            kvp => kvp.Key,
            kvp => new List<(List<string> descriptions, string type, bool? boolValue, int? intValue, string stringValue)>(kvp.Value)
        );
    }
    public Dictionary<string, List<bool>> GetPropertiesModified() { // Getter for properties modified
        return propertiesModified.ToDictionary(
            kvp => kvp.Key,
            kvp => new List<bool>(kvp.Value)
        );
    }
    public Dictionary<string, float> GetRank() { // Getter for beach rank
        return new Dictionary<string, float>(rank);
    }
}