using SQLite4Unity3d;

public class ZoneData { // Stores zone information from the database
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; } // Unique database id for the zone
    public string Name { get; set; } // Name of the zone
}