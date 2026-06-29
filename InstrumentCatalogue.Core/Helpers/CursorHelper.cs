using InstrumentCatalogue.Core.Exceptions;
using System.Text.Json;

namespace InstrumentCatalogue.Core.Helpers;

public static class CursorHelper
{
    public static string Encode<T>(T cursorPayload)
    {
        
        return Base64Helper.Base64Encode(JsonSerializer.Serialize(cursorPayload));

    }

    public static T Decode<T>(string cursor)
    {
        try
        {
            var decodedData = Base64Helper.Base64Decode(cursor);
            var cursorPayload = JsonSerializer.Deserialize<T>(decodedData);

            if (cursorPayload == null)
                throw new InvalidCursorException();

            return cursorPayload;
        }
        catch (FormatException ex) { throw new InvalidCursorException(ex); }
        catch (JsonException ex) { throw new InvalidCursorException(ex); }

    }

}
