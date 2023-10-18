public interface IChatGrain : IGrainWithStringKey
{
    Task<string> Chat(string input);
    Task<string[]> GetTodos();
}
