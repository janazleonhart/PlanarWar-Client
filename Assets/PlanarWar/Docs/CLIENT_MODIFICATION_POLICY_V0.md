# Planar War Client Modification and Accessibility Policy v0

## Status

This is a working policy for alpha and early tester builds. It is meant to be clear enough for testers, streamers, support staff, and developers while Planar War's client and world systems are still evolving.

The policy may change before public launch, but the core principle should remain stable:

> Players may customize how known information is displayed. They may not modify, automate, reveal, spoof, or fabricate gameplay truth.

## Why this policy exists

Planar War is intentionally built around layered world systems: City, Black Market, public services, regional pressure, recovery work, Mother Brain, future Rogue Director behavior, and future Market Cartel behavior. Some of those systems will expose partial truth, delayed truth, contested truth, or hidden opportunities.

That means the client must be friendly to customization and accessibility without becoming a tool for hidden-state extraction, botting, or unfair advantage.

## Plain-language player policy

Planar War supports official interface customization, accessibility settings, streamer-safe display tools, and may later support safe layout/theme packs.

Planar War does not allow modified clients, injected code, packet manipulation, memory reading, gameplay automation, hidden-state prediction tools, or UI changes that reveal non-public game state.

## Allowed customization

These should be supported through the official Unity client wherever possible:

- UI scale and font size.
- High-contrast and colorblind-friendly display options.
- Reduced motion and lower visual-noise modes.
- Movable, resizable, collapsible, and pinnable panels.
- Saved layout presets.
- Chat placement, channel visibility, filters, and font settings.
- Streamer/privacy mode for hiding account names, shards, private chat, sensitive reports, and other personal details.
- Compact and expanded card modes for pressure, resources, timers, and reports.
- Notification preferences for sound, flashing, banners, and priority alerts.
- Cosmetic themes or accent colors that do not obscure gameplay truth.

## Future safe customization layer

Because the team is small, Planar War should not begin with reviewed third-party plugins or arbitrary addon code.

A safer future option is a data-only layer that loads on top of the official client. For example:

```text
layout.json
theme.json
icons/
```

A layout/theme pack may eventually be allowed to:

- Reposition approved UI panels.
- Resize approved UI panels.
- Choose compact or expanded presentation.
- Change approved theme colors.
- Pin known public widgets.
- Hide or show known public widgets.
- Use approved icons or labels.
- Display known client-visible truth in a different layout.

A layout/theme pack must not:

- Execute scripts.
- Include custom C# assemblies, DLLs, JavaScript, Lua, or other executable code.
- Read memory.
- Read or modify packets.
- Call Planar War APIs directly outside approved client behavior.
- Add gameplay buttons that the official client does not support.
- Automate actions.
- Predict hidden systems.
- Reveal non-public Mother Brain, Rogue Director, Market Cartel, covert, pressure, evidence, or opportunity state.
- Invent timers, success chances, rewards, or warnings.
- Scrape private chat or player data.

## Not allowed

The following are not allowed unless the team explicitly approves a future exception:

- Modified Unity client assemblies.
- Injected DLLs or runtime patchers.
- Memory readers, memory writers, or overlays that inspect process memory.
- Packet sniffing, packet rewriting, packet replay, or proxy manipulation.
- Custom clients that connect to Planar War services without approval.
- Bots, clickers, input playback, or scripts that play unattended.
- Macros that choose targets, routes, abilities, trades, missions, or responses automatically.
- Tools that infer or reveal hidden game state.
- Tools that calculate private or intentionally obscured outcomes.
- Tools that spoof client identity, account identity, location, action timing, cooldowns, rewards, success chances, or pressure state.
- Tools that collect, scrape, publish, or automate private chat or personal player data.

## Accessibility policy

Planar War allows accessibility hardware, adaptive controllers, input remapping, and assistive software that translates a player's intent into game input.

Allowed accessibility examples include:

- Adaptive controllers.
- Remapped keyboards or mice.
- MMO mice.
- Foot pedals.
- Switch controls.
- Voice input.
- Eye-tracking input.
- Input translation software.
- Assistive overlays that help a player interact with the official client without making gameplay decisions for them.

The important distinction is:

| Allowed | Not allowed |
|---|---|
| The tool helps the player perform an intended input. | The tool chooses, repeats, farms, reacts, routes, or plays for the player. |

Players should not need to disclose medical details to use accessibility tools.

Support may eventually allow a voluntary accessibility note on an account. That note should not grant rule immunity, but it should warn moderation not to treat unusual input as proof of botting by itself.

## Anti-bot enforcement doctrine

Planar War should use server-side behavior analysis instead of trusting client-side restrictions alone.

The server must never trust the client for:

- Cooldowns.
- Timers.
- Rewards.
- Success chances.
- Pressure state.
- Hidden state.
- Mission execution results.
- Economy results.
- Movement or action validity without validation.

The client may request an action. The server decides whether the action is valid.

## No single-signal bans

No player should be punished from one signal alone.

Do not ban only because:

- A session is long.
- Input timing is unusual.
- Input timing is fast.
- The player uses adaptive hardware.
- The player uses remapping software.
- The player uses an MMO mouse or unusual controller.
- The player does not chat.
- The player plays at unusual hours.
- The player repeats a normal gameplay loop.

Long sessions are especially weak evidence. Old-school MMO players may camp rare spawns or rotate around long 24-hour or 48-hour windows. Long playtime can justify looking for other patterns, but it is not proof of automation.

## Bot-risk signals

Useful bot-risk signals include patterns such as:

- Extremely regular repeated action intervals over long windows.
- Action chains faster than the UI and server state could reasonably support.
- Repeated routes with no natural variation.
- Perfect reaction times after new information appears.
- Identical high-value farming loops across long periods.
- Multi-account synchronization that looks mechanically coordinated.
- Economy behavior that repeats exact buy/sell/transfer patterns without normal variance.
- Action requests that do not match what the official client could reasonably present.
- Repeated behavior that continues through unexpected state changes.

These signals should feed a risk score. They should not become automatic proof by themselves.

## Enforcement ladder

Use the least harmful response that protects the game.

1. **Telemetry only** — collect data without affecting the player.
2. **Soft internal flag** — mark for observation.
3. **Targeted friction** — slow or limit only the suspicious action path.
4. **In-game presence check** — ask for confirmation in a non-punitive way where appropriate.
5. **Temporary transaction or action hold** — protect the economy or world state when risk is high.
6. **Manual review** — require human review for severe action whenever possible.
7. **Temporary suspension** — only for strong evidence or active harm.
8. **Permanent ban** — reserved for high-confidence, repeated, or severe abuse.

When accessibility could plausibly explain a pattern, prefer friction, observation, or review over punishment.

## Human review and appeals

Planar War should have an appeal path before or after severe enforcement.

Appeals should be handled without requiring disabled players to disclose private medical details. The important review question is not "what disability does this player have?" It is "does the evidence show unattended automation, gameplay decision automation, spoofing, packet manipulation, or hidden-state abuse?"

## Streamer and privacy expectations

The official client should eventually include streamer/privacy settings so players do not need unsafe overlays for basic presentation needs.

Streamer mode should consider hiding or masking:

- Account names.
- Character/account identifiers.
- Shard/server identifiers where needed.
- Private chat.
- Sensitive reports.
- Invite codes or session tokens.
- Debug markers.
- Backend IDs.

## Approved-client stance

Planar War may later consider approved clients or approved overlay tools, but that should not be an alpha or early-beta dependency.

If approved clients ever become a possibility, minimum expectations should include:

- Registered maintainer identity.
- Signed builds.
- Public source or source submission.
- No automation.
- No packet modification.
- No memory reading.
- No hidden-state inference.
- No gameplay decision helpers.
- No private-chat scraping.
- Clear version compatibility.
- Server-side manifest/version enforcement.

Because plugin review is expensive and the team is small, the safer long-term default is official customization plus data-only layout/theme packs.

## Developer implementation notes

Prioritize in this order:

1. Robust official Unity client customization.
2. Accessibility and streamer settings.
3. Server-authoritative action validation.
4. Server-side bot-risk telemetry.
5. Data-only layout/theme pack format.
6. Optional approved overlay/client discussion much later.

Do not ship sensitive hidden-system logic to the client unless that logic is intentionally public.

Do not expose raw backend IDs, route names, internal daemon labels, or hidden-state markers in player-facing UI.

## Final policy sentence

Planar War supports generous official interface customization and accessibility, may later support safe data-only layout/theme packs, and does not allow modified clients, injected code, packet manipulation, automation, hidden-state inference, or tools that alter or reveal gameplay truth.
