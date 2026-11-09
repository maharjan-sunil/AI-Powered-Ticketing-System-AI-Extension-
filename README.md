**AI Ticketing Service**

An AI service built with C# / .NET that supports the ticketing backend in two ways:

Automated **ticket triage**: reads each new ticket message, then generates a triage report with a category and priority.
This runs asynchronously over **Apache Kafka.**
**RAG** question answering: answers client queries using Retrieval-Augmented Generation over a knowledge base, so responses are grounded in your own documents rather than the model's general knowledge.
