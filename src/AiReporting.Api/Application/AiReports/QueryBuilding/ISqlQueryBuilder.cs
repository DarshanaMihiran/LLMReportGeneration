using AiReporting.Api.Application.AiReports.SemanticModel;

namespace AiReporting.Api.Application.AiReports.QueryBuilding;

public interface ISqlQueryBuilder
{
    BuiltSqlQuery Build(SemanticQuery query);
}
