using System;
using System.Reflection;
using malyar_apk.Shared;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace malyar_apk
{
    [Obsolete]
    class ResponsiveTPMConverter : JsonConverter<TimedPictureModel>
    {
        public override TimedPictureModel Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var result = new TimedPictureModel();
            string current_property = string.Empty;
            while (reader.Read())
            {
                switch (reader.TokenType)
                {
                    case JsonTokenType.PropertyName:
                        current_property = reader.GetString();
                        break;
                    case JsonTokenType.String:
                        PropertyInfo pINF = typeof(TimedPictureModel).GetProperty(current_property);
                        if (pINF.PropertyType == typeof(string))
                        {
                            pINF.SetValue(result, reader.GetString());
                        }
                        else
                        {
                            pINF.SetValue(result, TimeSpan.Parse(reader.GetString()));
                        }
                        break;
                    case JsonTokenType.EndObject:
                        if(result.EndTime < result.StartTime)
                        {
                            result.end_time = result.end_time.Add(TimeSpan.FromDays(1));
                        }
                        return result;
                        break;
                }
            }
            throw new JsonException(); // Truncated file or internal error
        }

        public override void Write(Utf8JsonWriter writer, TimedPictureModel value, JsonSerializerOptions options)
        {
            writer.WriteStartObject();
            foreach(var propertyInf in value.GetType().GetProperties())
            {
                if(propertyInf.GetCustomAttribute(typeof(JsonIgnoreAttribute))!=null) {
                    continue;
                }
                writer.WritePropertyName(propertyInf.Name);
                object v = propertyInf.GetValue(value);
                writer.WriteStringValue(v is string? v as string : ((TimeSpan)v).ToString(Constants.TimeFormat));
            }
            writer.WriteEndObject();
        }
    }
}
