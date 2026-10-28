# Reading Hearthstone Logs

This guide teaches you how to read Hearthstone logs to understand game events and correlate them with our plugin's logs.

## Log File Locations

### Hearthstone Game Logs
```
C:\Program Files (x86)\Hearthstone\Logs\
```

### Plugin Logs (created by our plugin)
```
%AppData%\HearthstoneDeckTracker\Logs\BGActionTracker.log
```

---

## Key Log Files

| File | Purpose |
|------|---------|
| `Power_old.log` | Main game events - **most useful** for tracking actions |
| `Hearthstone.log` | Application-level events (startup, errors) |
| `LoadingScreen.log` | Screen transitions, game mode info |
| `GameNetLogger.log` | Connection and matchmaking |

**The main file you want is `Power_old.log`**

---

## Log Entry Format

Each line in Power_old.log follows this pattern:

```
D HH:MM:SS.microseconds GameState.DebugPrintPower() - <content>
```

- `D` = Debug level
- `HH:MM:SS.microseconds` = Timestamp
- `GameState.DebugPrintPower()` = Source of the log
- `<content>` = The actual event

Example:
```
D 20:47:19.2854107 GameState.DebugPrintPower() - TAG_CHANGE Entity=GameEntity tag=STEP value=MAIN_READY
```

---

## Finding Match Boundaries

### Start of a Match

Search for:
```
GameMgr.OnGameSetup()
```
or
```
SERVER_GAME_STARTED event - Loading Gameplay Scene
```

This appears in `Hearthstone.log`.

### Game Mode (Battlegrounds)

Search for:
```
gameMode=Battlegrounds
```

### End of a Match

Look for disconnect or:
```
GAME_SAVE_SUBKEY
```
or
```
Tracert: failed to add ICMP rule
```

---

## Game Phase Checkpoints

### Phase Indicators (STEP tag on GameEntity)

| Step Value | Meaning |
|------------|---------|
| `BEGIN_MULLIGAN` | Mulligan phase |
| `MAIN_READY` | Player turn starting |
| `MAIN_START` | Main phase begins |
| `MAIN_ACTION` | Action being taken |
| `MAIN_END` | Main phase ending |
| `MAIN_CLEANUP` | Cleanup phase |
| `MAIN_NEXT` | Transition to next step |

Example:
```
TAG_CHANGE Entity=GameEntity tag=STEP value=MAIN_READY
```

### Turn Counter

```
TAG_CHANGE Entity=GameEntity tag=TURN value=1
```

The turn number increments when the player gains control.

### Current Player

```
TAG_CHANGE Entity=SLOSK#1451 tag=CURRENT_PLAYER value=1
```

`value=1` means this player has control. `value=0` means they don't.

---

## Tracking Shop Phase vs Combat Phase

### Shop Phase

Shop phase is when `STEP = MAIN_READY` or `MAIN_START` and you see:
- Minion purchases (from shop)
- Refresh button interactions
- Tavern spells

### Combat Phase

Combat begins with:
```
tag=START_OF_COMBAT value=1
```

You'll also see:
```
TAG_CHANGE Entity=GameEntity tag=STEP value=MAIN_ACTION
```

---

## Tracking Card Actions

### Card Played (Minion, Spell, Hero Power)

Look for BLOCK_START with BlockType=POWER:

```
BLOCK_START BlockType=POWER Entity=[entityName=Fortify id=390 zone=PLAY zonePos=4 cardId=BG28_503 player=14]
```

### Card Moved to Hand

```
SHOW_ENTITY - Updating Entity=243 CardID=BG28_503
    tag=ZONE value=HAND
```

### Card Moved to Graveyard (Discarded/Sold)

```
TAG_CHANGE Entity=243 tag=ZONE value=GRAVEYARD
```

### Card Type Tags

| CardType | Meaning |
|----------|---------|
| `MINION` | A minion |
| `SPELL` | A spell |
| `HERO_POWER` | Hero power |
| `ENCHANTMENT` | Buff/enchantment |
| `MOVE_MINION_HOVER_TARGET` | UI element |

---

## Example: Finding When You Played a Minion

1. Search for `BLOCK_START BlockType=POWER`
2. Find entries with `cardId=BG...` (Battlegrounds cards)
3. Check `CARDTYPE value=MINION`
4. Note the timestamp

Example:
```
D 20:47:29.2354004 PowerTaskList.DebugPrintPower() - BLOCK_START BlockType=POWER Entity=[entityName=Fortify id=390 zone=PLAY zonePos=4 cardId=BG28_503 player=14]
```

---

## Correlating Plugin Logs with Game Logs

### Your Plugin Log

```
[14:30:45.123] OnCardPlayed: Id=BG28_503, Name=Fortify, Type=Minion, TechLevel=4, Cost=3
```

### Game Log

Search for `BG28_503` in Power_old.log:

```
...cardId=BG28_503 player=14]
```

The `player=14` means it's the friendly player (usually player 6 is friendly, 14 is enemy).

---

## Quick Reference: Search Patterns

| What to Find | Search For |
|--------------|------------|
| Your cards played | `player=6` + `BLOCK_START BlockType=POWER` |
| Your minions | `player=6` + `CARDTYPE value=MINION` |
| Spells | `CARDTYPE value=SPELL` |
| Turn start | `tag=TURN value=` |
| Shop phase | `STEP value=MAIN_READY` |
| Combat | `START_OF_COMBAT` |
| Card sold | `ZONE value=GRAVEYARD` + `zone=HAND` in earlier entry |
| Specific card | The card ID (e.g., `BG28_503`) |

---

## Tips

1. **Use timestamps** - Both logs have millisecond timestamps for correlation
2. **Watch the player ID** - `player=6` is typically the friendly player
3. **Card IDs** - All BG cards start with `BG` (e.g., `BG28_503`)
4. **Enchantments** - Cards like `BG29_813e` are buffs/effects, not played cards
5. **Zone changes** - Cards moving from `HAND` to `PLAY` = bought; `HAND` to `GRAVEYARD` = sold

---

## Common Battlegrounds Card Prefixes

| Prefix | Type |
|--------|------|
| `BG` | Battlegrounds minion |
| `TB_BaconShop` | Tavern/Bob related UI |
| `BG##_` | Tier ## minion (BG28 = Tier 4, etc.) |
| `Bacon_` | Bacon (BG) enchantment/buff |
