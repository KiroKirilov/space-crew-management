using Newtonsoft.Json;
using Space.CrewManagement.Services.Interfaces;

namespace Space.CrewManagement.Services.Services;
public class BodyParser : IBodyParser
{
    public async Task<T?> Parse<T>(Stream body)
        where T: class
    {
        try
        {
            using var streamReader = new StreamReader(body);
            var requestBody = await streamReader.ReadToEndAsync();
            var dto = JsonConvert.DeserializeObject<T>(requestBody);

            return dto;
        }
        catch (Exception)
        {
            return null;
        };
    }
}
