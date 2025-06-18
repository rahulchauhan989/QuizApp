namespace quiz.Domain.Dto
{
    public class CorrectAnswerDto
    {
        public int QuestionId { get; set; }
        public int CorrectOptionId { get; set; }
        public int Marks { get; set; } 
    }
}