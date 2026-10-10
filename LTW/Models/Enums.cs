namespace LTW.Models
{
    public enum UserRole
    {
        Admin = 1,
        Teacher = 2,
        Student = 3
    }

    public enum Skill
    {
        Listening = 1,
        Reading = 2,
        Speaking = 3,
        Writing = 4
    }

    public enum SkillType
    {
        Listening = 1,
        Reading = 2,
        Writing = 3,
        Speaking = 4,
        Grammar = 5,
        Vocabulary = 6
    }

    public enum QuestionFormat
    {
        MultipleChoice = 1,
        TrueFalse = 2,
        FillBlank = 3,
        Matching = 4,
        Writing = 5,
        Speaking = 6
    }

    public enum ClassStatus
    {
        Upcoming = 1,
        Open = 2,
        Ongoing = 3,
        Closed = 4
    }

    public enum ExamType
    {
        ExampleTest = 1,
        PlacementTest = 2
    }
}
