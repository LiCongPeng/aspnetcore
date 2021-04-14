// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Microsoft.AspNetCore.HttpLogging
{
    /// <summary>
    /// Logging enum to enable different request and response logging fields.
    /// </summary>
    [Flags]
    public enum HttpLoggingFields
    {
        /// <summary>
        /// None
        /// </summary>
        None = 0,

        /// <summary>
        /// Request Path
        /// </summary>
        Path = 1,

        /// <summary>
        /// Request Query String
        /// </summary>
        Query = 2,

        /// <summary>
        /// Request Protocol
        /// </summary>
        Protocol = 4,

        /// <summary>
        /// Request Method
        /// </summary>
        Method = 8,

        /// <summary>
        /// Request Scheme
        /// </summary>
        Scheme = 0x10,

        /// <summary>
        /// Response Status Code
        /// </summary>
        StatusCode = 0x20,

        /// <summary>
        /// Request Headers
        /// </summary>
        RequestHeaders = 0x40,

        /// <summary>
        /// Response Headers
        /// </summary>
        ResponseHeaders = 0x80,

        /// <summary>
        /// Request Body
        /// </summary>
        RequestBody = 0x100,

        /// <summary>
        /// Response Body
        /// </summary>
        ResponseBody = 0x200,

        /// <summary>
        /// Combination of request properties, including Path, Query, Protocol, Method, and Scheme
        /// </summary>
        RequestProperties = Path | Query | Protocol | Method | Scheme,

        /// <summary>
        /// Combination of Request Properties and Request Headers
        /// </summary>
        RequestPropertiesAndHeaders = RequestProperties | RequestHeaders,

        /// <summary>
        /// Combination of Response Properties and Response Headers
        /// </summary>
        ResponsePropertiesAndHeaders = StatusCode | ResponseHeaders,

        /// <summary>
        /// Entire Request
        /// </summary>
        Request = RequestPropertiesAndHeaders | RequestBody,

        /// <summary>
        /// Entire Response
        /// </summary>
        Response = StatusCode | ResponseHeaders | ResponseBody,

        /// <summary>
        /// Entire Request and Response
        /// </summary>
        All = Request | Response
    }
}
