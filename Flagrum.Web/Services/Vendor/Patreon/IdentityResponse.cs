using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Flagrum.Web.Services.Vendor.Patreon;

public class PatreonIdentityResponse
{
    [JsonPropertyName("included")]
    public IEnumerable<PatreonIdentityMembership> Memberships { get; set; }
}

public class PatreonIdentityMembership
{
    [JsonPropertyName("attributes")]
    public PatreonIdentityMembershipAttributes Attributes { get; set; }
}

public class PatreonIdentityMembershipAttributes
{
    [JsonPropertyName("patron_status")]
    public string PatronStatus { get; set; }
    
    [JsonPropertyName("currently_entitled_amount_cents")]
    public int CurrentlyEntitledAmountCents { get; set; }
}