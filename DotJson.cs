﻿/******************************
 * .JSON - Classes for dealing with dynamic JSON
 * http://github.com/kamranayub/.JSON/
 * 
 * Copyright 2011, Kamran Ayub. 
 * Dual licensed under the MIT or GPL Version 2 licenses (just like jQuery)
 * 
 ******************************/
namespace DotJson
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Specialized;
    using System.Dynamic;
    using System.Linq;
    using System.Net;
    using System.Reflection;
    using System.Text;
    using System.Web.Script.Serialization;

    /// <summary>
    /// Wraps JSON services within a class that return dynamic JSON dictionaries for
    /// easy access.
    /// </summary>
    public class JsonService
    {

        /// <summary>
        /// Instantiate dynamic JSON service with API url (root or method url). Private; use .Url(url) to start the fluent interface.
        /// </summary>
        /// <param name="baseUrlOrMethodUrl">The root API url or a method URL</param>
        private JsonService(string baseUrlOrMethodUrl)
        {
            this.BaseUri = new Uri(baseUrlOrMethodUrl);
            this.Encoding = UTF8Encoding.UTF8;
        }

        #region Private Props

        /// <summary>
        /// The base URI or method URI
        /// </summary>
        private Uri BaseUri { get; set; }

        /// <summary>
        /// Gets or sets the credentials used for the request when challenged.
        /// </summary>
        private ICredentials Credentials { get; set; }

        /// <summary>
        /// Gets or sets whether to forcefully send the Authorization HTTP header on the first request.
        /// </summary>
        private bool ForceSendAuthorization { get; set; }

        /// <summary>
        /// The Encoding to use when downloading/uploading data.
        /// </summary>
        private Encoding Encoding { get; set; }

        #endregion

        #region Static Methods

        /// <summary>
        /// Instantiate a new fluent interface for JsonService
        /// </summary>
        /// <param name="url">The root API or method URL to post/get to</param>
        /// <returns>JsonService</returns>
        public static JsonService For(string url)
        {
            return new JsonService(url);
        }

        /// <summary>
        /// Shortcut for new JsonService(baseUri).GET(relative)
        /// </summary>
        /// <param name="url">Full API URL to the GET metod</param>
        /// <returns></returns>
        public static dynamic GetFrom(string url)
        {
            return JsonService.For(url).Get();
        }

        /// <summary>
        /// Shortcut for new JsonService(baseUri).GET(relative)
        /// </summary>
        /// <param name="url">Full API URL to the GET metod</param>
        /// <returns></returns>
        public static dynamic GetFrom(string url, object options)
        {
            return JsonService.For(url).Get(options);
        }

        /// <summary>
        /// Shortcut for new JsonService(baseUri).GET(relative)
        /// </summary>
        /// <param name="url">Full API URL to the GET metod</param>
        /// <returns></returns>
        public static dynamic GetFrom(string url, IDictionary<string, string> options)
        {
            return JsonService.For(url).Get(options);
        }

        /// <summary>
        /// Shortcut for new JsonService(baseUri).POST(relative, params)
        /// </summary>
        /// <param name="url">Full API URL to the GET metod</param>
        /// <param name="dataParams">Key/value pair query parameters (e.g. ?key=value)</param>
        /// <returns></returns>
        public static dynamic PostTo(string url, object dataParams)
        {
            return JsonService.For(url).Post(dataParams);
        }

        /// <summary>
        /// Shortcut for new JsonService(baseUri).POST(relative, params)
        /// </summary>
        /// <param name="url">Full API URL to the POST metod</param>
        /// <param name="dataParams">String dictionary of Key/value pair query parameters (e.g. ?key=value)</param>
        /// <returns></returns>
        public static dynamic PostTo(string url, IDictionary<string, string> dataParams)
        {
            return JsonService.For(url).Post(dataParams);
        }

        #endregion

        #region Fluent Methods

        /// <summary>
        /// Set to use a specific encoding for requests
        /// </summary>
        /// <param name="encoding"></param>
        /// <returns></returns>
        public JsonService UseEncoding(Encoding encoding)
        {
            this.Encoding = encoding;

            return this;
        }

        /// <summary>
        /// Force sending Authorization HTTP header on the request
        /// </summary>
        /// <returns></returns>
        public JsonService ForceAuthorization()
        {
            this.ForceSendAuthorization = true;

            return this;
        }

        /// <summary>
        /// Shortcut for adding basic auth credential
        /// </summary>
        /// <param name="userName">Your username</param>
        /// <param name="password">Your password</param>
        /// <param name="force">Forcefully send authorization header on first request</param>
        public JsonService AuthenticateAsBasic(string userName, string password, bool force = false)
        {
            return this.AuthenticateAsBasic(new NetworkCredential(userName, password), force);
        }

        /// <summary>
        /// Shortcut for adding basic auth credential
        /// </summary>
        /// <param name="credential">A NetworkCredential for your Basic auth</param>
        /// <param name="force">Forcefully send authorization header on first request</param>
        public JsonService AuthenticateAsBasic(NetworkCredential credential, bool force = false)
        {
            return this.AuthenticateAs(new CredentialCache() { { BaseUri, "Basic", credential } }, force);
        }

        /// <summary>
        /// Authenticate using an ICredentials (Windows, Digest, NTLM, etc.)
        /// </summary>
        /// <param name="credentials"></param>
        /// <param name="force"></param>
        /// <returns></returns>
        public JsonService AuthenticateAs(ICredentials credentials, bool force = false)
        {
            this.Credentials = credentials;
            this.ForceSendAuthorization = force;

            return this;
        }

        #endregion

        #region Public Get/Post Methods

        /// <summary>
        /// GETs from API URL and returns a dynamically typed object representing the
        /// response from the service.
        /// </summary>
        /// <param name="pageMethod">Page, method, or file to append to end of API URL. Leave blank to use base url.</param>
        /// <returns></returns>
        public dynamic Get(string pageMethod = "")
        {
            return Get(pageMethod, null);
        }

        /// <summary>
        /// GETs base URL and returns a dynamically typed object representing the
        /// response from the service.
        /// </summary>
        /// <param name="queryParams">Anonymous object representing key/value pairs for querystring</param>
        /// <returns>Json</returns>
        public dynamic Get(object queryParams)
        {
            return Get(string.Empty, queryParams);
        }

        /// <summary>
        /// GETs base URL and returns a dynamically typed object representing the
        /// response from the service.
        /// </summary>
        /// <param name="queryParams">Dictionary representing key/value pairs for querystring</param>
        /// <returns>Json</returns>
        public dynamic Get(IDictionary<string, string> queryParams)
        {
            return Get(string.Empty, queryParams);
        }

        /// <summary>
        /// GETs from API URL and returns a dynamically typed object representing the
        /// response from the service.
        /// </summary>
        /// <param name="pageMethod">Page, method, or file to append to end of API URL</param>
        /// <param name="queryParams">Key/value pair query parameters (e.g. ?key=value)</param>
        /// <returns></returns>
        public dynamic Get(string pageMethod, object queryParams)
        {
            return PerformGET(pageMethod, queryParams.ToNameValueCollection());
        }

        /// <summary>
        /// GETs from API URL and returns a dynamically typed object representing the
        /// response from the service.
        /// </summary>
        /// <param name="pageMethod">Page, method, or file to append to end of API URL</param>
        /// <param name="queryParams">String dictionary of Key/value pair query parameters (e.g. ?key=value)</param>
        /// <returns></returns>
        public dynamic Get(string pageMethod, IDictionary<string, string> queryParams)
        {
            return PerformGET(pageMethod, queryParams.ToNameValueCollection());
        }

        /// <summary>
        /// POSTs to the base URL and returns a dynamically typed object representing the
        /// response from the service.
        /// </summary>
        /// <param name="formData">Key/value pair query parameters (e.g. ?key=value)</param>
        /// <returns></returns>
        public dynamic Post(object formData)
        {
            return PerformPOST(String.Empty, formData.ToNameValueCollection());

        }

        /// <summary>
        /// POSTs to the base URL and returns a dynamically typed object representing the
        /// response from the service.
        /// </summary>
        /// <param name="formData">Key/value pair dictionary for query parameters (e.g. ?key=value)</param>
        /// <returns></returns>
        public dynamic Post(IDictionary<string, string> formData)
        {
            return PerformPOST(String.Empty, formData.ToNameValueCollection());

        }

        /// <summary>
        /// POSTs to an API URL and returns a dynamically typed object representing the
        /// response from the service.
        /// </summary>
        /// <param name="pageMethod">Page, method, or file to append to end of API URL</param>
        /// <param name="formData">Key/value pair query parameters (e.g. ?key=value)</param>
        /// <returns></returns>
        public dynamic Post(string pageMethod, object formData)
        {
            return PerformPOST(pageMethod, formData.ToNameValueCollection());
        }

        /// <summary>
        /// POSTs to an API URL and returns a dynamically typed object representing the
        /// response from the service.
        /// </summary>
        /// <param name="pageMethod">Page, method, or file to append to end of API URL</param>
        /// <param name="formData">String dictionary of Key/value pair query parameters (e.g. ?key=value)</param>
        /// <returns></returns>
        public dynamic Post(string pageMethod, IDictionary<string, string> formData)
        {
            return PerformPOST(pageMethod, formData.ToNameValueCollection());
        }

        #endregion

        #region Private Helpers

        private dynamic PerformGET(string pageMethod, NameValueCollection queryData)
        {
            return Json.Parse(PerformRequest(pageMethod, HttpMethod.GET, queryData));
        }

        private dynamic PerformPOST(string pageMethod, NameValueCollection formData)
        {
            if (formData != null)
            {
                return Json.Parse(PerformRequest(pageMethod, HttpMethod.POST, formData));
            }
            else
                throw new ArgumentNullException("formData", "For POST, formData cannot be null.");
        }

        /// <summary>
        /// Wraps WebClient request in single method.
        /// </summary>
        /// <param name="pageMethod"></param>
        /// <param name="method"></param>
        /// <param name="requestData"></param>
        /// <returns></returns>
        private string PerformRequest(string pageMethod, HttpMethod method, NameValueCollection requestData)
        {
            using (var client = new WebClient())
            {
                if (this.Credentials == null)
                {
                    client.UseDefaultCredentials = true;
                }
                else
                {
                    // Force sending credentials on first request
                    if (this.ForceSendAuthorization)
                    {
                        var basicAuth = this.Credentials.GetCredential(this.BaseUri, "Basic");

                        if (basicAuth != null)
                        {
                            var cre = string.Format("{0}:{1}", basicAuth.UserName, basicAuth.Password);
                            var base64 = Convert.ToBase64String(Encoding.GetBytes(cre));

                            client.Headers.Add("Authorization", "Basic " + base64);
                        }
                    }

                    client.Credentials = this.Credentials;
                }

                // Send ACCEPT to accept JSON by default
                client.Headers.Add(HttpRequestHeader.Accept, "application/json");

                if (method == HttpMethod.POST)
                {
                    client.Headers.Add("Content-type", "application/x-www-form-urlencoded");

                    return Encoding.GetString(client.UploadValues(pageMethod.ToRelativeUri(BaseUri), requestData));
                }
                else
                {
                    client.QueryString = requestData;
                    return client.DownloadString(pageMethod.ToRelativeUri(BaseUri));
                }

            }
        }

        private enum HttpMethod
        {
            GET,
            POST
        }

        #endregion
    }

    /// <summary>
    /// Represents a dynamic JSON object and dictionary. It is a read-only dictionary and if
    /// a JSON property cannot be translated to a CLR property (x.prop), you can access it
    /// via the dictionary[key] syntax or compact form (e.g. "foo-bar" => foobar)
    /// </summary>
    public class Json : DynamicObject
    {
        // It's case-sensitive, baby!
        private readonly IDictionary<string, dynamic> DynamicDictionary = 
            new Dictionary<string, dynamic>(StringComparer.Ordinal);

        // Key: Compact key, Value: original value
        private readonly IDictionary<string, string> KeyDictionary = 
            new Dictionary<string, string>(StringComparer.Ordinal);

        // Store original deserialized object for serialization
        private readonly object OriginalObject;

        /// <summary>
        ///  Support dictionary key getting
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public object this[string key]
        {
            get
            {
                return this.DynamicDictionary[key];
            }
        }

        #region Static Methods

        /// <summary>
        /// Creates a new Dynamic JSON dictionary with given (assumes valid) JSON string.
        /// </summary>
        /// <param name="json"></param>
        public static dynamic Parse(string json)
        {
            return new Json(json);
        }

        /// <summary>
        /// Creates a new Dynamic JSON dictionary from given anonymous object/
        /// </summary>
        /// <param name="anonObject"></param>
        /// <returns></returns>
        public static dynamic Parse(object anonObject)
        {
            return new Json(anonObject);
        }

        /// <summary>
        /// Converts an object to a JSON string.
        /// </summary>
        /// <param name="anonObject"></param>
        /// <returns></returns>
        public static string Stringify(object anonObject)
        {
            return new JavaScriptSerializer().Serialize(anonObject);
        }

        #endregion

        #region Constructors

        private Json(string json)
        {
            this.OriginalObject = new JavaScriptSerializer().DeserializeObject(json);

            TranslateDictionary(new JavaScriptSerializer().Deserialize<IDictionary<string, dynamic>>(json));
        }

        private Json(object anonObject)
            : this(new JavaScriptSerializer().Serialize(anonObject))
        {
            this.OriginalObject = anonObject;
        }

        /// <summary>
        /// This constructor gets called for sub-properties in the Dynamic Dictionary, recursively.
        /// </summary>
        /// <param name="dictionary"></param>
        private Json(IDictionary<string, dynamic> dictionary)
        {
            this.OriginalObject = dictionary;

            TranslateDictionary(dictionary);
        }

        #endregion

        /// <summary>
        /// Returns stringified object
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            if (this.OriginalObject != null)
                return new JavaScriptSerializer().Serialize(this.OriginalObject);
            else
                return base.ToString();
        }

        #region Private Helpers

        /// <summary>
        /// Translates a JSON string, object dictionary into a dictionary that wraps
        /// inner JSON objects with the Json object as well. This makes it
        /// easy for callers to access JSON via CLR property syntax or dictionary syntax.
        /// </summary>
        /// <param name="dictionary"></param>
        private void TranslateDictionary(IDictionary<string, object> dictionary)
        {
            foreach (var kv in dictionary)
            {
                object value = kv.Value;

                if (kv.Value is IDictionary<string, object>)
                {
                    value = new Json(kv.Value as IDictionary<string, dynamic>);
                }
                else if (kv.Value is Array)
                {
                    var objects = kv.Value as object[];

                    if (objects.Any())
                    {
                        value = objects
                            .Select(o => o is IDictionary<string, dynamic> ?
                                new Json(o as IDictionary<string, dynamic>) : o).ToArray();
                    }
                }

                DynamicDictionary.Add(kv.Key, value.TryConvert());

                // Only add if existing case-sensitive key does not exist.
                if (!KeyDictionary.ContainsKey(kv.Key.ToCLSId()))
                    KeyDictionary.Add(kv.Key.ToCLSId(), kv.Key);
            }
        }

        /// <summary>
        /// Tries to look for a dictionary key by the typed property name (e.g. x.property1).
        /// </summary>
        /// <param name="jsonKeyPath"></param>
        /// <returns></returns>
        private dynamic GetPropertyValue(string jsonKeyPath)
        {
            // Get the key to use
            // If no key exists, try to get the compact version
            var key = (!this.DynamicDictionary.ContainsKey(jsonKeyPath) &&
                this.KeyDictionary.ContainsKey(jsonKeyPath)) ?
                this.KeyDictionary[jsonKeyPath] : jsonKeyPath;

            if (this.DynamicDictionary.ContainsKey(key) || key != null)
                return this.DynamicDictionary[key];

            return null;
        }

        #endregion

        #region DynamicObject Overrides

        /// <summary>
        /// Get result of dynamic member
        /// </summary>
        /// <param name="binder"></param>
        /// <param name="result"></param>
        /// <returns></returns>
        public override bool TryGetMember(GetMemberBinder binder, out object result)
        {
            result = GetPropertyValue(binder.Name);
            return result != null;
        }

        /// <summary>
        /// Forward non-existant method invocations to dictionary
        /// </summary>
        /// <param name="binder"></param>
        /// <param name="args"></param>
        /// <param name="result"></param>
        /// <returns></returns>
        public override bool TryInvokeMember(InvokeMemberBinder binder, object[] args, out object result)
        {
            result = DynamicDictionary.GetType().InvokeMember(binder.Name, BindingFlags.InvokeMethod, null, this.DynamicDictionary, args);

            return result != null;
        }

        #endregion
    }


    /// <summary>
    /// A set of extensions to help
    /// </summary>
    public static class DotJsonExtensions
    {

        /// <summary>
        /// Fixes up inconsistencies with how new Uri treats generating a URL. 
        /// Notably, removing/adding trailing or leading slashes.
        /// </summary>
        /// <param name="url"></param>
        /// <param name="baseUri"></param>
        /// <returns></returns>
        public static Uri ToRelativeUri(this string url, Uri baseUri)
        {
            var endSlashRx = new System.Text.RegularExpressions.Regex("/+$");
            var beginSlashRx = new System.Text.RegularExpressions.Regex("^/+");

            // If no leading slash on baseUri, add it (ignore if its absolute)
            if (!endSlashRx.IsMatch(baseUri.ToString()) && !baseUri.IsAbsoluteUri)
                baseUri = new Uri(baseUri + "/");

            // Remove leading slash on relative, if any
            if (beginSlashRx.IsMatch(url))
                url = beginSlashRx.Replace(url, String.Empty);

            return new Uri(baseUri, Uri.EscapeDataString(url));
        }

        /// <summary>
        /// Gets an anonymous object's properties as a NameValueCollection.
        /// </summary>
        /// <param name="thing"></param>
        /// <returns></returns>
        public static NameValueCollection ToNameValueCollection(this object thing)
        {
            if (thing == null)
                return null;

            var nvCollection = new NameValueCollection();

            foreach (var p in thing.GetType().GetProperties())
            {
                var value = p.GetValue(thing, null);

                nvCollection.Add(p.Name, value.ToString());
            }

            return nvCollection;
        }

        /// <summary>
        /// Converts a string dictionary to a NameValueCollection.
        /// </summary>
        /// <param name="dict"></param>
        /// <returns></returns>
        public static NameValueCollection ToNameValueCollection(this IDictionary<string, string> dict)
        {
            if (dict == null)
                return null;

            var nvCollection = new NameValueCollection();

            foreach (var kv in dict)
            {
                nvCollection.Add(kv.Key, kv.Value);
            }

            return nvCollection;
        }

        /// <summary>
        /// Converts a string to a CLS-compliant ID.
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static string ToCLSId(this string input)
        {
            return new System.Text.RegularExpressions.Regex(@"[^\p{Ll}\p{Lu}\p{Lt}\p{Lo}\p{Nd}\p{Nl}\p{Mn}\p{Mc}\p{Cf}\p{Pc}\p{Lm}]")
                .Replace(input, string.Empty);
        }

        /// <summary>
        /// Try to guess type and cast as that for some JSON values.
        /// </summary>
        /// <param name="thing"></param>
        /// <returns></returns>
        public static dynamic TryConvert(this object thing)
        {
            DateTime theDate;
            Uri theUri;

            // Try converting dates
            if (thing is string && DateTime.TryParse(thing.ToString(), out theDate))
            {
                return theDate;
            }

            // Try converting absolute URLs
            if (thing is string && Uri.TryCreate(thing.ToString(), UriKind.Absolute, out theUri))
            {
                return theUri;
            }

            return thing;
        }
    }
}