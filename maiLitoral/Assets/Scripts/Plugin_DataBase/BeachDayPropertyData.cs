using SQLite4Unity3d;

// Stores property values for a specific beach calendar day.
public class BeachDayPropertyData
{

    // Unique database id for the property value.
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }

    // Id of the associated calendar day.
    public int CalendarDayId { get; set; }

    // Id of the associated property definition.
    public int PropertyId { get; set; }

    // Boolean value for bool properties.
    public bool? BoolValue { get; set; }

    // Integer value for int properties.
    public int? IntValue { get; set; }

    // String value for string properties.
    public string StringValue { get; set; }
}