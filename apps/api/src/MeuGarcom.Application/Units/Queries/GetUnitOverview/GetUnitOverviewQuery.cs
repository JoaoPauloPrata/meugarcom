using Mediator;
using MeuGarcom.Domain.Common;

namespace MeuGarcom.Application.Units.Queries.GetUnitOverview;

public sealed record GetUnitOverviewQuery(Guid UnitId) : IQuery<Result<UnitOverviewDto>>;

/// <param name="QuestionsByHour">24 buckets (local time, America/Sao_Paulo) with customer messages of the last 7 days.</param>
/// <param name="DietaryQuestions">How many customer messages of the last 30 days mention each restriction.</param>
public sealed record UnitOverviewDto(
    int ConversationsToday,
    int QuestionsToday,
    int MenuItems,
    int Tables,
    int BlockedAttemptsLast7Days,
    IReadOnlyList<int> QuestionsByHour,
    IReadOnlyList<DietaryQuestionCount> DietaryQuestions);

public sealed record DietaryQuestionCount(string Attribute, int Count);
