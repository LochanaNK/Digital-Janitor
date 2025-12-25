namespace DigitalJanitor.Models;

public class JanitorSettings
{
    public string WatchPath {get;set;} = string.Empty;
    public string TargetBase {get;set;} = string.Empty;
    public Dictionary<string,string> Extensions {get;set;} = new();
}