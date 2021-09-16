using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Security.Cryptography;
using System.Text;
using System.IO;
using Microsoft.Extensions.Configuration;
using System.ComponentModel;
using Newtonsoft.Json;
using static RestrictIP.Middleware.DomainException;

namespace RestrictIP.Middleware
{
    public class IPFilter
    {

        private readonly RequestDelegate _next;
        private IConfiguration _configuration;
        public IPFilter(RequestDelegate next, IConfiguration configuration)
        {
            _next = next;
            _configuration = configuration;
        }
        //for encription of the url 

        public static string EncryptString(string key, string plainText)
        {
            byte[] iv = new byte[16];
            byte[] array;

            using (Aes aes = Aes.Create())
            {
                aes.Key = Encoding.UTF8.GetBytes(key);
                aes.IV = iv;

                ICryptoTransform encryptor = aes.CreateEncryptor(aes.Key, aes.IV);

                using (MemoryStream memoryStream = new MemoryStream())
                {
                    using (CryptoStream cryptoStream = new CryptoStream((Stream)memoryStream, encryptor, CryptoStreamMode.Write))
                    {
                        using (StreamWriter streamWriter = new StreamWriter((Stream)cryptoStream))
                        {
                            streamWriter.Write(plainText);
                        }

                        array = memoryStream.ToArray();
                    }
                }
            }

            return Convert.ToBase64String(array);
        }

        //private static Task HandleExceptionMessageAsync(HttpContext context, Exception exception)
        //{

        //    int statusCode = (int)HttpStatusCode.InternalServerError;

        //    var commanMessage = new CommanMessage
        //    {
        //        statusCode = statusCode,
        //        errorMessage = exception.Message
        //    };

        //    string jsonString = JsonConvert.SerializeObject(commanMessage);

        //    return context.Response.WriteAsync(jsonString);
        //}

        public bool DataCheck(HttpContext context)
        {
            var ipAddress = context.Connection.RemoteIpAddress;

            var domain = context.Response.HttpContext.Request.Host;

            string urlPath = context.Response.HttpContext.Request.Path;

            string url = domain + urlPath;

            string key = "b14ca5898a4e4133bbce2ea2315a1916";

            string encryptedurl = EncryptString(key, url);


            List<string> whiteListIPList = _configuration.GetSection("ApplicationOptions:Whitelist").Get<List<string>>();
            List<string> whitelisturlList = _configuration.GetSection("ApplicationOptions:WhitelistUrl").Get<List<string>>();

            var isInwhiteListIPList = whiteListIPList
                .Where(a => IPAddress.Parse(a)
                .Equals(ipAddress))
                .Any();


            var isInwhiteListDomain = whitelisturlList
                .Where(a => a
                .Equals(encryptedurl))
                .Any();


            if (!isInwhiteListIPList || !isInwhiteListDomain)
            {

                if (!isInwhiteListIPList)
                {
                    var commanMessage = new CommanMessage
                    {
                        statusCode = context.Response.StatusCode = (int)HttpStatusCode.Forbidden,
                        errorMessage = "You don't have valid IP"
                    };

                    string jsonString = JsonConvert.SerializeObject(commanMessage);
                    context.Response.WriteAsync(jsonString);

                }
                if (!isInwhiteListDomain)
                {

                    var commanMessage = new CommanMessage
                    {
                        statusCode = context.Response.StatusCode = (int)HttpStatusCode.Forbidden,
                        errorMessage = "You don't have valid domain"
                    };

                    string jsonString = JsonConvert.SerializeObject(commanMessage);
                    context.Response.WriteAsync(jsonString);
                }

                return false;

            }
            return true;
        }

        public async Task Invoke(HttpContext context)
        {
            bool check = DataCheck(context);

            if (check)
            {
                await _next.Invoke(context);
            }

        }
    }

    // Extension method used to add the middleware to the HTTP request pipeline.
    public static class MiddlewareExtensions
    {
        public static IApplicationBuilder UseIPFilter(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<IPFilter>();
        }
    }


    //public class ExceptionHandlingMiddleware : IMiddleware
    //{
    //    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    //    {
    //        try
    //        {
    //            await next(context);
    //        }
    //        catch (DomainNotFoundException exception)
    //        {
    //            int statusCode = (int)HttpStatusCode.InternalServerError;

    //            var commanMessage = new CommanMessage
    //            {
    //                statusCode = statusCode,
    //                errorMessage = exception.Message
    //            };

    //            string jsonString = JsonConvert.SerializeObject(commanMessage);

    //            await context.Response.WriteAsync(jsonString);
    //        }
    //        catch (DomainValidationException exception)
    //        {
                
    //            int statusCode = (int)HttpStatusCode.BadRequest;

    //            var commanMessage = new CommanMessage
    //            {
    //                statusCode = statusCode,
    //                errorMessage = exception.Message
    //            };

    //            string jsonString = JsonConvert.SerializeObject(commanMessage);

    //            await context.Response.WriteAsync(jsonString);
    //        }
    //        catch (Exception exception)
    //        {
    //            int statusCode = (int)HttpStatusCode.BadRequest;

    //            var commanMessage = new CommanMessage
    //            {
    //                statusCode = statusCode,
    //                errorMessage = exception.Message
    //            };

    //            string jsonString = JsonConvert.SerializeObject(commanMessage);

    //            await context.Response.WriteAsync(jsonString);
    //        }
    //    }
    //}



}
