namespace AiReporting.Api.Application.AiReports.SemanticModel;

public interface ISemanticModelRegistry
{
    IReadOnlyList<SemanticDataset> GetAllDatasets();
    SemanticDataset GetDataset(string name);
}
