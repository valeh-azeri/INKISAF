using OzunuInkisaf.Contracts.Common;

namespace OzunuInkisaf.Contracts.Duas;

public record DuaDto(
    Guid Id,
    string Title,
    string? ArabicText,
    string? Translation,
    string? Transliteration,
    DuaCategoryDto Category,
    bool IsActive,
    bool HasPdf);

public record UpsertDuaRequest(
    string Title,
    string? ArabicText,
    string? Translation,
    string? Transliteration,
    DuaCategoryDto Category);
