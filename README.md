<p align="center">
  <img src="icon.png" alt="Ranensol's Villagers" width="256"/>
</p>

# Aska - Ranensol's Villagers

![GitHub release](https://img.shields.io/github/v/release/Ranensol/Ranensol.BepInEx.Aska.Villagers)

A BepInEx mod for ASKA that automatically assigns summoned villagers to a house, and allows all villagers to be made homeless or assigned a house based on skill tier and house quality with a single keypress.

## Features

- 🏠 **Auto-assigns** - New villagers to a bed, the worst one available
- 🎯 **Tier-based priority** - Tier 4 villagers get the best houses first
- 📊 **Smart scoring** - Uses actual game happiness (Comfort + Area Desirability)
- ⌨️ **Hotkeys** - F8 to manually assign all homeless, F9 to make everyone homeless for re-sorting
- 📈 **HUD widget** - Shows available beds in top-right corner

## How It Works

When you summon a villager, they're automatically assigned to the worst available house. Higher-tier villagers get priority for better houses. The mod calculates which houses provide the most happiness and fills them optimally.

## Installation

**Backup your saves first!**

1. **Install BepInEx 6 IL2CPP** from [builds.bepinex.dev](https://builds.bepinex.dev/projects/bepinex_be)
2. **Download this mod** from [Releases](../../releases)
3. **Extract DLL** to `BepInEx/plugins/`
4. **Launch game** - config auto-generates at `BepInEx/config/Ranensol.BepInEx.Aska.Villagers.cfg`

## Configuration
```ini
[Housing]
## Auto-assign new villagers (default: true)
AutoHouseNewVillagers = true

## Prioritize higher-tier villagers (default: true)
TierBasedHousing = true

## Use actual happiness calculations (default: true)
UseHappinessScoring = true

## Include outpost houses - WARNING: assigns to ANY bed with no outpost logic (default: false)
IncludeOutposts = false

[Hotkeys]
## Manual assignment (default: F8)
ManualAssignmentKey = F8

## Make all homeless (default: F9)
MakeAllVillagersHomelessKey = F9

[UI]
## Show bed counter (default: true)
DisplayBedsAvailable = true
```

## Usage

**Auto mode (default):** Just summon villagers - they're housed automatically after a 3-second delay.

**Manual mode:** Press **F8** to assign all homeless villagers to houses.

**Re-sort everyone:** Press **F9** to make everyone homeless, then **F8** to reassign based on current tiers.

## Multiplayer

**Only the client that wants to use this mod needs it installed, other clients don't have to, and the server doesn't need it**

## Support

Issues? [Open one on GitHub](../../issues).

## Credits

Created by Ranensol


## License

MIT License - See LICENSE file for details


## Change log

v1.0.1 - Improved algorithm, corrected new villager assignment to worst not best house.
v1.0.0 - Initial release