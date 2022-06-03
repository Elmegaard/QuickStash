# QuickStash

A mod that uses _Compulsively Count_ on all nearby stashes with 1 click.  
Default key is G.

Note this is both a client and server mod.

### Installation

- Install [BepInEx](https://v-rising.thunderstore.io/package/BepInEx/BepInExPack_V_Rising/)
- Extract _quick_stash.dll_ into _(VRising folder)/BepInEx/plugins_
- Extract _quick_stash.dll_ into _(VRising folder)/VRising_Server/BepInEx/plugins_

### Configuration

The keybindings are changed in the menu in-game.

For server configuration, after running the game once, the config file will be generated.

- Update server config in _(VRising folder)/VRising_Server/BepInEx/config/quick_stash.cfg_

### Troubleshooting

- If the mod only works when a stash is open, it is because the server part of the mod is not installed. There has been reported issues with this in singleplayer. Unfortunately the only fix I have found for now is to run a dedicated server instead of running singleplayer.

### Known Issues

- Silver debuff will not get removed

### Support

- Open an [issue](https://github.com/Elmegaard/QuickStash/issues) on [github](https://github.com/Elmegaard/QuickStash)
- Ask in the [V Rising Mod Community](https://discord.gg/CWzkHvekg3) Discord

<details>
<summary>Changelog</summary>

`1.2.2` (unreleased)

- Reduce cooldown from 2 seconds to 0.5 seconds

`1.2.1`

- Fixed Readme

`1.2.0`

- Increased default range to 50
- Added Wetstone (keybinds added to controls in-game)
- Code refactor
- Fixed memory leak (but added small stutter when depositing)

`1.1.2`

- Fixed a client crash

`1.1.1`

- Updated Readme

`1.1.0`

- Set max distance
- Made config for keybind
- Made config for max distance

`1.0.1`

- Updated Readme

`1.0.0`

- Initial mod upload

</details>
