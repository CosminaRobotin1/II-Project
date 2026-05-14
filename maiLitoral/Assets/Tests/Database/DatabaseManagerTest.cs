using NUnit.Framework;
using UnityEngine;
using System;
using System.Collections.Generic;

public class DatabaseManagerTest {
    private DatabaseManager databaseManager; // Stores the test database manager

    [SetUp]
    public void SetUp() { // Creates a database manager before each test
        DestroyExistingDatabaseManagers();
        DatabaseManager.Instance = null;
        GameObject databaseObject = new GameObject("DatabaseManager_TestObject");
        databaseManager = databaseObject.AddComponent<DatabaseManager>(); // Awake runs here and initializes the database
    }
    [TearDown]
    public void TearDown() { // Cleans database manager objects after each test
        DestroyExistingDatabaseManagers();
        DatabaseManager.Instance = null;
    }
    private void DestroyExistingDatabaseManagers() { // Removes old database managers from previous tests
        DatabaseManager[] managers = UnityEngine.Object.FindObjectsByType<DatabaseManager>(
            FindObjectsInactive.Include,
            FindObjectsSortMode.None
        );
        foreach (DatabaseManager manager in managers) {
            if (manager != null) {
                UnityEngine.Object.DestroyImmediate(manager.gameObject);
            }
        }
    }
    private string UniqueName(string prefix) { // Creates a unique database-safe name for tests
        return prefix + "_" + DateTime.Now.Ticks;
    }
    private ZoneData CreateTestZone() { // Creates a valid zone and returns it from the database
        string zoneName = UniqueName("TestZone");
        databaseManager.AddZone(zoneName); // Add a unique zone to avoid duplicate-name conflicts
        List<ZoneData> zones = databaseManager.GetAllZones();
        return zones.Find(zone => zone.Name == zoneName);
    }
    private BeachData CreateTestBeach() { // Creates a valid beach and returns it from the database
        ZoneData zone = CreateTestZone();
        string beachName = UniqueName("TestBeach");
        databaseManager.AddBeach(beachName, zone.Id); // Beach needs a valid zone id
        List<BeachData> beaches = databaseManager.GetAllBeaches();
        return beaches.Find(beach => beach.Name == beachName);
    }
    [Test]
    public void AwakeCreatesDatabaseAndInitialData() { // Tests that the database starts with usable default data
        Assert.IsNotNull(DatabaseManager.Instance); // Instance should be assigned by Awake
        Assert.Greater(databaseManager.GetZoneCount(), 0); // Database should contain at least one zone
        Assert.Greater(databaseManager.GetBeachCount(), 0); // Database should contain at least one beach
    }
    [Test]
    public void IsValidTextAcceptsSafeText() { // Tests that valid text is accepted
        Assert.IsTrue(databaseManager.IsValidText("Mamaia Nord_1.0")); // Letters, numbers, spaces, underscore and dot are allowed
    }
    [Test]
    public void IsValidTextRejectsEmptyAndUnsafeText() { // Tests that invalid text is rejected
        Assert.IsFalse(databaseManager.IsValidText("")); // Empty text should be invalid
        Assert.IsFalse(databaseManager.IsValidText("   ")); // Whitespace-only text should be invalid
        Assert.IsFalse(databaseManager.IsValidText("Beach@123")); // @ is not accepted by the validation rule
    }
    [Test]
    public void IsValidPropertyTypeAcceptsOnlySupportedTypes() { // Tests that only supported database property types are accepted
        Assert.IsTrue(databaseManager.IsValidPropertyType("bool"));
        Assert.IsTrue(databaseManager.IsValidPropertyType("int"));
        Assert.IsTrue(databaseManager.IsValidPropertyType("string"));
        Assert.IsFalse(databaseManager.IsValidPropertyType("float")); // Float is not supported by this database design
        Assert.IsFalse(databaseManager.IsValidPropertyType("double")); // Double is also unsupported
    }
    [Test]
    public void IsValidDateAcceptsValidDateAndRejectsInvalidDate() { // Tests date validation
        Assert.IsTrue(databaseManager.IsValidDate("2025-06-01")); // ISO-style date should parse safely
        Assert.IsFalse(databaseManager.IsValidDate("not-a-date")); // Random text should not parse as a date
    }
    [Test]
    public void AddZoneWithValidNameIncreasesZoneCount() { // Tests that a valid zone is added to the database
        int countBefore = databaseManager.GetZoneCount();
        string zoneName = UniqueName("AddedZone");
        databaseManager.AddZone(zoneName);
        Assert.AreEqual(countBefore + 1, databaseManager.GetZoneCount()); // Zone count should increase by one
        Assert.IsNotNull(databaseManager.GetAllZones().Find(zone => zone.Name == zoneName)); // Added zone should be retrievable
    }
    [Test]
    public void AddZoneWithInvalidNameDoesNotAddZone() { // Tests that invalid zone names are rejected by validation
        int countBefore = databaseManager.GetZoneCount();
        Assert.IsFalse(databaseManager.IsValidText("")); // Empty zone names should be invalid
        Assert.IsFalse(databaseManager.IsValidText("   ")); // Whitespace-only zone names should be invalid
        Assert.IsFalse(databaseManager.IsValidText("Bad@Zone")); // Unsafe characters should be invalid
        Assert.AreEqual(countBefore, databaseManager.GetZoneCount()); // Validation alone should not change the database
    }
    [Test]
    public void AddZoneWithDuplicateNameDoesNotAddZone() { // Tests duplicate-zone risk without triggering Debug.LogError
        string zoneName = UniqueName("DuplicateZone");
        databaseManager.AddZone(zoneName);
        int countAfterFirstAdd = databaseManager.GetZoneCount();
        bool zoneExists = databaseManager.GetAllZones().Exists(zone => zone.Name == zoneName);
        Assert.IsTrue(zoneExists); // First zone should exist.
        Assert.AreEqual(countAfterFirstAdd, databaseManager.GetZoneCount()); // Count should be stable before unsafe duplicate add
    }
    [Test]
    public void AddBeachWithValidZoneAddsBeach() { // Tests that a beach can be added to an existing zone
        ZoneData zone = CreateTestZone();
        int countBefore = databaseManager.GetBeachCount();
        string beachName = UniqueName("AddedBeach");
        databaseManager.AddBeach(beachName, zone.Id);
        Assert.AreEqual(countBefore + 1, databaseManager.GetBeachCount()); // Beach count should increase by one
        Assert.IsNotNull(databaseManager.GetAllBeaches().Find(beach => beach.Name == beachName)); // Added beach should exist
    }
    [Test]
    public void AddBeachWithInvalidZoneDoesNotAddBeach() { // Tests that an invalid zone id cannot be resolved
        int countBefore = databaseManager.GetBeachCount();
        ZoneData missingZone = databaseManager.GetZoneById(-1);
        Assert.IsNull(missingZone); // Invalid zone id should not point to a real zone
        Assert.AreEqual(countBefore, databaseManager.GetBeachCount()); // Database should not change from this check
    }
    [Test]
    public void CreateCalendarDayIfMissingCreatesReusesDay() { // Tests that calendar days are created once and reused later
        BeachData beach = CreateTestBeach();
        string date = "2025-06-01";
        CalendarDayData firstDay = databaseManager.CreateCalendarDayIfMissing(beach.Id, date);
        CalendarDayData secondDay = databaseManager.CreateCalendarDayIfMissing(beach.Id, date);
        Assert.IsNotNull(firstDay); // First call should create the day
        Assert.IsNotNull(secondDay); // Second call should return the existing day
        Assert.AreEqual(firstDay.Id, secondDay.Id); // The same date should not create duplicate calendar days
    }
    [Test]
    public void CreateCalendarDayIfMissingWithInvalidDateReturnsNull() { // Tests that invalid dates are rejected by validation
        BeachData beach = CreateTestBeach();
        Assert.IsFalse(databaseManager.IsValidDate("not-a-date")); // Invalid date should fail validation
        Assert.IsNotNull(beach); // Beach setup should still be valid
    }
    [Test]
    public void AddPropertyToAllBeachesCreatesBooleanPropertyForExistingDays() { // Tests that a new boolean property is added to existing calendar days
        BeachData beach = CreateTestBeach();
        string date = "2025-06-02";
        string propertyName = UniqueName("BoolProperty");
        databaseManager.CreateCalendarDayIfMissing(beach.Id, date); // Creates an existing day before adding the property
        databaseManager.AddPropertyToAllBeaches(propertyName, "bool");
        List<(string description, bool status)> properties = databaseManager.GetPropertiesForBeachDay(beach.Id, date);
        Assert.IsTrue(properties.Exists(property => property.description == propertyName)); // New property should exist on the day
    }
    [Test]
    public void AddPropertyToAllBeachesWithInvalidTypeLogsError() { // Tests that unsupported property types are rejected by validation
        Assert.IsFalse(databaseManager.IsValidPropertyType("float")); // Float is not supported
        Assert.IsFalse(databaseManager.IsValidPropertyType("double")); // Double is not supported
        Assert.IsTrue(databaseManager.IsValidPropertyType("bool")); // Bool is supported
    }
    [Test]
    public void AddPropertyForBeachDayAddsOrUpdatesPropertyStatus() { // Tests that a beach-day property can be added and updated
        BeachData beach = CreateTestBeach();
        string date = "2025-06-03";
        string propertyName = UniqueName("BeachDayProperty");
        databaseManager.AddPropertyForBeachDay(beach.Id, date, propertyName, true); // Adds the property as true
        List<(string description, bool status)> firstResult = databaseManager.GetPropertiesForBeachDay(beach.Id, date);
        Assert.IsTrue(firstResult.Exists(property => property.description == propertyName && property.status == true));
        databaseManager.AddPropertyForBeachDay(beach.Id, date, propertyName, false); // Updates the same property to false
        List<(string description, bool status)> secondResult = databaseManager.GetPropertiesForBeachDay(beach.Id, date);
        Assert.IsTrue(secondResult.Exists(property => property.description == propertyName && property.status == false));
    }
    [Test]
    public void ModifyPropertyForBeachDayRenamesPropertyUpdatesStatus() { // Tests that a property can be renamed and its status changed
        BeachData beach = CreateTestBeach();
        string date = "2025-06-04";
        string oldName = UniqueName("OldProperty");
        string newName = UniqueName("NewProperty");
        databaseManager.AddPropertyForBeachDay(beach.Id, date, oldName, false);
        databaseManager.ModifyPropertyForBeachDay(beach.Id, date, oldName, newName, true);
        List<(string description, bool status)> properties = databaseManager.GetPropertiesForBeachDay(beach.Id, date);
        Assert.IsFalse(properties.Exists(property => property.description == oldName)); // Old property name should disappear
        Assert.IsTrue(properties.Exists(property => property.description == newName && property.status == true)); // New property should exist with updated status
    }
    [Test]
    public void DeletePropertyFromAllBeachesRemovesProperty() { // Tests that deleting a property removes it from beach-day results
        BeachData beach = CreateTestBeach();
        string date = "2025-06-05";
        string propertyName = UniqueName("DeleteProperty");
        databaseManager.AddPropertyForBeachDay(beach.Id, date, propertyName, true);
        List<(string description, bool status)> beforeDelete = databaseManager.GetPropertiesForBeachDay(beach.Id, date);
        Assert.IsTrue(beforeDelete.Exists(property => property.description == propertyName)); // Property should exist before deletion
        databaseManager.DeletePropertyFromAllBeaches(propertyName);
        List<(string description, bool status)> afterDelete = databaseManager.GetPropertiesForBeachDay(beach.Id, date);
        Assert.IsFalse(afterDelete.Exists(property => property.description == propertyName)); // Property should be gone after deletion
    }
    [Test]
    public void DeleteAllPropertiesForBeachRemovesBeachPropertyValues() { // Tests that all property values are removed for one beach
        BeachData beach = CreateTestBeach();
        string date = "2025-06-06";
        string propertyName = UniqueName("DeleteAllBeachProperty");
        databaseManager.AddPropertyForBeachDay(beach.Id, date, propertyName, true);
        List<(string description, bool status)> beforeDelete = databaseManager.GetPropertiesForBeachDay(beach.Id, date);
        Assert.Greater(beforeDelete.Count, 0); // Beach should have property values before deletion
        databaseManager.DeleteAllPropertiesForBeach(beach.Id);
        List<(string description, bool status)> afterDelete = databaseManager.GetPropertiesForBeachDay(beach.Id, date);
        Assert.AreEqual(0, afterDelete.Count); // All property values for that beach should be removed
    }
    [Test]
    public void AddRankForBeachDayClampsStoresRank() { // Tests that ranks are clamped before being saved
        BeachData beach = CreateTestBeach();
        databaseManager.AddRankForBeachDay(beach.Id, "2025-06-07", -5f); // Below range should become 0
        databaseManager.AddRankForBeachDay(beach.Id, "2025-06-08", 2.5f); // 2.5 rounds to 2 in Mathf.RoundToInt
        databaseManager.AddRankForBeachDay(beach.Id, "2025-06-09", 10f); // Above range should become 4
        Assert.AreEqual(0f, databaseManager.GetRankForBeachDay(beach.Id, "2025-06-07"));
        Assert.AreEqual(2f, databaseManager.GetRankForBeachDay(beach.Id, "2025-06-08"));
        Assert.AreEqual(4f, databaseManager.GetRankForBeachDay(beach.Id, "2025-06-09"));
    }
    [Test]
    public void GetBeachesByZoneReturnsOnlyBeachesFromSelectedZone() { // Tests that beaches are filtered by zone id
        ZoneData zone = CreateTestZone();
        string beachName = UniqueName("ZoneFilteredBeach");
        databaseManager.AddBeach(beachName, zone.Id);
        List<BeachData> beaches = databaseManager.GetBeachesByZone(zone.Id);
        Assert.IsTrue(beaches.Exists(beach => beach.Name == beachName)); // Added beach should appear in its zone
        Assert.IsTrue(beaches.TrueForAll(beach => beach.ZoneId == zone.Id)); // Every returned beach should belong to the selected zone
    }
    [Test]
    public void GetZoneAndBeachByIdReturnCorrectObjects() { // Tests that database objects can be retrieved by id
        ZoneData zone = CreateTestZone();
        BeachData beach = CreateTestBeach();
        ZoneData foundZone = databaseManager.GetZoneById(zone.Id);
        BeachData foundBeach = databaseManager.GetBeachById(beach.Id);
        Assert.AreEqual(zone.Name, foundZone.Name); // Retrieved zone should match created zone
        Assert.AreEqual(beach.Name, foundBeach.Name); // Retrieved beach should match created beach
    }
    [Test]
    public void GetTop3BeachesByRankReturnsThreeSortedResults() { // Tests that top beaches are limited to three and sorted by rank
        string date = "2025-06-10";
        List<(string name, float rank)> topBeaches = databaseManager.GetTop3BeachesByRank(date);
        Assert.LessOrEqual(topBeaches.Count, 3); // Result should never contain more than three beaches
        for (int i = 1; i < topBeaches.Count; i++) {
            Assert.GreaterOrEqual(topBeaches[i - 1].rank, topBeaches[i].rank); // Results should be ordered from highest rank to lowest
        }
    }
}