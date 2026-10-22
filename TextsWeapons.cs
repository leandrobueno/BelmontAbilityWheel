using T = System.Collections.Generic.Dictionary<string, string>;

namespace AbilityWheel;

// Setting row texts, part 3: the weapon wheel. Potion is the game's word (its settings' potions).
internal static partial class Texts
{
    internal static readonly T WeaponWheel = new()
    {
        ["en"] = "Weapon wheel", ["fr"] = "Roue des armes", ["de"] = "Waffenrad", ["es"] = "Rueda de armas",
        ["it"] = "Ruota delle armi", ["pt-BR"] = "Roda de armas", ["ru"] = "Колесо оружия", ["ja"] = "武器ホイール",
        ["ko"] = "무기 바퀴", ["zh-CN"] = "武器轮盘", ["zh-TW"] = "武器輪盤",
    };

    internal static readonly T WeaponButton = new()
    {
        ["en"] = "Weapon button (gamepad)", ["fr"] = "Bouton d'armes (manette)", ["de"] = "Waffentaste (Controller)",
        ["es"] = "Botón de armas (mando)", ["it"] = "Tasto armi (controller)", ["pt-BR"] = "Botão de armas (controle)",
        ["ru"] = "Кнопка оружия (геймпад)", ["ja"] = "武器ボタン（コントローラー）", ["ko"] = "무기 버튼(컨트롤러)",
        ["zh-CN"] = "武器键(手柄)", ["zh-TW"] = "武器鍵（控制器）",
    };

    internal static readonly T WeaponButtonHint = new()
    {
        ["en"] = "Default: hold the wheel button, then also hold LB; LB on its own still drinks a potion. Any other button: hold it on its own and press Y/X/B/A, and a tap still does its normal action. With LB, LB never drinks a potion. Can't be the wheel button itself.",
        ["fr"] = "Par défaut : maintenez le bouton de la roue, puis aussi LB ; LB seul boit toujours une potion. Autre bouton : maintenez-le seul et appuyez sur Y/X/B/A ; un appui bref garde son action normale. Avec LB, LB ne boit plus jamais de potion. Ne peut pas être le bouton de la roue.",
        ["de"] = "Standard: Halte die Radtaste und dann zusätzlich LB; LB allein trinkt weiterhin einen Trank. Andere Taste: Halte sie allein und drücke Y/X/B/A; kurz antippen behält ihre normale Aktion. Mit LB trinkt LB nie einen Trank. Darf nicht die Radtaste sein.",
        ["es"] = "Por defecto: mantén el botón de la rueda y luego también LB; LB solo sigue bebiendo una poción. Otro botón: mantenlo solo y pulsa Y/X/B/A; una pulsación corta hace su acción normal. Con LB, LB nunca bebe pociones. No puede ser el botón de la rueda.",
        ["it"] = "Predefinito: tieni premuto il tasto della ruota, poi anche LB; LB da solo beve ancora una pozione. Altro tasto: tienilo premuto da solo e premi Y/X/B/A; un tocco breve esegue la sua azione normale. Con LB, LB non beve mai pozioni. Non può essere il tasto della ruota.",
        ["pt-BR"] = "Padrão: segure o botão da roda e depois também LB; LB sozinho continua bebendo uma poção. Outro botão: segure-o sozinho e aperte Y/X/B/A; um toque rápido faz a ação normal dele. Com LB, LB nunca bebe poções. Não pode ser o botão da roda.",
        ["ru"] = "По умолчанию: удерживайте кнопку колеса, затем ещё LB; LB отдельно по-прежнему пьёт зелье. Другая кнопка: удерживайте её отдельно и нажмите Y/X/B/A; короткое нажатие выполняет её обычное действие. С LB кнопка LB никогда не пьёт зелья. Нельзя выбрать кнопку колеса.",
        ["ja"] = "初期設定：ホイールボタンを押しながらLBも押します。LBだけならポーションを使います。他のボタン：そのボタンだけを押しながらY/X/B/Aを押します。短く押すと本来の動作になります。LBにするとLBでポーションを使えなくなります。ホイールボタンと同じボタンは選べません。",
        ["ko"] = "기본: 바퀴 버튼을 누른 채 LB도 누릅니다. LB만 누르면 물약을 마십니다. 다른 버튼: 그 버튼만 누른 채 Y/X/B/A를 누르며, 짧게 누르면 원래 동작을 합니다. LB로 하면 LB로 물약을 마시지 않습니다. 바퀴 버튼과 같을 수 없습니다.",
        ["zh-CN"] = "默认:按住轮盘键后再按住 LB,单独按 LB 仍是使用药水。选择其他按键时,单独按住该键并按 Y/X/B/A 切换武器,轻按仍会执行该键原本的动作。选择 LB 时,LB 不会再使用药水。不能与轮盘键相同。",
        ["zh-TW"] = "預設：按住輪盤鍵後再按住 LB，單獨按 LB 仍是使用藥水。選擇其他按鍵時，單獨按住該鍵並按 Y/X/B/A 切換武器，輕按仍會執行該鍵原本的動作。選擇 LB 時，LB 不會再使用藥水。不能與輪盤鍵相同。",
    };

    internal static readonly T WheelThenLB = new()
    {
        ["en"] = "Wheel button + LB", ["fr"] = "Bouton de la roue + LB", ["de"] = "Radtaste + LB",
        ["es"] = "Botón de la rueda + LB", ["it"] = "Tasto della ruota + LB", ["pt-BR"] = "Botão da roda + LB",
        ["ru"] = "Кнопка колеса + LB", ["ja"] = "ホイールボタン + LB", ["ko"] = "바퀴 버튼 + LB",
        ["zh-CN"] = "轮盘键 + LB", ["zh-TW"] = "輪盤鍵 + LB",
    };

    internal static readonly T WeaponKeyHint = new()
    {
        ["en"] = "Switches to that slot's weapon at once.", ["fr"] = "Prend aussitôt l'arme de cet emplacement.",
        ["de"] = "Wechselt sofort zur Waffe dieses Platzes.", ["es"] = "Cambia al instante al arma de esa casilla.",
        ["it"] = "Passa subito all'arma di quello slot.", ["pt-BR"] = "Troca na hora para a arma desse espaço.",
        ["ru"] = "Сразу берёт оружие этой ячейки.", ["ja"] = "そのスロットの武器にすぐ切り替えます。",
        ["ko"] = "해당 슬롯의 무기로 바로 전환합니다.", ["zh-CN"] = "按下即装备该位置的武器。", ["zh-TW"] = "按下即裝備該位置的武器。",
    };
}
