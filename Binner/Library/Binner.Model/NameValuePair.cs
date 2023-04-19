namespace Binner.Model;

public class NameValuePair<T>
{
    public string? Name { get; set; }
    public T? Value { get; set; }
    public NameValuePair() { }
    public NameValuePair(string name, T value)
    {
        Name = name;
        Value = value;
    }
}