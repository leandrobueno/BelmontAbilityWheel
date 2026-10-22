using T = System.Collections.Generic.Dictionary<string, string>;

namespace AbilityWheel;

// Setting row texts, part 2: the HUD and the keyboard.
internal static partial class Texts
{
    internal static readonly T ButtonLabels = new()
    {
        ["en"] = "Button labels",
        ["fr"] = "Libellés des boutons",
        ["de"] = "Tastenbeschriftung",
        ["es"] = "Etiquetas de botones",
        ["it"] = "Etichette dei tasti",
        ["pt-BR"] = "Rótulos dos botões",
        ["ru"] = "Подписи кнопок",
        ["ja"] = "ボタン表示",
        ["ko"] = "버튼 표시",
        ["zh-CN"] = "按键标签",
        ["zh-TW"] = "按鍵標示",
    };

    internal static readonly T ButtonLabelsHint = new()
    {
        ["en"] = "When each card shows its button (Y/X/B/A, or its key after using the keyboard). Always shown while putting a card or weapon on the wheel.",
        ["fr"] = "Quand chaque carte affiche son bouton (Y/X/B/A, ou sa touche après usage du clavier). Toujours affiché quand vous placez une carte ou une arme sur la roue.",
        ["de"] = "Wann jede Karte ihre Taste zeigt (Y/X/B/A oder nach Tastaturnutzung ihre Taste). Beim Platzieren einer Karte oder Waffe auf dem Rad immer sichtbar.",
        ["es"] = "Cuándo muestra cada carta su botón (Y/X/B/A, o su tecla tras usar el teclado). Siempre visible al poner una carta o un arma en la rueda.",
        ["it"] = "Quando ogni carta mostra il suo tasto (Y/X/B/A, o il suo tasto della tastiera dopo averla usata). Sempre visibile quando metti una carta o un'arma sulla ruota.",
        ["pt-BR"] = "Quando cada carta mostra seu botão (Y/X/B/A, ou sua tecla após usar o teclado). Sempre visível ao colocar uma carta ou arma na roda.",
        ["ru"] = "Когда карты показывают свои кнопки (Y/X/B/A или клавиши после использования клавиатуры). Всегда видны, когда вы назначаете карту или оружие на колесо.",
        ["ja"] = "各カードにボタン（Y/X/B/A、キーボード使用後はキー）を表示するタイミング。カードや武器をホイールに入れる間は常に表示されます。",
        ["ko"] = "각 카드에 버튼(Y/X/B/A, 키보드 사용 후에는 키)을 표시하는 시점입니다. 카드나 무기를 바퀴에 넣는 동안에는 항상 표시됩니다.",
        ["zh-CN"] = "何时在卡牌上显示对应按键(Y/X/B/A,使用键盘时为键盘按键)。放入轮盘时始终显示。",
        ["zh-TW"] = "何時在卡牌上顯示對應按鍵（Y/X/B/A，使用鍵盤時為鍵盤按鍵）。放入輪盤時始終顯示。",
    };

    internal static readonly T FadeWithHud = new()
    {
        ["en"] = "Fade with game HUD",
        ["fr"] = "Masquer avec le HUD du jeu",
        ["de"] = "Mit Spiel-HUD ausblenden",
        ["es"] = "Ocultar con el HUD del juego",
        ["it"] = "Nascondi con l'HUD del gioco",
        ["pt-BR"] = "Ocultar com o HUD do jogo",
        ["ru"] = "Скрывать вместе с HUD игры",
        ["ja"] = "ゲームのHUDに合わせて隠す",
        ["ko"] = "게임 HUD와 함께 숨기기",
        ["zh-CN"] = "随游戏 HUD 隐藏",
        ["zh-TW"] = "隨遊戲 HUD 隱藏",
    };

    internal static readonly T FadeWithHudHint = new()
    {
        ["en"] = "Fade the cards out while the game's HUD is hidden, for example by HUD Auto Hide. Holding the wheel button or the Tarot menu always shows them.",
        ["fr"] = "Estompe les cartes quand le HUD du jeu est masqué, par exemple par HUD Auto Hide. Maintenir le bouton de la roue ou le menu Tarot les affiche toujours.",
        ["de"] = "Blendet die Karten aus, solange das Spiel-HUD verborgen ist, etwa durch HUD Auto Hide. Beim Halten der Radtaste und im Tarot-Menü sind sie immer sichtbar.",
        ["es"] = "Desvanece las cartas mientras el HUD del juego está oculto, por ejemplo con HUD Auto Hide. Mantener el botón de la rueda o el menú de Tarot siempre las muestra.",
        ["it"] = "Fa svanire le carte mentre l'HUD del gioco è nascosto, per esempio da HUD Auto Hide. Tenendo premuto il tasto della ruota o nel menu Tarocchi sono sempre visibili.",
        ["pt-BR"] = "Esmaece as cartas enquanto o HUD do jogo está oculto, por exemplo pelo HUD Auto Hide. Segurar o botão da roda ou o menu de Tarô sempre as mostra.",
        ["ru"] = "Карты исчезают, пока HUD игры скрыт, например модом HUD Auto Hide. При удержании кнопки колеса и в меню Таро они видны всегда.",
        ["ja"] = "ゲームのHUDが隠れている間（HUD Auto Hideなど）、カードもフェードアウトします。ホイールボタンを押している間やタロットメニューでは常に表示されます。",
        ["ko"] = "게임 HUD가 숨겨진 동안(예: HUD Auto Hide) 카드도 사라집니다. 바퀴 버튼을 누르고 있거나 타로 메뉴에서는 항상 표시됩니다.",
        ["zh-CN"] = "游戏 HUD 被隐藏时(例如使用 HUD Auto Hide),轮盘一同淡出。按住轮盘键或在塔罗菜单中时始终显示。",
        ["zh-TW"] = "遊戲 HUD 被隱藏時（例如使用 HUD Auto Hide），輪盤一同隱藏。按住輪盤鍵或在塔羅牌選單中時始終顯示。",
    };

    internal static readonly T Opacity = new()
    {
        ["en"] = "Wheel opacity", ["fr"] = "Opacité de la roue", ["de"] = "Deckkraft des Rads",
        ["es"] = "Opacidad de la rueda", ["it"] = "Opacità della ruota", ["pt-BR"] = "Opacidade da roda",
        ["ru"] = "Непрозрачность колеса", ["ja"] = "ホイールの不透明度", ["ko"] = "바퀴 불투명도",
        ["zh-CN"] = "轮盘不透明度", ["zh-TW"] = "輪盤不透明度",
    };

    internal static readonly T OpacityHint = new()
    {
        ["en"] = "How solid the wheel's tiles are.", ["fr"] = "Opacité des cases de la roue.",
        ["de"] = "Wie deckend die Felder des Rads sind.", ["es"] = "Cuán opacas son las casillas de la rueda.",
        ["it"] = "Quanto sono opache le caselle della ruota.", ["pt-BR"] = "O quanto as casas da roda são opacas.",
        ["ru"] = "Насколько непрозрачны ячейки колеса.", ["ja"] = "ホイールのマスの不透明さ。",
        ["ko"] = "바퀴 슬롯의 불투명한 정도입니다.", ["zh-CN"] = "轮盘卡牌的不透明程度。", ["zh-TW"] = "輪盤卡牌的不透明程度。",
    };

    internal static readonly T Size = new()
    {
        ["en"] = "Wheel size", ["fr"] = "Taille de la roue", ["de"] = "Größe des Rads", ["es"] = "Tamaño de la rueda",
        ["it"] = "Dimensione della ruota", ["pt-BR"] = "Tamanho da roda", ["ru"] = "Размер колеса",
        ["ja"] = "ホイールのサイズ", ["ko"] = "바퀴 크기", ["zh-CN"] = "轮盘大小", ["zh-TW"] = "輪盤大小",
    };

    internal static readonly T SizeHint = new()
    {
        ["en"] = "How big the wheel is.", ["fr"] = "Taille d'affichage de la roue.", ["de"] = "Wie groß das Rad ist.",
        ["es"] = "El tamaño de la rueda.", ["it"] = "Quanto è grande la ruota.", ["pt-BR"] = "O tamanho da roda.",
        ["ru"] = "Размер колеса на экране.", ["ja"] = "ホイールの表示サイズ。", ["ko"] = "바퀴의 표시 크기입니다.",
        ["zh-CN"] = "轮盘的显示大小。", ["zh-TW"] = "輪盤的顯示大小。",
    };

    internal static readonly T RightEdge = new()
    {
        ["en"] = "Distance from right edge", ["fr"] = "Distance du bord droit", ["de"] = "Abstand zum rechten Rand",
        ["es"] = "Distancia al borde derecho", ["it"] = "Distanza dal bordo destro", ["pt-BR"] = "Distância da borda direita",
        ["ru"] = "Отступ от правого края", ["ja"] = "右端からの距離", ["ko"] = "오른쪽 가장자리와의 거리",
        ["zh-CN"] = "距右边缘", ["zh-TW"] = "距右邊緣",
    };

    internal static readonly T BottomEdge = new()
    {
        ["en"] = "Distance from bottom edge", ["fr"] = "Distance du bord inférieur", ["de"] = "Abstand zum unteren Rand",
        ["es"] = "Distancia al borde inferior", ["it"] = "Distanza dal bordo inferiore", ["pt-BR"] = "Distância da borda inferior",
        ["ru"] = "Отступ от нижнего края", ["ja"] = "下端からの距離", ["ko"] = "아래쪽 가장자리와의 거리",
        ["zh-CN"] = "距下边缘", ["zh-TW"] = "距下邊緣",
    };

    internal static readonly T PixelsHint = new()
    {
        ["en"] = "In pixels at 1080p.", ["fr"] = "En pixels, en 1080p.", ["de"] = "In Pixeln bei 1080p.",
        ["es"] = "En píxeles a 1080p.", ["it"] = "In pixel a 1080p.", ["pt-BR"] = "Em pixels a 1080p.",
        ["ru"] = "В пикселях при 1080p.", ["ja"] = "1080p時のピクセル数。", ["ko"] = "1080p 기준 화소입니다.",
        ["zh-CN"] = "以 1080p 像素计。", ["zh-TW"] = "以 1080p 像素計。",
    };

    // {0}: the slot's button, Y/X/B/A.
    internal static readonly T SlotKey = new()
    {
        ["en"] = "Keyboard key for slot {0}", ["fr"] = "Touche clavier de l'emplacement {0}",
        ["de"] = "Tastaturtaste für Platz {0}", ["es"] = "Tecla para la casilla {0}", ["it"] = "Tasto tastiera dello slot {0}",
        ["pt-BR"] = "Tecla do espaço {0}", ["ru"] = "Клавиша для ячейки {0}", ["ja"] = "スロット{0}のキー",
        ["ko"] = "{0} 슬롯 키보드 키", ["zh-CN"] = "{0} 的键盘按键", ["zh-TW"] = "{0} 的鍵盤按鍵",
    };

    internal static readonly T SlotKeyHint = new()
    {
        ["en"] = "Casts that slot's spell at once; no hold needed on keyboard.",
        ["fr"] = "Lance aussitôt le sort de cet emplacement ; pas besoin de maintenir au clavier.",
        ["de"] = "Wirkt sofort den Zauber dieses Platzes; auf der Tastatur ohne Halten.",
        ["es"] = "Lanza al instante el hechizo de esa casilla; con teclado no hace falta mantener nada.",
        ["it"] = "Lancia subito l'incantesimo di quello slot; con la tastiera non serve tenere premuto nulla.",
        ["pt-BR"] = "Lança na hora o feitiço desse espaço; no teclado não é preciso segurar nada.",
        ["ru"] = "Сразу применяет заклинание этой ячейки; на клавиатуре ничего удерживать не нужно.",
        ["ja"] = "そのスロットのスペルをすぐに使います。キーボードでは長押し不要です。",
        ["ko"] = "해당 슬롯의 주문을 바로 사용합니다. 키보드에서는 누르고 있을 필요가 없습니다.",
        ["zh-CN"] = "按下即施放该位置的法术,键盘无需按住轮盘键。",
        ["zh-TW"] = "按下即使用該位置的法術，鍵盤無需按住輪盤鍵。",
    };

    internal static readonly T AssignKey = new()
    {
        ["en"] = "Add to wheel key (keyboard)", ["fr"] = "Touche Ajouter à la roue (clavier)",
        ["de"] = "Taste Zum Rad hinzufügen (Tastatur)", ["es"] = "Tecla Añadir a la rueda (teclado)",
        ["it"] = "Tasto Aggiungi alla ruota (tastiera)", ["pt-BR"] = "Tecla Adicionar à roda (teclado)",
        ["ru"] = "Клавиша добавления на колесо", ["ja"] = "ホイールに入れるキー（キーボード）",
        ["ko"] = "바퀴에 넣기 키(키보드)", ["zh-CN"] = "加入轮盘按键(键盘)", ["zh-TW"] = "加入輪盤按鍵（鍵盤）",
    };

    internal static readonly T AssignKeyHint = new()
    {
        ["en"] = "In the Tarot or Equipment menu, highlight a card or weapon, press this key, then a slot key to put it on the wheel. Gamepad: L3.",
        ["fr"] = "Dans le menu Tarot ou Équipement, sélectionnez une carte ou une arme, appuyez sur cette touche, puis sur la touche d'un emplacement pour la placer sur la roue. Manette : L3.",
        ["de"] = "Wähle im Menü Tarot oder Ausrüstung eine Karte oder Waffe, drücke diese Taste und dann die Taste eines Platzes, um sie aufs Rad zu legen. Controller: L3.",
        ["es"] = "En el menú Tarot o Equipamiento, selecciona una carta o un arma, pulsa esta tecla y luego la tecla de una casilla para ponerla en la rueda. Mando: L3.",
        ["it"] = "Nel menu Tarocchi o Equipaggiamento, seleziona una carta o un'arma, premi questo tasto e poi il tasto di uno slot per metterla sulla ruota. Controller: L3.",
        ["pt-BR"] = "No menu Tarô ou Equipamento, selecione uma carta ou arma, aperte esta tecla e depois a tecla de um espaço para colocá-la na roda. Controle: L3.",
        ["ru"] = "В меню Таро или Инвентарь выберите карту или оружие, нажмите эту клавишу, затем клавишу ячейки, чтобы поместить их на колесо. Геймпад: L3.",
        ["ja"] = "タロットまたは装備メニューでカードか武器を選び、このキー、次にスロットのキーを押すとホイールに入ります。コントローラー：L3。",
        ["ko"] = "타로 또는 장비 메뉴에서 카드나 무기를 고르고 이 키를 누른 뒤 슬롯 키를 누르면 바퀴에 넣습니다. 컨트롤러: L3.",
        ["zh-CN"] = "在塔罗菜单或装备菜单中选中一张卡牌或一把武器,按此键后再按对应的键盘按键即可放入轮盘。手柄使用 L3。",
        ["zh-TW"] = "在塔羅牌或裝備選單中選中一張卡牌或一把武器，按此鍵後再按對應的鍵盤按鍵即可放入輪盤。控制器使用 L3。",
    };
}
