﻿using Microsoft.AspNetCore.Http;
using OCM.API.Common;
using OCM.API.Common.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace OCM.API.OutputProviders
{
    interface IOutputProvider
    {
        string ContentType { get; set; }
        Task GetOutput(HttpContext context, Stream outputStream, IEnumerable<OCM.API.Common.Model.ChargePoint> dataList, APIRequestParams settings);
        Task GetOutput(HttpContext context, Stream outputStream, CoreReferenceData data, APIRequestParams settings);
        Task GetOutput(HttpContext context, Stream outputStream, Object data, APIRequestParams settings);
    }
}