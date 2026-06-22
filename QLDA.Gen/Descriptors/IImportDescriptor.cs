using QLDA.Gen.Metadata;

namespace QLDA.Gen.Descriptors;

public interface IImportDescriptor {
    string EntityName { get; }
    string TemplateFileName { get; }
    string TableName { get; }
    List<ImportColumn> Columns { get; }
    string OutputPath { get; set; }
    string? Title { get; }
    string? HintText { get; }
}
