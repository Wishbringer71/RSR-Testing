# Changes since 7.5.7.1+wsh1

What this release changes in the fight. For the whole distance from upstream, see
[`fork-changes-in-play.md`](fork-changes-in-play.md).

## Scholar: Recitation only for an Excogitation that will follow

Recitation went out for any tank below `Minimum HP percent to use Excogitation as a heal instead of a defensive buff`,
while the Excogitation it was pressed for waits for danger under `Excogitation as a heal only on a target in danger`
(on by default). A tank low but not in danger - the off-tank after a swap - got Recitation and then no Excogitation:
Recitation ran out unused, and its minute-long recast was missing at the next tankbuster. Recitation for a tank now asks
for the same danger as the Excogitation that follows it.
