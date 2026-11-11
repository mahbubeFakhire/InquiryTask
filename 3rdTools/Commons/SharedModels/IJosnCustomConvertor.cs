using System.Text.Json;
using System;

namespace SharedModels;

public interface IJosnCustomConvertor
{
    Action<Utf8JsonWriter> ExtraPropertyHandler { get; }
}