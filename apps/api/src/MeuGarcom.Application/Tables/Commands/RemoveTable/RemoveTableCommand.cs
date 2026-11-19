using Mediator;
using MeuGarcom.Application.Common.Interfaces;
using MeuGarcom.Application.Common.Persistence;
using MeuGarcom.Domain.Accounts;
using MeuGarcom.Domain.Common;
using Microsoft.EntityFrameworkCore;

namespace MeuGarcom.Application.Tables.Commands.RemoveTable;

public sealed record RemoveTableCommand(Guid UnitId, Guid TableId) : ICommand<Result>;
