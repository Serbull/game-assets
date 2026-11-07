# Serbull Responsive UI 1.0.0

Две независимые раскладки Unity uGUI: Portrait и Landscape. Плагин выделен из общей системы Steal Egg / Lucky Castle и не зависит от игровых скриптов, SMP, YG, Zenject или сторонних инспекторов.

## Установка

Responsive UI входит в `com.serbull.gameassets` начиная с версии 1.3.0. Требования: Unity 6 и Unity UI 2.0.0 (зависимость пакета). Runtime находится в `Runtime/ResponsiveUI`, Editor — в `Editor/ResponsiveUI`.

Сохранены namespace `Serbull.ResponsiveUI`, имена сборок `Serbull.ResponsiveUI` / `Serbull.ResponsiveUI.Editor` и GUID скриптов исходного `Serbull.ResponsiveUI-1.0.0.unitypackage`.

### Переход с отдельного unitypackage

В проекте должна быть только одна копия Responsive UI. Сначала сохраните изменения и резервную копию локальных скриптов. Если они менялись, перенесите правки в общий модуль. Уберите `Assets/Serbull/ResponsiveUI` вместе с его `.meta` из импортируемой области и подключите Game Assets 1.3.0. Не пересоздавайте MonoBehaviour-компоненты и не меняйте GUID: существующие ссылки префабов и snapshot-данные должны разрешиться на скрипты пакета. После импорта проверьте компиляцию, отсутствие Missing Script и переключение раскладок.

### Редактирование префабов Game Assets

Для изменения префабов самого пакета подключайте локальный checkout `game-assets` через Package Manager → Add package from disk → `package.json` либо используйте embedded package. Git-кэш `Library/PackageCache` не редактируйте. Для изменений только в конкретной игре создавайте Prefab Variant в `Assets`.

Alternative layout добавляется вручную только нужным компонентам. Установка пакета сама не меняет префабы и не добавляет им варианты.

## Настройка раскладки

1. Вне Play Mode выберите UI-объект в сцене или Prefab Mode.
2. Под шапкой объекта в Inspector нажмите **Add alternative layout → Rect Transform**. Текущее состояние запишется в оба варианта.
3. Выберите Portrait через **Tools → Responsive UI → Set Portrait**, настройте обычный RectTransform.
4. Нажмите **F9**, настройте Landscape. Правки через Inspector и инструменты Scene View автоматически записываются в активный вариант.
5. Сохраните сцену или префаб. Переключите F9 ещё раз для проверки обеих раскладок.

Для GridLayoutGroup, LayoutElement, Image, TMP_Text, CanvasScaler и других компонентов добавьте их через тот же список или меню компонента **Add Alternative Layout**. Каждый ComponentLayoutVariant хранит варианты одного целевого компонента. Поддерживается выбор нескольких объектов.

Кнопки Portrait / Landscape в строке компонента **записывают текущее состояние в указанный слот**. Для переключения используйте верхнюю кнопку режима, F9 или меню Tools. Панель **Responsive Layout** также доступна в оверлеях Scene View.

Внутренние объекты prefab instance редактируйте в Prefab Mode исходного префаба. Кнопка Use prefab values сбрасывает локальные overrides выбранных snapshot-компонентов. Открытие сцены/префаба и перезагрузка скриптов сами не применяют другой вариант и не создают overrides.

## Во время игры

ScreenLayoutWatcher создаётся автоматически перед загрузкой первой сцены и переживает смену сцен. Добавлять его вручную не требуется. Ширина экрана больше высоты означает Landscape; остальные пропорции — Portrait. В Play Mode режим определяется Game View, ручной F9 отключён.

RectTransformVariant переключает якоря, pivot, позицию, размер, масштаб и поворот. ComponentLayoutVariant переключает поддерживаемые сериализованные поля компонента. Обычные anchors, CanvasScaler и Layout Groups продолжают работать; если родительская Layout Group управляет размером ребёнка, настройте саму группу или LayoutElement.

## API

```csharp
using Serbull.ResponsiveUI;

bool portrait = ScreenLayoutMode.IsPortrait;
// ScreenLayoutMode.Changed: событие Action<LayoutMode> при смене ориентации.
// ScreenLayoutMode.ResolutionChanged: Action при любом изменении размера экрана.
// Подписывайтесь в OnEnable и отписывайтесь в OnDisable.
```

Режим редактора сохраняется отдельно для каждого проекта в EditorPrefs.

## Границы поддержки

Сохраняется поведение исходной системы: снимки произвольных компонентов содержат поддерживаемые простые поля (числа, bool, string, enum, векторы, цвет, Rect). Ссылки на Unity-объекты, массивы, вложенные структуры и состав иерархии не копируются. Текст TMP (`m_text`) исключён, чтобы переключение не затирало локализацию и динамические подписи. Стандартный компонентный snapshot не является универсальным сериализатором любых пользовательских типов; в player не поддерживаются вложенные пути, например padding.left.

В player поля компонента записываются через reflection; для AspectRatioFitter используются публичные setters, чтобы обновлялась геометрия. `Runtime/ResponsiveUI/link.xml` сохраняет поля стандартных uGUI/TMP-типов. Editor-компонент `ResponsiveLinkerProcessor` передаёт этот файл UnityLinker при сборке, поскольку link.xml внутри UPM-пакета не подключается как обычный файл из Assets. Для своих компонентов при stripping сохраняйте отражаемые поля через собственный link.xml или Preserve и проверяйте реальный player. Настройки stripping проекта плагин не меняет.

## Существующие проекты с прежней системой

У этого пакета новый namespace и новые GUID. Он не заменяет автоматически EggGame.Responsive или LuckyCastle.Responsive и не переносит их snapshot-данные. Не используйте две редакторские версии одновременно: у них общий F9 и пункт контекстного меню. Для перевода существующей игры требуется отдельная миграция компонентов с сохранением двух раскладок и обновлением подписчиков. Импорт в новый проект не требует миграции.

## Состав

- Runtime: ScreenLayoutMode, ScreenLayoutWatcher, RectTransformVariant, ComponentLayoutVariant, LayoutVariantSerialization, LayoutVariantRuntimeWriter.
- Editor: ResponsiveEditorMode, ResponsiveAutoCapture, ResponsiveComponentHeader, ResponsiveSceneOverlay.
- Assembly definitions, link.xml и ResponsiveLinkerProcessor для UPM-сборок.
