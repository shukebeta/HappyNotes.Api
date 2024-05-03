namespace Api.Framework.Result;

public class SuccessfulResult<T>: ApiResult<T>
{
    public SuccessfulResult(T data)
    {
        Successful = true;
        ErrorCode = 0;
        Message = "Successful";
        Data = data;
    }
}