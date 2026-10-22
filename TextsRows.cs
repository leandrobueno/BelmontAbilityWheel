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

    internal static readonly T PinY = new()
    {
        ["en"] = "Pin Y spell",
        ["fr"] = "Garder le sort Y",
        ["de"] = "Y-Zauber festhalten",
        ["es"] = "Fijar hechizo Y",
        ["it"] = "Fissa incantesimo Y",
        ["pt-BR"] = "Fixar feitiço Y",
        ["ru"] = "Закрепить заклинание Y",
        ["ja"] = "Yのスペルを固定",
        ["ko"] = "Y 주문 고정",
        ["zh-CN"] = "固定 Y 键法术",
        ["zh-TW"] = "固定 Y 鍵法術",
    };

    internal static readonly T PinYHint = new()
    {
        ["en"] = "On: the spell equipped in the Tarot menu always stays on Y; a wheel spell is only equipped while it plays. Mirror Walk stays equipped after placing its mirror, until you cast it again (teleport), press Y, cast another wheel spell or save; Wheel of Chaos stays equipped while its wheels are out. Casting another spell makes the mirror or wheels vanish. Off: the last wheel spell stays equipped.",
        ["fr"] = "Activé : le sort équipé dans le menu Tarot reste sur Y ; un sort de la roue n'est équipé que pendant son effet. Passe-Miroir reste équipé après avoir posé son miroir, jusqu'à ce que vous le relanciez (téléportation), appuyiez sur Y, lanciez un autre sort de la roue ou sauvegardiez ; Roue du chaos reste équipé tant que ses roues sont là. Lancer un autre sort fait disparaître le miroir ou les roues. Désactivé : le dernier sort de la roue reste équipé.",
        ["de"] = "An: Der im Tarot-Menü ausgerüstete Zauber bleibt auf Y; ein Zauber vom Rad ist nur ausgerüstet, solange er wirkt. Spiegelpfad bleibt nach dem Platzieren des Spiegels ausgerüstet, bis du ihn erneut wirkst (Teleport), Y drückst, einen anderen Zauber vom Rad wirkst oder speicherst; Rad des Chaos bleibt ausgerüstet, solange seine Räder da sind. Ein anderer Zauber lässt Spiegel oder Räder verschwinden. Aus: Der letzte Zauber vom Rad bleibt ausgerüstet.",
        ["es"] = "Activado: el hechizo equipado en el menú de Tarot se queda en Y; un hechizo de la rueda solo se equipa mientras actúa. Viaje de espejo sigue equipado tras colocar su espejo, hasta que lo lances otra vez (teletransporte), pulses Y, lances otro hechizo de la rueda o guardes; Estrella del caos sigue equipado mientras sus ruedas estén fuera. Lanzar otro hechizo hace desaparecer el espejo o las ruedas. Desactivado: el último hechizo de la rueda queda equipado.",
        ["it"] = "Attivato: l'incantesimo equipaggiato nel menu Tarocchi resta su Y; un incantesimo della ruota è equipaggiato solo mentre agisce. Camminata specchiata resta equipaggiata dopo aver posato lo specchio, finché non la lanci di nuovo (teletrasporto), premi Y, lanci un altro incantesimo della ruota o salvi; Ruota del caos resta equipaggiata finché le sue ruote sono in campo. Lanciare un altro incantesimo fa sparire lo specchio o le ruote. Disattivato: l'ultimo incantesimo della ruota resta equipaggiato.",
        ["pt-BR"] = "Ativado: o feitiço equipado no menu de Tarô fica sempre no Y; um feitiço da roda só fica equipado enquanto age. Travessia do Espelho continua equipado depois de colocar o espelho, até você lançá-lo de novo (teletransporte), apertar Y, lançar outro feitiço da roda ou salvar; Roda do Caos continua equipado enquanto as rodas estiverem em campo. Lançar outro feitiço faz o espelho ou as rodas sumirem. Desativado: o último feitiço da roda fica equipado.",
        ["ru"] = "Вкл.: заклинание, выбранное в меню Таро, всегда остаётся на Y; заклинание с колеса выбрано, только пока действует. Зазеркалье остаётся выбранным после установки зеркала, пока вы не примените его снова (телепорт), не нажмёте Y, не примените другое заклинание с колеса или не сохранитесь; Колесо хаоса остаётся выбранным, пока его колёса на поле. Другое заклинание убирает зеркало или колёса. Выкл.: последнее заклинание с колеса остаётся выбранным.",
        ["ja"] = "オン：タロットメニューで装備したスペルは常にYに残り、ホイールのスペルは発動中だけ装備されます。鏡歩きは鏡を置いた後も、もう一度使う（テレポート）、Yを押す、別のホイールのスペルを使う、セーブするまで装備されたままです。混沌の車輪は車輪が出ている間装備されたままです。別のスペルを使うと鏡や車輪は消えます。オフ：最後に使ったホイールのスペルが装備されたままになります。",
        ["ko"] = "켜기: 타로 메뉴에서 장착한 주문은 항상 Y에 남고, 바퀴 주문은 발동하는 동안만 장착됩니다. 거울 보행은 거울을 놓은 뒤에도 다시 사용하거나(순간이동) Y를 누르거나 다른 바퀴 주문을 쓰거나 저장할 때까지 장착된 채로 남습니다. 혼돈의 수레바퀴는 바퀴가 나와 있는 동안 장착된 채로 남습니다. 다른 주문을 쓰면 거울이나 바퀴가 사라집니다. 끄기: 마지막으로 사용한 바퀴 주문이 장착된 채로 남습니다.",
        ["zh-CN"] = "开启:在塔罗菜单中装备的法术始终留在 Y 键;轮盘法术只在施放期间装备。镜行回溯放出镜子后会保持装备,直到再次施放(传送回镜子)、按 Y、施放其他轮盘法术或存档;混沌之轮在轮存在期间保持装备。使用其他法术会让镜子或轮消失。关闭:最后施放的轮盘法术保持装备。",
        ["zh-TW"] = "開啟：在塔羅牌選單中裝備的法術始終留在 Y 鍵，輪盤法術只在施放期間裝備。鏡子漫步放出鏡子後會保持裝備，直到再次使用（傳送回鏡子）、按 Y、使用其他輪盤法術或儲存，混沌車輪在輪子存在期間保持裝備。使用其他法術會讓鏡子或輪子消失。關閉：最後使用的輪盤法術保持裝備。",
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
