using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Net;
using System.Text;
using Newtonsoft.Json;
using BJ_Task.Src.DataStructures;
using System.IO;
using Newtonsoft.Json.Linq;

namespace BJ_Task.Src
{
    //The class is declared static because there is no need to spawn instances in this implementation.
    static class ApiLayer
    {
        const string baseUrl = "https://uxcandy.com/~shapoval/test-task-backend/v2/";
        const string developerName = "DmitryAB";

        public static TasksQueryResult GetTasks(int page = 1, string sortField = "", bool sortDesc = false)
        {
            string responce = string.Empty;
            string request = new string(baseUrl);

            request += string.Format("?developer={0}", developerName);
            request += string.Format("&page={0}", page);
            if (!string.IsNullOrWhiteSpace(sortField))
            {
                request += string.Format("&sort_field={0}", sortField);
                request += string.Format("&sort_direction={0}", (sortDesc ? "desc" : "asc"));
            }

            var req = WebRequest.CreateHttp(request);
            req.Method = "GET";
            req.ContentType = "multipart/form-data";

            TasksQueryResult result = new TasksQueryResult();
            try
            {
                using (var resp = req.GetResponse() as HttpWebResponse)
                {
                    if (resp == null || resp.StatusCode != HttpStatusCode.OK)
                    {
                        result.result = "Unable to request data";
                        return result;
                    }
                    using (var rs = resp.GetResponseStream())
                    {
                        using (StreamReader sr = new StreamReader(rs))
                        {
                            responce = sr.ReadToEnd();
                        }
                    }
                }
            }
            catch (WebException)
            {
                result.result = "API Request error";
                return result;
            }
            // Parse the response, get the status, and, depending on it, parse the response or return the reason.
            try
            {

                var obj = JObject.Parse(responce);
                if (obj.SelectToken("status") == null || ((string)obj.SelectToken("status")) == "error")
                {
                    result.result = string.IsNullOrWhiteSpace((string)obj.SelectToken("message")) ? "UNKNOWN ERROR" : (string)obj.SelectToken("message");
                    return result; // If failure, return the reason
                }

                result.result = (string)obj.SelectToken("status");
                var message = obj.SelectToken("message");
                result.total_task_count = (int)message.SelectToken("total_task_count");
                var tasks = message.SelectToken("tasks");

                // Parse tasks into objects
                result.tasks = new List<TaskData>();
                foreach (var t in tasks)
                {
                    result.tasks.Add(t.ToObject<TaskData>());
                }
            }
            catch (JsonException)
            {
                result.result = "JSON Parsing error";
                return result;
            }
            return result;
        }

        /// <summary>
        /// User authorization function
        /// </summary>
        /// <param name = "username"> Username </param>
        /// <param name = "password"> Paro </param>
        /// <param name = "token"> Authorization token with "ok" status </param>
        /// <returns> ok or error description </returns>
        public static string LogIn(string username, string password, out string token)
        {
            string request = new string(baseUrl + "login");
            request += string.Format("?developer={0}", developerName);

            string responce = string.Empty;
            token = string.Empty;
            // For POST requests, the WebClient class is used
            try
            {
                using (var cli = new WebClient())
                {
                    var POST = new NameValueCollection();
                    POST["username"] = username;
                    POST["password"] = password;
                    var resp = cli.UploadValues(request, POST);
                    responce = Encoding.Default.GetString(resp);
                }
            }
            catch (WebException)
            {
                return "Request failed";
            }

            var obj = JObject.Parse(responce);
            if (obj.SelectToken("status") == null)
                return "Unknown error";

            if (((string)obj.SelectToken("status")) == "error")
            {
                string err = string.Empty;
                var message = obj.SelectToken("message");
                if (message == null)
                    return "No message data!";
                else if (message.SelectToken("username") != null)
                    return "User name: " + (string)message.SelectToken("username");
                else if (message.SelectToken("password") != null)
                    return "Password: " + (string)message.SelectToken("password");
            }

            var msg = obj.SelectToken("message");
            if (msg == null)
                return "JSON Error";
            token = (string)msg.SelectToken("token");

            return "ok";
        }

        public static TaskData CreateTask(string username, string email, string text)
        {
            string request = new string(baseUrl + "create");
            request += string.Format("?developer={0}", developerName);

            TaskData ret = new TaskData();
            string responce = string.Empty;
            try
            {
                using (var cli = new WebClient())
                {
                    var POST = new NameValueCollection();
                    POST["username"] = username;
                    POST["email"] = email;
                    POST["text"] = text;
                    var resp = cli.UploadValues(request, POST);
                    responce = Encoding.Default.GetString(resp);
                }
            }
            catch (WebException)
            {
                return null;
            }

            var obj = JObject.Parse(responce);
            if (obj.SelectToken("status") == null)
                return null;
            var message = obj.SelectToken("message");
            if (message == null)
                return null;

            if (((string)obj.SelectToken("status")) == "error")
            {
                ret.Id = -1;
            }
            else
            {
                if (message.SelectToken("id") != null)
                    ret.Id = (int)message.SelectToken("id");
            }

            if (message.SelectToken("username") != null)
                ret.Username = (string)message.SelectToken("username");
            if (message.SelectToken("email") != null)
                ret.Email = (string)message.SelectToken("email");
            if (message.SelectToken("text") != null)
                ret.Text = (string)message.SelectToken("text");

            return ret;
        }

        public static string EditTask(string token, int taskId, string text, int status)
        {
            string request = new string(baseUrl + "edit/" + taskId.ToString());
            request += string.Format("?developer={0}", developerName);

            TaskData ret = new TaskData();
            string responce = string.Empty;
            try
            {
                using (var cli = new WebClient())
                {
                    var POST = new NameValueCollection();
                    POST["token"] = token;
                    POST["text"] = text;
                    POST["status"] = status.ToString();
                    var resp = cli.UploadValues(request, POST);
                    responce = Encoding.Default.GetString(resp);
                }
            }
            catch (WebException)
            {
                return "Request error";
            }

            var obj = JObject.Parse(responce);
            if (obj.SelectToken("status") == null)
                return "Wrong responce";

            if (((string)obj.SelectToken("status")) == "ok")
                return "ok";

            var message = obj.SelectToken("message");
            if (message == null)
                return "Unknown error";

            if (message.SelectToken("token") == null)
                return "Unknown error";

            return (string)message.SelectToken("token");
        }
    }
}