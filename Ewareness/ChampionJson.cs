using Newtonsoft.Json.Serialization;

namespace Ewareness
{
    using System;
    using System.Net;
    using System.Collections.Generic;

    using Newtonsoft.Json;

    public partial class ChampionJson
    {
        [JsonProperty("format")]
        public string Format { get; set; }

        [JsonProperty("data")]
        public Data Data { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("version")]
        public string Version { get; set; }
    }

    public partial class Data
    {
        public ChampionData Hero { get; set; }
    }

    public partial class ChampionData
    {
        [JsonProperty("lore")]
        public string Lore { get; set; }

        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("blurb")]
        public string Blurb { get; set; }

        [JsonProperty("allytips")]
        public string[] Allytips { get; set; }

        [JsonProperty("enemytips")]
        public string[] Enemytips { get; set; }

        [JsonProperty("info")]
        public Info Info { get; set; }

        [JsonProperty("image")]
        public Image Image { get; set; }

        [JsonProperty("key")]
        public string Key { get; set; }

        [JsonProperty("recommended")]
        public Recommended[] Recommended { get; set; }

        [JsonProperty("partype")]
        public string Partype { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("passive")]
        public Passive Passive { get; set; }

        [JsonProperty("spells")]
        public Spell[] Spells { get; set; }

        [JsonProperty("tags")]
        public string[] Tags { get; set; }

        [JsonProperty("skins")]
        public Skin[] Skins { get; set; }

        [JsonProperty("stats")]
        public Stats Stats { get; set; }

        [JsonProperty("title")]
        public string Title { get; set; }
    }

    public partial class Info
    {
        [JsonProperty("defense")]
        public long Defense { get; set; }

        [JsonProperty("attack")]
        public long Attack { get; set; }

        [JsonProperty("difficulty")]
        public long Difficulty { get; set; }

        [JsonProperty("magic")]
        public long Magic { get; set; }
    }

    public partial class Image
    {
        [JsonProperty("sprite")]
        public string Sprite { get; set; }

        [JsonProperty("group")]
        public string Group { get; set; }

        [JsonProperty("full")]
        public string Full { get; set; }

        [JsonProperty("h")]
        public long H { get; set; }

        [JsonProperty("x")]
        public long X { get; set; }

        [JsonProperty("w")]
        public long W { get; set; }

        [JsonProperty("y")]
        public long Y { get; set; }
    }

    public partial class Recommended
    {
        [JsonProperty("customTag")]
        public string CustomTag { get; set; }

        [JsonProperty("requiredPerk")]
        public string RequiredPerk { get; set; }

        [JsonProperty("champion")]
        public string Champion { get; set; }

        [JsonProperty("blocks")]
        public Block[] Blocks { get; set; }

        [JsonProperty("customPanel")]
        public object CustomPanel { get; set; }

        [JsonProperty("map")]
        public string Map { get; set; }

        [JsonProperty("extensionPage")]
        public bool ExtensionPage { get; set; }

        [JsonProperty("mode")]
        public string Mode { get; set; }

        [JsonProperty("title")]
        public string Title { get; set; }

        [JsonProperty("sortrank")]
        public long? Sortrank { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; }
    }

    public partial class Block
    {
        [JsonProperty("minSummonerLevel")]
        public double MinSummonerLevel { get; set; }

        [JsonProperty("items")]
        public Item[] Items { get; set; }

        [JsonProperty("hideIfSummonerSpell")]
        public string HideIfSummonerSpell { get; set; }

        [JsonProperty("maxSummonerLevel")]
        public double MaxSummonerLevel { get; set; }

        [JsonProperty("recSteps")]
        public bool RecSteps { get; set; }

        [JsonProperty("recMath")]
        public bool RecMath { get; set; }

        [JsonProperty("showIfSummonerSpell")]
        public string ShowIfSummonerSpell { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; }
    }

    public partial class Item
    {
        [JsonProperty("hideCount")]
        public bool HideCount { get; set; }

        [JsonProperty("count")]
        public long Count { get; set; }

        [JsonProperty("id")]
        public string Id { get; set; }
    }

    public partial class Passive
    {
        [JsonProperty("image")]
        public Image Image { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }
    }

    public partial class Spell
    {
        [JsonProperty("effectBurn")]
        public string[] EffectBurn { get; set; }

        [JsonProperty("costBurn")]
        public string CostBurn { get; set; }

        [JsonProperty("cooldownBurn")]
        public string CooldownBurn { get; set; }

        [JsonProperty("cooldown")]
        public long[] Cooldown { get; set; }

        [JsonProperty("cost")]
        public long[] Cost { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }

        [JsonProperty("costType")]
        public string CostType { get; set; }

        [JsonProperty("effect")]
        public double[][] Effect { get; set; }

        [JsonProperty("maxammo")]
        public string Maxammo { get; set; }

        [JsonProperty("rangeBurn")]
        public string RangeBurn { get; set; }

        [JsonProperty("image")]
        public Image Image { get; set; }

        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("leveltip")]
        public Leveltip Leveltip { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("maxrank")]
        public long Maxrank { get; set; }

        [JsonProperty("range")]
        public long[] Range { get; set; }

        [JsonProperty("tooltip")]
        public string Tooltip { get; set; }

        [JsonProperty("resource")]
        public string Resource { get; set; }

        [JsonProperty("vars")]
        public Var[] Vars { get; set; }
    }

    public partial class Leveltip
    {
        [JsonProperty("effect")]
        public string[] Effect { get; set; }

        [JsonProperty("label")]
        public string[] Label { get; set; }
    }

    public partial class Var
    {
        public double? Double;

        public double[] DoubleArray;

    }

    public partial class Skin
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("chromas")]
        public bool Chromas { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("num")]
        public long Num { get; set; }
    }

    public partial class Stats
    {
        [JsonProperty("crit")]
        public long Crit { get; set; }

        [JsonProperty("attackdamageperlevel")]
        public double Attackdamageperlevel { get; set; }

        [JsonProperty("armorperlevel")]
        public double Armorperlevel { get; set; }

        [JsonProperty("armor")]
        public double Armor { get; set; }

        [JsonProperty("attackdamage")]
        public double Attackdamage { get; set; }

        [JsonProperty("attackspeedoffset")]
        public long Attackspeedoffset { get; set; }

        [JsonProperty("attackrange")]
        public long Attackrange { get; set; }

        [JsonProperty("attackspeedperlevel")]
        public double Attackspeedperlevel { get; set; }

        [JsonProperty("hpregen")]
        public double Hpregen { get; set; }

        [JsonProperty("mpperlevel")]
        public long Mpperlevel { get; set; }

        [JsonProperty("hp")]
        public long Hp { get; set; }

        [JsonProperty("critperlevel")]
        public long Critperlevel { get; set; }

        [JsonProperty("hpperlevel")]
        public long Hpperlevel { get; set; }

        [JsonProperty("movespeed")]
        public long Movespeed { get; set; }

        [JsonProperty("hpregenperlevel")]
        public double Hpregenperlevel { get; set; }

        [JsonProperty("mp")]
        public double Mp { get; set; }

        [JsonProperty("mpregenperlevel")]
        public double Mpregenperlevel { get; set; }

        [JsonProperty("mpregen")]
        public double Mpregen { get; set; }

        [JsonProperty("spellblock")]
        public double Spellblock { get; set; }

        [JsonProperty("spellblockperlevel")]
        public double Spellblockperlevel { get; set; }
    }

    public partial class ChampionJson
    {
        public static ChampionJson FromJson(string json, string championName)
        {
            return JsonConvert.DeserializeObject<ChampionJson>(json, Converter.Settings(championName));
        }
    }

    public static class Serialize
    {
        public static string ToJson(this ChampionJson self)
        {
            return JsonConvert.SerializeObject(self, Converter.Settings(""));
        }
    }

    public class Converter
    {
        public static JsonSerializerSettings Settings(string championName)
        {
            var settings = new JsonSerializerSettings
            {
                MetadataPropertyHandling = MetadataPropertyHandling.Ignore,
                DateParseHandling = DateParseHandling.None,
            };

            settings.ContractResolver = new CustomContractResolver(championName);

            return settings;
        }

        public class CustomContractResolver : DefaultContractResolver
        {
            private Dictionary<string, string> PropertyMappings { get; set; }

            public CustomContractResolver(string heroName)
            {
                this.PropertyMappings = new Dictionary<string, string>
                {
                    {"Hero", heroName},
                };
            }

            protected override string ResolvePropertyName(string propertyName)
            {
                string resolvedName = null;
                var resolved = this.PropertyMappings.TryGetValue(propertyName, out resolvedName);
                return (resolved) ? resolvedName : base.ResolvePropertyName(propertyName);
            }
        }
    }
}
