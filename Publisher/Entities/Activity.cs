namespace Publisher.Entities;

public class Activity
{
    public int Id { get; set; }
    public string Name { get; set; } = String.Empty;
    public string Location { get; set; } = String.Empty;
    public DateTime DateTime { get; set; }
    public int Capacity { get; set; }
    public double Price { get; set; } 
}