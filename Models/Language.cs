using System.Collections.Generic;

public class Language
{
    public int Id { get; set; }
    public string? Name1 { get; set; }
    public string? Name2 { get; set; }
    public List<Shloka> Data { get; set; } = new List<Shloka>();
    public string? Sanskrit { get; set; }
    public string? Gujrati { get; set; }
    public string? Hindi { get; set; }
    public string? Number1 { get; set; }
    public bool IsLiked { get; set; }
}
