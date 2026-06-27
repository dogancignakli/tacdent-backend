using Tacdent.Core.Entities;

namespace Tacdent.Core.DTOs;

public record AppointmentQuery(
    AppointmentStatus? Status,
    int Page,
    int PageSize,
    AppointmentSortField SortBy,
    SortDirection SortDirection);
