using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace LoggerApp.DTOs;

public record PaginationDTO
{
    [FromQuery(Name = "page")]
    public int Page { get; set; } = 1;

    [FromQuery(Name = "limit")]
    public int Limit { get; set; } = 10;

    [BindNever]
    public int Offset { get => (Page - 1) * Limit; }

}