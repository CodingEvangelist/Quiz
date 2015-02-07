using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace GeekQuiz.Models
{
    public class SaveResult
    {
        public bool Result { get; set; }

        public TriviaStatistic Statistic { get; set; }
    }
}