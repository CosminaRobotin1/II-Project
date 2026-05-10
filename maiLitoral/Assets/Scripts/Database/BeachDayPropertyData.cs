using SQLite4Unity3d;

public class BeachDayPropertyData { // Stores property values for a specific beach calendar day
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; } // Unique database id for the property value
    public int CalendarDayId { get; set; } // Id of the associated calendar day
    public int PropertyId { get; set; } // Id of the associated property definition
    public bool? BoolValue { get; set; } // Boolean value for bool properties
    public int? IntValue { get; set; } // Integer value for int properties
    public string StringValue { get; set; } // String value for string properties
}