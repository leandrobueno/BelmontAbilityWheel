using T = System.Collections.Generic.Dictionary<string, string>;

namespace AbilityWheel;

// The page's title and summary, and the visual rows changed in 1.6.0.
internal static partial class Texts
{
    internal static readonly T Title = new()
    {
        ["en"] = "Ability and Equipment Wheel", ["fr"] = "Roue des capacités et de l'équipement",
        ["de"] = "Fähigkeiten- und Ausrüstungsrad", ["es"] = "Rueda de habilidades y equipo",
        ["it"] = "Ruota di abilità ed equipaggiamento", ["pt-BR"] = "Roda de habilidades e equipamento",
        ["ru"] = "Колесо умений и снаряжения", ["ja"] = "アビリティと装備のホイール", ["ko"] = "능력 및 장비 바퀴",
        ["zh-CN"] = "技能与装备轮盘", ["zh-TW"] = "技能與裝備輪盤",
    };

    internal static readonly T Summary = new()
    {
        ["en"] = "Cast up to 4 Tarot spells and switch between 4 weapons: hold the wheel button and press a slot's button.",
        ["fr"] = "Lancez jusqu'à 4 sorts du Tarot et changez parmi 4 armes : maintenez le bouton de la roue et appuyez sur le bouton d'un emplacement.",
        ["de"] = "Wirke bis zu 4 Tarot-Zauber und wechsle zwischen 4 Waffen: Halte die Radtaste und drücke die Taste eines Platzes.",
        ["es"] = "Lanza hasta 4 hechizos del Tarot y cambia entre 4 armas: mantén el botón de la rueda y pulsa el botón de una casilla.",
        ["it"] = "Lancia fino a 4 incantesimi dei Tarocchi e passa tra 4 armi: tieni premuto il tasto della ruota e premi il tasto di uno slot.",
        ["pt-BR"] = "Lance até 4 feitiços do Tarô e troque entre 4 armas: segure o botão da roda e aperte o botão de um espaço.",
        ["ru"] = "До 4 заклинаний Таро и 4 вида оружия: удерживайте кнопку колеса и нажмите кнопку ячейки.",
        ["ja"] = "タロットのスペル4つと武器4つを切り替えられます：ホイールボタンを押しながらスロットのボタンを押します。",
        ["ko"] = "타로 주문 4개와 무기 4개를 사용합니다: 바퀴 버튼을 누른 채 슬롯 버튼을 누르세요.",
        ["zh-CN"] = "快速施放 4 张塔罗牌的法术并切换 4 把武器:按住轮盘键再按对应位置的按键。",
        ["zh-TW"] = "快速使用 4 張塔羅牌的法術並切換 4 把武器：按住輪盤鍵再按對應位置的按鍵。",
    };

    internal static readonly T PinY = new()
    {
        ["en"] = "Pin equipped card", ["fr"] = "Garder la carte équipée", ["de"] = "Ausgerüstete Karte festhalten",
        ["es"] = "Fijar carta equipada", ["it"] = "Fissa carta equipaggiata", ["pt-BR"] = "Fixar carta equipada",
        ["ru"] = "Закрепить выбранную карту", ["ja"] = "装備中のカードを固定", ["ko"] = "장착한 카드 고정",
        ["zh-CN"] = "固定已装备的卡牌", ["zh-TW"] = "固定已裝備的卡牌",
    };

    internal static readonly T PinYHint = new()
    {
        ["en"] = "On: your Tarot-menu card stays equipped; wheel spells are only borrowed while they play. Off: the last one stays.",
        ["fr"] = "Activé : la carte du menu Tarot reste équipée ; les sorts de la roue ne sont empruntés que le temps de leur effet. Désactivé : le dernier reste.",
        ["de"] = "An: Die Karte aus dem Tarot-Menü bleibt ausgerüstet; Zauber vom Rad werden nur geliehen, solange sie wirken. Aus: Der letzte bleibt.",
        ["es"] = "Activado: la carta del menú de Tarot sigue equipada; los hechizos de la rueda solo se toman prestados mientras actúan. Desactivado: queda el último.",
        ["it"] = "Attivato: la carta del menu Tarocchi resta equipaggiata; gli incantesimi della ruota sono presi in prestito solo mentre agiscono. Disattivato: resta l'ultimo.",
        ["pt-BR"] = "Ativado: a carta do menu de Tarô continua equipada; feitiços da roda só são emprestados enquanto agem. Desativado: fica o último.",
        ["ru"] = "Вкл.: карта из меню Таро остаётся выбранной; заклинания с колеса берутся лишь на время действия. Выкл.: остаётся последнее.",
        ["ja"] = "オン：タロットメニューのカードを装備したまま、ホイールのスペルは発動中だけ借ります。オフ：最後に使ったスペルが残ります。",
        ["ko"] = "켜기: 타로 메뉴의 카드가 장착된 채로 남고, 바퀴 주문은 발동하는 동안만 빌립니다. 끄기: 마지막 주문이 남습니다.",
        ["zh-CN"] = "开启:塔罗菜单中的卡牌保持装备,轮盘法术只在施放期间借用。关闭:最后施放的法术保持装备。",
        ["zh-TW"] = "開啟：塔羅牌選單中的卡牌保持裝備，輪盤法術只在施放期間借用。關閉：最後使用的法術保持裝備。",
    };

    internal static readonly T ButtonLabelsHint = new()
    {
        ["en"] = "When each card shows its button (or its key after using the keyboard). Always shown while putting a card or weapon on the wheel.",
        ["fr"] = "Quand chaque carte affiche son bouton (ou sa touche après usage du clavier). Toujours affiché quand vous placez une carte ou une arme sur la roue.",
        ["de"] = "Wann jede Karte ihre Taste zeigt (nach Tastaturnutzung ihre Tastaturtaste). Beim Platzieren einer Karte oder Waffe auf dem Rad immer sichtbar.",
        ["es"] = "Cuándo muestra cada carta su botón (o su tecla tras usar el teclado). Siempre visible al poner una carta o un arma en la rueda.",
        ["it"] = "Quando ogni carta mostra il suo tasto (o il tasto della tastiera dopo averla usata). Sempre visibile quando metti una carta o un'arma sulla ruota.",
        ["pt-BR"] = "Quando cada carta mostra seu botão (ou sua tecla após usar o teclado). Sempre visível ao colocar uma carta ou arma na roda.",
        ["ru"] = "Когда карты показывают свои кнопки (или клавиши после использования клавиатуры). Всегда видны, когда вы назначаете карту или оружие на колесо.",
        ["ja"] = "各カードにボタン（キーボード使用後はキー）を表示するタイミング。カードや武器をホイールに入れる間は常に表示されます。",
        ["ko"] = "각 카드에 버튼(키보드 사용 후에는 키)을 표시하는 시점입니다. 카드나 무기를 바퀴에 넣는 동안에는 항상 표시됩니다.",
        ["zh-CN"] = "何时在卡牌上显示对应按键(使用键盘时为键盘按键)。放入轮盘时始终显示。",
        ["zh-TW"] = "何時在卡牌上顯示對應按鍵（使用鍵盤時為鍵盤按鍵）。放入輪盤時始終顯示。",
    };
}
