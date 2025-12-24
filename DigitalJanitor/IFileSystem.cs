namespace DigitalJanitor.Interfaces;
// The Interface
public interface IFileSystem
{
    bool Exists(string path);
    void CreateDirectory(string path);
    void Move(string source, string dest, bool overwrite);
    DateTime GetCreationTime(string path);
    bool IsFileReady(string path); // Add this
}

// The Real Implementation (Used in Production)
public class PhysicalFileSystem : IFileSystem
{
    public bool Exists(string path) => File.Exists(path);
    public void CreateDirectory(string path) => Directory.CreateDirectory(path);
    public void Move(string source, string dest, bool overwrite) => File.Move(source, dest, overwrite);
    public DateTime GetCreationTime(string path) => File.GetCreationTime(path);

    public bool IsFileReady(string path)
    {
        try
        {
            using FileStream fs = File.Open(path, FileMode.Open, FileAccess.ReadWrite, FileShare.None);
            return true;
        }
        catch { return false; }
    }
}