using System;
using System.Collections.Generic;
using T = System.Collections.Generic.Dictionary<string, string>;

namespace AbilityWheel;

// The mod's texts in the game's 11 languages, by locale code as Belmont UI's LocalizedText takes
// them. Where the game has a term, these are its own words (Cancel, Equipment, Tarot, Armoury,
// Interact, Holy Light, the spell names). Every character must exist in that language's game font, or
// it shows as a blank box. The row texts are in TextsRows.cs.
internal static partial class Texts
{
    // The game's language, from Belmont UI 0.3 or later; English without it.
    internal static string Language = "en";

    internal static string Get(T text) => Resolve(text, Language);

    // The exact code, then without its region (never between the two Chinese), then English.
    internal static string Resolve(T text, string locale)
    {
        string code = (locale ?? "en").Replace('_', '-');
        foreach (var pair in text)
        {
            if (string.Equals(pair.Key, code, StringComparison.OrdinalIgnoreCase))
                return pair.Value;
        }
        int cut = code.IndexOf('-');
        if (cut > 0 && !code.StartsWith("zh", StringComparison.OrdinalIgnoreCase))
        {
            string language = code.Substring(0, cut);
            foreach (var pair in text)
            {
                if (string.Equals(pair.Key, language, StringComparison.OrdinalIgnoreCase))
                    return pair.Value;
            }
        }
        return text.TryGetValue("en", out var english) ? english : "";
    }

    // A text with {0} filled in, in every language.
    internal static T Format(T text, string arg)
    {
        var result = new T();
        foreach (var pair in text)
            result[pair.Key] = string.Format(pair.Value, arg);
        return result;
    }

    // The HUD's hints in the Tarot and Equipment menus.
    internal static readonly T AddToWheel = new()
    {
        ["en"] = "Add to wheel",
        ["fr"] = "Ajouter à la roue",
        ["de"] = "Zum Rad hinzufügen",
        ["es"] = "Añadir a la rueda",
        ["it"] = "Aggiungi alla ruota",
        ["pt-BR"] = "Adicionar à roda",
        ["ru"] = "На колесо",
        ["ja"] = "ホイールに入れる",
        ["ko"] = "바퀴에 넣기",
        ["zh-CN"] = "加入轮盘",
        ["zh-TW"] = "加入輪盤",
    };

    internal static readonly T ChooseSlot = new()
    {
        ["en"] = "Choose a slot",
        ["fr"] = "Choisir un emplacement",
        ["de"] = "Platz wählen",
        ["es"] = "Elige una casilla",
        ["it"] = "Scegli uno slot",
        ["pt-BR"] = "Escolha um espaço",
        ["ru"] = "Выберите ячейку",
        ["ja"] = "スロットを選択",
        ["ko"] = "슬롯 선택",
        ["zh-CN"] = "选择位置",
        ["zh-TW"] = "選擇位置",
    };

    // The game's own (UI_Common/Common_Cancel).
    internal static readonly T Cancel = new()
    {
        ["en"] = "Cancel",
        ["fr"] = "Annuler",
        ["de"] = "Abbrechen",
        ["es"] = "Cancelar",
        ["it"] = "Annulla",
        ["pt-BR"] = "Cancelar",
        ["ru"] = "Отмена",
        ["ja"] = "キャンセル",
        ["ko"] = "취소",
        ["zh-CN"] = "取消",
        ["zh-TW"] = "取消",
    };
}
