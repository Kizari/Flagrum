using System.Collections.Generic;

namespace Flagrum.Application.Features.Settings.Data;

public class LanguageTable
{
    private Dictionary<string, string> _table = new();

    public LanguageTable()
    {
        _table.Add("English", "English");
        _table.Add("Japanese", "日本語");
        _table.Add("Chinese (Simplified)", "简体中文");
        _table.Add("Chinese (Traditional)", "繁體中文");
    }

    public string this[string key]
    {
        get => _table[key];
        set => _table[key] = value;
    }
}