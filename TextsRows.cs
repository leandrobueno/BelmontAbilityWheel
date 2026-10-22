using T = System.Collections.Generic.Dictionary<string, string>;

namespace AbilityWheel;

// Setting row texts, part 1: the spell wheel. Spell names are the game's (Items_Spells),
// Interact and Holy Light its input names.
internal static partial class Texts
{
    internal static readonly T WheelButton = new()
    {
        ["en"] = "Wheel button (gamepad)",
        ["fr"] = "Bouton de la roue (manette)",
        ["de"] = "Radtaste (Controller)",
        ["es"] = "Botón de la rueda (mando)",
        ["it"] = "Tasto della ruota (controller)",
        ["pt-BR"] = "Botão da roda (controle)",
        ["ru"] = "Кнопка колеса (геймпад)",
        ["ja"] = "ホイールボタン（コントローラー）",
        ["ko"] = "바퀴 버튼(컨트롤러)",
        ["zh-CN"] = "轮盘键(手柄)",
        ["zh-TW"] = "輪盤鍵（控制器）",
    };

    internal static readonly T WheelButtonHint = new()
    {
        ["en"] = "Hold it and press Y/X/B/A to cast that slot's spell. A quick tap still does the button's normal action (RB: Interact). LT is Holy Light, so it's a poor choice.",
        ["fr"] = "Maintenez-le et appuyez sur Y/X/B/A pour lancer le sort de cet emplacement. Un appui bref garde l'action normale du bouton (RB : Interagir). LT est Lumière Sacrée, mieux vaut l'éviter.",
        ["de"] = "Halte sie und drücke Y/X/B/A, um den Zauber dieses Platzes zu wirken. Kurz antippen behält die normale Aktion (RB: Interagieren). LT ist Heiliges Licht, daher ungeeignet.",
        ["es"] = "Mantenlo y pulsa Y/X/B/A para lanzar el hechizo de esa casilla. Una pulsación corta hace la acción normal del botón (RB: Interactuar). LT es Luz sagrada, así que no conviene.",
        ["it"] = "Tienilo premuto e premi Y/X/B/A per lanciare l'incantesimo di quello slot. Un tocco breve esegue l'azione normale del tasto (RB: Interagisci). LT è Luce sacra, quindi è sconsigliato.",
        ["pt-BR"] = "Segure-o e aperte Y/X/B/A para lançar o feitiço desse espaço. Um toque rápido faz a ação normal do botão (RB: Interagir). LT é Luz Sagrada, então não é uma boa escolha.",
        ["ru"] = "Удерживайте её и нажмите Y/X/B/A, чтобы применить заклинание этой ячейки. Короткое нажатие выполняет обычное действие кнопки (RB: Взаимодействие). LT — это Благодатный огонь, поэтому выбирать её не стоит.",
        ["ja"] = "押しながらY/X/B/Aでそのスロットのスペルを使用します。短く押すとボタン本来の動作になります（RB：アクション）。LTはホーリーライトなのでおすすめしません。",
        ["ko"] = "누른 채 Y/X/B/A를 누르면 해당 슬롯의 주문을 사용합니다. 짧게 누르면 버튼의 원래 동작을 합니다(RB: 상호 작용). LT는 홀리 라이트이므로 추천하지 않습니다.",
        ["zh-CN"] = "按住此键并按 Y/X/B/A 施放对应位置的法术。轻按仍会执行该键原本的动作(RB:交互)。LT 是神圣辉光,不建议使用。",
        ["zh-TW"] = "按住此鍵並按 Y/X/B/A 使用對應位置的法術。輕按仍會執行該鍵原本的動作（RB：互動）。LT 是神聖之光，不建議使用。",
    };

    internal static readonly T ShowWheel = new()
    {
        ["en"] = "Show wheel",
        ["fr"] = "Afficher la roue",
        ["de"] = "Rad anzeigen",
        ["es"] = "Mostrar rueda",
        ["it"] = "Mostra ruota",
        ["pt-BR"] = "Mostrar roda",
        ["ru"] = "Показ колеса",
        ["ja"] = "ホイール表示",
        ["ko"] = "바퀴 표시",
        ["zh-CN"] = "轮盘显示",
        ["zh-TW"] = "輪盤顯示",
    };

    internal static readonly T ShowWheelHint = new()
    {
        ["en"] = "When the 4 cards show in the bottom-right corner during gameplay. Always shown in the Tarot menu.",
        ["fr"] = "Quand les 4 cartes s'affichent en bas à droite pendant le jeu. Toujours affichées dans le menu Tarot.",
        ["de"] = "Wann die 4 Karten im Spiel unten rechts erscheinen. Im Tarot-Menü immer sichtbar.",
        ["es"] = "Cuándo se muestran las 4 cartas abajo a la derecha durante el juego. Siempre visibles en el menú de Tarot.",
        ["it"] = "Quando le 4 carte compaiono in basso a destra durante il gioco. Sempre visibili nel menu Tarocchi.",
        ["pt-BR"] = "Quando as 4 cartas aparecem no canto inferior direito durante o jogo. Sempre visíveis no menu de Tarô.",
        ["ru"] = "Когда 4 карты видны в правом нижнем углу во время игры. В меню Таро видны всегда.",
        ["ja"] = "ゲーム中に右下へ4枚のカードを表示するタイミング。タロットメニューでは常に表示されます。",
        ["ko"] = "게임 중 오른쪽 아래에 카드 4장을 표시하는 시점입니다. 타로 메뉴에서는 항상 표시됩니다.",
        ["zh-CN"] = "游戏中何时在右下角显示 4 张卡牌。塔罗菜单中始终显示。",
        ["zh-TW"] = "遊戲中何時在右下角顯示 4 張卡牌。塔羅牌選單中始終顯示。",
    };

    // Choices of Show wheel and Button labels.
    internal static readonly T Always = new()
    {
        ["en"] = "Always", ["fr"] = "Toujours", ["de"] = "Immer", ["es"] = "Siempre", ["it"] = "Sempre",
        ["pt-BR"] = "Sempre", ["ru"] = "Всегда", ["ja"] = "常に", ["ko"] = "항상", ["zh-CN"] = "始终", ["zh-TW"] = "始終",
    };

    internal static readonly T WhileHeld = new()
    {
        ["en"] = "While holding the wheel button",
        ["fr"] = "En maintenant le bouton de la roue",
        ["de"] = "Beim Halten der Radtaste",
        ["es"] = "Al mantener el botón de la rueda",
        ["it"] = "Tenendo premuto il tasto della ruota",
        ["pt-BR"] = "Ao segurar o botão da roda",
        ["ru"] = "При удержании кнопки колеса",
        ["ja"] = "ホイールボタンを押している間",
        ["ko"] = "바퀴 버튼을 누르는 동안",
        ["zh-CN"] = "按住轮盘键时",
        ["zh-TW"] = "按住輪盤鍵時",
    };

    internal static readonly T Never = new()
    {
        ["en"] = "Never", ["fr"] = "Jamais", ["de"] = "Nie", ["es"] = "Nunca", ["it"] = "Mai",
        ["pt-BR"] = "Nunca", ["ru"] = "Никогда", ["ja"] = "表示しない", ["ko"] = "안 함", ["zh-CN"] = "从不", ["zh-TW"] = "從不",
    };
}
