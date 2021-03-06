﻿using System;
using System.IO;
using System.Net;
using System.Text;

namespace DMI.Data
{
    public static class WebRequestEx
    {
        public static void DownloadStringAsync(this WebRequest request, Action<string> callback)
        {
            DownloadStringAsync(request, Encoding.GetEncoding("iso-8859-1"), callback);
        }

        public static void DownloadStringAsync(this WebRequest request, Encoding encoding, Action<string> callback)
        {
            if (request == null)
                throw new ArgumentNullException("request");

            if (encoding == null)
                throw new ArgumentNullException("encoding");

            if (callback == null)
                throw new ArgumentNullException("callback");

            request.BeginGetResponse((IAsyncResult result) =>
            {
                try
                {
                    var response = request.EndGetResponse(result);
                    using (var reader = new StreamReader(response.GetResponseStream(), encoding))
                    {
                        callback(reader.ReadToEnd());
                    }
                }
                catch (WebException e)
                {
                    // Don't perform a callback, as this error is mostly due to
                    // there being no internet connection available. 
                    System.Diagnostics.Debug.WriteLine(e.Message);
                }
            }, request);
        }
    }
}
