using System;
using System.ComponentModel.DataAnnotations;

namespace ExpensesBook.Domain.Entities;

internal sealed class Group : IEntity
{
    [Key]
    public Guid Id { get; set; }

    [Required, StringLength(50)]
    public string Name { get; set; } = "";

    public int Sort { get; set; }
}

internal sealed class GroupDto
{
    public Guid? Id { get; set; }

    [Required(ErrorMessage = "Название обязательно")]
    [StringLength(50, ErrorMessage = "Название не должно быть больше 50 символов")]
    public string? Name { get; set; }

    public int? Sort { get; set; }
}

internal static class ExpensesGroupDtoExtensions
{
    public static string GetName(this GroupDto? grDto) => grDto?.Name ?? "";

    public static int GetSort(this GroupDto? grDto) => grDto?.Sort ?? 1;

    public static Guid GetId(this GroupDto? grDto) => grDto?.Id ?? Guid.Empty;
}

internal sealed record GroupDefaultCategory : IEntity
{
    public Guid Id { get; set; }

    public Guid GroupId { get; set; }

    public Guid CategoryId { get; set; }
}
