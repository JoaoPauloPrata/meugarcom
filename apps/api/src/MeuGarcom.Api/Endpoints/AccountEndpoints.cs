using Mediator;
using MeuGarcom.Api.Http;
using MeuGarcom.Application.Accounts.Commands.CompleteOnboarding;
using MeuGarcom.Application.Accounts.Queries.GetWorkspace;

namespace MeuGarcom.Api.Endpoints;

public sealed class AccountEndpoints : IEndpointGroup
{
    public void Map(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api").WithTags("Account").RequireAuthorization();

        group.MapGet("/me/workspace", async (ISender sender, CancellationToken ct) =>
            (await sender.Send(new GetWorkspaceQuery(), ct)).ToHttp());

        group.MapPost("/onboarding", async (CompleteOnboardingCommand command, ISender sender, CancellationToken ct) =>
            (await sender.Send(command, ct)).ToHttp());
    }
}
