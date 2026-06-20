namespace InstrumentCatalogue.API.ReadModels;

public class ApiResponse<T>
{
    public T? Data { get; set; }

    public bool IsError {  get; set; }

    public ErrorDetail? ErrorDetail { get; set; }


    public static ApiResponse<T> Success(T data)
    {
      return new ApiResponse<T> { Data = data, IsError = false };
    }

    public static ApiResponse<T> Fail(ErrorDetail error)
    {
        return new ApiResponse<T> { Data = default, IsError = true, ErrorDetail = error  };
    }




}
