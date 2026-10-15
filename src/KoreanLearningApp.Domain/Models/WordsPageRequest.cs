using KoreanLearningApp.Domain.Models.Enums;

namespace KoreanLearningApp.Domain.Models;

/// <summary>
/// Страница словаря: поиск, фильтр по тегам (слово подходит, если у него есть хотя бы один из тегов) и сортировка.
/// </summary>
public record WordsPageRequest(
    int Page,
    int PageSize,
    string? Search = null,
    IReadOnlyCollection<int>? TagIds = null,
    WordSortEnum Sort = WordSortEnum.Rank);
