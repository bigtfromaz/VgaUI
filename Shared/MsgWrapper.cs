
internal class MsgWrapper<T>
{
    // This class is used to wrap messages for transfer between clients and REST methods.
    public int StatusCode { get; set; }
    public string Message { get; set; } = "";
    public T? Content { get; set; }
}
