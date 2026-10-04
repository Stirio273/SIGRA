using System;

namespace SIGRA.Controllers.Helper;

public static class FileHelper
{
    public static string GetContentType(string filePath) =>
        System.IO.Path.GetExtension(filePath).ToLower() switch
        {
            ".pdf" => "application/pdf",
            ".png" => "image/png",
            ".jpg" => "image/jpeg",
            ".jpeg" => "image/jpeg",
            ".gif" => "image/gif",
            ".docx" => "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
            ".doc" => "application/msword",
            ".xlsx" => "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            ".xls" => "application/vnd.ms-excel",
            ".txt" => "text/plain",
            ".rst" => "text/x-rst",
            _ => "application/octet-stream"
        };
}