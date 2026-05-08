using SQLite4Unity3d;

// Stores zone information from the database.
public class ZoneData
{

    // Unique database id for the zone.
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }

    // Name of the zone.
    public string Name { get; set; }
}