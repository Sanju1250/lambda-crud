using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using lambda;
using System.Text.Json;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace lambda_crud;

public class Function
{
    private readonly DynamoDBContext _dynamoDBContext;
    public Function()
    {
        _dynamoDBContext = new DynamoDBContext(new AmazonDynamoDBClient());
    }
    /// <summary>
    /// A simple function that takes a string and does a ToUpper
    /// </summary>
    /// <param name="input">The event for the Lambda function handler to process.</param>
    /// <param name="context">The ILambdaContext that provides methods for logging and describing the Lambda environment.</param>
    /// <returns></returns>
    public async Task<APIGatewayHttpApiV2ProxyResponse> FunctionHandler(APIGatewayHttpApiV2ProxyRequest request, ILambdaContext context)
    {
        return request.RequestContext.Http.Method.ToUpper() switch
        {
            "GET" => await HandleGetRequest(request),
            "POST" => await HandlePostRequest(request),
            "DELETE" => await HandleDeleteRequest(request)
        };
    }

    private async Task<APIGatewayHttpApiV2ProxyResponse> HandleDeleteRequest(APIGatewayHttpApiV2ProxyRequest request)
    {
        request.PathParameters.TryGetValue("userId", out var userIdString);
        if (Guid.TryParse(userIdString, out var userId))
        {
            await _dynamoDBContext.DeleteAsync<User>(userId);
            return OkResponse();
        }
        return BadResponse("Invalid userId in the path");
    }

    private async Task<APIGatewayHttpApiV2ProxyResponse> HandlePostRequest(APIGatewayHttpApiV2ProxyRequest req)
    {
        var user = JsonSerializer.Deserialize<User>(req.Body);

        if (user == null)
        {
            return BadResponse("Invalid User details");
        }

        await _dynamoDBContext.SaveAsync(user);
        return OkResponse();
    }

    private static APIGatewayHttpApiV2ProxyResponse OkResponse()
    {
        return new APIGatewayHttpApiV2ProxyResponse()
        {
            StatusCode = 200
        };
    }

    private static APIGatewayHttpApiV2ProxyResponse BadResponse(string message)
    {
        return new APIGatewayHttpApiV2ProxyResponse()
        {
            Body = message,
            StatusCode = 400
        };
    }

    private async Task<APIGatewayHttpApiV2ProxyResponse> HandleGetRequest(APIGatewayHttpApiV2ProxyRequest request)
    {
        request.PathParameters.TryGetValue("userId", out var userIdString);
        if (Guid.TryParse(userIdString, out var userId))
        {
            var user = await _dynamoDBContext.LoadAsync<User>(userId);
            return new APIGatewayHttpApiV2ProxyResponse()
            {
                Body = JsonSerializer.Serialize(user),
                StatusCode = 200
            };
        }
        else
        {
            return BadResponse("Invalid user Id in the path");
        }
    }
}


