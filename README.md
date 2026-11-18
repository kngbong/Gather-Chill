<img width="144" height="144" alt="Icon" src="https://github.com/user-attachments/assets/06c00e19-8ea2-4879-84c8-9735d252e3ba" />

Gather & Chill
---------------------------

The plugin that is meant to help gather your Miner and Botanist needs, so you can focus on other things in your day. 

> **This is a fork of [LeontopodiumNivale14/Gather-Chill](https://github.com/LeontopodiumNivale14/Gather-Chill).**
>
> Install it by adding this repo to Dalamud: `https://raw.githubusercontent.com/kngbong/Gather-Chill/main/repo.json`
>
> Extra settings in this fork (Settings -> Repair & Spiritbond / Gathering Options):
> - **Repair Method -> Mender (Gil)**: before heading to a node, travels to a town mender with an aetheryte
>   and pays gil to repair all equipped gear once anything drops to the **Repair Threshold**.
>   Pick a specific destination or leave it on **Auto** (same zone first, then any attuned mender).
> - **Gather Missing Log Items**: while gathering, any item in the node that has never been gathered before
>   is collected once, so its Gathering Log entry gets filled.
>
> Note: this fork uses a different internal name (`GatherChillFork`), so it installs next to the original
> plugin without replacing it. Don't have both enabled at the same time.

- Add items to your list and set how many you want to gather, or even how many you want to keep in your inventory
- Save list of items that you need to gather. Want to keep a list of 7000 of each crystal? Maybe you have a gathering list for the new crafted armor you want
- Ability to return back to your desired location with support from [Lifestream](https://github.com/NightmareXIV/Lifestream), set if you want to return home, an fc, apartment, or even a custom command to run post
- Import item list from other sources into the plugin/websites (teamcraft, artisan, workshoppa), and allows for devs to send their requested item list/gatherable via IPC support
- See a list of all items, when the next time node wills show up and how soon it'll be
- Included is a mini-overlay so you can track what you're gathering, and what's next on the list with how much progress you've gotten

# Please Note
Navmesh is a requirement for this plugin to function. A lot of this wouldn't be possible without the movement capability. If you need the repo, you can find it here: `https://github.com/awgil/ffxiv_navmesh`

Future features as time goes on
---------------------------

 - [ ] Retainer support via [AutoRetainer](https://github.com/PunishXIV/AutoRetainer) (being able to do retainers between gathering nodes, or even subs)
 - [ ] A dedicated script mode, and allowing for turning in gathering collectables to vendors to collect scripts once your inventory reaches a certain point. Also allowing buying items from said vendors as well
 - [ ] Diadem support as a whole (being able to farm out gathering nodes across all the areas...)
 - [ ] Island Sanctuary support
 - [ ] Connectivity to WahTools/WahFishing
 - [ ] Allow for auto-buff/gathering without the need for navigating between nodes (for users who want to just move themselves)

