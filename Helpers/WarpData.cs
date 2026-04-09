using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;

namespace CreativeMode.Helpers;

public class WarpData
{
    [JsonProperty("warps")]
    public List<Warp> Warps { get; set; } = new List<Warp>();
}

public class Warp
{
    [JsonProperty("name")]
    public string Name { get; set; }

    [JsonProperty("position")]
    public Vector3Data Position { get; set; }
}

public class Vector3Data
{
    [JsonProperty("x")]
    public float X { get; set; }

    [JsonProperty("y")]
    public float Y { get; set; }

    [JsonProperty("z")]
    public float Z { get; set; }

    public Vector3Data() { }

    public Vector3Data(Vector3 vector)
    {
        X = vector.x;
        Y = vector.y;
        Z = vector.z;
    }

    public Vector3 ToVector3() => new Vector3(X, Y, Z);
}