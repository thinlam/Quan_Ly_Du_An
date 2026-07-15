using System.Net;
using Newtonsoft.Json;

namespace BuildingBlocks.Application.Common.DTOs;

public class ResultApi {
    public bool Result { get; set; } = true;
    public string? ErrorMessage { get; set; } = "";
    public object? DataResult { get; set; } = null;
    public int StatusCode { get; set; } = (int)HttpStatusCode.OK;
    public static ResultApi Ok(object? data = null) => new() { Result = true, DataResult = data, StatusCode = (int)HttpStatusCode.OK };
    public static ResultApi Fail(string msg) => new() { Result = false, ErrorMessage = msg, StatusCode = (int)HttpStatusCode.BadRequest };
    public override string ToString() => JsonConvert.SerializeObject(this);
}

public class ResultApi<T> : ResultApi {
    public new T DataResult { get; set; } = default!;
    public static ResultApi<T> Ok(T data) => new() { Result = true, DataResult = data, StatusCode = (int)HttpStatusCode.OK };
}
