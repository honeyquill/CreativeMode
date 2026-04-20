using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;

namespace CreativeMode.Helpers;

public class WarpData
{
    [JsonProperty("name")]
    public string Name { get; set; }

    [JsonProperty("position")]
    public float[] Position { get; set; }
}