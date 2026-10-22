using T = System.Collections.Generic.Dictionary<string, string>;

namespace AbilityWheel;

// Slot rows ({0}: the slot number, 1-4) and the merged wheels, new or changed in 1.6.0.
internal static partial class Texts
{
    internal static readonly T SlotKey = new()
    {
        ["en"] = "Keyboard key for spell slot {0}", ["fr"] = "Touche clavier de l'emplacement de sort {0}",
        ["de"] = "Tastaturtaste für Zauberplatz {0}", ["es"] = "Tecla de la casilla de hechizo {0}",
        ["it"] = "Tasto tastiera dello slot incantesimo {0}", ["pt-BR"] = "Tecla do espaço de feitiço {0}",
        ["ru"] = "Клавиша ячейки заклинания {0}", ["ja"] = "スペルスロット{0}のキー", ["ko"] = "주문 슬롯 {0} 키보드 키",
        ["zh-CN"] = "法术位置 {0} 的键盘按键", ["zh-TW"] = "法術位置 {0} 的鍵盤按鍵",
    };

    internal static readonly T WeaponKey = new()
    {
        ["en"] = "Keyboard key for weapon slot {0}", ["fr"] = "Touche clavier de l'emplacement d'arme {0}",
        ["de"] = "Tastaturtaste für Waffenplatz {0}", ["es"] = "Tecla de la casilla de arma {0}",
        ["it"] = "Tasto tastiera dello slot arma {0}", ["pt-BR"] = "Tecla do espaço de arma {0}",
        ["ru"] = "Клавиша ячейки оружия {0}", ["ja"] = "武器スロット{0}のキー", ["ko"] = "무기 슬롯 {0} 키보드 키",
        ["zh-CN"] = "武器位置 {0} 的键盘按键", ["zh-TW"] = "武器位置 {0} 的鍵盤按鍵",
    };

    internal static readonly T WeaponWheelHint = new()
    {
        ["en"] = "Hold the weapon button and press a slot's button to switch weapons. Add weapons in the Equipment menu with L3.",
        ["fr"] = "Maintenez le bouton d'armes et appuyez sur le bouton d'un emplacement pour changer d'arme. Ajoutez des armes avec L3 dans le menu Équipement.",
        ["de"] = "Halte die Waffentaste und drücke die Taste eines Platzes, um die Waffe zu wechseln. Waffen im Ausrüstungsmenü mit L3 hinzufügen.",
        ["es"] = "Mantén el botón de armas y pulsa el botón de una casilla para cambiar de arma. Añade armas con L3 en el menú Equipamiento.",
        ["it"] = "Tieni premuto il tasto armi e premi il tasto di uno slot per cambiare arma. Aggiungi armi con L3 nel menu Equipaggiamento.",
        ["pt-BR"] = "Segure o botão de armas e aperte o botão de um espaço para trocar de arma. Adicione armas com L3 no menu Equipamento.",
        ["ru"] = "Удерживайте кнопку оружия и нажмите кнопку ячейки, чтобы сменить оружие. Оружие добавляется в меню Инвентарь через L3.",
        ["ja"] = "武器ボタンを押しながらスロットのボタンで武器を切り替えます。武器は装備メニューでL3を押して追加します。",
        ["ko"] = "무기 버튼을 누른 채 슬롯 버튼을 누르면 무기를 전환합니다. 무기는 장비 메뉴에서 L3로 추가합니다.",
        ["zh-CN"] = "按住武器键再按对应位置的按键切换武器。在装备菜单中按 L3 放入武器。",
        ["zh-TW"] = "按住武器鍵再按對應位置的按鍵切換武器。在裝備選單中按 L3 加入武器。",
    };

    internal static readonly T SpellSlotButton = new()
    {
        ["en"] = "Spell slot {0} button", ["fr"] = "Bouton de l'emplacement de sort {0}", ["de"] = "Taste für Zauberplatz {0}",
        ["es"] = "Botón de la casilla de hechizo {0}", ["it"] = "Tasto dello slot incantesimo {0}",
        ["pt-BR"] = "Botão do espaço de feitiço {0}", ["ru"] = "Кнопка ячейки заклинания {0}", ["ja"] = "スペルスロット{0}のボタン",
        ["ko"] = "주문 슬롯 {0} 버튼", ["zh-CN"] = "法术位置 {0} 的按键", ["zh-TW"] = "法術位置 {0} 的按鍵",
    };

    internal static readonly T SpellSlotButtonHint = new()
    {
        ["en"] = "The gamepad button that casts this slot's spell while the wheel button is held. Each slot needs its own button.",
        ["fr"] = "Le bouton de la manette qui lance le sort de cet emplacement pendant que le bouton de la roue est maintenu. Chaque emplacement a besoin de son propre bouton.",
        ["de"] = "Die Controller-Taste, die den Zauber dieses Platzes wirkt, während die Radtaste gehalten wird. Jeder Platz braucht eine eigene Taste.",
        ["es"] = "El botón del mando que lanza el hechizo de esta casilla mientras mantienes el botón de la rueda. Cada casilla necesita su propio botón.",
        ["it"] = "Il tasto del controller che lancia l'incantesimo di questo slot mentre tieni premuto il tasto della ruota. Ogni slot ha bisogno di un tasto diverso.",
        ["pt-BR"] = "O botão do controle que lança o feitiço deste espaço enquanto o botão da roda está pressionado. Cada espaço precisa de um botão próprio.",
        ["ru"] = "Кнопка геймпада, которая применяет заклинание этой ячейки, пока удерживается кнопка колеса. У каждой ячейки должна быть своя кнопка.",
        ["ja"] = "ホイールボタンを押している間、このスロットのスペルを使うコントローラーのボタン。スロットごとに別のボタンが必要です。",
        ["ko"] = "바퀴 버튼을 누르고 있는 동안 이 슬롯의 주문을 사용하는 컨트롤러 버튼입니다. 슬롯마다 다른 버튼이 필요합니다.",
        ["zh-CN"] = "按住轮盘键时,施放此位置法术的手柄按键。每个位置需要不同的按键。",
        ["zh-TW"] = "按住輪盤鍵時，使用此位置法術的控制器按鍵。每個位置需要不同的按鍵。",
    };

    internal static readonly T WeaponSlotButton = new()
    {
        ["en"] = "Weapon slot {0} button", ["fr"] = "Bouton de l'emplacement d'arme {0}", ["de"] = "Taste für Waffenplatz {0}",
        ["es"] = "Botón de la casilla de arma {0}", ["it"] = "Tasto dello slot arma {0}", ["pt-BR"] = "Botão do espaço de arma {0}",
        ["ru"] = "Кнопка ячейки оружия {0}", ["ja"] = "武器スロット{0}のボタン", ["ko"] = "무기 슬롯 {0} 버튼",
        ["zh-CN"] = "武器位置 {0} 的按键", ["zh-TW"] = "武器位置 {0} 的按鍵",
    };

    internal static readonly T WeaponSlotButtonHint = new()
    {
        ["en"] = "The gamepad button that switches to this slot's weapon while the weapon wheel is open. Each slot needs its own button.",
        ["fr"] = "Le bouton de la manette qui prend l'arme de cet emplacement quand la roue des armes est ouverte. Chaque emplacement a besoin de son propre bouton.",
        ["de"] = "Die Controller-Taste, die zur Waffe dieses Platzes wechselt, während das Waffenrad offen ist. Jeder Platz braucht eine eigene Taste.",
        ["es"] = "El botón del mando que cambia al arma de esta casilla mientras la rueda de armas está abierta. Cada casilla necesita su propio botón.",
        ["it"] = "Il tasto del controller che passa all'arma di questo slot mentre la ruota delle armi è aperta. Ogni slot ha bisogno di un tasto diverso.",
        ["pt-BR"] = "O botão do controle que troca para a arma deste espaço enquanto a roda de armas está aberta. Cada espaço precisa de um botão próprio.",
        ["ru"] = "Кнопка геймпада, которая берёт оружие этой ячейки, пока колесо оружия открыто. У каждой ячейки должна быть своя кнопка.",
        ["ja"] = "武器ホイールが開いている間、このスロットの武器に切り替えるコントローラーのボタン。スロットごとに別のボタンが必要です。",
        ["ko"] = "무기 바퀴가 열려 있는 동안 이 슬롯의 무기로 전환하는 컨트롤러 버튼입니다. 슬롯마다 다른 버튼이 필요합니다.",
        ["zh-CN"] = "武器轮盘打开时,切换为此位置武器的手柄按键。每个位置需要不同的按键。",
        ["zh-TW"] = "武器輪盤開啟時，切換為此位置武器的控制器按鍵。每個位置需要不同的按鍵。",
    };

    internal static readonly T Merged = new()
    {
        ["en"] = "One button for both wheels", ["fr"] = "Un bouton pour les deux roues", ["de"] = "Eine Taste für beide Räder",
        ["es"] = "Un botón para ambas ruedas", ["it"] = "Un tasto per entrambe le ruote", ["pt-BR"] = "Um botão para as duas rodas",
        ["ru"] = "Одна кнопка для обоих колёс", ["ja"] = "両方のホイールを1つのボタンで", ["ko"] = "두 바퀴를 버튼 하나로",
        ["zh-CN"] = "一个键打开两个轮盘", ["zh-TW"] = "一個鍵開啟兩個輪盤",
    };

    internal static readonly T MergedHint = new()
    {
        ["en"] = "Holding the wheel button shows the spell and weapon wheels side by side, each slot on its own button (weapons on the D-pad by default). The weapon button doesn't apply.",
        ["fr"] = "Maintenir le bouton de la roue affiche côte à côte la roue des sorts et celle des armes, chaque emplacement sur son propre bouton (les armes sur la croix directionnelle par défaut). Le bouton d'armes ne s'applique pas.",
        ["de"] = "Beim Halten der Radtaste erscheinen Zauber- und Waffenrad nebeneinander, jeder Platz auf eigener Taste (Waffen standardmäßig auf dem Steuerkreuz). Die Waffentaste gilt dann nicht.",
        ["es"] = "Al mantener el botón de la rueda se muestran juntas la rueda de hechizos y la de armas, cada casilla en su propio botón (las armas en la cruceta por defecto). El botón de armas no se usa.",
        ["it"] = "Tenendo premuto il tasto della ruota compaiono affiancate la ruota degli incantesimi e quella delle armi, ogni slot sul proprio tasto (le armi sulla croce direzionale di base). Il tasto armi non si usa.",
        ["pt-BR"] = "Segurar o botão da roda mostra lado a lado a roda de feitiços e a de armas, cada espaço em seu próprio botão (as armas no direcional por padrão). O botão de armas não se aplica.",
        ["ru"] = "При удержании кнопки колеса колёса заклинаний и оружия видны рядом, у каждой ячейки своя кнопка (оружие по умолчанию на крестовине). Кнопка оружия не используется.",
        ["ja"] = "ホイールボタンを押すとスペルと武器のホイールが同時に表示され、スロットごとに別のボタンを使います（武器は初期設定で方向ボタン）。武器ボタンは使われません。",
        ["ko"] = "바퀴 버튼을 누르면 주문 바퀴와 무기 바퀴가 나란히 표시되며, 슬롯마다 다른 버튼을 사용합니다(무기는 기본으로 방향 버튼). 무기 버튼은 쓰이지 않습니다.",
        ["zh-CN"] = "按住轮盘键时,法术轮盘和武器轮盘并排显示,每个位置使用不同的按键(武器默认使用方向键)。武器键不再适用。",
        ["zh-TW"] = "按住輪盤鍵時，法術輪盤和武器輪盤並排顯示，每個位置使用不同的按鍵（武器預設使用方向鍵）。武器鍵不再適用。",
    };
}
