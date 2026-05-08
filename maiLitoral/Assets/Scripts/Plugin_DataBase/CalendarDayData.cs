using SQLite4Unity3d;

// Stores a calendar day associated with a specific beach.
public class CalendarDayData
{

    // Unique database id for the calendar day.
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }

    // Id of the beach associated with this day.
    public int BeachId { get; set; }

    // Date associated with the beach data.
    public string Date { get; set; }
}