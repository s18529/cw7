using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Zad3.Middleware
{
    public class LoggingMiddleware
    {
        private readonly RequestDelegate _next;
        public LoggingMiddleware(RequestDelegate next) { _next = next; }
        public async Task InvokeAsync(HttpContext httpContext)
        {
            httpContext.Request.EnableBuffering();
            string logg ="Metoda :"+ httpContext.Request.Method + " ";
            logg += "sciezka wywolania :" + httpContext.Request.Path + " ";
            var bodyStream = string.Empty;
            using (var reader = new StreamReader(httpContext.Request.Body, Encoding.UTF8, true, 1024, true))
            {
                bodyStream = await reader.ReadToEndAsync();
            }
            logg += "Body :"+bodyStream + " ";
            logg +="Querystring :"+ httpContext.Request.QueryString;
            var path = Directory.GetCurrentDirectory();
            path += "\\requestsLog.txt";
            using(StreamWriter sw = File.AppendText(path))
            {
                sw.WriteLine(logg);
                sw.Close();
            }
            httpContext.Request.Body.Seek(0, SeekOrigin.Begin);

            await _next(httpContext);
        }
    }
}
