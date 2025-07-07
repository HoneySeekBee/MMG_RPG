using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MMG
{
    public class FloatRoundingConverter : JsonConverter
    {
        private readonly int _decimalPlaces;

        public FloatRoundingConverter(int decimalPlaces)
        {
            _decimalPlaces = decimalPlaces;
        }

        public override bool CanConvert(System.Type objectType) =>
            objectType == typeof(float) || objectType == typeof(double);
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            double number;

            if (value is float f)
                number = f;
            else if (value is double d)
                number = d;
            else
                throw new JsonSerializationException($"FloatRoundingConverter: 예상치 못한 타입: {value.GetType()}");

            double rounded = System.Math.Round(number, _decimalPlaces);
            writer.WriteValue(rounded);
        }

        public override object ReadJson(JsonReader reader, System.Type objectType, object existingValue, JsonSerializer serializer)
        {
            return reader.Value;
        }
    }

}

