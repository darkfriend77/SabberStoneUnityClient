using SabberStoneCore.Enums;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

public class EntityExt
{
    public IDictionary<GameTag, int> Origin { get; set; }

    public int Id { get; set; }
    public BasicGen GameObjectScript { get; set; }
    public string CardId { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public IDictionary<GameTag, int> Tags { get; set; } = new Dictionary<GameTag, int>();

    public CardType CardType => Tags.ContainsKey(GameTag.CARDTYPE) ? (CardType)Tags[GameTag.CARDTYPE] : CardType.INVALID;

    public Zone Zone => Tags.ContainsKey(GameTag.ZONE) ? (Zone)Tags[GameTag.ZONE] : Zone.INVALID;

    public int Health => Tags[GameTag.HEALTH] - Tags[GameTag.DAMAGE];
    public Color HealthColor => Tags[GameTag.DAMAGE] == 0 ? Color.white : Color.red;

    public int Durability => Tags[GameTag.DURABILITY] - Tags[GameTag.DAMAGE];
    public Color DurabilityColor => Tags[GameTag.DAMAGE] == 0 ? Color.white : Color.red;

    public string Print()
    {
        var str = new StringBuilder();
        str.AppendLine($"Id={Id}, CardId={CardId} Tags=[");
        foreach (KeyValuePair<GameTag, int> pair in Tags)
        {
            str.AppendLine($"      [{pair.Key},{pair.Value}]");
        }
        str.Append("]");
        return str.ToString();
    }
}

