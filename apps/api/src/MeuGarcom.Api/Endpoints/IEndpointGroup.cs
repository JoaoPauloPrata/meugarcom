using System.Reflection;

namespace MeuGarcom.Api.Endpoints;

/// <summary>A group of endpoints of one feature. Discovered automatically by <see cref="EndpointExtensions.MapEndpoints"/>.</summary>
public interface IEndpointGroup
{
    void Map(IEndpointRouteBuilder app);
}

public static class EndpointExtensions
{
    public static IEndpointRouteBuilder MapEndpoints(this IEndpointRouteBuilder app)
    {
        var groups = Assembly.GetExecutingAssembly().GetTypes()
            .Where(t => t is { IsAbstract: false, IsInterface: false } && t.IsAssignableTo(typeof(IEndpointGroup)))
            .Select(Activator.CreateInstance)
            .Cast<IEndpointGroup>();

        foreach (var group in groups)
            group.Map(app);

        return app;
    }
}
