using Newtonsoft.Json;
using System;
using System.Globalization;

namespace WebAranzmani.Service
{
    public class DatumKonverter : JsonConverter<DateTime>
    {
        private readonly string[] formati =
        {
            "dd/MM/yyyy",
            "dd/MM/yyyy HH:mm:ss"
        };

        public override void WriteJson(JsonWriter writer, DateTime value, JsonSerializer serializer)
        {
            writer.WriteValue(value.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture));
        }

        public override DateTime ReadJson(JsonReader reader, Type objectType,
            DateTime existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            if (reader.Value == null)
                return DateTime.MinValue;

            string tekst = reader.Value.ToString().Trim();

            // 1. Pokušaj sa zadatim formatima
            if (DateTime.TryParseExact(tekst,
                                       formati,
                                       CultureInfo.InvariantCulture,
                                       DateTimeStyles.None,
                                       out var datum))
            {
                return datum;
            }

            // 2. Probaj sa običnim parsiranjem (hvata "01/01/1990 00:00:00")
            if (DateTime.TryParse(tekst, CultureInfo.InvariantCulture, DateTimeStyles.None, out var fallback))
            {
                return fallback;
            }

            // 3. Ako ništa ne uspe, NEMOJ bacati exception — samo vrati MinValue
            return DateTime.MinValue;
        }
    }
}
