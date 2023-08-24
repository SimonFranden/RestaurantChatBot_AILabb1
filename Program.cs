using Azure;
using Azure.AI.Language.QuestionAnswering;
using Azure.AI.Translation.Text;
namespace RestaurantChatBot
{

    class Program
    {
        //Endpoint and key for QnA bot
        private static string QAEndpoint = "https://labb1languageservice.cognitiveservices.azure.com/";
        private static string QAKey = "b9e3b9aa3b004db49cca966907709d8e";

        //Endpoint and key for Cognitive service
        private static string CognitiveEndpoint = "https://labb1cognitiveservice.cognitiveservices.azure.com/";
        private static string CognitiveKey = "b40a3960d4564de284b127527f23e266";

        private static string userLang = "en";
        static async Task Main(string[] args)
        {
            //Initialization
            Uri qaEndpoint = new Uri(QAEndpoint);
            AzureKeyCredential qaCredential = new AzureKeyCredential(QAKey);
            string projectName = "Labb1FAQ";
            string deploymentName = "production"; 
            
            QuestionAnsweringClient QAclient = new QuestionAnsweringClient(qaEndpoint, qaCredential);
            QuestionAnsweringProject project = new QuestionAnsweringProject(projectName, deploymentName);

            Uri cognitiveEndpoint = new (CognitiveEndpoint);
            AzureKeyCredential cognitiveCredential = new AzureKeyCredential(CognitiveKey);
            TextTranslationClient cognitiveClient = new(cognitiveCredential, cognitiveEndpoint);

            //UI
            Console.WriteLine("Welcome to Ristorante Al Dente. How can i help you?");
            string question = "";
            while (question.ToLower() != "quit")
            {
                Console.Write("You: ");
                question = await TranslateTextToEnglish(cognitiveClient, Console.ReadLine());//Input to chat bot

                Response<AnswersResult> response = QAclient.GetAnswers(question, project);
                foreach (KnowledgeBaseAnswer answer in response.Value.Answers)
                {
                    Console.WriteLine($"Bot: {await TranslateTextToUserLang(cognitiveClient, answer.Answer)}");//Output from chat bot
                }
            }           
        }

        //Translate input to english so the bot understands
        static async Task<string> TranslateTextToEnglish(TextTranslationClient client,string textToTranslate)
        {         
            try
            {
                string targetLanguage = "en";
                
                Response<IReadOnlyList<TranslatedTextItem>> response = await client.TranslateAsync(targetLanguage, textToTranslate).ConfigureAwait(false);
                IReadOnlyList<TranslatedTextItem> translations = response.Value;
                TranslatedTextItem translation = translations.FirstOrDefault();

                userLang = translation?.DetectedLanguage?.Language;//Change the userLang to the language the user used.
                return translation?.Translations?.FirstOrDefault()?.Text;
            }
            catch (RequestFailedException exception)
            {
                Console.WriteLine($"Error Code: {exception.ErrorCode}");
                Console.WriteLine($"Message: {exception.Message}");
                return "";
            }
        }
        
        //Translate answer to the user language so the user understand
        static async Task<string> TranslateTextToUserLang(TextTranslationClient client, string textToTranslate)
        {
            try
            {
                string targetLanguage = userLang;

                Response<IReadOnlyList<TranslatedTextItem>> response = await client.TranslateAsync(targetLanguage, textToTranslate).ConfigureAwait(false);
                IReadOnlyList<TranslatedTextItem> translations = response.Value;
                TranslatedTextItem translation = translations.FirstOrDefault();

                return translation?.Translations?.FirstOrDefault()?.Text;
            }
            catch (RequestFailedException exception)
            {
                Console.WriteLine($"Error Code: {exception.ErrorCode}");
                Console.WriteLine($"Message: {exception.Message}");
                return "";
            }
        }
    }
}