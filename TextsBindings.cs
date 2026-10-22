using T = System.Collections.Generic.Dictionary<string, string>;

namespace AbilityWheel;

// Belmont UI 0.4's binding rows: section headers, the reasons a press is refused and the hints of
// the rows you press a button on. Controller, Keyboard and Mouse and Already assigned are the
// game's own (UI_Settings).
internal static partial class Texts
{
    internal static readonly T HeaderWheel = new()
    {
        ["en"] = "Wheel", ["fr"] = "Roue", ["de"] = "Rad", ["es"] = "Rueda", ["it"] = "Ruota", ["pt-BR"] = "Roda",
        ["ru"] = "Колесо", ["ja"] = "ホイール", ["ko"] = "바퀴", ["zh-CN"] = "轮盘", ["zh-TW"] = "輪盤",
    };

    internal static readonly T HeaderGamepad = new()
    {
        ["en"] = "Controller", ["fr"] = "Manette", ["de"] = "Controller", ["es"] = "Mando", ["it"] = "Controller",
        ["pt-BR"] = "Controle", ["ru"] = "Геймпад", ["ja"] = "コントローラー", ["ko"] = "컨트롤러",
        ["zh-CN"] = "控制器", ["zh-TW"] = "控制器",
    };

    internal static readonly T HeaderKeyboard = new()
    {
        ["en"] = "Keyboard and Mouse", ["fr"] = "Clavier et souris", ["de"] = "Maus und Tastatur",
        ["es"] = "Teclado y ratón", ["it"] = "Tastiera e mouse", ["pt-BR"] = "Teclado e mouse",
        ["ru"] = "Клавиатура и мышь", ["ja"] = "キーボードとマウス", ["ko"] = "키보드와 마우스",
        ["zh-CN"] = "键鼠", ["zh-TW"] = "鍵盤與滑鼠",
    };

    internal static readonly T AlreadyAssigned = new()
    {
        ["en"] = "Already assigned", ["fr"] = "Déjà utilisée", ["de"] = "Wird bereits verwendet",
        ["es"] = "Asignación ya en uso", ["it"] = "Già in uso", ["pt-BR"] = "Atribuição feita", ["ru"] = "Уже привязано",
        ["ja"] = "重複しています", ["ko"] = "중복된 항목입니다", ["zh-CN"] = "已被占用。", ["zh-TW"] = "已重複",
    };

    internal static readonly T WheelFirst = new()
    {
        ["en"] = "Hold the wheel button first, then press the other button.",
        ["fr"] = "Maintenez d'abord le bouton de la roue, puis appuyez sur l'autre bouton.",
        ["de"] = "Halte zuerst die Radtaste, dann drücke die andere Taste.",
        ["es"] = "Mantén primero el botón de la rueda y luego pulsa el otro botón.",
        ["it"] = "Tieni premuto prima il tasto della ruota, poi premi l'altro tasto.",
        ["pt-BR"] = "Segure primeiro o botão da roda e depois aperte o outro botão.",
        ["ru"] = "Сначала удерживайте кнопку колеса, затем нажмите другую кнопку.",
        ["ja"] = "先にホイールボタンを押したまま、別のボタンを押してください。",
        ["ko"] = "먼저 바퀴 버튼을 누른 채로 다른 버튼을 누르세요.",
        ["zh-CN"] = "请先按住轮盘键,再按另一个键。",
        ["zh-TW"] = "請先按住輪盤鍵，再按另一個鍵。",
    };

    internal static readonly T WheelButtonBindHint = new()
    {
        ["en"] = "Select, then press the button to hold for the wheel. A quick tap still does the button's normal action (RB: Interact). LT is Holy Light, so it's a poor choice.",
        ["fr"] = "Sélectionnez, puis appuyez sur le bouton à maintenir pour la roue. Un appui bref garde l'action normale du bouton (RB : Interagir). LT est Lumière Sacrée, mieux vaut l'éviter.",
        ["de"] = "Auswählen, dann die Taste drücken, die du für das Rad hältst. Kurz antippen behält die normale Aktion (RB: Interagieren). LT ist Heiliges Licht, daher ungeeignet.",
        ["es"] = "Selecciona y pulsa el botón que mantendrás para la rueda. Una pulsación corta hace la acción normal del botón (RB: Interactuar). LT es Luz sagrada, así que no conviene.",
        ["it"] = "Seleziona, poi premi il tasto da tenere premuto per la ruota. Un tocco breve esegue l'azione normale del tasto (RB: Interagisci). LT è Luce sacra, quindi è sconsigliato.",
        ["pt-BR"] = "Selecione e aperte o botão que vai segurar para a roda. Um toque rápido faz a ação normal do botão (RB: Interagir). LT é Luz Sagrada, então não é uma boa escolha.",
        ["ru"] = "Выберите и нажмите кнопку, которую будете удерживать для колеса. Короткое нажатие выполняет обычное действие кнопки (RB: Взаимодействие). LT — это Благодатный огонь, поэтому выбирать её не стоит.",
        ["ja"] = "選択してから、ホイール用に押し続けるボタンを押します。短く押すとボタン本来の動作になります（RB：アクション）。LTはホーリーライトなのでおすすめしません。",
        ["ko"] = "선택한 다음 바퀴용으로 누르고 있을 버튼을 누르세요. 짧게 누르면 버튼의 원래 동작을 합니다(RB: 상호 작용). LT는 홀리 라이트이므로 추천하지 않습니다.",
        ["zh-CN"] = "选中后按下要按住的轮盘键。轻按仍会执行该键原本的动作(RB:交互)。LT 是神圣辉光,不建议使用。",
        ["zh-TW"] = "選中後按下要按住的輪盤鍵。輕按仍會執行該鍵原本的動作（RB：互動）。LT 是神聖之光，不建議使用。",
    };

    internal static readonly T WeaponButtonBindHint = new()
    {
        ["en"] = "Press a button, or hold the wheel button and press another (default RB + LB). LB alone never drinks a potion.",
        ["fr"] = "Appuyez sur un bouton, ou maintenez le bouton de la roue et appuyez sur un autre (par défaut RB + LB). LB seul ne boit jamais de potion.",
        ["de"] = "Drücke eine Taste oder halte die Radtaste und drücke eine weitere (Standard: RB + LB). LB allein trinkt nie einen Trank.",
        ["es"] = "Pulsa un botón, o mantén el botón de la rueda y pulsa otro (por defecto RB + LB). LB solo nunca bebe pociones.",
        ["it"] = "Premi un tasto, oppure tieni premuto il tasto della ruota e premine un altro (predefinito RB + LB). LB da solo non beve mai pozioni.",
        ["pt-BR"] = "Aperte um botão, ou segure o botão da roda e aperte outro (padrão RB + LB). LB sozinho nunca bebe poções.",
        ["ru"] = "Нажмите кнопку или удерживайте кнопку колеса и нажмите другую (по умолчанию RB + LB). LB отдельно никогда не пьёт зелья.",
        ["ja"] = "ボタンを押すか、ホイールボタンを押しながら別のボタンを押します（初期設定 RB + LB）。LB単独ではポーションを使いません。",
        ["ko"] = "버튼을 누르거나, 바퀴 버튼을 누른 채 다른 버튼을 누르세요(기본 RB + LB). LB 단독이면 물약을 마시지 않습니다.",
        ["zh-CN"] = "按下一个键,或按住轮盘键再按另一个键(默认 RB + LB)。单独使用 LB 时不会使用药水。",
        ["zh-TW"] = "按下一個鍵，或按住輪盤鍵再按另一個鍵（預設 RB + LB）。單獨使用 LB 時不會使用藥水。",
    };
}
