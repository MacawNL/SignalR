// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.md in the project root for license information.

using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Net;
using System.Security.Cryptography.X509Certificates;

namespace Microsoft.AspNet.SignalR.Client.Http
{
    public class HttpWebRequestWrapper : IRequest
    {
        private readonly IDictionary<string, string> _headerDictionary = new Dictionary<string, string>();
        private readonly HttpWebRequest _request;

        private IDictionary<string, Action<HttpWebRequest, string>> _restrictedHeaderSet = new Dictionary<string, Action<HttpWebRequest, string>>() {
                                                                        { HttpRequestHeader.Accept.ToString(), (request, value) => { request.Accept = value; } },
                                                                        { HttpRequestHeader.Connection.ToString(), (request, value) => { request.Connection = value; } },
                                                                        { HttpRequestHeader.ContentType.ToString(), (request, value) => { request.ContentType = value; } },
                                                                        { HttpRequestHeader.ContentLength.ToString(), (request, value) => { request.ContentLength = Int32.Parse(value); } },                                                                        
                                                                    };

        private IDictionary<string, Func<HttpWebRequest, string>> _restrictedHeaderGet = new Dictionary<string, Func<HttpWebRequest, string>>() {
                                                                        { HttpRequestHeader.Accept.ToString(), (request) => { return request.Accept; } },
                                                                        { HttpRequestHeader.Connection.ToString(), (request) => { return request.Connection; } },
                                                                        { HttpRequestHeader.ContentType.ToString(), (request) => { return request.ContentType; } },
                                                                        { HttpRequestHeader.ContentLength.ToString(), (request) => { return request.ContentLength.ToString(); } },                                                                        
                                                                    };

        public HttpWebRequestWrapper(HttpWebRequest request)
        {
            _request = request;
        }

        public string UserAgent
        {
            get
            {
                return _request.UserAgent;
            }
            set
            {
                _request.UserAgent = value;
            }
        }

        public ICredentials Credentials
        {
            get
            {
                return _request.Credentials;
            }
            set
            {
                _request.Credentials = value;
            }
        }

        public CookieContainer CookieContainer
        {
            get
            {
                return _request.CookieContainer;
            }
            set
            {
                _request.CookieContainer = value;
            }
        }

        public IDictionary<string, string> Headers
        {
            // Add logic to check the type of the header and see if it needs to set separately
            get
            {
                if (_headerDictionary.Count == 0)
                {
                    PopulateHeaderDict();
                }
                return _headerDictionary;
            }
            set
            {
                SetRequestHeader(value);
            }
        }

        private void PopulateHeaderDict()
        {
            foreach (KeyValuePair<string, string> entry in _request.Headers)
            {
                _headerDictionary.Add(entry.Key, entry.Value);
            }

            foreach (KeyValuePair<string, Func<HttpWebRequest, string>> entry in _restrictedHeaderGet)
            {
                _headerDictionary.Add(entry.Key, entry.Value(_request));
            }
        }

        private void SetRequestHeader(IDictionary<string, string> dictHeader)
        {
            foreach (KeyValuePair<string, string> headerEntry in dictHeader)
            {
                if (!_restrictedHeaderSet.Keys.Contains(headerEntry.Key))
                {
                    _request.Headers.Add(headerEntry.Key, headerEntry.Value);
                }
                else
                {
                    Action<HttpWebRequest, string> action;
                    _restrictedHeaderSet.TryGetValue(headerEntry.Key, out action);
                    if (action != null)
                    {
                        action.Invoke(_request, headerEntry.Value);
                    }
                }
            }
        }

        //public X509CertificateCollection ClientCertificates
        //{
        //    get
        //    {
        //        return _request.ClientCertificates;
        //    }
        //    set
        //    {
        //        _request.ClientCertificates = value;
        //    }
        //}

        public string Accept
        {
            get
            {
                return _request.Accept;
            }
            set
            {
                _request.Accept = value;
            }
        }

#if !SILVERLIGHT
        public IWebProxy Proxy
        {
            get
            {
                return _request.Proxy;
            }
            set
            {
                _request.Proxy = value;
            }
        }
#endif

        public void Abort()
        {
            _request.Abort();
        }
    }
}
