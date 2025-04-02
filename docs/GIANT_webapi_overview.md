# GIANT - Web API

The **Web API** serves as the intermediary communication layer between machine learning (ML) frameworks and Evaluation Environments. By handling data translation and transmission, it enables seamless interaction between these components, facilitating the use of multiple ML frameworks and Evaluation Environments without requiring direct integration between them.

When an optimization process is initiated within an ML framework and an evaluation is needed, the framework sends an evaluation request to the Web API. The Web API processes this request by translating the enclosed data into a format compatible with the designated Evaluation Environment. It then forwards the request to the Evaluation Environment, which executes the evaluation and generates a response. This response is then transmitted back to the Web API, which translates the data, if necessary, and relays it to the ML framework.

This architecture ensures flexibility, as ML frameworks can interact with different Evaluation Environments using a consistent API call. The Web API manages communication and data conversion, reducing the need for extensive modifications when switching between different Evaluation Environments.

## Web API Limitations

While the Web API offers significant advantages, it currently has some limitations:

- **Platform Dependency**: The Web API is implemented in **.NET**, making deployment on **Linux and macOS** more complex compared to **Windows** environments.
- **Limited Support for Data Translation**: At present, the Web API only supports the translation of data from the **EARS framework** to the **Unity Evaluation Environment**. However, it is designed to be extensible, allowing additional ML frameworks and Evaluation Environments to be supported with minimal development effort.

## Extending the Web API

To support additional ML frameworks and Evaluation Environments, the Web API can be extended by following these steps:

1. **Request Parsing**: Implement methods to parse and process incoming requests from the ML framework, ensuring compatibility with the target Evaluation Environment.
2. **Evaluation Handling**: Develop methods that forward the processed request to the Evaluation Environment, trigger the evaluation process, and collect the results.
3. **Response Processing**: Implement methods to interpret and format the evaluation response before forwarding it back to the ML framework.
