using AiReporting.Api.Application.AiReports.SemanticModel;

namespace AiReporting.Api.Application.AiReports.Validation;

public interface ISemanticQueryValidator
{
    void Validate(SemanticQuery query);
}
