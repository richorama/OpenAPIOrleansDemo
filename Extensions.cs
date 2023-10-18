using Azure.AI.OpenAI;

public static class IListExtensions
{
    public static void AddRange<T>(this IList<T> list, IEnumerable<T> items)
    {
        foreach (var item in items)
        {
            list.Add(item);
        }
    }

    public static void AddMessage<T>(this IList<ChatMessage> list, string role, string content) where T : ChatMessage
    {
        list.Add(new ChatMessage()
        {
            Content = content,
            Role = role
        }); 
    }
}