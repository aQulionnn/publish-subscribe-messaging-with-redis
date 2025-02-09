namespace Publisher.Dtos;

public class UpdateActivityDto
{
    public string Location { get; set; } = String.Empty;
    public DateTime DateTime { get; set; }
    public int Capacity { get; set; }
    public double Price { get; set; }
}