# Gameplay gap analysis (Settlers of Catan baseline)

This document tracks how the current **Game** aggregate and APIs compare to a standard **base Catan** ruleset, so future work can be prioritized.

## Implemented (high level)

- Board geometry (hexes, vertices, edges), initial placement flow, roads/settlements/cities, dice and resource distribution, robber movement and stealing, discard-on-7, maritime trades (2:1 / 3:1 / 4:1), player trades (offer/accept), development cards (knight, VP, road building, year of plenty, monopoly), turn end, join game from lobby.

## Gaps and follow-ups

1. **Victory** — Confirm 10 VP end condition, game end phase, and UI for winner; ensure VP cards and public VP are reflected in DTOs the client can show.
2. **Longest road / Largest army** — If not in domain, add scoring policies, recalculation on relevant events, and expose in `GameDto` / UI.
3. **Trade decline / counter** — Server may lack explicit “decline” or counter-offer; align API and UI with desired UX.
4. **Setup order** — Verify snake order for second settlement/road matches rules; document edge cases for reconnects.
5. **Bank exhaustion** — Official rules when bank runs out of a resource; implement or explicitly document simplification.
6. **Development deck** — Shuffle, secrecy of hand, and “can’t play VP on same turn drawn” style rules if you want strict compliance.
7. **Special builds** — “Build during turn” vs “only on your turn” for cities/settlements/roads outside main build phase if you add them.
8. **Disconnect / reconnect** — Policy for turn timers, replacing a player, or pausing (not required for MVP but listed in original plan).

## Suggested implementation order

1. Victory + game end (domain + `GameStateUpdated` payload fields + game page).
2. Longest road and largest army (domain + DTO + board UI badges).
3. Trade flow polish (decline, clear stale offers).
4. Stricter rule edge cases (bank, dev deck) as needed for your target audience.
