using Azure.Core;
using Microsoft.CodeAnalysis.Elfie.Diagnostics;

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace GUIWebApi.Tools
{
    public static class PathTools
    {
        private static IHttpContextAccessor _accessor;

        public static void Configure(IHttpContextAccessor accessor)
        {
            _accessor = accessor;
        }

        //public static string MakeUrl(this string virtualOrRelativePath)
        //{
        //    if (string.IsNullOrWhiteSpace(virtualOrRelativePath))
        //        return string.Empty;

        //    if (virtualOrRelativePath.StartsWith("http", StringComparison.OrdinalIgnoreCase))
        //        return virtualOrRelativePath;

        //    string baseUrl = "1";
        //    //string baseUrl = string.Concat(Request.Scheme, "://", Request.Host.ToUriComponent());
        //    string path = virtualOrRelativePath.StartsWith("/") ? virtualOrRelativePath : "/" + virtualOrRelativePath;

        //    return baseUrl + path;
        //}

        public static string MakeUrl(this string virtualOrRelativePath)
        {
            if (string.IsNullOrWhiteSpace(virtualOrRelativePath)) return string.Empty;
            if (virtualOrRelativePath.StartsWith("http", StringComparison.OrdinalIgnoreCase)) return virtualOrRelativePath;

            var request = _accessor?.HttpContext?.Request;
            if (request == null) return virtualOrRelativePath; // Fallback hvis kaldt udenfor HTTP context

            string baseUrl = $"{request.Scheme}://{request.Host.ToUriComponent()}";
            string path = virtualOrRelativePath.StartsWith("/") ? virtualOrRelativePath : "/" + virtualOrRelativePath;

            return baseUrl + path;
        }
    }
}
