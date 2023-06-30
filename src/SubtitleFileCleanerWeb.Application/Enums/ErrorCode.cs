﻿namespace SubtitleFileCleanerWeb.Application.Enums;

public enum ErrorCode
{
    UnprocessableContent = 402,
    NotFound = 404,

    // Validation errors should be in the range 600-699
    ValidationError = 600,

    // Infrastructure errors should be in the range 700-799
    BlobContextOperationException = 700,

    // Application errors should be in the range 800-899
    SubtitleConversionException = 800,
    PostConversionException = 801,

    UnknownError = 999
}
    