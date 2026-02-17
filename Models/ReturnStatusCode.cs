namespace Market.Models;

public enum ReturnStatusCode
{
    Success,
    BadRequest,
    Conflict,
    NotFound,
    Deleted
}

public record StatusResult(ReturnStatusCode Code, string Message);
public record StatusResultParametrs<T>(ReturnStatusCode Code, string Message, T Data);


