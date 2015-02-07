using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace GeekQuiz.Models
{
    public class TriviaStatistic
    {
        public int Id { get; set; }

        public string UserId { get; set; }

        public DateTime Date { get; set; }

        public int Passed { get; set; }
        public int Failed { get; set; }

        [JsonIgnore]
        public int Count
        {
            get { return Passed + Failed; }
        }
    }
}