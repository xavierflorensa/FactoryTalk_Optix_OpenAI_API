#region Using directives
using System;
using UAManagedCore;
using OpcUa = UAManagedCore.OpcUa;
using FTOptix.UI;
using FTOptix.HMIProject;
using FTOptix.NativeUI;
using FTOptix.Retentivity;
using FTOptix.CoreBase;
using FTOptix.Core;
using FTOptix.NetLogic;
using System.Net.Http;
using System.Threading.Tasks;
using System.Text;
using System.Text.Json;

#endregion

public class RuntimeNetLogic1 : BaseNetLogic
{
    public override async void Start()
    {
        // Insert code to be executed when the user-defined logic
       
    }

    public override void Stop()
    {
        // Insert code to be executed when the user-defined logic is stopped
    }

    [ExportMethod]
    
    public async void Translate()
    {
       var responseVar = Project.Current.GetVariable("Model/ResponseString");
       responseVar.Value = "Hello World!";
       var myTextbox = Project.Current.Get<TextBox>("UI/MainWindow/TextBox2");
            var apiKey = Environment.GetEnvironmentVariable("OPENAI_API_KEY"); // read from Windows env vars
            if (string.IsNullOrEmpty(apiKey))
            {
                Log.Error("OPENAI_API_KEY environment variable is not set. Aborting request.");
                responseVar.Value = "Error: OPENAI_API_KEY not set.";
                return;
            }
            var endpoint = "https://api.openai.com/v1/chat/completions";
        var client = new HttpClient();
        client.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiKey}");

        var request = new
        {
            model = "gpt-3.5-turbo",
            messages = new[]
            {
                //new { role = "user", content = "Translate the following English text to French: 'Hello, how are you?'" }
                new { role = "user", content = "Translate the following English text to "+myTextbox.Text+": 'Hello, how are you?'" }
            },
            max_tokens = 60
        };
        var content = new StringContent(JsonSerializer.Serialize(request), Encoding.UTF8, "application/json");

        var response = await client.PostAsync(endpoint, content);
        var responseText = await response.Content.ReadAsStringAsync();
        Log.Info("ResponseString: "+responseText);

        // Try to parse the JSON response and extract the assistant message content
        try
        {
            using (var doc = JsonDocument.Parse(responseText))
            {
                var root = doc.RootElement;
                if (root.TryGetProperty("choices", out var choices) && choices.ValueKind == JsonValueKind.Array && choices.GetArrayLength() > 0)
                {
                    var first = choices[0];
                    if (first.TryGetProperty("message", out var message) && message.ValueKind == JsonValueKind.Object && message.TryGetProperty("content", out var contentProp))
                    {
                        var assistantContent = contentProp.GetString();
                        responseVar.Value = assistantContent ?? responseText;
                    }
                    else
                    {
                        responseVar.Value = responseText;
                    }
                }
                else
                {
                    responseVar.Value = responseText;
                }
            }
        }
        catch (Exception ex)
        {
            Log.Error("Failed to parse response JSON: " + ex.Message);
            responseVar.Value = responseText;
        }
        
    }


}
