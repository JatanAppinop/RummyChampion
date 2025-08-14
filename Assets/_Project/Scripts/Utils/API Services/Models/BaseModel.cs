using System;
using System.Collections.Generic;

[System.Serializable]
public class BaseModel<T>
{
    public bool success;
    public string message;
    public int statusCode = 200;
    public T data;
    
    // 🔹 ENHANCED ERROR HANDLING AND METADATA
    public string errorCode;
    public List<string> errors;
    public Dictionary<string, object> metadata;
    public DateTime timestamp;
    public string requestId;
    public string version = "1.0";
    
    // Pagination Support
    public int page;
    public int pageSize;
    public int totalPages;
    public int totalRecords;
    public bool hasNextPage;
    public bool hasPreviousPage;
    
    // Performance Metrics
    public double responseTime; // in milliseconds
    public string serverRegion;
    
    // Security
    public string signature;
    public DateTime? expiresAt;
    
    // 🔹 HELPER METHODS
    public bool IsSuccessful => success && statusCode >= 200 && statusCode < 300;
    public bool HasErrors => errors != null && errors.Count > 0;
    public bool IsExpired => expiresAt.HasValue && DateTime.UtcNow > expiresAt.Value;
    
    public void AddError(string error)
    {
        if (errors == null) errors = new List<string>();
        errors.Add(error);
        success = false;
    }
    
    public void AddMetadata(string key, object value)
    {
        if (metadata == null) metadata = new Dictionary<string, object>();
        metadata[key] = value;
    }
}

// 🔹 COMMON API RESPONSE MODELS

[System.Serializable]
public class ApiResponse : BaseModel<object>
{
    
}

[System.Serializable]
public class StringResponse : BaseModel<string>
{
    
}

[System.Serializable]
public class BooleanResponse : BaseModel<bool>
{
    
}

[System.Serializable]
public class IntegerResponse : BaseModel<int>
{
    
}

[System.Serializable]
public class DoubleResponse : BaseModel<double>
{
    
}
