namespace VotingSystem.Helpers
{
    public static class EmailBodyBuilder
    {
        public static string GenrateEmailBody(string template, Dictionary<string, string> TemplateModel)
        {
            var TemplatePath = $"{Directory.GetCurrentDirectory()}/Templates/{template}.html";
            var StreamReader = new StreamReader(TemplatePath);
            var body = StreamReader.ReadToEnd();

            StreamReader.Close();
            foreach (var item in TemplateModel) { 
            body= body.Replace(item.Key , item.Value );
            }
            return body;
        }
    }
}