using Flagrum.Abstractions;
using Injectio.Attributes;

namespace Flagrum.Application.Features.Information.News;

/// <summary>
/// Creates instances of <see cref="NewsFlowBuilder" />.
/// </summary>
[RegisterSingleton<NewsFlowBuilderFactory>]
public class NewsFlowBuilderFactory(
    IProfileService profile,
    IApplication application)
{
    /// <summary>
    /// Creates a <see cref="NewsFlowBuilder" />.
    /// </summary>
    public NewsFlowBuilder Create() => new(profile, application);
}