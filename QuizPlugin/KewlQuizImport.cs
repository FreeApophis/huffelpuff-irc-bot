using System.IO;
using Huffelpuff.Utils;
using Plugin.Database.Quiz;

namespace Plugin
{
    class KewlQuizImport : IQuizImport
    {
        public string Language { get; set; }
        public string Author { get; set; }

        public void ImportFile(FileInfo file, Main db)
        {
            using (StreamReader sr = file.OpenText())
            {
                string s;
                var count = 0;
                QuizQuestion question = null;
                while ((s = sr.ReadLine()) != null)
                {
                    s = s.Trim();
                    ++count;

                    // Skip Header
                    if (count < 4) { continue; }

                    if (count % 3 == 1)
                    {
                        question = new QuizQuestion();
                        db.QuizQuestions.InsertOnSubmit(question);

                        // Line 1: Question
                        question.Question = s;
                        if (Language != null)
                        {
                            question.Language = Language;
                        }
                        if (Author != null)
                        {
                            question.Author = Author;
                        }
                    }

                    if (count % 3 == 2 && question != null)
                    {
                        // Line 2: Answer (manual work needed)
                        question.Answer = s;
                        db.SubmitChanges();
                    }

                    if (count % 3 == 0 && question != null && question.ID.HasValue && !s.IsNullOrEmpty())
                    {
                        // Line 3: A Hint
                        var hint = new Hint();
                        db.Hints.InsertOnSubmit(hint);

                        hint.Message = s;
                        hint.QuizQuestionID = question.ID.Value;

                        db.SubmitChanges();
                    }
                }

                db.SubmitChanges();
            }
        }
    }
}
