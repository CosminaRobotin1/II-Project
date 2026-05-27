using SQLite4Unity3d;

public class PropertyData { // Stores property definitions used by all beaches
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; } // Unique database id for the property
    public string NameRo { get; set; } // Name (romanian) of the property
    public string NameEn { get; set; } // Name (english) of the property
    public float Value { get; set; } // Property value (positive or negative)
    public string Type { get; set; } // Data type of the property: bool, int or string
}