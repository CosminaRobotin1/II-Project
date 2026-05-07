using NUnit.Framework;
using UnityEngine;
using System.Collections.Generic;

public class BeachTest{
    private Beach CreateBeach(){
        GameObject obj = new GameObject();
        return obj.AddComponent<Beach>();
    }

    [Test]
    public void AddProperty_ShouldAddPropertyAndModifiedFlagForDate(){
        Beach beach = CreateBeach();

        beach.AddProperty("2025-06-01", "Clean water", true);
        beach.AddProperty("2025-06-01", "Has lifeguard", true);

        var properties = beach.GetBeachProperties();
        var modified = beach.GetPropertiesModified();

        Assert.IsTrue(properties.ContainsKey("2025-06-01"));
        Assert.IsTrue(modified.ContainsKey("2025-06-01"));

        Assert.AreEqual(2, properties["2025-06-01"].Count);
        Assert.AreEqual(2, modified["2025-06-01"].Count);

        Assert.AreEqual("Clean water", properties["2025-06-01"][0].description);
        Assert.IsTrue(properties["2025-06-01"][0].status);

        Assert.AreEqual("Has lifeguard", properties["2025-06-01"][1].description);
        Assert.IsTrue(properties["2025-06-01"][1].status);
    }

    [Test]
    public void AddProperty_ShouldKeepPropertiesAndModifiedFlagsAligned(){
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
    public void ModifyProperty_ShouldUpdateOnlySelectedProperty(){
        Beach beach = CreateBeach();

        beach.AddProperty("2025-06-01", "Dirty beach", false);
        beach.AddProperty("2025-06-01", "No lifeguard", false);

        beach.ModifyProperty("2025-06-01", "Clean beach", true, 0);

        var properties = beach.GetBeachProperties();

        Assert.AreEqual("Clean beach", properties["2025-06-01"][0].description);
        Assert.IsTrue(properties["2025-06-01"][0].status);

        Assert.AreEqual("No lifeguard", properties["2025-06-01"][1].description);
        Assert.IsFalse(properties["2025-06-01"][1].status);
    }

    [Test]
    public void ModifyProperty_WithUnavailableDate_ShouldThrowException(){
        Beach beach = CreateBeach();

        Assert.Throws<KeyNotFoundException>(() =>
        {
            beach.ModifyProperty("2099-01-01", "Invalid update", true, 0);
        });
    }

    [Test]
    public void ModifyProperty_WithInvalidIndex_ShouldThrowException(){
        Beach beach = CreateBeach();

        beach.AddProperty("2025-06-01", "Clean water", true);

        Assert.Throws<System.ArgumentOutOfRangeException>(() =>
        {
            beach.ModifyProperty("2025-06-01", "Invalid update", false, 99);
        });
    }

    [Test]
    public void DeleteProperty_ShouldRemovePropertyAndMatchingModifiedFlag(){
        Beach beach = CreateBeach();

        beach.AddProperty("2025-06-01", "Property A", true);
        beach.AddProperty("2025-06-01", "Property B", false);
        beach.AddProperty("2025-06-01", "Property C", true);

        beach.DeleteProperty("2025-06-01", 1);

        var properties = beach.GetBeachProperties();
        var modified = beach.GetPropertiesModified();

        Assert.AreEqual(2, properties["2025-06-01"].Count);
        Assert.AreEqual(2, modified["2025-06-01"].Count);

        Assert.AreEqual("Property A", properties["2025-06-01"][0].description);
        Assert.AreEqual("Property C", properties["2025-06-01"][1].description);
    }

    [Test]
    public void DeleteProperty_WithInvalidIndex_ShouldThrowException(){
        Beach beach = CreateBeach();

        beach.AddProperty("2025-06-01", "Clean water", true);

        Assert.Throws<System.ArgumentOutOfRangeException>(() =>
        {
            beach.DeleteProperty("2025-06-01", -1);
        });
    }

    [Test]
    public void DeleteProperty_WithUnavailableDate_ShouldThrowException(){
        Beach beach = CreateBeach();

        Assert.Throws<KeyNotFoundException>(() =>
        {
            beach.DeleteProperty("2099-01-01", 0);
        });
    }

    [Test]
    public void DeleteAllProperties_ShouldClearPropertiesAndModifiedFlags(){
        Beach beach = CreateBeach();

        beach.AddProperty("2025-06-01", "Clean water", true);
        beach.AddProperty("2025-06-02", "Crowded", false);

        beach.DeleteAllProperties();

        Assert.AreEqual(0, beach.GetBeachProperties().Count);
        Assert.AreEqual(0, beach.GetPropertiesModified().Count);
    }

    [Test]
    public void AddRank_ShouldClampRankBetweenZeroAndFour(){
        Beach beach = CreateBeach();

        beach.AddRank("low", -5f);
        beach.AddRank("normal", 2.5f);
        beach.AddRank("high", 10f);

        var ranks = beach.GetRank();

        Assert.AreEqual(0f, ranks["low"]);
        Assert.AreEqual(2.5f, ranks["normal"]);
        Assert.AreEqual(4f, ranks["high"]);
    }

    [Test]
    public void AddRank_ShouldOverwriteExistingRankForSameDate(){
        Beach beach = CreateBeach();

        beach.AddRank("2025-06-01", 1f);
        beach.AddRank("2025-06-01", 3.5f);

        var ranks = beach.GetRank();

        Assert.AreEqual(3.5f, ranks["2025-06-01"]);
    }

    [Test]
    public void CopyBeachProperties_ShouldCreateIndependentCopy(){
        Beach beach = CreateBeach();

        var sourceProperties = new Dictionary<string, List<(string description, bool status)>>()
        {
            {
                "2025-06-01",
                new List<(string description, bool status)>()
                {
                    ("Clean water", true),
                    ("Has lifeguard", true)
                }
            }
        };

        var sourceModified = new Dictionary<string, List<bool>>()
        {
            {
                "2025-06-01",
                new List<bool>()
                {
                    false,
                    true
                }
            }
        };

        beach.CopyBeachProperties(sourceProperties, sourceModified);

        sourceProperties["2025-06-01"][0] = ("Changed outside", false);
        sourceModified["2025-06-01"][0] = true;

        var copiedProperties = beach.GetBeachProperties();
        var copiedModified = beach.GetPropertiesModified();

        Assert.AreEqual("Clean water", copiedProperties["2025-06-01"][0].description);
        Assert.IsTrue(copiedProperties["2025-06-01"][0].status);
        Assert.IsFalse(copiedModified["2025-06-01"][0]);
    }

    [Test]
    public void CopyBeachProperties_WithNullSourceProperties_ShouldThrowException(){
        Beach beach = CreateBeach();

        var sourceModified = new Dictionary<string, List<bool>>();

        Assert.Throws<System.NullReferenceException>(() =>
        {
            beach.CopyBeachProperties(null, sourceModified);
        });
    }

    [Test]
    public void CopyBeachProperties_WithNullSourceModified_ShouldThrowException(){
        Beach beach = CreateBeach();

        var sourceProperties = new Dictionary<string, List<(string description, bool status)>>();

        Assert.Throws<System.NullReferenceException>(() =>
        {
            beach.CopyBeachProperties(sourceProperties, null);
        });
    }

    [Test]
    public void GetBeachProperties_ShouldExposeInternalDictionaryReference(){
        Beach beach = CreateBeach();

        beach.AddProperty("2025-06-01", "Clean water", true);

        var exposedProperties = beach.GetBeachProperties();
        exposedProperties.Clear();

        Assert.AreEqual(
            0,
            beach.GetBeachProperties().Count,
            "BUG: Getter exposes internal dictionary, allowing external code to erase beach data."
        );
    }
}