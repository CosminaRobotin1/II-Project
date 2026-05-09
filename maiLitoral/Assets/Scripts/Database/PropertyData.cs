using SQLite4Unity3d;

public class PropertyData { // Stores property definitions used by all beaches
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; } // Unique database id for the property
    public string Name { get; set; } // Name of the property
    public string Type { get; set; } // Data type of the property: bool, int or string
}