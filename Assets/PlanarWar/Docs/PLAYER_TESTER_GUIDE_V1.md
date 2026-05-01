# Planar War — Player-Facing Tester Guide v1

## Purpose

This guide is for early testers using the Unity client after the client gameplay-surface closeout. It explains what the main desks do, how to perform the first useful actions, and which surfaces are intentionally not claiming deeper mechanics yet.

## First run

### 1. Connect

Start the client and confirm the top bar shows a connected state. If chat room state is detached but HTTP summary refresh still works, gameplay actions may still function; report the room state separately instead of treating every disconnected room label as a gameplay failure.

### 2. Register or sign in

Use the account gate:

- **Sign in** if you already have an account.
- **Register** if you need a new tester account.

Registration requires:

- display name
- email
- password
- matching password confirmation

After successful registration or login, the client loads live account summary truth. It does not create a settlement locally.

## Founder mode

If the account has no settlement, Home opens in founder mode.

Choose one lane:

### City

Pick City if you want the civic ruler lane:

- public growth
- buildings and research
- workshop progression
- formations and missions
- heroes and shared gear

### Black Market

Pick Black Market if you want the shadow ruler lane:

- deniable operations
- cells, routes, pressure, and covert support
- shadow-flavored development and operations surfaces
- operatives and shared gear

### Duplicate names

If the settlement name already exists, the client should show a clear failure message. Choose another name and try again.

## Home desk

Home is the command floor.

Use it to:

- refresh summary
- jump to Development
- check broad resources and warning/readiness summaries
- follow the post-founder handoff into the main desks

Home should not be treated as the place where every action happens. If a button opens a desk, that is intentional.

### Lane posture and first-hour action path

Home also shows a **Lane posture** card after the backend provides it. This card explains how the current City or Black Market opening is behaving right now.

The **First-hour action path** section is guidance from live `/api/me.earlyLanePosture.actionPath` truth. Use it to decide which desk to test next, especially when a workshop pickup, active build, active research, or lane-specific route is already live.

Important: the action path is not a tutorial tracker. It does not complete objectives, grant rewards, start timers, or fake progress. It only points testers toward the next live desk or receipt family that should already exist.

### Mother Brain pressure path

Home can also show a **Mother Brain pressure path** card when live `/api/me.motherBrainPressureStatus.actionPath` truth exists. This card translates the current pressure substrate into a suggested response lane.

Use it to read:

- the current Mother Brain pressure step
- the recommended response desk
- why the response matters
- blockers that currently prevent clean follow-through
- proof signals that explain which pressure seam produced the recommendation
- the next receipt family testers should expect if the response is acted on

Receipt follow-through is receipt/ledger truth only: it explains whether a response is waiting, ready, blocked, engaged, answered, cooling, or backsliding without starting a new action. When a response is blocked, the **blocker recovery** lines explain what should clear the blocker, what desk to monitor, and which receipt/replay signals prove the lane is cooling or still waiting. The Mother Brain pressure path button is route-only. It opens the recommended desk and does not launch events, complete objectives, spawn rewards, bypass blockers, start timers, or make Mother Brain autonomous. Rogue Director, TOMS, Crucible, and full world-director behavior remain future work.

Report the card if it is missing when backend pressure truth is visible elsewhere, recommends the wrong desk, hides blockers or blocker recovery, displays raw object/JSON text, or implies fake Mother Brain event spawning.

## Development desk

Development contains the growth lane:

- research
- workshop crafting
- buildings / fronts
- building routing

### Research

Use the Research lane to start available research. If research is already active, the client should show the active timer/state rather than pretending another research action can start freely.

### Workshop crafting

Use the Workshop lane to craft gear from live recipe truth.

Recommended flow:

1. Open Development.
2. Open the Workshop lane.
3. Pick a gear slot from the dropdown.
4. Pick a recipe from the recipe dropdown.
5. Review the selected recipe detail line.
6. Click the craft button for that recipe.
7. Watch the active workshop job timer.
8. When ready, collect the pickup.
9. Check Heroes / Operatives or shared armory to confirm the item is available through gear truth.

The client should not show fake recipes. If the count says recipes are available but the picker cannot find them, report it.

### Buildings and routing

Building management lets you view existing buildings and choose build/upgrade/remodel/destroy actions where available.

Routing labels currently mean:

| Routing | Meaning |
| --- | --- |
| Balanced | spreads output |
| Local | nearby demand |
| Reserve | protected stock |
| Exchange | trade flow |

Important: these labels are intentionally thin. Do not assume live NPC attack percentages, raid-protection percentages, or disruption/exposure math exists until backend truth explicitly surfaces those mechanics.

## Operations desk

Operations is for missions, routes, cells/formations, pressure, and active support actions.

Use it to:

- review available mission/action offers
- pick a mission/action
- choose assignment context when available
- start or resolve operations
- review receipts and status messages

Report any of these as bugs:

- raw JSON/object text in mission copy
- a button that looks actionable but does nothing
- an action result that disappears with no receipt
- a timer or active state that contradicts the action you just started

## Heroes / Operatives desk

This desk changes flavor by lane:

- City uses Heroes.
- Black Market uses Operatives / contacts where supported.

Use it to:

- review roster
- recruit or select candidates when available
- release idle heroes/operatives
- inspect shared armory
- pick a gear slot
- equip compatible gear
- unequip gear

Gear compatibility should follow backend slot truth. Do not report “my favorite item cannot equip in the wrong slot” as a bug unless the item data says it should fit.

## Social / Comms desk

Social shows room state, recent comms, and filters. The bottom chat tray is the live chat surface.

Use filters to view:

- All
- Room
- System

If chat disconnects but gameplay actions still work, report it as a comms/session issue rather than a full gameplay outage.

## What testers should report

Please report:

- compile errors
- login or registration failures
- missing founder setup
- City or Black Market creation failure with unclear feedback
- dead buttons
- raw JSON or object text
- invisible action results
- stale timers
- missing or wrong Mother Brain pressure path, blockers, blocker recovery, proof signals, receipt follow-through, or receipt family
- collect buttons that do not collect
- gear that vanishes after craft/collect/equip/release
- navigation dead ends
- lane-truth leaks, such as City showing Black Market-only copy or Black Market showing City-only copy

## What is intentionally future work

Do not report these as bugs unless a developer specifically asks you to test them:

- generated 2D town layout images
- number-heavy formula breakdown panels
- deep routing protection / exposure math
- heavy admin tools inside the player client
- full moderation / reporting policy UI
- autonomous Mother Brain event spawning or fiat outcomes
- Rogue Director implementation
- TOMS / Crucible systems
- autonomous Mother Brain event spawning or fiat outcomes
- Rogue Director implementation
- TOMS / Crucible systems
- advanced Black Market endgame systems
- City / Black Market endgame parity systems not yet surfaced

## Good smoke-test route

For a quick tester pass, run this loop:

1. Register or sign in.
2. Create City or Black Market if needed.
3. Open Home and refresh summary.
4. Read the Lane posture and First-hour action path if they are present.
5. Read the Mother Brain pressure path if it is present; note blockers, blocker recovery, and proof signals before clicking its route-only button.
6. Use recommended route buttons only to move to suggested live desks.
7. Open Development.
8. Start research if available.
9. Craft one Workshop item.
10. Wait for the timer or use a QA-shortened ready state if available.
11. Collect the item.
12. Open Heroes / Operatives.
13. Equip and unequip the crafted item if compatible.
14. Open Operations.
15. Start or resolve one available operation.
16. Open Social and send or filter chat if room state is attached.
17. Report only concrete failures or unclear results.
