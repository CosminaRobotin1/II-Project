using UnityEngine;
using SQLite4Unity3d;
using System.IO;
using System;
using System.Linq;
using System.Collections.Generic;

public class DatabaseManager : MonoBehaviour
{

    public static DatabaseManager Instance;

    private SQLiteConnection db;

    // Ensures that only one DatabaseManager instance exists in the application.
    private void Awake(){
        if (Instance == null){
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }else{
            Destroy(gameObject);}
    }

    // Opens the database connection, creates the tables and inserts initial data only once.
    private void Start(){
        string directoryPath = "Assets/Databases";
        string path = Path.Combine(directoryPath, "mailitoral.db");

        if (!Directory.Exists(directoryPath)){
            Directory.CreateDirectory(directoryPath);
        }

        db = new SQLiteConnection(path);

        CreateTables();

        if (!IsDatabasePopulated()){InsertStartData();}

        UnityEngine.Debug.Log("Database ready: " + path);
    }

    // Creates all database tables if they do not already exist.
    private void CreateTables(){
        db.CreateTable<ZoneData>();
        db.CreateTable<BeachData>();
        db.CreateTable<CalendarDayData>();
        db.CreateTable<PropertyData>();
        db.CreateTable<BeachDayPropertyData>();
    }

    // Checks if the database already contains initial data.
    private bool IsDatabasePopulated() {return db.Table<ZoneData>().Count() > 0;}

    // Inserts example data in the database only when the database is empty.
    private void InsertStartData(){
        db.Insert(new ZoneData { Name = "Mamaia Nord" });
        db.Insert(new ZoneData { Name = "Constanta" });

        int mamaiaId = db.Table<ZoneData>().First(z => z.Name == "Mamaia Nord").Id;
        int constantaId = db.Table<ZoneData>().First(z => z.Name == "Constanta").Id;

        db.Insert(new BeachData { Name = "Beach One", ZoneId = mamaiaId });
        db.Insert(new BeachData { Name = "Modern Beach", ZoneId = constantaId });

        db.Insert(new PropertyData { Name = "Has Lifeguard", Type = "bool" });
        db.Insert(new PropertyData { Name = "Water Temperature", Type = "int" });
        db.Insert(new PropertyData { Name = "Description", Type = "string" });

        foreach (BeachData beach in db.Table<BeachData>()){
            CreateCalendarDayIfMissing(beach.Id, DateTime.Now.ToString("dd-MM-yyyy"));
        }

        UnityEngine.Debug.Log("Start data inserted.");
    }

    // Checks if a text contains only accepted characters.
    public bool IsValidText(string text)
    {
        if (string.IsNullOrWhiteSpace(text)){return false;}

        return text.All(c =>
            char.IsLetterOrDigit(c) ||
            c == ' ' ||
            c == '-' ||
            c == '_' ||
            c == '.'
        );
    }

    // Checks if a property type is valid for the database.
    public bool IsValidPropertyType(string type){return type == "bool" || type == "int" || type == "string";}

    // Checks if the provided date can be parsed.
    public bool IsValidDate(string date){return DateTime.TryParse(date, out _);}

    // Adds a new zone in the database.
    public void AddZone(string zoneName){
        if (!IsValidText(zoneName)){
            UnityEngine.Debug.LogError("Invalid zone name.");
            return;
        }

        if (db.Table<ZoneData>().Any(z => z.Name == zoneName)){
            UnityEngine.Debug.LogError("Zone already exists.");
            return;
        }

        db.Insert(new ZoneData { Name = zoneName });

        UnityEngine.Debug.Log("Zone added.");
    }

    // Adds a new beach in the database and connects it to an existing zone.
    public void AddBeach(string beachName, int zoneId){
        if (!IsValidText(beachName)){
            UnityEngine.Debug.LogError("Invalid beach name.");
            return;
        }

        ZoneData zone = db.Find<ZoneData>(zoneId);

        if (zone == null){
            UnityEngine.Debug.LogError("Zone does not exist.");
            return;
        }

        if (db.Table<BeachData>().Any(b => b.Name == beachName)){
            UnityEngine.Debug.LogError("Beach already exists.");
            return;
        }

        db.Insert(new BeachData
        {
            Name = beachName,
            ZoneId = zoneId
        });

        UnityEngine.Debug.Log("Beach added.");
    }

    // Creates a calendar day for a beach if that day does not already exist.
    public CalendarDayData CreateCalendarDayIfMissing(int beachId, string date){
        if (!IsValidDate(date)){
            UnityEngine.Debug.LogError("Invalid date.");
            return null;
        }

        BeachData beach = db.Find<BeachData>(beachId);

        if (beach == null){
            UnityEngine.Debug.LogError("Beach does not exist.");
            return null;
        }

        CalendarDayData day = db.Table<CalendarDayData>().FirstOrDefault(d => d.BeachId == beachId && d.Date == date);

        if (day != null){
            return day;
        }

        db.Insert(new CalendarDayData
        {
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

    // Adds a new property definition and gives this property to all existing calendar days.
    public void AddPropertyToAllBeaches(string propertyName, string propertyType){
        if (!IsValidText(propertyName)){
            UnityEngine.Debug.LogError("Invalid property name.");
            return;
        }

        if (!IsValidPropertyType(propertyType)){
            UnityEngine.Debug.LogError("Invalid property type. Use bool, int or string.");
            return;
        }

        if (db.Table<PropertyData>().Any(p => p.Name == propertyName)){
            UnityEngine.Debug.LogError("Property already exists.");
            return;
        }

        db.Insert(new PropertyData
        {
            Name = propertyName,
            Type = propertyType
        });

        PropertyData newProperty = db.Table<PropertyData>().First(p => p.Name == propertyName);

        foreach (CalendarDayData day in db.Table<CalendarDayData>()){
            db.Insert(new BeachDayPropertyData
            {
                CalendarDayId = day.Id,
                PropertyId = newProperty.Id,
                BoolValue = propertyType == "bool" ? false : (bool?)null,
                IntValue = propertyType == "int" ? 0 : (int?)null,
                StringValue = propertyType == "string" ? "" : null
            });
        }

        UnityEngine.Debug.Log("Property added to all beaches.");
    }

    // Adds or updates a boolean property value for a specific beach and date.
    public void AddPropertyForBeachDay(int beachId, string date, string propertyName, bool status){
        if (!IsValidText(propertyName)){
            UnityEngine.Debug.LogError("Invalid property name.");
            return;
        }

        CalendarDayData day = CreateCalendarDayIfMissing(beachId, date);

        if (day == null){return;}

        PropertyData property = db.Table<PropertyData>().FirstOrDefault(p => p.Name == propertyName);

        if (property == null) {
            AddPropertyToAllBeaches(propertyName, "bool");
            property = db.Table<PropertyData>().First(p => p.Name == propertyName);
        }

        BeachDayPropertyData value = db.Table<BeachDayPropertyData>().FirstOrDefault(v => v.CalendarDayId == day.Id && v.PropertyId == property.Id);

        if (value == null){
            db.Insert(new BeachDayPropertyData
            {
                CalendarDayId = day.Id,
                PropertyId = property.Id,
                BoolValue = status
            });
        }else{
            value.BoolValue = status;
            db.Update(value);
        }

        UnityEngine.Debug.Log("Property added/updated for beach day.");
    }

    // Renames a property and updates its boolean value for a specific beach and date.
    public void ModifyPropertyForBeachDay(int beachId, string date, string oldPropertyName, string newPropertyName, bool newStatus){
        if (!IsValidText(oldPropertyName) || !IsValidText(newPropertyName)){
            UnityEngine.Debug.LogError("Invalid property name.");
            return;
        }

        CalendarDayData day = CreateCalendarDayIfMissing(beachId, date);

        if (day == null){return;}

        PropertyData property = db.Table<PropertyData>().FirstOrDefault(p => p.Name == oldPropertyName);

        if (property == null){
            UnityEngine.Debug.LogError("Property does not exist.");
            return;
        }

        property.Name = newPropertyName;
        db.Update(property);

        BeachDayPropertyData value = db.Table<BeachDayPropertyData>().FirstOrDefault(v => v.CalendarDayId == day.Id && v.PropertyId == property.Id);

        if (value != null){
            value.BoolValue = newStatus;
            db.Update(value);
        }

        UnityEngine.Debug.Log("Property modified.");
    }

    // Deletes a property from the database and removes its values from all beaches.
    public void DeletePropertyFromAllBeaches(string propertyName){
        if (!IsValidText(propertyName)){
            UnityEngine.Debug.LogError("Invalid property name.");
            return;
        }

        PropertyData property = db.Table<PropertyData>().FirstOrDefault(p => p.Name == propertyName);

        if (property == null){
            UnityEngine.Debug.LogError("Property does not exist.");
            return;
        }

        foreach (BeachDayPropertyData value in db.Table<BeachDayPropertyData>().Where(v => v.PropertyId == property.Id)){
            db.Delete(value);
        }

        db.Delete(property);

        UnityEngine.Debug.Log("Property deleted from all beaches.");
    }

    // Deletes all property values associated with a specific beach.
    public void DeleteAllPropertiesForBeach(int beachId){
        foreach (CalendarDayData day in db.Table<CalendarDayData>().Where(d => d.BeachId == beachId)){
            foreach (BeachDayPropertyData value in db.Table<BeachDayPropertyData>().Where(v => v.CalendarDayId == day.Id))
{
                db.Delete(value);
            }
        }

        UnityEngine.Debug.Log("All properties deleted for beach.");
    }

    // Adds or updates the rank value for a beach on a specific date.
    public void AddRankForBeachDay(int beachId, string date, float rank){
        rank = Mathf.Clamp(rank, 0f, 4f);

        CalendarDayData day = CreateCalendarDayIfMissing(beachId, date);

        if (day == null){return;}

        PropertyData rankProperty = db.Table<PropertyData>().FirstOrDefault(p => p.Name == "Rank");

        if (rankProperty == null) {
            AddPropertyToAllBeaches("Rank", "int");
            rankProperty = db.Table<PropertyData>().First(p => p.Name == "Rank");
        }

        BeachDayPropertyData value = db.Table<BeachDayPropertyData>().FirstOrDefault(v => v.CalendarDayId == day.Id && v.PropertyId == rankProperty.Id);

        if (value == null){
            db.Insert(new BeachDayPropertyData
            {
                CalendarDayId = day.Id,
                PropertyId = rankProperty.Id,
                IntValue = Mathf.RoundToInt(rank)
            });
        }else{
            value.IntValue = Mathf.RoundToInt(rank);
            db.Update(value);
        }

        UnityEngine.Debug.Log("Rank added/updated.");
    }

    // Returns all zones from the database.
    public List<ZoneData> GetAllZones(){return db.Table<ZoneData>().ToList();}

    // Returns all beaches from the database.
    public List<BeachData> GetAllBeaches(){return db.Table<BeachData>().ToList();}

    // Returns all beaches that belong to a specific zone.
    public List<BeachData> GetBeachesByZone(int zoneId){return db.Table<BeachData>().Where(b => b.ZoneId == zoneId).ToList(); }

    // Returns the number of beaches from the database.
    public int GetBeachCount(){return db.Table<BeachData>().Count();}

    // Returns the number of zones from the database.
    public int GetZoneCount(){return db.Table<ZoneData>().Count();}

    // Returns a zone by its database id.
    public ZoneData GetZoneById(int zoneId){return db.Find<ZoneData>(zoneId);}

    // Returns a beach by its database id.
    public BeachData GetBeachById(int beachId){return db.Find<BeachData>(beachId);}

    // Returns all boolean properties for a specific beach and date.
    public List<(string description, bool status)> GetPropertiesForBeachDay(int beachId, string date){
        List<(string description, bool status)> result = new List<(string description, bool status)>();

        CalendarDayData day = db.Table<CalendarDayData>().FirstOrDefault(d => d.BeachId == beachId && d.Date == date);

        if (day == null){return result;}

        foreach (BeachDayPropertyData value in db.Table<BeachDayPropertyData>().Where(v => v.CalendarDayId == day.Id)) {
            PropertyData property = db.Find<PropertyData>(value.PropertyId);

            if (property != null && property.Type == "bool"){result.Add((property.Name, value.BoolValue ?? false));}
        }

        return result;
    }

    // Returns the rank value for a specific beach and date.
    public float GetRankForBeachDay(int beachId, string date){
        CalendarDayData day = db.Table<CalendarDayData>().FirstOrDefault(d => d.BeachId == beachId && d.Date == date);

        if (day == null){return 0f;}

        PropertyData rankProperty = db.Table<PropertyData>().FirstOrDefault(p => p.Name == "Rank");

        if (rankProperty == null){return 0f;}

        BeachDayPropertyData value = db.Table<BeachDayPropertyData>().FirstOrDefault(v => v.CalendarDayId == day.Id && v.PropertyId == rankProperty.Id);

        if (value == null || value.IntValue == null){return 0f;}

        return value.IntValue.Value;
    }

    // Returns the top three beaches ordered by rank for a specific date.
    public List<(string name, float rank)> GetTop3BeachesByRank(string date){
        List<(string name, float rank)> result = new List<(string name, float rank)>();

        foreach (BeachData beach in db.Table<BeachData>()){
            float beachRank = GetRankForBeachDay(beach.Id, date);
            result.Add((beach.Name, beachRank));
        }

        return result.OrderByDescending(b => b.rank).Take(3).ToList();
    }
}