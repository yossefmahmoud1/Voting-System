namespace SurveyBasket.Dtos.Auth
{
    public record UsernameTakenResponse(
        string Message,           // الرسالة اللي تقول إن الاسم مستخدم
        List<string> Suggestions  // قائمة بأسماء مقترحة متاحة
    );
}
