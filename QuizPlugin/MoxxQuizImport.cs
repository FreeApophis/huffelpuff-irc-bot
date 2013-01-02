using System.IO;
using Plugin.Database.Quiz;
using Huffelpuff.Utils;
namespace Plugin
{
    class MoxxQuizImport : IQuizImport
    {
        public void ImportFile(FileInfo file, Main db)
        {
            using (StreamReader sr = file.OpenText())
            {
                string s;
                QuizQuestion question = null;
                bool category = false;
                while ((s = sr.ReadLine().Trim()) != null)
                {
                    if (s.StartsWith("#") || s.IsNullOrEmpty()) { continue; }

                    if (s.StartsWith("Category:"))
                    {
                        question = new QuizQuestion();
                        db.QuizQuestions.InsertOnSubmit(question);

                        question.Category = s.Substring(9);
                        category = true;
                    }

                    if (s.StartsWith("Question:") && question != null)
                    {
                        if (!category)
                        {
                            question = new QuizQuestion();
                            db.QuizQuestions.InsertOnSubmit(question);
                        }
                        category = false;
                        question.Question = s.Substring(9).Trim();
                    }

                    if (s.StartsWith("Answer:") && question != null)
                    {
                        question.Answer = s.Substring(7).Trim();
                        db.SubmitChanges();
                    }

                    if (s.StartsWith("Regexp:") && question != null)
                    {
                        question.AnswerRegExp = s.Substring(7).Trim();
                    }

                    if (s.StartsWith("Author:") && question != null)
                    {
                        question.Author = s.Substring(7).Trim();
                    }

                    if (s.StartsWith("Level:") && question != null)
                    {
                        switch (s.Substring(6).Trim())
                        {
                            case "baby":
                                question.Difficulty = 0;
                                break;
                            case "easy":
                                question.Difficulty = 3;
                                break;
                            case "normal":
                                question.Difficulty = 5;
                                break;
                            case "hard":
                                question.Difficulty = 8;
                                break;
                            case "extreme":
                                question.Difficulty = 10;
                                break;

                        }
                    }

                    if (s.StartsWith("Comment:") && question != null)
                    {
                        question.Comment = s.Substring(8).Trim();
                    }

                    if (s.StartsWith("Score:") && question != null)
                    {
                        question.Score = int.Parse(s.Substring(6).Trim());
                    }

                    if (s.StartsWith("Tip:") && question != null && question.ID.HasValue && !s.IsNullOrEmpty())
                    {
                        var hint = new Hint();
                        db.Hints.InsertOnSubmit(hint);

                        hint.Message = s.Substring(4).Trim();
                        hint.QuizQuestionID = question.ID.Value;

                        db.SubmitChanges();
                    }
                }
            }
        }
    }
}