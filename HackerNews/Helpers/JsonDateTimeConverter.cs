using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;


namespace Hacker_News.Helpers
{
    public class JsonDateTimeConverter : JsonConverter<DateTime>
    {
        public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            try
            {
                var seconds = reader.GetInt64();
                return DateTimeOffset.FromUnixTimeSeconds(seconds).UtcDateTime;
            }
            catch (Exception ex)
            {
                throw new JsonException("Failed to parse /Date(...) format", ex);
            }
        }

        public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options)
        {
            long seconds = new DateTimeOffset(value).ToUnixTimeSeconds();
            writer.WriteNumberValue(seconds);
        }
    }
}
