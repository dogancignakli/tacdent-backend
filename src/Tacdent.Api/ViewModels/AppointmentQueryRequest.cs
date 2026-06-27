using Tacdent.Core.DTOs;
using Tacdent.Core.Entities;

namespace Tacdent.Api.ViewModels;

public record AppointmentQueryRequest(
    AppointmentStatus? Status = null,
    int Page = 1,
    int PageSize = 20,
    AppointmentSortField SortBy = AppointmentSortField.PreferredDate,
    SortDirection SortDirection = SortDirection.Desc);
