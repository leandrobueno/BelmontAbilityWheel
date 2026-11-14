using T = System.Collections.Generic.Dictionary<string, string>;

namespace AbilityWheel;

// The rows new in 1.7.0: swapped wheels, slots without holding, select instead of cast, and the
// key rows' lines now that any key can be bound.
internal static partial class Texts
{
    // Two sentences as one hint, in every language.
    internal static T Join(T first, T second)
    {
        var result = new T();
        foreach (var pair in first)
            result[pair.Key] = second.TryGetValue(pair.Key, out var more) ? pair.Value + " " + more : pair.Value;
        return result;
    }

    internal static readonly T SwapWheels = new()
    {
        ["en"] = "Swap the wheels", ["fr"] = "Inverser les roues", ["de"] = "Räder tauschen",
        ["es"] = "Intercambiar las ruedas", ["it"] = "Scambia le ruote", ["pt-BR"] = "Trocar as rodas",
        ["ru"] = "Поменять колёса местами", ["ja"] = "ホイールを入れ替える", ["ko"] = "바퀴 서로 바꾸기",
        ["zh-CN"] = "交换两个轮盘", ["zh-TW"] = "交換兩個輪盤",
    };

    internal static readonly T SwapWheelsHint = new()
    {
        ["en"] = "On: the wheel button alone opens the weapon wheel, and the weapon button (default RB + LB) opens the spell wheel. Needs a weapon on the wheel.",
        ["fr"] = "Activé : le bouton de la roue seul ouvre la roue des armes, et le bouton d'armes (par défaut RB + LB) ouvre la roue des sorts. Il faut une arme sur la roue.",
        ["de"] = "An: Die Radtaste allein öffnet das Waffenrad, die Waffentaste (Standard: RB + LB) das Zauberrad. Dafür muss eine Waffe auf dem Rad liegen.",
        ["es"] = "Activado: el botón de la rueda solo abre la rueda de armas, y el botón de armas (por defecto RB + LB) abre la rueda de hechizos. Hace falta un arma en la rueda.",
        ["it"] = "Attivato: il tasto della ruota da solo apre la ruota delle armi, e il tasto armi (predefinito RB + LB) apre la ruota degli incantesimi. Serve un'arma sulla ruota.",
        ["pt-BR"] = "Ativado: o botão da roda sozinho abre a roda de armas, e o botão de armas (padrão RB + LB) abre a roda de feitiços. É preciso ter uma arma na roda.",
        ["ru"] = "Вкл.: кнопка колеса сама открывает колесо оружия, а кнопка оружия (по умолчанию RB + LB) — колесо заклинаний. На колесе должно быть оружие.",
        ["ja"] = "オン：ホイールボタンだけで武器のホイール、武器ボタン（初期設定 RB + LB）でスペルのホイールを開きます。ホイールに武器が必要です。",
        ["ko"] = "켜기: 바퀴 버튼만 누르면 무기 바퀴가, 무기 버튼(기본 RB + LB)을 누르면 주문 바퀴가 열립니다. 바퀴에 무기가 있어야 합니다.",
        ["zh-CN"] = "开启:单按轮盘键打开武器轮盘,武器键(默认 RB + LB)打开法术轮盘。轮盘中需要有武器。",
        ["zh-TW"] = "開啟：單按輪盤鍵開啟武器輪盤，武器鍵（預設 RB + LB）開啟法術輪盤。輪盤中需要有武器。",
    };

    internal static readonly T NoHold = new()
    {
        ["en"] = "Slots without holding", ["fr"] = "Emplacements sans maintenir", ["de"] = "Plätze ohne Halten",
        ["es"] = "Casillas sin mantener", ["it"] = "Slot senza tenere premuto", ["pt-BR"] = "Espaços sem segurar",
        ["ru"] = "Ячейки без удержания", ["ja"] = "長押しなしでスロットを使う", ["ko"] = "누르지 않고 슬롯 사용",
        ["zh-CN"] = "无需按住即可使用", ["zh-TW"] = "無需按住即可使用",
    };

    internal static readonly T NoHoldHint = new()
    {
        ["en"] = "Slot buttons on the D-pad or the right stick work without holding the wheel button. The D-pad then no longer moves Rose, and the right stick no longer moves the camera.",
        ["fr"] = "Les boutons d'emplacement placés sur la croix directionnelle ou le stick droit fonctionnent sans maintenir le bouton de la roue. La croix ne déplace alors plus Rose, et le stick droit ne déplace plus la caméra.",
        ["de"] = "Platztasten auf dem Steuerkreuz oder dem rechten Stick funktionieren, ohne die Radtaste zu halten. Das Steuerkreuz bewegt Rose dann nicht mehr, der rechte Stick nicht mehr die Kamera.",
        ["es"] = "Los botones de casilla de la cruceta o del stick derecho funcionan sin mantener el botón de la rueda. La cruceta ya no mueve a Rose, y el stick derecho ya no mueve la cámara.",
        ["it"] = "I tasti degli slot sulla croce direzionale o sulla levetta destra funzionano senza tenere premuto il tasto della ruota. La croce non muove più Rose e la levetta destra non muove più la telecamera.",
        ["pt-BR"] = "Os botões de espaço no direcional ou no analógico direito funcionam sem segurar o botão da roda. O direcional deixa de mover Rose, e o analógico direito deixa de mover a câmera.",
        ["ru"] = "Кнопки ячеек на крестовине или правом стике работают без удержания кнопки колеса. Крестовина при этом больше не управляет движением, а правый стик — камерой.",
        ["ja"] = "方向ボタンや右スティックに割り当てたスロットは、ホイールボタンを押さなくても使えます。その場合、方向ボタンでの移動と右スティックでのカメラ操作はできなくなります。",
        ["ko"] = "방향 버튼이나 오른쪽 아날로그에 지정한 슬롯은 바퀴 버튼을 누르지 않아도 작동합니다. 이 경우 방향 버튼으로 이동하거나 오른쪽 아날로그로 카메라를 움직일 수 없습니다.",
        ["zh-CN"] = "设在方向键或右摇杆上的位置按键无需按住轮盘键即可使用。此时方向键不再控制移动,右摇杆不再控制镜头。",
        ["zh-TW"] = "設在方向鍵或右搖桿上的位置按鍵無需按住輪盤鍵即可使用。此時方向鍵不再控制移動，右搖桿不再控制鏡頭。",
    };

    internal static readonly T SelectOnly = new()
    {
        ["en"] = "Select instead of cast", ["fr"] = "Sélectionner au lieu de lancer", ["de"] = "Auswählen statt wirken",
        ["es"] = "Seleccionar en vez de lanzar", ["it"] = "Seleziona invece di lanciare",
        ["pt-BR"] = "Selecionar em vez de lançar", ["ru"] = "Выбирать, а не применять", ["ja"] = "使用せずに選択だけする",
        ["ko"] = "사용하지 않고 선택만 하기", ["zh-CN"] = "只选择,不施放", ["zh-TW"] = "只選擇，不使用",
    };

    internal static readonly T SelectOnlyHint = new()
    {
        ["en"] = "On: a spell slot's button only equips its card, and your spell button casts it. Off: the slot's button casts it at once.",
        ["fr"] = "Activé : le bouton d'un emplacement de sort équipe seulement sa carte, et votre bouton de sort la lance. Désactivé : le bouton de l'emplacement la lance aussitôt.",
        ["de"] = "An: Die Taste eines Zauberplatzes rüstet seine Karte nur aus, und deine Zaubertaste wirkt sie. Aus: Die Platztaste wirkt sie sofort.",
        ["es"] = "Activado: el botón de una casilla de hechizo solo equipa su carta, y tu botón de hechizo la lanza. Desactivado: el botón de la casilla la lanza al instante.",
        ["it"] = "Attivato: il tasto di uno slot incantesimo equipaggia soltanto la sua carta, e il tuo tasto incantesimo la lancia. Disattivato: il tasto dello slot la lancia subito.",
        ["pt-BR"] = "Ativado: o botão de um espaço de feitiço só equipa a carta, e o seu botão de feitiço a lança. Desativado: o botão do espaço a lança na hora.",
        ["ru"] = "Вкл.: кнопка ячейки заклинания только выбирает карту, а применяет её ваша кнопка заклинания. Выкл.: кнопка ячейки сразу применяет заклинание.",
        ["ja"] = "オン：スペルスロットのボタンはカードを装備するだけで、スペルボタンで使用します。オフ：スロットのボタンですぐに使用します。",
        ["ko"] = "켜기: 주문 슬롯 버튼은 카드를 장착하기만 하고, 주문 버튼으로 사용합니다. 끄기: 슬롯 버튼으로 바로 사용합니다.",
        ["zh-CN"] = "开启:法术位置的按键只装备对应卡牌,再用法术键施放。关闭:按下位置按键即施放。",
        ["zh-TW"] = "開啟：法術位置的按鍵只裝備對應卡牌，再用法術鍵使用。關閉：按下位置按鍵即使用。",
    };

    // New in 1.8.0.
    internal static readonly T TapToSwitch = new()
    {
        ["en"] = "Tap to switch wheels", ["fr"] = "Changer de roue d'un appui", ["de"] = "Rad per Tastendruck wechseln",
        ["es"] = "Cambiar de rueda con un toque", ["it"] = "Cambia ruota con un tocco",
        ["pt-BR"] = "Trocar de roda com um toque", ["ru"] = "Смена колеса нажатием", ["ja"] = "ボタンを押してホイール切り替え",
        ["ko"] = "버튼을 눌러 바퀴 전환", ["zh-CN"] = "轻按切换轮盘", ["zh-TW"] = "輕按切換輪盤",
    };

    internal static readonly T TapToSwitchHint = new()
    {
        ["en"] = "On: while holding the wheel button, tap the combination's second button (LB) to turn to the other wheel; it stays until the next tap. Off: hold both.",
        ["fr"] = "Activé : en maintenant le bouton de la roue, un appui sur le second bouton de la combinaison (LB) passe à l'autre roue, qui reste jusqu'au prochain appui. Désactivé : maintenez les deux.",
        ["de"] = "An: Bei gehaltener Radtaste wechselt ein Druck auf die zweite Taste der Kombination (LB) zum anderen Rad; es bleibt bis zum nächsten Druck. Aus: beide halten.",
        ["es"] = "Activado: con el botón de la rueda mantenido, una pulsación del segundo botón de la combinación (LB) cambia a la otra rueda, que se queda hasta la siguiente pulsación. Desactivado: mantén los dos.",
        ["it"] = "Attivato: tenendo premuto il tasto della ruota, un tocco del secondo tasto della combinazione (LB) passa all'altra ruota, che resta fino al tocco successivo. Disattivato: tienili premuti entrambi.",
        ["pt-BR"] = "Ativado: segurando o botão da roda, um toque no segundo botão da combinação (LB) troca para a outra roda, que fica até o próximo toque. Desativado: segure os dois.",
        ["ru"] = "Вкл.: при удержании кнопки колеса нажатие второй кнопки комбинации (LB) переключает на другое колесо, и оно остаётся до следующего нажатия. Выкл.: удерживайте обе.",
        ["ja"] = "オン：ホイールボタンを押している間、組み合わせの2つ目のボタン（LB）を押すともう一方のホイールに切り替わり、次に押すまでそのままです。オフ：両方を押し続けます。",
        ["ko"] = "켜기: 바퀴 버튼을 누른 채 조합의 두 번째 버튼(LB)을 누르면 다른 바퀴로 바뀌고, 다시 누를 때까지 유지됩니다. 끄기: 두 버튼을 모두 누르고 있습니다.",
        ["zh-CN"] = "开启:按住轮盘键时,按一下组合中的第二个键(LB)即可切换到另一个轮盘,并保持到下次按下。关闭:需同时按住两个键。",
        ["zh-TW"] = "開啟：按住輪盤鍵時，按一下組合中的第二個鍵（LB）即可切換到另一個輪盤，並保持到下次按下。關閉：需同時按住兩個鍵。",
    };

    // After a slot key's own hint, on the press-to-bind row.
    internal static readonly T AnyKey = new()
    {
        ["en"] = "Select, then press any key. A key the game also uses does both.",
        ["fr"] = "Sélectionnez, puis appuyez sur n'importe quelle touche. Une touche que le jeu utilise aussi fait les deux.",
        ["de"] = "Auswählen, dann eine beliebige Taste drücken. Eine Taste, die auch das Spiel nutzt, macht beides.",
        ["es"] = "Selecciona y pulsa cualquier tecla. Una tecla que el juego también usa hace ambas cosas.",
        ["it"] = "Seleziona, poi premi un tasto qualsiasi. Un tasto usato anche dal gioco fa entrambe le cose.",
        ["pt-BR"] = "Selecione e aperte qualquer tecla. Uma tecla que o jogo também usa faz as duas coisas.",
        ["ru"] = "Выберите и нажмите любую клавишу. Клавиша, занятая игрой, выполнит оба действия.",
        ["ja"] = "選択してから好きなキーを押します。ゲームでも使うキーは両方の動作をします。",
        ["ko"] = "선택한 다음 원하는 키를 누르세요. 게임에서도 쓰는 키는 두 동작을 모두 합니다.",
        ["zh-CN"] = "选中后按下任意键。游戏本身也使用的按键会同时执行两种动作。",
        ["zh-TW"] = "選中後按下任意鍵。遊戲本身也使用的按鍵會同時執行兩種動作。",
    };

    // After the Add to wheel key's own hint.
    internal static readonly T AnyMenuKey = new()
    {
        ["en"] = "Any key the menus don't use themselves.",
        ["fr"] = "N'importe quelle touche que les menus n'utilisent pas.",
        ["de"] = "Jede Taste, die die Menüs nicht selbst nutzen.",
        ["es"] = "Cualquier tecla que los menús no usen.",
        ["it"] = "Qualsiasi tasto non usato dai menu.",
        ["pt-BR"] = "Qualquer tecla que os menus não usem.",
        ["ru"] = "Любая клавиша, не занятая в меню.",
        ["ja"] = "メニューで使われていないキーならどれでも使えます。",
        ["ko"] = "메뉴에서 쓰지 않는 키라면 무엇이든 됩니다.",
        ["zh-CN"] = "可使用菜单未占用的任意键。",
        ["zh-TW"] = "可使用選單未使用的任意鍵。",
    };
}
