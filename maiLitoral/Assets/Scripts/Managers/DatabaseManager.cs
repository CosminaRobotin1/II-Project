using UnityEngine;
using SQLite4Unity3d;
using System.IO;
using System;
using System.Linq;
using System.Collections.Generic;
using Unity.Android.Gradle;
using System.Globalization;
using Unity.VisualScripting;

public class DatabaseManager : MonoBehaviour {

    //* Attributes *//

    public static DatabaseManager Instance; // Instance of database manager
    private SQLiteConnection db; // Database reference from SQLite
    private List<string> zones = new List<string>() {"Mamaia Nord", "Mamaia", "Constanta", "Eforie Nord", "Vama Veche"};
    private List<string> beaches = new List<string>() {"Zanzibar Beach", "Oneiro Beach", "Kazeboo Beach", "H2O Beach", "Princess Beach", "Ipanera Beach", "Modern Beach", "Zoom Beach", "Trei Papuci", "Belona Beach", "Debarcader Beach", "Azur Beach", "Amphora Beach", "Stuf Beach", "Expirat Beach"};
    private List<(string name, string type)> propertiesEn = new() { ("Sand type", "string"), ("Clean", "bool"), ("Safe", "bool"), ("Water temp", "int"), ("Sunbeds", "bool"), ("Umbrellas", "bool"), ("Showers", "bool"), ("Toilets", "bool"), ("Parking", "bool"), ("Algae", "bool"), ("Jellyfish", "bool"), ("Sea Shells", "bool"), ("Wind", "string"), ("Weather", "string"), ("Crowdedness", "bool"), ("Lifeguards", "bool") };
    private List<(string name, string type)> propertiesRo = new() { ("Tip nisip", "string"), ("Curată", "bool"), ("Sigură", "bool"), ("Temp. apă", "int"), ("Șezlonguri", "bool"), ("Umbrele", "bool"), ("Dușuri", "bool"), ("Toalete", "bool"), ("Loc parcare", "string"), ("Alge", "bool"), ("Meduze", "bool"), ("Scoici", "bool"), ("Vânt", "string"), ("Vreme", "string"), ("Aglomerație", "bool"), ("Salvamar", "bool") };
    private List<int> propertiesValues = new() { 0, -1, -1, 1, -1, -1, -1, -1, -1, 1, 1, 1, 1, 1, 1, -1 }; // Values for each property (positive, negative)
    private string imageBaseUrl = "https://raw.githubusercontent.com/teodormorosanu/II-Project/refs/heads/main/Pictures/Beaches/";
    public struct PropertyContainer { // Storing the available types for properties (used for parsing different text into different types)
        public bool? BoolValue;
        public int? IntValue;
        public string StringValue;
        public bool Success;
    }

    //* Main Methods *//

    private void Awake() {
        DataBaseInit(); // Database initialization
    }
    private void OnDestroy() { // On switching scene the app ensures the database is no longer connected (can't connect from two sources to the same db)
        CloseDatabaseConnection();
    }
    private void OnApplicationQuit() { // On quiting the app it ensures the database is no longer connected (can't connect from two sources to the same db)
        CloseDatabaseConnection();
    }

    /* Custom Methods */
    
    private void DataBaseInit() { // Database initialization
        if (Instance == null) { // Ensures that only one DatabaseManager instance exists in the application
            Instance = this;
            DontDestroyOnLoad(gameObject);
        } else {
            Destroy(gameObject);
            return;
        }
        string directoryPath = Path.Combine(Application.persistentDataPath, "Databases"); // Setting a working path even for mobile
        string path = Path.Combine(directoryPath, "mailitoral.db");
        if (!Directory.Exists(directoryPath)) {
            Directory.CreateDirectory(directoryPath);
        }
        db = new SQLiteConnection(path);
        CreateTables();
        if (!(db.Table<ZoneData>().Count() > 0)) {
            InsertStartData();
        }
        EnsureCurrentDayExists(); // Ensures that current date is in database
    }
    private void CloseDatabaseConnection() {
        if (db != null) {
            db.Close();
            db.Dispose();
            db = null;
            Debug.Log("Database connection closed safely.");
        }
    }
    private void CreateTables() { // Creates all database tables if they do not already exist
        db.CreateTable<ZoneData>();
        db.CreateTable<BeachData>();
        db.CreateTable<CalendarDayData>();
        db.CreateTable<PropertyData>();
        db.CreateTable<BeachDayPropertyData>();
    }
    private void InsertStartData() { // Inserts example data in the database only when the database is empty
        foreach(string zoneName in zones) {
            db.Insert(new ZoneData { Name = zoneName });
        }
        int index = 1;
        foreach(string beachName in beaches) {
            string formattedName = beachName.Replace(" ", "_").ToLower();
            string finalUrl = imageBaseUrl + formattedName + ".png";
            db.Insert(new BeachData {
                Name = beachName,
                ZoneId = ((index - 1) / 3) + 1,
                ImageUrl = finalUrl
            });
            index++;
        }
        for (int i = 0; i < propertiesEn.Count; i++) {
            db.Insert(new PropertyData { 
            NameEn = propertiesEn[i].name, 
            NameRo = propertiesRo[i].name, 
            Type = propertiesEn[i].type,
            Value = propertiesValues[i]
            });
        }   
        foreach (BeachData beach in db.Table<BeachData>()) { // Create calendar day
            CreateCalendarDay(beach.Id, DateTime.Now.ToString("dd-MM-yyyy"));
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
            CultureInfo.InvariantCulture,
            DateTimeStyles.None,
            out _
        );
    }
    private void ResetDatabase() { // * ONLY FOR DEBUGGING *
    CloseDatabaseConnection();
    string directoryPath = Path.Combine(Application.persistentDataPath, "Databases");
    string path = Path.Combine(directoryPath, "mailitoral.db");
        if (File.Exists(path)) {
            File.Delete(path);
            Debug.Log("Database deleted.");
        }
    }
    public int AddZone(string zoneName) { // Adds a new zone in the database
        if (!IsValidText(zoneName)) {
            Debug.LogWarning("Invalid zone name!");
            return -1;
        }
        if (db.Table<ZoneData>().Any(z => z.Name.ToLower() == zoneName.ToLower())) {
            Debug.LogWarning("Zone already exists!");
            return 0;
        }
        return db.Insert(new ZoneData { Name = zoneName });
    }
    public int DeleteZone(string zoneName) { // Deleting a zone and all of it's connections
    ZoneData zone = db.Table<ZoneData>().FirstOrDefault(z => z.Name.ToLower() == zoneName.ToLower());
    if (zone == null) {
        Debug.LogWarning("Zone does not exist!");
        return 0;
    }
    List<BeachData> beachesInZone = db.Table<BeachData>().Where(b => b.ZoneId == zone.Id).ToList();
    foreach (BeachData beach in beachesInZone) {
        List<CalendarDayData> calendarDays = db.Table<CalendarDayData>().Where(d => d.BeachId == beach.Id).ToList();
        foreach (CalendarDayData day in calendarDays) {
            List<BeachDayPropertyData> dayProperties = db.Table<BeachDayPropertyData>().Where(p => p.CalendarDayId == day.Id).ToList();
            foreach (BeachDayPropertyData prop in dayProperties) {
                db.Delete(prop);
            }
            db.Delete(day);
        }
        db.Delete(beach);
    }
    db.Delete(zone);
    return db.Delete(zone);
}
    public int AddBeach(string beachName, int zoneId) { // Adds a new beach in the database and connects it to an existing zone
        if (!IsValidText(beachName)) {
            Debug.LogWarning("Invalid beach name!");
            return -2;
        }
        ZoneData zone = db.Find<ZoneData>(zoneId);
        if (zone == null) {
            Debug.LogWarning("Zone does not exist!");
            return -1;
        }
        if (db.Table<BeachData>().Any(b => b.Name.ToLower() == beachName.ToLower())) {
            Debug.LogWarning("Beach already exists!");
            return 0;
        }
        string formattedName = beachName.Replace(" ", "_").ToLower();
        string finalUrl = imageBaseUrl + formattedName + ".png";
        return db.Insert(new BeachData {
            Name = beachName,
            ZoneId = zoneId,
            ImageUrl = finalUrl
        });
    }
    public int ModifyBeach(string beachName, string newBeachName) {
        if (!IsValidText(newBeachName)) {
            Debug.LogWarning("Invalid beach name!");
            return -2;
        }
        BeachData beach = db.Table<BeachData>().FirstOrDefault(b => b.Name.ToLower() == beachName.ToLower());
        if (beach == null) {
            Debug.LogWarning("Beach does not exist!");
            return -1; 
        }
        if (db.Table<BeachData>().Any(b => b.Name.ToLower() == newBeachName.ToLower())) {
            Debug.LogWarning("The new beach name already exists!");
            return 0; 
        }
        beach.Name = newBeachName;
        return db.Update(beach);
    }
    public int DeleteBeach(int beachId) { // Deleting a beach and all of it's connections
        BeachData beach = db.Find<BeachData>(beachId);
        if (beach == null) {
            Debug.LogWarning("Beach does not exist!");
            return 0;
        }
        List<CalendarDayData> calendarDays = db.Table<CalendarDayData>().Where(d => d.BeachId == beachId).ToList();
        foreach (CalendarDayData day in calendarDays) {
            List<BeachDayPropertyData> dayProperties = db.Table<BeachDayPropertyData>().Where(p => p.CalendarDayId == day.Id).ToList();
            foreach (BeachDayPropertyData property in dayProperties) {
                db.Delete(property);
            }
            db.Delete(day);
        }
        return db.Delete(beach);
    }
    
    public CalendarDayData CreateCalendarDay(int beachId, string date) { // Creates a calendar day for a beach if that day does not already exist
        if (!IsValidDate(date)) {
            Debug.LogWarning("Invalid date.");
            return null;
        }
        BeachData beach = db.Find<BeachData>(beachId);
        if (beach == null) {
            Debug.LogWarning("Beach does not exist.");
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
            db.Insert(new BeachDayPropertyData {
                CalendarDayId = day.Id,
                PropertyId = property.Id,
                BoolValue = null,
                IntValue = 0,
                StringValue = ""
            });
        }
        return day;
    }
    private void EnsureCurrentDayExists() { // Ensures that current date is in database
        string today = DateTime.Now.ToString("dd-MM-yyyy");
        foreach (BeachData beach in db.Table<BeachData>()) {
            CreateCalendarDay(beach.Id, today);
        }
    }
    public PropertyData CreateProperty(List<string> descriptions, string propertyType) { // Creates a property if it not exists
        if (!IsValidText(descriptions[0]) || !IsValidText(descriptions[1])) {
            Debug.LogWarning("Invalid property name!");
            return null;
        }
        if (!IsValidPropertyType(propertyType)) {
            Debug.LogWarning("Invalid property type!");
            return null;
        }
        if (db.Table<PropertyData>().Any(p => p.NameEn == descriptions[0])) {
            Debug.LogWarning("Property already exists!");
            return null;
        }
        db.Insert(new PropertyData {
            NameEn = descriptions[0],
            NameRo = descriptions[1],
            Type = propertyType
        });
        return db.Table<PropertyData>().First(p => p.NameEn == descriptions[0]);
    }
    public void AddOrUpdateProperty(PropertyData property, string date, int beachId, string value) { // Adds or updates a property value for a specific beach and date
        if(property == null) {
            Debug.LogWarning("Invalid property!");
            return;
        }
        PropertyContainer data = ParseProperty(property.Type, value); // Getting the parsed property type and value
        CalendarDayData day = CreateCalendarDay(beachId, date);
        if (day == null) {
            Debug.LogWarning("Invalid day!");
            return;
        }
        BeachDayPropertyData record = db.Table<BeachDayPropertyData>().FirstOrDefault(v => v.CalendarDayId == day.Id && v.PropertyId == property.Id);
        if (record == null) {
            db.Insert(new BeachDayPropertyData {
                CalendarDayId = day.Id,
                PropertyId = property.Id,
                BoolValue = data.BoolValue,
                IntValue = data.IntValue,
                StringValue = data.StringValue
            });
        } else {
            record.BoolValue = data.BoolValue;
            record.IntValue = data.IntValue;
            record.StringValue = data.StringValue;
            db.Update(record);
        }
    }
    public void AddPropertyToAll(PropertyData property, string value) { // Adds a new property definition and gives this property to all existing calendar days
        if(property == null) {
            Debug.LogWarning("Invalid property!");
            return;
        }
        PropertyContainer data = ParseProperty(property.Type, value); // Getting the parsed property type and value
        foreach (CalendarDayData day in db.Table<CalendarDayData>()) {
            BeachDayPropertyData newProperty = db.Table<BeachDayPropertyData>().FirstOrDefault(d => d.PropertyId == property.Id && d.CalendarDayId == day.Id);
            if(newProperty != null) {
                continue;
            }
            db.Insert(new BeachDayPropertyData {
                CalendarDayId = day.Id,
                PropertyId = property.Id,
                BoolValue = property.Type == "bool" ? data.BoolValue : (bool?)null,
                IntValue = property.Type == "int" ? data.IntValue : (int?)null,
                StringValue = property.Type == "string" ? data.StringValue : null
            });
        }
    }
    public void ModifyProperty(PropertyData property, List<string> newDescriptions, string newType, string newValue) { // Modifies a property and updates its value for every beach and date
        if(property == null) {
            Debug.LogWarning("Invalid property!");
            return;
        }
        if (!IsValidText(newDescriptions[0]) || !IsValidText(newDescriptions[1])) {
            Debug.LogWarning("Invalid property name.");
            return;
        }
        if (!IsValidPropertyType(newType)) {
            Debug.LogWarning("Invalid property type!");
            return;
        }
        PropertyContainer data = ParseProperty(newType, newValue); // Getting the parsed property type and value
        PropertyData oldProperty = db.Table<PropertyData>().FirstOrDefault(p => p.Id == property.Id);
        if (oldProperty == null) {
            Debug.LogWarning("Property does not exist!");
            return;
        }
        oldProperty.NameEn = newDescriptions[0];
        oldProperty.NameRo = newDescriptions[1];
        oldProperty.Type = newType;
        db.Update(oldProperty);
        foreach (CalendarDayData day in db.Table<CalendarDayData>()) {
            BeachDayPropertyData oldPropertyValue = db.Table<BeachDayPropertyData>().First(d => d.PropertyId == property.Id && d.CalendarDayId == day.Id);
            oldPropertyValue.BoolValue = data.BoolValue;
            oldPropertyValue.IntValue = data.IntValue;
            oldPropertyValue.StringValue = data.StringValue;
            db.Update(oldPropertyValue);
        }
    }
    public void DeleteProperty(PropertyData property) { // Deletes a property from the database and removes its values from all beaches
        if(property == null) {
            Debug.LogWarning("Invalid property!");
            return;
        }
        foreach (BeachDayPropertyData value in db.Table<BeachDayPropertyData>().Where(v => v.PropertyId == property.Id)) {
            db.Delete(value);
        }
        db.Delete(property);
    }
    public void RemovePropertyForBeach(PropertyData property, int beachId) { // Deletes a property value associated with a specific beach
        foreach (CalendarDayData day in db.Table<CalendarDayData>().Where(d => d.BeachId == beachId)) {
            foreach (BeachDayPropertyData value in db.Table<BeachDayPropertyData>().Where(v => v.CalendarDayId == day.Id && v.PropertyId == property.Id)) {
                db.Delete(value);
            }
        }
    }
    public void AddRank(int beachId, string date, float rank) { // Adds or updates the rank value for a beach on a specific date
        rank = Mathf.Clamp(rank, 0f, 4f);
        CalendarDayData day = CreateCalendarDay(beachId, date);
        if (day == null) {
            return;
        }
        day.DailyRank = rank;
        db.Update(day);
    }
    public void CalculateRank(string date, int beachId) { // Calculating the beach rank based on the active properties
    CalendarDayData day = db.Table<CalendarDayData>().FirstOrDefault(d => d.BeachId == beachId && d.Date == date);
        if (day == null) {
            return;
        }
        float calculatedRank = 2.0f; // Starting rank
        float multiplier = 0.25f; // Property multiplier

    List<BeachDayPropertyData> dailyProperties = db.Table<BeachDayPropertyData>().Where(v => v.CalendarDayId == day.Id).ToList();
        foreach (BeachDayPropertyData record in dailyProperties)  {
            PropertyData propertyDef = db.Find<PropertyData>(record.PropertyId);
            if (propertyDef == null) continue;
            switch (propertyDef.Type.ToLower())  { // Changing the score based on property type
                case "bool":
                    if (record.BoolValue == true) {
                        calculatedRank += (propertyDef.Value * multiplier);
                    }
                    else if (record.BoolValue == false) {
                        calculatedRank -= (propertyDef.Value * multiplier); 
                    }
                    break;
                case "int":
                    if (record.IntValue.HasValue) {
                        float idealTemp = 27f; // Definim o valoare considerată perfectă
                        float diff = Mathf.Abs(idealTemp - record.IntValue.Value);
                        calculatedRank += (diff * 0.05f * propertyDef.Value);
                    }
                    break;
                case "string":
                    if (!string.IsNullOrEmpty(record.StringValue)) {
                        string lowerVal = record.StringValue.ToLower();
                        if (lowerVal.Contains("Buna") || lowerVal.Contains("Good")) {
                            calculatedRank -= multiplier;
                        } else if (lowerVal.Contains("Rea") || lowerVal.Contains("Bad")) {
                            calculatedRank += multiplier;
                        }
                    }
                    break;
            }
        }
        AddRank(beachId, date, calculatedRank);
    }
    public static PropertyContainer ParseProperty(string type, string value) { // Parsing a type and a value for that type (used for modular taken input)
        PropertyContainer container = new PropertyContainer { Success = false };
        switch (type.ToLower()) {
            case "bool":
                if (bool.TryParse(value, out bool parsedBool)) {
                    container.BoolValue = parsedBool;
                    container.Success = true;
                }
                break;
            case "int":
                if (int.TryParse(value, out int parsedInt)) {
                    container.IntValue = parsedInt;
                    container.Success = true;
                }
                break;
            case "string":
                container.StringValue = value;
                container.Success = true;
                break;
            default:
                Debug.LogWarning("Invalid property type!");
            return container;
        }
        if (!container.Success) {
            Debug.LogWarning("Check property type!");
        }
        return container;
    }

    //* Getters *//

    public List<ZoneData> GetAllZones() { // Returns all zones from the database
        return db.Table<ZoneData>().ToList();
    }
    public ZoneData GetZoneByName(string name) { // Returns the zone data based on name
        return db.Table<ZoneData>().FirstOrDefault(z => z.Name == name);
    }
    public List<BeachData> GetAllBeaches() { // Returns all beaches from the database
        return db.Table<BeachData>().ToList();
    }
    public List<BeachData> GetBeachesByZone(int zoneId) { // Returns all beaches that belong to a specific zone
        return db.Table<BeachData>().Where(b => b.ZoneId == zoneId).ToList();
    }
    public BeachData GetBeachDataByName(string name) { // Returns the beach data based on name
        return db.Table<BeachData>().FirstOrDefault(b => b.Name == name);
    }
    public PropertyData GetPropertyDataByIndex(int index) // Returns the property data based on index
    {
        return db.Table<PropertyData>().FirstOrDefault(p => p.Id == index);
    }
    public PropertyData GetPropertyDataByName(string name) { // Returns the property data based on name
        return db.Table<PropertyData>().FirstOrDefault(p => p.NameEn == name);
    }
    public List<PropertyData> GetAllProperties() { // Returns all properties from the database
        return db.Table<PropertyData>().ToList();
    }
    public List<(List<string> descriptions, string type, bool? boolValue, int? intValue, string stringValue)> GetPropertiesForBeachDay(int beachId, string date) { // Returns all properties for a specific beach and date
        List<(List<string> descriptions, string type, bool? boolValue, int? intValue, string stringValue)> result = new List<(List<string> descriptions, string type, bool? boolValue, int? intValue, string stringValue)>();
        CalendarDayData day = db.Table<CalendarDayData>().FirstOrDefault(d => d.BeachId == beachId && d.Date == date);
        if (day == null) {
            return result;
        }
        foreach (BeachDayPropertyData value in db.Table<BeachDayPropertyData>().Where(v => v.CalendarDayId == day.Id)) {
            PropertyData property = db.Find<PropertyData>(value.PropertyId);
            result.Add((new List<string> {
                property.NameEn,
                property.NameRo
            },
            property.Type,
            value.BoolValue,
            value.IntValue,
            value.StringValue
            ));
        }
        return result;
    }
    public float GetRankForBeachDay(int beachId, string date) { // Returns the rank value for a specific beach and date
        CalendarDayData day = db.Table<CalendarDayData>().FirstOrDefault(d => d.BeachId == beachId && d.Date == date);
        if (day == null) {
            return 0f;
        }
        return day.DailyRank;
    }
    public List<(string name, float rank)> GetTop3BeachesByRank(string date) { // Returns the top three beaches ordered by rank for a specific date
        List<(string name, float rank)> result = new List<(string name, float rank)>();
        foreach (BeachData beach in db.Table<BeachData>()) {
            float beachRank = GetRankForBeachDay(beach.Id, date);
            result.Add((beach.Name, beachRank));
        }
        return result.OrderBy(b => b.rank).Take(3).ToList();
    }
    public List<string> GetBeachesByActiveBoolProperty(PropertyData property, string date) { // Returns all beach names that have a specific boolean property active on a specific date
        List<string> result = new List<string>(); // Stores the matching beach names
        foreach (BeachData beach in db.Table<BeachData>()) { // Goes through all beaches from the database
            List<(List<string> descriptions, string type, bool? boolValue, int? intValue, string stringValue)> properties = GetPropertiesForBeachDay(beach.Id, date); // Gets the properties for the current beach and date
            foreach (var propertyFromList in properties) { // Goes through every boolean property of the beach
                if (!string.Equals(propertyFromList.descriptions[0], property.NameEn, StringComparison.OrdinalIgnoreCase)) {
                    continue; // Skips properties that are not the requested one
                }
                CalendarDayData calendarDay = db.Table<CalendarDayData>().FirstOrDefault(d => d.Date == date && d.BeachId == beach.Id);
                BeachDayPropertyData propertyData = db.Table<BeachDayPropertyData>().FirstOrDefault(b => b.CalendarDayId == calendarDay.Id && b.BoolValue == true && b.PropertyId == property.Id);
                if (propertyData == null) {
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
            .Select(property => property.NameEn)
            .ToList(); // Returns the newest property names
    }
}