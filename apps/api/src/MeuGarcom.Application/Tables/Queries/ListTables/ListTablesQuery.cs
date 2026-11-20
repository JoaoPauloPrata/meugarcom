using Mediator;
using MeuGarcom.Application.Common.Interfaces;
using MeuGarcom.Application.Common.Persistence;
using MeuGarcom.Domain.Accounts;
using MeuGarcom.Domain.Common;
using Microsoft.EntityFrameworkCore;

namespace MeuGarcom.Application.Tables.Queries.ListTables;

public sealed record ListTablesQuery(Guid UnitId) : IQuery<Result<TablesDto>>;

/// <param name="UnitPublicCode">Code of the unit-wide link (no specific table).</param>
public sealed record TablesDto(string UnitPublicCode, IReadOnlyList<TableDto> Tables);

public sealed record TableDto(Guid Id, int Number, string PublicCode);
