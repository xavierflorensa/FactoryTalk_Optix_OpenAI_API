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
       var responseVar = Project.Current.GetVariable("Model/ResponseString");
       responseVar.Value = "Hello World!";
        var apiKey = "Your_OpenAI_API_Key_Here"; // Replace with your actual API key
        var endpoint = "https://api.openai.com/v1/chat/completions";
        var client = new HttpClient();
        client.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiKey}");

        var request = new
        {
            model = "gpt-3.5-turbo",
            messages = new[]
            {
                new { role = "user", content = "Translate the following English text to French: 'Hello, how are you?'" }
            },
            max_tokens = 60
        };
        var content = new StringContent(JsonSerializer.Serialize(request), Encoding.UTF8, "application/json");

        var response = await client.PostAsync(endpoint, content);
        var responseText = await response.Content.ReadAsStringAsync();
        Log.Info("ResponseString: "+responseText);
        responseVar.Value = responseText;
        
    }

    public override void Stop()
    {
        // Insert code to be executed when the user-defined logic is stopped
    }
}
