using Tacdent.Core.Results;

namespace Tacdent.Application.Errors;

public static class UserErrors
{
    public static Error NotFound(Guid id) => Error.NotFound(
        "User.NotFound",
        $"User with id '{id}' was not found.");

    public static readonly Error EmailAlreadyExists = Error.Conflict(
        "User.EmailAlreadyExists",
        "A user with this email already exists.");

    public static readonly Error CannotModifyLastAdmin = Error.Conflict(
        "User.CannotModifyLastAdmin",
        "Cannot demote or deactivate the last active admin.");

    public static readonly Error InactiveAssignee = Error.Validation(
        "User.InactiveAssignee",
        "The selected assignee is inactive.");

    public static Error AssigneeNotFound(Guid id) => Error.NotFound(
        "User.AssigneeNotFound",
        $"Assignee with id '{id}' was not found.");
}
