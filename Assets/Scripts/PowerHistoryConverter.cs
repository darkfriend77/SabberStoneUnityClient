using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SabberStoneCore.Enums;
using SabberStoneCore.Kettle;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PowerHistoryConverter : JsonConverter
{
    #region Overrides of JsonConverter

    public override bool CanWrite => false;
    public override bool CanRead => true;
    public override bool CanConvert(System.Type objectType)
    {
        return objectType == typeof(IPowerHistoryEntry);
    }

    public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
    {
        throw new NotImplementedException();
    }

    public override object ReadJson(JsonReader reader, System.Type objectType, object existingValue, JsonSerializer serializer)
    {
        var jsonObject = JObject.Load(reader);
        IPowerHistoryEntry history;
        switch ((PowerType)jsonObject["PowerType"].Value<int>())
        {
            case PowerType.FULL_ENTITY:
                history = new PowerHistoryFullEntity();
                break;
            case PowerType.SHOW_ENTITY:
                history = new PowerHistoryShowEntity();
                break;
            case PowerType.HIDE_ENTITY:
                history = new PowerHistoryHideEntity();
                break;
            case PowerType.TAG_CHANGE:
                history = new PowerHistoryTagChange();
                break;
            case PowerType.BLOCK_START:
                history = new PowerHistoryBlockStart();
                break;
            case PowerType.BLOCK_END:
                history = new PowerHistoryBlockEnd();
                break;
            case PowerType.CREATE_GAME:
                history = new PowerHistoryCreateGame();
                break;
            case PowerType.META_DATA:
                history = new PowerHistoryMetaData();
                break;
            case PowerType.CHANGE_ENTITY:
            case PowerType.RESET_GAME:
            default:
                throw new ArgumentOutOfRangeException();
        }

        serializer.Populate(jsonObject.CreateReader(), history);
        return history;
    }
    #endregion
}
