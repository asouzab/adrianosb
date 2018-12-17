using System.Collections.Generic;

namespace Mosaic.MOL.API.Model
{
    public class Result
    {
        public bool Success { get; set; }
        public List<string> Messages { get; set; }
        public object Payload { get; set; }

        public Result()
        {
            Success = false;
            Messages = new List<string>();
        }

        public Result(bool success)
        {
            Success = success;
            Messages = new List<string>();
        }

        public Result(bool success, string message)
        {
            Success = success;
            Messages = new List<string>();
            Messages.Add(message);
        }

        public Result(bool success, string message, object payload)
        {
            Success = success;
            Messages = new List<string>();
            Messages.Add(message);
            Payload = payload;
        }
    }
}
