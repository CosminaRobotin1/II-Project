using SQLite4Unity3d;

public class CalendarDayData { // Stores a calendar day associated with a specific beach
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; } // Unique database id for the calendar day
    public int BeachId { get; set; } // Id of the beach associated with this day
    public string Date { get; set; } // Date associated with the beach data
    public float DailyRank { get; set; } // Rank of the beach
}