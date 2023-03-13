using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Components;

namespace Flagrum.Web.Utilities;

public static class Extensions
{
    public static IDictionary<string, string> GetQueryParameters(this NavigationManager navigationManager)
    {
        var queryStrings = navigationManager.Uri.Split('?').Last().Split('&');
        return queryStrings
            .Select(q => q.Split('='))
            .ToDictionary(r => r[0], r => r[1]);
    }
}