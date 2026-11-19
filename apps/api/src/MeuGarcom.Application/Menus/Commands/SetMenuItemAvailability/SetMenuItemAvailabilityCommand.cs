using Mediator;
using MeuGarcom.Domain.Common;

namespace MeuGarcom.Application.Menus.Commands.SetMenuItemAvailability;

/// <summary>Unavailable items are hidden from the customer chat.</summary>
public sealed record SetMenuItemAvailabilityCommand(Guid UnitId, Guid ItemId, bool IsAvailable) : ICommand<Result>;
