using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;

namespace BJ_Task.Src.DataStructures
{
    class TaskData
    {
        //According to the naming conventions in C #, property names must begin with an uppercase letter. For compatibility with json, attributes with field names are declared here.
        [JsonProperty(PropertyName = "id")]
        public int Id { get; set; }
        [JsonProperty(PropertyName = "username")]
        public string Username { get; set; }
        [JsonProperty(PropertyName = "email")]
        public string Email { get; set; }
        [JsonProperty(PropertyName = "text")]
        public string Text { get; set; }
        [JsonProperty(PropertyName = "status")]
        public int Status { get; set; }

        public TaskData()
        {
            Id = -1; //Initialization with id -1 to keep track of new TaskData instances
        }
    }
}