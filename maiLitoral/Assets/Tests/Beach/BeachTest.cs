using NUnit.Framework;
using UnityEngine;
using System;
using System.Collections.Generic;

public class BeachTest {
    private Beach CreateBeach() { // Creates a new Beach object for testing.
        GameObject obj = new GameObject();
        return obj.AddComponent<Beach>();
    }
    [Test]
    public void PropertyAndModifiedFlagForDate() { // Tests that adding properties stores both the property and its modified flag.
        Beach beach = CreateBeach();
        beach.AddProperty("2025-06-01", "Clean water", true); // Add first property.
        beach.AddProperty("2025-06-01", "Has lifeguard", true); // Add second property on the same date.
        var properties = beach.GetBeachProperties();
        var modified = beach.GetPropertiesModified();
        Assert.IsTrue(properties.ContainsKey("2025-06-01")); // The date should exist in the properties dictionary.
        Assert.IsTrue(modified.ContainsKey("2025-06-01")); // The same date should exist in the modified flags dictionary.
        Assert.AreEqual(2, properties["2025-06-01"].Count); // Two properties should be stored.
        Assert.AreEqual(2, modified["2025-06-01"].Count); // Two matching modified flags should be stored.
        Assert.AreEqual("Clean water", properties["2025-06-01"][0].description);
        Assert.IsTrue(properties["2025-06-01"][0].status);
        Assert.AreEqual("Has lifeguard", properties["2025-06-01"][1].description);
        Assert.IsTrue(properties["2025-06-01"][1].status);
    }
    [Test]
    public void PropertyAndModifiedFlagsAligned() { // Tests that properties and modified flags stay the same length.
        Beach beach = CreateBeach();
        beach.AddProperty("2025-06-01", "Clean water", true);
        beach.AddProperty("2025-06-01", "Crowded beach", false);
        beach.AddProperty("2025-06-01", "Has toilets", true);
        var properties = beach.GetBeachProperties();
        var modified = beach.GetPropertiesModified();
        Assert.AreEqual(
            properties["2025-06-01"].Count,
            modified["2025-06-01"].Count,
            "Every property must have a matching modified flag."
        );
    }
    [Test]
    public void ModifyPropertyUpdateOnlySelected() { // Tests that modifying a property only changes the selected entry.
        Beach beach = CreateBeach();
        beach.AddProperty("2025-06-01", "Dirty beach", false);
        beach.AddProperty("2025-06-01", "No lifeguard", false);
        beach.ModifyProperty("2025-06-01", "Clean beach", true, 0); // Modify only the first property.
        var properties = beach.GetBeachProperties();
        Assert.AreEqual("Clean beach", properties["2025-06-01"][0].description); // First property should change.
        Assert.IsTrue(properties["2025-06-01"][0].status);
        Assert.AreEqual("No lifeguard", properties["2025-06-01"][1].description); // Second property should stay unchanged.
        Assert.IsFalse(properties["2025-06-01"][1].status);
    }
    [Test]
    public void ModifyPropertyWithUnavailableDate() { // Tests that modifying a missing date causes an exception.
        Beach beach = CreateBeach();

        Assert.Throws<KeyNotFoundException>(() => {
            beach.ModifyProperty("2099-01-01", "Invalid update", true, 0); // This date does not exist.
        });
    }
    [Test]
    public void ModifyPropertyWithInvalidIndex() { // Tests that modifying with an invalid index causes an exception.
        Beach beach = CreateBeach();
        beach.AddProperty("2025-06-01", "Clean water", true);
        Assert.Throws<ArgumentOutOfRangeException>(() => {
            beach.ModifyProperty("2025-06-01", "Invalid update", false, 99); // Index 99 does not exist.
        });
    }
    [Test]
    public void DeletePropertyRemovePropertyAndFlag() { // Tests that deleting a property also removes its modified flag.
        Beach beach = CreateBeach();
        beach.AddProperty("2025-06-01", "Property A", true);
        beach.AddProperty("2025-06-01", "Property B", false);
        beach.AddProperty("2025-06-01", "Property C", true);
        beach.DeleteProperty("2025-06-01", 1); // Delete the middle property.
        var properties = beach.GetBeachProperties();
        var modified = beach.GetPropertiesModified();
        Assert.AreEqual(2, properties["2025-06-01"].Count); // Property list should shrink.
        Assert.AreEqual(2, modified["2025-06-01"].Count); // Modified list should shrink with it.
        Assert.AreEqual("Property A", properties["2025-06-01"][0].description);
        Assert.AreEqual("Property C", properties["2025-06-01"][1].description); // Property B should be gone.
    }
    [Test]
    public void DeletePropertyInvalidIndex() { // Tests that deleting with an invalid index causes an exception.
        Beach beach = CreateBeach();
        beach.AddProperty("2025-06-01", "Clean water", true);
        Assert.Throws<ArgumentOutOfRangeException>(() => {
            beach.DeleteProperty("2025-06-01", -1); // Negative indexes are invalid.
        });
    }
    [Test]
    public void DeletePropertyUnavailableDate() { // Tests that deleting from a missing date causes an exception.
        Beach beach = CreateBeach();
        Assert.Throws<KeyNotFoundException>(() => {
            beach.DeleteProperty("2099-01-01", 0); // This date does not exist.
        });
    }
    [Test]
    public void DeleteAllPropertiesTest() { // Tests that all stored properties and flags are cleared.
        Beach beach = CreateBeach();
        beach.AddProperty("2025-06-01", "Clean water", true);
        beach.AddProperty("2025-06-02", "Crowded", false);
        beach.DeleteAllProperties();
        Assert.AreEqual(0, beach.GetBeachProperties().Count); // Properties dictionary should be empty.
        Assert.AreEqual(0, beach.GetPropertiesModified().Count); // Modified flags dictionary should also be empty.
    }
    [Test]
    public void AddRankBetweenZeroAndFour() { // Tests that beach rank is limited between 0 and 4.
        Beach beach = CreateBeach();
        beach.AddRank("low", -5f); // Below valid range.
        beach.AddRank("normal", 2.5f); // Inside valid range.
        beach.AddRank("high", 10f); // Above valid range.
        var ranks = beach.GetRank();
        Assert.AreEqual(0f, ranks["low"]);
        Assert.AreEqual(2.5f, ranks["normal"]);
        Assert.AreEqual(4f, ranks["high"]);
    }
    [Test]
    public void AddRankOverwriteExistingRankForSameDate() { // Tests that adding a rank for the same date replaces the old value.
        Beach beach = CreateBeach();
        beach.AddRank("2025-06-01", 1f);
        beach.AddRank("2025-06-01", 3.5f); // Same key should overwrite old value.
        var ranks = beach.GetRank();
        Assert.AreEqual(3.5f, ranks["2025-06-01"]);
    }
    [Test]
    public void CopyBeachPropertiesCreateIndependentCopy() { // Tests that copied property data is independent from the original source.
        Beach beach = CreateBeach();
        var sourceProperties = new Dictionary<string, List<(string description, bool status)>>() {
            {
                "2025-06-01",
                new List<(string description, bool status)>() {
                    ("Clean water", true),
                    ("Has lifeguard", true)
                }
            }
        };
        var sourceModified = new Dictionary<string, List<bool>>() {
            {
                "2025-06-01",
                new List<bool>() {
                    false,
                    true
                }
            }
        };
        beach.CopyBeachProperties(sourceProperties, sourceModified);
        sourceProperties["2025-06-01"][0] = ("Changed outside", false); // Change original source after copy.
        sourceModified["2025-06-01"][0] = true;
        var copiedProperties = beach.GetBeachProperties();
        var copiedModified = beach.GetPropertiesModified();
        Assert.AreEqual("Clean water", copiedProperties["2025-06-01"][0].description); // Copied value should stay unchanged.
        Assert.IsTrue(copiedProperties["2025-06-01"][0].status);
        Assert.IsFalse(copiedModified["2025-06-01"][0]);
    }
    [Test]
    public void CopyBeachPropertiesNullSourceProperties() { // Tests that copying from a null properties source causes an exception.
        Beach beach = CreateBeach();
        var sourceModified = new Dictionary<string, List<bool>>();
        Assert.Throws<NullReferenceException>(() => {
            beach.CopyBeachProperties(null, sourceModified); // Null source is unsafe.
        });
    }
    [Test]
    public void CopyBeachPropertiesNullSourceModified() { // Tests that copying from a null modified-flags source causes an exception.
        Beach beach = CreateBeach();
        var sourceProperties = new Dictionary<string, List<(string description, bool status)>>();
        Assert.Throws<NullReferenceException>(() => {
            beach.CopyBeachProperties(sourceProperties, null); // Null modified list is unsafe.
        });
    }
    [Test]
    public void GetBeachPropertiesTest() { // Tests that the getter exposes the internal dictionary reference.
        Beach beach = CreateBeach();
        beach.AddProperty("2025-06-01", "Clean water", true);
        var exposedProperties = beach.GetBeachProperties();
        exposedProperties.Clear(); // Simulates another script accidentally clearing internal data.
        Assert.AreEqual(
            0,
            beach.GetBeachProperties().Count,
            "BUG: Getter exposes internal dictionary, allowing external code to erase beach data."
        );
    }
}