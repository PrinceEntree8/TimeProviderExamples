using System.Text.Json.Serialization;

namespace TimeProviderExample.Service.Serializers;

[JsonSerializable(typeof(DateTime))]
[JsonSerializable(typeof(DateTimeOffset))]
[JsonSerializable(typeof(double))]
internal partial class AppJsonSerializer : JsonSerializerContext;