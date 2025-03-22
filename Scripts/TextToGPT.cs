using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Networking;
using System.Collections;
using Newtonsoft.Json; 

public class ChatGPTIntegration : MonoBehaviour
{
    public TMP_InputField userInputField;
    public Button sendButton;
    public TextMeshProUGUI responseText;

    private string apiKey = ""; // API key goes here 

    private string apiURL = "https://api.openai.com/v1/chat/completions";

    void Start()
    {
        sendButton.onClick.AddListener(WhenSendButtonClicked); // Waits for button to be pressed
    }
    
    // Action when button is pressed
    public void WhenSendButtonClicked()
    {
        string prompt = userInputField.text; // User prompt
        if (!string.IsNullOrEmpty(prompt))
        {
            StartCoroutine(SendRequest(prompt)); // Send prompt to GPT
            userInputField.text = "";
        }
    }

    private IEnumerator SendRequest(string prompt) // sends request to ChatGPT
    {
        responseText.text = "Let me think for a second...";

        // Constructing the request object
        ChatGPTRequest requestData = new ChatGPTRequest
        {
            model = "gpt-3.5-turbo", // Model of GPT
            messages = new Message[] { new Message { role = "user", content = prompt } } 
        };
        
        // Convert the request object to JSON using Newtonsoft.Json
        string jsonData = JsonConvert.SerializeObject(requestData);
        Debug.Log("Sending JSON: " + jsonData);

        var request = new UnityWebRequest(apiURL, "POST");
        byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonData);
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        
        // Set headers
        request.SetRequestHeader("Content-Type", "application/json");
        request.SetRequestHeader("Authorization", "Bearer " + apiKey);

        // Send the request and wait for the response 
        yield return request.SendWebRequest();

        // If error encountered 
        if (request.result == UnityWebRequest.Result.ConnectionError ||
            request.result == UnityWebRequest.Result.ProtocolError)
        {
            Debug.LogError("Request failed" + request.error);
            Debug.LogError("Raw response: " + request.downloadHandler.text);
            
            // Log headers that are present
            foreach (var header in request.GetResponseHeaders())
            {
                Debug.Log($"Header: {header.Key} = {header.Value}");
            }
            responseText.text = "Error: " + request.error;
        }
        else
        {
            // GPT's response
            Debug.Log("Response: " + request.downloadHandler.text);
            ChatGPTResponse response = JsonUtility.FromJson<ChatGPTResponse>(request.downloadHandler.text);
            if (response != null && response.choices.Length > 0)
            {
                responseText.text = response.choices[0].message.content;
            }
            else
            {
                responseText.text = "No response received.";
            }
        }
    }
}

[System.Serializable]
public class ChatGPTResponse
{
    public Choice[] choices;
}

[System.Serializable]
public class Choice
{
    public Message message;
}

[System.Serializable]
public class ChatGPTRequest
{
    public string model;
    public Message[] messages;
}

[System.Serializable]
public class Message
{
    public string role;
    public string content;
}