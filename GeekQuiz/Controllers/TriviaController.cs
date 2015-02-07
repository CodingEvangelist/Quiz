using System.Data.Entity;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http.Description;
using GeekQuiz.Models;
using System.Web.Http;
using System.Linq;
using System;

namespace GeekQuiz.Controllers
{
    [Authorize]
    public class TriviaController : ApiController
    {
        private TriviaContext db = new TriviaContext();

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                this.db.Dispose();
            }

            base.Dispose(disposing);
        }

        private async Task<TriviaQuestion> NextQuestionAsync(string userId)
        {
            var lastQuestionId = await this.db.TriviaAnswers
                .Where(a => a.UserId == userId)
                .GroupBy(a => a.QuestionId)
                .Select(g => new { QuestionId = g.Key, Count = g.Count() })
                .OrderByDescending(q => new { q.Count, QuestionId = q.QuestionId })
                .Select(q => q.QuestionId)
                .FirstOrDefaultAsync();

            var questionsCount = await this.db.TriviaQuestions.CountAsync();

            var nextQuestionId = (lastQuestionId % questionsCount) + 1;
            return await this.db.TriviaQuestions.FindAsync(CancellationToken.None, nextQuestionId);
        }

        // GET api/Trivia
        [ResponseType(typeof(TriviaQuestion))]
        public async Task<IHttpActionResult> Get()
        {
            var userId = User.Identity.Name;

            TriviaQuestion nextQuestion = await this.NextQuestionAsync(userId);

            if (nextQuestion == null)
            {
                return this.NotFound();
            }

            return this.Ok(nextQuestion);
        }

        private async Task<SaveResult> StoreAsync(TriviaAnswer answer)
        {
            this.db.TriviaAnswers.Add(answer);

            var selectedOption = await this.db.TriviaOptions.FirstOrDefaultAsync(o => o.Id == answer.OptionId
                && o.QuestionId == answer.QuestionId);

            // Store data for statistics
            var userStatistics = await this.db.TriviaStatistics.FirstOrDefaultAsync(item => item.Date == DateTime.Today && item.UserId == this.User.Identity.Name);
            if (userStatistics == null)
            {
                userStatistics = new TriviaStatistic
                {
                    Date = DateTime.Today,
                    Failed = !selectedOption.IsCorrect ? 1: 0,
                    Passed = selectedOption.IsCorrect ? 1 : 0,
                    UserId = User.Identity.Name,
                };

                this.db.TriviaStatistics.Add(userStatistics);
            }
            else
            {
                db.TriviaStatistics.Attach(userStatistics);
                var entry = db.Entry(userStatistics);
                if (!selectedOption.IsCorrect)
                {
                    userStatistics.Failed++;
                    entry.Property(e => e.Failed).IsModified = true;
                }
                if (selectedOption.IsCorrect)
                {
                    userStatistics.Passed++;
                    entry.Property(e => e.Passed).IsModified = true;
                }
            }


            await this.db.SaveChangesAsync();
            return new SaveResult
            {
                Result = selectedOption.IsCorrect,
                Statistic = userStatistics
            };
        }

        // POST api/Trivia
        [ResponseType(typeof(TriviaAnswer))]
        public async Task<IHttpActionResult> Post(TriviaAnswer answer)
        {
            if (!ModelState.IsValid)
            {
                return this.BadRequest(this.ModelState);
            }

            answer.UserId = User.Identity.Name;

            //bool isCorrect = await this.StoreAsync(answer);
            SaveResult result = await this.StoreAsync(answer);
            return this.Ok<SaveResult>(result);
        }


    }
}
