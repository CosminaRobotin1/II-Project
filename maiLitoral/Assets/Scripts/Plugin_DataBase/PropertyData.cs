using SQLite4Unity3d;

// Stores property definitions used by all beaches.
public class PropertyData{
    // Unique database id for the property.
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }

    // Name of the property.
    public string Name { get; set; }

    // Data type of the property: bool, int or string.
    public string Type { get; set; }


}