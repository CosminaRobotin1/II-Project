using UnityEngine;
using SQLite4Unity3d;
using System.IO;
using System;
using System.Linq;
using System.Collections.Generic;

public class DatabaseManager : MonoBehaviour {

    //* Attributes *//

    public static DatabaseManager Instance; // Instance of database manager
    private SQLiteConnection db; // Database reference from SQLite
    private List<string> zones = new List<string>() {"Mamaia Nord", "Mamaia", "Constanta", "Eforie Nord", "Vama Veche"};
    private List<string> beaches = new List<string>() {"Zanzibar Beach", "Oneiro Beach", "Kazeboo Beach", "H2O Beach", "Princess Beach", "Ipanera Beach", "Modern Beach", "Zoom Beach", "Trei Papuci", "Belona Beach", "Debarcader Beach", "Azur Beach", "Amphora Beach", "Stuf Beach", "Expirat Beach"};
    private List<(string Name, string Type)> properties = new() {("Type", "string"), ("Cleanliness", "int"), ("Safeness", "int"), ("Sunbeds", "bool"), ("Umbrellas", "bool"), ("Showers", "bool"), ("Toilets", "bool"), ("Parking", "string"), ("Algae", "bool"), ("Jellyfish", "bool"), ("Sea Shells", "bool"), ("Wind", "string"), ("Weather", "string"), ("Crowdedness", "int"), ("Lifeguards", "bool")};

    //* Main Methods *//

    private void Awake() {
        if (Instance == null) { // Ensures that only one DatabaseManager instance exists in the application
            Instance = this;
            DontDestroyOnLoad(gameObject);
        } else {
            Destroy(gameObject);
        }
        string directoryPath = "Assets/Databases";
        string path = Path.Combine(directoryPath, "mailitoral.db");
        if (!Directory.Exists(directoryPath)) {
            Directory.CreateDirectory(directoryPath);
        }
        db = new SQLiteConnection(path);
        CreateTables();
        if (!IsDatabasePopulated()) {
            InsertStartData();
        }
        EnsureCurrentDayExistsForAll(); // Ensures that current date is in database
    }

    /* Custom Methods */

    private void CreateTables() { // Creates all database tables if they do not already exist
        db.CreateTable<ZoneData>();
        db.CreateTable<BeachData>();
        db.CreateTable<CalendarDayData>();
        db.CreateTable<PropertyData>();
        db.CreateTable<BeachDayPropertyData>();
    }
    private bool IsDatabasePopulated() { // Checks if the database already contains initial data
        return db.Table<ZoneData>().Count() > 0;
    }
    private void InsertStartData() { // Inserts example data in the database only when the database is empty
        foreach(string zoneName in zones) {
            db.Insert(new ZoneData { Name = zoneName });
        }
        int index = 1;
        foreach(string beachName in beaches) {
            db.Insert(new BeachData { Name = beachName, ZoneId = ((index - 1) / 3) + 1 });
            index++;
        }
        foreach((string propertyName, string type) in properties) {
            db.Insert(new PropertyData { Name = propertyName, Type = type });
        }
        foreach (BeachData beach in db.Table<BeachData>()) { // Create calendar day
            CreateCalendarDayIfMissing(beach.Id, DateTime.Now.ToString("dd-MM-yyyy"));
        }
    }
    public bool IsValidText(string text) { // Checks if a text contains only accepted characters
        if (string.IsNullOrWhiteSpace(text)) {
            return false;
        }
        return text.All(c =>
            char.IsLetterOrDigit(c) ||
            c == ' ' ||
            c == '-' ||
            c == '_' ||
            c == '.'
        );
    }
    public bool IsValidPropertyType(string type) { // Checks if a property type is valid for the database
        return type == "bool" || type == "int" || type == "string";
    }
    public bool IsValidDate(string date) { // Checks if the provided date can be parsed
        return DateTime.TryParseExact(
            date,
            "dd-MM-yyyy",
            System.Globalization.CultureInfo.InvariantCulture,
            System.Globalization.DateTimeStyles.None,
            out _
        );
    }
    public void AddZone(string zoneName) { // Adds a new zone in the database
        if (!IsValidText(zoneName)) {
            UnityEngine.Debug.LogError("Invalid zone name.");
            return;
        }
        if (db.Table<ZoneData>().Any(z => z.Name == zoneName)) {
            UnityEngine.Debug.LogError("Zone already exists.");
            return;
        }
        db.Insert(new ZoneData { Name = zoneName });

        //ZoneData newZone = db.Table<ZoneData>().First(z => z.Name == zoneName); // Upload Zone To Firebase
        //FirebaseDatabaseManager.Instance.UploadZone(newZone.Id, newZone.Name);

    }
    public void AddBeach(string beachName, int zoneId) { // Adds a new beach in the database and connects it to an existing zone
        if (!IsValidText(beachName)) {
            UnityEngine.Debug.LogError("Invalid beach name.");
            return;
        }
        ZoneData zone = db.Find<ZoneData>(zoneId);
        if (zone == null) {
            UnityEngine.Debug.LogError("Zone does not exist.");
            return;
        }
        if (db.Table<BeachData>().Any(b => b.Name == beachName)) {
            UnityEngine.Debug.LogError("Beach already exists.");
            return;
        }
        db.Insert(new BeachData {
            Name = beachName,
            ZoneId = zoneId
        });
    }
    public CalendarDayData CreateCalendarDayIfMissing(int beachId, string date) { // Creates a calendar day for a beach if that day does not already exist
        if (!IsValidDate(date)) {
            UnityEngine.Debug.LogError("Invalid date.");
            return null;
        }
        BeachData beach = db.Find<BeachData>(beachId);
        if (beach == null) {
            UnityEngine.Debug.LogError("Beach does not exist.");
            return null;
        }
        CalendarDayData day = db.Table<CalendarDayData>().FirstOrDefault(d => d.BeachId == beachId && d.Date == date);
        if (day != null) {
            return day;
        }
        db.Insert(new CalendarDayData {
            BeachId = beachId,
            Date = date
        });
        day = db.Table<CalendarDayData>().First(d => d.BeachId == beachId && d.Date == date);
        foreach (PropertyData property in db.Table<PropertyData>()){
            db.Insert(new BeachDayPropertyData
            {
                CalendarDayId = day.Id,
                PropertyId = property.Id,
                BoolValue = property.Type == "bool" ? false : (bool?)null,
                IntValue = property.Type == "int" ? 0 : (int?)null,
                StringValue = property.Type == "string" ? "" : null
            });
        }
        return day;
    }
    private void EnsureCurrentDayExistsForAll() { // Ensures that current date is in database
    string today = DateTime.Now.ToString("dd-MM-yyyy");
    foreach (BeachData beach in db.Table<BeachData>()) {
        CreateCalendarDayIfMissing(beach.Id, today);
    }
}
    public void AddPropertyToAllBeaches(string propertyName, string propertyType) { // Adds a new property definition and gives this property to all existing calendar days
        if (!IsValidText(propertyName)) {
            UnityEngine.Debug.LogError("Invalid property name.");
            return;
        }
        if (!IsValidPropertyType(propertyType)) {
            UnityEngine.Debug.LogError("Invalid property type. Use bool, int or string.");
            return;
        }
        if (db.Table<PropertyData>().Any(p => p.Name == propertyName)) {
            UnityEngine.Debug.LogError("Property already exists.");
            return;
        }
        db.Insert(new PropertyData {
            Name = propertyName,
            Type = propertyType
        });
        PropertyData newProperty = db.Table<PropertyData>().First(p => p.Name == propertyName);
        foreach (CalendarDayData day in db.Table<CalendarDayData>()) {
            db.Insert(new BeachDayPropertyData {
                CalendarDayId = day.Id,
                PropertyId = newProperty.Id,
                BoolValue = propertyType == "bool" ? false : (bool?)null,
                IntValue = propertyType == "int" ? 0 : (int?)null,
                StringValue = propertyType == "string" ? "" : null
            });
        }
    }
    public void AddPropertyForBeachDay(int beachId, string date, string propertyName, bool status) { // Adds or updates a boolean property value for a specific beach and date
        if (!IsValidText(propertyName)) {
            UnityEngine.Debug.LogError("Invalid property name.");
            return;
        }
        CalendarDayData day = CreateCalendarDayIfMissing(beachId, date);
        if (day == null) {
            return;
        }
        PropertyData property = db.Table<PropertyData>().FirstOrDefault(p => p.Name == propertyName);
        if (property == null) {
            AddPropertyToAllBeaches(propertyName, "bool");
            property = db.Table<PropertyData>().First(p => p.Name == propertyName);
        }
        BeachDayPropertyData value = db.Table<BeachDayPropertyData>().FirstOrDefault(v => v.CalendarDayId == day.Id && v.PropertyId == property.Id);
        if (value == null) {
            db.Insert(new BeachDayPropertyData {
                CalendarDayId = day.Id,
                PropertyId = property.Id,
                BoolValue = status
            });
        } else {
            value.BoolValue = status;
            db.Update(value);
        }
    }
    public void ModifyPropertyForBeachDay(int beachId, string date, string oldPropertyName, string newPropertyName, bool newStatus) { // Renames a property and updates its boolean value for a specific beach and date
        if (!IsValidText(oldPropertyName) || !IsValidText(newPropertyName)) {
            UnityEngine.Debug.LogError("Invalid property name.");
            return;
        }
        CalendarDayData day = CreateCalendarDayIfMissing(beachId, date);
        if (day == null) {
            return;
        }
        PropertyData property = db.Table<PropertyData>().FirstOrDefault(p => p.Name == oldPropertyName);
        if (property == null) {
            UnityEngine.Debug.LogError("Property does not exist.");
            return;
        }
        property.Name = newPropertyName;
        db.Update(property);
        BeachDayPropertyData value = db.Table<BeachDayPropertyData>().FirstOrDefault(v => v.CalendarDayId == day.Id && v.PropertyId == property.Id);
        if (value != null) {
            value.BoolValue = newStatus;
            db.Update(value);
        }
    }
    public void DeletePropertyFromAllBeaches(string propertyName) { // Deletes a property from the database and removes its values from all beaches
        if (!IsValidText(propertyName)) {
            UnityEngine.Debug.LogError("Invalid property name.");
            return;
        }
        PropertyData property = db.Table<PropertyData>().FirstOrDefault(p => p.Name == propertyName);
        if (property == null) {
            UnityEngine.Debug.LogError("Property does not exist.");
            return;
        }
        foreach (BeachDayPropertyData value in db.Table<BeachDayPropertyData>().Where(v => v.PropertyId == property.Id)) {
            db.Delete(value);
        }
        db.Delete(property);
    }
    public void DeleteAllPropertiesForBeach(int beachId) { // Deletes all property values associated with a specific beach
        foreach (CalendarDayData day in db.Table<CalendarDayData>().Where(d => d.BeachId == beachId)) {
            foreach (BeachDayPropertyData value in db.Table<BeachDayPropertyData>().Where(v => v.CalendarDayId == day.Id)) {
                db.Delete(value);
            }
        }
    }
    public void AddRankForBeachDay(int beachId, string date, float rank) { // Adds or updates the rank value for a beach on a specific date
        rank = Mathf.Clamp(rank, 0f, 4f);
        CalendarDayData day = CreateCalendarDayIfMissing(beachId, date);
        if (day == null) {
            return;
        }
        PropertyData rankProperty = db.Table<PropertyData>().FirstOrDefault(p => p.Name == "Rank");
        if (rankProperty == null) {
            AddPropertyToAllBeaches("Rank", "int");
            rankProperty = db.Table<PropertyData>().First(p => p.Name == "Rank");
        }
        BeachDayPropertyData value = db.Table<BeachDayPropertyData>().FirstOrDefault(v => v.CalendarDayId == day.Id && v.PropertyId == rankProperty.Id);
        if (value == null) {
            db.Insert(new BeachDayPropertyData {
                CalendarDayId = day.Id,
                PropertyId = rankProperty.Id,
                IntValue = Mathf.RoundToInt(rank)
            });
        } else {
            value.IntValue = Mathf.RoundToInt(rank);
            db.Update(value);
        }
    }

    //* Getters *//

    public List<ZoneData> GetAllZones() { // Returns all zones from the database
        return db.Table<ZoneData>().ToList();
    }
    public List<BeachData> GetAllBeaches() { // Returns all beaches from the database
        return db.Table<BeachData>().ToList();
    }
    public List<BeachData> GetBeachesByZone(int zoneId) { // Returns all beaches that belong to a specific zone
        return db.Table<BeachData>().Where(b => b.ZoneId == zoneId).ToList();
    }
    public int GetBeachCount() { // Returns the number of beaches from the database
        return db.Table<BeachData>().Count();
    }
    public int GetZoneCount() { // Returns the number of zones from the database
        return db.Table<ZoneData>().Count();
    }
    public ZoneData GetZoneById(int zoneId) { // Returns a zone by its database id
        return db.Find<ZoneData>(zoneId);
    }
    public BeachData GetBeachById(int beachId) { // Returns a beach by its database id
        return db.Find<BeachData>(beachId);
    }
    public List<(string description, bool status)> GetPropertiesForBeachDay(int beachId, string date) { // Returns all boolean properties for a specific beach and date
        List<(string description, bool status)> result = new List<(string description, bool status)>();
        CalendarDayData day = db.Table<CalendarDayData>().FirstOrDefault(d => d.BeachId == beachId && d.Date == date);
        if (day == null) {
            return result;
        }
        foreach (BeachDayPropertyData value in db.Table<BeachDayPropertyData>().Where(v => v.CalendarDayId == day.Id)) {
            PropertyData property = db.Find<PropertyData>(value.PropertyId);
            if (property != null && property.Type == "bool") {
                result.Add((property.Name, value.BoolValue ?? false));
            }
        }
        return result;
    }
    public float GetRankForBeachDay(int beachId, string date) { // Returns the rank value for a specific beach and date
        CalendarDayData day = db.Table<CalendarDayData>().FirstOrDefault(d => d.BeachId == beachId && d.Date == date);
        if (day == null) {
            return 0f;
        }
        PropertyData rankProperty = db.Table<PropertyData>().FirstOrDefault(p => p.Name == "Rank");
        if (rankProperty == null) {
            return 0f;
        }
        BeachDayPropertyData value = db.Table<BeachDayPropertyData>().FirstOrDefault(v => v.CalendarDayId == day.Id && v.PropertyId == rankProperty.Id);
        if (value == null || value.IntValue == null) {
            return 0f;
        }
        return value.IntValue.Value;
    }
    public List<(string name, float rank)> GetTop3BeachesByRank(string date) { // Returns the top three beaches ordered by rank for a specific date
        List<(string name, float rank)> result = new List<(string name, float rank)>();
        foreach (BeachData beach in db.Table<BeachData>()) {
            float beachRank = GetRankForBeachDay(beach.Id, date);
            result.Add((beach.Name, beachRank));
        }
        return result.OrderByDescending(b => b.rank).Take(3).ToList();
    }
    public List<string> GetBeachesByActiveBoolProperty(string date, string propertyName) { // Returns all beach names that have a specific boolean property active on a specific date
        List<string> result = new List<string>(); // Stores the matching beach names
        foreach (BeachData beach in db.Table<BeachData>()) { // Goes through all beaches from the database
            List<(string description, bool status)> properties = GetPropertiesForBeachDay(beach.Id, date); // Gets the boolean properties for the current beach and date
            foreach ((string description, bool status) property in properties) { // Goes through every boolean property of the beach
                if (!string.Equals(property.description, propertyName, StringComparison.OrdinalIgnoreCase)) {
                    continue; // Skips properties that are not the requested one
                }
                if (!property.status) {
                    continue; // Skips beaches where the requested property is inactive
                }
                result.Add(beach.Name); // Adds the beach because it matches the requested active property
            }
        }
        return result; // Returns all matching beach names
    }
    public List<string> GetLatestBeachNames(int count) { // Returns the latest added beaches based on database id order
        return db.Table<BeachData>()
            .OrderByDescending(beach => beach.Id)
            .Take(count)
            .Select(beach => beach.Name)
            .ToList(); // Returns the newest beach names
    }
    public List<string> GetLatestPropertyNames(int count) { // Returns the latest added properties based on database id order
        return db.Table<PropertyData>()
            .OrderByDescending(property => property.Id)
            .Take(count)
            .Select(property => property.Name)
            .ToList(); // Returns the newest property names
    }
    public List<PropertyData> GetAllProperties() { // Returns all properties from the database
        return db.Table<PropertyData>().ToList();
    }
}