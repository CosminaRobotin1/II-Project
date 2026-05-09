using SQLite4Unity3d;

public class BeachData { // Database model used for storing beach data and its associated zone
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; } // Unique database id for the beach
    public int ZoneId { get; set; } // Id of the zone to which the beach belongs
    public string Name { get; set; } // Name of the beach
}
