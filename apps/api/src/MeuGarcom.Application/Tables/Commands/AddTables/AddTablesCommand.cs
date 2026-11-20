using FluentValidation;
using Mediator;
using MeuGarcom.Application.Common.Interfaces;
using MeuGarcom.Application.Common.Messages;
using MeuGarcom.Application.Common.Persistence;
using Unit = MeuGarcom.Domain.Accounts.Unit;
using MeuGarcom.Domain.Accounts;
using MeuGarcom.Domain.Common;
using Microsoft.EntityFrameworkCore;

namespace MeuGarcom.Application.Tables.Commands.AddTables;

public sealed record AddTablesCommand(Guid UnitId, int Count) : ICommand<Result<int>>;
