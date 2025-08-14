using System;
using System.Collections.Generic;

[System.Serializable]
public class SimpleBaseModel
{
    public bool success;
    public string message;
    
    // 🔹 ENHANCED SIMPLE RESPONSE PROPERTIES
    public int statusCode = 200;
    public string errorCode;
    public DateTime timestamp = DateTime.UtcNow;
    public string requestId;
    
    // Quick status checks
    public Dictionary<string, string> details;
    public string action; // What action was performed
    public string resource; // What resource was affected
    
    // 🔹 HELPER METHODS
    public bool IsSuccessful => success && statusCode >= 200 && statusCode < 300;
    
    public void SetSuccess(string msg = "Operation completed successfully")
    {
        success = true;
        message = msg;
        statusCode = 200;
        timestamp = DateTime.UtcNow;
    }
    
    public void SetError(string msg, int code = 400, string errCode = "GENERAL_ERROR")
    {
        success = false;
        message = msg;
        statusCode = code;
        errorCode = errCode;
        timestamp = DateTime.UtcNow;
    }
    
    public void AddDetail(string key, string value)
    {
        if (details == null) details = new Dictionary<string, string>();
        details[key] = value;
    }
}

// 🔹 SPECIALIZED SIMPLE MODELS

[System.Serializable]
public class SimpleActionResponse : SimpleBaseModel
{
    public string actionResult;
    public object actionData;
}

[System.Serializable]
public class SimpleValidationResponse : SimpleBaseModel
{
    public bool isValid;
    public List<string> validationErrors;
    
    public void AddValidationError(string error)
    {
        if (validationErrors == null) validationErrors = new List<string>();
        validationErrors.Add(error);
        isValid = false;
        SetError("Validation failed");
    }
}

[System.Serializable]
public class SimpleStatusResponse : SimpleBaseModel
{
    public string status; // "active", "inactive", "pending", "completed", etc.
    public string statusDescription;
    public DateTime? statusChangedAt;
}

