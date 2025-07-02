[System.Serializable]
public class BaseModel<T>
{
    public bool success;
    public string message;
    //public int statusCode;
    public T data;
}
