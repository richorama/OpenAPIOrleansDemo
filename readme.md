# OpenAI - Orleans Demo

A demo showing a "Todo List" Orleans application calling OpenAI.

## Usage

Create a `.env` file with these contents:

```
OPEN_AI_ENDPOINT=https://XXX.openai.azure.com
OPEN_AI_KEY=XXX
```

Start the application:

```
% dotnet run
```

Open a browser to `http://localhost:5195/` to see the todo List.

POST plain text to this URL to chat with the todo list.

The AI is able to add and remove items from the list.

## License

MIT