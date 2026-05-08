using SQLite4Unity3d;

// Database model used for storing beach data and its associated zone.
public class BeachData
{

    // Unique database id for the beach.
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }

    // Id of the zone to which the beach belongs.
    public int ZoneId { get; set; }

    // Name of the beach.
    public string Name { get; set; }
}
