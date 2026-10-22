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

    internal static readonly T Title = new()
    {
        ["en"] = "Ability Wheel",
        ["zh-CN"] = "技能轮盘",
        ["zh-TW"] = "技能輪盤",
    };

    internal static readonly T Summary = new()
    {
        ["en"] = "Cast up to 4 Tarot spells: hold the wheel button and press Y/X/B/A. Hold LB too (or a weapon button of your choice) to switch between 4 weapons.",
        ["fr"] = "Lancez jusqu'à 4 sorts du Tarot : maintenez le bouton de la roue et appuyez sur Y/X/B/A. Maintenez aussi LB (ou un bouton d'armes de votre choix) pour changer parmi 4 armes.",
        ["de"] = "Wirke bis zu 4 Tarot-Zauber: Halte die Radtaste und drücke Y/X/B/A. Halte zusätzlich LB (oder eine eigene Waffentaste), um zwischen 4 Waffen zu wechseln.",
        ["es"] = "Lanza hasta 4 hechizos del Tarot: mantén el botón de la rueda y pulsa Y/X/B/A. Mantén también LB (o un botón de armas a tu elección) para cambiar entre 4 armas.",
        ["it"] = "Lancia fino a 4 incantesimi dei Tarocchi: tieni premuto il tasto della ruota e premi Y/X/B/A. Tieni premuto anche LB (o un tasto armi a tua scelta) per passare tra 4 armi.",
        ["pt-BR"] = "Lance até 4 feitiços do Tarô: segure o botão da roda e aperte Y/X/B/A. Segure também LB (ou um botão de armas à sua escolha) para trocar entre 4 armas.",
        ["ru"] = "До 4 заклинаний Таро: удерживайте кнопку колеса и нажмите Y/X/B/A. Удерживайте также LB (или выбранную кнопку оружия), чтобы переключаться между 4 видами оружия.",
        ["ja"] = "タロットのスペルを最大4つ使えます：ホイールボタンを押しながらY/X/B/Aを押します。さらにLB（または好きな武器ボタン）を押すと4つの武器を切り替えられます。",
        ["ko"] = "타로 주문을 최대 4개까지 사용합니다: 바퀴 버튼을 누른 채 Y/X/B/A를 누르세요. LB(또는 원하는 무기 버튼)도 누르고 있으면 무기 4개를 전환할 수 있습니다.",
        ["zh-CN"] = "按住轮盘键并按 Y/X/B/A,快速施放 4 张塔罗牌的法术;再按住 LB(或自选的武器键)可切换 4 把武器。",
        ["zh-TW"] = "按住輪盤鍵並按 Y/X/B/A，快速使用 4 張塔羅牌的法術，再按住 LB（或自選的武器鍵）可切換 4 把武器。",
    };

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
