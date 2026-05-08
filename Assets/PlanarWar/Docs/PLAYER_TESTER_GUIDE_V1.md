# Planar War — Player-Facing Tester Guide v1

## Purpose

This guide is for early testers using the Unity client after the client gameplay-surface closeout. It explains what the main desks do, how to perform the first useful actions, and which surfaces are intentionally not claiming deeper mechanics yet.

## First run

### 1. Connect

Start the client and confirm the top bar shows a connected state. The top **Room / Pocket** strip is physical-context truth: it may show a physical MUD room or a City/Black Market pocket. The Social desk and bottom tray use **chat room** for websocket chat-room truth. If chat-room state is detached but HTTP summary refresh still works, gameplay actions may still function; report chat-room state separately instead of treating every disconnected chat label as a gameplay failure. City and Black Market command shells are pocket-management contexts, so a physical-room-unattached label can be expected when the player is managing a settlement instead of standing in a physical MUD room.

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

Home also shows a **Lane posture** card after the server provides it. This card explains how the current City or Black Market opening is behaving right now.

The **First-hour action path** section is guidance from live server action-path truth. Use it to decide which desk to test next, especially when a workshop pickup, active build, active research, or lane-specific route is already live.

Important: the action path is not a tutorial tracker. It does not complete objectives, grant rewards, start timers, or fake progress. It only points testers toward the next live desk or report type that should already exist.

### Urgent pressure

Home can also show an **Urgent pressure** card when live server pressure truth exists. This card translates the current pressure substrate into a suggested response lane without asking testers to read internal system names.

Use it to read:

- the current pressure step
- the recommended response desk
- why the response matters
- blockers that currently prevent clean follow-through
- proof signals that explain which pressure seam produced the recommendation
- the next report type testers should expect if the response is acted on
- report follow-through state, latest report, outcome, server response, and source-region proof

Report follow-through is report/ledger truth only: it explains whether a response is waiting, ready, blocked, engaged, answered, cooling, or backsliding without starting a new action. When a response is blocked, the **blocker recovery** lines explain what should clear the blocker, what desk to monitor, and which report/replay signals prove the lane is cooling or still waiting. The **response history** lines list the most recent bounded Mother Brain response reports so testers can see what happened before the current blocked, cooling, or backsliding state. The urgent pressure button is route-only. It opens the recommended desk and does not launch events, complete objectives, spawn rewards, bypass blockers, start timers, or make Mother Brain autonomous. Rogue Director, TOMS, Crucible, and full world-director behavior remain future work.

Report the card if it is missing when server pressure truth is visible elsewhere, recommends the wrong desk, hides blockers, blocker recovery, or response history, displays raw object/JSON text, or implies fake Mother Brain event spawning.

### Public services

Home can also show a **Public services** card when live server public-service truth exists. This card explains whether NPC services are stable, strained, overloaded, or being helped by player-city infrastructure.

Use it to read:

- whether public services are stable, strained, overloaded, or shadow-exposed
- the recommended mode and service to test next
- why NPC public services remain baseline infrastructure instead of being replaced by player cities
- public-service, city-support, and shadow-risk proof signals
- the next report type testers should expect from public-service usage
- recent service reports, including latest report, service mode, queue/strain values, runway context, report count, and proof signals when the server provides them

Important: this surface is guidance and public-service report follow-through only. It reads existing public-service reports; it does not apply fake taxes, queue timers, service outcomes, rewards, public-service protection, shadow exposure, Rogue Director, TOMS, Crucible, or autonomous Mother Brain behavior.

Report the card if it is missing when server public-service truth is visible elsewhere, recommends the wrong desk, shows raw object/JSON text, or implies fake public-service mechanics.


### Regional support

Home can also show a **Regional support** card when live server support truth exists. This card explains how city support, public services, regional world consequences, and report truth are currently touching MUD-facing play.

Use it to read:

- whether regional support is quiet, supporting, pressured, or restricted
- the recommended support focus and route-only action label
- city support signals such as support band, recommended posture, support capacity, and exportable city resources
- MUD progression signals for vendor supply, mission board posture, and civic services
- regional life signals such as affected regions, severe consequence counts, and destabilization pressure
- recent report and consequence signals, including latest server response and latest world-consequence reports when present
- support follow-through, including state, clear-when guidance, watch-next signals, latest support/server/world report titles, and next report type
- guardrails that confirm player cities support/optimize public play without becoming mandatory

Important: this surface is guidance, report truth, and follow-through explanation only. Follow-through can say whether the bridge is waiting, ready, restricted, strained, or represented by receipts, but it does not grant items, rewards, levels, fake MUD progression, taxes, queue timers, public-service protection, shadow exposure, Rogue Director, TOMS, Crucible, autonomous Mother Brain behavior, or fabricated player identity/action truth.

Report the card if it is missing when server regional-support truth is visible elsewhere, recommends the wrong desk, shows raw object/JSON text, hides follow-through guidance or guardrails, or implies player cities are mandatory for baseline MUD progression.

### Recovery opportunities

Home can also show a **Recovery opportunities** card when live server recovery truth exists. This card summarizes existing city-backed regional recovery candidates for the full-session Unity client.

Use it to read:

- the current recovery-opportunity state
- the recommended city desk/action
- eligible region IDs
- the top recovery candidate
- runtime resource requirements when the server already knows them
- the next report type
- the latest relevant report or consequence summary
- guardrails that keep this board read-only

Important: this surface is guidance only. It does not execute contracts, start queues, grant rewards, grant items, grant levels, fake MUD progression, create taxes, create public-service protection, create shadow exposure, activate Rogue Director, activate TOMS/Crucible, make Mother Brain autonomous, or fabricate player identity/action truth.

Report the card if it is missing when server recovery truth is visible elsewhere, recommends the wrong desk, shows raw object/JSON text, hides resource/report/guardrail details, or implies player cities are mandatory for baseline MUD progression.

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

Important: these labels are intentionally thin. Do not assume live NPC attack percentages, raid-protection percentages, or disruption/exposure math exists until server truth explicitly surfaces those mechanics.

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

Gear compatibility should follow server slot truth. Do not report “my favorite item cannot equip in the wrong slot” as a bug unless the item data says it should fit.

## Social / Comms desk

Social shows chat-room state, physical/pocket context, recent comms, and filters. The bottom chat tray is the live chat surface.

City and Black Market shells may show a pocket context instead of a physical room. That is not automatically a failure: settlements are management contexts and should not fake regional room membership. Chat-room send still needs a real WebSocket chat-room attachment. A visible **Chat room lobby** and visible **City pocket** are two different truths, not a contradiction.

Use filters to view:

- All
- Chat room
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
- missing or wrong urgent pressure card, blockers, blocker recovery, response history, proof signals, report follow-through, or report family, or regional support signals or follow-through
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
5. Read the urgent pressure card if it is present; note blockers, blocker recovery, and proof signals before clicking its route-only button.
6. Read the regional support if it is present; note whether it frames city support as optimization rather than mandatory MUD progression.
7. Use recommended route buttons only to move to suggested live desks.
8. Open Development.
9. Start research if available.
10. Craft one Workshop item.
11. Wait for the timer or use a QA-shortened ready state if available.
12. Collect the item.
13. Open Heroes / Operatives.
14. Equip and unequip the crafted item if compatible.
15. Open Operations.
16. Start or resolve one available operation.
17. Open Social and send or filter chat if chat-room state is attached; if it shows both a Chat room and a City/Market pocket context, confirm the copy explains that websocket chat-room truth and physical/pocket context are separate.
18. Report only concrete failures or unclear results.
