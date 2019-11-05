﻿using System;
using Newtonsoft.Json;

namespace zMoneyWinX.Model
{
    [JsonObject]
    public class Deletion
    {
        [JsonProperty("id")]
        public Guid Id { get; set; }

        [JsonProperty("object")]
        public string Object { get; set; }

        [JsonProperty("stamp")]
        public long Stamp { get; set; }

        [JsonProperty("user")]
        public int User { get; set; }
    }
}