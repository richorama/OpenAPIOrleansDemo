using Azure.AI.OpenAI;
using Newtonsoft.Json;


public sealed class ChatGrain : Grain, IChatGrain
{
    private readonly OpenAIClient _client;
    private readonly List<ChatMessage> _messages = new List<ChatMessage>();
    private readonly List<string> _todos = new List<string>()
    {
        "Take the bins out",
        "Do the washing up",
        "Prepare the dinner"
    };

    public ChatGrain(OpenAIClient client)
    {
        _client = client;
    }

    public async Task<string> Chat(string input)
    {
        _messages.Add(new ChatMessage()
        {
            Content = input,
            Role = "user"
        });

        while (true)
        {
            var responseMessage = await CallOpenAI();
            _messages.Add(responseMessage);

            if (responseMessage.FunctionCall?.Name == "add_todo")
            {
                responseMessage.Content = AddTodo(responseMessage.FunctionCall.Arguments);
            }
            else if (responseMessage.FunctionCall?.Name == "remove_todo")
            {
                responseMessage.Content = RemoveTodo(responseMessage.FunctionCall.Arguments);
            }
            else
            {
                return responseMessage.Content;
            }
        }
    }

    private async Task<ChatMessage> CallOpenAI()
    {
        var options = new ChatCompletionsOptions();
        options.Messages.AddMessage<ChatMessage>("system", $@"You are a friendly chat bot which maintains a todo list.
You are very encouraging and enthusiastic about getting things done. These are the current items in the list:
---
{string.Join('\n', _todos)}");

        options.Messages.AddRange(_messages);
        options.Functions.Add(new FunctionDefinition()
        {
            Name = "add_todo",
            Parameters = BinaryData.FromString(@$"
{{
    ""type"": ""object"",
    ""properties"": {{
        ""description"": {{
            ""type"": ""string"",
            ""description"": ""Description of the todo item""
        }}
    }},
    ""required"": [""description""]
}}"),
            Description = "add a todo item to the list"
        });

        options.Functions.Add(new FunctionDefinition()
        {
            Name = "remove_todo",
            Parameters = BinaryData.FromString(@$"
{{
    ""type"": ""object"",
    ""properties"": {{
        ""index"": {{
            ""type"": ""integer"",
            ""description"": ""The zero-based index of the todo item to remove""
        }}
    }},
    ""required"": [""index""]
}}"),
            Description = "remove a completed todo item from the list by index"
        });

        var response = await _client.GetChatCompletionsAsync("gpt4", options);
        return response.Value.Choices[0].Message;
    }

    private string AddTodo(string serialisedArguments)
    {
        var arguments = JsonConvert.DeserializeObject<AddTodoParameters>(serialisedArguments);
        Console.WriteLine($"add_todo('{arguments?.description}')");
        _todos.Add(arguments?.description ?? "");
        return $"Added todo item: {arguments?.description}";
    }

    private string RemoveTodo(string serialisedArguments)
    {
        var arguments = JsonConvert.DeserializeObject<RemoveTodoParameters>(serialisedArguments);
        Console.WriteLine($"remove_todo({arguments?.index})");
        _todos.RemoveAt(arguments?.index ?? 0);
        return $"Removed todo at index {arguments?.index}";
    }

    public Task<string[]> GetTodos() => Task.FromResult(_todos.ToArray());
}

