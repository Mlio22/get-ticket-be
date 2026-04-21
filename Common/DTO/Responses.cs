namespace Common.DTO;

public class BaseResponse
{
    public bool IsOk { get; set; }
    public string ErrorMessage { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public int AnyChange { get; set; } = 0;
    public bool IsRefresh { get; set; } = true;
}

public class DataResponse<T> : BaseResponse
{
    public T? Data { get; set; }
}

public class SearchColumn
{
    public string Field { get; set; } = string.Empty;
    public string Label { get; set; } = string.Empty;
}

public class ListResponse<T> : BaseResponse
{
    public List<T> List { get; set; } = [];
    public List<SearchColumn> SearchColumnList { get; set; } = [];
    public int RecordCount { get; set; }
}
