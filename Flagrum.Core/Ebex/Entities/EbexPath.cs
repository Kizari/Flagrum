namespace Flagrum.Core.Ebex.Entities;

public class EbexPath
{
    private string _path;

    public EbexPath(string path)
    {
        _path = path;
    }

    public bool IsNullOrEmpty => string.IsNullOrEmpty(_path);

    public string Pop()
    {
        string result;
        var index = _path.IndexOf('.');

        if (index >= 0)
        {
            result = _path[..index];
            _path = _path[(index + 1)..];
            return result;
        }

        result = _path;
        _path = null;
        return result;
    }
}